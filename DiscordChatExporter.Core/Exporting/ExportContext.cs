using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal class ExportContext
{
    private readonly Dictionary<Snowflake, Member?> _membersById = new();
    private readonly Dictionary<Snowflake, Channel> _channelsById = new();
    private readonly Dictionary<Snowflake, Role> _rolesById = new();
    private readonly ExportAssetDownloader _assetDownloader;

    public DiscordClient Discord { get; }

    public ExportRequest Request { get; }

    public ExportContext(DiscordClient discord, ExportRequest request)
    {
        Discord = discord;
        Request = request;

        _assetDownloader = new ExportAssetDownloader(
            request.AssetsDirPath,
            request.ShouldReuseAssets
        );
    }

    public async ValueTask PopulateChannelsAndRolesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var channel in Discord.GetGuildChannelsAsync(Request.Guild.Id, cancellationToken)
        )
            _channelsById[channel.Id] = channel;

        await foreach (var role in Discord.GetGuildRolesAsync(Request.Guild.Id, cancellationToken))
            _rolesById[role.Id] = role;
    }

    // Because members cannot be pulled in bulk, we need to populate them on demand
    private async ValueTask PopulateMemberAsync(
        Snowflake id,
        User? fallbackUser,
        CancellationToken cancellationToken = default
    )
    {
        if (_membersById.ContainsKey(id))
            return;

        var member = await Discord.TryGetGuildMemberAsync(Request.Guild.Id, id, cancellationToken);

        // User may have left the guild since they were mentioned.
        // Create a dummy member object based on the user info.
        if (member is null)
        {
            var user = fallbackUser ?? await Discord.TryGetUserAsync(id, cancellationToken);

            // User may have been deleted since they were mentioned
            if (user is not null)
                member = Member.CreateFallback(user);
        }

        // Store the result even if it's null, to avoid re-fetching non-existing members
        _membersById[id] = member;
    }

    public async ValueTask PopulateMemberAsync(
        Snowflake id,
        CancellationToken cancellationToken = default
    ) => await PopulateMemberAsync(id, null, cancellationToken);

    public async ValueTask PopulateMemberAsync(
        User user,
        CancellationToken cancellationToken = default
    ) => await PopulateMemberAsync(user.Id, user, cancellationToken);

    public string FormatDate(DateTimeOffset instant) =>
        Request.DateFormat switch
        {
            "unix" => instant.ToUnixTimeSeconds().ToString(),
            "unixms" => instant.ToUnixTimeMilliseconds().ToString(),
            var format => instant.ToLocalString(format)
        };

    public Member? TryGetMember(Snowflake id) => _membersById.GetValueOrDefault(id);

    public Channel? TryGetChannel(Snowflake id) => _channelsById.GetValueOrDefault(id);

    public Role? TryGetRole(Snowflake id) => _rolesById.GetValueOrDefault(id);

    public IReadOnlyList<Role> GetUserRoles(Snowflake id) =>
        TryGetMember(id)?.RoleIds
            .Select(TryGetRole)
            .WhereNotNull()
            .OrderByDescending(r => r.Position)
            .ToArray() ?? Array.Empty<Role>();

    public Color? TryGetUserColor(Snowflake id) =>
        GetUserRoles(id).Where(r => r.Color is not null).Select(r => r.Color).FirstOrDefault();

    public async ValueTask<string> ResolveAssetUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        if (!Request.ShouldDownloadAssets)
            return url;

        try
        {
            var filePath = await _assetDownloader.DownloadAsync(url, cancellationToken);
            var relativeFilePath = Path.GetRelativePath(Request.OutputDirPath, filePath);

            // Prefer relative paths so that the output files can be copied around without breaking references.
            // If the asset directory is outside of the export directory, use an absolute path instead.
            var optimalFilePath =
                relativeFilePath.StartsWith(
                    ".." + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal
                )
                || relativeFilePath.StartsWith(
                    ".." + Path.AltDirectorySeparatorChar,
                    StringComparison.Ordinal
                )
                    ? filePath
                    : relativeFilePath;

            // For HTML, the path needs to be properly formatted
            if (Request.Format is ExportFormat.HtmlDark or ExportFormat.HtmlLight)
            {
                // Create a 'file:///' URI and then strip the 'file:///' prefix to allow for relative paths
                return new Uri(new Uri("file:///"), optimalFilePath).ToString()[8..];
            }

            return optimalFilePath;
        }
        // Try to catch only exceptions related to failed HTTP requests
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/332
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/372
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // We don't want this to crash the exporting process in case of failure.
            // TODO: add logging so we can be more liberal with catching exceptions.
            return url;
        }
    }
}

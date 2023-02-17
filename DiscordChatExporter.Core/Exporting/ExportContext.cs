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
    private readonly Dictionary<Snowflake, Member?> _members = new();
    private readonly Dictionary<Snowflake, Channel> _channels = new();
    private readonly Dictionary<Snowflake, Role> _roles = new();
    private readonly ExportAssetDownloader _assetDownloader;

    public DiscordClient Discord { get; }
    public ExportRequest Request { get; }

    public ExportContext(DiscordClient discord,
        ExportRequest request)
    {
        Discord = discord;
        Request = request;

        _assetDownloader = new ExportAssetDownloader(
            request.AssetsDirPath,
            request.ShouldReuseAssets
        );
    }

    public async ValueTask PopulateChannelsAndRolesAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var channel in Discord.GetGuildChannelsAsync(Request.Guild.Id, cancellationToken))
            _channels[channel.Id] = channel;

        await foreach (var role in Discord.GetGuildRolesAsync(Request.Guild.Id, cancellationToken))
            _roles[role.Id] = role;
    }

    // Because members are not pulled in bulk, we need to populate them on demand
    public async ValueTask PopulateMemberAsync(Snowflake id, CancellationToken cancellationToken = default)
    {
        if (_members.ContainsKey(id))
            return;

        var member = await Discord.TryGetGuildMemberAsync(Request.Guild.Id, id, cancellationToken);

        // Store the result even if it's null, to avoid re-fetching non-existing members
        _members[id] = member;
    }

    public string FormatDate(DateTimeOffset instant) => Request.DateFormat switch
    {
        "unix" => instant.ToUnixTimeSeconds().ToString(),
        "unixms" => instant.ToUnixTimeMilliseconds().ToString(),
        var format => instant.ToLocalString(format)
    };

    public Member? TryGetMember(Snowflake id) => _members.GetValueOrDefault(id);

    public Channel? TryGetChannel(Snowflake id) => _channels.GetValueOrDefault(id);

    public Role? TryGetRole(Snowflake id) => _roles.GetValueOrDefault(id);

    public Color? TryGetUserColor(Snowflake id)
    {
        var member = TryGetMember(id);

        var memberRoles = member?
            .RoleIds
            .Select(TryGetRole)
            .WhereNotNull()
            .ToArray();

        return memberRoles?
            .Where(r => r.Color is not null)
            .OrderByDescending(r => r.Position)
            .Select(r => r.Color)
            .FirstOrDefault();
    }

    public async ValueTask<string> ResolveAssetUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Request.ShouldDownloadAssets)
            return url;

        try
        {
            var filePath = await _assetDownloader.DownloadAsync(url, cancellationToken);
            var relativeFilePath = Path.GetRelativePath(Request.OutputDirPath, filePath);

            // Prefer relative paths so that the output files can be copied around without breaking references.
            // If the assets path is outside of the export directory, use the absolute path instead.
            var optimalFilePath =
                relativeFilePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                relativeFilePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
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
            // TODO: add logging so we can be more liberal with catching exceptions
            // We don't want this to crash the exporting process in case of failure
            return url;
        }
    }
}
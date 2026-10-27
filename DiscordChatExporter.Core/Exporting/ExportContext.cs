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
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Logging;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal class ExportContext(DiscordClient discord, ExportRequest request)
{
    private readonly Dictionary<Snowflake, Member?> _membersById = new();
    private readonly Dictionary<Snowflake, Channel?> _channelsById = new();
    private readonly Dictionary<Snowflake, Role> _rolesById = new();

    private readonly ExportAssetDownloader _assetDownloader = new(
        request.AssetsDirPath,
        request.ShouldReuseAssets
    );

    public DiscordClient Discord { get; } = discord;

    public ExportRequest Request { get; } = request;

    public int MemberCacheHits { get; private set; }

    public int MembersFetched { get; private set; }

    public int MemberFallbackLookups { get; private set; }

    public int ReusedAssetsCount => _assetDownloader.ReusedCount;

    public int DownloadedAssetsCount => _assetDownloader.DownloadedCount;

    public int FailedAssetsCount { get; private set; }

    public DateTimeOffset NormalizeDate(DateTimeOffset instant) =>
        Request.IsUtcNormalizationEnabled ? instant.ToUniversalTime() : instant.ToLocalTime();

    public string FormatDate(DateTimeOffset instant, string format = "g") =>
        NormalizeDate(instant).ToString(format, Request.CultureInfo);

    public async ValueTask PopulateChannelsAndRolesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var channel in Discord.GetGuildChannelsAsync(Request.Guild.Id, cancellationToken)
        )
        {
            _channelsById[channel.Id] = channel;
        }

        await foreach (var role in Discord.GetGuildRolesAsync(Request.Guild.Id, cancellationToken))
        {
            _rolesById[role.Id] = role;
        }

        Logger.Debug(
            LogCategory.Export,
            $"Preloaded {_channelsById.Count} channel(s) and {_rolesById.Count} role(s) into context for guild '{Request.Guild.Name}' (#{Request.Guild.Id})."
        );
    }

    // Threads are not preloaded, so we resolve them on demand
    public async ValueTask PopulateChannelAsync(
        Snowflake id,
        CancellationToken cancellationToken = default
    )
    {
        if (_channelsById.ContainsKey(id))
        {
            Logger.Trace(LogCategory.Export, $"Channel #{id} resolved from context cache.");
            return;
        }

        Logger.Debug(LogCategory.Export, $"Resolving uncached channel mention #{id} via API...");

        var channel = await Discord.TryGetChannelAsync(id, cancellationToken);

        // Store the result even if it's null, to avoid re-fetching non-existing channels
        _channelsById[id] = channel;
    }

    // Because members cannot be pulled in bulk, we need to populate them on demand
    private async ValueTask PopulateMemberAsync(
        Snowflake id,
        User? fallbackUser,
        CancellationToken cancellationToken = default
    )
    {
        if (_membersById.ContainsKey(id))
        {
            MemberCacheHits++;
            Logger.Trace(LogCategory.Export, $"Member #{id} resolved from context cache.");
            return;
        }

        var member = await Discord.TryGetGuildMemberAsync(Request.Guild.Id, id, cancellationToken);

        if (member is not null)
        {
            MembersFetched++;
            Logger.Debug(
                LogCategory.Export,
                $"Fetched guild member #{id} ('{member.User.FullName}')."
            );
        }
        // User may have left the guild since they were mentioned.
        // Create a dummy member object based on the user info.
        else
        {
            User? user;
            if (fallbackUser is not null)
            {
                user = fallbackUser;
                Logger.Debug(
                    LogCategory.Export,
                    $"Member #{id} not found in guild #{Request.Guild.Id}; using message author/user fallback ('{fallbackUser.FullName}')."
                );
            }
            else
            {
                MemberFallbackLookups++;
                Logger.Debug(
                    LogCategory.Export,
                    $"Member #{id} not found in guild #{Request.Guild.Id}; falling back to global user lookup."
                );
                user = await Discord.TryGetUserAsync(id, cancellationToken);
            }

            // User may have been deleted since they were mentioned
            if (user is not null)
            {
                member = Member.CreateFallback(user);
            }
            else
            {
                Logger.Debug(
                    LogCategory.Export,
                    $"User #{id} could not be resolved (account may be deleted)."
                );
            }
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

    public Member? TryGetMember(Snowflake id) => _membersById.GetValueOrDefault(id);

    public Channel? TryGetChannel(Snowflake id) => _channelsById.GetValueOrDefault(id);

    public Role? TryGetRole(Snowflake id) => _rolesById.GetValueOrDefault(id);

    public IReadOnlyList<Role> GetUserRoles(Snowflake id) =>
        TryGetMember(id)
            ?.RoleIds.Select(TryGetRole)
            .WhereNotNull()
            .OrderByDescending(r => r.Position)
            .ToArray()
        ?? [];

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

            // Prefer the relative path so that the export package can be copied around without breaking references.
            // However, if the assets directory lies outside the export directory, use the absolute path instead.
            var shouldUseAbsoluteFilePath =
                relativeFilePath.StartsWith(
                    ".." + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal
                )
                || relativeFilePath.StartsWith(
                    ".." + Path.AltDirectorySeparatorChar,
                    StringComparison.Ordinal
                );

            var optimalFilePath = shouldUseAbsoluteFilePath ? filePath : relativeFilePath;

            // For HTML, the path needs to be properly formatted
            if (Request.Format is ExportFormat.HtmlDark or ExportFormat.HtmlLight)
                return Url.EncodeFilePath(optimalFilePath);

            return optimalFilePath;
        }
        // Try to catch only exceptions related to failed HTTP requests
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/332
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/372
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // We don't want this to crash the exporting process in case of failure.
            FailedAssetsCount++;
            Logger.Warning(
                LogCategory.Asset,
                $"Failed to download asset '{url}'; falling back to remote URL ({ex.GetType().Name}: {ex.Message})."
            );
            return url;
        }
    }
}

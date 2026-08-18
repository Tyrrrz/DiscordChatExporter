using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Utils;
using Gress;
using JsonExtensions.Http;
using JsonExtensions.Reading;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Discord;

public class DiscordClient(
    string token,
    RateLimitPreference rateLimitPreference = RateLimitPreference.RespectAll
)
{
    private readonly Uri _baseUri = new("https://discord.com/api/v10/", UriKind.Absolute);
    private TokenKind? _resolvedTokenKind;

    // Session-scoped IDs for official-web X-Super-Properties. Minted per DiscordClient
    // so two tokens in one process do not share a launch fingerprint.
    // https://docs.discord.food/reference#client-properties
    private readonly string _clientLaunchId = Guid.NewGuid().ToString();
    private readonly string _launchSignature = GenerateLaunchSignature();
    private readonly string _clientHeartbeatSessionId = Guid.NewGuid().ToString();
    private string? _xSuperPropertiesHeader;
    private int _clientBuildNumberResolved;

    private async ValueTask<HttpResponseMessage> GetResponseAsync(
        string url,
        TokenKind tokenKind,
        CancellationToken cancellationToken = default
    )
    {
        // Scrape outside the retry pipeline so a failed /app fetch cannot be
        // retried as if it were a Discord API error.
        if (tokenKind == TokenKind.User)
            await RefreshClientBuildNumberAsync(cancellationToken);

        return await Http.ResponseResiliencePipeline.ExecuteAsync(
            async innerCancellationToken =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, url));

                // Don't validate because the token can have special characters
                // https://github.com/Tyrrrz/DiscordChatExporter/issues/828
                request.Headers.TryAddWithoutValidation(
                    "Authorization",
                    tokenKind == TokenKind.Bot ? $"Bot {token}" : token
                );

                if (tokenKind == TokenKind.User)
                    AddUserClientHeaders(request);

                var response = await Http.Client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    innerCancellationToken
                );

                // Discord has advisory rate limits (communicated via response headers), but they are typically
                // way stricter than the actual rate limits enforced by the server.
                // The user may choose to ignore the advisory rate limits and only retry on hard rate limits,
                // if they want to prioritize speed over compliance (and safety of their account/bot).
                // https://github.com/Tyrrrz/DiscordChatExporter/issues/1021
                if (rateLimitPreference.IsRespectedFor(tokenKind))
                {
                    var remainingRequestCount = response
                        .Headers.TryGetValue("X-RateLimit-Remaining")
                        ?.Pipe(s => int.ParseOrNull(s, CultureInfo.InvariantCulture));

                    var resetAfterDelay = response
                        .Headers.TryGetValue("X-RateLimit-Reset-After")
                        ?.Pipe(s => double.ParseOrNull(s, CultureInfo.InvariantCulture))
                        ?.Pipe(TimeSpan.FromSeconds);

                    // If this was the last request available before hitting the rate limit,
                    // wait out the reset time so that future requests can succeed.
                    // This may add an unnecessary delay in case the user doesn't intend to
                    // make any more requests, but implementing a smarter solution would
                    // require properly keeping track of Discord's global/per-route/per-resource
                    // rate limits and that's just way too much effort.
                    // https://discord.com/developers/docs/topics/rate-limits
                    if (remainingRequestCount <= 0 && resetAfterDelay is not null)
                    {
                        var delay =
                            // Adding a small buffer to the reset time reduces the chance of getting
                            // rate limited again, because it allows for more requests to be released.
                            (resetAfterDelay.Value + TimeSpan.FromSeconds(1))
                            // Sometimes Discord returns an absurdly high value for the reset time, which
                            // is not actually enforced by the server. So we cap it at a reasonable value.
                            .Clamp(TimeSpan.Zero, TimeSpan.FromSeconds(60));

                        await Task.Delay(delay, innerCancellationToken);
                    }
                }

                return response;
            },
            cancellationToken
        );
    }

    private void AddUserClientHeaders(HttpRequestMessage request)
    {
        // TryAddWithoutValidation keeps the User-Agent bytes identical to
        // browser_user_agent. Typed UserAgent APIs re-serialize the value.
        request.Headers.TryAddWithoutValidation("User-Agent", BrowserUserAgent);
        request.Headers.TryAddWithoutValidation("X-Super-Properties", GetXSuperPropertiesHeader());
        request.Headers.TryAddWithoutValidation("X-Discord-Locale", "en-US");
        request.Headers.TryAddWithoutValidation("Accept-Language", "en-US");
        request.Headers.TryAddWithoutValidation("X-Debug-Options", "bugReporterEnabled");

        if (TryGetIanaTimeZone() is { } timeZone)
            request.Headers.TryAddWithoutValidation("X-Discord-Timezone", timeZone);
    }

    private string GetXSuperPropertiesHeader() =>
        _xSuperPropertiesHeader ??= EncodeXSuperProperties(
            FallbackClientBuildNumber,
            _clientLaunchId,
            _launchSignature,
            _clientHeartbeatSessionId
        );

    private async ValueTask RefreshClientBuildNumberAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _clientBuildNumberResolved, 1, 0) != 0)
            return;

        var buildNumber = await TryScrapeClientBuildNumberAsync(cancellationToken);
        if (buildNumber is null)
            return;

        _xSuperPropertiesHeader = EncodeXSuperProperties(
            buildNumber.Value,
            _clientLaunchId,
            _launchSignature,
            _clientHeartbeatSessionId
        );
    }

    private static async ValueTask<int?> TryScrapeClientBuildNumberAsync(
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/app");
            request.Headers.TryAddWithoutValidation("User-Agent", BrowserUserAgent);

            using var response = await Http.Client.SendAsync(
                request,
                HttpCompletionOption.ResponseContentRead,
                timeout.Token
            );

            if (!response.IsSuccessStatusCode)
                return null;

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            return TryParseClientBuildNumber(html);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Keep the hardcoded build number. Never omit X-Super-Properties.
            return null;
        }
    }

    private static string? TryGetIanaTimeZone()
    {
        var timeZone = TimeZoneInfo.Local;
        if (timeZone.HasIanaId)
            return timeZone.Id;

        return TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZone.Id, out var ianaId)
            ? ianaId
            : null;
    }

    private async ValueTask<TokenKind> ResolveTokenKindAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (_resolvedTokenKind is not null)
            return _resolvedTokenKind.Value;

        // Try authenticating as a user
        using var userResponse = await GetResponseAsync(
            "users/@me",
            TokenKind.User,
            cancellationToken
        );

        if (userResponse.StatusCode != HttpStatusCode.Unauthorized)
            return (_resolvedTokenKind = TokenKind.User).Value;

        // Try authenticating as a bot
        using var botResponse = await GetResponseAsync(
            "users/@me",
            TokenKind.Bot,
            cancellationToken
        );

        if (botResponse.StatusCode != HttpStatusCode.Unauthorized)
            return (_resolvedTokenKind = TokenKind.Bot).Value;

        throw new DiscordChatExporterException("Authentication token is invalid.", true);
    }

    private async ValueTask<HttpResponseMessage> GetResponseAsync(
        string url,
        CancellationToken cancellationToken = default
    ) =>
        await GetResponseAsync(
            url,
            await ResolveTokenKindAsync(cancellationToken),
            cancellationToken
        );

    private async ValueTask<JsonElement> GetJsonResponseAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await GetResponseAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => throw new DiscordChatExporterException(
                    "Authentication token is invalid.",
                    true
                ),

                HttpStatusCode.Forbidden => throw new DiscordChatExporterException(
                    $"Request to '{url}' failed: forbidden."
                ),

                HttpStatusCode.NotFound => throw new DiscordChatExporterException(
                    $"Request to '{url}' failed: not found."
                ),

                _ => throw new DiscordChatExporterException(
                    $"""
                    Request to '{url}' failed: {response
                        .StatusCode.ToString()
                        .SeparateWords(' ')
                        .ToLowerInvariant()}.
                    Response content: {await response.Content.ReadAsStringAsync(
                        cancellationToken
                    )}
                    """,
                    true
                ),
            };
        }

        return await response.Content.ReadAsJsonAsync(cancellationToken);
    }

    private async ValueTask<JsonElement?> TryGetJsonResponseAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await GetResponseAsync(url, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsJsonAsync(cancellationToken)
            : null;
    }

    public async ValueTask<Application> GetApplicationAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await GetJsonResponseAsync("applications/@me", cancellationToken);
        return Application.Parse(response);
    }

    private async ValueTask EnsureMessageContentIntentAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (await ResolveTokenKindAsync(cancellationToken) != TokenKind.Bot)
            return;

        var application = await GetApplicationAsync(cancellationToken);
        if (application.IsMessageContentIntentEnabled)
            return;

        throw new DiscordChatExporterException(
            "Provided bot account is missing the MESSAGE_CONTENT privileged intent.",
            true
        );
    }

    public async ValueTask<User?> TryGetUserAsync(
        Snowflake userId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await TryGetJsonResponseAsync($"users/{userId}", cancellationToken);
        return response?.Pipe(User.Parse);
    }

    public async IAsyncEnumerable<Guild> GetUserGuildsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        yield return Guild.DirectMessages;

        var currentAfter = Snowflake.Zero;
        while (true)
        {
            var url = new UrlBuilder()
                .SetPath("users/@me/guilds")
                .SetQueryParameter("limit", "100")
                .SetQueryParameter("after", currentAfter.ToString())
                .Build();

            var response = await GetJsonResponseAsync(url, cancellationToken);

            var count = 0;
            foreach (var guildJson in response.EnumerateArray())
            {
                var guild = Guild.Parse(guildJson);
                yield return guild;

                currentAfter = guild.Id;
                count++;
            }

            if (count <= 0)
                yield break;
        }
    }

    public async ValueTask<Guild> GetGuildAsync(
        Snowflake guildId,
        CancellationToken cancellationToken = default
    )
    {
        if (guildId == Guild.DirectMessages.Id)
            return Guild.DirectMessages;

        var response = await GetJsonResponseAsync($"guilds/{guildId}", cancellationToken);
        return Guild.Parse(response);
    }

    public async IAsyncEnumerable<Channel> GetGuildChannelsAsync(
        Snowflake guildId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (guildId == Guild.DirectMessages.Id)
        {
            var response = await GetJsonResponseAsync("users/@me/channels", cancellationToken);
            foreach (var channelJson in response.EnumerateArray())
                yield return Channel.Parse(channelJson);
        }
        else
        {
            var response = await GetJsonResponseAsync(
                $"guilds/{guildId}/channels",
                cancellationToken
            );

            var channelsJson = response
                .EnumerateArray()
                .OrderBy(j => j.GetProperty("position").GetInt32())
                .ThenBy(j => j.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse))
                .ToArray();

            var parentsById = channelsJson
                .Where(j => j.GetProperty("type").GetInt32() == (int)ChannelKind.GuildCategory)
                .Select((j, i) => Channel.Parse(j, null, i + 1))
                .ToDictionary(j => j.Id);

            // Discord channel positions are relative, so we need to normalize them
            // so that the user may refer to them more easily in file name templates.
            var position = 0;

            foreach (var channelJson in channelsJson)
            {
                var parent = channelJson
                    .GetPropertyOrNull("parent_id")
                    ?.GetNonWhiteSpaceStringOrNull()
                    ?.Pipe(Snowflake.Parse)
                    .Pipe(parentsById.GetValueOrDefault);

                yield return Channel.Parse(channelJson, parent, position);
                position++;
            }
        }
    }

    public async IAsyncEnumerable<Channel> GetGuildThreadsAsync(
        Snowflake guildId,
        bool includeArchived = false,
        Snowflake? before = null,
        Snowflake? after = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (guildId == Guild.DirectMessages.Id)
            yield break;

        var channels = await GetGuildChannelsAsync(guildId, cancellationToken);

        foreach (
            var channel in await GetChannelThreadsAsync(
                channels,
                includeArchived,
                before,
                after,
                cancellationToken
            )
        )
        {
            yield return channel;
        }
    }

    public async IAsyncEnumerable<Role> GetGuildRolesAsync(
        Snowflake guildId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (guildId == Guild.DirectMessages.Id)
            yield break;

        var response = await GetJsonResponseAsync($"guilds/{guildId}/roles", cancellationToken);
        foreach (var roleJson in response.EnumerateArray())
            yield return Role.Parse(roleJson);
    }

    public async ValueTask<Member?> TryGetGuildMemberAsync(
        Snowflake guildId,
        Snowflake memberId,
        CancellationToken cancellationToken = default
    )
    {
        if (guildId == Guild.DirectMessages.Id)
            return null;

        var response = await TryGetJsonResponseAsync(
            $"guilds/{guildId}/members/{memberId}",
            cancellationToken
        );

        return response?.Pipe(j => Member.Parse(j, guildId));
    }

    public async ValueTask<Invite?> TryGetInviteAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var response = await TryGetJsonResponseAsync($"invites/{code}", cancellationToken);
        return response?.Pipe(Invite.Parse);
    }

    public async ValueTask<Channel> GetChannelAsync(
        Snowflake channelId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await GetJsonResponseAsync($"channels/{channelId}", cancellationToken);

        var parentId = response
            .GetPropertyOrNull("parent_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        // It's possible for the parent channel to be inaccessible, despite the
        // child channel being accessible.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1108
        var parent = parentId is not null
            ? await TryGetChannelAsync(parentId.Value, cancellationToken)
            : null;

        return Channel.Parse(response, parent);
    }

    public async ValueTask<Channel?> TryGetChannelAsync(
        Snowflake channelId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await TryGetJsonResponseAsync($"channels/{channelId}", cancellationToken);
        if (response is null)
            return null;

        var parentId = response
            .Value.GetPropertyOrNull("parent_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        // It's possible for the parent channel to be inaccessible, despite the
        // child channel being accessible.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1108
        var parent = parentId is not null
            ? await TryGetChannelAsync(parentId.Value, cancellationToken)
            : null;

        return Channel.Parse(response.Value, parent);
    }

    public async IAsyncEnumerable<Channel> GetChannelThreadsAsync(
        IReadOnlyList<Channel> channels,
        bool includeArchived = false,
        Snowflake? before = null,
        Snowflake? after = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var filteredChannels = channels
            // Categories cannot have threads
            .Where(c => !c.IsCategory)
            // Voice channels cannot have threads
            .Where(c => !c.IsVoice)
            // Empty channels cannot have threads
            .Where(c => !c.IsEmpty)
            // If the 'before' boundary is specified, skip channels that don't have messages
            // for that range, because thread-start event should always be accompanied by a message.
            // Note that we don't perform a similar check for the 'after' boundary, because
            // threads may have messages in range, even if the parent channel doesn't.
            .Where(c => before is null || c.MayHaveMessagesBefore(before.Value))
            .ToArray();

        // Track yielded thread IDs to avoid duplicates that can occur when a thread transitions
        // from active to archived between the two separate API calls used to fetch threads.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1433
        var seenThreadIds = new HashSet<Snowflake>();

        // User accounts can only fetch threads using the search endpoint
        if (await ResolveTokenKindAsync(cancellationToken) == TokenKind.User)
        {
            foreach (var channel in filteredChannels)
            {
                // Either include both active and archived threads, or only active threads
                foreach (
                    var isArchived in includeArchived ? new[] { false, true } : new[] { false }
                )
                {
                    // Offset is just the index of the last thread in the previous batch
                    var currentOffset = 0;
                    while (true)
                    {
                        var url = new UrlBuilder()
                            .SetPath($"channels/{channel.Id}/threads/search")
                            .SetQueryParameter("sort_by", "last_message_time")
                            .SetQueryParameter("sort_order", "desc")
                            .SetQueryParameter("archived", isArchived.ToString().ToLowerInvariant())
                            .SetQueryParameter("offset", currentOffset.ToString())
                            .Build();

                        // Can be null on channels that the user cannot access or channels without threads
                        var response = await TryGetJsonResponseAsync(url, cancellationToken);
                        if (response is null)
                            break;

                        var breakOuter = false;

                        foreach (
                            var threadJson in response.Value.GetProperty("threads").EnumerateArray()
                        )
                        {
                            var thread = Channel.Parse(threadJson, channel);

                            // If the 'after' boundary is specified, we can break early,
                            // because threads are sorted by last message timestamp.
                            if (after is not null && !thread.MayHaveMessagesAfter(after.Value))
                            {
                                breakOuter = true;
                                break;
                            }

                            if (seenThreadIds.Add(thread.Id))
                                yield return thread;

                            currentOffset++;
                        }

                        if (breakOuter)
                            break;

                        if (!response.Value.GetProperty("has_more").GetBoolean())
                            break;
                    }
                }
            }
        }
        // Bot accounts can only fetch threads using the threads endpoint
        else
        {
            var guilds = new HashSet<Snowflake>();
            foreach (var channel in filteredChannels)
                guilds.Add(channel.GuildId);

            // Active threads
            foreach (var guildId in guilds)
            {
                var parentsById = filteredChannels.ToDictionary(c => c.Id);

                var response = await GetJsonResponseAsync(
                    $"guilds/{guildId}/threads/active",
                    cancellationToken
                );

                foreach (var threadJson in response.GetProperty("threads").EnumerateArray())
                {
                    var parent = threadJson
                        .GetPropertyOrNull("parent_id")
                        ?.GetNonWhiteSpaceStringOrNull()
                        ?.Pipe(Snowflake.Parse)
                        .Pipe(parentsById.GetValueOrDefault);

                    if (filteredChannels.Contains(parent))
                    {
                        var thread = Channel.Parse(threadJson, parent);

                        if (seenThreadIds.Add(thread.Id))
                            yield return thread;
                    }
                }
            }

            // Archived threads
            if (includeArchived)
            {
                foreach (var channel in filteredChannels)
                {
                    foreach (var archiveType in new[] { "public", "private" })
                    {
                        // This endpoint parameter expects an ISO8601 timestamp, not a snowflake
                        var currentBefore = before
                            ?.ToDate()
                            .ToString("O", CultureInfo.InvariantCulture);

                        while (true)
                        {
                            // Threads are sorted by archive timestamp, not by last message timestamp
                            var url = new UrlBuilder()
                                .SetPath($"channels/{channel.Id}/threads/archived/{archiveType}")
                                .SetQueryParameter("before", currentBefore)
                                .Build();

                            // Can be null on certain channels
                            var response = await TryGetJsonResponseAsync(url, cancellationToken);
                            if (response is null)
                                break;

                            foreach (
                                var threadJson in response
                                    .Value.GetProperty("threads")
                                    .EnumerateArray()
                            )
                            {
                                var thread = Channel.Parse(threadJson, channel);

                                currentBefore = threadJson
                                    .GetProperty("thread_metadata")
                                    .GetProperty("archive_timestamp")
                                    .GetString();

                                if (seenThreadIds.Add(thread.Id))
                                    yield return thread;
                            }

                            if (!response.Value.GetProperty("has_more").GetBoolean())
                                break;
                        }
                    }
                }
            }
        }
    }

    public async ValueTask<Message?> TryGetMessageAsync(
        Snowflake channelId,
        Snowflake messageId,
        CancellationToken cancellationToken = default
    )
    {
        // Use the regular message listing endpoint with the 'around' parameter instead of the
        // dedicated single-message endpoint, because the latter is not accessible to user tokens.
        var url = new UrlBuilder()
            .SetPath($"channels/{channelId}/messages")
            .SetQueryParameter("around", messageId.ToString())
            .SetQueryParameter("limit", "1")
            .Build();

        // Can be null on channels that the user cannot access
        var response = await TryGetJsonResponseAsync(url, cancellationToken);
        if (response is null)
            return null;

        // The endpoint returns messages around the requested ID, so make sure to only return
        // the message that exactly matches it (it may be absent if it has been deleted).
        return response
            .Value.EnumerateArray()
            .Select(Message.Parse)
            .FirstOrDefault(m => m.Id == messageId);
    }

    private async ValueTask<Message?> TryGetFirstMessageAsync(
        Snowflake channelId,
        Snowflake? after = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = new UrlBuilder()
            .SetPath($"channels/{channelId}/messages")
            .SetQueryParameter("limit", "1")
            .SetQueryParameter("after", (after ?? Snowflake.Zero).ToString())
            .Build();

        // Can be null on channels that the user cannot access
        var response = await TryGetJsonResponseAsync(url, cancellationToken);
        if (response is null)
            return null;

        var message = response.Value.EnumerateArray().Select(Message.Parse).FirstOrDefault();

        return message;
    }

    private async ValueTask<Message?> TryGetLastMessageAsync(
        Snowflake channelId,
        Snowflake? before = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = new UrlBuilder()
            .SetPath($"channels/{channelId}/messages")
            .SetQueryParameter("limit", "1")
            .SetQueryParameter("before", before?.ToString())
            .Build();

        // Can be null on channels that the user cannot access
        var response = await TryGetJsonResponseAsync(url, cancellationToken);
        if (response is null)
            return null;

        return response.Value.EnumerateArray().Select(Message.Parse).LastOrDefault();
    }

    private async IAsyncEnumerable<Message> GetMessagesAsync(
        Snowflake channelId,
        Snowflake? after,
        Snowflake? before,
        IProgress<Percentage>? progress,
        bool isReverse,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        // To keep the understanding of message history independent of the fetching direction,
        // we'll refer to the two ends of the range as Alpha and Omega.
        // Depending on the direction, these are either 'before' and 'after', or 'after' and 'before'.
        //
        // Chronological order:
        // <after> Alpha [----->----->----] Omega <before>
        // Reverse chronological order:
        // <after> Omega [-----<-----<----] Alpha <before>

        // Because Discord API doesn't allow us to provide both 'after' and 'before' parameters
        // at the same time, we have to establish at least one end of the boundary manually.
        // To do that, we'll fetch the Omega message, which will be the terminal message in the range:
        // last message in chronological order, or first message in reverse chronological order.
        // This snapshotting also has the side benefit of allowing us to calculate progress by comparing
        // the timestamps of the Alpha message, Omega message, and the message being currently processed.
        var omegaMessage = !isReverse
            ? await TryGetLastMessageAsync(channelId, before, cancellationToken)
            : await TryGetFirstMessageAsync(channelId, after, cancellationToken);

        // If the Omega doesn't exist or falls outside of the range, then there are simply no messages
        // satisfying the specified range.
        if (
            omegaMessage is null
            || (!isReverse && omegaMessage.Timestamp < after?.ToDate())
            || (isReverse && omegaMessage.Timestamp > before?.ToDate())
        )
        {
            yield break;
        }

        // Persist the Alpha message as soon as we fetch the initial batch of messages.
        // This is only used for calculating progress.
        var alphaMessage = default(Message);

        var currentBoundary = !isReverse ? after ?? Snowflake.Zero : before;
        while (true)
        {
            var url = new UrlBuilder()
                .SetPath($"channels/{channelId}/messages")
                .SetQueryParameter("limit", "100")
                .SetQueryParameter(!isReverse ? "after" : "before", currentBoundary?.ToString())
                .Build();

            var response = await GetJsonResponseAsync(url, cancellationToken);

            var messages = response
                .EnumerateArray()
                .Select(Message.Parse)
                // Messages in batches are always returned from newest to oldest, so reverse if needed
                .Pipe(messages => isReverse ? messages : messages.Reverse())
                .ToArray();

            // Break if there are no messages (can happen if messages are deleted during execution)
            if (!messages.Any())
                yield break;

            // If all messages are empty, make sure that it's not because the bot account doesn't
            // have the MESSAGE_CONTENT intent enabled.
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/1106#issuecomment-1741548959
            if (messages.All(m => m.IsEmpty))
                await EnsureMessageContentIntentAsync(cancellationToken);

            foreach (var message in messages)
            {
                // Ensure that we're still in range by checking against the Omega
                if (!isReverse ? message.Id > omegaMessage.Id : message.Id < omegaMessage.Id)
                {
                    yield break;
                }

                alphaMessage ??= message;

                // Report progress based on timestamps
                if (progress is not null)
                {
                    var fetchedDuration = isReverse
                        ? alphaMessage.Timestamp - message.Timestamp
                        : message.Timestamp - alphaMessage.Timestamp;

                    var totalDuration = isReverse
                        ? alphaMessage.Timestamp - omegaMessage.Timestamp
                        : omegaMessage.Timestamp - alphaMessage.Timestamp;

                    progress.Report(
                        Percentage.FromFraction(
                            // Avoid division by zero if all messages have the exact same timestamp
                            // (which happens when there's only one message in the channel)
                            totalDuration > TimeSpan.Zero
                                ? fetchedDuration / totalDuration
                                : 1
                        )
                    );
                }

                // Some messages, for example thread starter messages, are returned by the API as content-less references.
                // Try to resolve them to the actual message so that they appear as they do in the Discord client.
                var actualMessage =
                    message.Kind == MessageKind.ThreadStarterMessage
                    && message.Reference?.ChannelId is { } referencedChannelId
                    && message.Reference?.MessageId is { } referencedMessageId
                        ? await TryGetMessageAsync(
                            referencedChannelId,
                            referencedMessageId,
                            cancellationToken
                        )
                        : null;

                yield return actualMessage ?? message;
            }

            // The new boundary is always determined by the last message in the batch,
            // because we order the messages based on fetching direction.
            currentBoundary = messages.Last().Id;
        }
    }

    public async IAsyncEnumerable<Message> GetMessagesAsync(
        Snowflake channelId,
        Snowflake? after = null,
        Snowflake? before = null,
        IProgress<Percentage>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var message in GetMessagesAsync(
                channelId,
                after,
                before,
                progress,
                false,
                cancellationToken
            )
        )
        {
            yield return message;
        }
    }

    public async IAsyncEnumerable<Message> GetMessagesInReverseAsync(
        Snowflake channelId,
        Snowflake? after = null,
        Snowflake? before = null,
        IProgress<Percentage>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var message in GetMessagesAsync(
                channelId,
                after,
                before,
                progress,
                true,
                cancellationToken
            )
        )
        {
            yield return message;
        }
    }

    public async IAsyncEnumerable<User> GetMessageReactionsAsync(
        Snowflake channelId,
        Snowflake messageId,
        Emoji emoji,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var reactionName = emoji.Id is not null
            // Custom emoji
            ? emoji.Name + ':' + emoji.Id
            // Standard emoji
            : emoji.Name;

        var currentAfter = Snowflake.Zero;
        while (true)
        {
            var url = new UrlBuilder()
                .SetPath(
                    $"channels/{channelId}/messages/{messageId}/reactions/{Uri.EscapeDataString(reactionName)}"
                )
                .SetQueryParameter("limit", "100")
                .SetQueryParameter("after", currentAfter.ToString())
                .Build();

            // Can be null on reactions with an emoji that has been deleted (?)
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/1226
            var response = await TryGetJsonResponseAsync(url, cancellationToken);
            if (response is null)
                yield break;

            var count = 0;
            foreach (var userJson in response.Value.EnumerateArray())
            {
                var user = User.Parse(userJson);
                yield return user;

                currentAfter = user.Id;
                count++;
            }

            if (count <= 0)
                yield break;
        }
    }

    // Official-web Chrome user agent. Must match X-Super-Properties.browser_user_agent.
    internal const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36";

    internal const int FallbackClientBuildNumber = 594503;

    // Client-mod detection bits (jQuery, BetterDiscord, Vencord, …).
    // https://docs.discord.food/reference#launch-signature
    internal static readonly UInt128 LaunchSignatureMask = new(
        0x0080101008100800,
        0x2081004001000800
    );

    internal static string GenerateLaunchSignature()
    {
        Span<byte> bytes = stackalloc byte[16];
        Guid.NewGuid().TryWriteBytes(bytes, bigEndian: true, out _);

        var value =
            new UInt128(
                BinaryPrimitives.ReadUInt64BigEndian(bytes),
                BinaryPrimitives.ReadUInt64BigEndian(bytes[8..])
            ) & ~LaunchSignatureMask;

        BinaryPrimitives.WriteUInt64BigEndian(bytes, (ulong)(value >> 64));
        BinaryPrimitives.WriteUInt64BigEndian(bytes[8..], (ulong)value);

        return new Guid(bytes, bigEndian: true).ToString();
    }

    internal static string EncodeXSuperProperties(
        int clientBuildNumber,
        string clientLaunchId,
        string launchSignature,
        string clientHeartbeatSessionId
    )
    {
        var buffer = new ArrayBufferWriter<byte>(512);
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("os", "Windows");
            writer.WriteString("browser", "Chrome");
            writer.WriteString("device", "");
            writer.WriteString("system_locale", "en-US");
            writer.WriteBoolean("has_client_mods", false);
            writer.WriteString("browser_user_agent", BrowserUserAgent);
            writer.WriteString("browser_version", "152.0.0.0");
            writer.WriteString("os_version", "10");
            writer.WriteString("referrer", "");
            writer.WriteString("referring_domain", "");
            writer.WriteString("referrer_current", "");
            writer.WriteString("referring_domain_current", "");
            writer.WriteString("release_channel", "stable");
            writer.WriteNumber("client_build_number", clientBuildNumber);
            writer.WriteNull("client_event_source");
            writer.WriteString("client_launch_id", clientLaunchId);
            writer.WriteString("launch_signature", launchSignature);
            writer.WriteString("client_heartbeat_session_id", clientHeartbeatSessionId);
            writer.WriteString("client_app_state", "unfocused");
            writer.WriteEndObject();
        }

        return Convert.ToBase64String(buffer.WrittenSpan);
    }

    internal static int? TryParseClientBuildNumber(string html)
    {
        const string marker = "\"BUILD_NUMBER\":\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
            return null;

        start += marker.Length;
        var end = html.IndexOf('"', start);
        if (end < 0)
            return null;

        return
            int.TryParse(
                html.AsSpan(start, end - start),
                CultureInfo.InvariantCulture,
                out var buildNumber
            )
            && buildNumber > 0
            ? buildNumber
            : null;
    }
}

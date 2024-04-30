using System;
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
using DiscordChatExporter.Core.Utils.Extensions;
using Gress;
using JsonExtensions.Http;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord;

public class DiscordClient(string token)
{
    private readonly Uri _baseUri = new("https://discord.com/api/v10/", UriKind.Absolute);
    private TokenKind? _resolvedTokenKind;

    private async ValueTask<HttpResponseMessage> GetResponseAsync(
        string url,
        TokenKind tokenKind,
        CancellationToken cancellationToken = default
    )
    {
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

                var response = await Http.Client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    innerCancellationToken
                );

                // If this was the last request available before hitting the rate limit,
                // wait out the reset time so that future requests can succeed.
                // This may add an unnecessary delay in case the user doesn't intend to
                // make any more requests, but implementing a smarter solution would
                // require properly keeping track of Discord's global/per-route/per-resource
                // rate limits and that's just way too much effort.
                // https://discord.com/developers/docs/topics/rate-limits
                var remainingRequestCount = response
                    .Headers.TryGetValue("X-RateLimit-Remaining")
                    ?.Pipe(s => int.Parse(s, CultureInfo.InvariantCulture));

                var resetAfterDelay = response
                    .Headers.TryGetValue("X-RateLimit-Reset-After")
                    ?.Pipe(s => double.Parse(s, CultureInfo.InvariantCulture))
                    .Pipe(TimeSpan.FromSeconds);

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

                return response;
            },
            cancellationToken
        );
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
                HttpStatusCode.Unauthorized
                    => throw new DiscordChatExporterException(
                        "Authentication token is invalid.",
                        true
                    ),

                HttpStatusCode.Forbidden
                    => throw new DiscordChatExporterException(
                        $"Request to '{url}' failed: forbidden."
                    ),

                HttpStatusCode.NotFound
                    => throw new DiscordChatExporterException(
                        $"Request to '{url}' failed: not found."
                    ),

                _
                    => throw new DiscordChatExporterException(
                        $"""
                        Request to '{url}' failed: {response
                            .StatusCode.ToString()
                            .ToSpaceSeparatedWords()
                            .ToLowerInvariant()}.
                        Response content: {await response.Content.ReadAsStringAsync(
                            cancellationToken
                        )}
                        """,
                        true
                    )
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

        var channels = (await GetGuildChannelsAsync(guildId, cancellationToken))
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

        // User accounts can only fetch threads using the search endpoint
        if (await ResolveTokenKindAsync(cancellationToken) == TokenKind.User)
        {
            // Active threads
            foreach (var channel in channels)
            {
                var currentOffset = 0;
                while (true)
                {
                    var url = new UrlBuilder()
                        .SetPath($"channels/{channel.Id}/threads/search")
                        .SetQueryParameter("sort_by", "last_message_time")
                        .SetQueryParameter("sort_order", "desc")
                        .SetQueryParameter("archived", "false")
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
                        // because threads are sorted by last message time.
                        if (after is not null && !thread.MayHaveMessagesAfter(after.Value))
                        {
                            breakOuter = true;
                            break;
                        }

                        yield return thread;
                        currentOffset++;
                    }

                    if (breakOuter)
                        break;

                    if (!response.Value.GetProperty("has_more").GetBoolean())
                        break;
                }
            }

            // Archived threads
            if (includeArchived)
            {
                foreach (var channel in channels)
                {
                    var currentOffset = 0;
                    while (true)
                    {
                        var url = new UrlBuilder()
                            .SetPath($"channels/{channel.Id}/threads/search")
                            .SetQueryParameter("sort_by", "last_message_time")
                            .SetQueryParameter("sort_order", "desc")
                            .SetQueryParameter("archived", "true")
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
                            // because threads are sorted by last message time.
                            if (after is not null && !thread.MayHaveMessagesAfter(after.Value))
                            {
                                breakOuter = true;
                                break;
                            }

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
            // Active threads
            {
                var parentsById = channels.ToDictionary(c => c.Id);

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

                    yield return Channel.Parse(threadJson, parent);
                }
            }

            // Archived threads
            if (includeArchived)
            {
                foreach (var channel in channels)
                {
                    // Public archived threads
                    {
                        // Can be null on certain channels
                        var response = await TryGetJsonResponseAsync(
                            $"channels/{channel.Id}/threads/archived/public",
                            cancellationToken
                        );

                        if (response is null)
                            continue;

                        foreach (
                            var threadJson in response.Value.GetProperty("threads").EnumerateArray()
                        )
                            yield return Channel.Parse(threadJson, channel);
                    }

                    // Private archived threads
                    {
                        // Can be null on certain channels
                        var response = await TryGetJsonResponseAsync(
                            $"channels/{channel.Id}/threads/archived/private",
                            cancellationToken
                        );

                        if (response is null)
                            continue;

                        foreach (
                            var threadJson in response.Value.GetProperty("threads").EnumerateArray()
                        )
                            yield return Channel.Parse(threadJson, channel);
                    }
                }
            }
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

        try
        {
            var parent = parentId is not null
                ? await GetChannelAsync(parentId.Value, cancellationToken)
                : null;

            return Channel.Parse(response, parent);
        }
        // It's possible for the parent channel to be inaccessible, despite the
        // child channel being accessible.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1108
        catch (DiscordChatExporterException)
        {
            return Channel.Parse(response);
        }
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

        var response = await GetJsonResponseAsync(url, cancellationToken);
        return response.EnumerateArray().Select(Message.Parse).LastOrDefault();
    }

    public async IAsyncEnumerable<Message> GetMessagesAsync(
        Snowflake channelId,
        Snowflake? after = null,
        Snowflake? before = null,
        IProgress<Percentage>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        // Get the last message in the specified range, so we can later calculate the
        // progress based on the difference between message timestamps.
        // This also snapshots the boundaries, which means that messages posted after
        // the export started will not appear in the output.
        var lastMessage = await TryGetLastMessageAsync(channelId, before, cancellationToken);
        if (lastMessage is null || lastMessage.Timestamp < after?.ToDate())
            yield break;

        // Keep track of the first message in range in order to calculate the progress
        var firstMessage = default(Message);

        var currentAfter = after ?? Snowflake.Zero;
        while (true)
        {
            var url = new UrlBuilder()
                .SetPath($"channels/{channelId}/messages")
                .SetQueryParameter("limit", "100")
                .SetQueryParameter("after", currentAfter.ToString())
                .Build();

            var response = await GetJsonResponseAsync(url, cancellationToken);

            var messages = response
                .EnumerateArray()
                .Select(Message.Parse)
                // Messages are returned from newest to oldest, so we need to reverse them
                .Reverse()
                .ToArray();

            // Break if there are no messages (can happen if messages are deleted during execution)
            if (!messages.Any())
                yield break;

            // If all messages are empty, make sure that it's not because the bot account doesn't
            // have the Message Content Intent enabled.
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/1106#issuecomment-1741548959
            if (
                messages.All(m => m.IsEmpty)
                && await ResolveTokenKindAsync(cancellationToken) == TokenKind.Bot
            )
            {
                var application = await GetApplicationAsync(cancellationToken);
                if (!application.IsMessageContentIntentEnabled)
                {
                    throw new DiscordChatExporterException(
                        "Provided bot account does not have the Message Content Intent enabled.",
                        true
                    );
                }
            }

            foreach (var message in messages)
            {
                firstMessage ??= message;

                // Ensure that the messages are in range
                if (message.Timestamp > lastMessage.Timestamp)
                    yield break;

                // Report progress based on timestamps
                if (progress is not null)
                {
                    var exportedDuration = (message.Timestamp - firstMessage.Timestamp).Duration();
                    var totalDuration = (lastMessage.Timestamp - firstMessage.Timestamp).Duration();

                    progress.Report(
                        Percentage.FromFraction(
                            // Avoid division by zero if all messages have the exact same timestamp
                            // (which happens when there's only one message in the channel)
                            totalDuration > TimeSpan.Zero
                                ? exportedDuration / totalDuration
                                : 1
                        )
                    );
                }

                yield return message;
                currentAfter = message.Id;
            }
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

            // Each batch can contain up to 100 users.
            // If we got fewer, then it's definitely the last batch.
            if (count < 100)
                yield break;
        }
    }
}

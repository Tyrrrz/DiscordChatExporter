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

public class DiscordClient
{
    private readonly string _token;
    private readonly Uri _baseUri = new("https://discord.com/api/v10/", UriKind.Absolute);

    private TokenKind? _resolvedTokenKind;

    public DiscordClient(string token) => _token = token;

    private async ValueTask<HttpResponseMessage> GetResponseAsync(
        string url,
        TokenKind tokenKind,
        CancellationToken cancellationToken = default)
    {
        return await Http.ResponseResiliencePolicy.ExecuteAsync(async innerCancellationToken =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, url));

            // Don't validate because the token can have special characters
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/828
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                tokenKind == TokenKind.Bot
                    ? $"Bot {_token}"
                    : _token
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
                .Headers
                .TryGetValue("X-RateLimit-Remaining")?
                .Pipe(s => int.Parse(s, CultureInfo.InvariantCulture));

            var resetAfterDelay = response
                .Headers
                .TryGetValue("X-RateLimit-Reset-After")?
                .Pipe(s => double.Parse(s, CultureInfo.InvariantCulture))
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
        }, cancellationToken);
    }

    private async ValueTask<TokenKind> GetTokenKindAsync(CancellationToken cancellationToken = default)
    {
        // Try authenticating as a user
        using var userResponse = await GetResponseAsync(
            "users/@me",
            TokenKind.User,
            cancellationToken
        );

        if (userResponse.StatusCode != HttpStatusCode.Unauthorized)
            return TokenKind.User;

        // Try authenticating as a bot
        using var botResponse = await GetResponseAsync(
            "users/@me",
            TokenKind.Bot,
            cancellationToken
        );

        if (botResponse.StatusCode != HttpStatusCode.Unauthorized)
            return TokenKind.Bot;

        throw DiscordChatExporterException.Unauthorized();
    }

    private async ValueTask<HttpResponseMessage> GetResponseAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var tokenKind = _resolvedTokenKind ??= await GetTokenKindAsync(cancellationToken);
        return await GetResponseAsync(url, tokenKind, cancellationToken);
    }

    private async ValueTask<JsonElement> GetJsonResponseAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => DiscordChatExporterException.Unauthorized(),
                HttpStatusCode.Forbidden => DiscordChatExporterException.Forbidden(),
                HttpStatusCode.NotFound => DiscordChatExporterException.NotFound(url),
                _ => DiscordChatExporterException.FailedHttpRequest(response)
            };
        }

        return await response.Content.ReadAsJsonAsync(cancellationToken);
    }

    private async ValueTask<JsonElement?> TryGetJsonResponseAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(url, cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsJsonAsync(cancellationToken)
            : null;
    }

    public async ValueTask<User?> TryGetUserAsync(
        Snowflake userId,
        CancellationToken cancellationToken = default)
    {
        var response = await TryGetJsonResponseAsync($"users/{userId}", cancellationToken);
        return response?.Pipe(User.Parse);
    }

    public async IAsyncEnumerable<Guild> GetUserGuildsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

            var isEmpty = true;
            foreach (var guild in response.EnumerateArray().Select(Guild.Parse))
            {
                yield return guild;

                currentAfter = guild.Id;
                isEmpty = false;
            }

            if (isEmpty)
                yield break;
        }
    }

    public async ValueTask<Guild> GetGuildAsync(
        Snowflake guildId,
        CancellationToken cancellationToken = default)
    {
        if (guildId == Guild.DirectMessages.Id)
            return Guild.DirectMessages;

        var response = await GetJsonResponseAsync($"guilds/{guildId}", cancellationToken);
        return Guild.Parse(response);
    }

    public async IAsyncEnumerable<Channel> GetGuildChannelsAsync(
        Snowflake guildId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (guildId == Guild.DirectMessages.Id)
        {
            var response = await GetJsonResponseAsync("users/@me/channels", cancellationToken);
            foreach (var channelJson in response.EnumerateArray())
                yield return Channel.Parse(channelJson);
        }
        else
        {
            var response = await GetJsonResponseAsync($"guilds/{guildId}/channels", cancellationToken);

            var responseOrdered = response
                .EnumerateArray()
                .OrderBy(j => j.GetProperty("position").GetInt32())
                .ThenBy(j => j.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse))
                .ToArray();

            var categories = responseOrdered
                .Where(j => j.GetProperty("type").GetInt32() == (int) ChannelKind.GuildCategory)
                .Select((j, index) => ChannelCategory.Parse(j, index + 1))
                .ToDictionary(j => j.Id.ToString(), StringComparer.Ordinal);

            // Discord positions are not deterministic, so we need to normalize them
            // because the user may refer to the channel position via file name template.
            var position = 0;

            foreach (var channelJson in responseOrdered)
            {
                var parentId = channelJson.GetPropertyOrNull("parent_id")?.GetNonWhiteSpaceStringOrNull();

                var category = !string.IsNullOrWhiteSpace(parentId)
                    ? categories.GetValueOrDefault(parentId)
                    : null;

                var channel = Channel.Parse(channelJson, category, position);

                position++;

                yield return channel;
            }
        }
    }

    public async IAsyncEnumerable<Role> GetGuildRolesAsync(
        Snowflake guildId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        CancellationToken cancellationToken = default)
    {
        if (guildId == Guild.DirectMessages.Id)
            return null;

        var response = await TryGetJsonResponseAsync($"guilds/{guildId}/members/{memberId}", cancellationToken);
        return response?.Pipe(j => Member.Parse(j, guildId));
    }

    public async ValueTask<Invite?> TryGetGuildInviteAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var response = await TryGetJsonResponseAsync($"invites/{code}", cancellationToken);
        return response?.Pipe(Invite.Parse);
    }

    public async ValueTask<ChannelCategory> GetChannelCategoryAsync(
        Snowflake channelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetJsonResponseAsync($"channels/{channelId}", cancellationToken);
            return ChannelCategory.Parse(response);
        }
        // In some cases, the Discord API returns an empty body when requesting channel category.
        // Instead, we use an empty channel category as a fallback.
        catch (DiscordChatExporterException)
        {
            return new ChannelCategory(channelId, "Unknown Category", 0);
        }
    }

    public async ValueTask<Channel> GetChannelAsync(
        Snowflake channelId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetJsonResponseAsync($"channels/{channelId}", cancellationToken);

        var parentId = response.GetPropertyOrNull("parent_id")?.GetNonWhiteSpaceStringOrNull()?.Pipe(Snowflake.Parse);

        var category = parentId is not null
            ? await GetChannelCategoryAsync(parentId.Value, cancellationToken)
            : null;

        return Channel.Parse(response, category);
    }

    private async ValueTask<Message?> TryGetLastMessageAsync(
        Snowflake channelId,
        Snowflake? before = null,
        CancellationToken cancellationToken = default)
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
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Get the last message in the specified range, so we can later calculate progress based on its date.
        // This also snapshots the boundaries, which means that messages posted after the export started
        // will not appear in the output.
        var lastMessage = await TryGetLastMessageAsync(channelId, before, cancellationToken);
        if (lastMessage is null || lastMessage.Timestamp < after?.ToDate())
            yield break;

        // Keep track of first message in range in order to calculate progress
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
                .Reverse() // reverse because messages appear newest first
                .ToArray();

            // Break if there are no messages (can happen if messages are deleted during execution)
            if (!messages.Any())
                yield break;

            foreach (var message in messages)
            {
                firstMessage ??= message;

                // Ensure messages are in range (take into account that last message could have been deleted)
                if (message.Timestamp > lastMessage.Timestamp)
                    yield break;

                // Report progress based on the duration of exported messages divided by total
                if (progress is not null)
                {
                    var exportedDuration = (message.Timestamp - firstMessage.Timestamp).Duration();
                    var totalDuration = (lastMessage.Timestamp - firstMessage.Timestamp).Duration();

                    progress.Report(Percentage.FromFraction(
                        // Avoid division by zero if all messages have the exact same timestamp
                        // (which may be the case if there's only one message in the channel)
                        totalDuration > TimeSpan.Zero
                            ? exportedDuration / totalDuration
                            : 1
                    ));
                }

                yield return message;
                currentAfter = message.Id;
            }
        }
    }
}
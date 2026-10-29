using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting;

// A member resolved from the snapshot file, along with the time it was captured at
public record MemberSnapshotEntry(Member Member, DateTimeOffset AsOf, string RawAsOf);

// Guild member data (nickname, roles) captured ahead of time and stored in a JSONL file,
// used instead of requesting each message author individually from Discord.
// Each line has the following shape:
// {"id": "<user id>", "guildId": "<guild id>", "asOf": "<ISO 8601 timestamp>", "member": <guild member object>}
public partial class MemberSnapshot
{
    private readonly IReadOnlyDictionary<Snowflake, MemberSnapshotEntry> _entriesByUserId;

    private MemberSnapshot(IReadOnlyDictionary<Snowflake, MemberSnapshotEntry> entriesByUserId) =>
        _entriesByUserId = entriesByUserId;

    public int Count => _entriesByUserId.Count;

    public MemberSnapshotEntry? TryGetEntry(Snowflake userId) =>
        _entriesByUserId.GetValueOrDefault(userId);
}

public partial class MemberSnapshot
{
    // Entries older than this are ignored, so that stale roles and nicknames are not exported
    public static TimeSpan MaxAge { get; } = TimeSpan.FromDays(30);

    public static MemberSnapshot Empty { get; } =
        new(new Dictionary<Snowflake, MemberSnapshotEntry>());

    public static MemberSnapshot Load(string? filePath, Snowflake guildId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return Empty;

        var entriesByUserId = new Dictionary<Snowflake, MemberSnapshotEntry>();
        var minAsOf = now - MaxAge;

        foreach (var line in File.ReadLines(filePath))
        {
            try
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;

                if (root.GetProperty("guildId").GetString() != guildId.ToString())
                    continue;

                var rawAsOf = root.GetProperty("asOf").GetString();
                if (
                    !DateTimeOffset.TryParse(
                        rawAsOf,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out var asOf
                    )
                    || asOf < minAsOf
                )
                {
                    continue;
                }

                var member = Member.Parse(root.GetProperty("member"), guildId);

                // If the same user appears more than once, the most recent entry wins
                if (
                    entriesByUserId.TryGetValue(member.Id, out var existing)
                    && existing.AsOf > asOf
                )
                {
                    continue;
                }

                entriesByUserId[member.Id] = new MemberSnapshotEntry(member, asOf, rawAsOf!);
            }
            catch (Exception ex)
                when (ex
                        is JsonException
                            or KeyNotFoundException
                            or InvalidOperationException
                            or FormatException
                            or ArgumentException
                )
            {
                // Malformed line: the member will be fetched from Discord instead
            }
        }

        return new MemberSnapshot(entriesByUserId);
    }

    public static MemberSnapshot Load(string? filePath, Snowflake guildId) =>
        Load(filePath, guildId, DateTimeOffset.UtcNow);
}

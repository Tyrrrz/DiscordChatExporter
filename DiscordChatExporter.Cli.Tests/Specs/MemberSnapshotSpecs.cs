using System;
using System.IO;
using System.Text.Json;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using PowerKit;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

// Pure unit tests, no Discord token required
public class MemberSnapshotSpecs
{
    private static readonly Snowflake GuildId = Snowflake.Parse("100000000000000001");
    private static readonly Snowflake OtherGuildId = Snowflake.Parse("100000000000000002");
    private static readonly Snowflake UserId = Snowflake.Parse("200000000000000001");
    private static readonly Snowflake RoleId = Snowflake.Parse("300000000000000001");

    private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    private static string FormatTimestamp(DateTimeOffset instant) =>
        instant.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

    private static string CreateLine(
        Snowflake guildId,
        string asOf,
        string? nick = "Nick",
        Snowflake? userId = null
    ) =>
        JsonSerializer.Serialize(
            new
            {
                id = (userId ?? UserId).ToString(),
                guildId = guildId.ToString(),
                asOf,
                member = new
                {
                    user = new
                    {
                        id = (userId ?? UserId).ToString(),
                        username = "user",
                        discriminator = "0",
                        bot = false,
                    },
                    nick,
                    roles = new[] { RoleId.ToString() },
                },
            }
        );

    private static MemberSnapshot Load(params string[] lines)
    {
        using var file = TempFile.Create();
        File.WriteAllLines(file.Path, lines);

        return MemberSnapshot.Load(file.Path, GuildId, Now);
    }

    [Fact]
    public void I_can_load_a_member_from_a_snapshot_file()
    {
        // Arrange
        var asOf = FormatTimestamp(Now.AddDays(-1));

        // Act
        var snapshot = Load(CreateLine(GuildId, asOf));

        // Assert
        snapshot.Count.Should().Be(1);

        var entry = snapshot.TryGetEntry(UserId);
        entry.Should().NotBeNull();
        entry.RawAsOf.Should().Be(asOf);
        entry.AsOf.Should().Be(Now.AddDays(-1));
        entry.Member.Id.Should().Be(UserId);
        entry.Member.User.Name.Should().Be("user");
        entry.Member.DisplayName.Should().Be("Nick");
        entry.Member.RoleIds.Should().Equal(RoleId);
    }

    [Fact]
    public void I_can_load_a_member_without_a_nickname_from_a_snapshot_file()
    {
        // Act
        var snapshot = Load(CreateLine(GuildId, FormatTimestamp(Now.AddDays(-1)), nick: null));

        // Assert
        var entry = snapshot.TryGetEntry(UserId);
        entry.Should().NotBeNull();
        entry.Member.DisplayName.Should().BeNull();
    }

    [Fact]
    public void I_can_load_a_snapshot_file_and_have_members_of_other_guilds_ignored()
    {
        // Act
        var snapshot = Load(CreateLine(OtherGuildId, FormatTimestamp(Now.AddDays(-1))));

        // Assert
        snapshot.Count.Should().Be(0);
        snapshot.TryGetEntry(UserId).Should().BeNull();
    }

    [Fact]
    public void I_can_load_a_snapshot_file_and_have_members_older_than_the_max_age_ignored()
    {
        // Arrange
        var otherUserId = Snowflake.Parse("200000000000000002");

        // Act
        var snapshot = Load(
            CreateLine(GuildId, FormatTimestamp(Now.AddDays(-31))),
            CreateLine(GuildId, FormatTimestamp(Now.AddDays(-29)), userId: otherUserId)
        );

        // Assert
        MemberSnapshot.MaxAge.Should().Be(TimeSpan.FromDays(30));
        snapshot.TryGetEntry(UserId).Should().BeNull();
        snapshot.TryGetEntry(otherUserId).Should().NotBeNull();
        snapshot.Count.Should().Be(1);
    }

    [Theory]
    [InlineData("not a date")]
    [InlineData("")]
    public void I_can_load_a_snapshot_file_and_have_members_with_an_invalid_timestamp_ignored(
        string asOf
    )
    {
        // Act
        var snapshot = Load(CreateLine(GuildId, asOf));

        // Assert
        snapshot.Count.Should().Be(0);
    }

    [Fact]
    public void I_can_load_a_snapshot_file_and_have_malformed_lines_ignored()
    {
        // Arrange
        var asOf = FormatTimestamp(Now.AddDays(-1));
        var otherUserId = Snowflake.Parse("200000000000000002");

        var missingMemberLine = JsonSerializer.Serialize(
            new
            {
                id = UserId.ToString(),
                guildId = GuildId.ToString(),
                asOf,
            }
        );

        var invalidRoleLine = CreateLine(GuildId, asOf, userId: otherUserId)
            .Replace(RoleId.ToString(), "not a snowflake");

        // Act
        var snapshot = Load(
            "{not json",
            "",
            "[1, 2, 3]",
            missingMemberLine,
            invalidRoleLine,
            CreateLine(GuildId, asOf)
        );

        // Assert
        snapshot.Count.Should().Be(1);
        snapshot.TryGetEntry(otherUserId).Should().BeNull();
        snapshot.TryGetEntry(UserId)!.Member.RoleIds.Should().Equal(RoleId);
    }

    [Fact]
    public void I_can_load_a_snapshot_file_and_have_the_most_recent_entry_win_for_duplicate_members()
    {
        // Arrange
        var newerAsOf = FormatTimestamp(Now.AddDays(-1));
        var olderAsOf = FormatTimestamp(Now.AddDays(-2));

        // Act
        // The newer entry is listed first, so that "last line wins" would fail this test
        var snapshot = Load(
            CreateLine(GuildId, newerAsOf, nick: "Newer"),
            CreateLine(GuildId, olderAsOf, nick: "Older")
        );

        // Assert
        var entry = snapshot.TryGetEntry(UserId);
        entry.Should().NotBeNull();
        entry.RawAsOf.Should().Be(newerAsOf);
        entry.Member.DisplayName.Should().Be("Newer");
    }

    [Fact]
    public void I_can_load_a_snapshot_file_and_have_a_later_more_recent_entry_replace_an_older_one()
    {
        // Arrange
        var olderAsOf = FormatTimestamp(Now.AddDays(-2));
        var newerAsOf = FormatTimestamp(Now.AddDays(-1));

        // Act
        var snapshot = Load(
            CreateLine(GuildId, olderAsOf, nick: "Older"),
            CreateLine(GuildId, newerAsOf, nick: "Newer")
        );

        // Assert
        snapshot.TryGetEntry(UserId)!.Member.DisplayName.Should().Be("Newer");
    }

    [Fact]
    public void I_can_load_a_snapshot_from_a_file_that_does_not_exist_and_get_an_empty_snapshot()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jsonl");

        // Act
        var snapshot = MemberSnapshot.Load(filePath, GuildId, Now);

        // Assert
        File.Exists(filePath).Should().BeFalse();
        snapshot.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void I_can_load_a_snapshot_without_a_file_path_and_get_an_empty_snapshot(
        string? filePath
    )
    {
        // Act
        var snapshot = MemberSnapshot.Load(filePath, GuildId, Now);

        // Assert
        snapshot.Count.Should().Be(0);
    }
}

using System;
using System.Buffers.Binary;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class ClientPropertiesSpecs
{
    [Fact]
    public void I_can_encode_official_web_super_properties_with_the_expected_shape()
    {
        // Arrange
        const string launchId = "00000000-0000-4000-8000-000000000001";
        const string launchSignature = "00000000-0000-4000-8000-000000000002";
        const string heartbeatId = "00000000-0000-4000-8000-000000000003";

        // Act
        var encoded = DiscordClient.EncodeXSuperProperties(
            DiscordClient.FallbackClientBuildNumber,
            launchId,
            launchSignature,
            heartbeatId
        );

        using var doc = JsonDocument.Parse(Convert.FromBase64String(encoded));
        var root = doc.RootElement;

        // Assert
        root.EnumerateObject()
            .Select(p => p.Name)
            .Should()
            .Equal(
                "os",
                "browser",
                "device",
                "system_locale",
                "has_client_mods",
                "browser_user_agent",
                "browser_version",
                "os_version",
                "referrer",
                "referring_domain",
                "referrer_current",
                "referring_domain_current",
                "release_channel",
                "client_build_number",
                "client_event_source",
                "client_launch_id",
                "launch_signature",
                "client_heartbeat_session_id",
                "client_app_state"
            );

        root.GetProperty("os").GetString().Should().Be("Windows");
        root.GetProperty("browser").GetString().Should().Be("Chrome");
        root.GetProperty("device").GetString().Should().BeEmpty();
        root.GetProperty("system_locale").GetString().Should().Be("en-US");
        root.GetProperty("has_client_mods").GetBoolean().Should().BeFalse();
        root.GetProperty("browser_user_agent")
            .GetString()
            .Should()
            .Be(DiscordClient.BrowserUserAgent);
        root.GetProperty("browser_version").GetString().Should().Be("152.0.0.0");
        root.GetProperty("os_version").GetString().Should().Be("10");
        root.GetProperty("referrer").GetString().Should().BeEmpty();
        root.GetProperty("referring_domain").GetString().Should().BeEmpty();
        root.GetProperty("referrer_current").GetString().Should().BeEmpty();
        root.GetProperty("referring_domain_current").GetString().Should().BeEmpty();
        root.GetProperty("release_channel").GetString().Should().Be("stable");
        root.GetProperty("client_build_number").ValueKind.Should().Be(JsonValueKind.Number);
        root.GetProperty("client_build_number")
            .GetInt32()
            .Should()
            .Be(DiscordClient.FallbackClientBuildNumber);
        root.GetProperty("client_event_source").ValueKind.Should().Be(JsonValueKind.Null);
        root.GetProperty("client_launch_id").GetString().Should().Be(launchId);
        root.GetProperty("launch_signature").GetString().Should().Be(launchSignature);
        root.GetProperty("client_heartbeat_session_id").GetString().Should().Be(heartbeatId);
        root.GetProperty("client_app_state").GetString().Should().Be("unfocused");
    }

    [Fact]
    public void User_agent_bytes_match_the_super_properties_browser_user_agent()
    {
        // Act
        var encoded = DiscordClient.EncodeXSuperProperties(
            DiscordClient.FallbackClientBuildNumber,
            Guid.NewGuid().ToString(),
            DiscordClient.GenerateLaunchSignature(),
            Guid.NewGuid().ToString()
        );

        using var doc = JsonDocument.Parse(Convert.FromBase64String(encoded));

        // Assert
        doc.RootElement.GetProperty("browser_user_agent")
            .GetString()
            .Should()
            .Be(DiscordClient.BrowserUserAgent);
    }

    [Fact]
    public void Launch_signature_clears_client_mod_detection_bits()
    {
        // Act
        var signatures = Enumerable
            .Range(0, 32)
            .Select(_ => DiscordClient.GenerateLaunchSignature())
            .ToArray();

        // Assert
        Span<byte> bytes = stackalloc byte[16];
        foreach (var signature in signatures)
        {
            var guid = Guid.Parse(signature);
            guid.TryWriteBytes(bytes, bigEndian: true, out _);

            var value = new UInt128(
                BinaryPrimitives.ReadUInt64BigEndian(bytes),
                BinaryPrimitives.ReadUInt64BigEndian(bytes[8..])
            );

            (value & DiscordClient.LaunchSignatureMask).Should().Be(UInt128.Zero);
            guid.ToString().Should().Be(signature);
        }
    }

    [Fact]
    public void I_can_parse_the_client_build_number_from_discord_app_html()
    {
        // Arrange
        const string html =
            """<script>window.GLOBAL_ENV = {"NODE_ENV":"production","BUILD_NUMBER":"594503","RELEASE_CHANNEL":"stable"}</script>""";

        // Act
        var buildNumber = DiscordClient.TryParseClientBuildNumber(html);

        // Assert
        buildNumber.Should().Be(594503);
        DiscordClient.TryParseClientBuildNumber("<html></html>").Should().BeNull();
        DiscordClient.TryParseClientBuildNumber("\"BUILD_NUMBER\":\"abc\"").Should().BeNull();
    }
}

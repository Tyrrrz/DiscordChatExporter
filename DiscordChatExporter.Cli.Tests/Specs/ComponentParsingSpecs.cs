using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class ComponentParsingSpecs
{
    [Fact]
    public void I_can_parse_a_link_button_component_from_a_message_payload()
    {
        // Arrange
        using var document = JsonDocument.Parse(
            """
            {
              "id": "123456789012345678",
              "type": 0,
              "author": {
                "id": "987654321098765432",
                "username": "Tester",
                "discriminator": "0",
                "avatar": null
              },
              "timestamp": "2026-02-25T00:00:00.000000+00:00",
              "content": "",
              "attachments": [],
              "components": [
                {
                  "type": 1,
                  "components": [
                    {
                      "type": 2,
                      "style": 5,
                      "label": "Direct Link",
                      "url": "https://www.example.com",
                      "custom_id": null,
                      "sku_id": null,
                      "disabled": false,
                      "emoji": {
                        "id": null,
                        "name": "📎",
                        "animated": false
                      }
                    }
                  ]
                }
              ],
              "embeds": [],
              "sticker_items": [],
              "reactions": [],
              "mentions": []
            }
            """
        );

        // Act
        var message = Message.Parse(document.RootElement);

        // Assert
        message.Components.Should().HaveCount(1);
        message.IsEmpty.Should().BeFalse();

        var actionRow = message.Components[0];
        actionRow.Components.Should().HaveCount(1);

        var button = actionRow.Components[0];
        button.Style.Should().Be(DiscordChatExporter.Core.Discord.Data.Components.ButtonStyle.Link);
        button.Label.Should().Be("Direct Link");
        button
            .Url.Should()
            .Be(
                "https://www.example.com"
            );
        button.IsUrlButton.Should().BeTrue();
        button.IsDisabled.Should().BeFalse();

        button.Emoji.Should().NotBeNull();
        button.Emoji!.Id.Should().BeNull();
        button.Emoji.Name.Should().Be("📎");
        button.Emoji.Code.Should().Be("paperclip");
    }
}

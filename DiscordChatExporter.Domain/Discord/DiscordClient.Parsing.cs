using System;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Internal;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Discord
{
    public partial class DiscordClient
    {
        private string ParseId(JsonElement json) =>
            json.GetProperty("id").GetString();

        private User ParseUser(JsonElement json)
        {
            var id = ParseId(json);
            var discriminator = json.GetProperty("discriminator").GetString().Pipe(int.Parse);
            var name = json.GetProperty("username").GetString();
            var avatarHash = json.GetProperty("avatar").GetString();
            var isBot = json.GetPropertyOrNull("bot")?.GetBoolean() ?? false;

            return new User(id, discriminator, name, avatarHash, isBot);
        }

        private Member ParseMember(JsonElement json)
        {
            var userId = json.GetProperty("user").Pipe(ParseId);
            var nick = json.GetPropertyOrNull("nick")?.GetString();

            var roleIds =
                json.GetPropertyOrNull("roles")?.EnumerateArray().Select(j => j.GetString()).ToArray() ??
                Array.Empty<string>();

            return new Member(userId, nick, roleIds);
        }

        private Guild ParseGuild(JsonElement json)
        {
            var id = ParseId(json);
            var name = json.GetProperty("name").GetString();
            var iconHash = json.GetProperty("icon").GetString();

            var roles =
                json.GetPropertyOrNull("roles")?.EnumerateArray().Select(ParseRole).ToArray() ??
                Array.Empty<Role>();

            return new Guild(id, name, iconHash, roles);
        }

        private Channel ParseChannel(JsonElement json)
        {
            var id = ParseId(json);
            var parentId = json.GetPropertyOrNull("parent_id")?.GetString();
            var type = (ChannelType) json.GetProperty("type").GetInt32();
            var topic = json.GetPropertyOrNull("topic")?.GetString();

            var guildId =
                json.GetPropertyOrNull("guild_id")?.GetString() ??
                Guild.DirectMessages.Id;

            var name =
                json.GetPropertyOrNull("name")?.GetString() ??
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(ParseUser).Select(u => u.Name).JoinToString(", ") ??
                id;

            return new Channel(id, guildId, parentId, type, name, topic);
        }

        private Role ParseRole(JsonElement json)
        {
            var id = ParseId(json);
            var name = json.GetProperty("name").GetString();
            var color = json.GetPropertyOrNull("color")?.GetInt32().Pipe(Color.FromArgb).ResetAlpha().NullIf(c => c.ToRgb() <= 0);
            var position = json.GetProperty("position").GetInt32();

            return new Role(id, name, color, position);
        }

        private Attachment ParseAttachment(JsonElement json)
        {
            var id = ParseId(json);
            var url = json.GetProperty("url").GetString();
            var width = json.GetPropertyOrNull("width")?.GetInt32();
            var height = json.GetPropertyOrNull("height")?.GetInt32();
            var fileName = json.GetProperty("filename").GetString();
            var fileSize = json.GetProperty("size").GetInt64().Pipe(FileSize.FromBytes);

            return new Attachment(id, url, fileName, width, height, fileSize);
        }

        private EmbedAuthor ParseEmbedAuthor(JsonElement json)
        {
            var name = json.GetPropertyOrNull("name")?.GetString();
            var url = json.GetPropertyOrNull("url")?.GetString();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetString();

            return new EmbedAuthor(name, url, iconUrl);
        }

        private EmbedField ParseEmbedField(JsonElement json)
        {
            var name = json.GetProperty("name").GetString();
            var value = json.GetProperty("value").GetString();
            var isInline = json.GetPropertyOrNull("inline")?.GetBoolean() ?? false;

            return new EmbedField(name, value, isInline);
        }

        private EmbedImage ParseEmbedImage(JsonElement json)
        {
            var url = json.GetPropertyOrNull("url")?.GetString();
            var width = json.GetPropertyOrNull("width")?.GetInt32();
            var height = json.GetPropertyOrNull("height")?.GetInt32();

            return new EmbedImage(url, width, height);
        }

        private EmbedFooter ParseEmbedFooter(JsonElement json)
        {
            var text = json.GetProperty("text").GetString();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetString();

            return new EmbedFooter(text, iconUrl);
        }

        private Embed ParseEmbed(JsonElement json)
        {
            var title = json.GetPropertyOrNull("title")?.GetString();
            var description = json.GetPropertyOrNull("description")?.GetString();
            var url = json.GetPropertyOrNull("url")?.GetString();
            var timestamp = json.GetPropertyOrNull("timestamp")?.GetDateTimeOffset();
            var color = json.GetPropertyOrNull("color")?.GetInt32().Pipe(Color.FromArgb).ResetAlpha();

            var author = json.GetPropertyOrNull("author")?.Pipe(ParseEmbedAuthor);
            var thumbnail = json.GetPropertyOrNull("thumbnail")?.Pipe(ParseEmbedImage);
            var image = json.GetPropertyOrNull("image")?.Pipe(ParseEmbedImage);
            var footer = json.GetPropertyOrNull("footer")?.Pipe(ParseEmbedFooter);

            var fields =
                json.GetPropertyOrNull("fields")?.EnumerateArray().Select(ParseEmbedField).ToArray() ??
                Array.Empty<EmbedField>();

            return new Embed(
                title,
                url,
                timestamp,
                color,
                author,
                description,
                fields,
                thumbnail,
                image,
                footer
            );
        }

        private Emoji ParseEmoji(JsonElement json)
        {
            var id = json.GetPropertyOrNull("id")?.GetString();
            var name = json.GetProperty("name").GetString();
            var isAnimated = json.GetPropertyOrNull("animated")?.GetBoolean() ?? false;

            return new Emoji(id, name, isAnimated);
        }

        private Reaction ParseReaction(JsonElement json)
        {
            var count = json.GetProperty("count").GetInt32();
            var emoji = json.GetProperty("emoji").Pipe(ParseEmoji);

            return new Reaction(emoji, count);
        }

        private Message ParseMessage(JsonElement json)
        {
            var id = ParseId(json);
            var channelId = json.GetProperty("channel_id").GetString();
            var timestamp = json.GetProperty("timestamp").GetDateTimeOffset();
            var editedTimestamp = json.GetPropertyOrNull("edited_timestamp")?.GetDateTimeOffset();
            var type = (MessageType) json.GetProperty("type").GetInt32();
            var isPinned = json.GetPropertyOrNull("pinned")?.GetBoolean() ?? false;

            var content = type switch
            {
                MessageType.RecipientAdd => "Added a recipient.",
                MessageType.RecipientRemove => "Removed a recipient.",
                MessageType.Call => "Started a call.",
                MessageType.ChannelNameChange => "Changed the channel name.",
                MessageType.ChannelIconChange => "Changed the channel icon.",
                MessageType.ChannelPinnedMessage => "Pinned a message.",
                MessageType.GuildMemberJoin => "Joined the server.",
                _ => json.GetPropertyOrNull("content")?.GetString() ?? ""
            };

            var author = json.GetProperty("author").Pipe(ParseUser);

            var attachments =
                json.GetPropertyOrNull("attachments")?.EnumerateArray().Select(ParseAttachment).ToArray() ??
                Array.Empty<Attachment>();

            var embeds =
                json.GetPropertyOrNull("embeds")?.EnumerateArray().Select(ParseEmbed).ToArray() ??
                Array.Empty<Embed>();

            var reactions =
                json.GetPropertyOrNull("reactions")?.EnumerateArray().Select(ParseReaction).ToArray() ??
                Array.Empty<Reaction>();

            var mentionedUsers =
                json.GetPropertyOrNull("mentions")?.EnumerateArray().Select(ParseUser).ToArray() ??
                Array.Empty<User>();

            return new Message(
                id,
                channelId,
                type,
                author,
                timestamp,
                editedTimestamp,
                isPinned,
                content,
                attachments,
                embeds,
                reactions,
                mentionedUsers
            );
        }
    }
}
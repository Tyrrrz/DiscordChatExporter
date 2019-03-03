using System;
using System.Drawing;
using System.Linq;
using ByteSizeLib;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService
    {
        private User ParseUser(JToken json)
        {
            var id = json["id"].Value<string>();
            var discriminator = json["discriminator"].Value<int>();
            var name = json["username"].Value<string>();
            var avatarHash = json["avatar"].Value<string>();

            return new User(id, discriminator, name, avatarHash);
        }

        private Guild ParseGuild(JToken json)
        {
            var id = json["id"].Value<string>();
            var name = json["name"].Value<string>();
            var iconHash = json["icon"].Value<string>();

            return new Guild(id, name, iconHash);
        }

        private Channel ParseChannel(JToken json)
        {
            // Get basic data
            var id = json["id"].Value<string>();
            var parentId = json["parent_id"]?.Value<string>();
            var type = (ChannelType) json["type"].Value<int>();
            var topic = json["topic"]?.Value<string>();

            // Try to extract guild ID
            var guildId = json["guild_id"]?.Value<string>();

            // If the guild ID is blank, it's direct messages
            if (guildId.IsBlank())
                guildId = Guild.DirectMessages.Id;

            // Try to extract name
            var name = json["name"]?.Value<string>();

            // If the name is blank, it's direct messages
            if (name.IsBlank())
                name = json["recipients"].Select(ParseUser).Select(u => u.Name).JoinToString(", ");

            return new Channel(id, parentId, guildId, name, topic, type);
        }

        private Role ParseRole(JToken json)
        {
            var id = json["id"].Value<string>();
            var name = json["name"].Value<string>();

            return new Role(id, name);
        }

        private Attachment ParseAttachment(JToken json)
        {
            var id = json["id"].Value<string>();
            var url = json["url"].Value<string>();
            var width = json["width"]?.Value<int>();
            var height = json["height"]?.Value<int>();
            var fileName = json["filename"].Value<string>();
            var fileSizeBytes = json["size"].Value<long>();

            var fileSize = ByteSize.FromBytes(fileSizeBytes);

            return new Attachment(id, width, height, url, fileName, fileSize);
        }

        private EmbedAuthor ParseEmbedAuthor(JToken json)
        {
            var name = json["name"]?.Value<string>();
            var url = json["url"]?.Value<string>();
            var iconUrl = json["icon_url"]?.Value<string>();

            return new EmbedAuthor(name, url, iconUrl);
        }

        private EmbedField ParseEmbedField(JToken json)
        {
            var name = json["name"].Value<string>();
            var value = json["value"].Value<string>();
            var isInline = json["inline"]?.Value<bool>() ?? false;

            return new EmbedField(name, value, isInline);
        }

        private EmbedImage ParseEmbedImage(JToken json)
        {
            var url = json["url"]?.Value<string>();
            var width = json["width"]?.Value<int>();
            var height = json["height"]?.Value<int>();

            return new EmbedImage(url, width, height);
        }

        private EmbedFooter ParseEmbedFooter(JToken json)
        {
            var text = json["text"].Value<string>();
            var iconUrl = json["icon_url"]?.Value<string>();

            return new EmbedFooter(text, iconUrl);
        }

        private Embed ParseEmbed(JToken json)
        {
            // Get basic data
            var title = json["title"]?.Value<string>();
            var description = json["description"]?.Value<string>();
            var url = json["url"]?.Value<string>();
            var timestamp = json["timestamp"]?.Value<DateTime>();

            // Get color
            var color = json["color"] != null
                ? Color.FromArgb(json["color"].Value<int>()).ResetAlpha()
                : Color.FromArgb(79, 84, 92); // default color

            // Get author
            var author = json["author"] != null ? ParseEmbedAuthor(json["author"]) : null;

            // Get fields
            var fields = json["fields"].EmptyIfNull().Select(ParseEmbedField).ToArray();

            // Get thumbnail
            var thumbnail = json["thumbnail"] != null ? ParseEmbedImage(json["thumbnail"]) : null;

            // Get image
            var image = json["image"] != null ? ParseEmbedImage(json["image"]) : null;

            // Get footer
            var footer = json["footer"] != null ? ParseEmbedFooter(json["footer"]) : null;

            return new Embed(title, url, timestamp, color, author, description, fields, thumbnail, image, footer);
        }

        private Emoji ParseEmoji(JToken json)
        {
            var id = json["id"]?.Value<string>();
            var name = json["name"]?.Value<string>();
            var isAnimated = json["animated"]?.Value<bool>() ?? false;

            return new Emoji(id, name, isAnimated);
        }

        private Reaction ParseReaction(JToken json)
        {
            var count = json["count"].Value<int>();
            var emoji = ParseEmoji(json["emoji"]);

            return new Reaction(count, emoji);
        }

        private Message ParseMessage(JToken json)
        {
            // Get basic data
            var id = json["id"].Value<string>();
            var channelId = json["channel_id"].Value<string>();
            var timestamp = json["timestamp"].Value<DateTime>();
            var editedTimestamp = json["edited_timestamp"]?.Value<DateTime?>();
            var content = json["content"].Value<string>();
            var type = (MessageType) json["type"].Value<int>();

            // Workarounds for non-default types
            if (type == MessageType.RecipientAdd)
                content = "Added a recipient.";
            else if (type == MessageType.RecipientRemove)
                content = "Removed a recipient.";
            else if (type == MessageType.Call)
                content = "Started a call.";
            else if (type == MessageType.ChannelNameChange)
                content = "Changed the channel name.";
            else if (type == MessageType.ChannelIconChange)
                content = "Changed the channel icon.";
            else if (type == MessageType.ChannelPinnedMessage)
                content = "Pinned a message.";
            else if (type == MessageType.GuildMemberJoin)
                content = "Joined the server.";

            // Get author
            var author = ParseUser(json["author"]);

            // Get attachments
            var attachments = json["attachments"].EmptyIfNull().Select(ParseAttachment).ToArray();

            // Get embeds
            var embeds = json["embeds"].EmptyIfNull().Select(ParseEmbed).ToArray();

            // Get reactions
            var reactions = json["reactions"].EmptyIfNull().Select(ParseReaction).ToArray();

            // Get mentioned users
            var mentionedUsers = json["mentions"].EmptyIfNull().Select(ParseUser).ToArray();

            return new Message(id, channelId, type, author, timestamp, editedTimestamp, content, attachments, embeds,
                reactions, mentionedUsers);
        }
    }
}
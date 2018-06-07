using System;
using System.Drawing;
using System.Linq;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Models.Embeds;
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

        private Role ParseRole(JToken json)
        {
            var id = json["id"].Value<string>();
            var name = json["name"].Value<string>();

            return new Role(id, name);
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

            return new Channel(id, guildId, name, topic, type);
        }

        private Attachment ParseAttachment(JToken json)
        {
            var id = json["id"].Value<string>();
            var url = json["url"].Value<string>();
            var isImage = json["width"] != null;
            var fileName = json["filename"].Value<string>();
            var fileSize = json["size"].Value<long>();

            return new Attachment(id, isImage, url, fileName, fileSize);
        }

        private EmbedFooter ParseEmbedFooter(JToken json)
        {
            var text = json["text"]?.Value<string>();
            var iconUrl = json["icon_url"]?.Value<string>();
            var proxyIconUrl = json["proxy_icon_url"]?.Value<string>();

            return new EmbedFooter(text, iconUrl, proxyIconUrl);
        }

        private EmbedImage ParseEmbedImage(JToken json)
        {
            var url = json["url"]?.Value<string>();
            var proxyUrl = json["proxy_url"]?.Value<string>();
            var height = json["height"]?.Value<int>();
            var width = json["width"]?.Value<int>();

            return new EmbedImage(url, proxyUrl, height, width);
        }

        private EmbedVideo ParseEmbedVideo(JToken json)
        {
            var url = json["url"]?.Value<string>();
            var height = json["height"]?.Value<int>();
            var width = json["width"]?.Value<int>();

            return new EmbedVideo(url, height, width);
        }

        private EmbedProvider ParseEmbedProvider(JToken json)
        {
            var name = json["name"]?.Value<string>();
            var url = json["url"]?.Value<string>();

            return new EmbedProvider(name, url);
        }

        private EmbedAuthor ParseEmbedAuthor(JToken json)
        {
            var name = json["name"]?.Value<string>();
            var url = json["url"]?.Value<string>();
            var iconUrl = json["icon_url"]?.Value<string>();
            var proxyIconUrl = json["proxy_icon_url"]?.Value<string>();

            return new EmbedAuthor(name, url, iconUrl, proxyIconUrl);
        }

        private EmbedField ParseEmbedField(JToken json)
        {
            var name = json["name"]?.Value<string>();
            var value = json["value"]?.Value<string>();
            var isInline = json["inline"]?.Value<bool>() ?? false;

            return new EmbedField(name, value, isInline);
        }

        private Embed ParseEmbed(JToken json)
        {
            // Get basic data
            var title = json["title"]?.Value<string>();
            var type = json["type"]?.Value<string>();
            var description = json["description"]?.Value<string>();
            var url = json["url"]?.Value<string>();
            var timestamp = json["timestamp"]?.Value<DateTime>();
            var color = json["color"] != null
                ? Color.FromArgb(json["color"].Value<int>())
                : (Color?) null;

            // Set color alpha to 1
            if (color != null)
                color = Color.FromArgb(1, color.Value);

            // Get footer
            var footer = json["footer"] != null ? ParseEmbedFooter(json["footer"]) : null;

            // Get image
            var image = json["image"] != null ? ParseEmbedImage(json["image"]) : null;

            // Get thumbnail
            var thumbnail = json["thumbnail"] != null ? ParseEmbedImage(json["thumbnail"]) : null;

            // Get video
            var video = json["video"] != null ? ParseEmbedVideo(json["video"]) : null;

            // Get provider
            var provider = json["provider"] != null ? ParseEmbedProvider(json["provider"]) : null;

            // Get author
            var author = json["author"] != null ? ParseEmbedAuthor(json["author"]) : null;

            // Get fields
            var fields = json["fields"].EmptyIfNull().Select(ParseEmbedField).ToArray();

            return new Embed(title, type, description, url, timestamp, color, footer, image, thumbnail, video, provider,
                author, fields);
        }

        private Message ParseMessage(JToken json)
        {
            // Get basic data
            var id = json["id"].Value<string>();
            var channelId = json["channel_id"].Value<string>();
            var timeStamp = json["timestamp"].Value<DateTime>();
            var editedTimeStamp = json["edited_timestamp"]?.Value<DateTime?>();
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

            return new Message(id, channelId, type, author, timeStamp, editedTimeStamp, content, attachments, embeds);
        }
    }
}
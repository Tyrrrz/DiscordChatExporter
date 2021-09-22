using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiscordChatExporter.Core.Discord.Data
{
    

    public partial class Temperatures
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("guild")]
        public Guild Guild { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("inviter")]
        public Inviter Inviter { get; set; }

        [JsonProperty("target_type")]
        public long TargetType { get; set; }

        [JsonProperty("target_user")]
        public Inviter TargetUser { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }
    }

    public partial class Guild
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("splash")]
        public object Splash { get; set; }

        [JsonProperty("banner")]
        public object Banner { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public object Icon { get; set; }

        [JsonProperty("features")]
        public List<string> Features { get; set; }

        [JsonProperty("verification_level")]
        public long VerificationLevel { get; set; }

        [JsonProperty("vanity_url_code")]
        public object VanityUrlCode { get; set; }
    }

    public partial class Inviter
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("discriminator")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Discriminator { get; set; }

        [JsonProperty("public_flags")]
    }

using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/emoji#emoji-object
    public partial class Emoji
    {
        // Only present on custom emoji
        public string? Id { get; }

        // Name of custom emoji (e.g. LUL) or actual representation of standard emoji (e.g. 🙂)
        public string Name { get; }

        // Name of custom emoji (e.g. LUL) or name of standard emoji (e.g. slight_smile)
        public string Code => !string.IsNullOrWhiteSpace(Id)
            ? Name
            : EmojiIndex.TryGetCode(Name) ?? Name;

        public bool IsAnimated { get; }

        public string ImageUrl { get; }

        public Emoji(string? id, string name, bool isAnimated, string imageUrl)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
            ImageUrl = imageUrl;
        }

        public override string ToString() => Name;
    }

    public partial class Emoji
    {
        private static string GetTwemojiName(string name) => name
            .GetRunes()
            // Variant selector rune is skipped in Twemoji names
            .Where(r => r.Value != 0xfe0f)
            .Select(r => r.Value.ToString("x"))
            .JoinToString("-");

        public static string GetImageUrl(string? id, string name, bool isAnimated)
        {
            // Custom emoji
            if (!string.IsNullOrWhiteSpace(id))
            {
                return isAnimated
                    ? $"https://cdn.discordapp.com/emojis/{id}.gif"
                    : $"https://cdn.discordapp.com/emojis/{id}.png";
            }

            // Standard emoji
            var twemojiName = GetTwemojiName(name);
            return $"https://twemoji.maxcdn.com/2/svg/{twemojiName}.svg";
        }

        public static Emoji Parse(JsonElement json)
        {
            var id = json.GetPropertyOrNull("id")?.GetString();
            var name = json.GetProperty("name").GetString();
            var isAnimated = json.GetPropertyOrNull("animated")?.GetBoolean() ?? false;

            var imageUrl = GetImageUrl(id, name, isAnimated);

            return new Emoji(id, name, isAnimated, imageUrl);
        }
    }
}
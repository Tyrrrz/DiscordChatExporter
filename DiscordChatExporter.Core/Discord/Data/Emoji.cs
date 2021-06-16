using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using JsonExtensions.Reading;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/emoji#emoji-object
    public partial class Emoji
    {
        public string? Id { get; }

        public string Name { get; }

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
        private static IEnumerable<Rune> GetRunes(string emoji)
        {
            var lastIndex = 0;
            while (lastIndex < emoji.Length && Rune.TryGetRuneAt(emoji, lastIndex, out var rune))
            {
                // Skip variant selector rune
                if (rune.Value != 0xfe0f)
                    yield return rune;

                lastIndex += rune.Utf16SequenceLength;
            }
        }

        private static string GetTwemojiName(IEnumerable<Rune> runes) =>
            runes.Select(r => r.Value.ToString("x")).JoinToString("-");

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
            var emojiRunes = GetRunes(name).ToArray();
            var twemojiName = GetTwemojiName(emojiRunes);
            return $"https://twemoji.maxcdn.com/2/72x72/{twemojiName}.png";
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
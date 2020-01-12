using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/emoji#emoji-object

    public partial class Emoji
    {
        public string? Id { get; }

        public string Name { get; }

        public bool IsAnimated { get; }

        public string ImageUrl { get; }

        public Emoji(string? id, string name, bool isAnimated)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;

            ImageUrl = GetImageUrl(id, name, isAnimated);
        }
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
                // Animated
                if (isAnimated)
                    return $"https://cdn.discordapp.com/emojis/{id}.gif";

                // Non-animated
                return $"https://cdn.discordapp.com/emojis/{id}.png";
            }

            // Standard unicode emoji
            var emojiRunes = GetRunes(name).ToArray();
            if (emojiRunes.Any())
            {
                // Get corresponding Twemoji image
                var twemojiName = GetTwemojiName(emojiRunes);
                return $"https://twemoji.maxcdn.com/2/72x72/{twemojiName}.png";
            }

            // Fallback in case of failure
            return name;
        }
    }
}
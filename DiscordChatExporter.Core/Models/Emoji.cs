using System.Collections.Generic;
using System.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/emoji#emoji-object

    public partial class Emoji
    {
        public string Id { get; }

        public string Name { get; }

        public bool IsAnimated { get; }

        public string ImageUrl
        {
            get
            {
                // Custom emoji
                if (Id.IsNotBlank())
                {
                    // Animated
                    if (IsAnimated)
                        return $"https://cdn.discordapp.com/emojis/{Id}.gif";

                    // Non-animated
                    return $"https://cdn.discordapp.com/emojis/{Id}.png";
                }

                // Standard unicode emoji (via twemoji)
                return $"https://twemoji.maxcdn.com/2/72x72/{GetTwemojiName(Name)}.png";
            }
        }

        public Emoji(string id, string name, bool isAnimated)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
        }
    }

    public partial class Emoji
    {
        private static IEnumerable<int> GetCodePoints(string emoji)
        {
            for (var i = 0; i < emoji.Length; i += char.IsHighSurrogate(emoji[i]) ? 2 : 1)
                yield return char.ConvertToUtf32(emoji, i);
        }

        private static string GetTwemojiName(string emoji)
            => GetCodePoints(emoji).Select(i => i.ToString("x")).JoinToString("-");
    }
}
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/emoji#emoji-object

    public class Emoji
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
                var codePoint = char.ConvertToUtf32(Name, 0).ToString("x");
                return $"https://twemoji.maxcdn.com/2/72x72/{codePoint}.png";
            }
        }

        public Emoji(string id, string name, bool isAnimated)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
        }
    }
}
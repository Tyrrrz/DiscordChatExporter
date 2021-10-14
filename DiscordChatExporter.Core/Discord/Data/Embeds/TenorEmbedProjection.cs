using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    public partial class TenorEmbedProjection
    {
        public string Url { get; }

        public TenorEmbedProjection(string url) => Url = url;

        [ExcludeFromCodeCoverage]
        public override string ToString() => Url;
    }

    public partial class TenorEmbedProjection
    {
        public static TenorEmbedProjection? TryResolve(Embed embed)
        {
            if (string.IsNullOrWhiteSpace(embed.Image?.Url))
                return null;

            // Has to be an image file
            var fileName = embed.Image?.Url;
            if (string.IsNullOrWhiteSpace(fileName))
                return null;
            if (!fileName.Contains("tenor"))
                return null;
            

            return new TenorEmbedProjection(fileName);
        }
    }
}
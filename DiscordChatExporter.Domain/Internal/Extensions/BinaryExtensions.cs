using System.Text;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class BinaryExtensions
    {
        public static string ToHex(this byte[] data)
        {
            var buffer = new StringBuilder();

            foreach (var t in data)
            {
                buffer.Append(t.ToString("X2"));
            }

            return buffer.ToString();
        }
    }
}
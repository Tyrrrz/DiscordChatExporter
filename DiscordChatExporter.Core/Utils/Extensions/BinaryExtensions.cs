using System.Globalization;
using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class BinaryExtensions
{
    public static string ToHex(this byte[] data, bool isUpperCase = true)
    {
        var buffer = new StringBuilder(2 * data.Length);

        foreach (var b in data)
        {
            buffer.Append(b.ToString(isUpperCase ? "X2" : "x2", CultureInfo.InvariantCulture));
        }

        return buffer.ToString();
    }
}

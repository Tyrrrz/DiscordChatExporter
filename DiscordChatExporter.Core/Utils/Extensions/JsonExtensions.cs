using System;
using System.Text.Json;

namespace DiscordChatExporter.Core.Utils.Extensions
{
    public static class JsonExtensions
    {
        public static string GetNonWhiteSpaceString(this JsonElement json)
        {
            if (json.ValueKind != JsonValueKind.String)
                throw new FormatException();

            var value = json.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException();

            return value;
        }
    }
}
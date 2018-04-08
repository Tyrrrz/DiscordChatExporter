using System.IO;
using System.Reflection;
using System.Resources;

namespace DiscordChatExporter.Core.Internal
{
    internal static class Extensions
    {
        public static string GetManifestResourceString(this Assembly assembly, string resourceName)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new MissingManifestResourceException($"Could not find resource [{resourceName}].");

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
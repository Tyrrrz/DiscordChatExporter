using System.IO;
using System.Reflection;
using System.Resources;

namespace DiscordChatExporter.Core.Internal
{
    internal static class AssemblyHelper
    {
        public static string GetResourceString(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
                throw new MissingManifestResourceException($"Could not find resource [{resourcePath}].");

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
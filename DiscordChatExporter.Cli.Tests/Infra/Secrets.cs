using System;
using System.IO;

namespace DiscordChatExporter.Cli.Tests.Infra
{
    public static class Secrets
    {
        private static readonly Lazy<string> DiscordTokenLazy = new(() =>
        {
            var fromEnvironment = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
                return fromEnvironment;

            var secretFilePath = Path.Combine(
                Path.GetDirectoryName(typeof(Secrets).Assembly.Location) ?? Directory.GetCurrentDirectory(),
                "DiscordToken.secret"
            );

            if (File.Exists(secretFilePath))
                return File.ReadAllText(secretFilePath);

            throw new InvalidOperationException("Discord token not provided for tests.");
        });

        private static readonly Lazy<bool> IsDiscordTokenBotLazy = new(() =>
        {
            // Default to true
            var fromEnvironment = Environment.GetEnvironmentVariable("DISCORD_TOKEN_BOT");
            if (string.IsNullOrWhiteSpace(fromEnvironment))
                return true;

            return string.Equals(fromEnvironment, "true", StringComparison.OrdinalIgnoreCase);
        });

        public static string DiscordToken => DiscordTokenLazy.Value;

        public static bool IsDiscordTokenBot => IsDiscordTokenBotLazy.Value;
    }
}
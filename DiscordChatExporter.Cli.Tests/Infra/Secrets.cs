using System;
using System.IO;

namespace DiscordChatExporter.Cli.Tests.Infra;

internal static class Secrets
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

    public static string DiscordToken => DiscordTokenLazy.Value;
}
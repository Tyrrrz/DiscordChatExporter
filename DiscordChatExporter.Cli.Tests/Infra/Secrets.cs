using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace DiscordChatExporter.Cli.Tests.Infra;

internal static class Secrets
{
    private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .AddEnvironmentVariables()
        .Build();

    public static string DiscordToken =>
        Configuration["DISCORD_TOKEN"]
        ?? throw new InvalidOperationException("Discord token not provided for tests.");
}

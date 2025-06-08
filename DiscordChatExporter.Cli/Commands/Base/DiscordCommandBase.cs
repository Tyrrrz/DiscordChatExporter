using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Cli.Commands.Base;

public abstract class DiscordCommandBase : ICommand
{
    [CommandOption(
        "token",
        't',
        EnvironmentVariable = "DISCORD_TOKEN",
        Description = "Authentication token."
    )]
    public required string Token { get; init; }

    [Obsolete("This option doesn't do anything. Kept for backwards compatibility.")]
    [CommandOption(
        "bot",
        'b',
        EnvironmentVariable = "DISCORD_TOKEN_BOT",
        Description = "This option doesn't do anything. Kept for backwards compatibility."
    )]
    public bool IsBotToken { get; init; } = false;

    [CommandOption(
        "respect-rate-limits",
        Description = "Whether to respect advisory rate limits. "
            + "If disabled, only hard rate limits (i.e. 429 responses) will be respected."
    )]
    public bool ShouldRespectRateLimits { get; init; } = true;

    [field: AllowNull, MaybeNull]
    protected DiscordClient Discord =>
        field ??= new DiscordClient(
            Token,
            ShouldRespectRateLimits ? RateLimitPreference.RespectAll : RateLimitPreference.IgnoreAll
        );

    public virtual ValueTask ExecuteAsync(IConsole console)
    {
#pragma warning disable CS0618
        // Warn if the bot option is used
        if (IsBotToken)
        {
            using (console.WithForegroundColor(ConsoleColor.DarkYellow))
            {
                console.Error.WriteLine(
                    "Warning: The --bot option is deprecated and should not be used. "
                        + "The token type is now inferred automatically. "
                        + "Please update your workflows as this option may be completely removed in a future version."
                );
            }
        }
#pragma warning restore CS0618

        // Note about interactivity for Docker
        if (console.IsOutputRedirected && Docker.IsRunningInContainer)
        {
            console.Error.WriteLine(
                "Note: Output streams are redirected, rich console interactions are disabled. "
                    + "If you are running this command in Docker, consider allocating a pseudo-terminal for better user experience (docker run -it ...)."
            );
        }

        return default;
    }
}

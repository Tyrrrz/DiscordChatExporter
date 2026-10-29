using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

// Pure unit tests, no Discord token required
public class CliOptionSpecs
{
    private static async Task<string> GetHelpTextAsync()
    {
        using var console = new FakeInMemoryConsole();

        var exitCode = await new CommandLineApplicationBuilder()
            .AddCommand(ExportChannelsCommand.Descriptor)
            .UseConsole(console)
            .Build()
            .RunAsync(["export", "--help"]);

        exitCode.Should().Be(0);

        return console.ReadOutputString();
    }

    [Theory]
    [InlineData("--member-snapshot", "DCE_MEMBER_SNAPSHOT")]
    public async Task I_can_see_the_environment_variable_bound_to_an_option_in_the_help_text(
        string optionName,
        string environmentVariableName
    )
    {
        // Act
        var helpText = await GetHelpTextAsync();

        // Assert
        var optionLine = helpText
            .Split('\n')
            .Select(l => l.Trim())
            .SingleOrDefault(l => l.StartsWith(optionName + " ", StringComparison.Ordinal));

        optionLine.Should().NotBeNull();
        optionLine.Should().Contain($"Environment variable: {environmentVariableName}.");
    }
}

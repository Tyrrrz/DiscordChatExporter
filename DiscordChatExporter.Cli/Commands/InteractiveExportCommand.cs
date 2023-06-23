using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Utils.Extensions;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Commands;

enum Screen
{
    Main,
    Guilds,
    GuildChannels,
    Export,
    Quit,
}

[Command("interactive", Description = "Interactive Export")]
public class InteractiveExportCommand : ExportCommandBase
{
    private readonly List<Channel> _selected = new();
    private Screen _screen = Screen.Main;
    private Guild? _selectedGuild;
    
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        while (true)
        {
            switch (_screen)
            {
                case Screen.Main:
                    await RenderMainScreen(console);
                    break;
                case Screen.Guilds:
                    await RenderGuildsScreen(cancellationToken);
                    break;
                case Screen.GuildChannels:
                    await RenderGuildChannelsScreen(cancellationToken);
                    break;
                case Screen.Export:
                    await console.Output.WriteAsync("Exporting: ");
                    await console.Output.WriteLineAsync(string.Join(", ", _selected.Select(c => $"{c.Category.Name} / {c.Name}").ToArray()));
                    await base.ExecuteAsync(console, _selected);
                    return;
                case Screen.Quit:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private async ValueTask RenderMainScreen(IConsole console)
    {
        await console.Output.WriteLineAsync("Discord Chat Exporter");
        await console.Output.WriteLineAsync("======================");
        if (_selected.Any())
        {
            await console.Output.WriteLineAsync("Selected channels:");
            await console.Output.WriteLineAsync(string.Join(", ", _selected.Select(c => $"{c.Category.Name} / {c.Name}").ToArray()));
        }
        
        var selection = AnsiConsole.Prompt<(Screen screen, string title)>(new SelectionPrompt<(Screen screen, string title)>()
            .Title("Select an option:")
            .AddChoices((Screen.Guilds, "Select Channels"), (Screen.Export, "Export"), (Screen.Quit, "Quit"))
            .UseConverter(i => i.Item2));
        
        _screen = selection.screen;
    }
    
    private async ValueTask RenderGuildsScreen(CancellationToken cancellationToken)
    {
        var options = (await Discord.GetUserGuildsAsync(cancellationToken))
            // Show direct messages first
            .OrderByDescending(g => g.Id == Guild.DirectMessages.Id)
            .ThenBy(g => g.Name)
            .Select<Guild, (Guild?, String)>(g => (g, g.Name))
            .Prepend((null, "Back"))
            .ToArray();
        var selection = AnsiConsole.Prompt<(Guild? guild, string name)>(new SelectionPrompt<(Guild? guild, string name)>()
            .Title("Select a guild:")
            .AddChoices(options)
            .UseConverter(i => i.Item2.EscapeMarkup()));
        if (selection.guild is not null)
        {
            _selectedGuild = selection.guild;
            _screen = Screen.GuildChannels;
        }
        else
        {
            _screen = Screen.Main;
        }
    }
    
    private async ValueTask RenderGuildChannelsScreen(CancellationToken cancellationToken)
    {
        if (_selectedGuild is null)
        {
            throw new InvalidOperationException("No guild selected");
        }
        
        var options = (await Discord.GetGuildChannelsAsync(_selectedGuild.Id, cancellationToken))
            .Where(c => c.Kind != ChannelKind.GuildCategory)
            .OrderBy(c => c.Category.Position)
            .ThenBy(c => c.Name)
            .Select<Channel, (Channel channel, string name)>(c => (c, $"{c.Category.Name} / {c.Name}"))
            .ToArray();
        var selections = AnsiConsole.Prompt(new MultiSelectionPrompt<(Channel channel, string name)>()
            .Title("Select channels:")
            .AddChoices(options)
            .UseConverter(i => i.name.EscapeMarkup()));
        
        _selected.RemoveAll(i => options.Any(o => o.channel == i));
        _selected.AddRange(selections.Select(s => s.channel));
        _screen = Screen.Guilds;
    }
}
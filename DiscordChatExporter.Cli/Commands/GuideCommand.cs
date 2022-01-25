using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace DiscordChatExporter.Cli.Commands;

[Command("guide", Description = "Explains how to obtain token, guild or channel ID.")]
public class GuideCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        // User token
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get user token:");

        console.Output.WriteLine(" 1. Open Discord in your web browser and login");
        console.Output.WriteLine(" 2. Press Ctrl+Shift+I to show developer tools");
        console.Output.WriteLine(" 3. Press Ctrl+Shift+M to toggle device toolbar");
        console.Output.WriteLine(" 4. Navigate to the Application tab");
        console.Output.WriteLine(" 5. On the left, expand Local Storage and select https://discord.com");
        console.Output.WriteLine(" 6. Type \"token\" into the Filter box");
        console.Output.WriteLine(" 7. If the token key does not appear, press Ctrl+R to reload");
        console.Output.WriteLine(" 8. Copy the value of the token key");
        console.Output.WriteLine(" *  Automating user accounts is technically against TOS, use at your own risk.");
        console.Output.WriteLine();

        // Bot token
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get bot token:");

        console.Output.WriteLine(" 1. Go to Discord developer portal");
        console.Output.WriteLine(" 2. Open your application's settings");
        console.Output.WriteLine(" 3. Navigate to the Bot section on the left");
        console.Output.WriteLine(" 4. Under Token click Copy");
        console.Output.WriteLine();

        // Guild or channel ID
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get guild ID or guild channel ID:");

        console.Output.WriteLine(" 1. Open Discord");
        console.Output.WriteLine(" 2. Open Settings");
        console.Output.WriteLine(" 3. Go to Appearance section");
        console.Output.WriteLine(" 4. Enable Developer Mode");
        console.Output.WriteLine(" 5. Right click on the desired guild or channel and click Copy ID");
        console.Output.WriteLine();

        // Direct message channel ID
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get direct message channel ID:");

        console.Output.WriteLine(" 1. Open Discord");
        console.Output.WriteLine(" 2. Open the desired direct message channel");
        console.Output.WriteLine(" 3. Press Ctrl+Shift+I to show developer tools");
        console.Output.WriteLine(" 4. Navigate to the Console tab");
        console.Output.WriteLine(" 5. Type \"window.location.href\" and press Enter");
        console.Output.WriteLine(" 6. Copy the first long sequence of numbers inside the URL");
        console.Output.WriteLine();

        // Wiki link
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("If you have questions or issues, please refer to the wiki:");
        using (console.WithForegroundColor(ConsoleColor.DarkCyan))
            console.Output.WriteLine("https://github.com/Tyrrrz/DiscordChatExporter/wiki");

        return default;
    }
}
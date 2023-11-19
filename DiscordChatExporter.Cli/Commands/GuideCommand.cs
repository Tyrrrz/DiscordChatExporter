using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace DiscordChatExporter.Cli.Commands;

[Command("guide", Description = "Explains how to obtain the token, guild or channel ID.")]
public class GuideCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        // User token
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get user token:");

        console
            .Output
            .WriteLine(
                " *  Automating user accounts is technically against TOS — USE AT YOUR OWN RISK!"
            );
        console.Output.WriteLine(" 1. Open Discord in your web browser and login");
        console.Output.WriteLine(" 2. Open any server or direct message channel");
        console.Output.WriteLine(" 3. Press Ctrl+Shift+I to show developer tools");
        console.Output.WriteLine(" 4. Navigate to the Network tab");
        console.Output.WriteLine(" 5. Press Ctrl+R to reload");
        console.Output.WriteLine(" 6. Switch between random channels to trigger network requests");
        console.Output.WriteLine(" 7. Search for a request that starts with \"messages\"");
        console.Output.WriteLine(" 8. Select the Headers tab on the right");
        console.Output.WriteLine(" 9. Scroll down to the Request Headers section");
        console.Output.WriteLine(" 10. Copy the value of the \"authorization\" header");
        console.Output.WriteLine();

        // Bot token
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get bot token:");

        console.Output.WriteLine(" 1. Go to Discord developer portal");
        console.Output.WriteLine(" 2. Open your application's settings");
        console.Output.WriteLine(" 3. Navigate to the Bot section on the left");
        console.Output.WriteLine(" 4. Under Token click Copy");
        console
            .Output
            .WriteLine(
                " *  Your bot needs to have Message Content Intent enabled to read messages"
            );
        console.Output.WriteLine();

        // Guild or channel ID
        using (console.WithForegroundColor(ConsoleColor.White))
            console.Output.WriteLine("To get guild ID or channel ID:");

        console.Output.WriteLine(" 1. Open Discord");
        console.Output.WriteLine(" 2. Open Settings");
        console.Output.WriteLine(" 3. Go to Advanced section");
        console.Output.WriteLine(" 4. Enable Developer Mode");
        console
            .Output
            .WriteLine(
                " 5. Right-click on the desired guild or channel and click Copy Server ID or Copy Channel ID"
            );
        console.Output.WriteLine();

        // Docs link
        using (console.WithForegroundColor(ConsoleColor.White))
            console
                .Output
                .WriteLine("If you have questions or issues, please refer to the documentation:");
        using (console.WithForegroundColor(ConsoleColor.DarkCyan))
            console
                .Output
                .WriteLine("https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs");

        return default;
    }
}

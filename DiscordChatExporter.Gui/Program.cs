using System;
using System.Reflection;
using Avalonia;
using DiscordChatExporter.Gui.Utils;

namespace DiscordChatExporter.Gui;

public static class Program
{
    private static Assembly Assembly { get; } = typeof(App).Assembly;

    public static string Name { get; } = Assembly.GetName().Name!;

    public static Version Version { get; } = Assembly.GetName().Version!;

    public static string VersionString { get; } = Version.ToString(3);

    public static string ProjectUrl { get; } = "https://github.com/Tyrrrz/DiscordChatExporter";

    public static string DocumentationUrl { get; } = ProjectUrl + "/tree/master/.docs";

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();

    [STAThread]
    public static int Main(string[] args)
    {
        // Build and run the app
        var builder = BuildAvaloniaApp();

        try
        {
            return builder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (OperatingSystem.IsWindows())
                _ = NativeMethods.Windows.MessageBox(0, ex.ToString(), "Fatal Error", 0x10);

            throw;
        }
        finally
        {
            // Clean up after application shutdown
            if (builder.Instance is IDisposable disposableApp)
                disposableApp.Dispose();
        }
    }
}

using System;
using System.IO;
using System.Reflection;

namespace DiscordChatExporter.Cli.Tests.Utils;

internal partial class TempDir(string path) : IDisposable
{
    public string Path { get; } = path;

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (DirectoryNotFoundException) { }
    }
}

internal partial class TempDir
{
    public static TempDir Create()
    {
        var dirPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? Directory.GetCurrentDirectory(),
            "Temp",
            Guid.NewGuid().ToString()
        );

        Directory.CreateDirectory(dirPath);

        return new TempDir(dirPath);
    }
}

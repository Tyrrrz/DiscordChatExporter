using System;
using System.IO;
using System.Reflection;
using PathEx = System.IO.Path;

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
        var dirPath = PathEx.Combine(
            PathEx.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? Directory.GetCurrentDirectory(),
            "Temp",
            Guid.NewGuid().ToString()
        );

        Directory.CreateDirectory(dirPath);

        return new TempDir(dirPath);
    }
}

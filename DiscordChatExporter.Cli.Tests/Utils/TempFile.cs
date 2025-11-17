using System;
using System.IO;
using System.Reflection;

namespace DiscordChatExporter.Cli.Tests.Utils;

internal partial class TempFile(string path) : IDisposable
{
    public string Path { get; } = path;

    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch (FileNotFoundException) { }
    }
}

internal partial class TempFile
{
    public static TempFile Create()
    {
        var dirPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? Directory.GetCurrentDirectory(),
            "Temp"
        );

        Directory.CreateDirectory(dirPath);

        var filePath = System.IO.Path.Combine(dirPath, Guid.NewGuid() + ".tmp");

        return new TempFile(filePath);
    }
}

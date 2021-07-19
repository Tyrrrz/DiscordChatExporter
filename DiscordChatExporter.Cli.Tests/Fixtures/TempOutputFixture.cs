using System;
using System.IO;

namespace DiscordChatExporter.Cli.Tests.Fixtures
{
    public class TempOutputFixture : IDisposable
    {
        public string DirPath => Path.Combine(
            Path.GetDirectoryName(typeof(TempOutputFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
            "Temp"
        );

        public TempOutputFixture() => Directory.CreateDirectory(DirPath);

        public string GetTempFilePath(string fileName) => Path.Combine(DirPath, fileName);

        public string GetTempFilePath() => GetTempFilePath(Guid.NewGuid().ToString());

        public void Dispose()
        {
            if (Directory.Exists(DirPath))
                Directory.Delete(DirPath, true);
        }
    }
}
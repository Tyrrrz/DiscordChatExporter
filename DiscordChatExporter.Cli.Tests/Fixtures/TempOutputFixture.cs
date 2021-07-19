using System;
using System.IO;

namespace DiscordChatExporter.Cli.Tests.Fixtures
{
    public class TempOutputFixture : IDisposable
    {
        public string DirPath => Path.Combine(
            Path.GetDirectoryName(typeof(TempOutputFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
            "Temp",
            Guid.NewGuid().ToString()
        );

        public TempOutputFixture() => Directory.CreateDirectory(DirPath);

        public string GetTempFilePath() => Path.Combine(DirPath, Guid.NewGuid().ToString());

        public string GetTempFilePath(string extension) => Path.ChangeExtension(GetTempFilePath(), extension);

        public void Dispose()
        {
            try
            {
                Directory.Delete(DirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
    }
}
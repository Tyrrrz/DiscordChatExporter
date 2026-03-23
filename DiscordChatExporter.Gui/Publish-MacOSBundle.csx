#!/usr/bin/env -S dotnet run
#:package CliFx

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

return await new CliApplicationBuilder()
    .AddCommand<PublishMacOSBundleCommand>()
    .Build()
    .RunAsync(args);

[Command(Description = "Publishes the GUI app as a macOS .app bundle.")]
public class PublishMacOSBundleCommand : ICommand
{
    [CommandOption("publish-dir", Description = "Path to the publish output directory.")]
    public required string PublishDirPath { get; init; }

    [CommandOption("icons-file", Description = "Path to the .icns icons file.")]
    public required string IconsFilePath { get; init; }

    [CommandOption("full-version", Description = "Full version string (e.g. '1.2.3.4').")]
    public required string FullVersion { get; init; }

    [CommandOption("short-version", Description = "Short version string (e.g. '1.2.3').")]
    public required string ShortVersion { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        // Set up paths
        var publishDirPath = Path.GetFullPath(PublishDirPath);
        var tempDirPath = Path.GetFullPath(
            Path.Combine(publishDirPath, "../publish-macos-app-temp")
        );
        var bundleName = "DiscordChatExporter.app";
        var bundleDirPath = Path.Combine(tempDirPath, bundleName);
        var contentsDirPath = Path.Combine(bundleDirPath, "Contents");
        var macosDirPath = Path.Combine(contentsDirPath, "MacOS");
        var resourcesDirPath = Path.Combine(contentsDirPath, "Resources");

        try
        {
            // Initialize the bundle's directory structure
            Directory.CreateDirectory(macosDirPath);
            Directory.CreateDirectory(resourcesDirPath);

            // Copy icons into the .app's Resources folder
            File.Copy(IconsFilePath, Path.Combine(resourcesDirPath, "AppIcon.icns"), true);

            // Generate the Info.plist metadata file with the app information
            // lang=xml
            var plistContent = $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                <plist version="1.0">
                  <dict>
                    <key>CFBundleDisplayName</key>
                    <string>DiscordChatExporter</string>
                    <key>CFBundleName</key>
                    <string>DiscordChatExporter</string>
                    <key>CFBundleExecutable</key>
                    <string>DiscordChatExporter</string>
                    <key>NSHumanReadableCopyright</key>
                    <string>© Oleksii Holub</string>
                    <key>CFBundleIdentifier</key>
                    <string>me.Tyrrrz.DiscordChatExporter</string>
                    <key>CFBundleSpokenName</key>
                    <string>Discord Chat Exporter</string>
                    <key>CFBundleIconFile</key>
                    <string>AppIcon</string>
                    <key>CFBundleIconName</key>
                    <string>AppIcon</string>
                    <key>CFBundleVersion</key>
                    <string>{FullVersion}</string>
                    <key>CFBundleShortVersionString</key>
                    <string>{ShortVersion}</string>
                    <key>NSHighResolutionCapable</key>
                    <true />
                    <key>CFBundlePackageType</key>
                    <string>APPL</string>
                  </dict>
                </plist>
                """;

            await File.WriteAllTextAsync(Path.Combine(contentsDirPath, "Info.plist"), plistContent);

            // Delete the previous bundle if it exists
            var existingBundlePath = Path.Combine(publishDirPath, bundleName);
            if (Directory.Exists(existingBundlePath))
                Directory.Delete(existingBundlePath, true);

            // Move all files from the publish directory into the MacOS directory
            foreach (var entry in Directory.GetFileSystemEntries(publishDirPath))
            {
                var destination = Path.Combine(macosDirPath, Path.GetFileName(entry));
                if (Directory.Exists(entry))
                    Directory.Move(entry, destination);
                else
                    File.Move(entry, destination);
            }

            // Move the final bundle into the publish directory for upload
            Directory.Move(bundleDirPath, Path.Combine(publishDirPath, bundleName));
        }
        finally
        {
            // Clean up the temporary directory
            if (Directory.Exists(tempDirPath))
                Directory.Delete(tempDirPath, true);
        }
    }
}

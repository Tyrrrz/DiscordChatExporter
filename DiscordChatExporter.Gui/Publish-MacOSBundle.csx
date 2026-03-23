// Publishes the GUI app as a macOS .app bundle.
// Usage: dotnet run Publish-MacOSBundle.csx -- <PublishDirPath> <IconsFilePath> <FullVersion> <ShortVersion>

if (args.Length < 4)
{
    Console.Error.WriteLine(
        "Usage: dotnet run Publish-MacOSBundle.csx -- <PublishDirPath> <IconsFilePath> <FullVersion> <ShortVersion>"
    );
    return 1;
}

var publishDirPath = Path.GetFullPath(args[0]);
var iconsFilePath = args[1];
var fullVersion = args[2];
var shortVersion = args[3];

// Setup paths
var tempDirPath = Path.GetFullPath(Path.Combine(publishDirPath, "../publish-macos-app-temp"));
var bundleName = "DiscordChatExporter.app";
var bundleDirPath = Path.Combine(tempDirPath, bundleName);
var contentsDirPath = Path.Combine(bundleDirPath, "Contents");
var macosDirPath = Path.Combine(contentsDirPath, "MacOS");
var resourcesDirPath = Path.Combine(contentsDirPath, "Resources");

try
{
    // Initialize the bundle's directory structure
    Directory.CreateDirectory(bundleDirPath);
    Directory.CreateDirectory(contentsDirPath);
    Directory.CreateDirectory(macosDirPath);
    Directory.CreateDirectory(resourcesDirPath);

    // Copy icons into the .app's Resources folder
    File.Copy(iconsFilePath, Path.Combine(resourcesDirPath, "AppIcon.icns"), overwrite: true);

    // Generate the Info.plist metadata file with the app information
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
    <string>{fullVersion}</string>
    <key>CFBundleShortVersionString</key>
    <string>{shortVersion}</string>
    <key>NSHighResolutionCapable</key>
    <true />
    <key>CFBundlePackageType</key>
    <string>APPL</string>
  </dict>
</plist>
""";

    File.WriteAllText(Path.Combine(contentsDirPath, "Info.plist"), plistContent);

    // Delete the previous bundle if it exists
    var existingBundlePath = Path.Combine(publishDirPath, bundleName);
    if (Directory.Exists(existingBundlePath))
        Directory.Delete(existingBundlePath, recursive: true);

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
        Directory.Delete(tempDirPath, recursive: true);
}

return 0;

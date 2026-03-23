#!/usr/bin/env -S dotnet run --

// Set up arguments
string GetArg(string name)
{
    var idx = Array.IndexOf(args, name);
    if (idx < 0 || idx + 1 >= args.Length)
        throw new InvalidOperationException($"Missing required option: {name}");
    return args[idx + 1];
}

var publishDirPathArg = GetArg("--publish-dir");
var iconsFilePathArg = GetArg("--icons-file");
var fullVersionArg = GetArg("--full-version");
var shortVersionArg = GetArg("--short-version");

const string BundleName = "DiscordChatExporter.app";
const string AppName = "DiscordChatExporter";
const string AppCopyright = "© Oleksii Holub";
const string AppIdentifier = "me.Tyrrrz.DiscordChatExporter";
const string AppSpokenName = "Discord Chat Exporter";
const string AppIconName = "AppIcon";

// Set up paths
var publishDirPath = Path.GetFullPath(publishDirPathArg);
var tempDirPath = Path.GetFullPath(Path.Combine(publishDirPath, "../publish-macos-app-temp"));

// Ensure the temporary directory is clean before use in case a previous run crashed
if (Directory.Exists(tempDirPath))
    Directory.Delete(tempDirPath, true);

var bundleDirPath = Path.Combine(tempDirPath, BundleName);
var contentsDirPath = Path.Combine(bundleDirPath, "Contents");

try
{
    // Copy icons into the .app's Resources folder
    Directory.CreateDirectory(Path.Combine(contentsDirPath, "Resources"));
    File.Copy(iconsFilePathArg, Path.Combine(contentsDirPath, "Resources", "AppIcon.icns"), true);

    // Generate the Info.plist metadata file with the app information
    // lang=xml
    var plistContent = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
        <plist version="1.0">
          <dict>
            <key>CFBundleDisplayName</key>
            <string>{AppName}</string>
            <key>CFBundleName</key>
            <string>{AppName}</string>
            <key>CFBundleExecutable</key>
            <string>{AppName}</string>
            <key>NSHumanReadableCopyright</key>
            <string>{AppCopyright}</string>
            <key>CFBundleIdentifier</key>
            <string>{AppIdentifier}</string>
            <key>CFBundleSpokenName</key>
            <string>{AppSpokenName}</string>
            <key>CFBundleIconFile</key>
            <string>{AppIconName}</string>
            <key>CFBundleIconName</key>
            <string>{AppIconName}</string>
            <key>CFBundleVersion</key>
            <string>{fullVersionArg}</string>
            <key>CFBundleShortVersionString</key>
            <string>{shortVersionArg}</string>
            <key>NSHighResolutionCapable</key>
            <true />
            <key>CFBundlePackageType</key>
            <string>APPL</string>
          </dict>
        </plist>
        """;

    await File.WriteAllTextAsync(Path.Combine(contentsDirPath, "Info.plist"), plistContent);

    // Delete the previous bundle if it exists
    var existingBundlePath = Path.Combine(publishDirPath, BundleName);
    if (Directory.Exists(existingBundlePath))
        Directory.Delete(existingBundlePath, true);

    // Move all files from the publish directory into the MacOS directory
    Directory.CreateDirectory(Path.Combine(contentsDirPath, "MacOS"));
    foreach (var entryPath in Directory.GetFileSystemEntries(publishDirPath))
    {
        var destinationPath = Path.Combine(contentsDirPath, "MacOS", Path.GetFileName(entryPath));

        if (Directory.Exists(entryPath))
            Directory.Move(entryPath, destinationPath);
        else
            File.Move(entryPath, destinationPath);
    }

    // Move the final bundle into the publish directory for upload
    Directory.Move(bundleDirPath, Path.Combine(publishDirPath, BundleName));
}
finally
{
    // Clean up the temporary directory
    if (Directory.Exists(tempDirPath))
        Directory.Delete(tempDirPath, true);
}

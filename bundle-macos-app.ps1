param(
    [Parameter(Mandatory=$true)]
    [string]$BundleName,

    [Parameter(Mandatory=$true)]
    [string]$PublishDir,

    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubSha
)

# Setup paths
$appName = "$BundleName.app"
$appDir = Join-Path "bundle-macos-app-staging" $appName
$contentsDir = Join-Path $appDir "Contents"
$macosDir = Join-Path $contentsDir "MacOS"
$resourcesDir = Join-Path $contentsDir "Resources"

# Create the macOS .app bundle directory structure
New-Item -ItemType Directory -Path $macosDir -Force
New-Item -ItemType Directory -Path $resourcesDir -Force

# Copy icon into the .app's Resources folder
Copy-Item -Path "favicon.icns" -Destination (Join-Path $resourcesDir "AppIcon.icns") -Force

# Generate Info.plist metadata file with app information
$plistContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleDisplayName</key>
    <string>$BundleName</string>
    <key>CFBundleName</key>
    <string>$BundleName</string>
    <key>CFBundleExecutable</key>
    <string>$BundleName</string>
    <key>NSHumanReadableCopyright</key>
    <string>© Oleksii Holub</string>
    <key>CFBundleIdentifier</key>
    <string>me.Tyrrrz.$BundleName</string>
    <key>CFBundleSpokenName</key>
    <string>Discord Chat Exporter</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundleIconName</key>
    <string>AppIcon</string>
    <key>CFBundleVersion</key>
    <string>$GitHubSha</string>
    <key>CFBundleShortVersionString</key>
    <string>$Version</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
  </dict>
</plist>
"@

Set-Content -Path (Join-Path $contentsDir "Info.plist") -Value $plistContent

# Move all files from the publish directory into the MacOS directory
Get-ChildItem -Path $PublishDir | ForEach-Object {
    Move-Item -Path $_.FullName -Destination $macosDir -Force
}

# Move the final .app bundle into the publish directory for upload
Move-Item -Path $appDir -Destination $PublishDir -Force
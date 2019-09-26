# --  GUI  --

$licenseFilePath = "$PSScriptRoot/../License.txt"

$projectDirPath = "$PSScriptRoot/../DiscordChatExporter.Gui"
$publishDirPath = "$PSScriptRoot/bin/build/"
$artifactFilePath = "$PSScriptRoot/bin/DiscordChatExporter.zip"

# Prepare directory
if (Test-Path $publishDirPath) {
    Remove-Item $publishDirPath -Recurse -Force
}
New-Item $publishDirPath -ItemType Directory -Force

# Build & publish
dotnet publish $projectDirPath -o $publishDirPath -c Release | Out-Host

$files = @()
$files += Get-Item -Path $licenseFilePath
$files += Get-ChildItem -Path $publishDirPath

# Pack into archive
$files | Compress-Archive -DestinationPath $artifactFilePath -Force


# --  CLI  --

$licenseFilePath = "$PSScriptRoot/../License.txt"

$projectDirPath = "$PSScriptRoot/../DiscordChatExporter.Cli"
$publishDirPath = "$PSScriptRoot/bin/build/"
$artifactFilePath = "$PSScriptRoot/bin/DiscordChatExporter.Cli.zip"

# Prepare directory
if (Test-Path $publishDirPath) {
    Remove-Item $publishDirPath -Recurse -Force
}
New-Item $publishDirPath -ItemType Directory -Force

# Build & publish
dotnet publish $projectDirPath -o $publishDirPath -c Release | Out-Host

$files = @()
$files += Get-Item -Path $licenseFilePath
$files += Get-ChildItem -Path $publishDirPath

# Pack into archive
$files | Compress-Archive -DestinationPath $artifactFilePath -Force
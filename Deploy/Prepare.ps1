$path = "..\DiscordChatExporter\bin\Release\*"
$include = "*.exe", "*.dll", "*.config"
$outputDir = "Output"
$outputFile = "DiscordChatExporter.zip"

# Create output directory
if (-Not (Test-Path $outputDir))
{
    New-Item $outputDir -ItemType Directory
}

# Delete output if already exists
if (Test-Path("$outputDir/$outputFile"))
{
    Remove-Item -Path "$outputDir/$outputFile"
}

# Get files
$files = Get-ChildItem -Path $path -Include $include

# Pack into archive
$files | Compress-Archive -DestinationPath "$outputDir/$outputFile"
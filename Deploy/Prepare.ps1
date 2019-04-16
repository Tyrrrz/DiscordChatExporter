New-Item "$PSScriptRoot\Portable\bin" -ItemType Directory -Force

# --- GUI ---

# Get files
$files = @()
$files += Get-Item -Path "$PSScriptRoot\..\License.txt"
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Gui\bin\Release\*" -Include "*.exe", "*.dll", "*.config"

# Pack into archive
$files | Compress-Archive -DestinationPath "$PSScriptRoot\Portable\bin\DiscordChatExporter.zip" -Force

# --- CLI ---

# Get files
$files = @()
$files += Get-Item -Path "$PSScriptRoot\..\License.txt"
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Cli\bin\Release\net46\*" -Include "*.exe", "*.dll", "*.config"

# Pack into archive
$files | Compress-Archive -DestinationPath "$PSScriptRoot\Portable\bin\DiscordChatExporter.CLI.zip" -Force
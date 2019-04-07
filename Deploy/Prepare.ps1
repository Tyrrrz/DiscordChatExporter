# --- PORTABLE / GUI ---

# Get files
$files = @()
$files += Get-Item -Path "$PSScriptRoot\..\License.txt"
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Gui\bin\Release\*" -Include "*.exe", "*.dll", "*.config"

# Pack into archive
New-Item "$PSScriptRoot\Portable\GUI\bin" -ItemType Directory -Force
$files | Compress-Archive -DestinationPath "$PSScriptRoot\Portable\GUI\bin\DiscordChatExporter.zip" -Force

# --- PORTABLE / CLI ---

# Get files
$files = @()
$files += Get-Item -Path "$PSScriptRoot\..\License.txt"
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Cli\bin\Release\net46\*" -Include "*.exe", "*.dll", "*.config"

# Pack into archive
New-Item "$PSScriptRoot\Portable\CLI\bin" -ItemType Directory -Force
$files | Compress-Archive -DestinationPath "$PSScriptRoot\Portable\CLI\bin\DiscordChatExporter.CLI.zip" -Force

# --- CHOCOLATEY ---

# Create package
New-Item "$PSScriptRoot\Choco\bin\" -ItemType Directory -Force
choco pack $PSScriptRoot\Choco\discordchatexporter.nuspec --out $PSScriptRoot\Choco\bin\
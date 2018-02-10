New-Item "$PSScriptRoot\bin" -ItemType Directory -Force

# GUI
$files = @()
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Gui\bin\Release\*" -Include "*.exe", "*.dll", "*.config"
$files | Compress-Archive -DestinationPath "$PSScriptRoot\bin\DiscordChatExporter.zip" -Force

# CLI
$files = @()
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Cli\bin\Release\net461\*" -Include "*.exe", "*.dll", "*.config"
$files | Compress-Archive -DestinationPath "$PSScriptRoot\bin\DiscordChatExporter.Cli.zip" -Force
New-Item "$PSScriptRoot\bin" -ItemType Directory -Force
$files = @()
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Gui\bin\Release\*" -Include "*.exe", "*.dll", "*.config"
$files += Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter.Cli\bin\Release\*" -Include "*.exe", "*.dll", "*.config"
$files | Compress-Archive -DestinationPath "$PSScriptRoot\bin\DiscordChatExporter.zip" -Force
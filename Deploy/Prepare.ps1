New-Item "$PSScriptRoot\bin" -ItemType Directory -Force
$files = Get-ChildItem -Path "$PSScriptRoot\..\DiscordChatExporter\bin\Release\*" -Include "*.exe", "*.dll", "*.config"
$files | Compress-Archive -DestinationPath "$PSScriptRoot\bin\DiscordChatExporter.zip" -Force
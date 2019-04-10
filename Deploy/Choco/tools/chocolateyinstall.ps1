$ErrorActionPreference = 'Stop'
$packageName = $env:ChocolateyPackageName
$installDirPath = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# Install package (GUI)

$packageArgs = @{
    packageName   = $packageName
    unzipLocation = $installDirPath
    url           = 'https://github.com/Tyrrrz/DiscordChatExporter/releases/download/2.12/DiscordChatExporter.zip'
}
Install-ChocolateyZipPackage @packageArgs

# Mark the executable as GUI
New-Item (Join-Path $installDirPath "DiscordChatExporter.exe.gui") -ItemType File -Force

# Install package (CLI)

$packageArgs = @{
    packageName   = $packageName
    unzipLocation = $installDirPath
    url           = 'https://github.com/Tyrrrz/DiscordChatExporter/releases/download/2.12/DiscordChatExporter.CLI.zip'
}
Install-ChocolateyZipPackage @packageArgs
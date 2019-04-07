$ErrorActionPreference = 'Stop';

# --- GUI ---

$packageArgs = @{
  packageName = $env:ChocolateyPackageName
  unzipLocation = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
  url = 'https://github.com/Tyrrrz/DiscordChatExporter/releases/download/2.11/DiscordChatExporter.zip'
}
Install-ChocolateyZipPackage @packageArgs

# --- CLI ---

$packageArgs = @{
  packageName = $env:ChocolateyPackageName
  unzipLocation = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
  url = 'https://github.com/Tyrrrz/DiscordChatExporter/releases/download/2.11/DiscordChatExporter.CLI.zip'
}
Install-ChocolateyZipPackage @packageArgs
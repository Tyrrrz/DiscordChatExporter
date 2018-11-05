param([string] $newVersion)

function Replace-TextInFile {
    param([string] $filePath, [string] $pattern, [string] $replacement)

    $content = [System.IO.File]::ReadAllText($filePath)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, $replacement)
    [System.IO.File]::WriteAllText($filePath, $content)
}

Replace-TextInFile "$PSScriptRoot\DiscordChatExporter.Core\DiscordChatExporter.Core.csproj" '(?<=<Version>)(.*?)(?=</Version>)' $newVersion
Replace-TextInFile "$PSScriptRoot\DiscordChatExporter.Cli\DiscordChatExporter.Cli.csproj" '(?<=<Version>)(.*?)(?=</Version>)' $newVersion
Replace-TextInFile "$PSScriptRoot\DiscordChatExporter.Gui\Properties\AssemblyInfo.cs" '(?<=Assembly.*?Version\(")(.*?)(?="\)\])' $newVersion
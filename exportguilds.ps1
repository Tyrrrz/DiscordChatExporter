# Info: https://github.com/Tyrrrz/DiscordChatExporter/wiki

$TOKEN = "<token>"
$TOKENTYPE = "<BOT/USER>"
$GUILDS = "<guild 1>", "<guild 2>", "<guild 3>", "<guild 4 etc.>"
$EXEPATH = "<exe>"
$FILENAME = "<name>"
$EXPORTDIRECTORY = "<dir>"
$EXPORTFORMAT = "<format>"
# Available export formats: PlainText, HtmlDark, HtmlLight, Csv

cd $EXEPATH

If($TOKENTYPE -match "BOT"){$ISBOTYES = "-b"}
Else{$ISBOTYES = ""}

foreach ($GUILD in $GUILDS){
	.\DiscordChatExporter.Cli.exe export -t $TOKEN $ISBOTYES -g $GUILD -f $EXPORTFORMAT -o exporttmp

	$Date = Get-Date -Format "yyyy-MMM-dd HH-mm"

	If($EXPORTFORMAT -match "PlainText"){mv exporttmp -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.txt"}
	ElseIf($EXPORTFORMAT -match "HtmlDark"){mv exporttmp -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.html"}
	ElseIf($EXPORTFORMAT -match "HtmlLight"){mv exporttmp -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.html"}
	ElseIf($EXPORTFORMAT -match "Csv"){mv exporttmp -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.csv"}
}
exit
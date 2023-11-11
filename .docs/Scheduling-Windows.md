# Scheduling exports on Windows

We'll be using [DiscordChatExporter CLI](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest), PowerShell, and Task Scheduler.

## Creating the script

1. Open a text editor such as Notepad and paste:

```console
# Info: https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs

$TOKEN = "tokenhere"
$CHANNEL = "channelhere"
$EXEPATH = "exefolderhere"
$FILENAME = "filenamehere"
$EXPORTDIRECTORY = "dirhere"
$EXPORTFORMAT = "formathere"
# Available export formats: PlainText, HtmlDark, HtmlLight, Json, Csv

cd $EXEPATH

.\DiscordChatExporter.Cli.exe export -t $TOKEN -c $CHANNEL -f $EXPORTFORMAT -o "$FILENAME.tmp"

$Date = Get-Date -Format "yyyy-MM-dd-HH-mm"

If($EXPORTFORMAT -match "PlainText"){mv "$FILENAME.tmp" -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.txt"}
ElseIf($EXPORTFORMAT -match "HtmlDark"){mv "$FILENAME.tmp" -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.html"}
ElseIf($EXPORTFORMAT -match "HtmlLight"){mv "$FILENAME.tmp" -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.html"}
ElseIf($EXPORTFORMAT -match "Json"){mv "$FILENAME.tmp" -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.json"}
ElseIf($EXPORTFORMAT -match "Csv"){mv "$FILENAME.tmp" -Destination "$EXPORTDIRECTORY\$FILENAME-$Date.csv"}
exit
```

2. Replace:

- `tokenhere` with your [Token](Token-and-IDs.md)
- `channelhere` with a [Channel ID](Token-and-IDs.md)
- `exefolderhere` with the .exe **directory's path** (e.g. C:\Users\User\Desktop\DiscordChatExporter)
- `filenamehere` with a filename without spaces
- `dirhere` with the export directory (e.g. C:\Users\User\Documents\Exports)
- `formathere` with one of the available export formats

Make sure not to delete the quotes (")

3. Save the file as `filename.ps1`, not as `.txt`

> **Note**: You can also modify the script to use other options, such as `include-threads` or switch to a different command, e. g. `exportguild`.

## Export at Startup

1. Press Windows + R, type `shell:startup` and press ENTER
2. Paste `filename.ps1` or a shortcut into this folder

## Scheduling with Task Scheduler

Please note that your computer must be turned on for the export to happen.

1. Press Windows + R, type `taskschd.msc` and press ENTER
2. Select `Task Scheduler Library`, create a Basic Task, and follow the instructions on-screen

<img src="https://i.imgur.com/MHRVGDi.png" height="500"/>

![Screenshot from Task Scheduler](https://i.imgur.com/m2DKhA8.png)

3. At 'Start a Program', write `powershell -file -ExecutionPolicy ByPass -WindowStyle Hidden "C:\path\to\filename.ps1"` in the Program/script text box

![](https://i.imgur.com/FGtWRod.png)

4. Click 'Yes'

![](https://i.imgur.com/DuaRBt3.png)

5. Click 'Finish'

![](https://i.imgur.com/LHgXp9Q.png)

---

Special thanks to [@Yudi](https://github.com/Yudi)

# Scheduling exports on macOS

Scheduling on macOS is a bit tricky, but it should work if you follow the instructions accordingly.

Make sure you already have **DiscordChatExporter.CLI** and **.NET Core** properly installed ([instructions here](MacOS.md)).

## Creating the script

1. Open TextEdit.app and create a new file

2. Convert the file to a plain text one in 'Format > Make Plain Text' (⇧⌘T)

![](https://i.imgur.com/WXrTtXM.png)

3. Paste the following into the text editor:

```bash
#!/bin/bash
# Info: https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs

TOKEN=tokenhere
CHANNELID=channelhere
DLLFOLDER=dceFOLDERpathhere
FILENAME=filenamehere
EXPORTDIRECTORY=dirhere
EXPORTFORMAT=formathere
# Available export formats: plaintext, htmldark, htmllight, json, csv
# /\ CaSe-SeNsItIvE /\
# You can edit the export command on line 43 if you'd like to include more options like date ranges and date format. You can't use partitioning (-p) with this script.

# This variable specifies in which directories the executable programs are located. Don't change it.
PATH=/usr/bin:/bin:/usr/sbin:/sbin:/usr/local/share/dotnet

# This will verify if EXPORTFORMAT is valid and will set the final file extension according to it. If the format is invalid, the script will display a message and exit.
if [[ "$EXPORTFORMAT" == "plaintext" ]]; then
FORMATEXT=.txt
elif [[ "$EXPORTFORMAT" == "htmldark" ]] || [[ "$EXPORTFORMAT" == "htmllight" ]]; then
FORMATEXT=.html
elif [[ "$EXPORTFORMAT" == "json" ]]; then
FORMATEXT=.json
elif [[ "$EXPORTFORMAT" == "csv" ]]; then
FORMATEXT=.csv
else
echo "$EXPORTFORMAT - Unknown export format"
echo "Available export formats: plaintext, htmldark, htmllight, csv, json"
echo "/\ CaSe-SeNsItIvE /\\"
exit 1
fi

# This will change the script's directory to DLLPATH, if unable to do so, the script will exit.
cd $DLLFOLDER || exit 1

# This will export your chat
dotnet DiscordChatExporter.Cli.dll export -t $TOKEN -c $CHANNELID -f $EXPORTFORMAT -o $FILENAME.tmp

# This sets the current time to a variable
CURRENTTIME=$(date +"%Y-%m-%d-%H-%M-%S")

# This will move the .tmp file to the desired export location. If unable to do so, it will attempt to delete the .tmp file.
if ! mv "$FILENAME.tmp" "${EXPORTDIRECTORY//\"}/$FILENAME-$CURRENTTIME$FORMATEXT" ; then
echo "Unable to move $FILENAME.tmp to $EXPORTDIRECTORY/$FILENAME-$CURRENTTIME$FORMATEXT."
echo "Cleaning up..."
  if ! rm -Rf "$FILENAME.tmp" ; then
  echo "Unable to remove $FILENAME.tmp."
  fi
exit 1
fi
exit 0
```

4. Replace:

- `tokenhere` with your [Token](Token-and-IDs.md)
- `channelhere` with a [Channel ID](Token-and-IDs.md)
- `dceFOLDERpathhere` with DCE's **directory's path** (e.g. `/Users/user/Desktop/DiscordChatExporterFolder`, NOT `/Users/user/Desktop/DiscordChatExporterFolder/DiscordChatExporter.DLL`)
- `filenamehere` with the exported channel's filename, without spaces
- `dirhere` with the directory you want the files to be saved at (e.g. `/Users/user/Documents/Discord\ Exports`)
- `formathere` with one of the available export formats

To quickly get file or folder paths, select the file/folder, then hit Command+I (⌘I) and copy what's after `Where:`.
After copying and pasting, make sure the file/folder name is not missing. If a folder has spaces in its name, add `\` before the spaces, like in the example below:

- `Discord\ Exports` - Wrong ✗
- `/Users/user/Documents` - Wrong ✗
- `/Users/user/Documents/Discord Exports` - Wrong ✗
- `/Users/user/Documents/Discord\ Exports/DCE.Cli.dll` - Wrong ✗
- `/Users/user/Documents/Discord \Exports` - Wrong ✗
- `/Users/user/Documents/Discord\ Exports` - Correct ✓
- `/Users/user/Desktop/DiscordChatExporter` - Correct ✓

![Screenshot of mac info window](https://i.imgur.com/29u6Nyx.png)

5. Save the file as `filename.sh`, not `.txt`
6. Open Terminal.app, type `chmod +x`, press the SPACE key, then drag & drop the `filename.sh` into the Terminal window and hit RETURN. You may be prompted for your password, and you won't be able to see it as you type.

## Creating the .plist file

Open TextEdit, make a Plain Text (⇧⌘T) and then paste the following into it:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>Label</key>
    <string>local.discordchatexporter</string>
    <key>Program</key>
    <string>/path/to/filename.sh</string>
    REPLACEME
  </dict>
</plist>
```

- The `Label` string is the name of the export job, it must be something unique. Replace the `local.discordchatexporter` between the `<string>` with another name if you'd like to run more than one script.
- The `Program` string is the path to the script. Replace `/path/to/filename.sh` between the `<string>` with the path of the previously created script.
- Replace the `REPLACEME` with the content presented in the following sections according to <u>when</u> you want to export.

When you're done, save the file with the same name as the `Label` and with the `.plist` extension (not `.txt`), like `local.discordchatexporter.plist`.

### Exporting on System Boot/User Login

```xml
<key>RunAtLoad</key>
<true/>
```

### Export every _n_ seconds

The following example is to export every 3600 seconds (1 hour), replace the integer value with your desired time:

```xml
<key>StartInterval</key>
<integer>3600</integer>
```

### Export at a specific time and date

```xml
<key>StartCalendarInterval</key>
<dict>
  <key>Weekday</key>
  <integer>0</integer>
  <key>Month</key>
  <integer>0</integer>
  <key>Day</key>
  <integer>0</integer>
  <key>Hour</key>
  <integer>0</integer>
  <key>Minute</key>
  <integer>0</integer>
</dict>
```

| Key         | Integer           |
| ----------- | ----------------- |
| **Month**   | 1-12              |
| **Day**     | 1-31              |
| **Weekday** | 0-6 (0 is Sunday) |
| **Hour**    | 0-23              |
| **Minute**  | 0-59              |

**Sunday** - 0; **Monday** - 1; **Tuesday** - 2; **Wednesday** - 3; **Thursday** - 4; **Friday** - 5; **Saturday** - 6

Replace the template's `0`s according to the desired times.

You can delete the `<key>`s you don't need, don't forget to remove the `<integer>0</integer>` under it.
Omitted keys are interpreted as wildcards, for example, if you delete the Minute key, the script will run at every minute, delete the Weekday key and it'll run at every weekday, and so on.

Be aware that if you set the day to '31', the script will only run on months that have the 31st day.

**Check the examples below ([or skip to step 3 (loading the file)](#3-loading-the-plist-into-launchctl)):**

Export everyday at 5:15 PM:

```xml
<key>StartCalendarInterval</key>
<dict>
  <key>Hour</key>
  <integer>17</integer>
  <key>Minute</key>
  <integer>15</integer>
</dict>

```

Every 15 minutes of an hour (xx:15):

```xml
<key>StartCalendarInterval</key>
<dict>
  <key>Minute</key>
  <integer>15</integer>
</dict>

```

Every Sunday at midnight and every Wednesday full hour (xx:00). Notice the inclusion of `<array>` and `</array>` to allow multiple values:

```xml
<key>StartCalendarInterval</key>
<array>
  <dict>
    <key>Weekday</key>
    <integer>0</integer>
    <key>Hour</key>
    <integer>00</integer>
    <key>Minute</key>
    <integer>00</integer>
  </dict>
  <dict>
    <key>Weekday</key>
    <integer>3</integer>
    <key>Minute</key>
    <integer>00</integer>
  </dict>
</array>
```

## Loading the .plist into launchctl

1. Copy your `filename.plist` file to one of these folders according to how you want it to run:

- `~/Library/LaunchAgents` runs as the current logged-in user.

- `/Library/LaunchDaemons` runs as the system "_administrator_" (root).

- If macOS has a single user:
  - If you want to export only when the user is logged in, choose the first one.
  - If you want the script to always run on System Startup, choose the second one.
- If macOS has multiple users:
  - If you want the script to run only when a certain user is logged in, choose the first one.
  - If you want the script to always run on System Startup, choose the second one.

To quickly go to these directories, open Finder and press Command+Shift+G (⌘⇧G), then paste the path into the text box.

2. To load the job into launchctl, in Terminal, type `launchctl load`, press SPACE, drag and drop the `.plist` into the Terminal window, then hit RETURN. It won't output anything if it was successfully loaded.

### Extra launchctl commands

**Unloading a job**

```
launchctl unload /path/to/Library/LaunchAgents/local.discordchatexporter.plist
```

**List every loaded job**

```
launchctl list
```

**Check if a specific job is enabled**
You can also see error codes (2nd number) by running this command.

```
launchctl list | grep local.discordchatexporter
```

---

Further reading: [Script management with launchd in Terminal on Mac](https://support.apple.com/guide/terminal/script-management-with-launchd-apdc6c1077b-5d5d-4d35-9c19-60f2397b2369/mac) and [launchd.info](https://launchd.info/).
Special thanks to [@Yudi](https://github.com/Yudi)

# Scheduling exports with Cron

Make sure you already have **DiscordChatExporter.CLI** and **.NET Core** properly installed ([instructions here](Linux.md)).

## Creating the script

1. Open Terminal and create a new text file with `nano /path/to/DiscordChatExporter/cron.sh`

> **Note**:
> You can't use your mouse in nano, use the arrow keys to control the cursor (caret).

2. Paste the following into the text file:

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
# You can edit the export command on line 40 if you'd like to include more options like date ranges and date format. You can't use partitioning (-p) with this script.

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

# This will move the .tmp file to the desired export location, if unable to do so, it will attempt to delete the .tmp file.
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

3. Replace:

- `tokenhere` with your [Token](Token-and-IDs.md).
- `channelhere` with a [Channel ID](Token-and-IDs.md).
- `dceFOLDERpathhere` with DCE's **directory path** (e.g. `/path/to/folder`, NOT `/path/to/folder/DiscordChatExporter.dll`).
- `filenamehere` with the exported channel's filename, without spaces.
- `dirhere` with the export directory (e.g. /home/user/Documents/Discord\ Exports).
- `formathere` with one of the available export formats.

> **Note**:
> Remember to escape spaces (add `\` before them) or to quote (") the paths (`"/home/my user"`)!

> **Note**:
> To save, hold down CTRL and then press O, if asked for a filename, type it and press ENTER. Hit CTRL+X to exit the text editor.
> [Check out this page](https://wiki.gentoo.org/wiki/Nano/Basics_Guide) if you want to know more about nano.

4. Make your script executable with `chmod +x /path/to/DiscordChatExporter/cron.sh`

5. Let's edit the cron file. If you want to run the script with your user privileges, edit it by running `crontab -e`. If you want to run the script as root, edit it with `sudo crontab -e`. If this is your first time running this command, you might be asked to select a text editor. Nano is easier for beginners.

6. Add the following to the end of the file `* * * * * /path/to/DiscordChatExporter/cron.sh >/tmp/discordchatexporter.log 2>/tmp/discordchatexportererror.log`. Don't forget to replace the `/path/to/DiscordChatExporter/cron.sh`!

> **Note**:
> If you don't want logs to be created, replace both `/tmp/discordchatexporter.log` with `/dev/null`.

Then replace the \*s according to:

![](https://i.imgur.com/RY7USM6.png)

---

**Examples**:

- If you want to execute the script at minute 15 of every hour: `15 * * * *`
- Every 30 minutes `*/30 * * * *`
- Every day at midnight `0 0 * * *`
- Every day at noon `0 12 * * *`
- Every day at 3, 4 and 6 PM `0 15,16,18 * * *`
- Every Wednesday at 9 AM `0 9 * * 3`

Verify your cron time [here](https://crontab.guru).

---

**Additional information**

The week starts on Sunday. 0 = SUN, 1 = MON ... 7 = SUN.

Be aware that if you set the day to '31', the script will only run on months that have the 31st day.

> [Learn more about running a cron job on the last day of the month here](https://stackoverflow.com/questions/6139189/cron-job-to-run-on-the-last-day-of-the-month) (expert).

The default filename for the exported channel is `YYYY-MM-DD-hh-mm-ss-yourfilename`. You can change it if you'd like.

Don't forget to update your token in the script after it has been reset!

---

Special thanks to [@Yudi](https://github.com/Yudi)

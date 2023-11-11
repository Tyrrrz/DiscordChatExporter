# Getting started

Welcome to the getting started page!
Here you'll learn how to use every **DiscordChatExporter** (DCE for short) feature.
For other things you can do with DCE, check out the [Guides](Readme.md#guides) section.

If you still have unanswered questions after reading this page or if you have encountered a problem, please visit our [FAQ & Troubleshooting](Troubleshooting.md) section.

The information presented on this page is valid for **all** platforms.

## GUI or CLI?

![GUI vs CLI](https://i.imgur.com/j9OTxRB.png)

**DCE** has two different versions:

- **Graphical User Interface** (**GUI**) - it's the preferred version for newcomers as it is easy to use.
  You can get it by [downloading](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) the `DiscordChatExporter.zip` file.
- **Command-line Interface** (**CLI**) - offers greater flexibility and more features for advanced users, such as export scheduling, ID lists, and more specific date ranges.
  You can get it by [downloading](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) the `DiscordChatExporter.CLI.zip` file.

If you're not comfortable with **Windows'** Command-line (cmd), please choose the GUI.
[**macOS**](MacOS.md), [**Linux**](Linux.md) and [**Docker**](Docker.md) users can only use the CLI version.

There are dedicated guides for each version:

- [Using the GUI](Using-the-GUI.md)
- [Using the CLI](Using-the-CLI.md)

## File formats

### HTML

![](https://i.imgur.com/S7lBTkV.png)
The HTML format replicates Discord's interface, making it the most user-friendly option.
It's the best format for attachment preview and sharing.
You can open `.html` files with a web browser, such as Google Chrome.

> **Warning**:
> If a picture is deleted, or if a user changes its avatar, the respective images will no longer be displayed.
> Export using the "Download referenced assets" (`--media`) option to avoid this.

### Plain Text

<img src="https://i.imgur.com/PbUyRXD.png" height="400"/>

The Plain Text format is the best option for archiving due to its small size.
You can open `.txt` files with a text editor, such as Notepad.

### JSON

<img src="https://i.imgur.com/FAeSA4O.png" height="400"/>

The JSON format contains more technical information and is easily parsable.
You can open `.json` files with a text editor, such as Notepad.

### CSV

![](https://i.imgur.com/VEVUsKs.png)
![](https://i.imgur.com/1vPmQqQ.png)

The CSV format allows for easy parsing of the chat log. Depending on your needs, the JSON format might be better.
You can open `.csv` files with a text editor, such as Notepad, or a spreadsheet app, like Microsoft Excel and Google Sheets.

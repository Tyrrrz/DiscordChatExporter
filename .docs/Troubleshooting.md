# Troubleshooting

Welcome to the Frequently Asked Questions (FAQ) and Troubleshooting page!
Here you'll find the answers to most of the questions related to **DiscordChatExporter** (DCE for short) and its core features.

- ❓ If you still have unanswered questions _after_ reading this page, feel free to [create a new discussion](https://github.com/Tyrrrz/DiscordChatExporter/discussions/new).
- 🐞 If you have encountered a problem that's not described here, has not [been discussed before](https://github.com/Tyrrrz/DiscordChatExporter/discussions), and is not a [known issue](https://github.com/Tyrrrz/DiscordChatExporter/issues?q=is%3Aissue), please [create a new discussion](https://github.com/Tyrrrz/DiscordChatExporter/discussions/new) or [open a bug report](https://github.com/Tyrrrz/DiscordChatExporter/issues/new). Don't forget to include your platform (Windows, Mac, Linux, etc.) and a detailed description of your question/problem.

## General questions

### Token stealer?

No. That's why this kind of software needs to be open-source, so the code can be audited by anyone.
Your token is only used to connect to Discord's API, it's not sent anywhere else.
If you're using the GUI, be aware that your token will be saved to a plain text file unless you disable it in the settings menu.

### Why should I be worried about the safety of my token?

A token can be used to log into your account, so treat it like a password and never share it.

### How can I reset my token?

Follow the [instructions here](Token-and-IDs.md).

### Will I get banned if I use this?

Automating user accounts is technically against [TOS](https://discord.com/terms), use at your discretion. [Bot accounts](https://discord.com/developers/docs/topics/oauth2#bots) don't have this restriction.

### Will the messages disappear from the exported file if I delete a message, delete my account or block a person?

Text messages will not be removed from the exported file, but if media, such as images and user avatars, is changed or deleted, it will no longer be displayed. To avoid this, export using the "Download media" (`--media`) option.

### Can DCE export messages that have already been deleted?

No, DCE cannot access them since they have been permanently deleted from Discord's servers.

### Can DCE export private chats?

Yes, if your account has access to them.

### Can DCE download images?

Yes, and other media too. Export using the "Download media" (`--media`) option.

### Can the exported chats be shared?

Yes.

### Can DCE export multiple formats at once?

No, you can only export one format at a time.

### Can DCE recreate the exported chats in Discord?

No, DCE is an exporter.

### Can DCE reupload exported messages to another channel?

No, DCE is an exporter.

### Can DCE add new messages to an existing export?

No.

## First steps

### How can I find my token?

Check the following page: [Obtaining token](Token-and-IDs.md)

### When I open DCE a black window pops up quickly or nothing shows up

If you have [.NET Core Runtime correctly installed](Dotnet.md), you might have downloaded the CLi flavor, try [downloading the GUI](Getting-started.md#gui-or-cli) instead.

### How do I run DCE on macOS or Linux?

Check the following pages:

- [macOS usage instructions](MacOS.md)
- [Linux usage instructions](Linux.md)

### How can I set DCE to export automatically at certain times?

Check the following pages to learn how to schedule **DiscordChatExporter.CLI** runs (advanced):

- [Windows scheduling](Scheduling-Windows.md)
- [macOS scheduling](Scheduling-MacOS.md)
- [Linux scheduling](Scheduling-Linux.md)

### The exported file is too large, I can't open it

Try opening it with a different program, try partitioning or use a different file format, like `PlainText`.

### DCE is crashing/failing

Check the following page: [Installing .NET Core Runtime](Dotnet.md)

If you already have .NET Core installed, please check if your problem is a [known issue](https://github.com/Tyrrrz/DiscordChatExporter/issues?q=is%3Aissue) before [opening a bug report](https://github.com/Tyrrrz/DiscordChatExporter/issues/new).

### .NET Core Runtime is required

Check the following page: [Installing .NET Core Runtime](Dotnet.md)

### I see messages in the export, but they have no content

Your bot is missing the 'Message Content Intent'. Go to the [Discord Developer Portal](https://discord.com/developers/applications), navigate to the 'Bot' section and enable it.

## CLI

### How do I use the CLI?

Check the following page:

- [Using the CLI](Using-the-CLI.md)

If you're using **Docker**, please refer to the [Docker Usage Instructions](Docker.md) instead.

### Where can I find the 'Channel IDs'?

Check the following page:

- [Obtaining Channel IDs](Token-and-IDs.md)

### I can't find Docker exported chats

Check the following page:

- [Docker usage instructions](Docker.md)

### I can't export Direct Messages

Make sure you're [copying the DM Channel ID](Token-and-IDs.md#how-to-get-a-direct-message-channel-id), not the person's user ID.

## Errors

```console
DiscordChatExporter.Domain.Exceptions.DiscordChatExporterException: Authentication token is invalid.
...
```

↳ Make sure the provided token is correct.

```console
DiscordChatExporter.Domain.Exceptions.DiscordChatExporterException: Requested resource does not exist.
```

↳ Check your channel ID, it might be invalid. [Read this if you need help](Token-and-IDs.md).

```console
DiscordChatExporter.Domain.Exceptions.DiscordChatExporterException: Access is forbidden.
```

↳ This means you don't have access to the channel.

```console
The application to execute does not exist:
```

↳ The `DiscordChatExporter.Cli.dll` file is missing. Keep the `.exe` and all the `.dll` files together. If you didn't move the files, try unzipping again.

```console
System.Net.WebException: Error: TrustFailure ... Invalid certificate received from server.
...
```

↳ Try running cert-sync.

Debian/Ubuntu: `cert-sync /etc/ssl/certs/ca-certificates.crt`

Red Hat: `cert-sync --user /etc/pki/tls/certs/ca-bundle.crt`

If it still doesn't work, try mozroots: `mozroots --import --ask-remove`

---

> ❓ If you still have unanswered questions, feel free to [create a new discussion](https://github.com/Tyrrrz/DiscordChatExporter/discussions/new).
>
> 🐞 If you have encountered a problem that's not described here, has not [been discussed before](https://github.com/Tyrrrz/DiscordChatExporter/discussions), and is not a [known issue](https://github.com/Tyrrrz/DiscordChatExporter/issues?q=is%3Aissue), please [create a new discussion](https://github.com/Tyrrrz/DiscordChatExporter/discussions/new) or [open a bug report](https://github.com/Tyrrrz/DiscordChatExporter/issues/new).

# Contributing

DiscordChatExporter accepts contributions in the form of issues and pull requests.

## Creating issues

If you have a feature suggestion or want to report a bug, you are welcome to create an issue.

Guidelines:

- Avoid creating an issue if a similar one already exists. Look through existing open and closed issues first.
- Keep your issue focused on one specific problem. If you have multiple suggestions or bug reports, please create separate issues for them.
- Provide a descriptive title for your issue. Don't use generic titles like "A couple suggestions" or "Not working".
- Provide more context in the body of the issue. If relevant, attach screenshots or screen recordings.
- Remain civil and respectful when participating in discussions.

## Creating pull requests

If you want to contribute code to the project, you can do so by creating a pull request.

Guidelines:

- Make sure that there is an existing issue that describes the problem solved by your pull request. If there isn't one, please create it first.
- Add a comment to the issue indicating that you want to submit a pull request for it. This can be a good opportunity to discuss possible solutions, finalize requirements, or ask questions.
- Keep your pull request focused and as small as possible. If you want to contribute multiple unrelated changes, please create separate pull requests for them.
- Follow the coding style and conventions already established by the project. When in doubt which style to use, ask in comments to your pull request.
- If you want to start a discussion regarding a specific change you made, add a review comment to your own code. This can be used to highlight something important or to seek input from others.

## Building the project locally

Prerequisites:

- [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- [.NET 3.1 SDK](https://dotnet.microsoft.com/download/dotnet/3.1) (temporarily as the app still targets .NET 3.1, but requires .NET 5.0 to build)
- _(Optional)_ C#/.NET IDE, such as [JetBrains Rider](https://www.jetbrains.com/rider), [VS Code](https://code.visualstudio.com/docs/languages/csharp), or [Visual Studio](https://visualstudio.microsoft.com).

To build the entire solution run the following command in the root of the repository:

```sh
> dotnet build
```

This will generate runtime artifacts for each project:

```plaintext
./DiscordChatExporter.Gui/bin/[Debug|Release]/[runtime]/*
./DiscordChatExporter.Cli/bin/[Debug|Release]/[runtime]/*
```

You can also build and run a specific project directly.
To do that, navigate to its directory and use `dotnet run`:

```sh
> cd DiscordChatExporter.Gui
> dotnet run
```

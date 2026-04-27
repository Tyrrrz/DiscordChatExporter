# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build, format, test

Targets .NET 10 (`global.json` pins SDK 10.0.100, `Directory.Build.props` sets `TargetFramework=net10.0`, `LangVersion=preview`, `Nullable=enable`, `TreatWarningsAsErrors=true`). Central package management via `Directory.Packages.props` — never put a `Version=` on a `PackageReference`, add it to that file. The default branch is `prime`, not `main`.

- `dotnet build` — builds and runs CSharpier formatting as part of the build (CSharpier.MsBuild is wired into every project). Pass `-p:CSharpier_Bypass=true` to skip formatting during iterative work; CI uses this on the build step and verifies formatting separately with `dotnet build -t:CSharpierFormat --configuration Release --no-restore`.
- `dotnet test` — runs the xUnit suite. The suite hits a **real Discord server** (no mocks); set `DISCORD_TOKEN` either as an env var or a user secret on the test project (`dotnet user-secrets set DISCORD_TOKEN <token> --project DiscordChatExporter.Cli.Tests`). Tests without a token will throw at first access. Test invitation lives in `DiscordChatExporter.Cli.Tests/Readme.md`.
- Run a single test with `dotnet test --filter "FullyQualifiedName~HtmlContentSpecs.I_can_export_a_channel_in_the_HTML_format"`.
- Publish: `dotnet publish DiscordChatExporter.Cli -r <rid> --self-contained -p:CSharpier_Bypass=true`. Both apps publish trimmed (`PublishTrimmed=true`); trim/AOT analyzers are intentionally disabled in the csproj files because C# 14 extension blocks confuse them — keep `[DynamicDependency]` attributes on `Program.Main` (CLI) for any new CliFx command/converter type or it will fail at runtime in trimmed builds.

## Architecture

Three projects, one-way dependency graph: `Core` ← `Cli` ← `Cli.Tests`, and `Core` ← `Gui`.

**`DiscordChatExporter.Core`** — the export engine. `Discord/DiscordClient.cs` is the only thing that talks to `discord.com/api/v10`; it autodetects user vs bot tokens, has its own rate-limit handling driven by `RateLimitPreference`, and wraps requests in a Polly resilience pipeline. `Exporting/ChannelExporter.cs` orchestrates a single channel export: it builds an `ExportContext` (populates channels/roles/members on demand), streams messages via `DiscordClient.GetMessagesAsync`, applies `MessageFilter`, and hands each message to `MessageExporter`. `MessageExporter` owns partitioning — it rotates the underlying `MessageWriter` (`HtmlMessageWriter`, `JsonMessageWriter`, `CsvMessageWriter`, `PlainTextMessageWriter`) when a `PartitionLimit` (count or byte size) is reached. HTML output is rendered by **RazorBlade** templates (`PreambleTemplate.cshtml`, `MessageGroupTemplate.cshtml`, `PostambleTemplate.cshtml`) — these are compile-time-generated, no runtime Razor. Markdown rendering is a hand-rolled parser/visitor under `Markdown/` (`MarkdownParser` produces a node tree; `HtmlMarkdownVisitor` and `PlainTextMarkdownVisitor` emit per-format output). Filter syntax (`from:`, `has:`, `mentions:`, etc.) is parsed under `Exporting/Filtering/Parsing/` using **Superpower**.

**`DiscordChatExporter.Cli`** — CliFx-based commands (`ExportChannelsCommand`, `ExportGuildCommand`, `ExportAllCommand`, list commands, etc.). Each command instantiates a `DiscordClient` and `ChannelExporter` directly; there is no DI container in the CLI. Templated output paths use the `%G %T %C %a %b %d` tokens documented in `.docs/Using-the-CLI.md`.

**`DiscordChatExporter.Gui`** — Avalonia 11 desktop app with CommunityToolkit.Mvvm. Compiled XAML bindings (`AvaloniaUseCompiledBindingsByDefault=true`); MVVM lives under `ViewModels/` with `Components/` (long-lived) and `Dialogs/` (modal). `Framework/` provides the dialog/view-model managers. `Services/SettingsService.cs` persists user settings via Cogwheel; **the user's Discord token is encrypted at rest** using the build-time `EncryptionSalt` MSBuild property (overridable via `-p:EncryptionSalt=...` at publish time, surfaced into code via `ThisAssembly.Project`). The MacOS app bundle is built post-publish by `Publish-MacOSBundle.ps1` when `-p:PublishMacOSBundle=true`. Self-update is handled by Onova.

**`DiscordChatExporter.Cli.Tests`** — xUnit + FluentAssertions. Almost all specs go through `Infra/ExportWrapper.cs`, which executes a real `ExportChannelsCommand` against fixed channel IDs in `Infra/ChannelIds.cs` and **caches exports** under `bin/.../ExportCache/` keyed by `<channelId>.<ext>` so the suite is fast on re-run. Specs read the resulting HTML with AngleSharp or JSON with `System.Text.Json` and assert on the document structure. Adding a new spec usually means: (1) add a constant to `ChannelIds.cs` referencing a channel on the test server, (2) write the channel content there (requires server permissions — coordinate with the maintainer), (3) call `ExportWrapper.GetMessagesAsHtmlAsync` / `GetMessagesAsJsonAsync` and assert.

## Conventions worth knowing

- `Snowflake` (in `Core/Discord/`) is the strongly-typed Discord ID — use it instead of raw `ulong`/`string` for IDs.
- All public Core APIs return `ValueTask` and accept `CancellationToken`; preserve this when adding methods.
- `DiscordChatExporterException` carries an `IsFatal` flag — wrap downstream exceptions with `IsFatal=false` for per-message errors that shouldn't abort the whole export, `true` for unrecoverable ones.
- Docker image is built from `DiscordChatExporter.Cli.dockerfile` at the repo root and uses the alpine `runtime-deps` base with ICU + tzdata installed (locale support is a documented feature).

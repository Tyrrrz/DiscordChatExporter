# Discord MCP Server & Archiving System — Design Document

## 1. Overview & Goals

### What We're Building

Two complementary systems on top of the existing `DiscordChatExporter.Core` library:

1. **Discord MCP Server** (`DiscordChatExporter.Mcp`) — a Model Context Protocol server that lets Claude interact with Discord channels in real time: listing guilds/channels/threads, reading messages, and triggering exports. Read-only to start; write mode added later when a bot token is available.

2. **Archiving System** — an on-demand script/skill that incrementally exports Discord channels to structured local JSON files, tracking what has already been exported so only new content is pulled each run. Thread-aware from day one.

### Goals

- Claude can query live Discord data (messages, channels, threads) without leaving a session
- Local JSON archives stay up to date incrementally — no full re-exports
- Threads are never silently skipped
- Exports are organized by project context so they land where Claude is working
- An HTML viewer allows jumping directly to a specific message from a Claude result
- The system stays flexible — no hard assumptions about which servers or channels matter
- Adding capabilities later (write mode, media download, bot token) requires no architectural rework

### Non-Goals (for now)

- Posting messages or reacting (requires bot token — see Section 9)
- Downloading and processing media/images (Phase 2 — see Section 7)
- Multi-user or hosted deployment
- Replacing the existing DCE GUI or CLI

## 2. System Architecture

### Components

```
┌─────────────────────────────────────────────────────────┐
│                     Claude Code                         │
│   (queries tools, reads archives, opens viewer links)   │
└──────────────┬──────────────────────┬───────────────────┘
               │ MCP stdio            │ reads files
               ▼                      ▼
┌──────────────────────┐   ┌─────────────────────────────┐
│  DiscordChatExporter │   │     Local JSON Archives      │
│       .Mcp           │   │  discord-exports/            │
│  (MCP server)        │   │    <guild>/<channel>/        │
└──────────┬───────────┘   └──────────┬──────────────────┘
           │ uses Core                │ written by
           │                          │
           ▼                          ▼
┌──────────────────────┐   ┌─────────────────────────────┐
│  DiscordChatExporter │   │      Archiving System        │
│       .Core          │◄──│  (script / Claude skill)     │
│  (existing library)  │   │  reads manifest, calls Core  │
└──────────┬───────────┘   └─────────────────────────────┘
           │
           ▼
┌──────────────────────┐   ┌─────────────────────────────┐
│    Discord API v10   │   │       HTML Viewer            │
│  discord.com/api/v10 │   │  local HTTP server           │
│  (rate-limited,      │   │  deep-links to messages      │
│   user token)        │   │  (Section 6)                 │
└──────────────────────┘   └─────────────────────────────┘
```

### Data Flow

**Live query:** Claude calls a tool → MCP server → `DiscordClient` (Core) → Discord API → structured response back to Claude.

**Export/archive:** Claude calls `export_channel` (or archiving script runs directly) → `ChannelExporter` (Core) → JSON + optional HTML files written to `discord-exports/` → manifest updated.

**Search:** Claude calls `search_messages` → MCP server reads local JSON archives → returns matching messages with viewer deep-link URLs.

**Viewer:** Claude receives a viewer URL (e.g., `http://localhost:5722/view#msg-866674314627121232`) → user opens it → HTML viewer renders the export at that message position.

### What Lives Where

| Component | Location | Status |
|---|---|---|
| Discord API client, data models, export engine | `DiscordChatExporter.Core` | Existing |
| MCP server | `DiscordChatExporter.Mcp/` | New |
| Archiving script | `scripts/archive.ps1` or Claude skill | New |
| HTML viewer | `DiscordChatExporter.Viewer/` | New |
| Export output | `discord-exports/` (gitignored) | Generated |
| Allowlist config | `mcp-settings.json` (gitignored) | New |
| Archive manifest | `discord-exports/.manifest.json` | Generated |

## 3. MCP Primer

### What MCP Is

The **Model Context Protocol** is an open standard that lets AI assistants (like Claude) communicate with external tools and data sources via a standardized interface. An MCP server exposes **tools** — functions Claude can call during a conversation to retrieve data or trigger actions. Claude decides when to call them based on context; the user doesn't have to explicitly ask.

### How It Works with Claude Code

Claude Code acts as an **MCP client**. It reads the server list from `.claude/settings.json`, spawns each registered server as a subprocess, and communicates via **stdio** (stdin/stdout). The protocol is JSON-RPC 2.0.

When Claude needs to look something up, it:
1. Selects the appropriate tool by name
2. Sends a `tools/call` request with parameters
3. Receives a structured result
4. Incorporates the result into its response

The user sees the tool calls in the session transcript (they can be approved or denied in restricted permission modes).

### Tools vs Resources vs Prompts

MCP supports three primitives. This project only uses **tools**:

- **Tools** — callable functions with typed parameters and return values. Used for everything here: listing channels, fetching messages, triggering exports.
- Resources — static or dynamic data Claude can subscribe to (not used here).
- Prompts — pre-written prompt templates (not used here).

### C# Implementation Pattern

Using the `ModelContextProtocol` NuGet package, tools are async methods decorated with `[McpServerTool]`:

```csharp
[McpServerTool(Name = "get_messages", Description = "Fetch messages from a Discord channel")]
public async Task<string> GetMessagesAsync(string channelId, int limit = 50)
{
    // validate allowlist, call DiscordClient, serialize, return
}
```

The SDK handles JSON-RPC serialization, tool discovery, and the stdio message loop. The server entry point just builds the host and runs it.

### Registration in Claude Code

Add to `.claude/settings.json` at the repo root:

```json
{
  "mcpServers": {
    "discord": {
      "command": "dotnet",
      "args": ["run", "--project", "DiscordChatExporter.Mcp", "--no-build"],
      "env": {
        "DISCORD_TOKEN": "${DISCORD_TOKEN}"
      }
    }
  }
}
```

`DISCORD_TOKEN` must be set in the shell environment before starting Claude Code. It is never stored in the settings file itself.

## 4. MCP Server Design

### Project Structure

New project `DiscordChatExporter.Mcp/` inside this repo. References `DiscordChatExporter.Core` directly — no new HTTP or export logic is written here; that all stays in Core. Uses the `ModelContextProtocol` NuGet package (Microsoft's official C# MCP SDK).

### Transport & Registration

Claude Code communicates with MCP servers via **stdio** — it spawns the server as a subprocess and exchanges JSON-RPC messages over stdin/stdout. The server must never write anything to stdout except MCP protocol messages; all debug/diagnostic output goes to stderr.

The server is registered once in `.claude/settings.json` at the repo root:

```json
{
  "mcpServers": {
    "discord": {
      "command": "dotnet",
      "args": ["run", "--project", "DiscordChatExporter.Mcp", "--no-build"],
      "env": {
        "DISCORD_TOKEN": "${DISCORD_TOKEN}"
      }
    }
  }
}
```

For production use (after first publish), point to the compiled binary instead of `dotnet run`.

### Configuration

| Source | Key | Purpose |
|---|---|---|
| Environment variable | `DISCORD_TOKEN` | Discord auth token (required) |
| Environment variable | `DISCORD_EXPORT_PATH` | Override default export output path |
| `mcp-settings.json` | `allowedGuilds` | Guild IDs the server will respond to |
| `mcp-settings.json` | `allowedChannels` | Specific channel IDs (within allowed guilds) |
| `mcp-settings.json` | `allowAllChannelsInGuilds` | If true, all channels in an allowed guild are accessible |

`mcp-settings.json` lives at the repo root and is gitignored. Example structure:

```json
{
  "allowedGuilds": ["123456789012345678"],
  "allowedChannels": ["987654321098765432"],
  "allowAllChannelsInGuilds": false
}
```

Any tool call referencing a guild or channel not in the allowlist returns a descriptive error to Claude: `"Channel {id} is not in the configured allowlist."` Full stack traces are written to stderr only.

### Output Routing

Exports triggered via `export_channel` land at:
```
<project-root>/discord-exports/<guild-name>/<channel-name>/
```

`<project-root>` is the working directory when Claude Code starts the server. Override with `DISCORD_EXPORT_PATH` env var or `exportPath` in `mcp-settings.json`.

### Startup Sequence

1. Read `DISCORD_TOKEN` from environment — fail fast with `"DISCORD_TOKEN environment variable is not set."` if missing
2. Load `mcp-settings.json` — fail fast if not found, with path hint
3. Instantiate `DiscordClient` from Core (handles rate limiting, token type detection, retries automatically)
4. Register all MCP tools
5. Write `"Discord MCP server ready."` to stderr
6. Begin stdio message loop

### Error Strategy

- **Allowlist violations** → natural language error in MCP response (Claude reads this)
- **Auth failures** → natural language error in MCP response
- **Discord API errors** → natural language summary in MCP response (e.g., `"Discord returned 403 for channel {id} — your token may lack access."`)
- **All stack traces** → stderr only, never in MCP tool responses

### Message Format

`get_messages` returns a JSON array. Each message object:

```json
{
  "id": "866674314627121232",
  "timestamp": "2021-07-22T14:30:00Z",
  "author": {
    "id": "123456789",
    "name": "Username",
    "nickname": "Nick",
    "roles": ["Admin", "Member"]
  },
  "content": "message text here",
  "attachments": [{ "url": "...", "filename": "image.png", "size": 12345 }],
  "embeds": [{ "title": "...", "description": "...", "url": "..." }],
  "reactions": [{ "emoji": "👍", "count": 3 }],
  "reply_to": "866710679758045195",
  "thread_id": null
}
```

This is a simplified projection of the Core data model — not the full DCE export format.

Default page size: **50 messages**. Pagination uses `before`/`after` Snowflake IDs (matching Discord's own model). Claude is responsible for paginating across multiple calls when needed.

### Phase 1 Tools (Read-only)

| Tool | Parameters | Returns |
|---|---|---|
| `list_guilds` | — | Allowed guilds: id, name, member count |
| `list_channels` | `guild_id` | Channels (filtered by allowlist): id, name, type, topic, category |
| `list_threads` | `channel_id` | Active + recent archived threads: id, name, message count, last activity |
| `get_messages` | `channel_id`, `before?`, `after?`, `limit=50` | Paginated message array |
| `get_channel_info` | `channel_id` | Name, topic, category, type, creation date, last message timestamp |
| `search_messages` | `query`, `channel_id?`, `guild_id?` | Matching messages from local JSON archives (full-text) |
| `export_channel` | `channel_id`, `format="json"` | Triggers incremental export via Core's `ChannelExporter`; returns output file path |

### Phase 2 Tools

| Tool | Parameters | Returns |
|---|---|---|
| `get_pinned_messages` | `channel_id` | Pinned messages in full message format |
| `get_reactions` | `channel_id`, `message_id` | Per-emoji breakdown with user list |
| `list_forum_posts` | `channel_id` | Forum posts (threads with tags) in a forum channel |
| `get_member_info` | `guild_id`, `user_id` | Roles, nickname, join date |

### Future Tools (Not Yet Designed)

Custom emoji index, channel topic history, voice state, server audit log, webhooks. Add as write mode and bot token become available.

## 5. Archiving System

### Purpose

Maintains up-to-date local copies of Discord channels as JSON files. Designed to be run on-demand (manually or via Claude skill) rather than as a background daemon. Each run only pulls content newer than the last successful export for each channel — no full re-exports.

### Manifest

A single file `discord-exports/.manifest.json` tracks state per channel:

```json
{
  "channels": {
    "987654321098765432": {
      "guildId": "123456789012345678",
      "guildName": "My Server",
      "channelName": "general",
      "lastExportedAt": "2024-11-15T22:45:00Z",
      "lastMessageId": "1234567890123456789",
      "exportPath": "discord-exports/My Server/general/",
      "includesThreads": true
    }
  }
}
```

On each run: read manifest → determine `--after` cutoff per channel → export → update manifest. If a channel has no manifest entry, do a full export from the beginning.

### Incremental Strategy

Uses DCE Core's `--after <lastMessageId>` (Snowflake-based, not timestamp-based) so no messages fall through clock-skew gaps. New messages are appended to the existing JSON file rather than producing a new dated snapshot. For very large channels (or very long gaps), DCE's existing partitioning handles file size automatically.

### Thread Handling

Threads are always exported. The archiving system:
1. Exports the parent channel
2. Enumerates all active + archived threads via `DiscordClient.GetThreadsAsync`
3. Exports each thread individually (threads are first-class channels in Discord's model)
4. Tracks each thread separately in the manifest under its own channel ID

This is the explicit fix for the original problem — no thread is ever silently skipped.

### Output Format

- **Primary:** JSON (one file per channel, named `<channel-id>.json`)
- **Optional:** HTML alongside JSON when `--html` flag is passed (human-readable, viewer-compatible)
- Folder structure: `discord-exports/<guild-name>/<channel-name>/`

### Existing TXT Export Migration

A one-time migration tool converts existing `.txt` exports to the JSON manifest format on a best-effort basis:
- Parses message content and timestamps from TXT format
- Creates manifest entries with a `migratedFromTxt: true` flag and no `lastMessageId`
- Next incremental run will do a full re-export (since no reliable cursor exists from TXT)
- Missing media is acknowledged — TXT exports don't contain attachment URLs

### Running the Archiver

```powershell
# Archive all channels in the allowlist
dotnet run --project DiscordChatExporter.Mcp -- archive

# Archive a specific channel
dotnet run --project DiscordChatExporter.Mcp -- archive --channel 987654321098765432

# Archive with HTML output alongside JSON
dotnet run --project DiscordChatExporter.Mcp -- archive --html
```

*Judgment call: the archiving command is implemented as a subcommand of the MCP project binary rather than a separate script. This keeps the allowlist config and Core dependency in one place. A standalone PowerShell script is an alternative if preferred.*

## 6. HTML Viewer

### Purpose

Provides a local browser-based viewer for exported HTML files with **deep-link support** — Claude can return a URL that opens the export at a specific message, similar to clicking a search result in the Discord app itself.

### How It Works

A minimal local HTTP server (`DiscordChatExporter.Viewer`) serves the HTML export files and injects a small JavaScript snippet that handles the deep-link hash on page load:

1. Claude returns a result like: `http://localhost:5722/view?channel=987654321098765432#msg-866674314627121232`
2. User opens the URL in their browser
3. Viewer serves the DCE-generated HTML for that channel
4. The injected script scrolls to the element with `data-message-id="866674314627121232"` and highlights it

No modifications to the existing DCE HTML export templates are required — the `data-message-id` attributes are already there (they're used in the test suite today).

### Deep-Link Format

```
http://localhost:5722/view?channel=<channel-id>#msg-<message-id>
```

The MCP server appends a `viewer_url` field to every message in `get_messages` and `search_messages` responses:

```json
{
  "id": "866674314627121232",
  "content": "...",
  "viewer_url": "http://localhost:5722/view?channel=987654321098765432#msg-866674314627121232"
}
```

If no viewer is running, the field is still present but the URL simply won't open. No errors.

### Viewer Startup

The viewer runs as a separate lightweight process, started on demand:

```powershell
dotnet run --project DiscordChatExporter.Viewer
```

*Judgment call: the viewer is a separate project rather than part of the MCP server to keep concerns separated and allow starting/stopping independently. If complexity is a concern, it can be folded into the MCP server as an optional HTTP endpoint on the same port.*

### Viewer Port

Default port `5722`. Configurable via `DISCORD_VIEWER_PORT` env var or `mcp-settings.json`. MCP server reads this setting when generating `viewer_url` values so the links are always correct.

## 7. Media Handling

### Phase 1 — URLs Only (Current)

All message responses include attachment and embed URLs. Claude can reference them but cannot see their content. Images, GIFs, and files are represented as:

```json
"attachments": [{ "url": "https://cdn.discordapp.com/...", "filename": "screenshot.png", "size": 84231 }]
```

No downloads occur. No local storage. This is the default until Phase 2 is explicitly enabled.

### Phase 2 — Download and Vision Processing

When enabled (opt-in flag on export or MCP config), the archiver downloads referenced media alongside exports:

**Storage location:**
```
discord-exports/<guild-name>/<channel-name>/media/<filename-or-hash>
```

**Deduplication:** Files are named by a hash of their CDN URL to avoid duplicate downloads across channels.

**What gets downloaded:** Only files referenced by exported messages — avatars, attachments, linked images. Not all historical Discord CDN content.

**Claude vision integration:** When the MCP server's `get_messages` detects a locally cached media file, it includes the local path alongside the URL. A separate MCP tool `read_media_file` returns the file content as base64 for Claude's vision capability.

**Discord CDN caveat:** Discord CDN URLs for attachments expire. Downloaded files must be saved locally before expiry — the archiver should download eagerly during export, not lazily on demand.

### Phase 2 Enablement

```json
// mcp-settings.json
{
  "mediaDownload": {
    "enabled": true,
    "downloadAvatars": false,
    "maxFileSizeMb": 25
  }
}
```

## 8. Implementation Phases

Each phase is independently useful — don't need later phases to get value from earlier ones.

### Phase 1 — MCP Server (Read-only Core)

*Goal: Claude can query live Discord data from within any Claude Code session.*

- `DiscordChatExporter.Mcp` project scaffolded, references Core
- `ModelContextProtocol` NuGet added
- `mcp-settings.json` allowlist config
- `.claude/settings.json` registration
- Tools: `list_guilds`, `list_channels`, `list_threads`, `get_messages`, `get_channel_info`, `export_channel`
- Startup validation (token, allowlist)
- Error handling strategy (natural language to Claude, stack traces to stderr)
- `discord-exports/` and `mcp-settings.json` added to `.gitignore`

### Phase 2 — Archiving System

*Goal: Local JSON archives stay incrementally up to date; threads never missed.*

- Manifest file design and read/write logic
- Incremental export via `--after <lastMessageId>`
- Thread enumeration + per-thread manifest entries
- `archive` subcommand on the MCP binary
- `search_messages` tool wired to local JSON archives (full-text)
- TXT migration utility (best-effort)
- Optional HTML output alongside JSON

### Phase 3 — HTML Viewer

*Goal: Claude results link directly to the message in context.*

- `DiscordChatExporter.Viewer` project (minimal ASP.NET)
- Serves HTML exports from `discord-exports/`
- Deep-link JavaScript injected at serve time
- MCP server appends `viewer_url` to message responses
- Port configuration

### Phase 4 — Phase 2 MCP Tools

*Goal: Richer context for Claude — pinned content, reactions, member roles, forums.*

- `get_pinned_messages`
- `get_reactions`
- `list_forum_posts`
- `get_member_info`

### Phase 5 — Media Download & Vision

*Goal: Claude can see images and attachments, not just know they exist.*

- Download-on-archive with CDN URL expiry awareness
- Local media storage with deduplication
- `read_media_file` MCP tool
- `viewer_url` extended to media files
- `mcp-settings.json` media config block

### Phase 6 — Write Mode (Bot Token)

*Goal: Claude can post messages, upload files, create threads.*

- See Section 9 for prerequisites
- New tools: `send_message`, `upload_file`, `create_thread`, `add_reaction`
- Rate limit model changes with bot token

## 9. Bot Token Roadmap

### Why a Bot Token

The current user token gives read access but posting messages, uploading files, and creating threads via a user token violates Discord's Terms of Service and risks account suspension. A bot token is the correct mechanism for any write operations.

### Getting a Bot Token

1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Create a New Application
3. Go to **Bot** → **Add Bot**
4. Copy the token — treat it like a password, never commit it
5. Under **OAuth2 → URL Generator**: select `bot` scope + the permissions you need (Send Messages, Attach Files, Create Threads)
6. Use the generated URL to invite the bot to your servers
7. Set `DISCORD_BOT_TOKEN` as a separate environment variable alongside `DISCORD_TOKEN`

### What Changes Architecturally

The `DiscordClient` already supports both user and bot tokens (it detects the type automatically). For write operations, a second `DiscordClient` instance initialized with the bot token will be used — keeping the read client and write client explicitly separate so it's always clear which is in use.

The allowlist config gains a `writeAllowedChannels` list — write operations are gated separately from read operations for safety.

### New Tools in Phase 6

| Tool | Description |
|---|---|
| `send_message` | Post a message to an allowed channel |
| `upload_file` | Attach a local file to a message |
| `create_thread` | Start a thread on an existing message |
| `add_reaction` | Add an emoji reaction to a message |

Claude will require explicit user confirmation before any write operation — this is handled at the MCP permission level in `.claude/settings.json` (tools can be marked as requiring approval).

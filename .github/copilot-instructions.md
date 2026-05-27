# Copilot Instructions for DiscordChatExporter

## Build, Test, and Lint Commands

### Build
```bash
# Full build
dotnet build --configuration Release

# Quick build without formatting checks
dotnet build -p:CSharpier_Bypass=true
```

### Test
```bash
# Run all tests
dotnet test --configuration Release

# Run a specific test file
dotnet test --configuration Release --filter "ClassName=HtmlContentSpecs"

# Run tests with code coverage
dotnet test -p:CSharpier_Bypass=true --configuration Release --collect:"XPlat Code Coverage"
```

### Format and Lint
```bash
# Format code with CSharpier (integrated into CI)
dotnet build -t:CSharpierFormat --configuration Release

# Just verify formatting without applying fixes
dotnet build -p:CSharpier_Bypass=true --configuration Release
```

> **Note:** CSharpier formatting is enforced in CI. Use `dotnet build -t:CSharpierFormat` before committing to avoid CI failures.

## High-Level Architecture

DiscordChatExporter is a .NET 10.0 application with a layered architecture:

### Layer 1: Core (`DiscordChatExporter.Core`)
- **Discord** - Discord API client and data models
  - `DiscordClient` - HTTP client for Discord API v10
  - Data models in `Discord/Data/` (records like `Channel`, `Message`, `Guild`) with `Parse()` methods for JSON deserialization
  - Rate-limit handling with configurable preference
- **Exporting** - Multi-format export engines
  - `ChannelExporter` - Orchestrates the export process
  - Format writers: `HtmlMessageWriter`, `JsonMessageWriter`, `CsvMessageWriter`, `PlainTextMessageWriter`
  - Asset downloading and context building
- **Markdown** - Converts Discord markdown to target format (HTML or plaintext)
- **Utils** - Shared utilities for HTTP, validation, etc.

### Layer 2: Interfaces
- **Cli** (`DiscordChatExporter.Cli`) - Command-line interface using CliFx
  - Commands in `Commands/` subdirectory (follows command pattern)
- **Gui** (`DiscordChatExporter.Gui`) - Graphical interface using Avalonia
  - ViewModels with MVVM pattern
  - Services for state management
  - Localization support

### Layer 3: Tests
- **Cli.Tests** (`DiscordChatExporter.Cli.Tests`) - Integration tests using xUnit
  - `Specs/` - Scenario tests for export formats and features
  - `Infra/` - Test infrastructure and helpers
  - Tests verify HTML/JSON/CSV/TXT exports against Discord test data

### Data Flow
```
Discord API → DiscordClient (rate-limited)
           → ExportContext (loads channel/role/user data)
           → MessageExporter (fetches and writes messages)
           → Format-specific Writer (HTML/JSON/CSV/TXT)
           → File output
```

## Key Conventions

### C# Language Features
- **File-scoped namespaces** - Use `namespace X;` (not braces)
- **Primary constructors** - `public class MyClass(string param)` for injecting dependencies
- **Nullable reference types** - Enabled globally; use `?` for nullable types, `!` only when safe
- **Treat warnings as errors** - All warnings must be resolved before commit

### Data Model Patterns
- Use `record` types for data classes (immutable by default)
- Implement `IHasId` interface for entities with ID fields
- Deserialization via `public static T Parse(JsonElement json)` method
- Partial records with separate `Parse` methods in distinct file sections
- Use `Pipe()` extension for method chaining transformations

```csharp
// Example pattern:
public partial record Message(Snowflake Id, string Content) : IHasId { }

public partial record Message
{
    public static Message Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var content = json.GetProperty("content").GetNonWhiteSpaceString();
        return new(id, content);
    }
}
```

### Exception Handling
- Custom exceptions inherit from `DiscordChatExporterException`
- Specific exception types for domain errors: `ChannelEmptyException`, `InvalidStateException`, etc.
- Exceptions include helpful context about the guild/channel where applicable

### Discord API Integration
- All API URLs are relative to base URI `https://discord.com/api/v10/`
- Token authorization uses `Authorization` header (either `Bot {token}` or raw token)
- Rate limiting respects Discord advisory headers but can be configured to respect only hard limits
- Use `Http.ResponseResiliencePipeline` for retry logic (configured via Polly)

### Export Format Implementation
- Each format has a dedicated `*MessageWriter` class
- Writers implement `MessageWriter` interface
- Template files (`.cshtml`) use RazorBlade for HTML/plaintext rendering
- Markdown conversion uses separate visitors: `HtmlMarkdownVisitor`, `PlainTextMarkdownVisitor`

### Testing
- Tests in `DiscordChatExporter.Cli.Tests/Specs/` follow naming pattern: `[Format][Feature]Specs.cs`
- Use xUnit `[Fact]` for individual tests
- Test infrastructure in `Infra/` includes `ExportWrapper` for export orchestration
- Tests require Discord API access; sensitive tests need `DISCORD_TOKEN` secret
- Use FluentAssertions for readable assertions: `.Should().Equal(...)`, `.Should().Contain(...)`

### Dependencies and Injection
- Microsoft.Extensions.DependencyInjection for IoC
- Services typically injected via primary constructor
- Configuration loaded via Microsoft.Extensions.Configuration (supports env vars and user secrets)

### Code Organization
- Folder structure mirrors namespace structure
- Data models organized under domain folder (e.g., `Discord/Data/`)
- Keep public methods at the top of the class
- Use `async ValueTask` for small async operations, `async Task` for larger ones

## Architecture Details

### Why This Structure?
- **Separation of concerns**: Core library independent from UI implementations
- **Multi-UI support**: CLI and GUI share identical core export logic
- **Testability**: Core is fully testable without UI dependencies
- **Extensibility**: New export formats are isolated to a single writer class

### Important Flow Details
- Message export is **stream-based** to handle large channels efficiently
- Discord API client implements **exponential backoff** for rate limits
- Exports can be **partitioned** by size or date range to manage large channel history
- Assets (images, videos, etc.) can be **selectively downloaded** during export

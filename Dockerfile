# Build
FROM microsoft/dotnet:3.0-sdk AS build
WORKDIR /src

COPY favicon.ico ./

COPY DiscordChatExporter.Core.Markdown DiscordChatExporter.Core.Markdown
COPY DiscordChatExporter.Core.Models DiscordChatExporter.Core.Models
COPY DiscordChatExporter.Core.Rendering DiscordChatExporter.Core.Rendering
COPY DiscordChatExporter.Core.Services DiscordChatExporter.Core.Services
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli -o DiscordChatExporter.Cli/publish -c Release

# Run
FROM microsoft/dotnet:3.0-runtime AS run
WORKDIR /app

COPY --from=build /src/DiscordChatExporter.Cli/publish ./

WORKDIR /app/out
ENTRYPOINT ["dotnet", "/app/DiscordChatExporter.Cli.dll"]
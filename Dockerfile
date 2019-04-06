# Build
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

COPY favicon.ico ./

COPY DiscordChatExporter.Core.Markdown DiscordChatExporter.Core.Markdown
COPY DiscordChatExporter.Core.Models DiscordChatExporter.Core.Models
COPY DiscordChatExporter.Core.Rendering DiscordChatExporter.Core.Rendering
COPY DiscordChatExporter.Core.Services DiscordChatExporter.Core.Services
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli -c Release -f netcoreapp2.1

# Run
FROM microsoft/dotnet:2.1-runtime AS run
WORKDIR /app

COPY --from=build /src/DiscordChatExporter.Cli/bin/Release/netcoreapp2.1/publish ./

WORKDIR /app/out
ENTRYPOINT ["dotnet", "/app/DiscordChatExporter.Cli.dll"]
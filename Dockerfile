# Build
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY favicon.ico ./
COPY Directory.Build.props ./
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli -o DiscordChatExporter.Cli/publish -c Release

# Run
FROM mcr.microsoft.com/dotnet/runtime:3.1 AS run
WORKDIR /app

COPY --from=build /src/DiscordChatExporter.Cli/publish ./

WORKDIR /app/out
ENTRYPOINT ["dotnet", "/app/DiscordChatExporter.Cli.dll"]

# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

COPY favicon.ico ./
COPY NuGet.config ./
COPY Directory.Build.props ./
COPY DiscordChatExporter.Core ./DiscordChatExporter.Core
COPY DiscordChatExporter.Cli ./DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli -c Release -o ./publish

# Run
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS run

COPY --from=build ./publish ./

WORKDIR ./out
ENTRYPOINT ["dotnet", "/DiscordChatExporter.Cli.dll"]
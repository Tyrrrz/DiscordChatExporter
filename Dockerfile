# Build
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

COPY favicon.ico ./

COPY DiscordChatExporter.Core.Markdown/*.csproj DiscordChatExporter.Core.Markdown/
RUN dotnet restore DiscordChatExporter.Core.Markdown

COPY DiscordChatExporter.Core/*.csproj DiscordChatExporter.Core/
RUN dotnet restore DiscordChatExporter.Core

COPY DiscordChatExporter.Cli/*.csproj DiscordChatExporter.Cli/
RUN dotnet restore DiscordChatExporter.Cli

COPY DiscordChatExporter.Core.Markdown DiscordChatExporter.Core.Markdown
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli -c Release -f netcoreapp2.1

# Run
FROM microsoft/dotnet:2.1-runtime AS run
WORKDIR /app

COPY --from=build /src/DiscordChatExporter.Cli/bin/Release/netcoreapp2.1/publish ./

WORKDIR /app/out
ENTRYPOINT ["dotnet", "/app/DiscordChatExporter.Cli.dll"]
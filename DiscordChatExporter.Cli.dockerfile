# Build
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

WORKDIR /build

COPY favicon.ico ./
COPY NuGet.config ./
COPY Directory.Build.props ./
COPY DiscordChatExporter.Core ./DiscordChatExporter.Core
COPY DiscordChatExporter.Cli ./DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli \
    --self-contained \
    --use-current-runtime \
    --configuration Release \
    --output ./publish

# Run
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine

RUN adduser \
    --disabled-password \
    --no-create-home \
    dce
USER dce

WORKDIR /opt/discord_chat_exporter

COPY --from=build /build/publish ./

ENV PATH="$PATH:/opt/discord_chat_exporter"
ENTRYPOINT ["DiscordChatExporter.Cli"]

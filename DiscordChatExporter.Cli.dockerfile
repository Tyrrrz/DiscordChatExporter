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

RUN apk add --no-cache tzdata && \
    mkdir -p /opt/discord_chat_exporter && \
    adduser \
    --disabled-password \
    --no-create-home \
    dce
USER dce
COPY --from=build /build/publish /opt/discord_chat_exporter

WORKDIR /out

ENV PATH="$PATH:/opt/discord_chat_exporter"
ENTRYPOINT ["DiscordChatExporter.Cli"]

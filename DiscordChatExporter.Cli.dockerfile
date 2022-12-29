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

# tzdata is needed for DateTimeOffset.ToLocalTime (for TimeZoneInfo.Local, to be precise)
RUN apk add --no-cache tzdata && \
    adduser \
    --disabled-password \
    --no-create-home \
    dce
USER dce
COPY --from=build /build/publish /opt/discord_chat_exporter

# Need to keep this as /out for backwards compatibility with documentation.
# A lot of people have this directory mounted in their scripts files, so
# changing it would break existing workflows.
WORKDIR /out

ENV PATH="$PATH:/opt/discord_chat_exporter"
ENTRYPOINT ["DiscordChatExporter.Cli"]

# -- Build
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

# -- Run
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine

# Alpine dotnet image doesn't include timezone data, which is needed
# for certain date/time operations.
RUN apk add --no-cache tzdata

# Create a non-root user to run the app, so that the output files
# can be accessed by the host.
# https://github.com/Tyrrrz/DiscordChatExporter/issues/851
RUN adduser \
    --disabled-password \
    --no-create-home \
    dce

USER dce

COPY --from=build /build/publish /opt/dce

# Need to keep this as /out for backwards compatibility with documentation.
# A lot of people have this directory mounted in their scripts files, so
# changing it would break existing workflows.
WORKDIR /out

# Add the app directory to PATH so that it's easier to debug using a shell
ENV PATH="$PATH:/opt/dce"
ENTRYPOINT ["DiscordChatExporter.Cli"]

# -- Build
# Use the same platform for the build image as the runtime image, so that it's easier
# to produce a self-contained assembly for the right platform.
FROM mcr.microsoft.com/dotnet/sdk:7.0-runtime AS build

WORKDIR /build

COPY favicon.ico ./
COPY NuGet.config ./
COPY Directory.Build.props ./
COPY DiscordChatExporter.Core ./DiscordChatExporter.Core
COPY DiscordChatExporter.Cli ./DiscordChatExporter.Cli

# Publish a self-contained assembly so we can use a slimmer runtime image
RUN dotnet publish DiscordChatExporter.Cli \
    --self-contained \
    --use-current-runtime \
    --configuration Release \
    --output ./publish

# -- Run
# Use Alpine for the runtime image due to its smaller size.
# Use `runtime-deps` instead of `runtime` since we're running a self-contained assembly.
# https://github.com/dotnet/dotnet-docker/blob/main/samples/selecting-tags.md
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine

# Alpine image doesn't include timezone data, which is needed for certain date/time operations
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
RUN apk add --no-cache tzdata

# Create a non-root user to run the app, so that the output files can be accessed by the host
# https://github.com/Tyrrrz/DiscordChatExporter/issues/851
RUN adduser --disabled-password --no-create-home dce
USER dce

COPY --from=build /build/publish /opt/dce

# Add the app directory to the PATH so that it's easier to run the app via `docker exec`
ENV PATH="$PATH:/opt/dce"

# This directory is exposed to the user for mounting purposes, so it's important
# that it stays the same for backwards compatibility.
WORKDIR /out

ENTRYPOINT ["DiscordChatExporter.Cli"]

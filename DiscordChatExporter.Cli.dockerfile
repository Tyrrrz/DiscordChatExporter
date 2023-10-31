# -- Build
# Specify the platform here so that we pull the SDK image matching the host platform,
# instead of the target platform specified during build by the `--platform` option.
# Use the .NET 8.0 preview because the `--arch` option is only available in that version.
# https://github.com/dotnet/dotnet-docker/issues/4388#issuecomment-1459038566
# TODO: switch images to Alpine once .NET 8.0 is released.
# Currently, the correct preview version is only available on Debian.
# https://github.com/dotnet/dotnet-docker/blob/main/samples/selecting-tags.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

# Expose the target architecture set by the `docker build --platform` option, so that
# we can build the assembly for the correct platform.
ARG TARGETARCH

WORKDIR /tmp/dce

COPY favicon.ico .
COPY NuGet.config .
COPY Directory.Build.props .
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

# Publish a self-contained assembly so we can use a slimmer runtime image
RUN dotnet publish DiscordChatExporter.Cli \
    --configuration Release \
    -p:CSharpier_Bypass=true \
    --self-contained \
    --use-current-runtime \
    --arch $TARGETARCH \
    --output DiscordChatExporter.Cli/bin/publish/

# -- Run
# Use `runtime-deps` instead of `runtime` because we have a self-contained assembly
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime-deps:7.0 AS run

# Create a non-root user to run the app, so that the output files can be accessed by the host
# https://github.com/Tyrrrz/DiscordChatExporter/issues/851
RUN adduser --disabled-password --no-create-home dce
USER dce

# This directory is exposed to the user for mounting purposes, so it's important that it always
# stays the same for backwards compatibility.
WORKDIR /out

COPY --from=build /tmp/dce/DiscordChatExporter.Cli/bin/publish /opt/dce
ENTRYPOINT ["/opt/dce/DiscordChatExporter.Cli"]
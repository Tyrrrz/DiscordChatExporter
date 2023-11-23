# -- Build
# Specify the platform here so that we pull the SDK image matching the host platform,
# instead of the target platform specified during build by the `--platform` option.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Expose the target architecture set by the `docker build --platform` option, so that
# we can build the assembly for the correct platform.
ARG TARGETARCH

# Allow setting the assembly version from the build command
ARG VERSION=0.0.0

WORKDIR /tmp/app

COPY favicon.ico .
COPY NuGet.config .
COPY Directory.Build.props .
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

# Publish a self-contained assembly so we can use a slimmer runtime image
RUN dotnet publish DiscordChatExporter.Cli \
    -p:Version=$VERSION \
    -p:CSharpier_Bypass=true \
    --configuration Release \
    --self-contained \
    --use-current-runtime \
    --arch $TARGETARCH \
    --output DiscordChatExporter.Cli/bin/publish/

# -- Run
# Use `runtime-deps` instead of `runtime` because we have a self-contained assembly
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS run

LABEL org.opencontainers.image.title="DiscordChatExporter.Cli"
LABEL org.opencontainers.image.description="DiscordChatExporter is an application that can be used to export message history from any Discord channel to a file."
LABEL org.opencontainers.image.authors="tyrrrz.me"
LABEL org.opencontainers.image.source="https://github.com/Tyrrrz/DiscordChatExporter"
LABEL org.opencontainers.image.licenses="MIT"

# Alpine image doesn't come with the ICU libraries pre-installed, so we need to install them manually.
# We need the full ICU data because we allow the user to specify any locale for formatting purposes.
RUN apk add --no-cache icu-libs
RUN apk add --no-cache icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8

# Use a non-root user to ensure that the files shared with the host are accessible by the host user.
# We can use the default user provided by the base image for this purpose.
# https://github.com/Tyrrrz/DiscordChatExporter/issues/851
USER $APP_UID

# This directory is exposed to the user for mounting purposes, so it's important that it always
# stays the same for backwards compatibility.
WORKDIR /out

COPY --from=build /tmp/app/DiscordChatExporter.Cli/bin/publish /opt/app
ENTRYPOINT ["/opt/app/DiscordChatExporter.Cli"]
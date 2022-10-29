# Linux Alpine environment with .NET runtime
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk add --no-cache icu-libs
RUN mkdir /opt/wexflow

# Copy local download of wexflow-5.7-linux-netcore.zip
# which was pre-downloaded and unzipped from https://github.com/aelassas/Wexflow/releases/download/v5.7/wexflow-5.7-linux-netcore.zip
COPY wexflow-5.7-linux-netcore/ /opt/wexflow/

# Instructions below translated from https://github.com/aelassas/Wexflow/wiki/Installation

# Tell Docker to use this as the base run directory for image:
# (replaces original 'cd' command in Wexflow installation)
WORKDIR /opt/wexflow/Wexflow.Server

# Modification of installation instructions so that
# Docker exposes the wexflow server once it’s run:
EXPOSE 8000
ENTRYPOINT ["dotnet", "Wexflow.Server.dll"]
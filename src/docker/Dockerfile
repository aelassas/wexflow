# syntax=docker/dockerfile:1

# Linux .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

#RUN apk add --no-cache icu-dev icu-libs icu-data-full

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

RUN mkdir /opt/wexflow

# Copy local download of wexflow-linux-netcore.zip
# which was pre-downloaded and unzipped from https://github.com/aelassas/wexflow/releases/latest
COPY wexflow/ /opt/wexflow/

# Instructions below translated from https://github.com/aelassas/wexflow/wiki/Installation

# Tell Docker to use this as the base run directory for image:
# (replaces original 'cd' command in Wexflow installation)
WORKDIR /opt/wexflow/Wexflow.Server

# Modification of installation instructions so that
# Docker exposes the wexflow server once it’s run:
ENTRYPOINT ["dotnet", "Wexflow.Server.dll"]
EXPOSE 8000

SQLite won't work on nanoserver-ltsc2025 because it is too minimal and does not support the APIs required by SQLite and Visual C++. You need to switch to another DB, for example LiteDB:
1. Download [wexflow-10.0-windows-netcore.zip](https://github.com/aelassas/wexflow/releases/download/v10.0/wexflow-10.0-windows-netcore.zip) and extract it to `wexflow` folder
1. Open `wexflow\Wexflow\Wexflow.xml` config file
1. Set `dbType` to `LiteDB`
1. Comment `SQLite` connection string
1. Uncomment `LiteDB` connection string located at the end of the file

`wexflow` folder must be next to `Dockerfile`.

Here is a working `Dockerfile` on nanoserver-ltsc2025:
```dockerfile
# syntax=docker/dockerfile:1

# .NET 10 ASP.NET runtime on Nano Server LTSC 2025
FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2025

# Create Wexflow installation directory under Apps
RUN mkdir "C:\Apps\Wexflow"

# Copy the pre-unzipped wexflow-10.0-linux-netcore.zip Wexflow release
# COPY folder (without trailing backslash)
COPY wexflow "C:\Apps\Wexflow"

# Copy wexflow config folder
RUN mkdir "C:\Apps\Wexflow-netcore"
COPY wexflow/Wexflow-netcore "C:\Wexflow-netcore"

# Set working directory to Wexflow.Server
WORKDIR "C:\Apps\Wexflow\Wexflow.Server"

# Expose Wexflow HTTP endpoint
EXPOSE 8000

# Start Wexflow
ENTRYPOINT ["dotnet", "Wexflow.Server.dll"]
```

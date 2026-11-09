[![build](https://github.com/aelassas/wexflow/actions/workflows/build.yml/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/build.yml)
[![Docker Image](https://img.shields.io/badge/docker-image-brightgreen?style=flat&logo=docker)](https://hub.docker.com/r/aelassas/wexflow)
[![NuGet](https://img.shields.io/nuget/v/Wexflow.svg)](https://www.nuget.org/packages/Wexflow/)
[![](https://img.shields.io/badge/docs-wiki-brightgreen)](https://github.com/aelassas/wexflow/wiki)

<!--
[![build](https://github.com/aelassas/wexflow/actions/workflows/build.yml/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/build.yml)
[![release](https://github.com/aelassas/wexflow/actions/workflows/release.yml/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/release.yml)
[![Docker Image](https://img.shields.io/badge/docker-image-brightgreen?style=flat&logo=docker)](https://hub.docker.com/r/aelassas/wexflow)
[![Nuget](https://img.shields.io/nuget/dt/Wexflow)](https://www.nuget.org/packages/Wexflow/) 
[![](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
[![NuGet](https://img.shields.io/nuget/v/Wexflow.svg)](https://www.nuget.org/packages/Wexflow/)
[![](https://img.shields.io/badge/docs-wiki-brightgreen)](https://github.com/aelassas/wexflow/wiki)

[![Build Status](https://aelassas.visualstudio.com/wexflow/_apis/build/status/aelassas.wexflow?branchName=main)](https://aelassas.visualstudio.com/wexflow/_build/latest?definitionId=3&branchName=main) 
[![Latest release](https://img.shields.io/github/v/release/aelassas/wexflow?label=Release&logo=github)](https://github.com/aelassas/wexflow/releases/latest)
[![docker-hub](https://github.com/aelassas/wexflow/actions/workflows/docker-hub.yml/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/docker-hub.yml)
-->

<!--
[![](https://wexflow.github.io/content/cover-small.png)](https://wexflow.github.io/)

* [Migration Guide to v10.0](https://github.com/aelassas/wexflow/wiki/Migration-Guide-to-v10.0)
-->

## Wexflow

Wexflow is a cross-platform workflow automation engine that supports a wide range of tasks, from file operations and system processes to scripting, networking, and more. Wexflow targets both developers and technical users who need automation (file operations, tasks, scheduling, alerts, etc.). Wexflow focuses on automating technical jobs like moving or uploading files, sending emails, running scripts, or scheduling batch processes. For more complex scenarios, you can create your own custom activities, install them, and use them to extend its capabilities.

Workflows can be sequential or flowchart-based, with conditional logic such as if/else, while, and switch. They can be launched when the engine starts, manually, periodically, or on a cron schedule, and a FileSystemWatcher task can react to file events. Workflows run asynchronously, can be suspended and resumed, and can include approval steps so that a person approves or rejects a run before the remaining tasks execute.

Wexflow ships with more than 100 built-in tasks covering file operations, FTP and SFTP transfers, archives, encryption, HTTP calls, SQL, email, scripting, media processing, and integrations with services such as Slack, YouTube, and Twilio. Workflows can use local variables, global variables, and REST variables that are set at runtime when a workflow is started through the API, along with built-in functions such as `$DateTime()` and `$Guid()`. Custom tasks are written in C# using the [Wexflow NuGet package](https://www.nuget.org/packages/Wexflow) and installed as assemblies, and a SubWorkflow task lets one workflow call another.

Workflows are managed from a web admin panel that includes a drag and drop visual designer, real-time statistics, and detailed logs. An Android app lets you list workflows and start, stop, suspend, resume, approve, or reject them from a phone. A RESTful API exposes the engine to external systems, with official client examples in C#, PowerShell, JavaScript, PHP, Python, Go, Rust, Ruby, Java, and C++, as well as a command line client. Server-Sent Events push workflow job updates to clients in real time, so they do not need to poll the server.

Wexflow stores its data in SQLite by default, and it also supports MongoDB, SQL Server, PostgreSQL, MySQL, and LiteDB. Authentication relies on JWT, PBKDF2-hashed passwords, HttpOnly cookies, and HTTPS, which together help protect against XSS, XST, CSRF, and MITM attacks. The admin panel is available in English, French, and Danish. Wexflow runs on Windows, Linux, and macOS, and it is also distributed as a Docker image for easy deployment.

## Quick Links
- [Download](https://github.com/aelassas/wexflow/releases/latest)
- [Installation Guide](https://github.com/aelassas/wexflow/wiki/Installing)
- [Getting Started](https://github.com/aelassas/wexflow/wiki/Getting-Started)
- [Full Documentation](https://github.com/aelassas/wexflow/wiki)

## Quick Start

You can run Wexflow using Docker from the official image on Docker Hub:
```bash
docker run -d -p 8000:8000 --name wexflow aelassas/wexflow:latest
```

Then access the admin panel at: http://localhost:8000
- **Username:** `admin`  
- **Password:** `wexflow2018`

For full Docker usage and options, see the [Docker Hub page](https://hub.docker.com/r/aelassas/wexflow).

## Stats for Nerds

[![LoC Total](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/loc-total.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
[![Nuget](https://img.shields.io/nuget/dt/Wexflow)](https://www.nuget.org/packages/Wexflow/) 

<!--
[![LoC Prod](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/loc-prod.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
[![LoC Tests](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/loc-tests.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
-->

## Support

Wexflow is free and open source. If it helped you, saved you time, or powered your production workflows, consider supporting the project via  [PayPal](https://www.paypal.me/aelassaspp), [GitHub Sponsors](https://github.com/sponsors/aelassas), [Liberapay](https://liberapay.com/aelassas/), or [Buy Me a Coffee](https://www.buymeacoffee.com/aelassas) to keep it active, independent, and free for everyone.

## Acknowledgments

This project is supported by:

<a href="https://www.jetbrains.com/community/opensource/">
  <img alt="JetBrains" src="https://github.com/user-attachments/assets/f2500294-c5da-47cb-8d5e-e65b4d90c88e"/>
</a>

## License

Wexflow is [MIT licensed](https://github.com/aelassas/wexflow/blob/main/LICENSE.txt).

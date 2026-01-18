[![Build Status](https://aelassas.visualstudio.com/wexflow/_apis/build/status/aelassas.wexflow?branchName=main)](https://aelassas.visualstudio.com/wexflow/_build/latest?definitionId=3&branchName=main) 
[![release](https://github.com/aelassas/wexflow/actions/workflows/release.yml/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/release.yml)
[![Docker Image](https://img.shields.io/badge/docker-image-brightgreen?style=flat&logo=docker)](https://hub.docker.com/r/aelassas/wexflow)
[![Nuget](https://img.shields.io/nuget/dt/Wexflow)](https://www.nuget.org/packages/Wexflow/) 
[![](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
[![NuGet](https://img.shields.io/nuget/v/Wexflow.svg)](https://www.nuget.org/packages/Wexflow/)
[![](https://img.shields.io/badge/docs-wiki-brightgreen)](https://github.com/aelassas/wexflow/wiki)

<!--
[![](https://img.shields.io/badge/docs-wiki-brightgreen)](https://github.com/aelassas/wexflow/wiki)
[![](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
[![Latest release](https://img.shields.io/github/v/release/aelassas/wexflow?label=Release&logo=github)](https://github.com/aelassas/wexflow/releases/latest)

[![docker-hub](https://github.com/aelassas/wexflow/actions/workflows/docker-hub.yml/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/docker-hub.yml)
-->

[![](https://wexflow.github.io/content/cover-small.png)](https://wexflow.github.io/)

* [Migration Guide to v10.0](https://github.com/aelassas/wexflow/wiki/Migration-Guide-to-v10.0)

## Wexflow

Wexflow is a workflow automation engine that supports a wide range of tasks, from file operations and system processes to scripting, networking, and more. Wexflow targets both developers and technical users who need automation (file ops, tasks, scheduling, alerts, etc.). Wexflow focuses on automating technical jobs like moving or uploading files, sending emails, running scripts, or scheduling batch processes. For more complex scenarios, you can create your own custom activities, install them, and use them to extend its capabilities.

## Quick Links
<!--
- [Download](https://github.com/aelassas/wexflow/releases/latest)
- [Install Guide](https://github.com/aelassas/wexflow/wiki/Installing)
- [Official Docker Hub Image](https://hub.docker.com/r/aelassas/wexflow)
- [Getting Started](https://github.com/aelassas/wexflow/wiki/Getting-Started)
- [Configuration Guide](https://github.com/aelassas/wexflow/wiki/Configuration)
- [REST API Reference](https://github.com/aelassas/wexflow/wiki/RESTful-API)
- [Built-in Tasks](https://github.com/aelassas/wexflow/wiki/Tasks)
- [Custom Tasks](https://github.com/aelassas/wexflow/wiki/Custom-Tasks)
- [Full Documentation](https://github.com/aelassas/wexflow/wiki)
-->

- [Download](https://github.com/aelassas/wexflow/releases/latest)
- [Install Guide](https://github.com/aelassas/wexflow/wiki/Installing)
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

<!--
## Admin Panel Login

Once Wexflow is installed, you can access the admin panel at:

- **URL:** http://localhost:8000/  
- **Username:** `admin`  
- **Password:** `wexflow2018`

**Important:** For your security, change the default password after your first login.
-->

<!--
## Is Wexflow a Business Process Management Solution?

Wexflow is primarily a workflow automation engine, not a full BPM suite. Wexflow does not natively support BPMN, human workflows, or built-in user forms. So if your business processes require a lot of human interaction, approvals, or business rule evaluation, Wexflow would be limited.

Wexflow excels at automating technical tasks, such as moving or transforming files, uploading to FTP/SFTP, running scripts (PowerShell, Bash, Python, etc.), scheduling and chaining tasks, triggering workflows by events, manual input, cron or watchfolders, designing flows visually (Designer UI), integrating with APIs and databases, supporting conditional logic (if/else, switch, while).

You can use Wexflow if your processes are mostly system-based, such as back-office automation (file syncing, reporting, monitoring), ETL pipelines, DevOps or IT operations automation or API integrations between systems.
-->

<!--
## How Does Wexflow Compare?

| Feature / Tool         | **Wexflow**            | Zapier         | Power Automate   | n8n             | Apache Airflow   |
|------------------------|------------------------|----------------|------------------|------------------|------------------|
| **Open Source**        | ✅ Yes                 | ❌ No          | ❌ No            | ✅ Yes          | ✅ Yes           |
| **Self-Hosted**        | ✅ Yes                 | ❌ No          | ✅ (Premium)     | ✅ Yes          | ✅ Yes           |
| **Visual Designer**    | ✅ Built-in (Web)      | ✅ Yes         | ✅ Yes           | ✅ Yes          | 🟡 Limited (via plugins) |
| **Custom Task Support**| ✅ C# tasks, scripts   | ❌ No          | 🟡 Limited       | ✅ JS functions | ✅ Python         |
| **Execution Graph**    | ✅ Yes (Flowchart)     | ❌ No          | ❌ No            | 🟡 Simple logic | ✅ DAGs          |
| **Trigger Types**      | Cron, Events, Watchers | App events     | App events       | Cron, Webhooks  | Cron, DAG Triggers |
| **Offline Usage**      | ✅ Yes                 | ❌ No          | ❌ No            | ✅ Yes          | ✅ Yes           |
| **Best For**           | Devs & Sysadmins       | Non-tech users | Business users   | Devs & startups | Data engineers   |

Wexflow gives you full control, extensibility, and offline capability with no vendor lock-in.
-->
## Features

### Workflow Engine
* Cross-platform workflow server
* Supports sequential, flowchart, and approval workflows
* Cron-based scheduling
* 100+ built-in activities
* 6+ database engines supported
* Extensible architecture for custom activities
* Push Notifications via SSE: Get real-time workflow job updates without polling
* Asynchronous workflow execution for improved concurrency and performance

### UI & Visualization
* Powerful web dashboard
* Visual workflow designer with drag & drop interface
* Real-time workflow statistics and monitoring
* Extensive logging for transparency and debugging

### Multi-Platform Support
* Native Android app
* Responsive web interface

### Internationalization & APIs
* Multiple language support (English, French, Danish)
* RESTful API for integration with external systems
* REST API Clients: Official examples for popular languages (C#, PowerShell, JS, PHP, Python, Go, Rust, Ruby, Java, C++)
* Extensible with Custom Activities via NuGet

### Security & Performance
* Secure against XSS, XST, CSRF, and MITM
* Docker support for easy deployment
* Error monitoring

### Deployment & Compatibility
* Runs on macOS, Linux, Windows, and Docker

## Support

If this project helped you, saved you time, or inspired you in any way, please consider supporting its future growth and maintenance. You can show your support by starring the repository (it helps increase visibility and shows your appreciation), sharing the project (recommend it to colleagues, communities, or on social media), or making a donation (if you'd like to financially support the development) via [GitHub Sponsors](https://github.com/sponsors/aelassas) (one-time or monthly), [PayPal](https://www.paypal.me/aelassaspp), or [Buy Me a Coffee](https://www.buymeacoffee.com/aelassas). Open-source software requires time, effort, and resources to maintain—your support helps keep this project alive, up-to-date, and accessible to everyone. Every contribution, big or small, makes a difference and motivates continued work on features, bug fixes, and new ideas.

<!--<a href="https://github.com/sponsors/aelassas"><img src="https://aelassas.github.io/content/github-sponsor-button.png" alt="GitHub" width="210"></a>
<a href="https://www.paypal.me/aelassaspp"><img src="https://aelassas.github.io/content/paypal-button-v2.png" alt="PayPal" width="208"></a>
<a href="https://www.buymeacoffee.com/aelassas"><img src="https://aelassas.github.io/content/bmc-button.png" alt="Buy Me A Coffee" width="160"></a>-->

<!--
## Website Source Code (wexflow.github.io)

The source code for the official Wexflow website is available here:

[https://github.com/wexflow/wexflow.github.io](https://github.com/wexflow/wexflow.github.io)

It features a clean landing page with multilingual support, dark mode, and SEO optimizations to help it reach users in different languages and regions.

The codebase follows the Separation of Concerns (SoC) principle, with a modular and maintainable architecture that aligns with the Single Responsibility Principle (SRP), modularity, and modern frontend best practices. It uses GitHub Actions for automatic builds and deployments.

⚡ **Ultra-fast performance**

The website loads in under 1.5 seconds on slow 4G with **0ms blocking**, **0 layout shift**, and a blazing **Speed Index of 0.8**.

Feel free to explore the code, suggest improvements, or use it as a template for your own landing page.
-->

## Sponsors

This project is supported by:

<a href="https://www.jetbrains.com/community/opensource/">
  <img alt="JetBrains" src="https://aelassas.github.io/content/jetbrains.png"/>
</a>

## License

Wexflow is [MIT licensed](https://github.com/aelassas/wexflow/blob/main/LICENSE.txt).

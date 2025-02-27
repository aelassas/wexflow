[![Build Status](https://aelassas.visualstudio.com/wexflow/_apis/build/status/aelassas.wexflow?branchName=main)](https://aelassas.visualstudio.com/wexflow/_build/latest?definitionId=3&branchName=main) [![Nuget](https://img.shields.io/nuget/dt/wexflow)](https://www.nuget.org/packages/Wexflow/) [![](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml) [![NuGet](https://img.shields.io/nuget/v/Wexflow.svg)](https://www.nuget.org/packages/Wexflow/)

<!--
[![](https://img.shields.io/badge/docs-wiki-brightgreen)](https://github.com/aelassas/wexflow/wiki)
[![](https://raw.githubusercontent.com/aelassas/wexflow/refs/heads/loc/badge.svg)](https://github.com/aelassas/wexflow/actions/workflows/loc.yml)
[![Latest release](https://img.shields.io/github/v/release/aelassas/wexflow?label=Release&logo=github)](https://github.com/aelassas/wexflow/releases/latest)
-->

Wexflow is an open-source, cross-platform workflow engine and automation platform designed to streamline and automate recurring tasks. Developed primarily in .NET, it offers a robust solution for building automation and workflow processes with ease.

Wexflow provides a cross-platform workflow server, an admin dashboard for designing, managing, and tracking workflows, sequential workflows, flowchart workflows, and approval workflows on generic business objects called records.

Wexflow provides 100+ activities for different kind of tasks and allows custom integration with other solutions through custom activities and Wexflow API.

## Features

* Cross-platform workflow server
* Powerful dashboard
* UI designer
* Native Android App
* Sequential workflows
* Flowchart workflows
* Approval workflows
* 6+ databases supported
* 100+ activities
* Cron scheduling
* Extensive logging
* Real-time stats
* Multiple Language support
* RESTful API
* Extensible
* Supported Platforms: macOS, Linux, Windows, Docker

## Documentation

1. [Installing](https://github.com/aelassas/wexflow/wiki/Installing)
    1. [Windows](https://github.com/aelassas/wexflow/wiki/Installing#windows-net)
    2. [Linux](https://github.com/aelassas/wexflow/wiki/Installing#linux-net-core)
    3. [macOS](https://github.com/aelassas/wexflow/wiki/Installing#macos-net-core)
2. [Screenshots](https://github.com/aelassas/wexflow/wiki/Screenshots)
3. [Docker](https://github.com/aelassas/wexflow/wiki/Docker)
4. [Configuration](https://github.com/aelassas/wexflow/wiki/Configuration)
   1. [Wexflow Server](https://github.com/aelassas/wexflow/wiki/Configuration#wexflow-server)
   2. [Wexflow.xml](https://github.com/aelassas/wexflow/wiki/Configuration#wexflowxml)
   3. [Backend](https://github.com/aelassas/wexflow/wiki/Configuration#backend)
5. [Persistence Providers](https://github.com/aelassas/wexflow/wiki/Persistence-Providers)
6. [Getting Started](https://github.com/aelassas/wexflow/wiki/Getting-Started)
7. [Android App](https://github.com/aelassas/wexflow/wiki/Android-App)
8. [Samples](https://github.com/aelassas/wexflow/wiki/Samples)
   1. [Sequential workflows](https://github.com/aelassas/wexflow/wiki/Samples#sequential-workflows)
   2. [Execution graph](https://github.com/aelassas/wexflow/wiki/Samples#execution-graph)
   3. [Flowchart workflows](https://github.com/aelassas/wexflow/wiki/Samples#flowchart-workflows)
       1. [If](https://github.com/aelassas/wexflow/wiki/Samples#if)
       2. [While](https://github.com/aelassas/wexflow/wiki/Samples#while)
       3. [Switch](https://github.com/aelassas/wexflow/wiki/Samples#switch)
   4. [Approval workflows](https://github.com/aelassas/wexflow/wiki/Samples#approval-workflows)
        1. [Simple approval workflow](https://github.com/aelassas/wexflow/wiki/Samples#simple-approval-workflow)
        2. [OnRejected workflow event](https://github.com/aelassas/wexflow/wiki/Samples#onrejected-workflow-event)
        3. [YouTube approval workflow](https://github.com/aelassas/wexflow/wiki/Samples#youtube-approval-workflow)
        4. [Form submission approval workflow](https://github.com/aelassas/wexflow/wiki/Samples#form-submission-approval-workflow)
   5. [Workflow events](https://github.com/aelassas/wexflow/wiki/Samples#workflow-events)
10. [Local Variables](https://github.com/aelassas/wexflow/wiki/Local-Variables)
11. [Global Variables](https://github.com/aelassas/wexflow/wiki/Global-Variables)
12. [REST Variables](https://github.com/aelassas/wexflow/wiki/REST-Variables)
12. [Functions](https://github.com/aelassas/wexflow/wiki/Functions)
13. [Cron Scheduling](https://github.com/aelassas/wexflow/wiki/Cron-Scheduling)
14. [Logging](https://github.com/aelassas/wexflow/wiki/Logging)
9. [Built-in Tasks](https://github.com/aelassas/wexflow/wiki/Tasks)
    1. [File system tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#file-system-tasks)
    2. [Encryption tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#encryption-tasks)
    3. [Compression tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#compression-tasks)
    4. [Iso tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#iso-tasks)
    5. [Speech tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#speech-tasks)
    6. [Hashing tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#hashing-tasks)
    7. [Process tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#process-tasks)
    8. [Network tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#network-tasks)
    9. [XML tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#xml-tasks)
    10. [SQL tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#sql-tasks)
    11. [WMI tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#wmi-tasks)
    12. [Image tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#image-tasks)
    13. [Audio and video tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#audio-and-video-tasks)
    14. [Email tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#email-tasks)
    15. [Workflow tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#workflow-tasks)
    16. [Social media tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#social-media-tasks)
    17. [Waitable tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#waitable-tasks)
    18. [Reporting tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#reporting-tasks)
    19. [Web tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#web-tasks)
    20. [Script tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#script-tasks)
    21. [JSON and YAML tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#json-and-yaml-tasks)
    22. [Entities tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#entities-tasks)
    23. [Flowchart tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#flowchart-tasks)
    24. [Approval tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#approval-tasks)
    25. [Notification tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#notification-tasks)
    26. [SMS tasks](https://github.com/aelassas/Wexflow/wiki/Tasks#sms-tasks)
15. [Custom Tasks](https://github.com/aelassas/wexflow/wiki/Custom-Tasks)
    1. [General](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#general)
    2. [.NET Core](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#net-core)
    3. [Referenced Assemblies](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#referenced-assemblies)
    4. [Update](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#update)
    5. [Suspend/Resume](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#suspendresume)
    6. [Logging](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#logging)
    7. [Files](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#files)
    8. [Entities](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#entities)
    10. [Shared Memory](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#shared-memory)
    11. [Designer](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#designer)
    12. [Debugging](https://github.com/aelassas/wexflow/wiki/Custom-Tasks#debugging)
16. [Command Line Client](https://github.com/aelassas/wexflow/wiki/Command-Line-Client)
17. [RESTful API](https://github.com/aelassas/wexflow/wiki/RESTful-API)
    1. [API](https://github.com/aelassas/wexflow/wiki/RESTful-API)
    2. [C# client](https://github.com/aelassas/wexflow/wiki/C%23-Client)
    3. [PHP client](https://github.com/aelassas/wexflow/wiki/PHP-client)
18. [Run from Source](https://github.com/aelassas/wexflow/wiki/Run-From-Source)

## Support

If you find this project helpful, consider buying me a coffee.

<a href="https://www.buymeacoffee.com/aelassas" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

<!--
## Sponsors

[![JetBrains](https://wexflow.github.io/content/jetbrains.png)](https://www.jetbrains.com/)
-->

## License

Wexflow is [MIT licensed](https://github.com/aelassas/wexflow/blob/main/LICENSE.txt).

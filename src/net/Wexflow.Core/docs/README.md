# Wexflow

**Core assembly of the Wexflow workflow engine**

This NuGet package provides the core components needed to build and extend workflows in [Wexflow](https://github.com/aelassas/wexflow), a powerful and extensible open-source workflow automation engine.

Use this package to create **custom tasks** that can be integrated into your workflows. It includes the base `Task` class, logging, execution helpers, and utility functions.

Full documentation for creating custom tasks is available [here](https://github.com/aelassas/wexflow/wiki/Custom-Tasks).


## Features

- Base class for custom task development
- Integrated logging and utility helpers
- Compatible with all Wexflow editions .NET 4.8 and .NET 8.0+)
- Cross-platform support

## Installation

To install via .NET CLI:
```
dotnet add package Wexflow
```
Or via NuGet Package Manager:
```
Install-Package Wexflow
```

## Quick Links

- [Download Latest Release](https://github.com/aelassas/wexflow/releases/latest)
- [Install Guide](https://github.com/aelassas/wexflow/wiki/Installing)
- [Getting Started](https://github.com/aelassas/wexflow/wiki/Getting-Started)
- [Configuration Guide](https://github.com/aelassas/wexflow/wiki/Configuration)
- [REST API Reference](https://github.com/aelassas/wexflow/wiki/RESTful-API)
- [Built-in Tasks](https://github.com/aelassas/wexflow/wiki/Tasks)
- [Custom Tasks](https://github.com/aelassas/wexflow/wiki/Custom-Tasks)

## Example: Creating a Custom Task

To define your own task, inherit from the `Task` class and override the `Run` method:
```cs
using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;

namespace Wexflow.Tasks.MyTask
{
    public class MyTask : Task
    {
        public MyTask(XElement xe, Workflow wf) : base(xe, wf)
        {
            // Initialize task settings from the XML element if needed
            // Example: string settingValue = GetSetting("mySetting");
        }

        public override TaskStatus Run()
        {
            try
            {
                // Task logic goes here
                Info("Running my custom task...");

                // WaitOne() enables suspend/resume support in .NET 8.0+.
                // Call this to pause the task when the workflow is suspended.
                WaitOne();

                // Return success when the task completes successfully
                return new TaskStatus(Status.Success);
            }
            catch (ThreadInterruptedException)
            {
                // Required for proper stop handling (do not swallow this exception)
                throw;
            }
            catch (Exception ex)
            {
                // Log unexpected errors and return error status
                ErrorFormat("An error occurred while executing the task.", ex);
                return new TaskStatus(Status.Error);
            }
        }
    }
}
```

**Need a starting point?**

- For **.NET Framework 4.8 (Legacy Version)**, you can find a complete example of a custom task here:  
  [Wexflow.Tasks.Template (.NET 4.8)](https://github.com/aelassas/wexflow/tree/main/src/net/Wexflow.Tasks.Template)

- For **.NET 8.0+ (Stable Version)**, check out the full example here:  
  [Wexflow.Tasks.Template (.NET 8.0+)](https://github.com/aelassas/wexflow/tree/main/src/netcore/Wexflow.Tasks.Template)


## Installing Your Custom Task in Wexflow

### .NET Framework 4.8 (Legacy Version)

Once you've finished coding your custom task, compile the class library project and copy the `Wexflow.Tasks.MyTask.dll` assembly into one of the following folders:

- `C:\Program Files\Wexflow\`
- `C:\Wexflow\Tasks\` *(default for Wexflow .NET 4.8 version)*

The `Tasks` folder path can be configured via the `tasksFolder` setting in the configuration file:  `C:\Wexflow\Wexflow.xml`

**Important:** The namespace and DLL filename of your task **must start with `Wexflow.Tasks`**.

### .NET 8.0+ (Stable Version)

If you're using the .NET 8.0+ version of Wexflow, copy `Wexflow.Tasks.MyTask.dll` to the appropriate platform-specific folder:

- **Windows**:
  - `C:\Wexflow-netcore\Tasks`
  - `.\Wexflow.Server`
- **Linux**:
  - `/opt/wexflow/Wexflow/Tasks`
  - `/opt/wexflow/Wexflow.Server`
- **macOS**:
  - `/Applications/wexflow/Wexflow/Tasks`
  - `/Applications/wexflow/Wexflow.Server`



### Referenced Assemblies

If your custom task depends on additional assemblies (DLLs), copy them as follows:

- **.NET 4.8**: `C:\Program Files\Wexflow\`
- **.NET 8.0+**:
  - **Windows**: `C:\Wexflow-netcore\Tasks` or `.\Wexflow.Server`
  - **Linux**: `/opt/wexflow/Wexflow/Tasks` or `/opt/wexflow/Wexflow.Server`
  - **macOS**: `/Applications/wexflow/Wexflow/Tasks` or `/Applications/wexflow/Wexflow.Server`



### Updating a Custom Task

To update an existing custom task:

1. Replace the old DLL and its referenced assemblies with the new versions in the correct folder.
2. Restart the Wexflow server:

- **.NET 4.8**: Restart the **Wexflow Windows Service**
- **.NET 8.0+**:
  - **Windows**: Run `.\run.bat`
  - **Linux**: Run `sudo systemctl restart wexflow`
  - **macOS**: Run `dotnet /Applications/wexflow/Wexflow.Server/Wexflow.Server.dll`



### Using Your Custom Task

Once installed, your task can be used in workflows like this:
```xml
<Task id="$int" name="MyTask" description="My task description" enabled="true">
    <Setting name="settingName" value="settingValue" />
</Task>
```
You can then test your custom task by creating a new workflow using either the **Designer** or the **XML editor**.

Example using the XML editor:
```xml
<Workflow xmlns="urn:wexflow-schema" id="99" name="Workflow_MyWorkflow" description="Workflow_MyWorkflow">
    <Settings>
        <Setting name="launchType" value="trigger" /<!-- startup | trigger | periodic | cron -->
        <Setting name="enabled" value="true" /      <!-- true | false -->
    </Settings>
    <Tasks>
        <Task id="1" name="MyTask" description="My task description" enabled="true">
            <Setting name="settingName" value="settingValue" />
        </Task>
    </Tasks>
</Workflow>
```
This workflow will appear in the Wexflow Manager. You can launch and monitor it from there.

That's it! You're now ready to create, install, and run your own custom tasks in Wexflow.

---

For issues, contributions, or updates, visit the [Wexflow GitHub repository](https://github.com/aelassas/wexflow).

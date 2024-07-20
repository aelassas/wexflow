using System.Diagnostics;
using System.IO;
using System.Threading;
using Wexflow.Core;

namespace Wexflow.Tests
{
    public class Helper
    {
        public const string TEMP_FOLDER = @"C:\Wexflow\Temp\";
        public const string SOURCE_FILES_FOLDER = @"C:\WexflowTesting\";

        private static readonly WexflowEngine WexflowEngine = new WexflowEngine(
            @"C:\Wexflow\Wexflow.xml"
            , LogLevel.All
            , false
            , USERNAME
            , false
            , string.Empty
            , 0
            , false
            , string.Empty
            , string.Empty
            , string.Empty);

        private const string USERNAME = "admin";

        public static void SaveWorkflow(string xml, bool schedule)
        {
            _ = WexflowEngine.SaveWorkflow(USERNAME, Core.Db.UserProfile.SuperAdministrator, xml, schedule);
        }

        public static void Run()
        {
            WexflowEngine.Run();
        }

        public static void Stop()
        {
            WexflowEngine.Stop(false, false);
        }

        public static System.Guid StartWorkflow(int workflowId)
        {
            var instanceId = WexflowEngine.StartWorkflow(WexflowEngine.SuperAdminUsername, workflowId);

            // Wait until the workflow finishes
            Thread.Sleep(1000);
            var workflow = WexflowEngine.GetWorkflow(workflowId);
            var isRunning = workflow.IsRunning;
            var isWaitingForApproval = workflow.IsWaitingForApproval;
            while (isRunning && !isWaitingForApproval)
            {
                Thread.Sleep(100);
                workflow = WexflowEngine.GetWorkflow(workflowId);
                isRunning = workflow.IsRunning;
                isWaitingForApproval = workflow.IsWaitingForApproval;
            }

            return instanceId;
        }

        public static System.Guid StartWorkflowAsync(int workflowId)
        {
            return WexflowEngine.StartWorkflow(WexflowEngine.SuperAdminUsername, workflowId);
        }

        public static void StopWorkflow(int workflowId, System.Guid instanceId)
        {
            _ = WexflowEngine.StopWorkflow(workflowId, instanceId, WexflowEngine.SuperAdminUsername);
        }

        public static void SuspendWorkflow(int workflowId, System.Guid instanceId)
        {
            _ = WexflowEngine.SuspendWorkflow(workflowId, instanceId);
        }

        public static void ResumeWorkflow(int workflowId, System.Guid instanceId)
        {
            WexflowEngine.ResumeWorkflow(workflowId, instanceId);
        }

        public static void ApproveWorkflow(int workflowId, System.Guid instanceId)
        {
            _ = WexflowEngine.ApproveWorkflow(workflowId, instanceId, WexflowEngine.SuperAdminUsername);
        }

        public static void RejectWorkflow(int workflowId, System.Guid instanceId)
        {
            _ = WexflowEngine.RejectWorkflow(workflowId, instanceId, WexflowEngine.SuperAdminUsername);
        }

        public static Core.Workflow GetWorkflow(int workflowId)
        {
            return WexflowEngine.GetWorkflow(workflowId);
        }

        public static void DeleteFilesAndFolders(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            DeleteFiles(folder);

            foreach (var dir in Directory.GetDirectories(folder))
            {
                DeleteDirRec(dir);
            }
        }

        public static void DeleteFiles(string dir)
        {
            DeleteFiles(dir, "*.*");
        }

        public static void DeleteFiles(string dir, string searchPattern)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(dir, searchPattern))
            {
                File.Delete(file);
            }
        }

        public static void DeleteDirRec(string dir)
        {
            //foreach (var file in Directory.GetFiles(dir))
            //{
            //    File.Delete(file);
            //}

            foreach (var subdir in Directory.GetDirectories(dir))
            {
                DeleteDirRec(subdir);
            }

            Directory.Delete(dir, true);
        }

        public static void CopyDirRec(string src, string dest)
        {
            var dirName = Path.GetFileName(src);
            var destDir = Path.Combine(dest, dirName);
            _ = Directory.CreateDirectory(destDir);

            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
            {
                _ = Directory.CreateDirectory(dirPath.Replace(src, destDir));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(src, destDir), true);
            }
        }

        public static void StartProcess(string name, string cmd, bool hideGui)
        {
            var startInfo = new ProcessStartInfo(name, cmd)
            {
                CreateNoWindow = hideGui,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorHandler;
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public static string[] GetFiles(string dir, string pattern, SearchOption searchOption)
        {
            return Directory.GetFiles(dir, pattern, searchOption);
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("{0}", outLine.Data);
        }

        private static void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("{0}", outLine.Data);
        }
    }
}

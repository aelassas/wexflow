using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;
using Wexflow.Core.Db;
using Wexflow.Core.ExecutionGraph.Flowchart;
using Wexflow.Server.Contracts;
using LaunchType = Wexflow.Server.Contracts.LaunchType;
using StatusCount = Wexflow.Server.Contracts.StatusCount;
using User = Wexflow.Server.Contracts.User;
using UserProfile = Wexflow.Server.Contracts.UserProfile;

namespace Wexflow.Server
{
    public sealed partial class WexflowService
    {
        private const string Root = "/wexflow/";
        private static readonly XNamespace xn = "urn:wexflow-schema";

        private readonly IEndpointRouteBuilder _endpoints;

        public WexflowService(IEndpointRouteBuilder endpoints)
        {
            _endpoints = endpoints;
        }

        public void Map()
        {
            //
            // Dashboard
            //
            GetStatusCount();
            GetEntriesCountByDate();
            SearchEntriesByPageOrderBy();
            GetEntryStatusDateMin();
            GetEntryStatusDateMax();
            GetEntryLogs();

            //
            // Records
            //
            UploadVersion();
            DeleteTempVersionFile();
            DeleteTempVersionFiles();
            DownloadFile();
            SaveRecord();
            DeleteRecords();
            SearchRecords();
            GetRecordsCreatedBy();
            SearchRecordsCreatedByOrAssignedTo();

            //
            // Manager
            //
            Search();
            GetWorkflow();
            GetJob();
            GetJobs();
            StartWorkflow();
            StartWorkflowWithVariables();
            StopWorkflow();
            SuspendWorkflow();
            ResumeWorkflow();

            //
            // Approval
            //
            SearchApprovalWorkflows();
            ApproveWorkflow();
            RejectWorkflow();

            //
            // Designer
            // 
            GetTasks();
            GetWorkflowXml();
            GetWorkflowJson();
            GetTaskNames();
            SearchTaskNames();
            GetSettings();
            GetTaskXml();
            IsWorkflowIdValid();
            IsCronExpressionValid();
            IsPeriodValid();
            IsXmlWorkflowValid();
            GetNewWorkflowId();
            SaveXmlWorkflow();
            SaveWorkflow();
            DisableWorkflow();
            EnableWorkflow();
            DeleteWorkflow();
            DeleteWorkflows();
            GetExecutionGraph();
            GetExecutionGraphAsXml();
            GetExecutionGraphAsBlockly();
            UploadWorkflow();

            //
            // History
            //
            GetHistoryEntriesCountByDate();
            SearchHistoryEntriesByPageOrderBy();
            GetHistoryEntryStatusDateMin();
            GetHistoryEntryStatusDateMax();
            GetHistoryEntryLogs();

            //
            // Users
            //
            GetUser();
            SearchUsers();
            GetNonRestrictedUsers();
            InsertUser();
            UpdateUser();
            UpdateUsernameAndEmailAndUserProfile();
            DeleteUser();
            ResetPassword();

            //
            // Profiles
            //
            SearchAdministrators();
            GetUserWorkflows();
            SaveUserWorkflows();

            //
            // Notifications
            //
            HasNotifications();
            MarkNotificationsAsRead();
            MarkNotificationsAsUnread();
            DeleteNotifications();
            SearchNotifications();
            Notify();
            NotifyApprovers();
        }

        private static async System.Threading.Tasks.Task WriteFalse(HttpContext context)
        {
            await context.Response.WriteAsync(JsonConvert.SerializeObject(false));
        }

        private static async System.Threading.Tasks.Task WriteTrue(HttpContext context)
        {
            await context.Response.WriteAsync(JsonConvert.SerializeObject(true));
        }

        private static async System.Threading.Tasks.Task WriteEmpty(HttpContext context)
        {
            await context.Response.WriteAsync(JsonConvert.SerializeObject(string.Empty));
        }

        private static async System.Threading.Tasks.Task Unauthorized(HttpContext context)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(JsonConvert.SerializeObject("Unauthorized!"));
        }

        private static async System.Threading.Tasks.Task Error(HttpContext context, Exception e)
        {
            Console.WriteLine(e);
            await WriteFalse(context);
        }

        private static async System.Threading.Tasks.Task WorkflowNotFound(HttpContext context)
        {
            context.Response.StatusCode = 204;
            await context.Response.WriteAsync(JsonConvert.SerializeObject("Workflow not found!"));
        }

        /// <summary>
        /// Search for workflows.
        /// </summary>
        private void Search()
        {
            _endpoints.MapGet(Root + "search", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string keywordToUpper = context.Request.Query["s"].ToString().ToUpper();

                WorkflowInfo[] workflows = Array.Empty<WorkflowInfo>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        workflows = WexflowServer.WexflowEngine.Workflows
                            .ToList()
                            .Where(wf =>
                                    wf.Name.ToUpper().Contains(keywordToUpper)
                                    || wf.Description.ToUpper().Contains(keywordToUpper)
                                    || wf.Id.ToString().Contains(keywordToUpper))
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                wf.IsExecutionGraphEmpty
                               , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                               , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])))
                            .ToArray();
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        workflows = WexflowServer.WexflowEngine.GetUserWorkflows(user.GetDbId())
                                                .ToList()
                                                .Where(wf =>
                                                    wf.Name.ToUpper().Contains(keywordToUpper)
                                                    || wf.Description.ToUpper().Contains(keywordToUpper)
                                                    || wf.Id.ToString().Contains(keywordToUpper))
                                                .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                                                    (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                    wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                    wf.IsExecutionGraphEmpty
                                                   , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                                                   , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])))
                                                .ToArray();
                    }
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflows));
            });
        }

        /// <summary>
        /// Search for approval workflows.
        /// </summary>
        private void SearchApprovalWorkflows()
        {
            _endpoints.MapGet(Root + "searchApprovalWorkflows", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string keywordToUpper = context.Request.Query["s"].ToString().ToUpper();

                WorkflowInfo[] workflows = Array.Empty<WorkflowInfo>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        workflows = WexflowServer.WexflowEngine.Workflows
                            .ToList()
                            .Where(wf =>
                                wf.IsApproval &&
                                (wf.Name.ToUpper().Contains(keywordToUpper)
                                || wf.Description.ToUpper().Contains(keywordToUpper)
                                || wf.Id.ToString().Contains(keywordToUpper)))
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                wf.IsExecutionGraphEmpty
                               , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                               , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])))
                            .ToArray();
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        workflows = WexflowServer.WexflowEngine.GetUserWorkflows(user.GetDbId())
                                                .ToList()
                                                .Where(wf =>
                                                    wf.IsApproval &&
                                                    (wf.Name.ToUpper().Contains(keywordToUpper)
                                                    || wf.Description.ToUpper().Contains(keywordToUpper)
                                                    || wf.Id.ToString().Contains(keywordToUpper)))
                                                .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                                                    (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                    wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                    wf.IsExecutionGraphEmpty
                                                   , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                                                   , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])))
                                                .ToArray();
                    }
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflows));
            });
        }

        /// <summary>
        /// Returns a workflow from its id.
        /// </summary>
        private void GetWorkflow()
        {
            _endpoints.MapGet(Root + "workflow", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int id = int.Parse(context.Request.Query["w"].ToString());

                Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(id);
                if (wf != null)
                {
                    WorkflowInfo workflow = new(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description,
                        wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                        wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                        , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                        );

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);

                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                            if (check)
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                            }
                        }
                    }
                }
                else
                {
                    await WorkflowNotFound(context);
                }
            });
        }

        /// <summary>
        /// Returns a job from a workflow id and an instance id.
        /// </summary>
        private void GetJob()
        {
            _endpoints.MapGet(Root + "job", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int id = int.Parse(context.Request.Query["w"].ToString());
                string jobId = context.Request.Query["i"].ToString();

                Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(id);
                if (wf != null)
                {
                    if (jobId != wf.InstanceId.ToString())
                    {
                        wf = wf.Jobs.Where(j => j.Key.ToString() == jobId).Select(j => j.Value).FirstOrDefault();
                    }

                    if (wf != null)
                    {
                        WorkflowInfo workflow = new(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description,
                            wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                            , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                            , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                            );

                        Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);

                        if (user.Password.Equals(password))
                        {
                            if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                            }
                            else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                            {
                                bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                                if (check)
                                {
                                    await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                                }
                            }
                        }
                    }
                }
                else
                {
                    await WorkflowNotFound(context);
                }
            });
        }

        /// <summary>
        /// Returns a jobs from a workflow id.
        /// </summary>
        private void GetJobs()
        {
            _endpoints.MapGet(Root + "jobs", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int id = int.Parse(context.Request.Query["w"].ToString());

                Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(id);
                if (wf != null)
                {
                    IEnumerable<WorkflowInfo> jobs = wf.Jobs.Select(j => j.Value).Select(
                        w => new WorkflowInfo(w.DbId, w.Id, w.InstanceId, w.Name, w.FilePath, (LaunchType)w.LaunchType, w.IsEnabled, w.IsApproval, w.EnableParallelJobs, w.IsWaitingForApproval, w.Description,
                            w.IsRunning, w.IsPaused, w.Period.ToString(@"dd\.hh\:mm\:ss"), w.CronExpression,
                            w.IsExecutionGraphEmpty
                            , w.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                            , w.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                            ));

                    if (wf != null)
                    {
                        Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);

                        if (user.Password.Equals(password))
                        {
                            if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(jobs));
                            }
                            else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                            {
                                bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                                if (check)
                                {
                                    await context.Response.WriteAsync(JsonConvert.SerializeObject(jobs));
                                }
                            }
                        }
                    }
                }
                else
                {
                    await WorkflowNotFound(context);
                }
            });
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        private void StartWorkflow()
        {
            _endpoints.MapPost(Root + "start", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = int.Parse(context.Request.Query["w"].ToString());

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        Guid instanceId = WexflowServer.WexflowEngine.StartWorkflow(username, workflowId);
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            Guid instanceId = WexflowServer.WexflowEngine.StartWorkflow(username, workflowId);
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                        }
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        private static string GetBody(HttpContext context)
        {
            string body;
            using (StreamReader reader = new(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                body = reader.ReadToEnd();
            }
            return body;
        }

        /// <summary>
        /// Starts a workflow with variables.
        /// </summary>
        private void StartWorkflowWithVariables()
        {
            _endpoints.MapPost(Root + "startWithVariables", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string json = GetBody(context);
                JObject o = JObject.Parse(json);
                int workflowId = o.Value<int>("WorkflowId");
                JArray variables = o.Value<JArray>("Variables");

                List<Core.Variable> vars = new();
                foreach (JToken variable in variables)
                {
                    vars.Add(new Core.Variable { Key = variable.Value<string>("Name"), Value = variable.Value<string>("Value") });
                }

                Core.Workflow workflow = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId);

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        workflow.RestVariables.Clear();
                        workflow.RestVariables.AddRange(vars);
                        Guid instanceId = WexflowServer.WexflowEngine.StartWorkflow(username, workflowId);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = workflow.DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            workflow.RestVariables.Clear();
                            workflow.RestVariables.AddRange(vars);
                            Guid instanceId = WexflowServer.WexflowEngine.StartWorkflow(username, workflowId);

                            await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                        }
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Stops a workflow.
        /// </summary>
        private void StopWorkflow()
        {
            _endpoints.MapPost(Root + "stop", async context =>
            {
                bool res = false;

                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = int.Parse(context.Request.Query["w"].ToString());
                Guid instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = WexflowServer.WexflowEngine.StopWorkflow(workflowId, instanceId, username);
                        await context.Response.WriteAsync(string.Empty);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            res = WexflowServer.WexflowEngine.StopWorkflow(workflowId, instanceId, username);
                            await context.Response.WriteAsync(string.Empty);
                        }
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Suspends a workflow.
        /// </summary>
        private void SuspendWorkflow()
        {
            _endpoints.MapPost(Root + "suspend", async context =>
            {
                bool res = false;

                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = int.Parse(context.Request.Query["w"].ToString());
                Guid instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = WexflowServer.WexflowEngine.SuspendWorkflow(workflowId, instanceId);
                        await context.Response.WriteAsync(string.Empty);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            res = WexflowServer.WexflowEngine.SuspendWorkflow(workflowId, instanceId);
                            await context.Response.WriteAsync(string.Empty);
                        }
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        private void ResumeWorkflow()
        {
            _endpoints.MapPost(Root + "resume", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = int.Parse(context.Request.Query["w"].ToString());
                Guid instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        WexflowServer.WexflowEngine.ResumeWorkflow(workflowId, instanceId);
                        await context.Response.WriteAsync(string.Empty);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            WexflowServer.WexflowEngine.ResumeWorkflow(workflowId, instanceId);
                            await context.Response.WriteAsync(string.Empty);
                        }
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Approves a workflow.
        /// </summary>
        private void ApproveWorkflow()
        {
            _endpoints.MapPost(Root + "approve", async context =>
            {
                bool res = false;

                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = int.Parse(context.Request.Query["w"].ToString());
                Guid instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = WexflowServer.WexflowEngine.ApproveWorkflow(workflowId, instanceId, username);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            res = WexflowServer.WexflowEngine.ApproveWorkflow(workflowId, instanceId, username);
                        }
                    }
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Rejects a workflow.
        /// </summary>
        private void RejectWorkflow()
        {
            _endpoints.MapPost(Root + "reject", async context =>
            {
                bool res = false;

                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = int.Parse(context.Request.Query["w"].ToString());
                Guid instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = WexflowServer.WexflowEngine.RejectWorkflow(workflowId, instanceId, username);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (check)
                        {
                            res = WexflowServer.WexflowEngine.RejectWorkflow(workflowId, instanceId, username);
                        }
                    }
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Returns workflow's tasks.
        /// </summary>
        private void GetTasks()
        {
            _endpoints.MapGet(Root + "tasks/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"].ToString()));
                    if (wf != null)
                    {
                        IList<TaskInfo> taskInfos = new List<TaskInfo>();

                        foreach (Task task in wf.Tasks)
                        {
                            IList<SettingInfo> settingInfos = new List<SettingInfo>();

                            foreach (Setting setting in task.Settings)
                            {
                                IList<AttributeInfo> attributeInfos = new List<AttributeInfo>();

                                foreach (Core.Attribute attribute in setting.Attributes)
                                {
                                    AttributeInfo attributeInfo = new(attribute.Name, attribute.Value);
                                    attributeInfos.Add(attributeInfo);
                                }

                                SettingInfo settingInfo = new(setting.Name, setting.Value, attributeInfos.ToArray());
                                settingInfos.Add(settingInfo);
                            }

                            TaskInfo taskInfo = new(task.Id, task.Name, task.Description, task.IsEnabled, settingInfos.ToArray());

                            taskInfos.Add(taskInfo);
                        }

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(taskInfos));
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns next vacant Task Id.
        /// </summary>
        private void GetNewWorkflowId()
        {
            _endpoints.MapGet(Root + "workflowId", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                int workflowId = 0;
                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    try
                    {
                        IList<Core.Workflow> workflows = WexflowServer.WexflowEngine.Workflows;

                        if (workflows != null && workflows.Count > 0)
                        {
                            workflowId = workflows.Select(w => w.Id).Max() + 1;
                        }

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(workflowId));
                    }
                    catch (Exception e)
                    {
                        await Error(context, e);
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns a workflow as XML.
        /// </summary>
        private void GetWorkflowXml()
        {
            _endpoints.MapGet(Root + "xml/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"].ToString()));
                    if (wf != null)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(wf.Xml));
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns a workflow as JSON.
        /// </summary>
        private void GetWorkflowJson()
        {
            _endpoints.MapGet(Root + "json/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"].ToString()));
                    if (wf != null)
                    {
                        List<Contracts.Variable> variables = new();
                        foreach (Core.Variable variable in wf.LocalVariables)
                        {
                            variables.Add(new Contracts.Variable { Key = variable.Key, Value = variable.Value });
                        }

                        Contracts.Workflow.WorkflowInfo wi = new()
                        {
                            Id = wf.Id,
                            Name = wf.Name,
                            FilePath = wf.FilePath,
                            LaunchType = (int)wf.LaunchType,
                            Period = wf.Period.ToString(),
                            CronExpression = wf.CronExpression,
                            IsEnabled = wf.IsEnabled,
                            IsApproval = wf.IsApproval,
                            EnableParallelJobs = wf.EnableParallelJobs,
                            Description = wf.Description,
                            LocalVariables = variables.ToArray()
                        };

                        List<TaskInfo> tasks = new();
                        foreach (Task task in wf.Tasks)
                        {
                            List<SettingInfo> settings = new();
                            foreach (Setting setting in task.Settings)
                            {
                                List<AttributeInfo> attributes = new();
                                foreach (Core.Attribute attr in setting.Attributes)
                                {
                                    attributes.Add(new AttributeInfo(attr.Name, attr.Value));
                                }

                                settings.Add(new SettingInfo(setting.Name, setting.Value, attributes.ToArray()));
                            }
                            tasks.Add(new TaskInfo(task.Id, task.Name, task.Description, task.IsEnabled, settings.ToArray()));
                        }

                        Contracts.Workflow.Workflow workflow = new()
                        {
                            WorkflowInfo = wi,
                            Tasks = tasks.ToArray(),
                            ExecutionGraph = wf.ExecutionGraph
                        };

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns task names.
        /// </summary>
        private void GetTaskNames()
        {
            _endpoints.MapGet(Root + "taskNames", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    TaskName[] taskNames;
                    try
                    {
                        JArray array = JArray.Parse(File.ReadAllText(WexflowServer.WexflowEngine.TasksNamesFile));
                        taskNames = array.ToObject<TaskName[]>().OrderBy(x => x.Name).ToArray();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        taskNames = new[] { new TaskName { Name = "TasksNames.json is not valid." } };
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(taskNames));
                }
                else
                {
                    await Unauthorized(context);
                }

            });
        }

        /// <summary>
        /// Returns task names.
        /// </summary>
        private void SearchTaskNames()
        {
            _endpoints.MapGet(Root + "searchTaskNames", async context =>
            {
                string keywordToUpper = context.Request.Query["s"].ToString().ToUpper();

                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    TaskName[] taskNames;
                    try
                    {
                        JArray array = JArray.Parse(File.ReadAllText(WexflowServer.WexflowEngine.TasksNamesFile));
                        taskNames = array
                        .ToObject<TaskName[]>()
                        .Where(x => x.Name.ToUpper().Contains(keywordToUpper))
                        .OrderBy(x => x.Name).ToArray();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        taskNames = new[] { new TaskName { Name = "TasksNames.json is not valid." } };
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(taskNames));
                }
                else
                {
                    await Unauthorized(context);
                }

            });
        }

        /// <summary>
        /// Returns task settings.
        /// </summary>
        private void GetSettings()
        {
            _endpoints.MapGet(Root + "settings/{taskName}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    TaskSetting[] taskSettings;
                    try
                    {
                        JObject o = JObject.Parse(File.ReadAllText(WexflowServer.WexflowEngine.TasksSettingsFile));
                        JToken token = o.SelectToken(context.Request.RouteValues["taskName"].ToString());
                        taskSettings = token != null ? token.ToObject<TaskSetting[]>() : Array.Empty<TaskSetting>();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        taskSettings = new TaskSetting[] { new TaskSetting { Name = "TasksSettings.json is not valid." } };
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(taskSettings));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns a task as XML.
        /// </summary>
        private void GetTaskXml()
        {
            _endpoints.MapPost(Root + "taskToXml", async context =>
            {
                try
                {
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        string json = GetBody(context);

                        JObject task = JObject.Parse(json);

                        int taskId = (int)task.SelectToken("Id");
                        string taskName = (string)task.SelectToken("Name");
                        string taskDesc = (string)task.SelectToken("Description");
                        bool isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                        XElement xtask = new("Task"
                            , new XAttribute("id", taskId)
                            , new XAttribute("name", taskName)
                            , new XAttribute("description", taskDesc)
                            , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                        );

                        JToken settings = task.SelectToken("Settings");
                        foreach (JToken setting in settings)
                        {
                            string settingName = (string)setting.SelectToken("Name");
                            string settingValue = (string)setting.SelectToken("Value");

                            XElement xsetting = new("Setting"
                                , new XAttribute("name", settingName)
                            );

                            if (!string.IsNullOrEmpty(settingValue))
                            {
                                xsetting.SetAttributeValue("value", settingValue);
                            }

                            if (settingName == "selectFiles" || settingName == "selectAttachments")
                            {
                                if (!string.IsNullOrEmpty(settingValue))
                                {
                                    xsetting.SetAttributeValue("value", settingValue);
                                }
                            }
                            else
                            {
                                xsetting.SetAttributeValue("value", settingValue);
                            }

                            JToken attributes = setting.SelectToken("Attributes");
                            foreach (JToken attribute in attributes)
                            {
                                string attributeName = (string)attribute.SelectToken("Name");
                                string attributeValue = (string)attribute.SelectToken("Value");
                                xsetting.SetAttributeValue(attributeName, attributeValue);
                            }

                            xtask.Add(xsetting);
                        }

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(xtask.ToString()));
                    }
                    else
                    {
                        await Unauthorized(context);
                    }
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Checks if a workflow id is valid.
        /// </summary>
        private void IsWorkflowIdValid()
        {
            _endpoints.MapGet(Root + "isWorkflowIdValid/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    int workflowId = int.Parse(context.Request.RouteValues["id"].ToString());
                    foreach (Core.Workflow workflow in WexflowServer.WexflowEngine.Workflows)
                    {
                        if (workflow.Id == workflowId)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    await WriteTrue(context);
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Checks if a cron expression is valid.
        /// </summary>
        private void IsCronExpressionValid()
        {
            _endpoints.MapGet(Root + "isCronExpressionValid", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    string expression = context.Request.Query["e"].ToString();
                    bool res = WexflowEngine.IsCronExpressionValid(expression);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                else
                {
                    await Unauthorized(context);
                }

            });
        }

        /// <summary>
        /// Checks if a period is valid.
        /// </summary>
        private void IsPeriodValid()
        {
            _endpoints.MapGet(Root + "isPeriodValid/{period}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    bool res = TimeSpan.TryParse(context.Request.RouteValues["period"].ToString(), out TimeSpan ts);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                else
                {
                    await Unauthorized(context);
                }

            });
        }


        /// <summary>
        /// Checks if the XML of a workflow is valid.
        /// </summary>
        private void IsXmlWorkflowValid()
        {
            _endpoints.MapPost(Root + "isXmlWorkflowValid", async context =>
            {
                try
                {
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        string json = GetBody(context);
                        JObject o = JObject.Parse(json);
                        string xml = o.Value<string>("xml");
                        XDocument xdoc = XDocument.Parse(xml);

                        _ = new Core.Workflow(
                                 WexflowServer.WexflowEngine
                              , 1
                              , new Dictionary<Guid, Core.Workflow>()
                              , "-1"
                              , xdoc.ToString()
                              , WexflowServer.WexflowEngine.TempFolder
                              , WexflowServer.WexflowEngine.TasksFolder
                              , WexflowServer.WexflowEngine.ApprovalFolder
                              , WexflowServer.WexflowEngine.XsdPath
                              , WexflowServer.WexflowEngine.Database
                              , WexflowServer.WexflowEngine.GlobalVariables
                            );

                        await WriteTrue(context);
                    }
                    else
                    {
                        await Unauthorized(context);
                    }
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Saves a workflow from XML.
        /// </summary>
        private void SaveXmlWorkflow()
        {
            _endpoints.MapPost(Root + "saveXml", async context =>
            {
                try
                {
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    string json = GetBody(context);
                    bool res = false;

                    JObject o = JObject.Parse(json);
                    int workflowId = int.Parse((string)o.SelectToken("workflowId"));
                    string path = (string)o.SelectToken("filePath");
                    string xml = (string)o.SelectToken("xml");

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            string id = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xml, true);
                            if (id == "-1")
                            {
                                res = false;
                            }
                            else
                            {
                                res = true;
                            }
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            Core.Workflow workflow = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(w => w.Id == workflowId);

                            if (workflow != null)
                            {
                                string workflowDbId = workflow.DbId;
                                bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                                if (check)
                                {
                                    string id = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xml, true);
                                    if (id == "-1")
                                    {
                                        res = false;
                                    }
                                    else
                                    {
                                        res = true;
                                    }
                                }
                            }
                            else
                            {
                                string id = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xml, true);
                                if (id == "-1")
                                {
                                    res = false;
                                }
                                else
                                {
                                    res = true;
                                }
                            }
                        }
                    }

                    if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                    {
                        if (res)
                        {
                            if (string.IsNullOrEmpty(path))
                            {
                                path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, "Workflow_" + workflowId.ToString() + ".xml");
                                WexflowServer.WexflowEngine.GetWorkflow(workflowId).FilePath = path;
                            }
                            XDocument xdoc = XDocument.Parse(xml);
                            xdoc.Save(path);
                        }
                    }
                    else
                    {
                        path = null;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new SaveResult { FilePath = path, Result = res }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new SaveResult { FilePath = string.Empty, Result = false }));
                }
            });
        }

        private Core.Workflow GetWorkflowRecursive(int workflowId)
        {
            Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(workflowId);
            if (wf != null)
            {
                return wf;
            }
            else
            {
                Thread.Sleep(500);
                return GetWorkflowRecursive(workflowId);
            }
        }

        //private static string CleanupXml(string xml)
        //{
        //    char[] trimChars = new char[] { '\r', '\n', '"', '\'' };
        //    return xml
        //        .TrimStart(trimChars)
        //        .TrimEnd(trimChars)
        //        .Replace("\\r", string.Empty)
        //        .Replace("\\n", string.Empty)
        //        .Replace("\\t", string.Empty)
        //        .Replace("\\\"", "\"")
        //        .Replace("\\\\", "\\");
        //}

        private static string DecodeBase64(string str)
        {
            byte[] data = Convert.FromBase64String(str);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        private static Auth GetAuth(HttpRequest request)
        {
            string auth = request.Headers["Authorization"].First();
            auth = auth.Replace("Basic ", string.Empty);
            auth = DecodeBase64(auth);
            string[] authParts = auth.Split(':');
            string username = authParts[0];
            string password = authParts[1];
            return new Auth { Username = username, Password = password };
        }

        private static XElement JsonNodeToXmlNode(JToken node)
        {
            int? ifId = (int?)node.SelectToken("IfId");
            int? whileId = (int?)node.SelectToken("WhileId");
            int? switchId = (int?)node.SelectToken("SwitchId");
            if (ifId != null)
            {
                XElement xif = new(xn + "If", new XAttribute("id", (int)node.SelectToken("Id")),
                    new XAttribute("parent", (int)node.SelectToken("ParentId")),
                    new XAttribute("if", ifId));

                XElement xdo = new(xn + "Do");
                JArray doNodes = (JArray)node.SelectToken("DoNodes");
                if (doNodes != null)
                {
                    foreach (JToken doNode in doNodes)
                    {
                        int taskId = (int)doNode.SelectToken("Id");
                        int parentId = (int)doNode.SelectToken("ParentId");
                        xdo.Add(new XElement(xn + "Task", new XAttribute("id", taskId),
                            new XElement(xn + "Parent", new XAttribute("id", parentId))));
                    }
                }
                xif.Add(xdo);

                XElement xelse = new(xn + "Else");
                JToken elseNodesToken = node.SelectToken("ElseNodes");
                if (elseNodesToken != null && elseNodesToken.HasValues)
                {
                    JArray elseNodes = (JArray)elseNodesToken;
                    foreach (JToken elseNode in elseNodes)
                    {
                        int taskId = (int)elseNode.SelectToken("Id");
                        int parentId = (int)elseNode.SelectToken("ParentId");
                        xelse.Add(new XElement(xn + "Task", new XAttribute("id", taskId),
                            new XElement(xn + "Parent", new XAttribute("id", parentId))));
                    }

                    if (elseNodes.Count > 0) // Fix
                    {
                        xif.Add(xelse);
                    }
                }

                return xif;
            }
            else if (whileId != null)
            {
                XElement xwhile = new(xn + "While", new XAttribute("id", (int)node.SelectToken("Id")),
                    new XAttribute("parent", (int)node.SelectToken("ParentId")),
                    new XAttribute("while", whileId));

                JArray doNodes = (JArray)node.SelectToken("Nodes");
                if (doNodes != null)
                {
                    foreach (JToken doNode in doNodes)
                    {
                        int taskId = (int)doNode.SelectToken("Id");
                        int parentId = (int)doNode.SelectToken("ParentId");
                        xwhile.Add(new XElement(xn + "Task", new XAttribute("id", taskId),
                            new XElement(xn + "Parent", new XAttribute("id", parentId))));
                    }
                }

                return xwhile;
            }
            else if (switchId != null)
            {
                XElement xswitch = new(xn + "Switch", new XAttribute("id", (int)node.SelectToken("Id")),
                    new XAttribute("parent", (int)node.SelectToken("ParentId")),
                    new XAttribute("switch", switchId));

                JArray cases = (JArray)node.SelectToken("Cases");
                foreach (JToken @case in cases)
                {
                    string value = (string)@case.SelectToken("Value");

                    XElement xcase = new(xn + "Case", new XAttribute("value", value));

                    JArray doNodes = (JArray)@case.SelectToken("Nodes");
                    if (doNodes != null)
                    {
                        foreach (JToken doNode in doNodes)
                        {
                            int taskId = (int)doNode.SelectToken("Id");
                            int parentId = (int)doNode.SelectToken("ParentId");
                            xcase.Add(new XElement(xn + "Task", new XAttribute("id", taskId),
                                new XElement(xn + "Parent", new XAttribute("id", parentId))));
                        }

                    }

                    xswitch.Add(xcase);
                }

                JArray @default = (JArray)node.SelectToken("Default");
                if (@default != null && @default.Count > 0)
                {
                    XElement xdefault = new(xn + "Default");

                    foreach (JToken doNode in @default)
                    {
                        int taskId = (int)doNode.SelectToken("Id");
                        int parentId = (int)doNode.SelectToken("ParentId");
                        xdefault.Add(new XElement(xn + "Task", new XAttribute("id", taskId),
                            new XElement(xn + "Parent", new XAttribute("id", parentId))));
                    }

                    xswitch.Add(xdefault);
                }

                return xswitch;
            }
            else
            {
                int taskId = (int)node.SelectToken("Id");
                int parentId = (int)node.SelectToken("ParentId");
                XElement xtask = new(xn + "Task", new XAttribute("id", taskId),
                    new XElement(xn + "Parent", new XAttribute("id", parentId)));
                return xtask;
            }
        }

        private static XElement GetExecutionGraph(JToken eg)
        {
            if (eg != null && eg.Any())
            {
                XElement xeg = new(xn + "ExecutionGraph");
                JArray nodes = (JArray)eg.SelectToken("Nodes");
                if (nodes != null)
                {
                    foreach (JToken node in nodes)
                    {
                        xeg.Add(JsonNodeToXmlNode(node));
                    }
                }

                // OnSuccess
                JToken onSuccess = eg.SelectToken("OnSuccess");
                if (onSuccess != null && onSuccess.Any())
                {
                    XElement xEvent = new(xn + "OnSuccess");
                    JArray doNodes = (JArray)onSuccess.SelectToken("Nodes");
                    foreach (JToken doNode in doNodes)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                // OnWarning
                JToken onWarning = eg.SelectToken("OnWarning");
                if (onWarning != null && onWarning.Any())
                {
                    XElement xEvent = new(xn + "OnWarning");
                    JArray doNodes = (JArray)onWarning.SelectToken("Nodes");
                    foreach (JToken doNode in doNodes)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                // OnError
                JToken onError = eg.SelectToken("OnError");
                if (onError != null && onError.Any())
                {
                    XElement xEvent = new(xn + "OnError");
                    JArray doNodes = (JArray)onError.SelectToken("Nodes");
                    foreach (JToken doNode in doNodes)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                // OnRejected
                JToken onRejected = eg.SelectToken("OnRejected");
                if (onRejected != null && onRejected.Any())
                {
                    XElement xEvent = new(xn + "OnRejected");
                    JArray doNodes = (JArray)onRejected.SelectToken("Nodes");
                    foreach (JToken doNode in doNodes)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                return xeg;
            }

            return null;
        }

        private static SaveResult SaveJsonWorkflow(Core.Db.User user, string json)
        {
            JObject o = JObject.Parse(json);
            JToken wi = o.SelectToken("WorkflowInfo");
            int currentWorkflowId = (int)wi.SelectToken("Id");
            bool isNew = !WexflowServer.WexflowEngine.Workflows.Any(w => w.Id == currentWorkflowId);
            string path = string.Empty;

            if (isNew)
            {
                XDocument xdoc = new();

                int workflowId = (int)wi.SelectToken("Id");
                string workflowName = (string)wi.SelectToken("Name");
                LaunchType workflowLaunchType = (LaunchType)(int)wi.SelectToken("LaunchType");
                string p = (string)wi.SelectToken("Period");
                TimeSpan workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                string cronExpression = (string)wi.SelectToken("CronExpression");

                if (workflowLaunchType == LaunchType.Cron && !WexflowEngine.IsCronExpressionValid(cronExpression))
                {
                    throw new Exception("The cron expression '" + cronExpression + "' is not valid.");
                }

                bool isWorkflowEnabled = (bool)wi.SelectToken("IsEnabled");
                bool isWorkflowApproval = (bool)wi.SelectToken("IsApproval");
                bool enableParallelJobs = (bool)wi.SelectToken("EnableParallelJobs");
                string workflowDesc = (string)wi.SelectToken("Description");

                // Local variables
                XElement xLocalVariables = new(xn + "LocalVariables");
                JToken variables = wi.SelectToken("LocalVariables");
                foreach (JToken variable in variables)
                {
                    string key = (string)variable.SelectToken("Key");
                    string value = (string)variable.SelectToken("Value");

                    XElement xVariable = new(xn + "Variable"
                            , new XAttribute("name", key)
                            , new XAttribute("value", value)
                    );

                    xLocalVariables.Add(xVariable);
                }

                // tasks
                XElement xtasks = new(xn + "Tasks");
                JToken tasks = o.SelectToken("Tasks");
                foreach (JToken task in tasks)
                {
                    int taskId = (int)task.SelectToken("Id");
                    string taskName = (string)task.SelectToken("Name");
                    string taskDesc = (string)task.SelectToken("Description");
                    bool isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                    XElement xtask = new(xn + "Task"
                        , new XAttribute("id", taskId)
                        , new XAttribute("name", taskName)
                        , new XAttribute("description", taskDesc)
                        , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                    );

                    JToken settings = task.SelectToken("Settings");
                    foreach (JToken setting in settings)
                    {
                        string settingName = (string)setting.SelectToken("Name");
                        string settingValue = (string)setting.SelectToken("Value");

                        XElement xsetting = new(xn + "Setting"
                            , new XAttribute("name", settingName)
                        );

                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            xsetting.SetAttributeValue("value", settingValue);
                        }

                        if (settingName == "selectFiles" || settingName == "selectAttachments")
                        {
                            if (!string.IsNullOrEmpty(settingValue))
                            {
                                xsetting.SetAttributeValue("value", settingValue);
                            }
                        }
                        else
                        {
                            xsetting.SetAttributeValue("value", settingValue);
                        }

                        JToken attributes = setting.SelectToken("Attributes");
                        foreach (JToken attribute in attributes)
                        {
                            string attributeName = (string)attribute.SelectToken("Name");
                            string attributeValue = (string)attribute.SelectToken("Value");
                            xsetting.SetAttributeValue(attributeName, attributeValue);
                        }

                        if (settingName == "selectFiles" || settingName == "selectAttachments" || settingName != "selectFiles" && settingName != "selectAttachments" && !string.IsNullOrEmpty(settingValue))
                        {
                            xtask.Add(xsetting);
                        }
                    }

                    xtasks.Add(xtask);
                }

                // root
                XElement xwf = new(xn + "Workflow"
                    , new XAttribute("id", workflowId)
                    , new XAttribute("name", workflowName)
                    , new XAttribute("description", workflowDesc)
                    , new XElement(xn + "Settings"
                        , new XElement(xn + "Setting"
                            , new XAttribute("name", "launchType")
                            , new XAttribute("value", workflowLaunchType.ToString().ToLower()))
                        , new XElement(xn + "Setting"
                            , new XAttribute("name", "enabled")
                            , new XAttribute("value", isWorkflowEnabled.ToString().ToLower()))
                        , new XElement(xn + "Setting"
                        , new XAttribute("name", "approval")
                        , new XAttribute("value", isWorkflowApproval.ToString().ToLower()))
                        , new XElement(xn + "Setting"
                        , new XAttribute("name", "enableParallelJobs")
                        , new XAttribute("value", enableParallelJobs.ToString().ToLower()))
                    //, new XElement(xn + "Setting"
                    //    , new XAttribute("name", "period")
                    //    , new XAttribute("value", workflowPeriod.ToString(@"dd\.hh\:mm\:ss")))
                    //, new XElement(xn + "Setting"
                    //    , new XAttribute("name", "cronExpression")
                    //    , new XAttribute("value", cronExpression))
                    )
                    , xLocalVariables
                    , xtasks
                );

                if (workflowLaunchType == LaunchType.Periodic)
                {
                    xwf.Element(xn + "Settings").Add(
                         new XElement(xn + "Setting"
                            , new XAttribute("name", "period")
                            , new XAttribute("value", workflowPeriod.ToString(@"dd\.hh\:mm\:ss")))
                        );
                }

                if (workflowLaunchType == LaunchType.Cron)
                {
                    xwf.Element(xn + "Settings").Add(
                         new XElement(xn + "Setting"
                            , new XAttribute("name", "cronExpression")
                            , new XAttribute("value", cronExpression))
                        );
                }

                // Execution graph
                JToken eg = o.SelectToken("ExecutionGraph");
                XElement xeg = GetExecutionGraph(eg);
                if (xeg != null)
                {
                    xwf.Add(xeg);
                }

                xdoc.Add(xwf);
                string id = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);

                if (id == "-1")
                {
                    return new SaveResult { FilePath = path, Result = false };
                }

                if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                {
                    path = (string)wi.SelectToken("FilePath");
                    if (string.IsNullOrEmpty(path))
                    {
                        path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, "Workflow_" + workflowId.ToString() + ".xml");
                        WexflowServer.WexflowEngine.GetWorkflow(workflowId).FilePath = path;
                    }
                    xdoc.Save(path);
                }
                else
                {
                    path = null;
                }
            }
            else
            {
                Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(currentWorkflowId);
                if (wf != null)
                {
                    XDocument xdoc = wf.XDoc;

                    int workflowId = (int)wi.SelectToken("Id");
                    string workflowName = (string)wi.SelectToken("Name");
                    LaunchType workflowLaunchType = (LaunchType)(int)wi.SelectToken("LaunchType");
                    string p = (string)wi.SelectToken("Period");
                    TimeSpan workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                    string cronExpression = (string)wi.SelectToken("CronExpression");

                    if (workflowLaunchType == LaunchType.Cron &&
                        !WexflowEngine.IsCronExpressionValid(cronExpression))
                    {
                        throw new Exception("The cron expression '" + cronExpression + "' is not valid.");
                    }

                    bool isWorkflowEnabled = (bool)wi.SelectToken("IsEnabled");
                    bool isWorkflowApproval = (bool)(wi.SelectToken("IsApproval") ?? false);
                    bool enableParallelJobs = (bool)(wi.SelectToken("EnableParallelJobs") ?? true);
                    string workflowDesc = (string)wi.SelectToken("Description");

                    //if(xdoc.Root == null) throw new Exception("Root is null");
                    xdoc.Root.Attribute("id").Value = workflowId.ToString();
                    xdoc.Root.Attribute("name").Value = workflowName;
                    xdoc.Root.Attribute("description").Value = workflowDesc;

                    XElement xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                        wf.XmlNamespaceManager);
                    xwfEnabled.Attribute("value").Value = isWorkflowEnabled.ToString().ToLower();
                    XElement xwfLaunchType = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='launchType']",
                        wf.XmlNamespaceManager);
                    xwfLaunchType.Attribute("value").Value = workflowLaunchType.ToString().ToLower();

                    XElement xwfApproval = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='approval']",
                    wf.XmlNamespaceManager);
                    if (xwfApproval == null)
                    {
                        xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                            .Add(new XElement(xn + "Setting"
                                    , new XAttribute("name", "approval")
                                    , new XAttribute("value", isWorkflowApproval.ToString().ToLower())));
                    }
                    else
                    {
                        xwfApproval.Attribute("value").Value = isWorkflowApproval.ToString().ToLower();
                    }

                    XElement xwfEnableParallelJobs = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enableParallelJobs']",
                    wf.XmlNamespaceManager);
                    if (xwfEnableParallelJobs == null)
                    {
                        xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                            .Add(new XElement(xn + "Setting"
                                    , new XAttribute("name", "enableParallelJobs")
                                    , new XAttribute("value", enableParallelJobs.ToString().ToLower())));
                    }
                    else
                    {
                        xwfEnableParallelJobs.Attribute("value").Value = enableParallelJobs.ToString().ToLower();
                    }

                    XElement xwfPeriod = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='period']",
                        wf.XmlNamespaceManager);
                    if (workflowLaunchType == LaunchType.Periodic)
                    {
                        if (xwfPeriod != null)
                        {
                            xwfPeriod.Attribute("value").Value = workflowPeriod.ToString(@"dd\.hh\:mm\:ss");
                        }
                        else
                        {
                            xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                                .Add(new XElement(wf.XNamespaceWf + "Setting", new XAttribute("name", "period"),
                                    new XAttribute("value", workflowPeriod.ToString())));
                        }
                    }
                    //else
                    //{
                    //    if (xwfPeriod != null)
                    //    {
                    //        xwfPeriod.Remove();
                    //    }
                    //}

                    XElement xwfCronExpression = xdoc.Root.XPathSelectElement(
                        "wf:Settings/wf:Setting[@name='cronExpression']",
                        wf.XmlNamespaceManager);

                    if (workflowLaunchType == LaunchType.Cron)
                    {
                        if (xwfCronExpression != null)
                        {
                            xwfCronExpression.Attribute("value").Value = cronExpression ?? string.Empty;
                        }
                        else if (!string.IsNullOrEmpty(cronExpression))
                        {
                            xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                                .Add(new XElement(wf.XNamespaceWf + "Setting", new XAttribute("name", "cronExpression"),
                                    new XAttribute("value", cronExpression)));
                        }
                    }
                    else
                    {
                        if (xwfCronExpression != null)
                        {
                            xwfCronExpression.Attribute("value").Value = cronExpression ?? string.Empty;
                        }
                    }
                    //else
                    //{
                    //    if(xwfCronExpression != null)
                    //    {
                    //        xwfCronExpression.Remove();
                    //    }
                    //}

                    // Local variables
                    XElement xLocalVariables = xdoc.Root.Element(wf.XNamespaceWf + "LocalVariables");
                    if (xLocalVariables != null)
                    {
                        IEnumerable<XElement> allVariables = xLocalVariables.Elements(wf.XNamespaceWf + "Variable");
                        allVariables.Remove();
                    }
                    else
                    {
                        xLocalVariables = new XElement(wf.XNamespaceWf + "LocalVariables");
                        xdoc.Root.Element(wf.XNamespaceWf + "Tasks").AddBeforeSelf(xLocalVariables);
                    }

                    JToken variables = wi.SelectToken("LocalVariables");
                    foreach (JToken variable in variables)
                    {
                        string key = (string)variable.SelectToken("Key");
                        string value = (string)variable.SelectToken("Value");

                        XElement xVariable = new(wf.XNamespaceWf + "Variable"
                                , new XAttribute("name", key)
                                , new XAttribute("value", value)
                        );

                        xLocalVariables.Add(xVariable);
                    }

                    XElement xtasks = xdoc.Root.Element(wf.XNamespaceWf + "Tasks");
                    IEnumerable<XElement> alltasks = xtasks.Elements(wf.XNamespaceWf + "Task");
                    alltasks.Remove();

                    JToken tasks = o.SelectToken("Tasks");
                    foreach (JToken task in tasks)
                    {
                        int taskId = (int)task.SelectToken("Id");
                        string taskName = (string)task.SelectToken("Name");
                        string taskDesc = (string)task.SelectToken("Description");
                        bool isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                        XElement xtask = new(wf.XNamespaceWf + "Task"
                            , new XAttribute("id", taskId)
                            , new XAttribute("name", taskName)
                            , new XAttribute("description", taskDesc)
                            , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                        );

                        JToken settings = task.SelectToken("Settings");
                        foreach (JToken setting in settings)
                        {
                            string settingName = (string)setting.SelectToken("Name");
                            string settingValue = (string)setting.SelectToken("Value");

                            XElement xsetting = new(wf.XNamespaceWf + "Setting"
                                , new XAttribute("name", settingName)
                            );

                            if (settingName == "selectFiles" || settingName == "selectAttachments")
                            {
                                if (!string.IsNullOrEmpty(settingValue))
                                {
                                    xsetting.SetAttributeValue("value", settingValue);
                                }
                            }
                            else
                            {
                                xsetting.SetAttributeValue("value", settingValue);
                            }

                            JToken attributes = setting.SelectToken("Attributes");
                            foreach (JToken attribute in attributes)
                            {
                                string attributeName = (string)attribute.SelectToken("Name");
                                string attributeValue = (string)attribute.SelectToken("Value");
                                xsetting.SetAttributeValue(attributeName, attributeValue);
                            }

                            if (settingName == "selectFiles" || settingName == "selectAttachments" || settingName != "selectFiles" && settingName != "selectAttachments" && !string.IsNullOrEmpty(settingValue))
                            {
                                xtask.Add(xsetting);
                            }
                        }

                        xtasks.Add(xtask);
                    }

                    // ExecutionGraph
                    XElement xExecutionGraph = xdoc.Root.XPathSelectElement(
                        "wf:ExecutionGraph",
                        wf.XmlNamespaceManager);

                    xExecutionGraph?.Remove();

                    JToken eg = o.SelectToken("ExecutionGraph");
                    XElement xeg = GetExecutionGraph(eg);
                    if (xeg != null)
                    {
                        xdoc.Root.Add(xeg);
                    }

                    string qid = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);
                    if (qid == "-1")
                    {
                        return new SaveResult { FilePath = path, Result = false };
                    }

                    if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                    {
                        path = (string)wi.SelectToken("FilePath");
                        if (string.IsNullOrEmpty(path))
                        {
                            path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, "Workflow_" + workflowId.ToString() + ".xml");
                            WexflowServer.WexflowEngine.GetWorkflow(workflowId).FilePath = path;
                        }
                        xdoc.Save(path);
                    }
                    else
                    {
                        path = null;
                    }
                }
            }

            return new SaveResult { FilePath = path, Result = true };
        }

        /// <summary>
        /// Saves a workflow from json.
        /// </summary>
        private void SaveWorkflow()
        {
            _endpoints.MapPost(Root + "save", async context =>
            {
                try
                {
                    string json = GetBody(context);

                    JObject o = JObject.Parse(json);
                    JToken wi = o.SelectToken("WorkflowInfo");
                    int currentWorkflowId = (int)wi.SelectToken("Id");
                    bool isNew = !WexflowServer.WexflowEngine.Workflows.Any(w => w.Id == currentWorkflowId);

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);

                    if (!user.Password.Equals(password))
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Restricted)
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Administrator && !isNew)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == currentWorkflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (!check)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    SaveResult res = SaveJsonWorkflow(user, json);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Disbles a workflow.
        /// </summary>
        private void DisableWorkflow()
        {
            _endpoints.MapPost(Root + "disable/{id}", async context =>
            {
                try
                {
                    int workflowId = int.Parse(context.Request.RouteValues["id"].ToString());
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    Core.Workflow wf = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(w => w.Id == workflowId);
                    bool res = false;

                    if (!user.Password.Equals(password))
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Restricted)
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Administrator && wf != null)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (!check)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    if (wf != null)
                    {
                        XDocument xdoc = wf.XDoc;
                        XElement xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                        wf.XmlNamespaceManager);
                        xwfEnabled.Attribute("value").Value = false.ToString().ToLower();
                        string qid = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);

                        if (qid != "-1")
                        {
                            res = true;
                        }
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                        return;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Enables a workflow.
        /// </summary>
        private void EnableWorkflow()
        {
            _endpoints.MapPost(Root + "enable/{id}", async context =>
            {
                try
                {
                    int workflowId = int.Parse(context.Request.RouteValues["id"].ToString());
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    Core.Workflow wf = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(w => w.Id == workflowId);
                    bool res = false;

                    if (!user.Password.Equals(password))
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Restricted)
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Administrator && wf != null)
                    {
                        string workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (!check)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    if (wf != null)
                    {
                        XDocument xdoc = wf.XDoc;
                        XElement xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                        wf.XmlNamespaceManager);
                        xwfEnabled.Attribute("value").Value = true.ToString().ToLower();
                        string qid = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);

                        if (qid != "-1")
                        {
                            res = true;
                        }
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                        return;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Uploads a workflow from a file.
        /// </summary>
        private void UploadWorkflow()
        {
            _endpoints.MapPost(Root + "upload", async context =>
            {
                try
                {
                    bool res = true;
                    SaveResult ressr = new() { FilePath = string.Empty, Result = false };
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    IFormFile file = context.Request.Form.Files.Single();
                    string fileName = file.FileName;
                    MemoryStream ms = new();
                    await file.CopyToAsync(ms);
                    string fileValue = Encoding.UTF8.GetString(ms.ToArray());

                    int index = fileValue.IndexOf('<');
                    if (index > 0)
                    {
                        fileValue = fileValue[index..];
                    }

                    int workflowId = -1;
                    string extension = Path.GetExtension(fileName).ToLower();
                    bool isXml = extension == ".xml";
                    if (isXml) // xml
                    {
                        XNamespace xn = "urn:wexflow-schema";
                        XDocument xdoc = XDocument.Parse(fileValue);
                        workflowId = int.Parse(xdoc.Element(xn + "Workflow").Attribute("id").Value);
                    }
                    else // json
                    {
                        JObject o = JObject.Parse(fileValue);
                        JToken wi = o.SelectToken("WorkflowInfo");
                        workflowId = (int)wi.SelectToken("Id");
                    }

                    bool isAuthorized = false;
                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            isAuthorized = true;
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowId.ToString());
                            if (check)
                            {
                                isAuthorized = true;
                            }
                        }
                    }

                    // if extension is xml then XML else JSON
                    if (isAuthorized)
                    {
                        if (isXml)
                        {
                            string id = WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, fileValue, true);
                            res = id != "-1";

                            if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                            {
                                if (res)
                                {
                                    string path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, fileName);
                                    XDocument xdoc = XDocument.Parse(fileValue);
                                    xdoc.Save(path);
                                    ressr = new SaveResult { FilePath = path, Result = true };
                                }
                            }
                            else
                            {
                                ressr = new SaveResult { FilePath = null, Result = true };
                            }
                        }
                        else
                        {
                            ressr = SaveJsonWorkflow(user, fileValue);
                            res = ressr.Result;
                        }
                    }

                    if (!res)
                    {
                        workflowId = -1;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { WorkflowId = workflowId, SaveResult = ressr }));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Deletes a workflow.
        /// </summary>
        private void DeleteWorkflow()
        {
            _endpoints.MapPost(Root + "delete", async context =>
            {
                try
                {
                    bool res = false;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    int workflowId = int.Parse(context.Request.Query["w"].ToString());
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(workflowId);

                    if (wf != null)
                    {
                        Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);

                        if (user.Password.Equals(password))
                        {
                            if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                            {
                                WexflowServer.WexflowEngine.DeleteWorkflow(wf.DbId);
                                res = true;
                            }
                            else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                            {
                                bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                                if (check)
                                {
                                    WexflowServer.WexflowEngine.DeleteWorkflow(wf.DbId);
                                    res = true;
                                }
                            }

                            await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                        }
                        else
                        {
                            await Unauthorized(context);
                        }
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                    }
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Returns the execution graph of the workflow.
        /// </summary>
        private void GetExecutionGraph()
        {
            _endpoints.MapGet(Root + "graph/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"].ToString()));
                    if (wf != null)
                    {
                        IList<Node> nodes = new List<Node>();

                        foreach (Core.ExecutionGraph.Node node in wf.ExecutionGraph.Nodes)
                        {
                            Task task = wf.Tasks.FirstOrDefault(t => t.Id == node.Id);
                            string nodeName = "Task " + node.Id + (task != null ? ": " + task.Description : "");

                            if (node is If)
                            {
                                nodeName = "If...EndIf";
                            }
                            else if (node is While)
                            {
                                nodeName = "While...EndWhile";
                            }
                            else if (node is Switch)
                            {
                                nodeName = "Switch...EndSwitch";
                            }

                            string nodeId = "n" + node.Id;
                            string parentId = "n" + node.ParentId;

                            nodes.Add(new Node(nodeId, nodeName, parentId));
                        }

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(nodes));
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns the execution graph of the workflow.
        /// </summary>
        private void GetExecutionGraphAsXml()
        {
            _endpoints.MapGet(Root + "graphXml/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string graph = "<ExecutionGraph />";

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"].ToString()));
                    if (wf != null)
                    {
                        XElement xgraph = wf.XDoc.Descendants(wf.XNamespaceWf + "ExecutionGraph").FirstOrDefault();
                        if (xgraph != null)
                        {
                            string res = MyRegex().Replace(xgraph.ToString().Replace(" xmlns=\"urn:wexflow-schema\"", string.Empty), "\t");
                            StringBuilder builder = new();
                            string[] lines = res.Split('\n');
                            for (int i = 0; i < lines.Length; i++)
                            {
                                string line = lines[i];
                                if (i < lines.Length - 1)
                                {
                                    builder.Append('\t').Append(line).Append('\n');
                                }
                                else
                                {
                                    builder.Append('\t').Append(line);
                                }
                            }
                            graph = builder.ToString();
                        }
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(graph));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns the execution graph of the workflow.
        /// </summary>
        private void GetExecutionGraphAsBlockly()
        {
            _endpoints.MapGet(Root + "graphBlockly/{id}", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string graph = "<xml />";

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Workflow wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"].ToString()));
                    if (wf != null)
                    {
                        if (wf.ExecutionGraph != null)
                        {
                            XElement xml = ExecutionGraphToBlockly(wf.ExecutionGraph);
                            if (xml != null)
                            {
                                graph = xml.ToString();
                            }
                        }
                        else
                        {
                            List<Core.ExecutionGraph.Node> nodes = new();
                            for (int i = 0; i < wf.Tasks.Length; i++)
                            {
                                Task task = wf.Tasks[i];
                                if (i == 0)
                                {
                                    nodes.Add(new Core.ExecutionGraph.Node(task.Id, -1));
                                }
                                else
                                {
                                    nodes.Add(new Core.ExecutionGraph.Node(task.Id, wf.Tasks[i - 1].Id));
                                }
                            }

                            Core.ExecutionGraph.Graph sgraph = new(nodes, null, null, null, null);
                            XElement xml = ExecutionGraphToBlockly(sgraph);
                            if (xml != null)
                            {
                                graph = xml.ToString();
                            }
                        }

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(graph));
                    }
                    else
                    {
                        await WorkflowNotFound(context);
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        private XElement ExecutionGraphToBlockly(Core.ExecutionGraph.Graph graph)
        {
            if (graph != null)
            {
                Core.ExecutionGraph.Node[] nodes = graph.Nodes;
                XElement xml = new("xml");
                Core.ExecutionGraph.Node startNode = GetStartupNode(nodes);
                int depth = 0;
                XElement block = NodeToBlockly(graph, startNode, nodes, startNode is If || startNode is While || startNode is Switch, false, ref depth);
                xml.Add(block);
                return xml;
            }

            return null;
        }

        private XElement NodeToBlockly(Core.ExecutionGraph.Graph graph, Core.ExecutionGraph.Node node, Core.ExecutionGraph.Node[] nodes, bool isFlowchart, bool isEvent, ref int depth)
        {
            XElement block = new("block");

            if (nodes.Any())
            {
                if (node is If)
                {
                    if (node is If @if)
                    {
                        block.Add(new XAttribute("type", "if"), new XElement("field", new XAttribute("name", "IF"), @if.IfId));
                        XElement @do = new("statement", new XAttribute("name", "DO"));
                        @do.Add(NodeToBlockly(graph, GetStartupNode(@if.DoNodes), @if.DoNodes, true, isEvent, ref depth));
                        block.Add(@do);
                        if (@if.ElseNodes != null && @if.ElseNodes.Length > 0)
                        {
                            XElement @else = new("statement", new XAttribute("name", "ELSE"));
                            @else.Add(NodeToBlockly(graph, GetStartupNode(@if.ElseNodes), @if.ElseNodes, true, isEvent, ref depth));
                            block.Add(@else);
                        }
                        depth = 0;
                    }
                }
                else if (node is While)
                {
                    if (node is While @while)
                    {
                        block.Add(new XAttribute("type", "while"), new XElement("field", new XAttribute("name", "WHILE"), @while.WhileId));
                        XElement @do = new("statement", new XAttribute("name", "DO"));
                        @do.Add(NodeToBlockly(graph, GetStartupNode(@while.Nodes), @while.Nodes, true, isEvent, ref depth));
                        block.Add(@do);
                        depth = 0;
                    }
                }
                else if (node is Switch)
                {
                    if (node is Switch @switch)
                    {
                        block.Add(new XAttribute("type", "switch"), new XElement("field", new XAttribute("name", "SWITCH"), @switch.SwitchId));
                        XElement @case = new("statement", new XAttribute("name", "CASE"));
                        if (@switch.Cases.Length > 0)
                        {
                            XElement xcases = SwitchCasesToBlockly(graph, @switch.Cases[0], 0, @switch.Cases.Length > 1 ? @switch.Cases[1] : null, @switch, true, isEvent, ref depth);
                            @case.Add(xcases);
                        }
                        block.Add(@case);
                        if (@switch.Default != null && @switch.Default.Length > 0)
                        {
                            XElement @default = new("statement", new XAttribute("name", "DEFAULT"));
                            @default.Add(NodeToBlockly(graph, GetStartupNode(@switch.Default), @switch.Default, true, isEvent, ref depth));
                            block.Add(@default);
                        }
                        depth = 0;
                    }
                }
                else
                {
                    block.Add(new XAttribute("type", "task"), new XElement("field", new XAttribute("name", "TASK"), node.Id));
                    if (isFlowchart || isEvent)
                    {
                        depth++;
                    }
                }

                if (isFlowchart && depth == 0)
                {
                    isFlowchart = false;
                }

                Core.ExecutionGraph.Node childNode = nodes.FirstOrDefault(n => n.ParentId == node.Id);
                if (childNode != null)
                {
                    block.Add(new XElement("next", NodeToBlockly(graph, childNode, nodes, isFlowchart, isEvent, ref depth)));
                }
                else if (childNode == null && !isFlowchart && !isEvent)
                {
                    block.Add(new XElement("next",
                        new XElement("block", new XAttribute("type", "onSuccess"), new XElement("statement", new XAttribute("name", "ON_SUCCESS"), NodeToBlockly(graph, GetStartupNode((graph.OnSuccess ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes), (graph.OnSuccess ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes, false, true, ref depth))
                            , new XElement("next",
                                new XElement("block", new XAttribute("type", "onWarning"), new XElement("statement", new XAttribute("name", "ON_WARNING"), NodeToBlockly(graph, GetStartupNode((graph.OnWarning ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes), (graph.OnWarning ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes, false, true, ref depth))
                                , new XElement("next",
                                new XElement("block", new XAttribute("type", "onError"), new XElement("statement", new XAttribute("name", "ON_ERROR"), NodeToBlockly(graph, GetStartupNode((graph.OnError ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes), (graph.OnError ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes, false, true, ref depth))
                                , new XElement("next",
                                new XElement("block", new XAttribute("type", "onRejected"), new XElement("statement", new XAttribute("name", "ON_REJECTED"), NodeToBlockly(graph, GetStartupNode((graph.OnRejected ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes), (graph.OnRejected ?? new Core.ExecutionGraph.GraphEvent(Array.Empty<Core.ExecutionGraph.Node>())).Nodes, false, true, ref depth))
                                ))))))
                            )));
                }
            }

            if (block.Attribute("type") == null)
            {
                return null;
            }

            return block;
        }

        private XElement SwitchCasesToBlockly(Core.ExecutionGraph.Graph graph, Case @case, int caseIndex, Case nextCase, Switch @switch, bool isFlowchart, bool isEvent, ref int depth)
        {
            XElement xscase = new("block", new XAttribute("type", "case"), new XElement("field", new XAttribute("name", "CASE_VALUE"), @case.Value));
            xscase.Add(new XElement("statement", new XAttribute("name", "CASE"), NodeToBlockly(graph, GetStartupNode(@case.Nodes), @case.Nodes, isFlowchart, isEvent, ref depth)));
            if (nextCase != null)
            {
                caseIndex++;
                xscase.Add(new XElement("next", SwitchCasesToBlockly(graph, nextCase, caseIndex, @switch.Cases.Length > caseIndex + 1 ? @switch.Cases[caseIndex + 1] : null, @switch, isFlowchart, isEvent, ref depth)));
            }
            return xscase;
        }

        private Core.ExecutionGraph.Node GetStartupNode(IEnumerable<Core.ExecutionGraph.Node> nodes)
        {
            return nodes.FirstOrDefault(n => n.ParentId == Core.Workflow.StartId);
        }

        /// <summary>
        /// Returns status count.
        /// </summary>
        private void GetStatusCount()
        {
            _endpoints.MapGet(Root + "statusCount", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    Core.Db.StatusCount statusCount = WexflowServer.WexflowEngine.GetStatusCount();
                    StatusCount sc = new()
                    {
                        PendingCount = statusCount.PendingCount,
                        RunningCount = statusCount.RunningCount,
                        DoneCount = statusCount.DoneCount,
                        FailedCount = statusCount.FailedCount,
                        WarningCount = statusCount.WarningCount,
                        DisabledCount = statusCount.DisabledCount,
                        RejectedCount = statusCount.RejectedCount,
                        StoppedCount = statusCount.StoppedCount
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(sc));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns a user from his username.
        /// </summary>
        private void GetUser()
        {
            _endpoints.MapGet(Root + "user", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string username = context.Request.Query["username"].ToString();

                Core.Db.User othuser = WexflowServer.WexflowEngine.GetUser(qusername);

                if (othuser.Password.Equals(qpassword))
                {
                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    string dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

                    if (user != null)
                    {
                        User u = new()
                        {
                            Id = user.GetDbId(),
                            Username = user.Username,
                            Password = user.Password,
                            UserProfile = (UserProfile)(int)user.UserProfile,
                            Email = user.Email,
                            CreatedOn = user.CreatedOn.ToString(dateTimeFormat),
                            ModifiedOn = user.ModifiedOn.ToString(dateTimeFormat)
                        };

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(u));
                    }
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Searches for users.
        /// </summary>
        private void SearchUsers()
        {
            _endpoints.MapGet(Root + "searchUsers", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;
                string keyword = context.Request.Query["keyword"].ToString();
                int uo = int.Parse(context.Request.Query["uo"].ToString());

                User[] q = Array.Empty<User>();
                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.User[] users = WexflowServer.WexflowEngine.GetUsers(keyword, (UserOrderBy)uo);

                    string dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

                    q = users.Select(u => new User
                    {
                        Id = u.GetDbId(),
                        Username = u.Username,
                        Password = u.Password,
                        UserProfile = (UserProfile)(int)u.UserProfile,
                        Email = u.Email,
                        CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                        ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                    }).ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(q));
            });
        }

        /// <summary>
        /// Returns non restricted users.
        /// </summary>
        private void GetNonRestrictedUsers()
        {
            _endpoints.MapGet(Root + "nonRestrictedUsers", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                User[] q = Array.Empty<User>();
                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.User[] users = WexflowServer.WexflowEngine.GetNonRestrictedUsers();

                    string dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

                    q = users.Select(u => new User
                    {
                        Id = u.GetDbId(),
                        Username = u.Username,
                        Password = u.Password,
                        UserProfile = (UserProfile)(int)u.UserProfile,
                        Email = u.Email,
                        CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                        ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                    }).ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(q));
            });
        }

        /// <summary>
        /// Searches for administrators.
        /// </summary>
        private void SearchAdministrators()
        {
            _endpoints.MapGet(Root + "searchAdmins", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string keyword = context.Request.Query["keyword"].ToString();
                int uo = int.Parse(context.Request.Query["uo"].ToString());

                User[] q = Array.Empty<User>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.User[] users = WexflowServer.WexflowEngine.GetAdministrators(keyword, (UserOrderBy)uo);
                    string dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

                    q = users.Select(u => new User
                    {
                        Id = u.GetDbId(),
                        Username = u.Username,
                        Password = u.Password,
                        UserProfile = (UserProfile)(int)u.UserProfile,
                        Email = u.Email,
                        CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                        ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                    }).ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(q));

            });
        }

        /// <summary>
        /// Saves user workflow relations.
        /// </summary>
        private void SaveUserWorkflows()
        {
            _endpoints.MapPost(Root + "saveUserWorkflows", async context =>
            {
                try
                {
                    Auth auth = GetAuth(context.Request);
                    string qusername = auth.Username;
                    string qpassword = auth.Password;

                    string json = GetBody(context);

                    bool res = false;
                    JObject o = JObject.Parse(json);

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);

                    if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        string userId = o.Value<string>("UserId");
                        JArray jArray = o.Value<JArray>("UserWorkflows");
                        WexflowServer.WexflowEngine.DeleteUserWorkflowRelations(userId);
                        foreach (JObject item in jArray.Cast<JObject>())
                        {
                            string workflowId = item.Value<string>("WorkflowId");
                            WexflowServer.WexflowEngine.InsertUserWorkflowRelation(userId, workflowId);
                        }

                        res = true;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });

        }

        /// <summary>
        /// Returns user workflows.
        /// </summary>
        private void GetUserWorkflows()
        {
            _endpoints.MapGet(Root + "userWorkflows", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string userId = context.Request.Query["u"].ToString();

                WorkflowInfo[] res = Array.Empty<WorkflowInfo>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    try
                    {
                        Core.Workflow[] workflows = WexflowServer.WexflowEngine.GetUserWorkflows(userId);
                        res = workflows
                            .ToList()
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                           , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                           , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])))
                            .ToArray();
                    }
                    catch (Exception e)
                    {
                        await Error(context, e);
                        return;
                    }
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Inserts a user.
        /// </summary>
        private void InsertUser()
        {
            _endpoints.MapPost(Root + "insertUser", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string username = context.Request.Query["username"].ToString();
                string password = context.Request.Query["password"].ToString();
                int userProfile = int.Parse(context.Request.Query["up"].ToString());
                string email = context.Request.Query["email"].ToString();

                try
                {
                    bool res = false;
                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        WexflowServer.WexflowEngine.InsertUser(username, password, (Core.Db.UserProfile)userProfile, email);
                        res = true;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        private void UpdateUser()
        {
            _endpoints.MapPost(Root + "updateUser", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string userId = context.Request.Query["userId"].ToString();
                string username = context.Request.Query["username"].ToString();
                string password = context.Request.Query["password"].ToString();
                int userProfile = int.Parse(context.Request.Query["up"].ToString());
                string email = context.Request.Query["email"].ToString();

                try
                {
                    bool res = false;
                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        WexflowServer.WexflowEngine.UpdateUser(userId, username, password, (Core.Db.UserProfile)userProfile, email);
                        res = true;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }

            });
        }

        /// <summary>
        /// Updates the username, the email and the user profile of a user.
        /// </summary>
        private void UpdateUsernameAndEmailAndUserProfile()
        {
            _endpoints.MapPost(Root + "updateUsernameAndEmailAndUserProfile", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string userId = context.Request.Query["userId"].ToString();
                string username = context.Request.Query["username"].ToString();
                string email = context.Request.Query["email"].ToString();
                int up = int.Parse(context.Request.Query["up"].ToString());

                try
                {
                    bool res = false;
                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        WexflowServer.WexflowEngine.UpdateUsernameAndEmailAndUserProfile(userId, username, email, up);
                        res = true;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }

            });
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        private void DeleteUser()
        {
            _endpoints.MapPost(Root + "deleteUser", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string qusername = auth.Username;
                string qpassword = auth.Password;

                string username = context.Request.Query["username"].ToString();
                string password = context.Request.Query["password"].ToString();

                try
                {
                    bool res = false;
                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        WexflowServer.WexflowEngine.DeleteUser(username, password);
                        res = true;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }

            });
        }

        /// <summary>
        /// Resets a password.
        /// </summary>
        private void ResetPassword()
        {
            _endpoints.MapPost(Root + "resetPassword", async context =>
            {
                string username = context.Request.Query["u"].ToString();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        string newPassword = "wexflow" + GenerateRandomNumber();
                        string newPasswordHash = Db.GetMd5(newPassword);

                        // Send email
                        string subject = "Wexflow - Password reset of user " + username;
                        string body = "Your new password is: " + newPassword;

                        string host = WexflowServer.Config["Smtp.Host"];
                        int port = int.Parse(WexflowServer.Config["Smtp.Port"]);
                        bool enableSsl = bool.Parse(WexflowServer.Config["Smtp.EnableSsl"]);
                        string smtpUser = WexflowServer.Config["Smtp.User"];
                        string smtpPassword = WexflowServer.Config["Smtp.Password"];
                        string from = WexflowServer.Config["Smtp.From"];

                        Send(host, port, enableSsl, smtpUser, smtpPassword, user.Email, from, subject, body);

                        // Update password
                        WexflowServer.WexflowEngine.UpdatePassword(username, newPasswordHash);

                        await WriteTrue(context);
                        return;
                    }
                    catch (Exception e)
                    {
                        await Error(context, e);
                        return;
                    }
                }

                await WriteFalse(context);
            });
        }

        /// <summary>
        /// Generates a random number of 4 digits.
        /// </summary>
        /// <returns></returns>
        private int GenerateRandomNumber()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new();
            return _rdm.Next(_min, _max);
        }

        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="enableSsl"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private void Send(string host, int port, bool enableSsl, string user, string password, string to, string from, string subject, string body)
        {
            SmtpClient smtp = new()
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password)
            };

            using MailMessage msg = new();
            msg.From = new MailAddress(from);
            msg.To.Add(new MailAddress(to));
            msg.Subject = subject;
            msg.Body = body;

            smtp.Send(msg);
        }

        /// <summary>
        /// Searches for history entries.
        /// </summary>
        private void SearchHistoryEntriesByPageOrderBy()
        {
            _endpoints.MapGet(Root + "searchHistoryEntriesByPageOrderBy", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    string keyword = context.Request.Query["s"].ToString();
                    double from = double.Parse(context.Request.Query["from"].ToString());
                    double to = double.Parse(context.Request.Query["to"].ToString());
                    int page = int.Parse(context.Request.Query["page"].ToString());
                    int entriesCount = int.Parse(context.Request.Query["entriesCount"].ToString());
                    int heo = int.Parse(context.Request.Query["heo"].ToString());

                    DateTime baseDate = new(1970, 1, 1);
                    DateTime fromDate = baseDate.AddMilliseconds(from);
                    DateTime toDate = baseDate.AddMilliseconds(to);

                    Core.Db.HistoryEntry[] entries = WexflowServer.WexflowEngine.GetHistoryEntries(keyword, fromDate, toDate, page,
                        entriesCount, (EntryOrderBy)heo);

                    Contracts.HistoryEntry[] q = entries.Select(e =>
                       new Contracts.HistoryEntry
                       {
                           Id = e.GetDbId(),
                           WorkflowId = e.WorkflowId,
                           Name = e.Name,
                           LaunchType = (LaunchType)(int)e.LaunchType,
                           Description = e.Description,
                           Status = (Contracts.Status)(int)e.Status,
                           //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                           StatusDate = e.StatusDate.ToString(WexflowServer.Config["DateTimeFormat"])
                       }).ToArray();

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(q));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Searches for entries.
        /// </summary>
        private void SearchEntriesByPageOrderBy()
        {
            _endpoints.MapGet(Root + "searchEntriesByPageOrderBy", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    string keyword = context.Request.Query["s"].ToString();
                    double from = double.Parse(context.Request.Query["from"].ToString());
                    double to = double.Parse(context.Request.Query["to"].ToString());
                    int page = int.Parse(context.Request.Query["page"].ToString());
                    int entriesCount = int.Parse(context.Request.Query["entriesCount"].ToString());
                    int heo = int.Parse(context.Request.Query["heo"].ToString());

                    DateTime baseDate = new(1970, 1, 1);
                    DateTime fromDate = baseDate.AddMilliseconds(from);
                    DateTime toDate = baseDate.AddMilliseconds(to);

                    Core.Db.Entry[] entries = WexflowServer.WexflowEngine.GetEntries(keyword, fromDate, toDate, page, entriesCount, (EntryOrderBy)heo);

                    Contracts.Entry[] q = entries.Select(e =>
                        new Contracts.Entry
                        {
                            Id = e.GetDbId(),
                            WorkflowId = e.WorkflowId,
                            Name = e.Name,
                            LaunchType = (LaunchType)(int)e.LaunchType,
                            Description = e.Description,
                            Status = (Contracts.Status)(int)e.Status,
                            //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                            StatusDate = e.StatusDate.ToString(WexflowServer.Config["DateTimeFormat"])
                        }).ToArray();

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(q));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns history entries count by keyword and date filter.
        /// </summary>
        private void GetHistoryEntriesCountByDate()
        {
            _endpoints.MapGet(Root + "historyEntriesCountByDate", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    string keyword = context.Request.Query["s"].ToString();
                    double from = double.Parse(context.Request.Query["from"].ToString());
                    double to = double.Parse(context.Request.Query["to"].ToString());

                    DateTime baseDate = new(1970, 1, 1);
                    DateTime fromDate = baseDate.AddMilliseconds(from);
                    DateTime toDate = baseDate.AddMilliseconds(to);
                    long count = WexflowServer.WexflowEngine.GetHistoryEntriesCount(keyword, fromDate, toDate);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(count));
                }
                else
                {
                    await Unauthorized(context);
                }

            });
        }

        /// <summary>
        /// Returns entries count by keyword and date filter.
        /// </summary>
        private void GetEntriesCountByDate()
        {
            _endpoints.MapGet(Root + "entriesCountByDate", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    string keyword = context.Request.Query["s"].ToString();
                    double from = double.Parse(context.Request.Query["from"].ToString());
                    double to = double.Parse(context.Request.Query["to"].ToString());

                    DateTime baseDate = new(1970, 1, 1);
                    DateTime fromDate = baseDate.AddMilliseconds(from);
                    DateTime toDate = baseDate.AddMilliseconds(to);
                    long count = WexflowServer.WexflowEngine.GetEntriesCount(keyword, fromDate, toDate);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(count));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns history entry min date.
        /// </summary>
        private void GetHistoryEntryStatusDateMin()
        {
            _endpoints.MapGet(Root + "historyEntryStatusDateMin", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    DateTime date = WexflowServer.WexflowEngine.GetHistoryEntryStatusDateMin();
                    DateTime baseDate = new(1970, 1, 1);
                    double d = (date - baseDate).TotalMilliseconds;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(d));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns history entry max date.
        /// </summary>
        private void GetHistoryEntryStatusDateMax()
        {
            _endpoints.MapGet(Root + "historyEntryStatusDateMax", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    DateTime date = WexflowServer.WexflowEngine.GetHistoryEntryStatusDateMax();
                    DateTime baseDate = new(1970, 1, 1);
                    double d = (date - baseDate).TotalMilliseconds;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(d));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns entry min date.
        /// </summary>
        private void GetEntryStatusDateMin()
        {
            _endpoints.MapGet(Root + "entryStatusDateMin", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    DateTime date = WexflowServer.WexflowEngine.GetEntryStatusDateMin();
                    DateTime baseDate = new(1970, 1, 1);
                    double d = (date - baseDate).TotalMilliseconds;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(d));
                }
                else
                {
                    await Unauthorized(context);
                }
            });
        }

        /// <summary>
        /// Returns entry max date.
        /// </summary>
        private void GetEntryStatusDateMax()
        {
            _endpoints.MapGet(Root + "entryStatusDateMax", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    DateTime date = WexflowServer.WexflowEngine.GetEntryStatusDateMax();
                    DateTime baseDate = new(1970, 1, 1);
                    double d = (date - baseDate).TotalMilliseconds;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(d));
                }
                else
                {
                    await Unauthorized(context);
                }

            });
        }

        /// <summary>
        /// Deletes workflows.
        /// </summary>
        private void DeleteWorkflows()
        {
            _endpoints.MapPost(Root + "deleteWorkflows", async context =>
            {
                try
                {
                    string json = GetBody(context);

                    bool res = false;

                    JObject o = JObject.Parse(json);
                    string[] workflowDbIds = JsonConvert.DeserializeObject<string[]>(((JArray)o.SelectToken("WorkflowsToDelete")).ToString());

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            res = WexflowServer.WexflowEngine.DeleteWorkflows(workflowDbIds);
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            bool tres = true;
                            foreach (string id in workflowDbIds)
                            {
                                bool check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), id);
                                if (check)
                                {
                                    try
                                    {
                                        WexflowServer.WexflowEngine.DeleteWorkflow(id);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        tres &= false;
                                    }

                                }
                            }
                            res = tres;
                        }

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                    }
                    else
                    {
                        await Unauthorized(context);
                    }
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });

        }

        /// <summary>
        /// Returns entry logs.
        /// </summary>
        private void GetEntryLogs()
        {
            _endpoints.MapGet(Root + "entryLogs", async context =>
            {
                try
                {
                    string entryId = context.Request.Query["id"].ToString();
                    string res = string.Empty;
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        res = WexflowServer.WexflowEngine.GetEntryLogs(entryId);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });

        }

        /// <summary>
        /// Returns history entry logs.
        /// </summary>
        private void GetHistoryEntryLogs()
        {
            _endpoints.MapGet(Root + "historyEntryLogs", async context =>
            {
                try
                {
                    string entryId = context.Request.Query["id"].ToString();
                    string res = string.Empty;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        res = WexflowServer.WexflowEngine.GetHistoryEntryLogs(entryId);
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                    }
                    else
                    {
                        await Unauthorized(context);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await WriteEmpty(context);
                }
            });

        }

        /// <summary>
        /// Returns human readable file size.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>File size.</returns>
        private string GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = new FileInfo(filePath).Length;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }

                // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                // show a single decimal place, and no space.
                string result = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0.##} {1}", len, sizes[order]);

                return result;
            }

            return string.Empty;
        }

        /// <summary>
        /// Uploads a version.
        /// </summary>
        private void UploadVersion()
        {
            _endpoints.MapPost(Root + "uploadVersion", async context =>
            {
                try
                {
                    SaveResult ressr = new() { FilePath = string.Empty, FileName = string.Empty, Result = false };
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    IFormFile file = context.Request.Form.Files.Single();
                    string fileName = file.FileName;
                    MemoryStream ms = new();
                    await file.CopyToAsync(ms);

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string recordId = context.Request.Query["r"].ToString();
                        string guid = Guid.NewGuid().ToString();
                        string dir = Path.Combine(WexflowServer.WexflowEngine.RecordsTempFolder, WexflowServer.WexflowEngine.DbFolderName, recordId, guid);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        string filePath = Path.Combine(dir, fileName);
                        File.WriteAllBytes(filePath, ms.ToArray());
                        ressr.Result = true;
                        ressr.FilePath = filePath;
                        ressr.FileName = Path.GetFileName(filePath);
                        ressr.FileSize = GetFileSize(filePath);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ressr));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new SaveResult { FilePath = string.Empty, Result = false }));
                }
            });
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        private void DownloadFile()
        {
            _endpoints.MapGet(Root + "downloadFile", async context =>
            {
                try
                {
                    string path = context.Request.Query["p"].ToString();
                    string fileName = Path.GetFileName(path);
                    context.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    await context.Response.SendFileAsync(path);
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Deletes a temp version file.
        /// </summary>
        private void DeleteTempVersionFile()
        {
            _endpoints.MapPost(Root + "deleteTempVersionFile", async context =>
            {
                try
                {
                    bool res = false;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string path = context.Request.Query["p"].ToString();

                        if (path.Contains(WexflowServer.WexflowEngine.RecordsTempFolder))
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                                res = true;

                                string parentDir = Path.GetDirectoryName(path);
                                if (WexflowServer.WexflowEngine.IsDirectoryEmpty(parentDir))
                                {
                                    Directory.Delete(parentDir);
                                    string recordTempDir = Directory.GetParent(parentDir).FullName;
                                    if (WexflowServer.WexflowEngine.IsDirectoryEmpty(recordTempDir))
                                    {
                                        Directory.Delete(recordTempDir);
                                    }
                                }
                            }
                        }
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await WriteFalse(context);
                }
            });
        }

        /// <summary>
        /// Deletes temp version files.
        /// </summary>
        private void DeleteTempVersionFiles()
        {
            _endpoints.MapPost(Root + "deleteTempVersionFiles", async context =>
            {
                try
                {
                    bool res = true;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string json = GetBody(context);
                        JObject o = JObject.Parse(json);
                        Contracts.Version[] versions = JsonConvert.DeserializeObject<Contracts.Version[]>(o.Value<JArray>("Versions").ToString());

                        foreach (Contracts.Version version in versions)
                        {
                            string path = version.FilePath;

                            try
                            {
                                if (path.Contains(WexflowServer.WexflowEngine.RecordsTempFolder))
                                {
                                    if (File.Exists(path))
                                    {
                                        File.Delete(path);

                                        string parentDir = Path.GetDirectoryName(path);
                                        if (WexflowServer.WexflowEngine.IsDirectoryEmpty(parentDir))
                                        {
                                            Directory.Delete(parentDir);
                                            string recordTempDir = Directory.GetParent(parentDir).FullName;
                                            if (WexflowServer.WexflowEngine.IsDirectoryEmpty(recordTempDir))
                                            {
                                                Directory.Delete(recordTempDir);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                res = false;
                            }
                        }
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await WriteFalse(context);
                }
            });
        }

        /// <summary>
        /// Saves a record.
        /// </summary>
        private void SaveRecord()
        {
            _endpoints.MapPost(Root + "saveRecord", async context =>
            {
                try
                {
                    bool res = false;
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string json = GetBody(context);
                        JObject o = JObject.Parse(json);

                        string id = o.Value<string>("Id");
                        string name = o.Value<string>("Name");
                        string description = o.Value<string>("Description");
                        string startDate = o.Value<string>("StartDate");
                        string endDate = o.Value<string>("EndDate");
                        string comments = o.Value<string>("Comments");
                        bool approved = o.Value<bool>("Approved");
                        string managerComments = o.Value<string>("ManagerComments");
                        string modifiedBy = o.Value<string>("ModifiedBy");
                        string modifiedOn = o.Value<string>("ModifiedOn");
                        string createdBy = o.Value<string>("CreatedBy");
                        string createdOn = o.Value<string>("CreatedOn");
                        string assignedTo = o.Value<string>("AssignedTo");
                        string assignedOn = o.Value<string>("AssignedOn");
                        Contracts.Version[] versions = JsonConvert.DeserializeObject<Contracts.Version[]>(o.Value<JArray>("Versions").ToString());

                        string dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

                        Core.Db.Record record = new()
                        {
                            Name = name,
                            Description = description,
                            StartDate = string.IsNullOrEmpty(startDate) ? null : DateTime.ParseExact(startDate, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                            EndDate = string.IsNullOrEmpty(endDate) ? null : DateTime.ParseExact(endDate, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                            Comments = comments,
                            Approved = approved,
                            ManagerComments = managerComments,
                            ModifiedBy = !string.IsNullOrEmpty(modifiedBy) ? WexflowServer.WexflowEngine.GetUser(modifiedBy).GetDbId() : null,
                            ModifiedOn = string.IsNullOrEmpty(modifiedOn) ? null : DateTime.ParseExact(modifiedOn, dateTimeFormat, CultureInfo.InvariantCulture),
                            CreatedBy = !string.IsNullOrEmpty(createdBy) ? WexflowServer.WexflowEngine.GetUser(createdBy).GetDbId() : null,
                            CreatedOn = !string.IsNullOrEmpty(createdOn) ? DateTime.ParseExact(createdOn, dateTimeFormat, CultureInfo.InvariantCulture) : DateTime.MinValue,
                            AssignedTo = !string.IsNullOrEmpty(assignedTo) ? WexflowServer.WexflowEngine.GetUser(assignedTo).GetDbId() : null,
                            AssignedOn = string.IsNullOrEmpty(assignedOn) ? null : DateTime.ParseExact(assignedOn, dateTimeFormat, CultureInfo.InvariantCulture)
                        };

                        List<Core.Db.Version> recordVersions = new();
                        foreach (Contracts.Version version in versions)
                        {
                            recordVersions.Add(new Core.Db.Version
                            {
                                RecordId = version.RecordId,
                                FilePath = version.FilePath
                            });
                        }

                        string recordId = WexflowServer.WexflowEngine.SaveRecord(id, record, recordVersions);

                        res = recordId != "-1";
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Deletes records
        /// </summary>
        private void DeleteRecords()
        {
            _endpoints.MapPost(Root + "deleteRecords", async context =>
            {
                try
                {
                    bool res = false;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string json = GetBody(context);
                        string[] recordIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
                        res = WexflowServer.WexflowEngine.DeleteRecords(recordIds);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Searches for records by keyword.
        /// </summary>
        private void SearchRecords()
        {
            _endpoints.MapGet(Root + "searchRecords", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string keyword = context.Request.Query["s"].ToString();

                Contracts.Record[] records = Array.Empty<Contracts.Record>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.Record[] recordsArray = WexflowServer.WexflowEngine.GetRecords(keyword);
                    List<Contracts.Record> recordsList = new();
                    foreach (Core.Db.Record record in recordsArray)
                    {
                        Core.Db.User createdBy = !string.IsNullOrEmpty(record.CreatedBy) ? WexflowServer.WexflowEngine.GetUserById(record.CreatedBy) : null;
                        Core.Db.User modifiedBy = !string.IsNullOrEmpty(record.ModifiedBy) ? WexflowServer.WexflowEngine.GetUserById(record.ModifiedBy) : null;
                        Core.Db.User assignedTo = !string.IsNullOrEmpty(record.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(record.AssignedTo) : null;
                        Contracts.Record r = new()
                        {
                            Id = record.GetDbId(),
                            Name = record.Name,
                            Description = record.Description,
                            StartDate = record.StartDate != null ? ((DateTime)record.StartDate).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            EndDate = record.EndDate != null ? ((DateTime)record.EndDate).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            Comments = record.Comments,
                            Approved = record.Approved,
                            ManagerComments = record.ManagerComments,
                            ModifiedBy = modifiedBy != null ? modifiedBy.Username : string.Empty,
                            ModifiedOn = record.ModifiedOn != null ? ((DateTime)record.ModifiedOn).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            CreatedBy = createdBy != null ? createdBy.Username : string.Empty,
                            CreatedOn = record.CreatedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                            AssignedTo = assignedTo != null ? assignedTo.Username : string.Empty,
                            AssignedOn = record.AssignedOn != null ? ((DateTime)record.AssignedOn).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty
                        };

                        // Approvers
                        Core.Db.Approver[] approvers = WexflowServer.WexflowEngine.GetApprovers(record.GetDbId());
                        List<Contracts.Approver> approversList = new();
                        foreach (Core.Db.Approver approver in approvers)
                        {
                            Core.Db.User approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
                            if (approverUser != null)
                            {
                                Contracts.Approver a = new()
                                {
                                    ApprovedBy = approverUser.Username,
                                    Approved = approver.Approved,
                                    ApprovedOn = approver.ApprovedOn == null ? string.Empty : approver.ApprovedOn.Value.ToString(WexflowServer.Config["DateTimeFormat"])
                                };
                                approversList.Add(a);
                            }
                        }
                        r.Approvers = approversList.ToArray();

                        // Versions
                        Core.Db.Version[] versions = WexflowServer.WexflowEngine.GetVersions(record.GetDbId());
                        List<Contracts.Version> versionsList = new();
                        foreach (Core.Db.Version version in versions)
                        {
                            Contracts.Version v = new()
                            {
                                Id = version.GetDbId(),
                                RecordId = version.RecordId,
                                FilePath = version.FilePath,
                                FileName = Path.GetFileName(version.FilePath),
                                CreatedOn = version.CreatedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                                FileSize = GetFileSize(version.FilePath)
                            };
                            versionsList.Add(v);
                        }
                        r.Versions = versionsList.ToArray();
                        recordsList.Add(r);
                    }
                    records = recordsList.ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(records));
            });
        }

        /// <summary>
        /// Retrieves records created by a user.
        /// </summary>
        private void GetRecordsCreatedBy()
        {
            _endpoints.MapGet(Root + "recordsCreatedBy", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string createdByUsername = context.Request.Query["c"].ToString();
                Core.Db.User createdBy = WexflowServer.WexflowEngine.GetUser(createdByUsername);

                Contracts.Record[] records = Array.Empty<Contracts.Record>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.Record[] recordsArray = WexflowServer.WexflowEngine.GetRecordsCreatedBy(createdBy.GetDbId());
                    List<Contracts.Record> recordsList = new();
                    foreach (Core.Db.Record record in recordsArray)
                    {
                        Core.Db.User createdByUser = !string.IsNullOrEmpty(record.CreatedBy) ? WexflowServer.WexflowEngine.GetUserById(record.CreatedBy) : null;
                        Core.Db.User modifiedByUser = !string.IsNullOrEmpty(record.ModifiedBy) ? WexflowServer.WexflowEngine.GetUserById(record.ModifiedBy) : null;
                        Core.Db.User assignedToUser = !string.IsNullOrEmpty(record.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(record.AssignedTo) : null;
                        Contracts.Record r = new()
                        {
                            Id = record.GetDbId(),
                            Name = record.Name,
                            Description = record.Description,
                            StartDate = record.StartDate.HasValue ? record.StartDate.Value.ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            EndDate = record.EndDate.HasValue ? record.EndDate.Value.ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            Comments = record.Comments,
                            Approved = record.Approved,
                            ManagerComments = record.ManagerComments,
                            ModifiedBy = modifiedByUser != null ? modifiedByUser.Username : string.Empty,
                            ModifiedOn = record.ModifiedOn.HasValue ? record.ModifiedOn.Value.ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            CreatedBy = createdByUser != null ? createdByUser.Username : string.Empty,
                            CreatedOn = record.CreatedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                            AssignedTo = assignedToUser != null ? assignedToUser.Username : string.Empty,
                            AssignedOn = record.AssignedOn.HasValue ? record.AssignedOn.Value.ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty
                        };

                        // Approvers
                        Core.Db.Approver[] approvers = WexflowServer.WexflowEngine.GetApprovers(record.GetDbId());
                        List<Contracts.Approver> approversList = new();
                        foreach (Core.Db.Approver approver in approvers)
                        {
                            Core.Db.User approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
                            if (approverUser != null)
                            {
                                Contracts.Approver a = new()
                                {
                                    ApprovedBy = approverUser.Username,
                                    Approved = approver.Approved,
                                    ApprovedOn = approver.ApprovedOn == null ? string.Empty : approver.ApprovedOn.Value.ToString(WexflowServer.Config["DateTimeFormat"])
                                };
                                approversList.Add(a);
                            }
                        }
                        r.Approvers = approversList.ToArray();

                        // Versions
                        Core.Db.Version[] versions = WexflowServer.WexflowEngine.GetVersions(record.GetDbId());
                        List<Contracts.Version> versionsList = new();
                        foreach (Core.Db.Version version in versions)
                        {
                            Contracts.Version v = new()
                            {
                                Id = version.GetDbId(),
                                RecordId = version.RecordId,
                                FilePath = version.FilePath,
                                FileName = Path.GetFileName(version.FilePath),
                                CreatedOn = version.CreatedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                                FileSize = GetFileSize(version.FilePath)
                            };
                            versionsList.Add(v);
                        }
                        r.Versions = versionsList.ToArray();
                        recordsList.Add(r);
                    }
                    records = recordsList.ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(records));
            });
        }

        /// <summary>
        /// Searches for records assigned to or created by a user by keyword.
        /// </summary>
        private void SearchRecordsCreatedByOrAssignedTo()
        {
            _endpoints.MapGet(Root + "searchRecordsCreatedByOrAssignedTo", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string keyword = context.Request.Query["s"].ToString();
                string createdByUsername = context.Request.Query["c"].ToString();
                Core.Db.User createdBy = !string.IsNullOrEmpty(createdByUsername) ? WexflowServer.WexflowEngine.GetUser(createdByUsername) : null;
                string assignedToUsername = context.Request.Query["a"].ToString();
                Core.Db.User assignedTo = !string.IsNullOrEmpty(assignedToUsername) ? WexflowServer.WexflowEngine.GetUser(assignedToUsername) : null;

                Contracts.Record[] records = Array.Empty<Contracts.Record>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.Record[] recordsArray = WexflowServer.WexflowEngine.GetRecordsCreatedByOrAssignedTo(createdBy != null ? createdBy.GetDbId() : string.Empty, assignedTo != null ? assignedTo.GetDbId() : string.Empty, keyword);
                    List<Contracts.Record> recordsList = new();
                    foreach (Core.Db.Record record in recordsArray)
                    {
                        Core.Db.User createdByUser = !string.IsNullOrEmpty(record.CreatedBy) ? WexflowServer.WexflowEngine.GetUserById(record.CreatedBy) : null;
                        Core.Db.User modifiedByUser = !string.IsNullOrEmpty(record.ModifiedBy) ? WexflowServer.WexflowEngine.GetUserById(record.ModifiedBy) : null;
                        Core.Db.User assignedToUser = !string.IsNullOrEmpty(record.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(record.AssignedTo) : null;
                        Contracts.Record r = new()
                        {
                            Id = record.GetDbId(),
                            Name = record.Name,
                            Description = record.Description,
                            StartDate = record.StartDate != null ? ((DateTime)record.StartDate).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            EndDate = record.EndDate != null ? ((DateTime)record.EndDate).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            Comments = record.Comments,
                            Approved = record.Approved,
                            ManagerComments = record.ManagerComments,
                            ModifiedBy = modifiedByUser != null ? modifiedByUser.Username : string.Empty,
                            ModifiedOn = record.ModifiedOn != null ? ((DateTime)record.ModifiedOn).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty,
                            CreatedBy = createdByUser != null ? createdByUser.Username : string.Empty,
                            CreatedOn = record.CreatedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                            AssignedTo = assignedToUser != null ? assignedToUser.Username : string.Empty,
                            AssignedOn = record.AssignedOn != null ? ((DateTime)record.AssignedOn).ToString(WexflowServer.Config["DateTimeFormat"]) : string.Empty
                        };

                        // Approvers
                        Core.Db.Approver[] approvers = WexflowServer.WexflowEngine.GetApprovers(record.GetDbId());
                        List<Contracts.Approver> approversList = new();
                        foreach (Core.Db.Approver approver in approvers)
                        {
                            Core.Db.User approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
                            if (approverUser != null)
                            {
                                Contracts.Approver a = new()
                                {
                                    ApprovedBy = approverUser.Username,
                                    Approved = approver.Approved,
                                    ApprovedOn = approver.ApprovedOn == null ? string.Empty : approver.ApprovedOn.Value.ToString(WexflowServer.Config["DateTimeFormat"])
                                };
                                approversList.Add(a);
                            }
                        }
                        r.Approvers = approversList.ToArray();

                        // Versions
                        Core.Db.Version[] versions = WexflowServer.WexflowEngine.GetVersions(record.GetDbId());
                        List<Contracts.Version> versionsList = new();
                        foreach (Core.Db.Version version in versions)
                        {
                            Contracts.Version v = new()
                            {
                                Id = version.GetDbId(),
                                RecordId = version.RecordId,
                                FilePath = version.FilePath,
                                FileName = Path.GetFileName(version.FilePath),
                                CreatedOn = version.CreatedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                                FileSize = GetFileSize(version.FilePath)
                            };
                            versionsList.Add(v);
                        }
                        r.Versions = versionsList.ToArray();
                        recordsList.Add(r);
                    }
                    records = recordsList.ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(records));
            });
        }

        /// <summary>
        /// Indicates whether the user has notifications or not.
        /// </summary>
        private void HasNotifications()
        {
            _endpoints.MapGet(Root + "hasNotifications", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string assignedToUsername = context.Request.Query["a"].ToString();
                Core.Db.User assignedTo = WexflowServer.WexflowEngine.GetUser(assignedToUsername);

                bool res = false;

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    res = WexflowServer.WexflowEngine.HasNotifications(assignedTo.GetDbId());
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Marks notifications as read.
        /// </summary>
        private void MarkNotificationsAsRead()
        {
            _endpoints.MapPost(Root + "markNotificationsAsRead", async context =>
            {
                try
                {
                    bool res = false;
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string json = GetBody(context);
                        string[] notificationIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
                        res = WexflowServer.WexflowEngine.MarkNotificationsAsRead(notificationIds);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Marks notifications as unread.
        /// </summary>
        private void MarkNotificationsAsUnread()
        {
            _endpoints.MapPost(Root + "markNotificationsAsUnread", async context =>
            {
                try
                {
                    bool res = false;
                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string json = GetBody(context);
                        string[] notificationIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
                        res = WexflowServer.WexflowEngine.MarkNotificationsAsUnread(notificationIds);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Deletes notifications.
        /// </summary>
        private void DeleteNotifications()
        {
            _endpoints.MapPost(Root + "deleteNotifications", async context =>
            {
                try
                {
                    bool res = false;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string json = GetBody(context);
                        string[] notificationIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
                        res = WexflowServer.WexflowEngine.DeleteNotifications(notificationIds);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Searches for notifications of a user.
        /// </summary>
        private void SearchNotifications()
        {
            _endpoints.MapGet(Root + "searchNotifications", async context =>
            {
                Auth auth = GetAuth(context.Request);
                string username = auth.Username;
                string password = auth.Password;

                string assignedToUsername = context.Request.Query["a"].ToString();
                Core.Db.User assignedTo = WexflowServer.WexflowEngine.GetUser(assignedToUsername);
                string keyword = context.Request.Query["s"].ToString();

                Contracts.Notification[] notifications = Array.Empty<Contracts.Notification>();

                Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    Core.Db.Notification[] notificationsArray = WexflowServer.WexflowEngine.GetNotifications(assignedTo.GetDbId(), keyword);
                    List<Contracts.Notification> notificationList = new();
                    foreach (Core.Db.Notification notification in notificationsArray)
                    {
                        Core.Db.User assignedByUser = !string.IsNullOrEmpty(notification.AssignedBy) ? WexflowServer.WexflowEngine.GetUserById(notification.AssignedBy) : null;
                        Core.Db.User assignedToUser = !string.IsNullOrEmpty(notification.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(notification.AssignedTo) : null;
                        Contracts.Notification n = new()
                        {
                            Id = notification.GetDbId(),
                            AssignedBy = assignedByUser != null ? assignedByUser.Username : string.Empty,
                            AssignedOn = notification.AssignedOn.ToString(WexflowServer.Config["DateTimeFormat"]),
                            AssignedTo = assignedToUser != null ? assignedToUser.Username : string.Empty,
                            Message = notification.Message,
                            IsRead = notification.IsRead
                        };
                        notificationList.Add(n);
                    }
                    notifications = notificationList.ToArray();
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(notifications));
            });
        }

        /// <summary>
        /// Notifies a user.
        /// </summary>
        /// <param name="assignedBy">Assigned by.</param>
        /// <param name="assignedTo">Assigned to.</param>
        /// <param name="message">Message.</param>
        /// <returns>Result.</returns>
        private bool NotifyUser(Core.Db.User assignedBy, Core.Db.User assignedTo, string message)
        {
            bool res = false;
            if (assignedTo != null && !string.IsNullOrEmpty(message))
            {
                Core.Db.Notification notification = new()
                {
                    AssignedBy = assignedBy.GetDbId(),
                    AssignedOn = DateTime.Now,
                    AssignedTo = assignedTo.GetDbId(),
                    Message = message,
                    IsRead = false
                };
                string id = WexflowServer.WexflowEngine.InsertNotification(notification);
                res = id != "-1";

                bool enableEmailNotifications = bool.Parse(WexflowServer.Config["EnableEmailNotifications"]);
                if (enableEmailNotifications)
                {
                    string subject = "Wexflow notification from " + assignedBy.Username;
                    string body = message;

                    string host = WexflowServer.Config["Smtp.Host"];
                    int port = int.Parse(WexflowServer.Config["Smtp.Port"]);
                    bool enableSsl = bool.Parse(WexflowServer.Config["Smtp.EnableSsl"]);
                    string smtpUser = WexflowServer.Config["Smtp.User"];
                    string smtpPassword = WexflowServer.Config["Smtp.Password"];
                    string from = WexflowServer.Config["Smtp.From"];

                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                }
            }
            return res;
        }

        /// <summary>
        /// Notifies a user.
        /// </summary>
        private void Notify()
        {
            _endpoints.MapPost(Root + "notify", async context =>
            {
                try
                {
                    bool res = false;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string assignedToUsername = context.Request.Query["a"].ToString();
                        string message = context.Request.Query["m"].ToString();
                        Core.Db.User assignedTo = WexflowServer.WexflowEngine.GetUser(assignedToUsername);
                        res = NotifyUser(user, assignedTo, message);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Notifies a approvers.
        /// </summary>
        private void NotifyApprovers()
        {
            _endpoints.MapPost(Root + "notifyApprovers", async context =>
            {
                try
                {
                    bool res = true;

                    Auth auth = GetAuth(context.Request);
                    string username = auth.Username;
                    string password = auth.Password;

                    Core.Db.User user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        string recordId = context.Request.Query["r"].ToString();
                        string message = context.Request.Query["m"].ToString();

                        Core.Db.Approver[] approvers = WexflowServer.WexflowEngine.GetApprovers(recordId);
                        foreach (Core.Db.Approver approver in approvers)
                        {
                            Core.Db.User approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
                            res &= NotifyUser(user, approverUser, message);
                        }
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        [GeneratedRegex("( )(?:[^\\w>/])")]
        private static partial Regex MyRegex();
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;
using Wexflow.Core.Auth;
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
        public const string ROOT = "api/v1";
        private static readonly XNamespace Xn = "urn:wexflow-schema";

        private readonly IEndpointRouteBuilder _endpoints;
        private readonly IConfiguration _config;
        private readonly int _jwtExpireAtMinutes;

        public WexflowService(IEndpointRouteBuilder endpoints, IConfiguration config)
        {
            _endpoints = endpoints;
            _config = config;
            _jwtExpireAtMinutes = int.TryParse(config["JwtExpireAtMinutes"], out var res)
                ? res
                : 1440;
        }

        public void Map()
        {
            //
            // Greeting
            //
            Hello();

            //
            // Auth
            //
            Login();
            Logout();
            ValidateToken();
            ValidateUser();
            VerifyPassword();

            //
            // SSE
            //
            WorkflowStatusSse();
            StatusCountSse();

            //
            // Dashboard
            //
            GetStatusCount();
            GetEntriesCountByDate();
            SearchEntriesByPageOrderBy();
            GetEntryStatusDateMin();
            GetEntryStatusDateMax();
            GetEntryLogs();
            GetEntry();

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

        #region utils

        private static string GetPattern(string pattern) => $"/{ROOT}/{pattern}";

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
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Unauthorized!" }));
        }

        private static async System.Threading.Tasks.Task Error(HttpContext context, Exception e)
        {
            Console.WriteLine(e);
            await WriteFalse(context);
        }

        private static void NotFound(HttpContext context)
        {
            context.Response.StatusCode = 204;
        }

        #endregion

        /// <summary>
        /// Returns a JSON message confirming the service is running.
        /// </summary>
        private void Hello()
        {
            _ = _endpoints.MapGet(GetPattern("hello"), async context =>
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Wexflow Service is running..." }));
            });
        }

        /// <summary>
        /// Login with username and password. Returns a JWT token if successful.
        /// </summary>
        private void Login()
        {
            _ = _endpoints.MapPost(GetPattern("login"), async context =>
            {
                var json = await GetBodyAsync(context);
                var o = JObject.Parse(json);
                var username = o.Value<string>("username");
                var password = o.Value<string>("password");
                var stayConnected = o.Value<bool>("stayConnected");

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if (user != null && PasswordHasher.VerifyPassword(password, user.Password))
                    {
                        var token = JwtHelper.GenerateToken(username, _jwtExpireAtMinutes, stayConnected);

                        var expireMinutes = int.TryParse(_config["JwtExpireAtMinutes"], out var res) ? res : 1440;
                        var https = bool.TryParse(_config["HTTPS"], out var httpsRes) && httpsRes;

                        context.Response.Cookies.Append(JwtHelper.AuthCookieName, token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = https,
                            SameSite = SameSiteMode.Lax,
                            Expires = stayConnected ? DateTimeOffset.UtcNow.AddYears(1) : DateTimeOffset.UtcNow.AddMinutes(expireMinutes)
                        });

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { access_token = token }));
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            });
        }

        /// <summary>
        /// Logout.
        /// </summary>
        private void Logout()
        {
            _ = _endpoints.MapPost(GetPattern("logout"), async context =>
            {
                context.Response.Cookies.Delete(JwtHelper.AuthCookieName);
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("{\"logged_out\":true}");
            });
        }

        /// <summary>
        /// Validates JWT token.
        /// </summary>
        private void ValidateToken()
        {
            _ = _endpoints.MapPost(GetPattern("validate-token"), async context =>
            {
                var username = context.Request.Query["u"].ToString();
                var identity = context.User?.Identity;

                if (identity?.IsAuthenticated == true && string.Equals(identity.Name, username, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"valid\":true}");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token");
                }
            });
        }

        /// <summary>
        /// Validates user.
        /// </summary>
        private void ValidateUser()
        {
            _ = _endpoints.MapPost(GetPattern("validate-user"), async context =>
            {
                var username = context.Request.Query["username"].ToString();
                var identity = context.User?.Identity;

                if (identity?.IsAuthenticated == true && string.Equals(identity.Name, username, StringComparison.OrdinalIgnoreCase))
                {
                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    var dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

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
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired token");

            });
        }

        /// <summary>
        /// Verifies user password.
        /// </summary>
        private void VerifyPassword()
        {
            _ = _endpoints.MapPost(GetPattern("verify-password"), async context =>
            {
                var username = context.Request.Query["username"].ToString();
                var password = context.Request.Query["p"].ToString();

                var user = WexflowServer.WexflowEngine.GetUser(username);

                var res = false;
                try
                {
                    if (user != null)
                    {
                        res = Db.VerifyPassword(password, user.Password);
                    }

                }
                catch (Exception e)
                {
                    await Error(context, e);
                    return;
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Maps a Server-Sent Events (SSE) endpoint that notifies the client
        /// when a workflow job finishes and sends its final status as a single SSE message.
        /// </summary>
        /// <remarks>
        /// The route pattern is <c>sse/{workflowId:int}/{jobId}</c>.  
        /// This endpoint is intended for clients who want to be notified when a long-running job
        /// completes (successfully or with failure), without polling the API continuously.
        /// </remarks>
        /// <example>
        /// GET /sse/42/abc123
        /// </example>
        /// <exception cref="BadHttpRequestException">
        /// Thrown if <c>workflowId</c> is invalid or <c>jobId</c> is missing.
        /// </exception>
        private void WorkflowStatusSse()
        {
            _ = _endpoints.MapGet(GetPattern("sse/{workflowId:int}/{jobId}"), async context =>
            {
                if (
                    !int.TryParse(context.Request.RouteValues["workflowId"]?.ToString(), out var workflowId) ||
                    string.IsNullOrEmpty(context.Request.RouteValues["jobId"]?.ToString())
                    )
                {
                    throw new BadHttpRequestException("Missing or invalid workflowId or jobId.");
                }

                var workflow = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(wf => wf.Id == workflowId);
                if (workflow == null)
                {
                    throw new BadHttpRequestException("Invalid workflowId.");
                }

                // check user profile
                var username = context.User.Identity?.Name;
                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflow.DbId);
                }

                if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }

                // authorized
                var jobId = context.Request.RouteValues["jobId"].ToString();

                context.Response.Headers["Content-Type"] = "text/event-stream";
                context.Response.Headers["Cache-Control"] = "no-cache";
                context.Response.Headers["Connection"] = "keep-alive";

                var broadcaster = context.RequestServices.GetRequiredService<WorkflowStatusBroadcaster>();
                var tcs = new TaskCompletionSource();

                void Send(string status)
                {
                    if (context.RequestAborted.IsCancellationRequested)
                    {
                        tcs.TrySetResult();
                        return;
                    }
                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        var json = JsonConvert.SerializeObject(new
                        {
                            workflowId,
                            jobId,
                            status,
                            name = workflow.Name,
                            description = workflow.Description,
                        });

                        var message = $"data: {json}\n\n";
                        await context.Response.WriteAsync(message);
                        await context.Response.Body.FlushAsync();

                        broadcaster.Unsubscribe(workflowId, jobId, Send);
                        tcs.TrySetResult();
                    });
                }

                broadcaster.Subscribe(workflowId, jobId, Send);

                var cancellation = context.RequestAborted.Register(() =>
                {
                    broadcaster.Unsubscribe(workflowId, jobId, Send);
                    tcs.TrySetResult();
                });

                await tcs.Task;
                cancellation.Dispose();
            });
        }

        /// <summary>
        /// Maps the Server-Sent Events (SSE) endpoint for streaming real-time <see cref="StatusCount"/> updates.
        /// Clients connecting to this endpoint receive updates whenever the workflow status counts change.
        /// </summary>
        private void StatusCountSse()
        {
            _ = _endpoints.MapGet(GetPattern("sse/status-count"), async context =>
            {
                context.Response.Headers["Content-Type"] = "text/event-stream";
                context.Response.Headers["Cache-Control"] = "no-cache";
                context.Response.Headers["Connection"] = "keep-alive";

                var cancellationToken = context.RequestAborted;

                // Add this response to the list of SSE subscribers
                WexflowServer.StatusCountClients.TryAdd(context.Response, true);

                try
                {
                    // Keep the connection open until the client disconnects
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Optionally send a comment to keep the connection alive every 15 seconds
                        await context.Response.WriteAsync(": keep-alive\n\n", cancellationToken);
                        await context.Response.Body.FlushAsync(cancellationToken);

                        // Wait before sending next keep-alive or update
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Client disconnected
                }
                finally
                {
                    // Remove this client from the subscribers list on disconnect
                    WexflowServer.StatusCountClients.TryRemove(context.Response, out _);
                }
            });
        }


        /// <summary>
        /// Search for workflows.
        /// </summary>
        private void Search()
        {
            _ = _endpoints.MapGet(GetPattern("search"), async context =>
            {
                var username = context.User.Identity?.Name;

                var keywordToUpper = context.Request.Query["s"].ToString().ToUpper();

                var workflows = Array.Empty<WorkflowInfo>();

                var user = WexflowServer.WexflowEngine.GetUser(username);

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    workflows = WexflowServer.WexflowEngine.Workflows
                        .ToList()
                        .Where(wf =>
                                wf.Name.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                                || wf.Description.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                                || wf.Id.ToString().Contains(keywordToUpper))
                        .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                           , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                           , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                           , wf.RetryCount
                           , wf.RetryTimeout
                           , wf.JobStatus
                           ))
                        .ToArray();
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    workflows = WexflowServer.WexflowEngine.GetUserWorkflows(user.GetDbId())
                                            .ToList()
                                            .Where(wf =>
                                                wf.Name.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                                                || wf.Description.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                                                || wf.Id.ToString().Contains(keywordToUpper))
                                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                wf.IsExecutionGraphEmpty
                                               , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                                               , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                                               , wf.RetryCount
                                               , wf.RetryTimeout
                                               , wf.JobStatus
                                               ))
                                            .ToArray();
                }


                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflows));
            });
        }

        /// <summary>
        /// Search for approval workflows.
        /// </summary>
        private void SearchApprovalWorkflows()
        {
            _ = _endpoints.MapGet(GetPattern("search-approval-workflows"), async context =>
            {
                var username = context.User.Identity?.Name;

                var keywordToUpper = context.Request.Query["s"].ToString().ToUpper();

                var workflows = Array.Empty<WorkflowInfo>();

                var user = WexflowServer.WexflowEngine.GetUser(username);

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    workflows = WexflowServer.WexflowEngine.Workflows
                        .ToList()
                        .Where(wf =>
                            wf.IsApproval &&
                            (wf.Name.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                            || wf.Description.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                            || wf.Id.ToString().Contains(keywordToUpper)))
                        .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                           , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                           , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                           , wf.RetryCount
                           , wf.RetryTimeout
                           , wf.JobStatus
                           ))
                        .ToArray();
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    workflows = WexflowServer.WexflowEngine.GetUserWorkflows(user.GetDbId())
                                            .ToList()
                                            .Where(wf =>
                                                wf.IsApproval &&
                                                (wf.Name.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                                                || wf.Description.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase)
                                                || wf.Id.ToString().Contains(keywordToUpper)))
                                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                wf.IsExecutionGraphEmpty
                                               , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                                               , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                                               , wf.RetryCount
                                               , wf.RetryTimeout
                                               , wf.JobStatus
                                               ))
                                            .ToArray();
                }


                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflows));
            });
        }

        /// <summary>
        /// Returns a workflow from its id.
        /// </summary>
        private void GetWorkflow()
        {
            _ = _endpoints.MapGet(GetPattern("workflow"), async context =>
            {
                var username = context.User.Identity?.Name;

                var id = int.Parse(context.Request.Query["w"].ToString());

                var wf = WexflowServer.WexflowEngine.GetWorkflow(id);
                if (wf != null)
                {
                    WorkflowInfo workflow = new(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description,
                        wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                        wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                        , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                        , wf.RetryCount
                        , wf.RetryTimeout
                        , wf.JobStatus
                        );

                    var user = WexflowServer.WexflowEngine.GetUser(username);


                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                        if (check)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                        }
                    }

                }
                else
                {
                    NotFound(context);
                }
            });
        }

        /// <summary>
        /// Returns a job from a workflow id and an instance id.
        /// </summary>
        private void GetJob()
        {
            _ = _endpoints.MapGet(GetPattern("job"), async context =>
            {
                var username = context.User.Identity?.Name;

                var id = int.Parse(context.Request.Query["w"].ToString());
                var jobId = context.Request.Query["i"].ToString();

                var wf = WexflowServer.WexflowEngine.GetWorkflow(id);
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
                            , wf.RetryCount
                            , wf.RetryTimeout
                            , wf.JobStatus
                            );

                        var user = WexflowServer.WexflowEngine.GetUser(username);


                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                            if (check)
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));
                            }
                        }

                    }
                }
                else
                {
                    NotFound(context);
                }
            });
        }

        /// <summary>
        /// Returns a jobs from a workflow id.
        /// </summary>
        private void GetJobs()
        {
            _ = _endpoints.MapGet(GetPattern("jobs"), async context =>
            {
                var username = context.User.Identity?.Name;

                var id = int.Parse(context.Request.Query["w"].ToString());

                var wf = WexflowServer.WexflowEngine.GetWorkflow(id);
                if (wf != null)
                {
                    var jobs = wf.Jobs.Select(j => j.Value).Select(
                        w => new WorkflowInfo(w.DbId, w.Id, w.InstanceId, w.Name, w.FilePath, (LaunchType)w.LaunchType, w.IsEnabled, w.IsApproval, w.EnableParallelJobs, w.IsWaitingForApproval, w.Description,
                            w.IsRunning, w.IsPaused, w.Period.ToString(@"dd\.hh\:mm\:ss"), w.CronExpression,
                            w.IsExecutionGraphEmpty
                            , w.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                            , w.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                            , wf.RetryCount
                            , wf.RetryTimeout
                            , wf.JobStatus
                            ));

                    var user = WexflowServer.WexflowEngine.GetUser(username);


                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(jobs));
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                        if (check)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(jobs));
                        }
                    }
                }

            });
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        private void StartWorkflow()
        {
            _ = _endpoints.MapPost(GetPattern("start"), async context =>
            {
                var username = context.User.Identity?.Name;

                var workflowId = int.Parse(context.Request.Query["w"].ToString());

                var user = WexflowServer.WexflowEngine.GetUser(username);

                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    var instanceId = await WexflowServer.WexflowEngine.StartWorkflowAsync(username, workflowId);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        var instanceId = await WexflowServer.WexflowEngine.StartWorkflowAsync(username, workflowId);
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                }

            });
        }

        private static async Task<string> GetBodyAsync(HttpContext context)
        {
            using StreamReader reader = new(context.Request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
            var body = await reader.ReadToEndAsync(); // Non-blocking
            return body;
        }

        /// <summary>
        /// Starts a workflow with variables.
        /// </summary>
        private void StartWorkflowWithVariables()
        {
            _ = _endpoints.MapPost(GetPattern("start-with-variables"), async context =>
            {
                var username = context.User.Identity?.Name;

                var json = await GetBodyAsync(context);
                var o = JObject.Parse(json);
                var workflowId = o.Value<int>("WorkflowId");
                var variables = o.Value<JArray>("Variables");

                List<Core.Variable> restVariables = [];
                foreach (var variable in variables)
                {
                    restVariables.Add(new Core.Variable { Key = variable.Value<string>("Name"), Value = variable.Value<string>("Value") });
                }

                var workflow = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId);

                var user = WexflowServer.WexflowEngine.GetUser(username);

                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    var instanceId = await WexflowServer.WexflowEngine.StartWorkflowAsync(username, workflowId, restVariables);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = workflow.DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        var instanceId = await WexflowServer.WexflowEngine.StartWorkflowAsync(username, workflowId, restVariables);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(instanceId.ToString()));
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
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
            _ = _endpoints.MapPost(GetPattern("stop"), async context =>
            {
                var res = false;

                var username = context.User.Identity?.Name;

                var workflowId = int.Parse(context.Request.Query["w"].ToString());
                var instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    res = WexflowServer.WexflowEngine.StopWorkflow(workflowId, instanceId, username);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        res = WexflowServer.WexflowEngine.StopWorkflow(workflowId, instanceId, username);
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));

            });
        }

        /// <summary>
        /// Suspends a workflow.
        /// </summary>
        private void SuspendWorkflow()
        {
            _ = _endpoints.MapPost(GetPattern("suspend"), async context =>
            {
                var res = false;

                var username = context.User.Identity?.Name;

                var workflowId = int.Parse(context.Request.Query["w"].ToString());
                var instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    res = WexflowServer.WexflowEngine.SuspendWorkflow(workflowId, instanceId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        res = WexflowServer.WexflowEngine.SuspendWorkflow(workflowId, instanceId);
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));

            });
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        private void ResumeWorkflow()
        {
            _ = _endpoints.MapPost(GetPattern("resume"), async context =>
            {
                var username = context.User.Identity?.Name;

                var workflowId = int.Parse(context.Request.Query["w"].ToString());
                var instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    WexflowServer.WexflowEngine.ResumeWorkflow(workflowId, instanceId);
                    await context.Response.WriteAsync(string.Empty);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        WexflowServer.WexflowEngine.ResumeWorkflow(workflowId, instanceId);
                        await context.Response.WriteAsync(string.Empty);
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
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
            _ = _endpoints.MapPost(GetPattern("approve"), async context =>
            {
                var res = false;

                var username = context.User.Identity?.Name;

                var workflowId = int.Parse(context.Request.Query["w"].ToString());
                var instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    res = WexflowServer.WexflowEngine.ApproveWorkflow(workflowId, instanceId, username);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        res = WexflowServer.WexflowEngine.ApproveWorkflow(workflowId, instanceId, username);
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }


                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Rejects a workflow.
        /// </summary>
        private void RejectWorkflow()
        {
            _ = _endpoints.MapPost(GetPattern("reject"), async context =>
            {
                var res = false;

                var username = context.User.Identity?.Name;

                var workflowId = int.Parse(context.Request.Query["w"].ToString());
                var instanceId = Guid.Parse(context.Request.Query["i"].ToString());

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = true;

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    res = WexflowServer.WexflowEngine.RejectWorkflow(workflowId, instanceId, username);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                    if (authorized)
                    {
                        res = WexflowServer.WexflowEngine.RejectWorkflow(workflowId, instanceId, username);
                    }
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }


                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            });
        }

        /// <summary>
        /// Returns workflow's tasks.
        /// </summary>
        private void GetTasks()
        {
            _ = _endpoints.MapGet(GetPattern("tasks/{id}"), async context =>
            {

                var wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException()));
                if (wf != null)
                {
                    IList<TaskInfo> taskInfos = [];

                    foreach (var task in wf.Tasks)
                    {
                        IList<SettingInfo> settingInfos = [];

                        foreach (var setting in task.Settings)
                        {
                            IList<AttributeInfo> attributeInfos = [];

                            foreach (var attribute in setting.Attributes)
                            {
                                AttributeInfo attributeInfo = new(attribute.Name, attribute.Value);
                                attributeInfos.Add(attributeInfo);
                            }

                            SettingInfo settingInfo = new(setting.Name, setting.Value, [.. attributeInfos]);
                            settingInfos.Add(settingInfo);
                        }

                        TaskInfo taskInfo = new(task.Id, task.Name, task.Description, task.IsEnabled, [.. settingInfos]);

                        taskInfos.Add(taskInfo);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(taskInfos));
                }
                else
                {
                    NotFound(context);
                }

            });
        }

        /// <summary>
        /// Returns next vacant Task Id.
        /// </summary>
        private void GetNewWorkflowId()
        {
            _ = _endpoints.MapGet(GetPattern("workflow-id"), async context =>
            {


                var workflowId = 0;


                try
                {
                    var workflows = WexflowServer.WexflowEngine.Workflows;

                    if (workflows is { Count: > 0 })
                    {
                        workflowId = workflows.Select(w => w.Id).Max() + 1;
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(workflowId));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }

            });
        }

        /// <summary>
        /// Returns a workflow as XML.
        /// </summary>
        private void GetWorkflowXml()
        {
            _ = _endpoints.MapGet(GetPattern("xml/{id}"), async context =>
            {
                var username = context.User.Identity?.Name;

                var wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException()));

                if (wf == null)
                {
                    NotFound(context);
                    return;
                }

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = false;

                if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }
                else if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    authorized = true;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }


                await context.Response.WriteAsync(JsonConvert.SerializeObject(wf.Xml));

            });
        }

        /// <summary>
        /// Returns a workflow as JSON.
        /// </summary>
        private void GetWorkflowJson()
        {
            _ = _endpoints.MapGet(GetPattern("json/{id}"), async context =>
            {
                var username = context.User.Identity?.Name;

                var wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException()));

                if (wf == null)
                {
                    NotFound(context);
                    return;
                }

                var user = WexflowServer.WexflowEngine.GetUser(username);
                var authorized = false;

                if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    authorized = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    authorized = false;
                }
                else if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    authorized = true;
                }

                if (!authorized)
                {
                    await Unauthorized(context);
                    return;
                }


                List<Contracts.Variable> variables = [];
                foreach (var variable in wf.LocalVariables)
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
                    LocalVariables = [.. variables],
                    RetryCount = wf.RetryCount,
                    RetryTimeout = wf.RetryTimeout
                };

                List<TaskInfo> tasks = [];
                foreach (var task in wf.Tasks)
                {
                    List<SettingInfo> settings = [];
                    foreach (var setting in task.Settings)
                    {
                        List<AttributeInfo> attributes = [];
                        foreach (var attr in setting.Attributes)
                        {
                            attributes.Add(new AttributeInfo(attr.Name, attr.Value));
                        }

                        settings.Add(new SettingInfo(setting.Name, setting.Value, [.. attributes]));
                    }
                    tasks.Add(new TaskInfo(task.Id, task.Name, task.Description, task.IsEnabled, [.. settings]));
                }

                Contracts.Workflow.Workflow workflow = new()
                {
                    WorkflowInfo = wi,
                    Tasks = [.. tasks],
                    ExecutionGraph = wf.ExecutionGraph
                };

                await context.Response.WriteAsync(JsonConvert.SerializeObject(workflow));


            });
        }

        /// <summary>
        /// Returns task names.
        /// </summary>
        private void GetTaskNames()
        {
            _ = _endpoints.MapGet(GetPattern("task-names"), async context =>
            {

                TaskName[] taskNames;
                try
                {
                    var array = JArray.Parse(await File.ReadAllTextAsync(WexflowServer.WexflowEngine.TasksNamesFile));
                    taskNames = [.. array.ToObject<TaskName[]>().OrderBy(x => x.Name)];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    taskNames = [new TaskName { Name = "TasksNames.json is not valid." }];
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(taskNames));

            });
        }

        /// <summary>
        /// Returns task names.
        /// </summary>
        private void SearchTaskNames()
        {
            _ = _endpoints.MapGet(GetPattern("search-task-names"), async context =>
            {
                var keywordToUpper = context.Request.Query["s"].ToString().ToUpper();


                TaskName[] taskNames;
                try
                {
                    var array = JArray.Parse(await File.ReadAllTextAsync(WexflowServer.WexflowEngine.TasksNamesFile));
                    taskNames = [.. array
                        .ToObject<TaskName[]>()
                        .Where(x => x.Name.Contains(keywordToUpper, StringComparison.CurrentCultureIgnoreCase))
                        .OrderBy(x => x.Name)];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    taskNames = [new TaskName { Name = "TasksNames.json is not valid." }];
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(taskNames));

            });
        }

        /// <summary>
        /// Returns task settings.
        /// </summary>
        private void GetSettings()
        {
            _ = _endpoints.MapGet(GetPattern("settings/{taskName}"), async context =>
            {


                TaskSetting[] taskSettings;
                try
                {
                    var o = JObject.Parse(await File.ReadAllTextAsync(WexflowServer.WexflowEngine.TasksSettingsFile));
                    var token = o.SelectToken(context.Request.RouteValues["taskName"]?.ToString() ?? throw new InvalidOperationException());
                    taskSettings = token != null ? token.ToObject<TaskSetting[]>() : [];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    taskSettings = [new TaskSetting { Name = "TasksSettings.json is not valid." }];
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(taskSettings));

            });
        }

        /// <summary>
        /// Returns a task as XML.
        /// </summary>
        private void GetTaskXml()
        {
            _ = _endpoints.MapPost(GetPattern("task-to-xml"), async context =>
            {
                try
                {


                    var json = await GetBodyAsync(context);

                    var task = JObject.Parse(json);

                    var taskId = (int)task.SelectToken("Id");
                    var taskName = (string)task.SelectToken("Name");
                    var taskDesc = (string)task.SelectToken("Description");
                    var isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                    XElement xtask = new("Task"
                        , new XAttribute("id", taskId)
                        , new XAttribute("name", taskName!)
                        , new XAttribute("description", taskDesc ?? string.Empty)
                        , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                    );

                    var settings = task.SelectToken("Settings");
                    foreach (var setting in settings!)
                    {
                        var settingName = (string)setting.SelectToken("Name");
                        var settingValue = (string)setting.SelectToken("Value");

                        XElement xsetting = new("Setting", new XAttribute("name", settingName!));

                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            xsetting.SetAttributeValue("value", settingValue);
                        }

                        if (settingName is "selectFiles" or "selectAttachments")
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

                        var attributes = setting.SelectToken("Attributes");
                        foreach (var attribute in attributes!)
                        {
                            var attributeName = (string)attribute.SelectToken("Name");
                            var attributeValue = (string)attribute.SelectToken("Value");
                            xsetting.SetAttributeValue(attributeName!, attributeValue);
                        }

                        xtask.Add(xsetting);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(xtask.ToString()));

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
            _ = _endpoints.MapGet(GetPattern("is-workflow-id-valid/{id}"), async context =>
            {


                var workflowId = int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException());
                foreach (var workflow in WexflowServer.WexflowEngine.Workflows)
                {
                    if (workflow.Id == workflowId)
                    {
                        await WriteFalse(context);
                        return;
                    }
                }

                await WriteTrue(context);

            });
        }

        /// <summary>
        /// Checks if a cron expression is valid.
        /// </summary>
        private void IsCronExpressionValid()
        {
            _ = _endpoints.MapGet(GetPattern("is-cron-expression-valid"), async context =>
            {


                var expression = context.Request.Query["e"].ToString();
                var res = WexflowEngine.IsCronExpressionValid(expression);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));

            });
        }

        /// <summary>
        /// Checks if a period is valid.
        /// </summary>
        private void IsPeriodValid()
        {
            _ = _endpoints.MapGet(GetPattern("is-period-valid/{period}"), async context =>
            {

                var res = TimeSpan.TryParse(context.Request.RouteValues["period"]?.ToString(), out _);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));

            });
        }

        /// <summary>
        /// Checks if the XML of a workflow is valid.
        /// </summary>
        private void IsXmlWorkflowValid()
        {
            _ = _endpoints.MapPost(GetPattern("is-xml-workflow-valid"), async context =>
            {
                try
                {

                    var json = await GetBodyAsync(context);
                    var o = JObject.Parse(json);
                    var xml = o.Value<string>("xml");
                    var xdoc = XDocument.Parse(xml);

                    _ = new Core.Workflow(
                             WexflowServer.WexflowEngine
                          , 1
                          , []
                          , "-1"
                          , xdoc.ToString()
                          , WexflowServer.WexflowEngine.TempFolder
                          , WexflowServer.WexflowEngine.TasksFolder
                          , WexflowServer.WexflowEngine.ApprovalFolder
                          , WexflowServer.WexflowEngine.XsdPath
                          , WexflowServer.WexflowEngine.Database
                          , WexflowServer.WexflowEngine.GlobalVariablesFile
                        );

                    await WriteTrue(context);

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
            _ = _endpoints.MapPost(GetPattern("save-xml"), async context =>
            {
                try
                {
                    var username = context.User.Identity?.Name;

                    var json = await GetBodyAsync(context);
                    var res = false;

                    var o = JObject.Parse(json);
                    var workflowId = int.Parse((string)o.SelectToken("workflowId") ?? throw new InvalidOperationException());
                    var path = (string)o.SelectToken("filePath");
                    var xml = (string)o.SelectToken("xml") ?? throw new InvalidOperationException();

                    var idFromXml = WexflowServer.WexflowEngine.GetWorkflowId(xml);

                    if (idFromXml == -1)
                    {
                        await context.Response.WriteAsync(
                            JsonConvert.SerializeObject(
                            new SaveResult
                            {
                                FilePath = path,
                                Result = false,
                                WrongWorkflowId = false,
                                WrongXml = true,
                            }));
                        return;
                    }

                    if (idFromXml != workflowId)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new SaveResult { FilePath = path, Result = false, WrongWorkflowId = true }));
                        return;
                    }

                    var user = WexflowServer.WexflowEngine.GetUser(username);

                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        var id = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xml, true);
                        res = id != "-1";
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflow = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(w => w.Id == workflowId);

                        if (workflow != null)
                        {
                            var workflowDbId = workflow.DbId;
                            var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                            if (check)
                            {
                                var id = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xml, true);
                                res = id != "-1";
                            }
                        }
                        else
                        {
                            var id = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xml, true);
                            res = id != "-1";
                        }
                    }


                    if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                    {
                        if (res)
                        {
                            if (string.IsNullOrEmpty(path))
                            {
                                path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, $"Workflow_{workflowId}.xml");
                                WexflowServer.WexflowEngine.GetWorkflow(workflowId).FilePath = path;
                            }
                            var xdoc = XDocument.Parse(xml);
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

        private static XElement JsonNodeToXmlNode(JToken node)
        {
            var ifId = (int?)node.SelectToken("IfId");
            var whileId = (int?)node.SelectToken("WhileId");
            var switchId = (int?)node.SelectToken("SwitchId");
            if (ifId != null)
            {
                var xif = new XElement(Xn + "If", new XAttribute("id", (int)node.SelectToken("Id")),
                    new XAttribute("parent", (int)node.SelectToken("ParentId")),
                    new XAttribute("if", ifId));

                var xdo = new XElement(Xn + "Do");
                var doNodes = (JArray)node.SelectToken("DoNodes");
                if (doNodes != null)
                {
                    foreach (var doNode in doNodes)
                    {
                        xdo.Add(JsonNodeToXmlNode(doNode));
                    }
                }
                xif.Add(xdo);

                var xelse = new XElement(Xn + "Else");
                var elseNodesToken = node.SelectToken("ElseNodes");
                if (elseNodesToken != null && elseNodesToken.HasValues)
                {
                    var elseNodes = (JArray)elseNodesToken;
                    foreach (var elseNode in elseNodes)
                    {
                        xelse.Add(JsonNodeToXmlNode(elseNode));
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
                var xwhile = new XElement(Xn + "While", new XAttribute("id", (int)node.SelectToken("Id")),
                    new XAttribute("parent", (int)node.SelectToken("ParentId")),
                    new XAttribute("while", whileId));

                var doNodes = (JArray)node.SelectToken("Nodes");
                if (doNodes != null)
                {
                    foreach (var doNode in doNodes)
                    {
                        xwhile.Add(JsonNodeToXmlNode(doNode));
                    }
                }

                return xwhile;
            }
            else if (switchId != null)
            {
                var xswitch = new XElement(Xn + "Switch", new XAttribute("id", (int)node.SelectToken("Id")),
                    new XAttribute("parent", (int)node.SelectToken("ParentId")),
                    new XAttribute("switch", switchId));

                var cases = (JArray)node.SelectToken("Cases") ?? throw new InvalidOperationException();
                foreach (var @case in cases)
                {
                    var value = (string)@case.SelectToken("Value") ?? throw new InvalidOperationException();

                    var xcase = new XElement(Xn + "Case", new XAttribute("value", value));

                    var doNodes = (JArray)@case.SelectToken("Nodes");
                    if (doNodes != null)
                    {
                        foreach (var doNode in doNodes)
                        {
                            xcase.Add(JsonNodeToXmlNode(doNode));
                        }
                    }

                    xswitch.Add(xcase);
                }

                var @default = (JArray)node.SelectToken("Default");
                if (@default != null && @default.Count > 0)
                {
                    var xdefault = new XElement(Xn + "Default");

                    foreach (var doNode in @default)
                    {
                        xdefault.Add(JsonNodeToXmlNode(doNode));
                    }

                    xswitch.Add(xdefault);
                }

                return xswitch;
            }
            else
            {
                var taskId = (int)node.SelectToken("Id");
                var parentId = (int)node.SelectToken("ParentId");
                var xtask = new XElement(Xn + "Task", new XAttribute("id", taskId),
                    new XElement(Xn + "Parent", new XAttribute("id", parentId)));
                return xtask;
            }
        }

        private static XElement GetExecutionGraph(JToken eg)
        {
            if (eg != null && eg.Any())
            {
                XElement xeg = new(Xn + "ExecutionGraph");
                var nodes = (JArray)eg.SelectToken("Nodes");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        xeg.Add(JsonNodeToXmlNode(node));
                    }
                }

                // OnSuccess
                var onSuccess = eg.SelectToken("OnSuccess");
                if (onSuccess != null && onSuccess.Any())
                {
                    XElement xEvent = new(Xn + "OnSuccess");
                    var doNodes = (JArray)onSuccess.SelectToken("Nodes");
                    foreach (var doNode in doNodes!)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                // OnWarning
                var onWarning = eg.SelectToken("OnWarning");
                if (onWarning != null && onWarning.Any())
                {
                    XElement xEvent = new(Xn + "OnWarning");
                    var doNodes = (JArray)onWarning.SelectToken("Nodes");
                    foreach (var doNode in doNodes!)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                // OnError
                var onError = eg.SelectToken("OnError");
                if (onError != null && onError.Any())
                {
                    XElement xEvent = new(Xn + "OnError");
                    var doNodes = (JArray)onError.SelectToken("Nodes");
                    foreach (var doNode in doNodes!)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                // OnRejected
                var onRejected = eg.SelectToken("OnRejected");
                if (onRejected != null && onRejected.Any())
                {
                    XElement xEvent = new(Xn + "OnRejected");
                    var doNodes = (JArray)onRejected.SelectToken("Nodes");
                    foreach (var doNode in doNodes!)
                    {
                        xEvent.Add(JsonNodeToXmlNode(doNode));
                    }
                    xeg.Add(xEvent);
                }

                return xeg;
            }

            return null;
        }

        private static async System.Threading.Tasks.Task<SaveResult> SaveJsonWorkflow(Core.Db.User user, string json)
        {
            var o = JObject.Parse(json);
            var wi = o.SelectToken("WorkflowInfo");
            var currentWorkflowId = (int)(wi ?? throw new InvalidOperationException("WorkflowInfo is null")).SelectToken("Id");
            var isNew = WexflowServer.WexflowEngine.Workflows.All(w => w.Id != currentWorkflowId);
            var path = string.Empty;

            if (isNew)
            {
                XDocument xdoc = new();

                var workflowId = (int)wi.SelectToken("Id");
                var workflowName = (string)wi.SelectToken("Name");
                var workflowLaunchType = (LaunchType)(int)wi.SelectToken("LaunchType");
                var p = (string)wi.SelectToken("Period");
                var workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                var cronExpression = (string)wi.SelectToken("CronExpression");

                if (workflowLaunchType == LaunchType.Cron && !WexflowEngine.IsCronExpressionValid(cronExpression))
                {
                    throw new Exception($"The cron expression '{cronExpression}' is not valid.");
                }

                var isWorkflowEnabled = (bool)wi.SelectToken("IsEnabled");
                var isWorkflowApproval = (bool)wi.SelectToken("IsApproval");
                var enableParallelJobs = (bool)wi.SelectToken("EnableParallelJobs");
                var workflowDesc = (string)wi.SelectToken("Description");

                var retryCount = (int)wi.SelectToken("RetryCount");
                var retryTimeout = (int)wi.SelectToken("RetryTimeout");

                // Local variables
                XElement xLocalVariables = new(Xn + "LocalVariables");
                var variables = wi.SelectToken("LocalVariables");
                if (variables != null)
                {
                    foreach (var variable in variables!)
                    {
                        var key = (string)variable.SelectToken("Key");
                        var value = (string)variable.SelectToken("Value");

                        XElement xVariable = new(Xn + "Variable"
                            , new XAttribute("name", key)
                            , new XAttribute("value", value)
                        );

                        xLocalVariables.Add(xVariable);
                    }
                }

                // tasks
                XElement xtasks = new(Xn + "Tasks");
                var tasks = o.SelectToken("Tasks");
                foreach (var task in tasks)
                {
                    var taskId = (int)task.SelectToken("Id");
                    var taskName = (string)task.SelectToken("Name");
                    var taskDesc = (string)task.SelectToken("Description");
                    var isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                    XElement xtask = new(Xn + "Task"
                        , new XAttribute("id", taskId)
                        , new XAttribute("name", taskName)
                        , new XAttribute("description", taskDesc)
                        , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                    );

                    var settings = task.SelectToken("Settings");
                    foreach (var setting in settings)
                    {
                        var settingName = (string)setting.SelectToken("Name");
                        var settingValue = (string)setting.SelectToken("Value");

                        XElement xsetting = new(Xn + "Setting"
                            , new XAttribute("name", settingName)
                        );

                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            xsetting.SetAttributeValue("value", settingValue);
                        }

                        if (settingName is "selectFiles" or "selectAttachments")
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

                        var attributes = setting.SelectToken("Attributes");
                        foreach (var attribute in attributes)
                        {
                            var attributeName = (string)attribute.SelectToken("Name");
                            var attributeValue = (string)attribute.SelectToken("Value");
                            xsetting.SetAttributeValue(attributeName, attributeValue);
                        }

                        if (settingName == "selectFiles" || settingName == "selectAttachments" || (settingName != "selectFiles" && settingName != "selectAttachments" && !string.IsNullOrEmpty(settingValue)))
                        {
                            xtask.Add(xsetting);
                        }
                    }

                    xtasks.Add(xtask);
                }

                // root
                XElement xwf = new(Xn + "Workflow"
                    , new XAttribute("id", workflowId)
                    , new XAttribute("name", workflowName)
                    , new XAttribute("description", workflowDesc)
                    , new XElement(Xn + "Settings"
                        , new XElement(Xn + "Setting"
                            , new XAttribute("name", "launchType")
                            , new XAttribute("value", workflowLaunchType.ToString().ToLower()))
                        , new XElement(Xn + "Setting"
                            , new XAttribute("name", "enabled")
                            , new XAttribute("value", isWorkflowEnabled.ToString().ToLower()))
                        , new XElement(Xn + "Setting"
                        , new XAttribute("name", "approval")
                        , new XAttribute("value", isWorkflowApproval.ToString().ToLower()))
                        , new XElement(Xn + "Setting"
                        , new XAttribute("name", "enableParallelJobs")
                        , new XAttribute("value", enableParallelJobs.ToString().ToLower()))
                        , new XElement(Xn + "Setting"
                            , new XAttribute("name", "retryCount")
                            , new XAttribute("value", retryCount))
                        , new XElement(Xn + "Setting"
                            , new XAttribute("name", "retryTimeout")
                            , new XAttribute("value", retryTimeout))
                    )
                    , xLocalVariables
                    , xtasks
                );

                if (workflowLaunchType == LaunchType.Periodic)
                {
                    xwf.Element(Xn + "Settings").Add(
                         new XElement(Xn + "Setting"
                            , new XAttribute("name", "period")
                            , new XAttribute("value", workflowPeriod.ToString(@"dd\.hh\:mm\:ss")))
                        );
                }

                if (workflowLaunchType == LaunchType.Cron)
                {
                    xwf.Element(Xn + "Settings").Add(
                         new XElement(Xn + "Setting"
                            , new XAttribute("name", "cronExpression")
                            , new XAttribute("value", cronExpression))
                        );
                }

                // Execution graph
                var eg = o.SelectToken("ExecutionGraph");
                var xeg = GetExecutionGraph(eg);
                if (xeg != null)
                {
                    xwf.Add(xeg);
                }

                xdoc.Add(xwf);
                var id = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);

                if (id == "-1")
                {
                    return new SaveResult { FilePath = path, Result = false };
                }

                if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                {
                    path = (string)wi.SelectToken("FilePath");
                    if (string.IsNullOrEmpty(path))
                    {
                        path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, $"Workflow_{workflowId}.xml");
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
                var wf = WexflowServer.WexflowEngine.GetWorkflow(currentWorkflowId);
                if (wf != null)
                {
                    var xdoc = wf.XDoc;

                    var workflowId = (int)wi.SelectToken("Id");
                    var workflowName = (string)wi.SelectToken("Name");
                    var workflowLaunchType = (LaunchType)(int)wi.SelectToken("LaunchType");
                    var p = (string)wi.SelectToken("Period");
                    var workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                    var cronExpression = (string)wi.SelectToken("CronExpression");

                    if (workflowLaunchType == LaunchType.Cron &&
                        !WexflowEngine.IsCronExpressionValid(cronExpression))
                    {
                        throw new Exception($"The cron expression '{cronExpression}' is not valid.");
                    }

                    var isWorkflowEnabled = (bool)wi.SelectToken("IsEnabled");
                    var isWorkflowApproval = (bool)(wi.SelectToken("IsApproval") ?? false);
                    var enableParallelJobs = (bool)(wi.SelectToken("EnableParallelJobs") ?? true);
                    var workflowDesc = (string)wi.SelectToken("Description");

                    var retryCount = (int)wi.SelectToken("RetryCount");
                    var retryTimeout = (int)wi.SelectToken("RetryTimeout");

                    if (xdoc.Root == null)
                    {
                        throw new InvalidOperationException("Root is null");
                    }

                    xdoc.Root.Attribute("id")!.Value = workflowId.ToString();
                    xdoc.Root.Attribute("name")!.Value = workflowName ?? throw new InvalidOperationException();
                    xdoc.Root.Attribute("description")!.Value = workflowDesc ?? string.Empty;

                    var xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                        wf.XmlNamespaceManager) ?? throw new InvalidOperationException();
                    xwfEnabled.Attribute("value")!.Value = isWorkflowEnabled.ToString().ToLower();
                    var xwfLaunchType = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='launchType']",
                        wf.XmlNamespaceManager) ?? throw new InvalidOperationException();
                    xwfLaunchType.Attribute("value")!.Value = workflowLaunchType.ToString().ToLower();

                    var xwfApproval = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='approval']",
                    wf.XmlNamespaceManager);
                    if (xwfApproval == null)
                    {
                        xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)!
                            .Add(new XElement(Xn + "Setting"
                                    , new XAttribute("name", "approval")
                                    , new XAttribute("value", isWorkflowApproval.ToString().ToLower())));
                    }
                    else
                    {
                        xwfApproval.Attribute("value")!.Value = isWorkflowApproval.ToString().ToLower();
                    }

                    var xwfEnableParallelJobs = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enableParallelJobs']",
                    wf.XmlNamespaceManager);
                    if (xwfEnableParallelJobs == null)
                    {
                        xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)!
                            .Add(new XElement(Xn + "Setting"
                                    , new XAttribute("name", "enableParallelJobs")
                                    , new XAttribute("value", enableParallelJobs.ToString().ToLower())));
                    }
                    else
                    {
                        xwfEnableParallelJobs.Attribute("value")!.Value = enableParallelJobs.ToString().ToLower();
                    }

                    var xwfPeriod = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='period']",
                        wf.XmlNamespaceManager);
                    if (workflowLaunchType == LaunchType.Periodic)
                    {
                        if (xwfPeriod != null)
                        {
                            xwfPeriod.Attribute("value")!.Value = workflowPeriod.ToString(@"dd\.hh\:mm\:ss");
                        }
                        else
                        {
                            xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)!
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

                    var xwfCronExpression = xdoc.Root.XPathSelectElement(
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

                    var xwfRetryCount = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='retryCount']",
                        wf.XmlNamespaceManager);
                    if (xwfRetryCount != null)
                    {
                        (xwfRetryCount.Attribute("value") ?? throw new InvalidOperationException()).Value = retryCount.ToString();
                    }
                    else
                    {
                        (xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager) ?? throw new InvalidOperationException())
                            .Add(new XElement(wf.XNamespaceWf + "Setting", new XAttribute("name", "retryCount"),
                                new XAttribute("value", retryCount)));
                    }

                    var xwfRetryTimeout = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='retryTimeout']",
                        wf.XmlNamespaceManager);
                    if (xwfRetryTimeout != null)
                    {
                        (xwfRetryTimeout.Attribute("value") ?? throw new InvalidOperationException()).Value = retryTimeout.ToString();
                    }
                    else
                    {
                        (xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager) ?? throw new InvalidOperationException())
                            .Add(new XElement(wf.XNamespaceWf + "Setting", new XAttribute("name", "retryTimeout"),
                                new XAttribute("value", retryTimeout)));
                    }

                    // Local variables
                    var xLocalVariables = xdoc.Root.Element(wf.XNamespaceWf + "LocalVariables");
                    if (xLocalVariables != null)
                    {
                        var allVariables = xLocalVariables.Elements(wf.XNamespaceWf + "Variable");
                        allVariables.Remove();
                    }
                    else
                    {
                        xLocalVariables = new XElement(wf.XNamespaceWf + "LocalVariables");
                        xdoc.Root.Element(wf.XNamespaceWf + "Tasks").AddBeforeSelf(xLocalVariables);
                    }

                    var variables = wi.SelectToken("LocalVariables");
                    if (variables != null)
                    {
                        foreach (var variable in variables)
                        {
                            var key = (string)variable.SelectToken("Key");
                            var value = (string)variable.SelectToken("Value");

                            XElement xVariable = new(wf.XNamespaceWf + "Variable"
                                , new XAttribute("name", key)
                                , new XAttribute("value", value)
                            );

                            xLocalVariables.Add(xVariable);
                        }
                    }

                    var xtasks = xdoc.Root.Element(wf.XNamespaceWf + "Tasks");
                    var alltasks = xtasks.Elements(wf.XNamespaceWf + "Task");
                    alltasks.Remove();

                    var tasks = o.SelectToken("Tasks");
                    foreach (var task in tasks)
                    {
                        var taskId = (int)task.SelectToken("Id");
                        var taskName = (string)task.SelectToken("Name");
                        var taskDesc = (string)task.SelectToken("Description");
                        var isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                        XElement xtask = new(wf.XNamespaceWf + "Task"
                            , new XAttribute("id", taskId)
                            , new XAttribute("name", taskName)
                            , new XAttribute("description", taskDesc)
                            , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                        );

                        var settings = task.SelectToken("Settings");
                        foreach (var setting in settings)
                        {
                            var settingName = (string)setting.SelectToken("Name");
                            var settingValue = (string)setting.SelectToken("Value");

                            XElement xsetting = new(wf.XNamespaceWf + "Setting"
                                , new XAttribute("name", settingName)
                            );

                            if (settingName is "selectFiles" or "selectAttachments")
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

                            var attributes = setting.SelectToken("Attributes");
                            foreach (var attribute in attributes)
                            {
                                var attributeName = (string)attribute.SelectToken("Name");
                                var attributeValue = (string)attribute.SelectToken("Value");
                                xsetting.SetAttributeValue(attributeName, attributeValue);
                            }

                            if (settingName == "selectFiles" || settingName == "selectAttachments" || (settingName != "selectFiles" && settingName != "selectAttachments" && !string.IsNullOrEmpty(settingValue)))
                            {
                                xtask.Add(xsetting);
                            }
                        }

                        xtasks.Add(xtask);
                    }

                    // ExecutionGraph
                    var xExecutionGraph = xdoc.Root.XPathSelectElement(
                        "wf:ExecutionGraph",
                        wf.XmlNamespaceManager);

                    xExecutionGraph?.Remove();

                    var eg = o.SelectToken("ExecutionGraph");
                    var xeg = GetExecutionGraph(eg);
                    if (xeg != null)
                    {
                        xdoc.Root.Add(xeg);
                    }

                    var qid = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);
                    if (qid == "-1")
                    {
                        return new SaveResult { FilePath = path, Result = false };
                    }

                    if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                    {
                        path = (string)wi.SelectToken("FilePath");
                        if (string.IsNullOrEmpty(path))
                        {
                            path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, $"Workflow_{workflowId}.xml");
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
            _ = _endpoints.MapPost(GetPattern("save"), async context =>
            {
                try
                {
                    var json = await GetBodyAsync(context);

                    var o = JObject.Parse(json);
                    var wi = o.SelectToken("WorkflowInfo") ?? throw new InvalidOperationException();
                    var currentWorkflowId = (int)wi.SelectToken("Id");
                    var isNew = WexflowServer.WexflowEngine.Workflows.All(w => w.Id != currentWorkflowId);

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);

                    if (user.UserProfile == Core.Db.UserProfile.Restricted)
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Administrator && !isNew)
                    {
                        var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == currentWorkflowId).DbId;
                        var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (!check)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    var res = await SaveJsonWorkflow(user, json);

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
            _ = _endpoints.MapPost(GetPattern("disable/{id}"), async context =>
            {
                try
                {
                    var workflowId = int.Parse(context.Request.RouteValues["id"]?.ToString()!);
                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    var wf = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(w => w.Id == workflowId);
                    var res = false;

                    if (user.UserProfile == Core.Db.UserProfile.Restricted)
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Administrator && wf != null)
                    {
                        var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (!check)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    if (wf != null)
                    {
                        var xdoc = wf.XDoc;
                        if (xdoc.Root is null)
                        {
                            throw new InvalidOperationException(" xdoc.Root is null");
                        }

                        var xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                        wf.XmlNamespaceManager) ?? throw new InvalidOperationException();
                        xwfEnabled.Attribute("value")!.Value = false.ToString().ToLower();
                        var qid = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);

                        if (qid != "-1")
                        {
                            res = true;
                        }
                    }
                    else
                    {
                        NotFound(context);
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
            _ = _endpoints.MapPost(GetPattern("enable/{id}"), async context =>
            {
                try
                {
                    var workflowId = int.Parse(context.Request.RouteValues["id"]?.ToString()!);
                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    var wf = WexflowServer.WexflowEngine.Workflows.FirstOrDefault(w => w.Id == workflowId);
                    var res = false;

                    if (user.UserProfile == Core.Db.UserProfile.Restricted)
                    {
                        await WriteFalse(context);
                        return;
                    }

                    if (user.UserProfile == Core.Db.UserProfile.Administrator && wf != null)
                    {
                        var workflowDbId = WexflowServer.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowDbId);
                        if (!check)
                        {
                            await WriteFalse(context);
                            return;
                        }
                    }

                    if (wf != null)
                    {
                        var xdoc = wf.XDoc;
                        if (xdoc.Root is null)
                        {
                            throw new InvalidOperationException("xdoc.Root is null");
                        }

                        var xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                        wf.XmlNamespaceManager) ?? throw new InvalidOperationException();
                        xwfEnabled.Attribute("value")!.Value = true.ToString().ToLower();
                        var qid = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, xdoc.ToString(), true);

                        if (qid != "-1")
                        {
                            res = true;
                        }
                    }
                    else
                    {
                        NotFound(context);
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
            _ = _endpoints.MapPost(GetPattern("upload"), async context =>
            {
                try
                {
                    var res = true;
                    SaveResult ressr = new() { FilePath = string.Empty, Result = false };
                    var username = context.User.Identity?.Name;

                    var file = context.Request.Form.Files.Single();
                    var fileName = file.FileName;
                    MemoryStream ms = new();
                    await file.CopyToAsync(ms);
                    var fileValue = Encoding.UTF8.GetString(ms.ToArray());

                    var index = fileValue.IndexOf('<');
                    if (index > 0)
                    {
                        fileValue = fileValue[index..];
                    }

                    int workflowId;
                    var extension = Path.GetExtension(fileName).ToLower();
                    var isXml = extension == ".xml";
                    if (isXml) // xml
                    {
                        XNamespace xn = "urn:wexflow-schema";
                        var xdoc = XDocument.Parse(fileValue);
                        workflowId = int.Parse(xdoc.Element(xn + "Workflow")?.Attribute("id")!.Value ?? throw new InvalidOperationException());
                    }
                    else // json
                    {
                        var o = JObject.Parse(fileValue);
                        var wi = o.SelectToken("WorkflowInfo") ?? throw new InvalidOperationException();
                        workflowId = (int)wi.SelectToken("Id");
                    }

                    var isAuthorized = false;
                    var user = WexflowServer.WexflowEngine.GetUser(username);

                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        isAuthorized = true;
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), workflowId.ToString());
                        if (check)
                        {
                            isAuthorized = true;
                        }
                    }


                    // if extension is xml then XML else JSON
                    if (isAuthorized)
                    {
                        if (isXml)
                        {
                            var id = await WexflowServer.WexflowEngine.SaveWorkflow(user.GetDbId(), user.UserProfile, fileValue, true);
                            res = id != "-1";

                            if (WexflowServer.WexflowEngine.EnableWorkflowsHotFolder)
                            {
                                if (res)
                                {
                                    var path = Path.Combine(WexflowServer.WexflowEngine.WorkflowsFolder, fileName);
                                    var xdoc = XDocument.Parse(fileValue);
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
                            ressr = await SaveJsonWorkflow(user, fileValue);
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
            _ = _endpoints.MapPost(GetPattern("delete"), async context =>
            {
                try
                {
                    var res = false;

                    var username = context.User.Identity?.Name;

                    var workflowId = int.Parse(context.Request.Query["w"].ToString());
                    var wf = WexflowServer.WexflowEngine.GetWorkflow(workflowId);

                    if (wf != null)
                    {
                        var user = WexflowServer.WexflowEngine.GetUser(username);


                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            WexflowServer.WexflowEngine.DeleteWorkflow(wf.DbId);
                            res = true;
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), wf.DbId);
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
                        NotFound(context);
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
            _ = _endpoints.MapGet(GetPattern("graph/{id}"), async context =>
            {


                var wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException()));
                if (wf != null)
                {
                    IList<Node> nodes = [];

                    foreach (var node in wf.ExecutionGraph.Nodes)
                    {
                        var task = wf.Tasks.FirstOrDefault(t => t.Id == node.Id);
                        var nodeName = $"Task {node.Id}{(task != null ? ": " + task.Description : "")}";

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

                        var nodeId = $"n{node.Id}";
                        var parentId = $"n{node.ParentId}";

                        nodes.Add(new Node(nodeId, nodeName, parentId));
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(nodes));
                }

            });
        }

        /// <summary>
        /// Returns the execution graph of the workflow.
        /// </summary>
        private void GetExecutionGraphAsXml()
        {
            _ = _endpoints.MapGet(GetPattern("graph-xml/{id}"), async context =>
            {


                var graph = "<ExecutionGraph />";


                var wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException()));
                if (wf != null)
                {
                    var xgraph = wf.XDoc.Descendants(wf.XNamespaceWf + "ExecutionGraph").FirstOrDefault();
                    if (xgraph != null)
                    {
                        var res = MyRegex().Replace(xgraph.ToString().Replace(" xmlns=\"urn:wexflow-schema\"", string.Empty), "\t");
                        StringBuilder builder = new();
                        var lines = res.Split('\n');
                        for (var i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i];
                            _ = i < lines.Length - 1 ? builder.Append('\t').Append(line).Append('\n') : builder.Append('\t').Append(line);
                        }
                        graph = builder.ToString();
                    }
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(graph));

            });
        }

        /// <summary>
        /// Returns the execution graph of the workflow.
        /// </summary>
        private void GetExecutionGraphAsBlockly()
        {
            _ = _endpoints.MapGet(GetPattern("graph-blockly/{id}"), async context =>
            {
                var username = context.User.Identity?.Name;

                var graph = "<xml />";


                var wf = WexflowServer.WexflowEngine.GetWorkflow(int.Parse(context.Request.RouteValues["id"]?.ToString() ?? throw new InvalidOperationException()));
                if (wf != null)
                {
                    if (wf.ExecutionGraph != null)
                    {
                        var xml = ExecutionGraphToBlockly(wf.ExecutionGraph);
                        if (xml != null)
                        {
                            graph = xml.ToString();
                        }
                    }
                    else
                    {
                        List<Core.ExecutionGraph.Node> nodes = [];
                        for (var i = 0; i < wf.Tasks.Length; i++)
                        {
                            var task = wf.Tasks[i];
                            nodes.Add(i == 0
                                ? new Core.ExecutionGraph.Node(task.Id, -1)
                                : new Core.ExecutionGraph.Node(task.Id, wf.Tasks[i - 1].Id));
                        }

                        Core.ExecutionGraph.Graph sgraph = new(nodes, null, null, null, null);
                        var xml = ExecutionGraphToBlockly(sgraph);
                        if (xml != null)
                        {
                            graph = xml.ToString();
                        }
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(graph));
                }
                else
                {
                    NotFound(context);
                }

            });
        }

        private XElement ExecutionGraphToBlockly(Core.ExecutionGraph.Graph graph)
        {
            if (graph != null)
            {
                var nodes = graph.Nodes;
                XElement xml = new("xml");
                var startNode = GetStartupNode(nodes);
                var depth = 0;
                var block = NodeToBlockly(graph, startNode, nodes, startNode is If or While or Switch, false, ref depth);
                xml.Add(block);
                return xml;
            }

            return null;
        }

        private XElement NodeToBlockly(Core.ExecutionGraph.Graph graph, Core.ExecutionGraph.Node node, Core.ExecutionGraph.Node[] nodes, bool isFlowchart, bool isEvent, ref int depth)
        {
            var block = new XElement("block");

            if (nodes.Length != 0)
            {
                if (node is If)
                {
                    if (node is If @if)
                    {
                        block.Add(new XAttribute("type", "if"), new XElement("field", new XAttribute("name", "IF"), @if.IfId));
                        var @do = new XElement("statement", new XAttribute("name", "DO"));
                        @do.Add(NodeToBlockly(graph, GetStartupNode(@if.DoNodes), @if.DoNodes, true, isEvent, ref depth));
                        block.Add(@do);
                        if (@if.ElseNodes != null && @if.ElseNodes.Length > 0)
                        {
                            var @else = new XElement("statement", new XAttribute("name", "ELSE"));
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
                        var @do = new XElement("statement", new XAttribute("name", "DO"));
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
                        var @case = new XElement("statement", new XAttribute("name", "CASE"));
                        if (@switch.Cases.Length > 0)
                        {
                            var xcases = SwitchCasesToBlockly(graph, @switch.Cases[0], 0, @switch.Cases.Length > 1 ? @switch.Cases[1] : null, @switch, true, isEvent, ref depth);
                            @case.Add(xcases);
                        }
                        block.Add(@case);
                        if (@switch.Default != null && @switch.Default.Length > 0)
                        {
                            var @default = new XElement("statement", new XAttribute("name", "DEFAULT"));
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

                var childNode = nodes.FirstOrDefault(n => n.ParentId == node.Id);
                if (childNode != null)
                {
                    block.Add(new XElement("next", NodeToBlockly(graph, childNode, nodes, isFlowchart, isEvent, ref depth)));
                }
                else if (!isFlowchart && !isEvent)
                {
                    var lastNode = graph.Nodes.LastOrDefault();

                    if (lastNode?.Id == node.Id && lastNode?.Depth == node.Depth)
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
            }

            return block.Attribute("type") == null ? null : block;
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

        private static Core.ExecutionGraph.Node GetStartupNode(IEnumerable<Core.ExecutionGraph.Node> nodes)
            => nodes.FirstOrDefault(n => n.ParentId == Core.Workflow.START_ID);

        /// <summary>
        /// Returns status count.
        /// </summary>
        private void GetStatusCount()
        {
            _ = _endpoints.MapGet(GetPattern("status-count"), async context =>
            {

                var statusCount = WexflowServer.WexflowEngine.GetStatusCount();
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

            });
        }

        /// <summary>
        /// Returns a user from his username.
        /// </summary>
        private void GetUser()
        {
            _ = _endpoints.MapGet(GetPattern("user"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var username = context.Request.Query["username"].ToString();

                var othuser = WexflowServer.WexflowEngine.GetUser(qusername);

                if (othuser != null)
                {
                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    var dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

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
            _ = _endpoints.MapGet(GetPattern("search-users"), async context =>
            {
                var qusername = context.User.Identity?.Name;
                var keyword = context.Request.Query["keyword"].ToString();
                var uo = int.Parse(context.Request.Query["uo"].ToString());

                var q = Array.Empty<User>();
                var user = WexflowServer.WexflowEngine.GetUser(qusername);

                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var users = WexflowServer.WexflowEngine.GetUsers(keyword, (UserOrderBy)uo);

                    var dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

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
            _ = _endpoints.MapGet(GetPattern("non-restricted-users"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var q = Array.Empty<User>();
                var user = WexflowServer.WexflowEngine.GetUser(qusername);

                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var users = WexflowServer.WexflowEngine.GetNonRestrictedUsers();

                    var dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

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
            _ = _endpoints.MapGet(GetPattern("search-admins"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var keyword = context.Request.Query["keyword"].ToString();
                var uo = int.Parse(context.Request.Query["uo"].ToString());

                var q = Array.Empty<User>();

                var user = WexflowServer.WexflowEngine.GetUser(qusername);

                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var users = WexflowServer.WexflowEngine.GetAdministrators(keyword, (UserOrderBy)uo);
                    var dateTimeFormat = WexflowServer.Config["DateTimeFormat"];

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
            _ = _endpoints.MapPost(GetPattern("save-user-workflows"), async context =>
            {
                try
                {
                    var qusername = context.User.Identity?.Name;

                    var json = await GetBodyAsync(context);

                    var res = false;
                    var o = JObject.Parse(json);

                    var user = WexflowServer.WexflowEngine.GetUser(qusername);

                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        var userId = o.Value<string>("UserId");
                        var jArray = o.Value<JArray>("UserWorkflows");
                        WexflowServer.WexflowEngine.DeleteUserWorkflowRelations(userId);
                        foreach (var item in jArray.Cast<JObject>())
                        {
                            var workflowId = item.Value<string>("WorkflowId");
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
            _ = _endpoints.MapGet(GetPattern("user-workflows"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var userId = context.Request.Query["u"].ToString();

                var res = Array.Empty<WorkflowInfo>();

                var user = WexflowServer.WexflowEngine.GetUser(qusername);

                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    try
                    {
                        var workflows = WexflowServer.WexflowEngine.GetUserWorkflows(userId);
                        res = workflows
                            .ToList()
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.InstanceId, wf.Name, wf.FilePath,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.EnableParallelJobs, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss")
                            , wf.CronExpression
                            , wf.IsExecutionGraphEmpty
                            , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                            , wf.StartedOn.ToString(WexflowServer.Config["DateTimeFormat"])
                            , wf.RetryCount
                            , wf.RetryTimeout
                            , wf.JobStatus
                           )).ToArray();
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
            _ = _endpoints.MapPost(GetPattern("insert-user"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var username = context.Request.Query["username"].ToString();
                var password = context.Request.Query["password"].ToString();
                var userProfile = int.Parse(context.Request.Query["up"].ToString());
                var email = context.Request.Query["email"].ToString();

                try
                {
                    var res = false;
                    var user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
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
            _ = _endpoints.MapPost(GetPattern("update-user"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var userId = context.Request.Query["userId"].ToString();
                var username = context.Request.Query["username"].ToString();
                var password = context.Request.Query["password"].ToString();
                var userProfile = int.Parse(context.Request.Query["up"].ToString());
                var email = context.Request.Query["email"].ToString();

                try
                {
                    var res = false;
                    var user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
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
            _ = _endpoints.MapPost(GetPattern("update-username-email-user-profile"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var userId = context.Request.Query["userId"].ToString();
                var username = context.Request.Query["username"].ToString();
                var email = context.Request.Query["email"].ToString();
                var up = int.Parse(context.Request.Query["up"].ToString());

                try
                {
                    var res = false;
                    var user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
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
            _ = _endpoints.MapPost(GetPattern("delete-user"), async context =>
            {
                var qusername = context.User.Identity?.Name;

                var username = context.Request.Query["username"].ToString();
                var password = context.Request.Query["password"].ToString();

                try
                {
                    var res = false;
                    var user = WexflowServer.WexflowEngine.GetUser(qusername);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
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
            _ = _endpoints.MapPost(GetPattern("reset-password"), async context =>
            {
                var username = context.Request.Query["u"].ToString();

                var user = WexflowServer.WexflowEngine.GetUser(username);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        var newPassword = $"wexflow{GenerateRandomNumber()}";
                        var newPasswordHash = Db.HashPassword(newPassword);

                        // Send email
                        var subject = $"Wexflow - Password reset of user {username}";
                        var body = $"Your new password is: {newPassword}";

                        var host = WexflowServer.Config["Smtp.Host"];
                        var port = int.Parse(WexflowServer.Config["Smtp.Port"] ?? throw new InvalidOperationException("Smtp.Port setting not found"));
                        var enableSsl = bool.Parse(WexflowServer.Config["Smtp.EnableSsl"] ?? throw new InvalidOperationException("Smtp.EnableSsl setting not found"));
                        var smtpUser = WexflowServer.Config["Smtp.User"];
                        var smtpPassword = WexflowServer.Config["Smtp.Password"];
                        var from = WexflowServer.Config["Smtp.From"];

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
        private static int GenerateRandomNumber()
        {
            const int min = 1000;
            const int max = 9999;
            Random rdm = new();
            return rdm.Next(min, max);
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
        private static void Send(string host, int port, bool enableSsl, string user, string password, string to, string from, string subject, string body)
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
            _ = _endpoints.MapGet(GetPattern("search-history-entries-by-page-order-by"), async context =>
            {
                var keyword = context.Request.Query["s"].ToString();
                var from = double.Parse(context.Request.Query["from"].ToString());
                var to = double.Parse(context.Request.Query["to"].ToString());
                var page = int.Parse(context.Request.Query["page"].ToString());
                var entriesCount = int.Parse(context.Request.Query["entriesCount"].ToString());
                var heo = int.Parse(context.Request.Query["heo"].ToString());

                DateTime baseDate = new(1970, 1, 1);
                var fromDate = baseDate.AddMilliseconds(from);
                var toDate = baseDate.AddMilliseconds(to);

                var entries = WexflowServer.WexflowEngine.GetHistoryEntries(keyword, fromDate, toDate, page,
                    entriesCount, (EntryOrderBy)heo);

                var q = entries.Select(e =>
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

            });
        }

        /// <summary>
        /// Searches for entries.
        /// </summary>
        private void SearchEntriesByPageOrderBy()
        {
            _ = _endpoints.MapGet(GetPattern("search-entries-by-page-order-by"), async context =>
            {

                var keyword = context.Request.Query["s"].ToString();
                var from = double.Parse(context.Request.Query["from"].ToString());
                var to = double.Parse(context.Request.Query["to"].ToString());
                var page = int.Parse(context.Request.Query["page"].ToString());
                var entriesCount = int.Parse(context.Request.Query["entriesCount"].ToString());
                var heo = int.Parse(context.Request.Query["heo"].ToString());

                DateTime baseDate = new(1970, 1, 1);
                var fromDate = baseDate.AddMilliseconds(from);
                var toDate = baseDate.AddMilliseconds(to);

                var entries = WexflowServer.WexflowEngine.GetEntries(keyword, fromDate, toDate, page, entriesCount, (EntryOrderBy)heo);

                var q = entries.Select(e =>
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

            });
        }

        /// <summary>
        /// Returns history entries count by keyword and date filter.
        /// </summary>
        private void GetHistoryEntriesCountByDate()
        {
            _ = _endpoints.MapGet(GetPattern("history-entries-count-by-date"), async context =>
            {
                var keyword = context.Request.Query["s"].ToString();
                var from = double.Parse(context.Request.Query["from"].ToString());
                var to = double.Parse(context.Request.Query["to"].ToString());

                DateTime baseDate = new(1970, 1, 1);
                var fromDate = baseDate.AddMilliseconds(from);
                var toDate = baseDate.AddMilliseconds(to);
                var count = WexflowServer.WexflowEngine.GetHistoryEntriesCount(keyword, fromDate, toDate);

                await context.Response.WriteAsync(JsonConvert.SerializeObject(count));

            });
        }

        /// <summary>
        /// Returns entries count by keyword and date filter.
        /// </summary>
        private void GetEntriesCountByDate()
        {
            _ = _endpoints.MapGet(GetPattern("entries-count-by-date"), async context =>
            {

                var keyword = context.Request.Query["s"].ToString();
                var from = double.Parse(context.Request.Query["from"].ToString());
                var to = double.Parse(context.Request.Query["to"].ToString());

                DateTime baseDate = new(1970, 1, 1);
                var fromDate = baseDate.AddMilliseconds(from);
                var toDate = baseDate.AddMilliseconds(to);
                var count = WexflowServer.WexflowEngine.GetEntriesCount(keyword, fromDate, toDate);

                await context.Response.WriteAsync(JsonConvert.SerializeObject(count));

            });
        }

        /// <summary>
        /// Returns history entry min date.
        /// </summary>
        private void GetHistoryEntryStatusDateMin()
        {
            _ = _endpoints.MapGet(GetPattern("history-entry-status-date-min"), async context =>
            {

                var date = WexflowServer.WexflowEngine.GetHistoryEntryStatusDateMin();
                DateTime baseDate = new(1970, 1, 1);
                var d = (date - baseDate).TotalMilliseconds;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(d));

            });
        }

        /// <summary>
        /// Returns history entry max date.
        /// </summary>
        private void GetHistoryEntryStatusDateMax()
        {
            _ = _endpoints.MapGet(GetPattern("history-entry-status-date-max"), async context =>
            {

                var date = WexflowServer.WexflowEngine.GetHistoryEntryStatusDateMax();
                DateTime baseDate = new(1970, 1, 1);
                var d = (date - baseDate).TotalMilliseconds;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(d));

            });
        }

        /// <summary>
        /// Returns entry min date.
        /// </summary>
        private void GetEntryStatusDateMin()
        {
            _ = _endpoints.MapGet(GetPattern("entry-status-date-min"), async context =>
            {

                var date = WexflowServer.WexflowEngine.GetEntryStatusDateMin();
                DateTime baseDate = new(1970, 1, 1);
                var d = (date - baseDate).TotalMilliseconds;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(d));

            });
        }

        /// <summary>
        /// Returns entry max date.
        /// </summary>
        private void GetEntryStatusDateMax()
        {
            _ = _endpoints.MapGet(GetPattern("entry-status-date-max"), async context =>
            {

                var date = WexflowServer.WexflowEngine.GetEntryStatusDateMax();
                DateTime baseDate = new(1970, 1, 1);
                var d = (date - baseDate).TotalMilliseconds;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(d));

            });
        }

        /// <summary>
        /// Deletes workflows.
        /// </summary>
        private void DeleteWorkflows()
        {
            _ = _endpoints.MapPost(GetPattern("delete-workflows"), async context =>
            {
                try
                {
                    var json = await GetBodyAsync(context);

                    var res = false;

                    var o = JObject.Parse(json);
                    var workflowDbIds = JsonConvert.DeserializeObject<string[]>(((JArray)o.SelectToken("WorkflowsToDelete"))!.ToString());

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);

                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = WexflowServer.WexflowEngine.DeleteWorkflows(workflowDbIds);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var tres = true;
                        foreach (var id in workflowDbIds)
                        {
                            var check = WexflowServer.WexflowEngine.CheckUserWorkflow(user.GetDbId(), id);
                            if (check)
                            {
                                try
                                {
                                    WexflowServer.WexflowEngine.DeleteWorkflow(id);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    tres = false;
                                }
                            }
                        }
                        res = tres;
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
        /// Returns entry logs.
        /// </summary>
        private void GetEntryLogs()
        {
            _ = _endpoints.MapGet(GetPattern("entry-logs"), async context =>
            {
                try
                {
                    var entryId = context.Request.Query["id"].ToString();
                    var res = string.Empty;
                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);

                    res = WexflowServer.WexflowEngine.GetEntryLogs(entryId);


                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    await Error(context, e);
                }
            });
        }

        /// <summary>
        /// Returns an entry from workflowId and jobId.
        /// </summary>
        private void GetEntry()
        {
            _ = _endpoints.MapGet(GetPattern("entry"), async context =>
            {
                try
                {
                    var workflowId = int.Parse(context.Request.Query["w"].ToString());
                    var jobIdStr = context.Request.Query["i"].ToString();
                    Contracts.Entry res = null;

                    Core.Db.Entry e = null;

                    if (!string.IsNullOrEmpty(jobIdStr))
                    {
                        var jobId = Guid.Parse(jobIdStr);
                        e = WexflowServer.WexflowEngine.GetEntry(workflowId, jobId);
                    }
                    else
                    {
                        e = WexflowServer.WexflowEngine.GetEntry(workflowId);
                    }

                    if (e != null)
                    {
                        res = new Contracts.Entry
                        {
                            Id = e.GetDbId(),
                            WorkflowId = e.WorkflowId,
                            Name = e.Name,
                            LaunchType = (LaunchType)(int)e.LaunchType,
                            Description = e.Description,
                            Status = (Contracts.Status)(int)e.Status,
                            //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                            StatusDate = e.StatusDate.ToString(WexflowServer.Config["DateTimeFormat"])
                        };
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
            _ = _endpoints.MapGet(GetPattern("history-entry-logs"), async context =>
            {
                try
                {
                    var entryId = context.Request.Query["id"].ToString();

                    var res = WexflowServer.WexflowEngine.GetHistoryEntryLogs(entryId);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(res));

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
        private static string GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                string[] sizes = ["B", "KB", "MB", "GB", "TB"];
                double len = new FileInfo(filePath).Length;
                var order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }

                // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                // show a single decimal place, and no space.
                var result = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0.##} {1}", len, sizes[order]);

                return result;
            }

            return string.Empty;
        }

        /// <summary>
        /// Uploads a version.
        /// </summary>
        private void UploadVersion()
        {
            _ = _endpoints.MapPost(GetPattern("upload-version"), async context =>
            {
                try
                {
                    SaveResult ressr = new() { FilePath = string.Empty, FileName = string.Empty, Result = false };
                    var username = context.User.Identity?.Name;

                    var file = context.Request.Form.Files.Single();
                    var fileName = file.FileName;
                    MemoryStream ms = new();
                    await file.CopyToAsync(ms);

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var recordId = context.Request.Query["r"].ToString();
                        var guid = Guid.NewGuid().ToString();
                        var dir = Path.Combine(WexflowServer.WexflowEngine.RecordsTempFolder, WexflowServer.WexflowEngine.DbFolderName, recordId, guid);
                        if (!Directory.Exists(dir))
                        {
                            _ = Directory.CreateDirectory(dir);
                        }
                        var filePath = Path.Combine(dir, fileName);
                        await File.WriteAllBytesAsync(filePath, ms.ToArray());
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
            _ = _endpoints.MapGet(GetPattern("download-file"), async context =>
            {
                try
                {
                    var path = context.Request.Query["p"].ToString();
                    var fileName = Path.GetFileName(path);
                    context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
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
            _ = _endpoints.MapPost(GetPattern("delete-temp-version-file"), async context =>
            {
                try
                {
                    var res = false;

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var path = context.Request.Query["p"].ToString();

                        if (path.Contains(WexflowServer.WexflowEngine.RecordsTempFolder))
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                                res = true;

                                var parentDir = Path.GetDirectoryName(path) ?? throw new InvalidOperationException();
                                if (WexflowEngine.IsDirectoryEmpty(parentDir))
                                {
                                    Directory.Delete(parentDir);
                                    var recordTempDir = (Directory.GetParent(parentDir) ?? throw new InvalidOperationException()).FullName;
                                    if (WexflowEngine.IsDirectoryEmpty(recordTempDir))
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
            _ = _endpoints.MapPost(GetPattern("delete-temp-version-files"), async context =>
            {
                try
                {
                    var res = true;

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var json = await GetBodyAsync(context);
                        var o = JObject.Parse(json);
                        var versions = JsonConvert.DeserializeObject<Contracts.Version[]>(o.Value<JArray>("Versions").ToString());

                        foreach (var version in versions)
                        {
                            var path = version.FilePath;

                            try
                            {
                                if (path.Contains(WexflowServer.WexflowEngine.RecordsTempFolder))
                                {
                                    if (File.Exists(path))
                                    {
                                        File.Delete(path);

                                        var parentDir = Path.GetDirectoryName(path) ?? throw new InvalidOperationException();
                                        if (WexflowEngine.IsDirectoryEmpty(parentDir))
                                        {
                                            Directory.Delete(parentDir);
                                            var recordTempDir = (Directory.GetParent(parentDir) ?? throw new InvalidOperationException()).FullName;
                                            if (WexflowEngine.IsDirectoryEmpty(recordTempDir))
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
            _ = _endpoints.MapPost(GetPattern("save-record"), async context =>
            {
                try
                {
                    var res = false;
                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var json = await GetBodyAsync(context);
                        var o = JObject.Parse(json);

                        var id = o.Value<string>("Id");
                        var name = o.Value<string>("Name");
                        var description = o.Value<string>("Description");
                        var startDate = o.Value<string>("StartDate");
                        var endDate = o.Value<string>("EndDate");
                        var comments = o.Value<string>("Comments");
                        var approved = o.Value<bool>("Approved");
                        var managerComments = o.Value<string>("ManagerComments");
                        var modifiedBy = o.Value<string>("ModifiedBy");
                        var modifiedOn = o.Value<string>("ModifiedOn");
                        var createdBy = o.Value<string>("CreatedBy");
                        var createdOn = o.Value<string>("CreatedOn");
                        var assignedTo = o.Value<string>("AssignedTo");
                        var assignedOn = o.Value<string>("AssignedOn");
                        var versions = JsonConvert.DeserializeObject<Contracts.Version[]>(o.Value<JArray>("Versions").ToString());

                        var dateTimeFormat = WexflowServer.Config["DateTimeFormat"] ?? throw new InvalidOperationException("DateTimeFormat setting not found");

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

                        List<Core.Db.Version> recordVersions = [];
                        foreach (var version in versions)
                        {
                            recordVersions.Add(new Core.Db.Version
                            {
                                RecordId = version.RecordId,
                                FilePath = version.FilePath
                            });
                        }

                        var recordId = WexflowServer.WexflowEngine.SaveRecord(id, record, recordVersions);

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
            _ = _endpoints.MapPost(GetPattern("delete-records"), async context =>
            {
                try
                {
                    var res = false;

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var json = await GetBodyAsync(context);
                        var recordIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
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
            _ = _endpoints.MapGet(GetPattern("search-records"), async context =>
            {
                var username = context.User.Identity?.Name;

                var keyword = context.Request.Query["s"].ToString();

                var records = Array.Empty<Contracts.Record>();

                var user = WexflowServer.WexflowEngine.GetUser(username);
                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var recordsArray = WexflowServer.WexflowEngine.GetRecords(keyword);
                    List<Contracts.Record> recordsList = [];
                    foreach (var record in recordsArray)
                    {
                        var createdBy = !string.IsNullOrEmpty(record.CreatedBy) ? WexflowServer.WexflowEngine.GetUserById(record.CreatedBy) : null;
                        var modifiedBy = !string.IsNullOrEmpty(record.ModifiedBy) ? WexflowServer.WexflowEngine.GetUserById(record.ModifiedBy) : null;
                        var assignedTo = !string.IsNullOrEmpty(record.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(record.AssignedTo) : null;
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
                        var approvers = WexflowServer.WexflowEngine.GetApprovers(record.GetDbId());
                        List<Contracts.Approver> approversList = [];
                        foreach (var approver in approvers)
                        {
                            var approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
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
                        r.Approvers = [.. approversList];

                        // Versions
                        var versions = WexflowServer.WexflowEngine.GetVersions(record.GetDbId());
                        List<Contracts.Version> versionsList = [];
                        foreach (var version in versions)
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
                        r.Versions = [.. versionsList];
                        recordsList.Add(r);
                    }
                    records = [.. recordsList];
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(records));
            });
        }

        /// <summary>
        /// Retrieves records created by a user.
        /// </summary>
        private void GetRecordsCreatedBy()
        {
            _ = _endpoints.MapGet(GetPattern("records-created-by"), async context =>
            {
                var username = context.User.Identity?.Name;

                var createdByUsername = context.Request.Query["c"].ToString();
                var createdBy = WexflowServer.WexflowEngine.GetUser(createdByUsername);

                var records = Array.Empty<Contracts.Record>();

                var user = WexflowServer.WexflowEngine.GetUser(username);
                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var recordsArray = WexflowServer.WexflowEngine.GetRecordsCreatedBy(createdBy.GetDbId());
                    List<Contracts.Record> recordsList = [];
                    foreach (var record in recordsArray)
                    {
                        var createdByUser = !string.IsNullOrEmpty(record.CreatedBy) ? WexflowServer.WexflowEngine.GetUserById(record.CreatedBy) : null;
                        var modifiedByUser = !string.IsNullOrEmpty(record.ModifiedBy) ? WexflowServer.WexflowEngine.GetUserById(record.ModifiedBy) : null;
                        var assignedToUser = !string.IsNullOrEmpty(record.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(record.AssignedTo) : null;
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
                        var approvers = WexflowServer.WexflowEngine.GetApprovers(record.GetDbId());
                        List<Contracts.Approver> approversList = [];
                        foreach (var approver in approvers)
                        {
                            var approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
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
                        r.Approvers = [.. approversList];

                        // Versions
                        var versions = WexflowServer.WexflowEngine.GetVersions(record.GetDbId());
                        List<Contracts.Version> versionsList = [];
                        foreach (var version in versions)
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
                        r.Versions = [.. versionsList];
                        recordsList.Add(r);
                    }
                    records = [.. recordsList];
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(records));
            });
        }

        /// <summary>
        /// Searches for records assigned to or created by a user by keyword.
        /// </summary>
        private void SearchRecordsCreatedByOrAssignedTo()
        {
            _ = _endpoints.MapGet(GetPattern("search-records-created-by-or-assigned-to"), async context =>
            {
                var username = context.User.Identity?.Name;

                var keyword = context.Request.Query["s"].ToString();
                var createdByUsername = context.Request.Query["c"].ToString();
                var createdBy = !string.IsNullOrEmpty(createdByUsername) ? WexflowServer.WexflowEngine.GetUser(createdByUsername) : null;
                var assignedToUsername = context.Request.Query["a"].ToString();
                var assignedTo = !string.IsNullOrEmpty(assignedToUsername) ? WexflowServer.WexflowEngine.GetUser(assignedToUsername) : null;

                var records = Array.Empty<Contracts.Record>();

                var user = WexflowServer.WexflowEngine.GetUser(username);
                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var recordsArray = WexflowServer.WexflowEngine.GetRecordsCreatedByOrAssignedTo(createdBy != null ? createdBy.GetDbId() : string.Empty, assignedTo != null ? assignedTo.GetDbId() : string.Empty, keyword);
                    List<Contracts.Record> recordsList = [];
                    foreach (var record in recordsArray)
                    {
                        var createdByUser = !string.IsNullOrEmpty(record.CreatedBy) ? WexflowServer.WexflowEngine.GetUserById(record.CreatedBy) : null;
                        var modifiedByUser = !string.IsNullOrEmpty(record.ModifiedBy) ? WexflowServer.WexflowEngine.GetUserById(record.ModifiedBy) : null;
                        var assignedToUser = !string.IsNullOrEmpty(record.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(record.AssignedTo) : null;
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
                        var approvers = WexflowServer.WexflowEngine.GetApprovers(record.GetDbId());
                        List<Contracts.Approver> approversList = [];
                        foreach (var approver in approvers)
                        {
                            var approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
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
                        r.Approvers = [.. approversList];

                        // Versions
                        var versions = WexflowServer.WexflowEngine.GetVersions(record.GetDbId());
                        List<Contracts.Version> versionsList = [];
                        foreach (var version in versions)
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
                        r.Versions = [.. versionsList];
                        recordsList.Add(r);
                    }
                    records = [.. recordsList];
                }

                await context.Response.WriteAsync(JsonConvert.SerializeObject(records));
            });
        }

        /// <summary>
        /// Indicates whether the user has notifications or not.
        /// </summary>
        private void HasNotifications()
        {
            _ = _endpoints.MapGet(GetPattern("has-notifications"), async context =>
            {
                var username = context.User.Identity?.Name;

                var assignedToUsername = context.Request.Query["a"].ToString();
                var assignedTo = WexflowServer.WexflowEngine.GetUser(assignedToUsername);

                var res = false;

                var user = WexflowServer.WexflowEngine.GetUser(username);
                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
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
            _ = _endpoints.MapPost(GetPattern("mark-notifications-as-read"), async context =>
            {
                try
                {
                    var res = false;
                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var json = await GetBodyAsync(context);
                        var notificationIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
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
            _ = _endpoints.MapPost(GetPattern("mark-notifications-as-unread"), async context =>
            {
                try
                {
                    var res = false;
                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var json = await GetBodyAsync(context);
                        var notificationIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
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
            _ = _endpoints.MapPost(GetPattern("delete-notifications"), async context =>
            {
                try
                {
                    var res = false;

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var json = await GetBodyAsync(context);
                        var notificationIds = JsonConvert.DeserializeObject<string[]>(JArray.Parse(json).ToString());
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
            _ = _endpoints.MapGet(GetPattern("search-notifications"), async context =>
            {
                var username = context.User.Identity?.Name;

                var assignedToUsername = context.Request.Query["a"].ToString();
                var assignedTo = WexflowServer.WexflowEngine.GetUser(assignedToUsername);
                var keyword = context.Request.Query["s"].ToString();

                var notifications = Array.Empty<Contracts.Notification>();

                var user = WexflowServer.WexflowEngine.GetUser(username);
                if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var notificationsArray = WexflowServer.WexflowEngine.GetNotifications(assignedTo.GetDbId(), keyword);
                    List<Contracts.Notification> notificationList = [];
                    foreach (var notification in notificationsArray)
                    {
                        var assignedByUser = !string.IsNullOrEmpty(notification.AssignedBy) ? WexflowServer.WexflowEngine.GetUserById(notification.AssignedBy) : null;
                        var assignedToUser = !string.IsNullOrEmpty(notification.AssignedTo) ? WexflowServer.WexflowEngine.GetUserById(notification.AssignedTo) : null;
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
                    notifications = [.. notificationList];
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
        private static bool NotifyUser(Core.Db.User assignedBy, Core.Db.User assignedTo, string message)
        {
            var res = false;
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
                var id = WexflowServer.WexflowEngine.InsertNotification(notification);
                res = id != "-1";

                var enableEmailNotifications = bool.Parse(WexflowServer.Config["EnableEmailNotifications"] ?? throw new InvalidOperationException());
                if (enableEmailNotifications)
                {
                    var subject = $"Wexflow notification from {assignedBy.Username}";
                    var body = message;

                    var host = WexflowServer.Config["Smtp.Host"];
                    var port = int.Parse(WexflowServer.Config["Smtp.Port"] ?? throw new InvalidOperationException());
                    var enableSsl = bool.Parse(WexflowServer.Config["Smtp.EnableSsl"] ?? throw new InvalidOperationException());
                    var smtpUser = WexflowServer.Config["Smtp.User"];
                    var smtpPassword = WexflowServer.Config["Smtp.Password"];
                    var from = WexflowServer.Config["Smtp.From"];

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
            _ = _endpoints.MapPost(GetPattern("notify"), async context =>
            {
                try
                {
                    var res = false;

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var assignedToUsername = context.Request.Query["a"].ToString();
                        var message = context.Request.Query["m"].ToString();
                        var assignedTo = WexflowServer.WexflowEngine.GetUser(assignedToUsername);
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
            _ = _endpoints.MapPost(GetPattern("notify-approvers"), async context =>
            {
                try
                {
                    var res = true;

                    var username = context.User.Identity?.Name;

                    var user = WexflowServer.WexflowEngine.GetUser(username);
                    if ((user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        var recordId = context.Request.Query["r"].ToString();
                        var message = context.Request.Query["m"].ToString();

                        var approvers = WexflowServer.WexflowEngine.GetApprovers(recordId);
                        foreach (var approver in approvers)
                        {
                            var approverUser = WexflowServer.WexflowEngine.GetUserById(approver.UserId);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Wexflow.Core.Db;
using Wexflow.Core.ExecutionGraph;
using Wexflow.Core.ExecutionGraph.Flowchart;

namespace Wexflow.Core
{
    /// <summary>
    /// Workflow.
    /// </summary>
    public class Workflow
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// This constant is used to determine the key size of the encryption algorithm in bits.
        /// We divide this by 8 within the code below to get the equivalent number of bytes.
        /// </summary>
        public const int KEY_SIZE = 128;
        /// <summary>
        /// This constant determines the number of iterations for the password bytes generation function. 
        /// </summary>
        public const int DERIVATION_ITERATIONS = 1000;
        /// <summary>
        /// PassPhrase.
        /// </summary>
        public const string PASS_PHRASE = "FHMWW-EORNR-XXF0Q-E8Q#G-YC!RG-KV=TN-M9MQJ-AySDI-LAC5Q-UV==QE-VSVNL-OV1IZ";

        /// <summary>
        /// Default parent node id to start with in the execution graph.
        /// </summary>
        public const int START_ID = -1;

        #region Properties
        /// <summary>
        /// Wexflow engine.
        /// </summary>
        public WexflowEngine WexflowEngine { get; }
        /// <summary>
        /// Database ID.
        /// </summary>
        public string DbId { get; }
        /// <summary>
        /// Username of the user that started the workflow.
        /// </summary>
        public string StartedBy { get; private set; }
        /// <summary>
        /// Username of the user that started the workflow.
        /// </summary>
        public string ApprovedBy { get; private set; }
        /// <summary>
        /// Username of the user that started the workflow.
        /// </summary>
        public string RejectedBy { get; private set; }
        /// <summary>
        /// Username of the user that started the workflow.
        /// </summary>
        public string StoppedBy { get; private set; }
        /// <summary>
        /// Workflow file path.
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// XML of the workflow.
        /// </summary>
        public string Xml { get; }
        /// <summary>
        /// Wexflow temp folder.
        /// </summary>
        public string WexflowTempFolder { get; }
        /// <summary>
        /// Workflow temp folder.
        /// </summary>
        public string WorkflowTempFolder { get; private set; }
        /// <summary>
        /// Approval folder.
        /// </summary>
        public string ApprovalFolder { get; }
        /// <summary>
        /// XSD path.
        /// </summary>
        public string XsdPath { get; }
        /// <summary>
        /// Workflow Id.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Workflow name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Workflow description.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Workflow lanch type.
        /// </summary>
        public LaunchType LaunchType { get; private set; }
        /// <summary>
        /// Workflow period.
        /// </summary>
        public TimeSpan Period { get; private set; }
        /// <summary>
        /// Cron expression
        /// </summary>
        public string CronExpression { get; private set; }
        /// <summary>
        /// Shows whether this workflow is enabled or not.
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Shows whether this workflow is an approval workflow or not.
        /// </summary>
        public bool IsApproval { get; private set; }
        /// <summary>
        /// Shows whether workflow jobs are executed in parallel. Otherwise jobs are queued. Defaults to true.
        /// </summary>
        public bool EnableParallelJobs { get; private set; }
        /// <summary>
        /// Shows whether this workflow is waiting for approval.
        /// </summary>
        public bool IsWaitingForApproval { get; set; }
        /// <summary>
        /// Shows whether this workflow is rejected or not.
        /// </summary>
        public bool IsRejected { get; private set; }
        /// <summary>
        /// Shows whether this workflow is running or not.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Shows whether this workflow is suspended or not.
        /// </summary>
        public bool IsPaused { get; private set; }
        /// <summary>
        /// Workflow tasks.
        /// </summary>
        public Task[] Tasks { get; private set; }
        /// <summary>
        /// Workflow files.
        /// </summary>
        public Dictionary<int, List<FileInf>> FilesPerTask { get; }
        /// <summary>
        /// Workflow entities.
        /// </summary>
        public Dictionary<int, List<Entity>> EntitiesPerTask { get; }
        /// <summary>
        /// Job Id.
        /// </summary>
        public int JobId { get; private set; }
        /// <summary>
        /// Parallel job Id.
        /// </summary>
        public int ParallelJobId { get; private set; }
        /// <summary>
        /// Log tag.
        /// </summary>
        public string LogTag => $"[{Name} / {JobId}]";
        /// <summary>
        /// Xml Namespace Manager.
        /// </summary>
        public XmlNamespaceManager XmlNamespaceManager { get; private set; }
        /// <summary>
        /// Execution graph.
        /// </summary>
        public Graph ExecutionGraph { get; private set; }
        /// <summary>
        /// Workflow XDocument.
        /// </summary>
        public XDocument XDoc { get; private set; }
        /// <summary>
        /// XNamespace.
        /// </summary>
        public XNamespace XNamespaceWf { get; private set; }
        /// <summary>
        /// Shows whether the execution graph is empty or not.
        /// </summary>
        public bool IsExecutionGraphEmpty { get; private set; }
        /// <summary>
        /// Hashtable used as shared memory for tasks.
        /// </summary>
        public Hashtable SharedMemory { get; private set; }
        /// <summary>
        /// Database.
        /// </summary>
        public Db.Db Database { get; }
        /// <summary>
        /// Global variables.
        /// </summary>
        public Variable[] GlobalVariables { get; }
        /// <summary>
        /// Local variables.
        /// </summary>
        public Variable[] LocalVariables { get; private set; }
        /// <summary>
        /// Rest variables.
        /// </summary>
        public List<Variable> RestVariables { get; private set; }
        /// <summary>
        /// Tasks folder.
        /// </summary>
        public string TasksFolder { get; }
        /// <summary>
        /// Workflow jobs.
        /// </summary>
        public Dictionary<Guid, Workflow> Jobs { get; }
        /// <summary>
        /// Instance Id.
        /// </summary>
        public Guid InstanceId { get; private set; }
        /// <summary>
        /// Log messages.
        /// </summary>
        public List<string> Logs { get; }
        /// <summary>
        /// Started on date time.
        /// </summary>
        public DateTime StartedOn { get; private set; }
        /// <summary>
        /// Number of retry times in case of failures. Default is 0.
        /// </summary>
        public int RetryCount { get; private set; }
        /// <summary>
        /// The retry timeout between two tries. Default is 1500ms.
        /// </summary>
        public int RetryTimeout { get; private set; }
        /// <summary>
        /// Job Status.
        /// </summary>
        public Db.Status JobStatus { get; private set; }
        #endregion

        private readonly Queue<Job> _jobsQueue;
        private Thread _thread;
        private HistoryEntry _historyEntry;
        private bool _stopCalled;

        /// <summary>
        /// Creates a new workflow.
        /// </summary>
        /// <param name="wexflowEngine">Wexflow engine.</param>
        /// <param name="jobId">First job Id.</param>
        /// <param name="jobs">Workflow jobs.</param>
        /// <param name="dbId">Database ID.</param>
        /// <param name="xml">XML of the workflow.</param>
        /// <param name="wexflowTempFolder">Wexflow temp folder.</param>
        /// <param name="tasksFolder">Tasks folder.</param>
        /// <param name="approvalFolder">Approval folder.</param>
        /// <param name="xsdPath">XSD path.</param>
        /// <param name="database">Database.</param>
        /// <param name="globalVariables">Global variables.</param>
        public Workflow(
              WexflowEngine wexflowEngine
            , int jobId
            , Dictionary<Guid, Workflow> jobs
            , string dbId
            , string xml
            , string wexflowTempFolder
            , string tasksFolder
            , string approvalFolder
            , string xsdPath
            , Db.Db database
            , Variable[] globalVariables)
        {
            WexflowEngine = wexflowEngine;
            Logs = new List<string>();
            JobId = jobId;
            ParallelJobId = jobId;
            Jobs = jobs;
            _jobsQueue = new Queue<Job>();
            _thread = null;
            DbId = dbId;
            Xml = xml;
            WexflowTempFolder = wexflowTempFolder;
            TasksFolder = tasksFolder;
            ApprovalFolder = approvalFolder;
            XsdPath = xsdPath;
            Database = database;
            FilesPerTask = new Dictionary<int, List<FileInf>>();
            EntitiesPerTask = new Dictionary<int, List<Entity>>();
            SharedMemory = new Hashtable();
            GlobalVariables = globalVariables;
            RestVariables = new List<Variable>();
            StartedOn = DateTime.MinValue;
            Check();
            LoadLocalVariables();
            Load(Xml);

            if (!IsEnabled)
            {
                Database.IncrementDisabledCount();
            }
        }

        /// <summary>
        /// Returns informations about this workflow.
        /// </summary>
        /// <returns>Informations about this workflow.</returns>
        public override string ToString()
        {
            return $"{{id: {Id}, name: {Name}, enabled: {IsEnabled}, launchType: {LaunchType}}}";
        }

        private void Check()
        {
            var schemas = new XmlSchemaSet();
            _ = schemas.Add("urn:wexflow-schema", XsdPath);

            var doc = XDocument.Parse(Xml);
            var msg = string.Empty;
            doc.Validate(schemas, (o, e) =>
            {
                msg += e.Message + Environment.NewLine;
            });

            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception($"The workflow XML document is not valid. Error: {msg}");
            }
        }

        private void LoadLocalVariables()
        {
            using (var xmlReader = XmlReader.Create(new StringReader(Xml)))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }
                else
                {
                    throw new Exception($"xmlNameTable of {Id} is null");
                }

                var xdoc = XDocument.Parse(Xml);
                var localVariables = new List<Variable>();

                foreach (var xvariable in xdoc.XPathSelectElements("/wf:Workflow/wf:LocalVariables/wf:Variable",
                    XmlNamespaceManager))
                {
                    var key = (xvariable.Attribute("name") ?? throw new InvalidOperationException("name attribute of local varible not found")).Value;
                    var value = (xvariable.Attribute("value") ?? throw new InvalidOperationException("value attribute of local varible not found")).Value;

                    var variable = new Variable
                    {
                        Key = key,
                        Value = value
                    };

                    localVariables.Add(variable);
                }

                LocalVariables = localVariables.ToArray();
            }
        }

        private string Parse(string src)
        {
            string dest;

            //
            // Parse functions.
            // $DateTime(): current unix timestamp in milliseconds
            // $Guid(): new Guid
            //
            using (var sr = new StringReader(src))
            using (var sw = new StringWriter())
            {
                long unixTimestampMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace("$DateTime()", unixTimestampMilliseconds.ToString());
                    line = line.Replace("$Guid()", Guid.NewGuid().ToString());
                    sw.WriteLine(line);
                }
                dest = sw.ToString();
            }

            //
            // Parse global variables.
            //
            using (var sr = new StringReader(dest))
            using (var sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in GlobalVariables)
                    {
                        line = line.Replace($"${variable.Key}", variable.Value);
                    }
                    sw.WriteLine(line);
                }
                dest = sw.ToString();
            }

            //
            // Load local variables with their final values (parsed)
            //
            var localVariablesParsed = new List<Variable>();
            using (var xmlReader = XmlReader.Create(new StringReader(dest)))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }
                else
                {
                    throw new Exception($"xmlNameTable of {Id} is null");
                }

                var xdoc = XDocument.Parse(dest);

                foreach (var xvariable in xdoc.XPathSelectElements("/wf:Workflow/wf:LocalVariables/wf:Variable",
                    XmlNamespaceManager))
                {
                    var key = (xvariable.Attribute("name") ?? throw new InvalidOperationException("name attribute of local varible not found")).Value;
                    var value = (xvariable.Attribute("value") ?? throw new InvalidOperationException("value attribute of local varible not found")).Value;

                    var variable = new Variable
                    {
                        Key = key,
                        Value = value
                    };

                    localVariablesParsed.Add(variable);
                }
            }

            //
            // Parse local variables.
            //
            string res;
            using (var sr = new StringReader(dest))
            using (var sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var pattern = "{.*?}";
                    var m = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        if (m.Value.StartsWith("{date:"))
                        {
                            var replaceValue = DateTime.Now.ToString(m.Value.Remove(m.Value.Length - 1).Remove(0, 6));
                            line = Regex.Replace(line, pattern, replaceValue);
                        }
                    }
                    foreach (var variable in localVariablesParsed)
                    {
                        line = line.Replace($"${variable.Key}", variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res = sw.ToString();
            }

            //
            // Parse Rest variables.
            //
            string res2;
            using (var sr = new StringReader(res))
            using (var sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in RestVariables)
                    {
                        if (variable != null)
                        {
                            line = line.Replace("$" + variable.Key, variable.Value);
                        }
                    }
                    sw.WriteLine(line);
                }
                res2 = sw.ToString();
            }

            return res2;
        }

        private void Load(string xml)
        {
            FilesPerTask.Clear();
            EntitiesPerTask.Clear();

            using (var xmlReader = XmlReader.Create(new StringReader(xml)))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }
                else
                {
                    throw new Exception($"xmlNameTable of {Id} is null");
                }

                // Loading settings
                var xdoc = XDocument.Parse(xml);
                XDoc = xdoc;
                XNamespaceWf = "urn:wexflow-schema";

                Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
                Name = GetWorkflowAttribute(xdoc, "name");
                Description = GetWorkflowAttribute(xdoc, "description");
                LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType", true), true);

                var period = GetWorkflowSetting(xdoc, "period", false);
                if (LaunchType == LaunchType.Periodic || !string.IsNullOrEmpty(period))
                {
                    Period = TimeSpan.Parse(period);
                }

                var cronexp = GetWorkflowSetting(xdoc, "cronExpression", false);
                if (LaunchType == LaunchType.Cron || !string.IsNullOrEmpty(cronexp))
                {
                    CronExpression = cronexp;
                    if (!WexflowEngine.IsCronExpressionValid(CronExpression))
                    {
                        throw new Exception($"The cron expression '{CronExpression}' is not valid.");
                    }
                }
                IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled", true));
                var isApprovalStr = GetWorkflowSetting(xdoc, "approval", false);
                IsApproval = bool.Parse(string.IsNullOrEmpty(isApprovalStr) ? "false" : isApprovalStr);

                var enableParallelJobsStr = GetWorkflowSetting(xdoc, "enableParallelJobs", false);
                EnableParallelJobs = bool.Parse(string.IsNullOrEmpty(enableParallelJobsStr) ? "true" : enableParallelJobsStr);

                var retryCount = GetWorkflowSetting(xdoc, "retryCount", false);
                RetryCount = int.Parse(string.IsNullOrEmpty(retryCount) ? "0" : retryCount);

                var retryTimeout = GetWorkflowSetting(xdoc, "retryTimeout", false);
                RetryTimeout = int.Parse(string.IsNullOrEmpty(retryTimeout) ? "1500" : retryTimeout);

                if (xdoc.Root != null)
                {
                    var xExecutionGraph = xdoc.Root.Element(XNamespaceWf + "ExecutionGraph");
                    IsExecutionGraphEmpty = xExecutionGraph == null || !xExecutionGraph.Elements().Any();
                }

                // Loading tasks
                var tasks = new List<Task>();
                foreach (var xTask in xdoc.XPathSelectElements("/wf:Workflow/wf:Tasks/wf:Task", XmlNamespaceManager))
                {
                    var xAttribute = xTask.Attribute("name");
                    if (xAttribute != null)
                    {
                        var name = xAttribute.Value;
                        var assemblyName = $"Wexflow.Tasks.{name}";
                        var typeName = $"Wexflow.Tasks.{name}.{name}, {assemblyName}";

                        // Try to load from root
                        var type = Type.GetType(typeName);

                        if (type == null) // Try to load from Tasks folder
                        {
                            var taskAssemblyFile = Path.Combine(TasksFolder, assemblyName + ".dll");
                            if (File.Exists(taskAssemblyFile))
                            {
                                var taskAssembly = Assembly.LoadFile(taskAssemblyFile);
                                var typeFullName = $"Wexflow.Tasks.{name}.{name}";
                                type = taskAssembly.GetType(typeFullName);
                            }
                        }

                        if (type != null)
                        {
                            var task = (Task)Activator.CreateInstance(type, xTask, this);
                            tasks.Add(task);
                        }
                        else
                        {
                            throw new Exception($"The type of the task {name} could not be loaded.");
                        }
                    }
                    else
                    {
                        throw new Exception($"Name attribute of the task {xTask} does not exist.");
                    }
                }
                Tasks = tasks.ToArray();

                // Loading execution graph
                var xExectionGraph = xdoc.XPathSelectElement("/wf:Workflow/wf:ExecutionGraph", XmlNamespaceManager);
                if (xExectionGraph != null)
                {
                    var taskNodes = GetTaskNodes(xExectionGraph);

                    // Check startup node, parallel tasks and infinite loops
                    if (taskNodes.Length != 0)
                    {
                        CheckStartupNode(taskNodes, "Startup node with parentId=-1 not found in ExecutionGraph execution graph.");
                    }

                    CheckParallelTasks(taskNodes, "Parallel tasks execution detected in ExecutionGraph execution graph.");
                    CheckInfiniteLoop(taskNodes, "Infinite loop detected in ExecutionGraph execution graph.");

                    // OnSuccess
                    GraphEvent onSuccess = null;
                    var xOnSuccess = xExectionGraph.XPathSelectElement("wf:OnSuccess", XmlNamespaceManager);
                    if (xOnSuccess != null)
                    {
                        var onSuccessNodes = GetTaskNodes(xOnSuccess);
                        CheckStartupNode(onSuccessNodes, "Startup node with parentId=-1 not found in OnSuccess execution graph.");
                        CheckParallelTasks(onSuccessNodes, "Parallel tasks execution detected in OnSuccess execution graph.");
                        CheckInfiniteLoop(onSuccessNodes, "Infinite loop detected in OnSuccess execution graph.");
                        onSuccess = new GraphEvent(onSuccessNodes);
                    }

                    // OnWarning
                    GraphEvent onWarning = null;
                    var xOnWarning = xExectionGraph.XPathSelectElement("wf:OnWarning", XmlNamespaceManager);
                    if (xOnWarning != null)
                    {
                        var onWarningNodes = GetTaskNodes(xOnWarning);
                        CheckStartupNode(onWarningNodes, "Startup node with parentId=-1 not found in OnWarning execution graph.");
                        CheckParallelTasks(onWarningNodes, "Parallel tasks execution detected in OnWarning execution graph.");
                        CheckInfiniteLoop(onWarningNodes, "Infinite loop detected in OnWarning execution graph.");
                        onWarning = new GraphEvent(onWarningNodes);
                    }

                    // OnError
                    GraphEvent onError = null;
                    var xOnError = xExectionGraph.XPathSelectElement("wf:OnError", XmlNamespaceManager);
                    if (xOnError != null)
                    {
                        var onErrorNodes = GetTaskNodes(xOnError);
                        CheckStartupNode(onErrorNodes, "Startup node with parentId=-1 not found in OnError execution graph.");
                        CheckParallelTasks(onErrorNodes, "Parallel tasks execution detected in OnError execution graph.");
                        CheckInfiniteLoop(onErrorNodes, "Infinite loop detected in OnError execution graph.");
                        onError = new GraphEvent(onErrorNodes);
                    }

                    // OnRejected
                    GraphEvent onRejected = null;
                    var xOnDispproved = xExectionGraph.XPathSelectElement("wf:OnRejected", XmlNamespaceManager);
                    if (xOnDispproved != null)
                    {
                        var onRejectedNodes = GetTaskNodes(xOnDispproved);
                        CheckStartupNode(onRejectedNodes, "Startup node with parentId=-1 not found in OnError execution graph.");
                        CheckParallelTasks(onRejectedNodes, "Parallel tasks execution detected in OnError execution graph.");
                        CheckInfiniteLoop(onRejectedNodes, "Infinite loop detected in OnError execution graph.");
                        onRejected = new GraphEvent(onRejectedNodes);
                    }

                    ExecutionGraph = new Graph(taskNodes, onSuccess, onWarning, onError, onRejected);
                }
            }
        }

        private Node[] GetTaskNodes(XElement xExectionGraph)
        {
            int depth = 0;
            var xNodes = xExectionGraph
                .Elements()
                .Where(xe => xe.Name.LocalName != "OnSuccess" && xe.Name.LocalName != "OnWarning" && xe.Name.LocalName != "OnError" && xe.Name.LocalName != "OnRejected")
                .ToArray();

            var nodesList = new List<Node>();
            foreach (var xNode in xNodes)
            {
                var node = XNodeToNode(xNode, ref depth);
                nodesList.Add(node);
            }

            var nodes = nodesList.ToArray();
            return nodes;
        }

        private Node XNodeToNode(XElement xNode, ref int depth)
        {
            int currentDepth = depth; // Copy current depth by value to get depth at each level

            switch (xNode.Name.LocalName)
            {
                case "Task":
                    var xId = xNode.Attribute("id") ?? throw new Exception("Task id not found.");
                    var id = int.Parse(xId.Value);

                    var xParentId = (xNode.XPathSelectElement("wf:Parent", XmlNamespaceManager) ?? throw new InvalidOperationException("wf:Parent not found"))
                        .Attribute("id") ?? throw new Exception("Parent id not found.");
                    var parentId = int.Parse(xParentId.Value);

                    Node node = new Node(id, parentId, currentDepth);
                    return node;
                case "If":
                    return XIfToIf(xNode, ref depth);
                case "While":
                    return XWhileToWhile(xNode, ref depth);
                case "Switch":
                    return XSwitchToSwitch(xNode, ref depth);
                default:
                    throw new Exception(xNode.Name.LocalName + " is not supported.");
            }
        }

        private If XIfToIf(XElement xIf, ref int depth)
        {
            int currentDepth = depth; // current depth

            depth++;

            var xId = xIf.Attribute("id") ?? throw new Exception("If id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xIf.Attribute("parent") ?? throw new Exception("If parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xIfId = xIf.Attribute("if") ?? throw new Exception("If attribute not found.");
            var ifId = int.Parse(xIfId.Value);

            // Do nodes
            var xNodes = (xIf.XPathSelectElement("wf:Do", XmlNamespaceManager) ?? throw new InvalidOperationException("wf:Do not found"))
              .Elements();
            var doNodesList = new List<Node>();
            foreach (var xNode in xNodes)
            {
                var node = XNodeToNode(xNode, ref depth);
                doNodesList.Add(node);
            }
            var doNodes = doNodesList.ToArray();

            CheckStartupNode(doNodes, "Startup node with parentId=-1 not found in DoIf>Do execution graph.");
            CheckParallelTasks(doNodes, "Parallel tasks execution detected in DoIf>Do execution graph.");
            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoIf>Do execution graph.");

            // Otherwise nodes
            Node[] elseNodes = null;
            var xElse = xIf.XPathSelectElement("wf:Else", XmlNamespaceManager);
            if (xElse != null)
            {
                var elseNodesList = new List<Node>();
                var xElseNodes = xElse.Elements();
                foreach (var xNode in xElseNodes)
                {
                    var node = XNodeToNode(xNode, ref depth);
                    elseNodesList.Add(node);
                }
                elseNodes = elseNodesList.ToArray();

                CheckStartupNode(elseNodes, "Startup node with parentId=-1 not found in DoIf>Otherwise execution graph.");
                CheckParallelTasks(elseNodes, "Parallel tasks execution detected in DoIf>Otherwise execution graph.");
                CheckInfiniteLoop(elseNodes, "Infinite loop detected in DoIf>Otherwise execution graph.");
            }

            return new If(id, parentId, ifId, doNodes, elseNodes, currentDepth);
        }

        private While XWhileToWhile(XElement xWhile, ref int depth)
        {
            int currentDepth = depth; // current depth

            depth++;

            var xId = xWhile.Attribute("id") ?? throw new Exception("While Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xWhile.Attribute("parent") ?? throw new Exception("While parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xWhileId = xWhile.Attribute("while") ?? throw new Exception("While attribute not found.");
            var whileId = int.Parse(xWhileId.Value);

            var xNodes = xWhile.Elements();
            var nodesList = new List<Node>();
            foreach (var xNode in xNodes)
            {
                var node = XNodeToNode(xNode, ref depth);
                nodesList.Add(node);
            }
            var doNodes = nodesList.ToArray();

            CheckStartupNode(doNodes, "Startup node with parentId=-1 not found in DoWhile>Do execution graph.");
            CheckParallelTasks(doNodes, "Parallel tasks execution detected in DoWhile>Do execution graph.");
            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoWhile>Do execution graph.");

            return new While(id, parentId, whileId, doNodes, currentDepth);
        }

        private Switch XSwitchToSwitch(XElement xSwitch, ref int depth)
        {
            int currentDepth = depth; // current depth

            depth++;

            var xId = xSwitch.Attribute("id") ?? throw new Exception("Switch Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xSwitch.Attribute("parent") ?? throw new Exception("Switch parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xSwitchId = xSwitch.Attribute("switch") ?? throw new Exception("Switch attribute not found.");
            var switchId = int.Parse(xSwitchId.Value);

            var xCases = xSwitch.XPathSelectElements("wf:Case", XmlNamespaceManager);
            var casesList = new List<Case>();

            foreach (var xCase in xCases)
            {
                var xValue = xCase.Attribute("value") ?? throw new Exception("Case value attribute not found.");
                var val = xValue.Value;

                var xNodes = xCase.Elements();
                var nodesList = new List<Node>();
                foreach (var xNode in xNodes)
                {
                    var node = XNodeToNode(xNode, ref depth);
                    nodesList.Add(node);
                }
                var nodes = nodesList.ToArray();

                var nodeName = $"Switch>Case(value={val})";
                CheckStartupNode(nodes, $"Startup node with parentId=-1 not found in {nodeName} execution graph.");
                CheckParallelTasks(nodes, $"Parallel tasks execution detected in {nodeName} execution graph.");
                CheckInfiniteLoop(nodes, $"Infinite loop detected in {nodeName} execution graph.");

                var caseNode = new Case(val, nodes);
                casesList.Add(caseNode);
            }
            var cases = casesList.ToArray();

            var xDefault = xSwitch.XPathSelectElement("wf:Default", XmlNamespaceManager);
            if (xDefault == null)
            {
                return new Switch(id, parentId, switchId, cases, null, currentDepth);
            }

            var xDefaultNodes = xDefault.Elements();
            var defaultNodesList = new List<Node>();
            foreach (var xNode in xDefaultNodes)
            {
                var node = XNodeToNode(xNode, ref depth);
                defaultNodesList.Add(node);
            }
            var @default = defaultNodesList.ToArray();

            if (@default.Length > 0)
            {
                CheckStartupNode(@default,
                    "Startup node with parentId=-1 not found in Switch>Default execution graph.");
                CheckParallelTasks(@default, "Parallel tasks execution detected in Switch>Default execution graph.");
                CheckInfiniteLoop(@default, "Infinite loop detected in Switch>Default execution graph.");
            }

            return new Switch(id, parentId, switchId, cases, @default, currentDepth);
        }

        private void CheckStartupNode(Node[] nodes, string errorMsg)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(); // new ArgumentNullException(nameof(nodes))
            }

            if (nodes.All(n => n.ParentId != START_ID))
            {
                throw new Exception(errorMsg);
            }
        }

        private void CheckParallelTasks(Node[] taskNodes, string errorMsg)
        {
            var parallelTasksDetected = false;
            foreach (var taskNode in taskNodes)
            {
                if (taskNodes.Count(n => n.ParentId == taskNode.Id) > 1)
                {
                    parallelTasksDetected = true;
                    break;
                }
            }

            if (parallelTasksDetected)
            {
                throw new Exception(errorMsg);
            }
        }

        private void CheckInfiniteLoop(Node[] taskNodes, string errorMsg)
        {
            var startNode = taskNodes.FirstOrDefault(n => n.ParentId == START_ID);

            if (startNode != null)
            {
                var infiniteLoopDetected = CheckInfiniteLoop(startNode, taskNodes);

                if (!infiniteLoopDetected)
                {
                    foreach (var taskNode in taskNodes.Where(n => n.Id != startNode.Id))
                    {
                        infiniteLoopDetected |= CheckInfiniteLoop(taskNode, taskNodes);

                        if (infiniteLoopDetected)
                        {
                            break;
                        }
                    }
                }

                if (infiniteLoopDetected)
                {
                    throw new Exception(errorMsg);
                }
            }
        }

        private bool CheckInfiniteLoop(Node startNode, Node[] taskNodes)
        {
            foreach (var taskNode in taskNodes.Where(n => n.ParentId != startNode.ParentId))
            {
                if (taskNode.Id == startNode.Id)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetWorkflowAttribute(XDocument xdoc, string attr)
        {
            var xAttribute = (xdoc.XPathSelectElement("/wf:Workflow", XmlNamespaceManager) ?? throw new InvalidOperationException("wf:Workflow not found")).Attribute(attr);
            return xAttribute != null ? xAttribute.Value : throw new Exception($"Workflow attribute {attr}not found.");
        }

        private string GetWorkflowSetting(XDocument xdoc, string name, bool throwExceptionIfNotFound)
        {
            var xSetting = xdoc
                .XPathSelectElement(
                    $"/wf:Workflow[@id='{Id}']/wf:Settings/wf:Setting[@name='{name}']",
                    XmlNamespaceManager);

            if (xSetting != null)
            {
                var xAttribute = xSetting.Attribute("value");
                if (xAttribute != null)
                {
                    return xAttribute.Value;
                }
                else if (throwExceptionIfNotFound)
                {
                    throw new Exception($"Workflow setting {name} not found.");
                }
            }
            else if (throwExceptionIfNotFound)
            {
                throw new Exception($"Workflow setting {name} not found.");
            }

            return string.Empty;
        }

        /// <summary>
        /// Starts this workflow asynchronously.
        /// </summary>
        /// <param name="startedBy">Username of the user that started the workflow.</param>
        /// <param name="restVariables">Optional REST variables.</param>
        /// <returns>A task that resolves to the workflow instance ID, or <see cref="Guid.Empty"/> if queued.</returns>
        public async System.Threading.Tasks.Task<Guid> StartAsync(string startedBy, List<Variable> restVariables = null)
        {
            if (IsRunning && !EnableParallelJobs)
            {
                StartedBy = startedBy;
                var job = new Job() { Workflow = this, QueuedOn = DateTime.Now };
                _jobsQueue.Enqueue(job);
                return Guid.Empty;
            }

            if (IsRunning && EnableParallelJobs)
            {
                var parallelWorkflow = new Workflow(
                    WexflowEngine,
                    ++ParallelJobId,
                    Jobs,
                    DbId,
                    Xml,
                    WexflowTempFolder,
                    TasksFolder,
                    ApprovalFolder,
                    XsdPath,
                    Database,
                    GlobalVariables)
                {
                    RestVariables = RestVariables,
                    StartedBy = startedBy
                };

                return await parallelWorkflow.StartAsync(startedBy, restVariables);
            }

            StartedOn = DateTime.Now;
            StartedBy = startedBy;
            var instanceId = Guid.NewGuid();
            _thread = null;

            // LEGACY THREAD-BASED VERSION
            // Used to capture the thread for support with Thread.Interrupt() in Stop() method.
            // Note: This approach is only suitable for legacy scenarios (e.g., .NET Framework / compatibility mode).
            _thread = new Thread(() =>
            {
                try
                {
                    // Block the thread and run the workflow synchronously (async bridge).
                    var warning = false;
                    StartInternalAsync(startedBy, instanceId, warning, restVariables).GetAwaiter().GetResult();
                }
                catch (ThreadInterruptedException)
                {
                    // Graceful exit when workflow is stopped using Thread.Interrupt()
                }
            });

            _thread.Start();


            return instanceId;
        }

        /// <summary>
        /// Starts this workflow asynchronously (internal logic).
        /// </summary>
        /// <param name="startedBy">Username of the user that started the workflow.</param>
        /// <param name="instanceId">Workflow instance ID.</param>
        /// <param name="resultWarning">Set to true if workflow finishes with a warning.</param>
        /// <param name="restVariables">REST variables passed to the workflow.</param>
        /// <returns>True if the workflow completed successfully; otherwise false.</returns>
        public async System.Threading.Tasks.Task<bool> StartInternalAsync(
            string startedBy,
            Guid instanceId,
            bool resultWarning,
            List<Variable> restVariables = null)
        {
            await _semaphore.WaitAsync();

            if (IsRunning)
            {
                if (EnableParallelJobs)
                {
                    var pInstanceId = Guid.NewGuid();
                    var warning = false;

                    var parallelWorkflow = new Workflow(
                        WexflowEngine,
                        ++ParallelJobId,
                        Jobs,
                        DbId,
                        Xml,
                        WexflowTempFolder,
                        TasksFolder,
                        ApprovalFolder,
                        XsdPath,
                        Database,
                        GlobalVariables)
                    {
                        RestVariables = RestVariables,
                        StartedBy = startedBy
                    };

                    return await parallelWorkflow.StartInternalAsync(startedBy, pInstanceId, warning, restVariables);
                }
                else
                {
                    var job = new Job() { Workflow = this, QueuedOn = DateTime.Now };
                    _jobsQueue.Enqueue(job);
                    return true;
                }
            }

            var result = new RunResult();

            JobStatus = Db.Status.Running;

            try
            {
                StartedOn = DateTime.Now;
                StartedBy = startedBy;
                InstanceId = instanceId;
                Jobs.Add(InstanceId, this);

                if (restVariables != null)
                {
                    RestVariables.Clear();
                    RestVariables.AddRange(restVariables);
                }

                var dest = Parse(Xml);
                Load(dest);

                _stopCalled = false;
                Logs.Clear();

                if (WexflowEngine.LogLevel != LogLevel.None)
                {
                    var msg = $"{LogTag} Workflow started - Instance Id: {InstanceId}";
                    Logger.Info(msg);
                    Logs.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  INFO - {msg}");
                }

                Database.IncrementRunningCount();

                var entry = Database.GetEntry(Id, InstanceId);
                if (entry == null)
                {
                    entry = new Entry
                    {
                        WorkflowId = Id,
                        JobId = InstanceId.ToString(),
                        Name = Name,
                        LaunchType = (Db.LaunchType)(int)LaunchType,
                        Description = Description,
                        Status = Db.Status.Running,
                        StatusDate = DateTime.Now,
                        Logs = string.Join("\r\n", Logs)
                    };
                    Database.InsertEntry(entry);
                }
                else
                {
                    entry.Status = Db.Status.Running;
                    entry.StatusDate = DateTime.Now;
                    entry.Logs = string.Join("\r\n", Logs);
                    Database.UpdateEntry(entry.GetDbId(), entry);
                }

                _historyEntry = new HistoryEntry
                {
                    WorkflowId = Id,
                    Name = Name,
                    LaunchType = (Db.LaunchType)(int)LaunchType,
                    Description = Description
                };


                IsRunning = true;
                IsRejected = false;
                CreateTempFolder();

                if (ExecutionGraph == null)
                {
                    await RunSequentialTasksAsync(Tasks, result);
                }
                else
                {
                    var status = await RunTasksAsync(ExecutionGraph.Nodes, Tasks, false);

                    if (!_stopCalled)
                    {
                        switch (status)
                        {
                            case Status.Success:
                                if (ExecutionGraph.OnSuccess != null)
                                {
                                    var successTasks = NodesToTasks(ExecutionGraph.OnSuccess.Nodes);
                                    await RunTasksAsync(ExecutionGraph.OnSuccess.Nodes, successTasks, false);
                                }
                                break;
                            case Status.Warning:
                                if (ExecutionGraph.OnWarning != null)
                                {
                                    var warningTasks = NodesToTasks(ExecutionGraph.OnWarning.Nodes);
                                    await RunTasksAsync(ExecutionGraph.OnWarning.Nodes, warningTasks, false);
                                }
                                resultWarning = true;
                                break;
                            case Status.Error:
                                if (ExecutionGraph.OnError != null)
                                {
                                    var errorTasks = NodesToTasks(ExecutionGraph.OnError.Nodes);
                                    await RunTasksAsync(ExecutionGraph.OnError.Nodes, errorTasks, false);
                                }
                                result.Success = false;
                                break;
                            case Status.Rejected:
                                if (ExecutionGraph.OnRejected != null)
                                {
                                    var rejectedTasks = NodesToTasks(ExecutionGraph.OnRejected.Nodes);
                                    await RunTasksAsync(ExecutionGraph.OnRejected.Nodes, rejectedTasks, true);
                                }
                                break;
                        }
                    }
                }

                if (!_stopCalled)
                {
                    LogWorkflowFinished();

                    entry = Database.GetEntry(Id, InstanceId);
                    entry.StatusDate = DateTime.Now;
                    entry.Logs = string.Join("\r\n", Logs);

                    if (IsRejected)
                    {
                        Database.IncrementRejectedCount();
                        entry.Status = Db.Status.Rejected;
                        JobStatus = Db.Status.Rejected;
                        _historyEntry.Status = Db.Status.Rejected;
                    }
                    else if (result.Success)
                    {
                        Database.IncrementDoneCount();
                        entry.Status = Db.Status.Done;
                        JobStatus = Db.Status.Done;
                        _historyEntry.Status = Db.Status.Done;
                    }
                    else if (result.Warning)
                    {
                        Database.IncrementWarningCount();
                        entry.Status = Db.Status.Warning;
                        JobStatus = Db.Status.Warning;
                        _historyEntry.Status = Db.Status.Warning;
                        resultWarning = true;
                    }
                    else
                    {
                        Database.IncrementFailedCount();
                        entry.Status = Db.Status.Failed;
                        JobStatus = Db.Status.Failed;
                        _historyEntry.Status = Db.Status.Failed;
                        result.Success = false;
                    }

                    Database.UpdateEntry(entry.GetDbId(), entry);

                    _historyEntry.StatusDate = DateTime.Now;
                    _historyEntry.Logs = entry.Logs;
                    Database.InsertHistoryEntry(_historyEntry);
                    Database.DecrementRunningCount();
                }
            }
            catch (Exception ex)
            {
                if (WexflowEngine.LogLevel != LogLevel.None)
                {
                    var msg = $"An error occurred while running the workflow. Error: {this}";
                    Logger.Error(msg, ex);
                    Logs.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  ERROR - {msg}\r\n{ex}");
                }

                Database.DecrementRunningCount();
                Database.IncrementFailedCount();

                var entry = Database.GetEntry(Id, InstanceId);
                entry.Status = Db.Status.Failed;
                JobStatus = Db.Status.Failed;
                entry.StatusDate = DateTime.Now;
                entry.Logs = string.Join("\r\n", Logs);
                Database.UpdateEntry(entry.GetDbId(), entry);

                _historyEntry.Status = Db.Status.Failed;
                _historyEntry.StatusDate = DateTime.Now;
                _historyEntry.Logs = entry.Logs;
                Database.InsertHistoryEntry(_historyEntry);

                result.Success = false;
            }
            finally
            {
                if (!_stopCalled)
                {
                    Logs.Clear();
                }

                foreach (var files in FilesPerTask.Values) files.Clear();
                foreach (var entities in EntitiesPerTask.Values) entities.Clear();

                IsRunning = false;
                IsRejected = false;
                GC.Collect();

                JobId = ++ParallelJobId;
                _ = Jobs.Remove(InstanceId);

                if (_jobsQueue.Count > 0)
                {
                    var job = _jobsQueue.Dequeue();
                    _ = job.Workflow.StartAsync(startedBy);
                }
                else
                {
                    if (!_stopCalled)
                    {
                        Load(Xml); // Reload original workflow definition
                    }
                    RestVariables.Clear();
                }
            }

            _semaphore.Release();

            return result.Success;
        }

        private void LogWorkflowFinished()
        {
            if (WexflowEngine.LogLevel != LogLevel.None)
            {
                var msg = $"{LogTag} Workflow finished.";
                Logger.Info(msg);
                Logs.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} INFO  - {msg}");
            }
        }

        private Task[] NodesToTasks(Node[] nodes)
        {
            var tasks = new List<Task>();

            if (nodes == null)
            {
                return tasks.ToArray();
            }

            foreach (var node in nodes)
            {
                if (node is If @if)
                {
                    var doTasks = NodesToTasks(@if.DoNodes);
                    var otherwiseTasks = NodesToTasks(@if.ElseNodes);

                    var ifTasks = new List<Task>(doTasks);
                    foreach (var task in otherwiseTasks)
                    {
                        if (ifTasks.All(t => t.Id != task.Id))
                        {
                            ifTasks.Add(task);
                        }
                    }

                    tasks.AddRange(ifTasks);
                }
                else if (node is While @while)
                {
                    tasks.AddRange(NodesToTasks(@while.Nodes));
                }
                else if (node is Switch @switch)
                {
                    tasks.AddRange(NodesToTasks(@switch.Default).Where(task => tasks.All(t => t.Id != task.Id)));
                    tasks.AddRange(NodesToTasks(@switch.Cases.SelectMany(@case => @case.Nodes).ToArray()).Where(task => tasks.All(t => t.Id != task.Id)));
                }
                else
                {
                    var task = GetTask(node.Id);

                    if (task != null)
                    {
                        if (tasks.All(t => t.Id != task.Id))
                        {
                            tasks.Add(task);
                        }
                    }
                    else
                    {
                        throw new Exception($"Task {node.Id} not found.");
                    }
                }
            }

            return tasks.ToArray();
        }

        /// <summary>
        /// Asynchronously runs a list of workflow nodes and returns the final workflow execution status.
        /// </summary>
        /// <param name="nodes">The workflow nodes to execute.</param>
        /// <param name="tasks">The list of available tasks.</param>
        /// <param name="force">Whether to force execution even if rejected.</param>
        /// <returns>The overall execution <see cref="Status"/>.</returns>
        private async System.Threading.Tasks.Task<Status> RunTasksAsync(Node[] nodes, Task[] tasks, bool force)
        {
            var result = new RunResult();

            if (nodes.Length > 0)
            {
                var startNode = GetStartupNode(nodes);

                if (startNode is If ifNode)
                {
                    await RunIfAsync(tasks, nodes, ifNode, force, result);
                }
                else if (startNode is While whileNode)
                {
                    await RunWhileAsync(tasks, nodes, whileNode, force, result);
                }
                else
                {
                    if (startNode.ParentId == START_ID)
                    {
                        await RunTasksAsync(tasks, nodes, startNode, force, result);
                    }
                }
            }

            if (IsRejected)
            {
                return Status.Rejected;
            }

            if (result.Success)
            {
                return Status.Success;
            }

            if (result.AtLeastOneSucceeded || result.Warning)
            {
                return Status.Warning;
            }

            return Status.Error;
        }

        /// <summary>
        /// Runs a workflow task asynchronously with retry support.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <returns>
        /// A <see cref="Task{TaskStatus}"/> representing the asynchronous operation, containing the final <see cref="TaskStatus"/> after retries.
        /// </returns>
        /// <remarks>
        /// This method calls the task's <c>RunAsync()</c> method and, if the task does not succeed,
        /// retries execution up to <c>RetryCount</c> times with a delay of <c>RetryTimeout</c> milliseconds between attempts.
        /// Retry attempts are logged using <c>task.InfoFormat</c>.
        /// </remarks>
        private async System.Threading.Tasks.Task<TaskStatus> RunTaskAsync(Task task)
        {
            var status = await task.RunAsync();

            var retries = 0;

            // Retry logic for faulted or unsuccessful tasks
            while (status.Status != Status.Success && retries < RetryCount)
            {
                // Wait before retrying
                Thread.Sleep(RetryTimeout);

                // Log retry attempt
                task.InfoFormat("Retry attempt {0}", retries + 1);

                // Re-run task asynchronously
                status = await task.RunAsync();
                retries++;
            }

            return status;
        }

        /// <summary>
        /// Executes a list of tasks sequentially and updates the result object with the aggregated status.
        /// </summary>
        /// <param name="tasks">The tasks to execute.</param>
        /// <param name="result">An object that tracks overall success, warnings, and errors.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async System.Threading.Tasks.Task RunSequentialTasksAsync(IEnumerable<Task> tasks, RunResult result)
        {
            var atLeastOneSucceed = false;
            var enumerable = tasks as Task[] ?? tasks.ToArray();

            foreach (var task in enumerable)
            {
                if (!task.IsEnabled)
                {
                    continue;
                }

                if (task.IsStopped)
                {
                    break;
                }

                if (IsApproval && IsRejected)
                {
                    Logs.AddRange(task.Logs);
                    continue;
                }

                task.Logs.Clear();
                var status = await RunTaskAsync(task);
                Logs.AddRange(task.Logs);

                result.Success &= status.Status == Status.Success;
                result.Warning |= status.Status == Status.Warning;
                result.Error |= status.Status == Status.Error;

                if (!atLeastOneSucceed && status.Status == Status.Success)
                {
                    atLeastOneSucceed = true;
                }
            }

            // Promote to warning if at least one task succeeded but not all
            if (enumerable.Length > 0 && !result.Success && atLeastOneSucceed)
            {
                result.Warning = true;
            }
        }

        private async System.Threading.Tasks.Task RunTasksAsync(Task[] tasks, Node[] nodes, Node node, bool force, RunResult result)
        {
            if (node == null) return;

            if (node is If ifNode)
            {
                await RunIfAsync(tasks, nodes, ifNode, force, result);
            }
            else if (node is While whileNode)
            {
                await RunWhileAsync(tasks, nodes, whileNode, force, result);
            }
            else if (node is Switch switchNode)
            {
                await RunSwitchAsync(tasks, nodes, switchNode, force, result);
            }
            else
            {
                var task = GetTask(tasks, node.Id) ?? throw new Exception($"Task {node.Id} not found.");

                if (task.IsEnabled && !task.IsStopped && (!IsApproval || (IsApproval && !IsRejected) || force))
                {
                    task.Logs.Clear();
                    var status = await task.RunAsync();
                    Logs.AddRange(task.Logs);

                    result.Success &= status.Status == Status.Success;
                    result.Warning |= status.Status == Status.Warning;
                    if (!result.AtLeastOneSucceeded && status.Status == Status.Success)
                    {
                        result.AtLeastOneSucceeded = true;
                    }

                    var childNode = nodes.FirstOrDefault(n => n.ParentId == node.Id);
                    if (childNode != null)
                    {
                        if (childNode is If childIf)
                        {
                            await RunIfAsync(tasks, nodes, childIf, force, result);
                        }
                        else if (childNode is While childWhile)
                        {
                            await RunWhileAsync(tasks, nodes, childWhile, force, result);
                        }
                        else if (childNode is Switch childSwitch)
                        {
                            await RunSwitchAsync(tasks, nodes, childSwitch, force, result);
                        }
                        else
                        {
                            var childTask = GetTask(tasks, childNode.Id) ?? throw new Exception($"Task {childNode.Id} not found.");

                            if (childTask.IsEnabled && !childTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected) || force))
                            {
                                childTask.Logs.Clear();
                                var childStatus = await childTask.RunAsync();
                                Logs.AddRange(childTask.Logs);

                                result.Success &= childStatus.Status == Status.Success;
                                result.Warning |= childStatus.Status == Status.Warning;
                                if (!result.AtLeastOneSucceeded && childStatus.Status == Status.Success)
                                {
                                    result.AtLeastOneSucceeded = true;
                                }

                                var ccNode = nodes.FirstOrDefault(n => n.ParentId == childNode.Id);

                                if (ccNode is If ccIf)
                                {
                                    await RunIfAsync(tasks, nodes, ccIf, force, result);
                                }
                                else if (ccNode is While ccWhile)
                                {
                                    await RunWhileAsync(tasks, nodes, ccWhile, force, result);
                                }
                                else if (ccNode is Switch ccSwitch)
                                {
                                    await RunSwitchAsync(tasks, nodes, ccSwitch, force, result);
                                }
                                else
                                {
                                    await RunTasksAsync(tasks, nodes, ccNode, force, result);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs an If node asynchronously, executing either the Do or Else branch based on the condition.
        /// </summary>
        /// <param name="tasks">All tasks of the workflow.</param>
        /// <param name="nodes">All nodes of the workflow.</param>
        /// <param name="ifNode">The If node to execute.</param>
        /// <param name="force">Whether to force execution despite approval/rejection rules.</param>
        /// <param name="result">An object to track success, warnings, and completion state.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async System.Threading.Tasks.Task RunIfAsync(Task[] tasks, Node[] nodes, If ifNode, bool force, RunResult result)
        {
            var ifTask = GetTask(ifNode.IfId);

            if (ifTask == null)
            {
                throw new Exception($"Task {ifNode.Id} not found.");
            }

            if (ifTask.IsEnabled && !ifTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected) || force))
            {
                ifTask.Logs.Clear();
                var status = await ifTask.RunAsync();
                Logs.AddRange(ifTask.Logs);

                result.Success &= status.Status == Status.Success;
                result.Warning |= status.Status == Status.Warning;
                if (!result.AtLeastOneSucceeded && status.Status == Status.Success)
                {
                    result.AtLeastOneSucceeded = true;
                }

                // Run Do branch if condition is true
                if (status.Status == Status.Success && status.Condition)
                {
                    if (ifNode.DoNodes.Length > 0)
                    {
                        var doTasks = NodesToTasks(ifNode.DoNodes);
                        var doStartNode = GetStartupNode(ifNode.DoNodes);

                        if (doStartNode.ParentId == START_ID)
                        {
                            await RunTasksAsync(doTasks, ifNode.DoNodes, doStartNode, force, result);
                        }
                    }
                }
                // Run Else branch if condition is false
                else if (!status.Condition && ifNode.ElseNodes != null && ifNode.ElseNodes.Length > 0)
                {
                    var elseTasks = NodesToTasks(ifNode.ElseNodes);
                    var elseStartNode = GetStartupNode(ifNode.ElseNodes);

                    await RunTasksAsync(elseTasks, ifNode.ElseNodes, elseStartNode, force, result);
                }

                // Run child node of the If block
                var childNode = nodes.FirstOrDefault(n => n.ParentId == ifNode.Id);
                if (childNode != null)
                {
                    await RunTasksAsync(tasks, nodes, childNode, force, result);
                }
            }
        }

        /// <summary>
        /// Runs a While node asynchronously. Executes the loop body as long as the condition task succeeds and returns true.
        /// </summary>
        /// <param name="tasks">All tasks in the workflow.</param>
        /// <param name="nodes">All nodes in the workflow.</param>
        /// <param name="whileNode">The While node to execute.</param>
        /// <param name="force">Whether to force execution even if rejected.</param>
        /// <param name="result">An object that accumulates the workflow run results.</param>
        /// <returns>A task representing the asynchronous execution.</returns>
        private async System.Threading.Tasks.Task RunWhileAsync(Task[] tasks, Node[] nodes, While whileNode, bool force, RunResult result)
        {
            var whileTask = GetTask(whileNode.WhileId);

            if (whileTask == null)
            {
                throw new Exception($"Task {whileNode.Id} not found.");
            }

            if (whileTask.IsEnabled && !whileTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected) || force))
            {
                while (true)
                {
                    whileTask.Logs.Clear();
                    var status = await whileTask.RunAsync();
                    Logs.AddRange(whileTask.Logs);

                    result.Success &= status.Status == Status.Success;
                    result.Warning |= status.Status == Status.Warning;
                    if (!result.AtLeastOneSucceeded && status.Status == Status.Success)
                    {
                        result.AtLeastOneSucceeded = true;
                    }

                    if (status.Status == Status.Success && status.Condition)
                    {
                        if (whileNode.Nodes.Length > 0)
                        {
                            var loopTasks = NodesToTasks(whileNode.Nodes);
                            var loopStartNode = GetStartupNode(whileNode.Nodes);

                            await RunTasksAsync(loopTasks, whileNode.Nodes, loopStartNode, force, result);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // Run the child node after the while block
                var childNode = nodes.FirstOrDefault(n => n.ParentId == whileNode.Id);
                if (childNode != null)
                {
                    await RunTasksAsync(tasks, nodes, childNode, force, result);
                }
            }
        }

        /// <summary>
        /// Runs a Switch node asynchronously. Executes the matching case block or the default block based on the evaluated switch value.
        /// </summary>
        /// <param name="tasks">All tasks in the workflow.</param>
        /// <param name="nodes">All nodes in the workflow.</param>
        /// <param name="switchNode">The Switch node to execute.</param>
        /// <param name="force">Whether to force execution even if rejected.</param>
        /// <param name="result">An object that accumulates the workflow run results.</param>
        /// <returns>A task representing the asynchronous execution.</returns>
        private async System.Threading.Tasks.Task RunSwitchAsync(Task[] tasks, Node[] nodes, Switch switchNode, bool force, RunResult result)
        {
            var switchTask = GetTask(switchNode.SwitchId);

            if (switchTask == null)
            {
                throw new Exception($"Task {switchNode.Id} not found.");
            }

            if (switchTask.IsEnabled && !switchTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected) || force))
            {
                switchTask.Logs.Clear();
                var status = await switchTask.RunAsync();
                Logs.AddRange(switchTask.Logs);

                result.Success &= status.Status == Status.Success;
                result.Warning |= status.Status == Status.Warning;
                if (!result.AtLeastOneSucceeded && status.Status == Status.Success)
                {
                    result.AtLeastOneSucceeded = true;
                }

                if (status.Status == Status.Success)
                {
                    var executedCase = false;

                    foreach (var @case in switchNode.Cases)
                    {
                        if (@case.Value == status.SwitchValue)
                        {
                            if (@case.Nodes.Length > 0)
                            {
                                var caseTasks = NodesToTasks(@case.Nodes);
                                var caseStartNode = GetStartupNode(@case.Nodes);

                                await RunTasksAsync(caseTasks, @case.Nodes, caseStartNode, force, result);
                            }

                            executedCase = true;
                            break;
                        }
                    }

                    if (!executedCase && switchNode.Default != null && switchNode.Default.Length > 0)
                    {
                        var defaultTasks = NodesToTasks(switchNode.Default);
                        var defaultStartNode = GetStartupNode(switchNode.Default);

                        await RunTasksAsync(defaultTasks, switchNode.Default, defaultStartNode, force, result);
                    }

                    // Run child node after switch block
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == switchNode.Id);
                    if (childNode != null)
                    {
                        await RunTasksAsync(tasks, nodes, childNode, force, result);
                    }
                }
            }
        }

        /// <summary>
        /// Stops this workflow.
        /// </summary>
        /// <param name="stoppedBy">Username of the user who stopped the workflow.</param>
        public bool Stop(string stoppedBy)
        {
            if (IsRunning)
            {
                try
                {
                    var jobId = JobId;
                    StoppedBy = stoppedBy;
                    _stopCalled = true;

                    // Stop all running tasks
                    foreach (var task in Tasks)
                    {
                        task.Stop();

                        //if (ExecutionGraph == null)
                        //{
                        //    Logs.AddRange(task.Logs);
                        //}
                    }

                    // Interrupt and wait for the workflow thread to finish
                    if (_thread != null)
                    {
                        _thread.Abort();
                        _thread.Join();
                    }

                    var logs = string.Join("\r\n", Logs);
                    IsWaitingForApproval = false;
                    Database.DecrementRunningCount();
                    Database.IncrementStoppedCount();
                    var entry = Database.GetEntry(Id, InstanceId);
                    entry.Status = Db.Status.Stopped;
                    JobStatus = Db.Status.Stopped;
                    entry.StatusDate = DateTime.Now;
                    entry.Logs = logs;
                    Database.UpdateEntry(entry.GetDbId(), entry);
                    _historyEntry.Status = Db.Status.Stopped;
                    _historyEntry.StatusDate = DateTime.Now;
                    _historyEntry.Logs = logs;
                    Database.InsertHistoryEntry(_historyEntry);
                    IsRejected = false;
                    Logs.Clear();
                    _ = Jobs.Remove(InstanceId);

                    //if (_jobsQueue.Count > 0)
                    //{
                    //    var job = _jobsQueue.Dequeue();
                    //    _ = job.Workflow.StartAsync(StartedBy);
                    //}

                    foreach (var job in _jobsQueue)
                    {
                        _ = job.Workflow.Stop(stoppedBy);
                    }

                    Load(Xml); // Reload the original workflow

                    if (WexflowEngine.LogLevel != LogLevel.None)
                    {
                        Logger.Info($"Workflow {Name} / {jobId} / {InstanceId} stopped.");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    _stopCalled = false;

                    if (WexflowEngine.LogLevel != LogLevel.None)
                    {
                        var msg = $"An error occured while stopping the workflow : {this}";
                        Logger.Error(msg, e);
                        Logs.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}  ERROR - {msg}\r\n{e}");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Suspends this workflow.
        /// </summary>
        public bool Suspend()
        {
            if (IsRunning)
            {
                try
                {
#pragma warning disable CS0618
                    _thread.Suspend();
#pragma warning restore CS0618
                    IsPaused = true;
                    Database.IncrementPendingCount();
                    Database.DecrementRunningCount();
                    var entry = Database.GetEntry(Id, InstanceId);
                    entry.Status = Db.Status.Pending;
                    entry.StatusDate = DateTime.Now;
                    Database.UpdateEntry(entry.GetDbId(), entry);
                    return true;
                }
                catch (Exception e)
                {
                    if (WexflowEngine.LogLevel != LogLevel.None)
                    {
                        var msg = $"An error occured while suspending the workflow : {this}";
                        Logger.Error(msg, e);
                        Logs.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}  ERROR - {msg}\r\n{e}");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Resumes this workflow.
        /// </summary>
        public void Resume()
        {
            if (IsPaused)
            {
                try
                {
#pragma warning disable 618
                    _thread.Resume();
#pragma warning restore 618
                    Database.IncrementRunningCount();
                    Database.DecrementPendingCount();
                    var entry = Database.GetEntry(Id, InstanceId);
                    entry.Status = Db.Status.Running;
                    entry.StatusDate = DateTime.Now;
                    Database.UpdateEntry(entry.GetDbId(), entry);
                }
                catch (Exception e)
                {
                    if (WexflowEngine.LogLevel != LogLevel.None)
                    {
                        var msg = $"An error occured while resuming the workflow : {this}";
                        Logger.Error(msg, e);
                        Logs.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}  ERROR - {msg}\r\n{e}");
                    }
                }
                finally
                {
                    IsPaused = false;
                }
            }
        }

        /// <summary>
        /// Approves the current workflow.
        /// </summary>
        /// <param name="approvedBy">Username of the user who approved the workflow.</param>
        public void Approve(string approvedBy)
        {
            if (IsApproval)
            {
                ApprovedBy = approvedBy;
                var task = Tasks.First(t => t.IsWaitingForApproval);
                var dir = Path.Combine(ApprovalFolder, Id.ToString(), InstanceId.ToString(), task.Id.ToString());
                _ = Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "task.approved"), $"Task {task.Id} of the workflow {Id} approved.");
                IsRejected = false;
            }
        }

        /// <summary>
        /// Rejects the current workflow.
        /// </summary>
        /// <param name="rejectedBy">Username of the user who rejected the workflow.</param>
        public void Reject(string rejectedBy)
        {
            if (IsApproval)
            {
                RejectedBy = rejectedBy;
                IsRejected = true;
            }
        }

        private void CreateTempFolder()
        {
            // WorkflowId/dd-MM-yyyy/HH-mm-ss-fff
            var wfTempFolder = Path.Combine(WexflowTempFolder, Id.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(wfTempFolder))
            {
                _ = Directory.CreateDirectory(wfTempFolder);
            }

            var wfDayTempFolder = Path.Combine(wfTempFolder, $"{DateTime.Now:yyyy-MM-dd}");
            if (!Directory.Exists(wfDayTempFolder))
            {
                _ = Directory.CreateDirectory(wfDayTempFolder);
            }

            var wfJobTempFolder = Path.Combine(wfDayTempFolder, $"{DateTime.Now:HH-mm-ss-fff}");
            if (!Directory.Exists(wfJobTempFolder))
            {
                _ = Directory.CreateDirectory(wfJobTempFolder);
            }

            WorkflowTempFolder = wfJobTempFolder;
        }

        private Node GetStartupNode(IEnumerable<Node> nodes)
        {
            return nodes.First(n => n.ParentId == START_ID);
        }

        private Task GetTask(int id)
        {
            return Tasks.FirstOrDefault(t => t.Id == id);
        }

        private Task GetTask(IEnumerable<Task> tasks, int id)
        {
            return tasks.FirstOrDefault(t => t.Id == id);
        }
    }
}

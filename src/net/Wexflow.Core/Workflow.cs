using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
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
        /// <summary>
        /// This constant is used to determine the key size of the encryption algorithm in bits.
        /// We divide this by 8 within the code below to get the equivalent number of bytes.
        /// </summary>
        public static readonly int KeySize = 128;
        /// <summary>
        /// This constant determines the number of iterations for the password bytes generation function. 
        /// </summary>
        public static readonly int DerivationIterations = 1000;
        /// <summary>
        /// PassPhrase.
        /// </summary>
        public static readonly string PassPhrase = "FHMWW-EORNR-XXF0Q-E8Q#G-YC!RG-KV=TN-M9MQJ-AySDI-LAC5Q-UV==QE-VSVNL-OV1IZ";

        /// <summary>
        /// Default parent node id to start with in the execution graph.
        /// </summary>
        public const int StartId = -1;

        /// <summary>
        /// Wexflow engine.
        /// </summary>
        public WexflowEngine WexflowEngine { get; private set; }
        /// <summary>
        /// Database ID.
        /// </summary>
        public string DbId { get; private set; }
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
        public string Xml { get; private set; }
        /// <summary>
        /// Wexflow temp folder.
        /// </summary>
        public string WexflowTempFolder { get; private set; }
        /// <summary>
        /// Workflow temp folder.
        /// </summary>
        public string WorkflowTempFolder { get; private set; }
        /// <summary>
        /// Approval folder.
        /// </summary>
        public string ApprovalFolder { get; private set; }
        /// <summary>
        /// XSD path.
        /// </summary>
        public string XsdPath { get; private set; }
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
        public Dictionary<int, List<FileInf>> FilesPerTask { get; private set; }
        /// <summary>
        /// Workflow entities.
        /// </summary>
        public Dictionary<int, List<Entity>> EntitiesPerTask { get; private set; }
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
        public string LogTag { get { return string.Format("[{0} / {1}]", Name, JobId); } }
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
        public Db.Db Database { get; private set; }
        /// <summary>
        /// Global variables.
        /// </summary>
        public Variable[] GlobalVariables { get; private set; }
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
        public string TasksFolder { get; private set; }
        /// <summary>
        /// Workflow jobs.
        /// </summary>
        public Dictionary<Guid, Workflow> Jobs { get; private set; }
        /// <summary>
        /// Instance Id.
        /// </summary>
        public Guid InstanceId { get; private set; }
        /// <summary>
        /// Log messages.
        /// </summary>
        public List<string> Logs { get; private set; }
        /// <summary>
        /// Started on date time.
        /// </summary>
        public DateTime StartedOn { get; private set; }

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
            return string.Format("{{id: {0}, name: {1}, enabled: {2}, launchType: {3}}}", Id, Name, IsEnabled, LaunchType);
        }

        private void Check()
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("urn:wexflow-schema", XsdPath);

            var doc = XDocument.Parse(Xml);
            string msg = string.Empty;
            doc.Validate(schemas, (o, e) =>
            {
                msg += e.Message + Environment.NewLine;
            });

            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception("The workflow XML document is not valid. Error: " + msg);
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
                    throw new Exception("xmlNameTable of " + Id + " is null");
                }

                var xdoc = XDocument.Parse(Xml);
                List<Variable> localVariables = new List<Variable>();

                foreach (var xvariable in xdoc.XPathSelectElements("/wf:Workflow/wf:LocalVariables/wf:Variable",
                    XmlNamespaceManager))
                {
                    string key = xvariable.Attribute("name").Value;
                    string value = xvariable.Attribute("value").Value;

                    Variable variable = new Variable
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
            var dest = string.Empty;

            //
            // Parse global variables.
            //
            using (StringReader sr = new StringReader(src))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in GlobalVariables)
                    {
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                dest = sw.ToString();
            }

            //
            // Load local variables with their final values (parsed)
            //
            List<Variable> localVariablesParsed = new List<Variable>();
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
                    throw new Exception("xmlNameTable of " + Id + " is null");
                }

                var xdoc = XDocument.Parse(dest);

                foreach (var xvariable in xdoc.XPathSelectElements("/wf:Workflow/wf:LocalVariables/wf:Variable",
                    XmlNamespaceManager))
                {
                    string key = xvariable.Attribute("name").Value;
                    string value = xvariable.Attribute("value").Value;

                    Variable variable = new Variable
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
            var res = string.Empty;
            using (StringReader sr = new StringReader(dest))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string pattern = @"{.*?}";
                    Match m = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
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
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res = sw.ToString();
            }

            //
            // Parse Rest variables.
            //
            var res2 = string.Empty;
            using (StringReader sr = new StringReader(res))
            using (StringWriter sw = new StringWriter())
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
                    throw new Exception("xmlNameTable of " + Id + " is null");
                }

                // Loading settings
                var xdoc = XDocument.Parse(xml);
                XDoc = xdoc;
                XNamespaceWf = "urn:wexflow-schema";

                Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
                Name = GetWorkflowAttribute(xdoc, "name");
                Description = GetWorkflowAttribute(xdoc, "description");
                LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType", true), true);

                string period = GetWorkflowSetting(xdoc, "period", false);
                if (LaunchType == LaunchType.Periodic || !string.IsNullOrEmpty(period))
                {
                    Period = TimeSpan.Parse(period);
                }

                string cronexp = GetWorkflowSetting(xdoc, "cronExpression", false);
                if (LaunchType == LaunchType.Cron || !string.IsNullOrEmpty(cronexp))
                {
                    CronExpression = cronexp;
                    if (!WexflowEngine.IsCronExpressionValid(CronExpression))
                    {
                        throw new Exception("The cron expression '" + CronExpression + "' is not valid.");
                    }
                }
                IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled", true));
                var isApprovalStr = GetWorkflowSetting(xdoc, "approval", false);
                IsApproval = bool.Parse(string.IsNullOrEmpty(isApprovalStr) ? "false" : isApprovalStr);

                var enableParallelJobsStr = GetWorkflowSetting(xdoc, "enableParallelJobs", false);
                EnableParallelJobs = bool.Parse(string.IsNullOrEmpty(enableParallelJobsStr) ? "true" : enableParallelJobsStr);

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
                        Type type = null;
                        var name = xAttribute.Value;
                        var assemblyName = "Wexflow.Tasks." + name;
                        var typeName = "Wexflow.Tasks." + name + "." + name + ", " + assemblyName;

                        // Try to load from root
                        type = Type.GetType(typeName);

                        if (type == null) // Try to load from Tasks folder
                        {
                            var taskAssemblyFile = Path.Combine(TasksFolder, assemblyName + ".dll");
                            if (File.Exists(taskAssemblyFile))
                            {
                                var taskAssembly = Assembly.LoadFile(taskAssemblyFile);
                                var typeFullName = "Wexflow.Tasks." + name + "." + name;
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
                            throw new Exception("The type of the task " + name + " could not be loaded.");
                        }
                    }
                    else
                    {
                        throw new Exception("Name attribute of the task " + xTask + " does not exist.");
                    }
                }
                Tasks = tasks.ToArray();

                // Loading execution graph
                var xExectionGraph = xdoc.XPathSelectElement("/wf:Workflow/wf:ExecutionGraph", XmlNamespaceManager);
                if (xExectionGraph != null)
                {
                    var taskNodes = GetTaskNodes(xExectionGraph);

                    // Check startup node, parallel tasks and infinite loops
                    if (taskNodes.Any()) CheckStartupNode(taskNodes, "Startup node with parentId=-1 not found in ExecutionGraph execution graph.");
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
            var nodes = xExectionGraph
                .Elements()
                .Where(xe => xe.Name.LocalName != "OnSuccess" && xe.Name.LocalName != "OnWarning" && xe.Name.LocalName != "OnError" && xe.Name.LocalName != "OnRejected")
                .Select(XNodeToNode)
                .ToArray();

            return nodes;
        }

        private If XIfToIf(XElement xIf)
        {
            var xId = xIf.Attribute("id");
            if (xId == null) throw new Exception("If id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xIf.Attribute("parent");
            if (xParent == null) throw new Exception("If parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xIfId = xIf.Attribute("if");
            if (xIfId == null) throw new Exception("If attribute not found.");
            var ifId = int.Parse(xIfId.Value);

            // Do nodes
            var doNodes = xIf.XPathSelectElement("wf:Do", XmlNamespaceManager)
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            CheckStartupNode(doNodes, "Startup node with parentId=-1 not found in DoIf>Do execution graph.");
            CheckParallelTasks(doNodes, "Parallel tasks execution detected in DoIf>Do execution graph.");
            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoIf>Do execution graph.");

            // Otherwise nodes
            Node[] elseNodes = null;
            var xElse = xIf.XPathSelectElement("wf:Else", XmlNamespaceManager);
            if (xElse != null)
            {
                elseNodes = xElse
                    .Elements()
                    .Select(XNodeToNode)
                    .ToArray();

                CheckStartupNode(elseNodes, "Startup node with parentId=-1 not found in DoIf>Otherwise execution graph.");
                CheckParallelTasks(elseNodes, "Parallel tasks execution detected in DoIf>Otherwise execution graph.");
                CheckInfiniteLoop(elseNodes, "Infinite loop detected in DoIf>Otherwise execution graph.");
            }

            return new If(id, parentId, ifId, doNodes, elseNodes);
        }

        private While XWhileToWhile(XElement xWhile)
        {
            var xId = xWhile.Attribute("id");
            if (xId == null) throw new Exception("While Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xWhile.Attribute("parent");
            if (xParent == null) throw new Exception("While parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xWhileId = xWhile.Attribute("while");
            if (xWhileId == null) throw new Exception("While attribute not found.");
            var whileId = int.Parse(xWhileId.Value);

            var doNodes = xWhile
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            CheckStartupNode(doNodes, "Startup node with parentId=-1 not found in DoWhile>Do execution graph.");
            CheckParallelTasks(doNodes, "Parallel tasks execution detected in DoWhile>Do execution graph.");
            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoWhile>Do execution graph.");

            return new While(id, parentId, whileId, doNodes);
        }

        private Switch XSwitchToSwitch(XElement xSwitch)
        {
            var xId = xSwitch.Attribute("id");
            if (xId == null) throw new Exception("Switch Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xSwitch.Attribute("parent");
            if (xParent == null) throw new Exception("Switch parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xSwitchId = xSwitch.Attribute("switch");
            if (xSwitchId == null) throw new Exception("Switch attribute not found.");
            var switchId = int.Parse(xSwitchId.Value);

            var cases = xSwitch
                .XPathSelectElements("wf:Case", XmlNamespaceManager)
                .Select(xCase =>
                {
                    var xValue = xCase.Attribute("value");
                    if (xValue == null) throw new Exception("Case value attribute not found.");
                    var val = xValue.Value;

                    var nodes = xCase
                        .Elements()
                        .Select(XNodeToNode)
                        .ToArray();

                    var nodeName = string.Format("Switch>Case(value={0})", val);
                    CheckStartupNode(nodes, "Startup node with parentId=-1 not found in " + nodeName + " execution graph.");
                    CheckParallelTasks(nodes, "Parallel tasks execution detected in " + nodeName + " execution graph.");
                    CheckInfiniteLoop(nodes, "Infinite loop detected in " + nodeName + " execution graph.");

                    return new Case(val, nodes);
                });

            var xDefault = xSwitch.XPathSelectElement("wf:Default", XmlNamespaceManager);
            if (xDefault == null) return new Switch(id, parentId, switchId, cases, null);
            var @default = xDefault
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            if (@default.Length > 0)
            {
                CheckStartupNode(@default,
                    "Startup node with parentId=-1 not found in Switch>Default execution graph.");
                CheckParallelTasks(@default, "Parallel tasks execution detected in Switch>Default execution graph.");
                CheckInfiniteLoop(@default, "Infinite loop detected in Switch>Default execution graph.");
            }

            return new Switch(id, parentId, switchId, cases, @default);
        }

        private Node XNodeToNode(XElement xNode)
        {
            switch (xNode.Name.LocalName)
            {
                case "Task":
                    var xId = xNode.Attribute("id");
                    if (xId == null) throw new Exception("Task id not found.");
                    var id = int.Parse(xId.Value);

                    var xParentId = xNode.XPathSelectElement("wf:Parent", XmlNamespaceManager)
                        .Attribute("id");

                    if (xParentId == null) throw new Exception("Parent id not found.");
                    var parentId = int.Parse(xParentId.Value);

                    var node = new Node(id, parentId);
                    return node;
                case "If":
                    return XIfToIf(xNode);
                case "While":
                    return XWhileToWhile(xNode);
                case "Switch":
                    return XSwitchToSwitch(xNode);
                default:
                    throw new Exception(xNode.Name.LocalName + " is not supported.");
            }
        }

        private void CheckStartupNode(Node[] nodes, string errorMsg)
        {
            if (nodes == null) throw new ArgumentNullException(); // new ArgumentNullException(nameof(nodes))
            if (nodes.All(n => n.ParentId != StartId)) throw new Exception(errorMsg);
        }

        private void CheckParallelTasks(Node[] taskNodes, string errorMsg)
        {
            bool parallelTasksDetected = false;
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
            var startNode = taskNodes.FirstOrDefault(n => n.ParentId == StartId);

            if (startNode != null)
            {
                var infiniteLoopDetected = CheckInfiniteLoop(startNode, taskNodes);

                if (!infiniteLoopDetected)
                {
                    foreach (var taskNode in taskNodes.Where(n => n.Id != startNode.Id))
                    {
                        infiniteLoopDetected |= CheckInfiniteLoop(taskNode, taskNodes);

                        if (infiniteLoopDetected) break;
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
            var xAttribute = xdoc.XPathSelectElement("/wf:Workflow", XmlNamespaceManager).Attribute(attr);
            if (xAttribute != null)
            {
                return xAttribute.Value;
            }

            throw new Exception("Workflow attribute " + attr + "not found.");
        }

        private string GetWorkflowSetting(XDocument xdoc, string name, bool throwExceptionIfNotFound)
        {
            var xSetting = xdoc
                .XPathSelectElement(
                    string.Format("/wf:Workflow[@id='{0}']/wf:Settings/wf:Setting[@name='{1}']", Id, name),
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
                    throw new Exception("Workflow setting " + name + " not found.");
                }
            }
            else if (throwExceptionIfNotFound)
            {
                throw new Exception("Workflow setting " + name + " not found.");
            }

            return string.Empty;
        }

        /// <summary>
        /// Starts this workflow asynchronously.
        /// </summary>
        /// <param name="startedBy">Username of the user that started the workflow.</param>
        /// <returns>Instance Id.</returns>
        public Guid StartAsync(string startedBy)
        {
            if (IsRunning && !EnableParallelJobs)
            {
                StartedBy = startedBy;
                var job = new Job { Workflow = this, QueuedOn = DateTime.Now };
                _jobsQueue.Enqueue(job);
                return Guid.Empty;
            }
            else if (IsRunning && EnableParallelJobs)
            {
                var workflow = new Workflow(
                      WexflowEngine
                    , ++ParallelJobId
                    , Jobs
                    , DbId
                    , Xml
                    , WexflowTempFolder
                    , TasksFolder
                    , ApprovalFolder
                    , XsdPath
                    , Database
                    , GlobalVariables
                    );
                workflow.RestVariables = RestVariables;
                workflow.StartedBy = startedBy;
                return workflow.StartAsync(startedBy);
            }

            StartedOn = DateTime.Now;
            StartedBy = startedBy;
            var instanceId = Guid.NewGuid();
            var warning = false;
            var thread = new Thread(() => StartSync(startedBy, instanceId, ref warning));
            _thread = thread;
            thread.Start();

            return instanceId;
        }

        /// <summary>
        /// Starts this workflow synchronously.
        /// </summary>
        /// <param name="startedBy">Username of the user that started the workflow.</param>
        /// <param name="instanceId">Instance id.</param>
        /// <param name="resultWarning">Indicates whether the final result is warning or not.</param>
        public bool StartSync(string startedBy, Guid instanceId, ref bool resultWarning)
        {
            var resultSuccess = true;

            StartedOn = DateTime.Now;
            StartedBy = startedBy;
            InstanceId = instanceId;
            Jobs.Add(InstanceId, this);

            //
            // Parse the workflow definition (Global variables and local variables.)
            //
            var dest = Parse(Xml);
            Load(dest);

            _stopCalled = false;
            Logs.Clear();
            var msg = string.Format("{0} Workflow started - Instance Id: {1}", LogTag, InstanceId);
            Logger.Info(msg);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  INFO - " + msg);

            Database.IncrementRunningCount();

            var entry = Database.GetEntry(Id, InstanceId);
            if (entry == null)
            {
                var newEntry = new Entry
                {
                    WorkflowId = Id,
                    JobId = InstanceId.ToString(),
                    Name = Name,
                    LaunchType = ((Db.LaunchType)(int)LaunchType),
                    Description = Description,
                    Status = Db.Status.Running,
                    StatusDate = DateTime.Now,
                    Logs = string.Join("\r\n", Logs)
                };
                Database.InsertEntry(newEntry);
            }
            else
            {
                entry.Status = Db.Status.Running;
                entry.StatusDate = DateTime.Now;
                entry.Logs = string.Join("\r\n", Logs);
                Database.UpdateEntry(entry.GetDbId(), entry);
            }
            entry = Database.GetEntry(Id, InstanceId);

            _historyEntry = new HistoryEntry
            {
                WorkflowId = Id,
                Name = Name,
                LaunchType = ((Db.LaunchType)(int)LaunchType),
                Description = Description
            };

            try
            {
                IsRunning = true;
                IsRejected = false;

                // Create the temp folder
                CreateTempFolder();

                // Run the tasks
                if (ExecutionGraph == null)
                {
                    bool success = true;
                    bool warning = false;
                    bool error = true;
                    RunSequentialTasks(Tasks, ref success, ref warning, ref error);

                    if (!_stopCalled)
                    {
                        if (IsRejected)
                        {
                            LogWorkflowFinished();
                            Database.IncrementRejectedCount();
                            entry.Status = Db.Status.Rejected;
                            entry.StatusDate = DateTime.Now;
                            entry.Logs = string.Join("\r\n", Logs);
                            Database.UpdateEntry(entry.GetDbId(), entry);
                            _historyEntry.Status = Db.Status.Rejected;
                            resultSuccess = true;
                        }
                        else
                        {
                            if (success)
                            {
                                LogWorkflowFinished();
                                Database.IncrementDoneCount();
                                entry.Status = Db.Status.Done;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Done;
                                resultSuccess = true;

                            }
                            else if (warning)
                            {
                                LogWorkflowFinished();
                                Database.IncrementWarningCount();
                                entry.Status = Db.Status.Warning;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Warning;
                                resultWarning = true;
                            }
                            else if (error)
                            {
                                LogWorkflowFinished();
                                Database.IncrementFailedCount();
                                entry.Status = Db.Status.Failed;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Failed;
                                resultSuccess = false;
                            }
                        }
                    }
                }
                else
                {
                    var status = RunTasks(ExecutionGraph.Nodes, Tasks, false);

                    if (!_stopCalled)
                    {
                        switch (status)
                        {
                            case Status.Success:
                                if (ExecutionGraph.OnSuccess != null)
                                {
                                    var successTasks = NodesToTasks(ExecutionGraph.OnSuccess.Nodes);
                                    RunTasks(ExecutionGraph.OnSuccess.Nodes, successTasks, false);
                                }
                                LogWorkflowFinished();
                                Database.IncrementDoneCount();
                                entry.Status = Db.Status.Done;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Done;
                                resultSuccess = true;
                                break;
                            case Status.Warning:
                                if (ExecutionGraph.OnWarning != null)
                                {
                                    var warningTasks = NodesToTasks(ExecutionGraph.OnWarning.Nodes);
                                    RunTasks(ExecutionGraph.OnWarning.Nodes, warningTasks, false);
                                }
                                LogWorkflowFinished();
                                Database.IncrementWarningCount();
                                entry.Status = Db.Status.Warning;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Warning;
                                resultWarning = true;
                                break;
                            case Status.Error:
                                if (ExecutionGraph.OnError != null)
                                {
                                    var errorTasks = NodesToTasks(ExecutionGraph.OnError.Nodes);
                                    RunTasks(ExecutionGraph.OnError.Nodes, errorTasks, false);
                                }
                                LogWorkflowFinished();
                                Database.IncrementFailedCount();
                                entry.Status = Db.Status.Failed;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Failed;
                                resultSuccess = false;
                                break;
                            case Status.Rejected:
                                if (ExecutionGraph.OnRejected != null)
                                {
                                    var rejectedTasks = NodesToTasks(ExecutionGraph.OnRejected.Nodes);
                                    RunTasks(ExecutionGraph.OnRejected.Nodes, rejectedTasks, true);
                                }
                                LogWorkflowFinished();
                                Database.IncrementRejectedCount();
                                entry.Status = Db.Status.Rejected;
                                entry.StatusDate = DateTime.Now;
                                entry.Logs = string.Join("\r\n", Logs);
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Rejected;
                                resultSuccess = true;
                                break;
                        }
                    }
                }

                if (!_stopCalled)
                {
                    _historyEntry.StatusDate = DateTime.Now;
                    _historyEntry.Logs = string.Join("\r\n", Logs);
                    Database.InsertHistoryEntry(_historyEntry);
                    Database.DecrementRunningCount();
                }
            }
            catch (ThreadAbortException)
            {
                _stopCalled = true;
            }
            catch (Exception e)
            {
                var emsg = string.Format("An error occured while running the workflow. Error: {0}", this);
                Logger.Error(emsg, e);
                Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + msg + "\r\n" + e);
                Database.DecrementRunningCount();
                Database.IncrementFailedCount();
                entry.Status = Db.Status.Failed;
                entry.StatusDate = DateTime.Now;
                entry.Logs = string.Join("\r\n", Logs);
                Database.UpdateEntry(entry.GetDbId(), entry);
                _historyEntry.Status = Db.Status.Failed;
                _historyEntry.StatusDate = DateTime.Now;
                _historyEntry.Logs = string.Join("\r\n", Logs);
                Database.InsertHistoryEntry(_historyEntry);
            }
            finally
            {
                // Cleanup
                if (!_stopCalled)
                {
                    Logs.Clear();
                }
                foreach (List<FileInf> files in FilesPerTask.Values) files.Clear();
                foreach (List<Entity> entities in EntitiesPerTask.Values) entities.Clear();
                IsRunning = false;
                IsRejected = false;
                GC.Collect();

                JobId = ++ParallelJobId;
                Jobs.Remove(InstanceId);

                if (_jobsQueue.Count > 0)
                {
                    var job = _jobsQueue.Dequeue();
                    job.Workflow.StartAsync(startedBy);
                }
                else
                {
                    if (!_stopCalled)
                    {
                        Load(Xml); // Reload the original workflow
                    }
                    RestVariables.Clear();
                }

            }

            return resultSuccess;
        }

        private void LogWorkflowFinished()
        {
            var msg = string.Format("{0} Workflow finished.", LogTag);
            Logger.Info(msg);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " INFO  - " + msg);
        }

        private Task[] NodesToTasks(Node[] nodes)
        {
            var tasks = new List<Task>();

            if (nodes == null) return tasks.ToArray();

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
                else if (node is While)
                {
                    tasks.AddRange(NodesToTasks(((While)node).Nodes));
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
                        throw new Exception("Task " + node.Id + " not found.");
                    }
                }
            }

            return tasks.ToArray();
        }

        private Status RunTasks(Node[] nodes, Task[] tasks, bool force)
        {
            var success = true;
            var warning = false;
            var atLeastOneSucceed = false;

            if (nodes.Any())
            {
                var startNode = GetStartupNode(nodes);

                if (startNode is If @if)
                {
                    var doIf = @if;
                    RunIf(tasks, nodes, doIf, force, ref success, ref warning, ref atLeastOneSucceed);
                }
                else if (startNode is While doWhile)
                {
                    RunWhile(tasks, nodes, doWhile, force, ref success, ref warning, ref atLeastOneSucceed);
                }
                else
                {
                    if (startNode.ParentId == StartId)
                    {
                        RunTasks(tasks, nodes, startNode, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }

            if (IsRejected)
            {
                return Status.Rejected;
            }

            if (success)
            {
                return Status.Success;
            }

            if (atLeastOneSucceed || warning)
            {
                return Status.Warning;
            }

            return Status.Error;
        }

        private void RunSequentialTasks(IEnumerable<Task> tasks, ref bool success, ref bool warning, ref bool error)
        {
            var atLeastOneSucceed = false;
            foreach (var task in tasks)
            {
                if (!task.IsEnabled) continue;
                if (task.IsStopped) break;
                if (IsApproval && IsRejected)
                {
                    Logs.AddRange(task.Logs);
                    continue;
                }
                var status = task.Run();
                Logs.AddRange(task.Logs);
                success &= status.Status == Status.Success;
                warning |= status.Status == Status.Warning;
                error &= status.Status == Status.Error;
                if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;
            }

            if (tasks.Count() > 0 && !success && atLeastOneSucceed)
            {
                warning = true;
            }
        }

        private void RunTasks(Task[] tasks, Node[] nodes, Node node, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            if (node != null)
            {
                if (node is If || node is While || node is Switch)
                {
                    if (node is If if1)
                    {
                        var @if = if1;
                        RunIf(tasks, nodes, @if, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                    else if (node is While @while)
                    {
                        RunWhile(tasks, nodes, @while, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                    else
                    {
                        var @switch = (Switch)node;
                        RunSwitch(tasks, nodes, @switch, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
                else
                {
                    var task = GetTask(tasks, node.Id);
                    if (task != null)
                    {
                        if (task.IsEnabled && !task.IsStopped && ((!IsApproval || (IsApproval && !IsRejected)) || force))
                        {
                            var status = task.Run();
                            Logs.AddRange(task.Logs);

                            success &= status.Status == Status.Success;
                            warning |= status.Status == Status.Warning;
                            if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                            var childNode = nodes.FirstOrDefault(n => n.ParentId == node.Id);

                            if (childNode != null)
                            {
                                if (childNode is If if1)
                                {
                                    var @if = if1;
                                    RunIf(tasks, nodes, @if, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                else if (childNode is While while2)
                                {
                                    RunWhile(tasks, nodes, while2, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                else if (childNode is Switch switch2)
                                {
                                    RunSwitch(tasks, nodes, switch2, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                else
                                {
                                    var childTask = GetTask(tasks, childNode.Id);
                                    if (childTask != null)
                                    {
                                        if (childTask.IsEnabled && !childTask.IsStopped && ((!IsApproval || (IsApproval && !IsRejected)) || force))
                                        {
                                            var childStatus = childTask.Run();
                                            Logs.AddRange(childTask.Logs);

                                            success &= childStatus.Status == Status.Success;
                                            warning |= childStatus.Status == Status.Warning;
                                            if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                                            // Recusive call
                                            var ccNode = nodes.FirstOrDefault(n => n.ParentId == childNode.Id);

                                            if (ccNode is If node1)
                                            {
                                                var @if = node1;
                                                RunIf(tasks, nodes, @if, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                            else if (ccNode is While @while)
                                            {
                                                RunWhile(tasks, nodes, @while, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                            else if (ccNode is Switch @switch)
                                            {
                                                RunSwitch(tasks, nodes, @switch, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                            else
                                            {
                                                RunTasks(tasks, nodes, ccNode, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Task " + childNode.Id + " not found.");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Task " + node.Id + " not found.");
                    }
                }
            }
        }

        private void RunIf(Task[] tasks, Node[] nodes, If @if, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var ifTask = GetTask(@if.IfId);

            if (ifTask != null)
            {
                if (ifTask.IsEnabled && !ifTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected)))
                {
                    var status = ifTask.Run();
                    Logs.AddRange(ifTask.Logs);

                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                    if (status.Status == Status.Success && status.Condition)
                    {
                        if (@if.DoNodes.Length > 0)
                        {
                            // Build Tasks
                            var doIfTasks = NodesToTasks(@if.DoNodes);

                            // Run Tasks
                            var doIfStartNode = GetStartupNode(@if.DoNodes);

                            if (doIfStartNode.ParentId == StartId)
                            {
                                RunTasks(doIfTasks, @if.DoNodes, doIfStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                    }
                    else if (status.Condition == false)
                    {
                        if (@if.ElseNodes != null && @if.ElseNodes.Length > 0)
                        {
                            // Build Tasks
                            var elseTasks = NodesToTasks(@if.ElseNodes);

                            // Run Tasks
                            var elseStartNode = GetStartupNode(@if.ElseNodes);

                            RunTasks(elseTasks, @if.ElseNodes, elseStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                        }
                    }

                    // Child node
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == @if.Id);

                    if (childNode != null)
                    {
                        RunTasks(tasks, nodes, childNode, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + @if.Id + " not found.");
            }
        }

        private void RunWhile(Task[] tasks, Node[] nodes, While @while, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var whileTask = GetTask(@while.WhileId);

            if (whileTask != null)
            {
                if (whileTask.IsEnabled && !whileTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected)))
                {
                    while (true)
                    {
                        var status = whileTask.Run();
                        Logs.AddRange(whileTask.Logs);

                        success &= status.Status == Status.Success;
                        warning |= status.Status == Status.Warning;
                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                        if (status.Status == Status.Success && status.Condition)
                        {
                            if (@while.Nodes.Length > 0)
                            {
                                // Build Tasks
                                var doWhileTasks = NodesToTasks(@while.Nodes);

                                // Run Tasks
                                var doWhileStartNode = GetStartupNode(@while.Nodes);

                                RunTasks(doWhileTasks, @while.Nodes, doWhileStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                        else if (status.Condition == false)
                        {
                            break;
                        }
                    }

                    // Child node
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == @while.Id);

                    if (childNode != null)
                    {
                        RunTasks(tasks, nodes, childNode, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + @while.Id + " not found.");
            }
        }

        private void RunSwitch(Task[] tasks, Node[] nodes, Switch @switch, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var switchTask = GetTask(@switch.SwitchId);

            if (switchTask != null)
            {
                if (switchTask.IsEnabled && !switchTask.IsStopped && (!IsApproval || (IsApproval && !IsRejected)))
                {
                    var status = switchTask.Run();
                    Logs.AddRange(switchTask.Logs);

                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                    if (status.Status == Status.Success)
                    {
                        bool aCaseHasBeenExecuted = false;
                        foreach (var @case in @switch.Cases)
                        {
                            if (@case.Value == status.SwitchValue)
                            {
                                if (@case.Nodes.Length > 0)
                                {
                                    // Build Tasks
                                    var switchTasks = NodesToTasks(@case.Nodes);

                                    // Run Tasks
                                    var switchStartNode = GetStartupNode(@case.Nodes);

                                    RunTasks(switchTasks, @case.Nodes, switchStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                aCaseHasBeenExecuted = true;
                                break;
                            }
                        }

                        if (!aCaseHasBeenExecuted && @switch.Default != null && @switch.Default.Any())
                        {
                            // Build Tasks
                            var defalutTasks = NodesToTasks(@switch.Default);

                            // Run Tasks
                            var defaultStartNode = GetStartupNode(@switch.Default);

                            RunTasks(defalutTasks, @switch.Default, defaultStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                        }

                        // Child node
                        var childNode = nodes.FirstOrDefault(n => n.ParentId == @switch.Id);

                        if (childNode != null)
                        {
                            RunTasks(tasks, nodes, childNode, force, ref success, ref warning, ref atLeastOneSucceed);
                        }
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
                    if (_thread != null)
                    {
                        _thread.Abort();
                        _thread.Join();
                    }
                    foreach (var task in Tasks)
                    {
                        task.Stop();
                        Logs.AddRange(task.Logs);
                    }
                    var logs = string.Join("\r\n", Logs);
                    IsWaitingForApproval = false;
                    Database.DecrementRunningCount();
                    Database.IncrementStoppedCount();
                    var entry = Database.GetEntry(Id, InstanceId);
                    entry.Status = Db.Status.Stopped;
                    entry.StatusDate = DateTime.Now;
                    entry.Logs = logs;
                    Database.UpdateEntry(entry.GetDbId(), entry);
                    _historyEntry.Status = Db.Status.Stopped;
                    _historyEntry.StatusDate = DateTime.Now;
                    _historyEntry.Logs = logs;
                    Database.InsertHistoryEntry(_historyEntry);
                    IsRejected = false;
                    Logs.Clear();
                    Jobs.Remove(InstanceId);

                    if (_jobsQueue.Count > 0)
                    {
                        var job = _jobsQueue.Dequeue();
                        job.Workflow.StartAsync(StartedBy);
                    }

                    Load(Xml); // Reload the original workflow

                    Logger.Info($"Workflow {Name} / {jobId} / {InstanceId} stopped.");
                    return true;
                }
                catch (Exception e)
                {
                    _stopCalled = false;
                    var msg = string.Format("An error occured while stopping the workflow : {0}", this);
                    Logger.Error(msg, e);
                    Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + msg + "\r\n" + e);
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
#pragma warning disable 618
                    _thread.Suspend();
#pragma warning restore 618
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
                    var msg = string.Format("An error occured while suspending the workflow : {0}", this);
                    Logger.Error(msg, e);
                    Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + msg + "\r\n" + e);
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
                    var msg = string.Format("An error occured while resuming the workflow : {0}", this);
                    Logger.Error(msg, e);
                    Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + msg + "\r\n" + e);
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
                var task = Tasks.Where(t => t.IsWaitingForApproval).First();
                var dir = Path.Combine(ApprovalFolder, Id.ToString(), InstanceId.ToString(), task.Id.ToString());
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "task.approved"), "Task " + task.Id + " of the workflow " + Id + " approved.");
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
            if (!Directory.Exists(wfTempFolder)) Directory.CreateDirectory(wfTempFolder);

            var wfDayTempFolder = Path.Combine(wfTempFolder, string.Format("{0:yyyy-MM-dd}", DateTime.Now));
            if (!Directory.Exists(wfDayTempFolder)) Directory.CreateDirectory(wfDayTempFolder);

            var wfJobTempFolder = Path.Combine(wfDayTempFolder, string.Format("{0:HH-mm-ss-fff}", DateTime.Now));
            if (!Directory.Exists(wfJobTempFolder)) Directory.CreateDirectory(wfJobTempFolder);

            WorkflowTempFolder = wfJobTempFolder;
        }

        private Node GetStartupNode(IEnumerable<Node> nodes)
        {
            return nodes.First(n => n.ParentId == StartId);
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

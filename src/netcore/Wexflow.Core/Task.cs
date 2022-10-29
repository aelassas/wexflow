using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wexflow.Core
{
    /// <summary>
    /// Task.
    /// </summary>
    public abstract class Task
    {
        /// <summary>
        /// Task Id.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Task name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Task description.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Shows whether this task is enabled or not.
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Shows whether this task is waiting for approval.
        /// </summary>
        public bool IsWaitingForApproval { get; set; }
        /// <summary>
        /// Task settings.
        /// </summary>
        public Setting[] Settings { get; private set; }
        /// <summary>
        /// Workflow.
        /// </summary>
        public Workflow Workflow { get; private set; }
        /// <summary>
        /// Log messages.
        /// </summary>
        public List<string> Logs { get; private set; }
        /// <summary>
        /// Log messages.
        /// </summary>
        public bool IsStopped { get; private set; }
        /// <summary>
        /// Task files.
        /// </summary>
        public List<FileInf> Files
        {
            get
            {
                return Workflow.FilesPerTask[Id];
            }
        }
        /// <summary>
        /// Task entities.
        /// </summary>
        public List<Entity> Entities
        {
            get
            {
                return Workflow.EntitiesPerTask[Id];
            }
        }
        /// <summary>
        /// Hashtable used as shared memory for tasks.
        /// </summary>
        public Hashtable SharedMemory
        {
            get
            {
                return Workflow.SharedMemory;
            }
        }

        private readonly XElement _xElement;

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="xe">XElement.</param>
        /// <param name="wf">Workflow.</param>
		protected Task(XElement xe, Workflow wf)
        {
            Logs = new List<string>();
            IsStopped = false;
            _xElement = xe;
            var xId = xe.Attribute("id");
            if (xId == null) throw new Exception("Task id attribute not found.");
            Id = int.Parse(xId.Value);
            var xName = xe.Attribute("name");
            if (xName == null) throw new Exception("Task name attribute not found.");
            Name = xName.Value;
            var xDesc = xe.Attribute("description");
            if (xDesc == null) throw new Exception("Task description attribute not found.");
            Description = xDesc.Value;
            var xEnabled = xe.Attribute("enabled");
            if (xEnabled == null) throw new Exception("Task enabled attribute not found.");
            IsEnabled = bool.Parse(xEnabled.Value);
            IsWaitingForApproval = false;
            Workflow = wf;
            Workflow.FilesPerTask.Add(Id, new List<FileInf>());
            Workflow.EntitiesPerTask.Add(Id, new List<Entity>());

            // settings
            IList<Setting> settings = new List<Setting>();

            foreach (var xSetting in xe.XPathSelectElements("wf:Setting", Workflow.XmlNamespaceManager))
            {
                // setting name
                var xSettingName = xSetting.Attribute("name");
                if (xSettingName == null) throw new Exception("Setting name not found");
                string settingName = xSettingName.Value;

                // setting value
                var xSettingValue = xSetting.Attribute("value");
                string settingValue = string.Empty;
                if (xSettingValue != null) settingValue = xSettingValue.Value;

                // setting attributes
                IList<Attribute> attributes = new List<Attribute>();

                foreach (var xAttribute in xSetting.Attributes().Where(attr => attr.Name.LocalName != "name" && attr.Name.LocalName != "value"))
                {
                    Attribute attr = new Attribute(xAttribute.Name.LocalName, xAttribute.Value);
                    attributes.Add(attr);
                }

                Setting setting = new Setting(settingName, settingValue, attributes.ToArray());
                settings.Add(setting);
            }

            Settings = settings.ToArray();
        }

        /// <summary>
        /// Starts the task.
        /// </summary>
        /// <returns>Task status.</returns>
        public abstract TaskStatus Run();

        /// <summary>
        /// Stops the task.
        /// </summary>
        public virtual void Stop()
        {
            IsStopped = true;
        }

        /// <summary>
        /// Returns a setting value from its name.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <returns>Setting value.</returns>
        public string GetSetting(string name)
        {
            var xNode = _xElement.XPathSelectElement(string.Format("wf:Setting[@name='{0}']", name), Workflow.XmlNamespaceManager);
            if (xNode != null)
            {
                var xSetting = xNode.Attribute("value");
                if (xSetting == null)
                {
                    throw new Exception("Setting " + name + " value attribute not found.");
                }
                return xSetting.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns a setting value from its name and returns a default value if the setting value is not found.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public string GetSetting(string name, string defaultValue)
        {
            var returnValue = GetSetting(name);

            if (string.IsNullOrEmpty(returnValue))
            {
                return defaultValue;
            }

            return returnValue;
        }

        /// <summary>
        /// Returns a setting value from its name and returns a default value if the setting value is not found.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public T GetSetting<T>(string name, T defaultValue = default(T))
        {
            var returnValue = GetSetting(name);

            if (string.IsNullOrEmpty(returnValue))
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(returnValue, typeof(T), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a setting value from its name and returns a default value if the setting value is not found.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public int GetSettingInt(string name, int defaultValue)
        {
            var value = GetSetting(name, defaultValue.ToString());
            return int.Parse(value);
        }

        /// <summary>
        /// Returns a setting value from its name and returns a default value if the setting value is not found.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public bool GetSettingBool(string name, bool defaultValue)
        {
            var value = GetSetting(name, defaultValue.ToString());
            return bool.Parse(value);
        }

        /// <summary>
        /// Returns a list of setting values from a setting name.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <returns>A list of setting values.</returns>
        public string[] GetSettings(string name)
        {
            return _xElement.XPathSelectElements(string.Format("wf:Setting[@name='{0}']", name), Workflow.XmlNamespaceManager).Select(xe =>
            {
                var xAttribute = xe.Attribute("value");
                if (xAttribute == null)
                {
                    throw new Exception("Setting " + name + " value attribute not found.");
                }

                return xAttribute.Value;
            }).ToArray();
        }

        /// <summary>
        /// Returns a list of integers from a setting name.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <returns>A list of integers.</returns>
        public int[] GetSettingsInt(string name)
        {
            return GetSettings(name).Select(int.Parse).ToArray();
        }

        /// <summary>
        /// Returns a list of setting values as XElements from a setting name.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <returns>A list of setting values as XElements.</returns>
        public XElement[] GetXSettings(string name)
        {
            return _xElement.XPathSelectElements(string.Format("wf:Setting[@name='{0}']", name), Workflow.XmlNamespaceManager).ToArray();
        }

        /// <summary>
        /// Returns a list of the files loaded by this task through selectFiles setting.
        /// </summary>
        /// <returns>A list of the files loaded by this task through selectFiles setting.</returns>
        public FileInf[] SelectFiles()
        {
            var files = new List<FileInf>();
            foreach (var xSelectFile in GetXSettings("selectFiles"))
            {
                var xTaskId = xSelectFile.Attribute("value");
                if (xTaskId != null)
                {
                    var taskId = int.Parse(xTaskId.Value);

                    var qf = QueryFiles(Workflow.FilesPerTask[taskId], xSelectFile).ToArray();

                    files.AddRange(qf);
                }
                else
                {
                    var qf = (from lf in Workflow.FilesPerTask.Values
                              from f in QueryFiles(lf, xSelectFile)
                              select f).Distinct().ToArray();

                    files.AddRange(qf);
                }
            }
            return files.ToArray();
        }

        /// <summary>
        /// Filters a list of files from the tags in selectFiles setting.
        /// </summary>
        /// <param name="files">Files to filter.</param>
        /// <param name="xSelectFile">selectFile as an XElement</param>
        /// <returns>A list of files from the tags in selectFiles setting.</returns>
        public IEnumerable<FileInf> QueryFiles(IEnumerable<FileInf> files, XElement xSelectFile)
        {
            var fl = new List<FileInf>();

            if (xSelectFile.Attributes().Count(t => t.Name != "value") == 1)
            {
                return files;
            }

            foreach (var file in files)
            {
                // Check file tags
                bool ok = true;
                foreach (var xa in xSelectFile.Attributes())
                {
                    if (xa.Name != "name" && xa.Name != "value")
                    {
                        ok &= file.Tags.Any(tag => tag.Key == xa.Name && tag.Value == xa.Value);
                    }
                }

                if (ok)
                {
                    fl.Add(file);
                }
            }

            return fl;
        }

        /// <summary>
        /// Returns a list of the entities loaded by this task through selectEntities setting.
        /// </summary>
        /// <returns>A list of the entities loaded by this task through selectEntities setting.</returns>
        public Entity[] SelectEntities()
        {
            var entities = new List<Entity>();
            foreach (string id in GetSettings("selectEntities"))
            {
                var taskId = int.Parse(id);
                entities.AddRange(Workflow.EntitiesPerTask[taskId]);
            }
            return entities.ToArray();
        }

        /// <summary>
        /// Returns an object from the Hashtable through selectObject setting.
        /// </summary>
        /// <returns>An object from the Hashtable through selectObject setting.</returns>
        public object SelectObject()
        {
            var key = GetSetting("selectObject");
            if (SharedMemory.ContainsKey(key))
            {
                return SharedMemory[key];
            }
            return null;
        }

        private string BuildLogMsg(string msg)
        {
            return string.Format("{0} [{1}] {2}", Workflow.LogTag, GetType().Name, msg);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public void Info(string msg)
        {
            var message = BuildLogMsg(msg);
            Logger.Info(message);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  INFO - " + message);
        }

        /// <summary>
        /// Logs a formatted information message.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="args">Arguments.</param>
        public void InfoFormat(string msg, params object[] args)
        {
            var message = string.Format(BuildLogMsg(msg), args);
            Logger.Info(message);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  INFO - " + message);
        }

        /// <summary>
        /// Logs a Debug log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public void Debug(string msg)
        {
            var message = BuildLogMsg(msg);
            Logger.Debug(msg);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  DEBUG - " + message);
        }

        /// <summary>
        /// Logs a formatted debug message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public void DebugFormat(string msg, params object[] args)
        {
            var message = string.Format(BuildLogMsg(msg), args);
            Logger.DebugFormat(message);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  DEBUG - " + message);
        }

        /// <summary>
        /// Logs an error log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public void Error(string msg)
        {
            var message = BuildLogMsg(msg);
            Logger.Error(message);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + message);
        }

        /// <summary>
        /// Logs a formatted error message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public void ErrorFormat(string msg, params object[] args)
        {
            var message = string.Format(BuildLogMsg(msg), args);
            Logger.Error(message);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + message);
        }

        /// <summary>
        /// Logs an error message and an exception.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="e">Exception.</param>
        public void Error(string msg, Exception e)
        {
            var message = BuildLogMsg(msg);
            Logger.Error(message, e);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + message + "\r\n" + e);
        }

        /// <summary>
        /// Logs a formatted log message and an exception.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="e">Exception.</param>
        /// <param name="args">Arguments.</param>
        public void ErrorFormat(string msg, Exception e, params object[] args)
        {
            var message = string.Format(BuildLogMsg(msg), args);
            Logger.Error(message, e);
            Logs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  ERROR - " + message + "\r\n" + e);
        }
    }
}

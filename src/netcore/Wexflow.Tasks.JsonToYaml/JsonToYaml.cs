using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using YamlDotNet.Serialization;

namespace Wexflow.Tasks.JsonToYaml
{
    public class JsonToYaml : Task
    {
        public JsonToYaml(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Converting JSON files to YAML files...");

            bool success;
            var atLeastOneSuccess = false;
            try
            {
                success = ConvertFiles(ref atLeastOneSuccess);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting files.", e);
                success = false;
            }

            var status = Status.Success;
            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool ConvertFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var yamlFiles = SelectFiles();

            foreach (var yamlFile in yamlFiles)
            {
                try
                {
                    var source = File.ReadAllText(yamlFile.Path);

                    ExpandoObjectConverter expConverter = new();
                    dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(source, expConverter);

                    Serializer serializer = new();
                    dynamic yaml = serializer.Serialize(deserializedObject);

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(yamlFile.FileName) + ".yml");
                    File.WriteAllText(destPath, yaml);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The JSON file {0} has been converted -> {1}", yamlFile.Path, destPath);
                    if (!atLeastOneSuccess)
                    {
                        atLeastOneSuccess = true;
                    }
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while converting the JSON file {0}: {1}", yamlFile.Path, e.Message);
                    success = false;
                }
                finally
                {
                    WaitOne();
                }
            }
            return success;
        }
    }
}

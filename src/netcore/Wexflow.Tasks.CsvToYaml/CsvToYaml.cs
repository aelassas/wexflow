using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using YamlDotNet.Serialization;

namespace Wexflow.Tasks.CsvToYaml
{
    public class CsvToYaml : Task
    {
        public string Separator { get; }

        public CsvToYaml(XElement xe, Workflow wf) : base(xe, wf)
        {
            Separator = GetSetting("separator", ";");
        }

        public override TaskStatus Run()
        {
            Info("Converting CSV files to YAML files...");

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
            var csvFiles = SelectFiles();

            foreach (var csvFile in csvFiles)
            {
                try
                {
                    var yaml = Convert(csvFile.Path, Separator);
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(csvFile.FileName) + ".yml");
                    File.WriteAllText(destPath, yaml);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The CSV file {0} has been converted -> {1}", csvFile.Path, destPath);
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
                    ErrorFormat("An error occured while converting the CSV file {0}: {1}", csvFile.Path, e.Message);
                    success = false;
                }
                finally
                {
                    WaitOne();
                }
            }

            return success;
        }

        private static string Convert(string path, string separator)
        {
            List<string[]> csv = [];
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                csv.Add(line.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries));
            }

            var properties = lines[0].Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            List<Dictionary<string, string>> listObjResult = [];

            for (var i = 1; i < lines.Length; i++)
            {
                Dictionary<string, string> objResult = [];
                for (var j = 0; j < properties.Length; j++)
                {
                    objResult.Add(properties[j], csv[i][j]);
                }

                listObjResult.Add(objResult);
            }

            Serializer serializer = new();
            var yaml = serializer.Serialize(listObjResult);
            return yaml;
        }
    }
}

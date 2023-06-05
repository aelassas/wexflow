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
        public string Separator { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public CsvToYaml(XElement xe, Workflow wf) : base(xe, wf)
        {
            Separator = GetSetting("separator", ";");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Converting CSV files to YAML files...");


            bool success = true;
            bool atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = ConvertFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = ConvertFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting files.", e);
                success = false;
            }

            Status status = Status.Success;

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
            bool success = true;
            FileInf[] csvFiles = SelectFiles();

            foreach (FileInf csvFile in csvFiles)
            {
                try
                {
                    string yaml = Convert(csvFile.Path, Separator);
                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(csvFile.FileName) + ".yml");
                    File.WriteAllText(destPath, yaml);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The CSV file {0} has been converted -> {1}", csvFile.Path, destPath);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while converting the CSV file {0}: {1}", csvFile.Path, e.Message);
                    success = false;
                }
            }

            return success;
        }

        private string Convert(string path, string separator)
        {
            List<string[]> csv = new List<string[]>();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                csv.Add(line.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
            }

            string[] properties = lines[0].Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            List<Dictionary<string, string>> listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                Dictionary<string, string> objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                {
                    objResult.Add(properties[j], csv[i][j]);
                }

                listObjResult.Add(objResult);
            }

            Serializer serializer = new Serializer();
            string yaml = serializer.Serialize(listObjResult);
            return yaml;
        }

    }
}

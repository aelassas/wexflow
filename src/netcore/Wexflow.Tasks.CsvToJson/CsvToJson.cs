using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.CsvToJson
{
    public class CsvToJson : Task
    {
        public string Separator { get; private set; }

        public CsvToJson(XElement xe, Workflow wf) : base(xe, wf)
        {
            Separator = GetSetting("separator", ";");
        }

        public override TaskStatus Run()
        {
            Info("Converting CSV files to JSON files...");

            bool success;
            bool atLeastOneSuccess = false;
            try
            {
                success = ConvertFiles(ref atLeastOneSuccess);
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
                    string json = Convert(csvFile.Path, Separator);
                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(csvFile.FileName) + ".json");
                    File.WriteAllText(destPath, json);
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
            List<string[]> csv = new();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                csv.Add(line.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
            }

            string[] properties = lines[0].Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            List<Dictionary<string, string>> listObjResult = new();

            for (int i = 1; i < lines.Length; i++)
            {
                Dictionary<string, string> objResult = new();
                for (int j = 0; j < properties.Length; j++)
                {
                    objResult.Add(properties[j], csv[i][j]);
                }

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }

    }
}

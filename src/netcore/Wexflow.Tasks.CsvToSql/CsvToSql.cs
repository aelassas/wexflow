using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.CsvToSql
{
    public class CsvToSql : Task
    {
        public string TableName { get; set; }
        public string Separator { get; set; }

        public CsvToSql(XElement xe, Workflow wf)
           : base(xe, wf)
        {
            TableName = GetSetting("tableName");
            Separator = GetSetting("separator");
        }

        public override TaskStatus Run()
        {
            Info("Converting CSV to SQL...");

            var succeeded = true;
            var atLeastOneSucceed = false;

            try
            {
                var csvFiles = SelectFiles();

                foreach (var csvFile in csvFiles)
                {
                    var sqlPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"{Path.GetFileNameWithoutExtension(csvFile.FileName)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.sql");
                    succeeded &= ConvertCsvToSql(csvFile.Path, sqlPath, TableName, Separator);
                    if (succeeded && !atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting CSV files to SQL: {0}", e.Message);
                return new TaskStatus(Status.Error);
            }

            var status = Status.Success;

            if (!succeeded && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!succeeded)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool ConvertCsvToSql(string csvPath, string sqlPath, string tableName, string separator)
        {
            try
            {
                using StreamReader sr = new(csvPath);
                using StreamWriter sw = new(sqlPath);
                var columnsLine = sr.ReadLine(); // First line contains columns
                string line;
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    sw.Write($"INSERT INTO {tableName}({columnsLine!.Replace(separator, ",").TrimEnd(',')}) VALUES ");
                    sw.Write("(");
                    var values = line.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var value in values)
                    {
                        if (int.TryParse(value, out var i))
                        {
                            sw.Write(i);
                        }
                        else if (double.TryParse(value, out var d))
                        {
                            sw.Write(d);
                        }
                        else if (float.TryParse(value, out var f))
                        {
                            sw.Write(f);
                        }
                        else
                        {
                            sw.Write($"'{value}'");
                        }

                        if (!values.Last().Equals(value, StringComparison.Ordinal))
                        {
                            sw.Write(", ");
                        }
                    }
                    sw.Write(");\r\n");
                }

                Files.Add(new FileInf(sqlPath, Id));
                InfoFormat("SQL script {0} created from {1} with success.", sqlPath, csvPath);

                return true;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting the CSV {0} to SQL: {1}", csvPath, e.Message);
                return false;
            }
            finally
            {
                WaitOne();
            }
        }
    }
}

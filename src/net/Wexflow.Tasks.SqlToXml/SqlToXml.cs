using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Teradata.Client.Provider;
using Wexflow.Core;

namespace Wexflow.Tasks.SqlToXml
{
    public enum Type
    {
        SqlServer,
        Access,
        Oracle,
        MySql,
        Sqlite,
        PostGreSql,
        Teradata,
        Odbc
    }

    public class SqlToXml : Task
    {
        public Type DbType { get; set; }
        public string ConnectionString { get; set; }
        public string SqlScript { get; set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }
        public bool ExcludeEmptyValues { get; }

        public SqlToXml(XElement xe, Workflow wf) : base(xe, wf)
        {
            DbType = (Type)Enum.Parse(typeof(Type), GetSetting("type"), true);
            ConnectionString = GetSetting("connectionString");
            SqlScript = GetSetting("sql", string.Empty);
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
            ExcludeEmptyValues = bool.Parse(GetSetting("excludeEmptyValues", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Executing SQL scripts...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = ExecuteSqlFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = ExecuteSqlFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while copying files.", e);
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
            return new TaskStatus(status, false);
        }

        private bool ExecuteSqlFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            // Execute SqlScript if necessary
            try
            {
                if (!string.IsNullOrEmpty(SqlScript))
                {
                    ExecuteSql(SqlScript);
                    Info("The script has been executed through the sql option of the task.");
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing sql script. Error: {0}", e.Message);
                success = false;
            }

            // Execute SQL files scripts
            foreach (var file in SelectFiles())
            {
                try
                {
                    var sql = File.ReadAllText(file.Path);
                    ExecuteSql(sql);
                    InfoFormat("The script {0} has been executed.", file.Path);

                    if (!atLeastOneSuccess)
                    {
                        atLeastOneSuccess = true;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while executing sql script {0}. Error: {1}", file.Path, e.Message);
                    success = false;
                }
            }
            return success;
        }

        private void ExecuteSql(string sql)
        {
            switch (DbType)
            {
                case Type.SqlServer:
                    using (var connection = new SqlConnection(ConnectionString))
                    using (var command = new SqlCommand(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.Access:
                    using (var conn = new OleDbConnection(ConnectionString))
                    using (var comm = new OleDbCommand(sql, conn))
                    {
                        ConvertToXml(conn, comm);
                    }
                    break;
                case Type.Oracle:
                    using (var connection = new OracleConnection(ConnectionString))
                    using (var command = new OracleCommand(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.MySql:
                    using (var connection = new MySqlConnection(ConnectionString))
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.Sqlite:
                    using (var connection = new SQLiteConnection(ConnectionString))
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.PostGreSql:
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.Teradata:
                    using (var connenction = new TdConnection(ConnectionString))
                    using (var command = new TdCommand(sql, connenction))
                    {
                        ConvertToXml(connenction, command);
                    }
                    break;
                case Type.Odbc:
                    using (var connenction = new OdbcConnection(ConnectionString))
                    using (var command = new OdbcCommand(sql, connenction))
                    {
                        ConvertToXml(connenction, command);
                    }
                    break;
                default:
                    break;
            }
        }

        private void ConvertToXml(DbConnection connection, DbCommand command)
        {
            connection.Open();
            var reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                var columns = new List<string>();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"SqlToXml_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                var xdoc = new XDocument();
                var xobjects = new XElement("Records");

                while (reader.Read())
                {
                    var xobject = new XElement("Record");

                    foreach (var column in columns)
                    {
                        var xmlvalue = CleanInvalidXmlChars(reader[column].ToString());
                        var columntype = reader[column].GetType();
                        if (
                            (columntype == typeof(int) && int.TryParse(xmlvalue, out var number) && number == 0) ||
                            (columntype == typeof(decimal) && decimal.TryParse(xmlvalue, out var decnumber) && decnumber == 0) ||
                            (columntype == typeof(DateTime) && (Convert.ToDateTime(xmlvalue) == SqlDateTime.MinValue || xmlvalue == "01-01-1900 00:00:00"))
                            )
                        {
                            xmlvalue = "";
                        }
                        if (!ExcludeEmptyValues || !string.IsNullOrEmpty(xmlvalue))
                        {
                            xobject.Add(new XElement("Cell"
                                , new XAttribute("column", SecurityElement.Escape(column) ?? throw new InvalidOperationException())
                                , new XAttribute("value", SecurityElement.Escape(xmlvalue) ?? throw new InvalidOperationException())));
                        }
                    }
                    xobjects.Add(xobject);
                }

                xdoc.Add(xobjects);
                xdoc.Save(destPath);
                Files.Add(new FileInf(destPath, Id));
                InfoFormat("XML file generated: {0}", destPath);
            }
        }

        public static string CleanInvalidXmlChars(string text)
        {
            // From xml spec valid chars: 
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]     
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. 
            var re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }
    }
}

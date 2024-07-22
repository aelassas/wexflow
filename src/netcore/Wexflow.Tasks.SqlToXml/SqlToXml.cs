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
        public bool ExcludeEmptyValues { get; set; }

        public SqlToXml(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            DbType = (Type)Enum.Parse(typeof(Type), GetSetting("type"), true);
            ConnectionString = GetSetting("connectionString");
            SqlScript = GetSetting("sql", string.Empty);
            ExcludeEmptyValues = bool.Parse(GetSetting("excludeEmptyValues", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Executing SQL scripts...");

            var success = true;
            var atLeastOneSucceed = false;

            // Execute SqlScript if necessary
            try
            {
                if (!string.IsNullOrEmpty(SqlScript))
                {
                    ExecuteSql(SqlScript);
                    Info("The script has been executed through the sql option of the task.");
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing sql script. Error: {0}", e.Message);
                success = false;
            }
            finally
            {
                WaitOne();
            }

            // Execute SQL files scripts
            foreach (var file in SelectFiles())
            {
                try
                {
                    var sql = File.ReadAllText(file.Path);
                    ExecuteSql(sql);
                    InfoFormat("The script {0} has been executed.", file.Path);

                    if (!atLeastOneSucceed)
                    {
                        atLeastOneSucceed = true;
                    }
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while executing sql script {0}. Error: {1}", file.Path, e.Message);
                    success = false;
                }
                finally
                {
                    WaitOne();
                }
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
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

        private void ExecuteSql(string sql)
        {
            switch (DbType)
            {
                case Type.SqlServer:
                    using (SqlConnection connection = new(ConnectionString))
                    using (SqlCommand command = new(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.Access:
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    using (OleDbConnection conn = new(ConnectionString))
                    using (OleDbCommand comm = new(sql, conn))
                    {
                        ConvertToXml(conn, comm);
                    }
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                    break;
                case Type.Oracle:
                    using (OracleConnection connection = new(ConnectionString))
                    using (OracleCommand command = new(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.MySql:
                    using (MySqlConnection connection = new(ConnectionString))
                    using (MySqlCommand command = new(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.Sqlite:
                    using (SQLiteConnection connection = new(ConnectionString))
                    using (SQLiteCommand command = new(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.PostGreSql:
                    using (NpgsqlConnection connection = new(ConnectionString))
                    using (NpgsqlCommand command = new(sql, connection))
                    {
                        ConvertToXml(connection, command);
                    }
                    break;
                case Type.Teradata:
                    using (TdConnection connenction = new(ConnectionString))
                    using (TdCommand command = new(sql, connenction))
                    {
                        ConvertToXml(connenction, command);
                    }
                    break;
                case Type.Odbc:
                    using (OdbcConnection connenction = new(ConnectionString))
                    using (OdbcCommand command = new(sql, connenction))
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
                List<string> columns = [];

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"SqlToXml_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                XDocument xdoc = new();
                XElement xobjects = new("Records");

                while (reader.Read())
                {
                    XElement xobject = new("Record");

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
                            , new XAttribute("column", SecurityElement.Escape(column))
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

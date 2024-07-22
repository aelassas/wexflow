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
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Teradata.Client.Provider;
using Wexflow.Core;

namespace Wexflow.Tasks.SqlToCsv
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

    public class SqlToCsv : Task
    {
        public Type DbType { get; }
        public string ConnectionString { get; }
        public string SqlScript { get; }
        public string Separator { get; }
        public string QuoteString { get; }
        public string EndOfLine { get; }
        public bool Headers { get; }
        public bool SingleRecordHeaders { get; }
        public bool DoNotGenerateFilesIfEmpty { get; }

        public SqlToCsv(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            DbType = (Type)Enum.Parse(typeof(Type), GetSetting("type"), true);
            ConnectionString = GetSetting("connectionString");
            SqlScript = GetSetting("sql", string.Empty);
            QuoteString = GetSetting("quote", string.Empty);
            EndOfLine = GetSetting("endline", "\r\n");
            Separator = QuoteString + GetSetting("separator", ";") + QuoteString;
            if (bool.TryParse(GetSetting("headers", bool.TrueString), out var result1))
            {
                Headers = result1;
            }

            if (bool.TryParse(GetSetting("singlerecordheaders", bool.TrueString), out var result2))
            {
                SingleRecordHeaders = result2;
            }

            DoNotGenerateFilesIfEmpty = bool.Parse(GetSetting("doNotGenerateFilesIfEmpty", "false"));
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
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Access:

#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    using (OleDbConnection connection = new(ConnectionString))
                    using (OleDbCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme

                    break;
                case Type.Oracle:
                    using (OracleConnection connection = new(ConnectionString))
                    using (OracleCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.MySql:
                    using (MySqlConnection connection = new(ConnectionString))
                    using (MySqlCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Sqlite:
                    using (SQLiteConnection connection = new(ConnectionString))
                    using (SQLiteCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.PostGreSql:
                    using (NpgsqlConnection connection = new(ConnectionString))
                    using (NpgsqlCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Teradata:
                    using (TdConnection connection = new(ConnectionString))
                    using (TdCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Odbc:
                    using (OdbcConnection connection = new(ConnectionString))
                    using (OdbcCommand command = new(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                default:
                    break;
            }
        }

        private void ConvertToCsv(DbConnection conn, DbCommand comm)
        {
            conn.Open();
            var reader = comm.ExecuteReader();
            var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                $"SqlToCsv_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.csv");

            using (StreamWriter sw = new(destPath))
            {
                var hasRows = reader.HasRows;

                while (hasRows)
                {
                    List<string> columns = [];
                    List<string> values = [];
                    var readColumns = false;
                    var headerDone = false;
                    var readRecord = false;
                    while (reader.Read())
                    {
                        if (readRecord)
                        {
                            if (!headerDone && Headers)
                            {
                                headerDone = true;
                                if (Headers)
                                {
                                    sw.Write(QuoteString + string.Join(Separator, columns) + QuoteString);
                                    sw.Write(EndOfLine);
                                }
                            }
                            sw.Write(QuoteString + string.Join(Separator, values) + QuoteString);
                            sw.Write(EndOfLine);
                            values.Clear();
                        }

                        int i;
                        if (!readColumns)
                        {
                            for (i = 0; i < reader.FieldCount; i++)
                            {
                                columns.Add(reader.GetName(i));
                            }
                            readColumns = true;
                        }
                        for (i = 0; i < reader.FieldCount; i++)
                        {
                            values.Add(reader[i].ToString());
                        }
                        readRecord = true;
                    }

                    if (!headerDone && SingleRecordHeaders && Headers)
                    {
                        sw.Write(QuoteString + string.Join(Separator, columns) + QuoteString);
                        sw.Write(EndOfLine);
                    }

                    sw.Write(QuoteString + string.Join(Separator, values) + QuoteString);
                    sw.Write(EndOfLine);
                    values.Clear();
                    columns.Clear();
                    hasRows = reader.NextResult();
                }
            }

            if (!reader.HasRows && DoNotGenerateFilesIfEmpty)
            {
                InfoFormat("No file was generated because the result set is empty.");
            }
            else
            {
                Files.Add(new FileInf(destPath, Id));
                InfoFormat("CSV file generated: {0}", destPath);
            }
        }
    }
}

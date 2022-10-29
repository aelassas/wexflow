using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Teradata.Client.Provider;
using Wexflow.Core;
using System.Data.Odbc;

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
        public bool SingleRecordHeaders{ get; }
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
            if (bool.TryParse(GetSetting("headers", bool.TrueString), out var result1)) Headers = result1;
            if (bool.TryParse(GetSetting("singlerecordheaders", bool.TrueString), out var result2)) SingleRecordHeaders = result2;
            DoNotGenerateFilesIfEmpty = bool.Parse(GetSetting("doNotGenerateFilesIfEmpty", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Executing SQL scripts...");

            bool success = true;
            bool atLeastOneSucceed = false;

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
            foreach (FileInf file in SelectFiles())
            {
                try
                {
                    var sql = File.ReadAllText(file.Path);
                    ExecuteSql(sql);
                    InfoFormat("The script {0} has been executed.", file.Path);

                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
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
                    using (var connection = new SqlConnection(ConnectionString))
                    using (var command = new SqlCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Access:
                    using (var connection = new OleDbConnection(ConnectionString))
                    using (var command = new OleDbCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Oracle:
                    using (var connection = new OracleConnection(ConnectionString))
                    using (var command = new OracleCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.MySql:
                    using (var connection = new MySqlConnection(ConnectionString))
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Sqlite:
                    using (var connection = new SQLiteConnection(ConnectionString))
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.PostGreSql:
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Teradata:
                    using (var connection = new TdConnection(ConnectionString))
                    using (var command = new TdCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
                case Type.Odbc:
                    using (var connection = new OdbcConnection(ConnectionString))
                    using (var command = new OdbcCommand(sql, connection))
                    {
                        ConvertToCsv(connection, command);
                    }
                    break;
            }
        }

        private void ConvertToCsv(DbConnection conn, DbCommand comm)
        {
            conn.Open();
            var reader = comm.ExecuteReader();
            string destPath = Path.Combine(Workflow.WorkflowTempFolder, string.Format("SqlToCsv_{0:yyyy-MM-dd-HH-mm-ss-fff}.csv", DateTime.Now));

            using (var sw = new StreamWriter(destPath))
            {
                bool hasRows = reader.HasRows;

                while (hasRows)
                {
                    List<string> columns = new List<string>();
                    List<string> values = new List<string>();
                    bool readColumns = false;
                    bool headerDone = false;
                    bool readRecord = false;
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

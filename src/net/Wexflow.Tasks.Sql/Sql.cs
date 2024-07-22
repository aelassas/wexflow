using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
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

namespace Wexflow.Tasks.Sql
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

    public class Sql : Task
    {
        public Type DbType { get; set; }
        public string ConnectionString { get; set; }
        public string SqlScript { get; set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public Sql(XElement xe, Workflow wf) : base(xe, wf)
        {
            DbType = (Type)Enum.Parse(typeof(Type), GetSetting("type"), true);
            ConnectionString = GetSetting("connectionString");
            SqlScript = GetSetting("sql", string.Empty);
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Executing SQL scripts...");

            bool success;
            var atLeastOneSuccess = false;

            // Execute SQL scripts
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
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        var comm = new SqlCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.Access:
                    using (var conn = new OleDbConnection(ConnectionString))
                    {
                        var comm = new OleDbCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.Oracle:
                    using (var conn = new OracleConnection(ConnectionString))
                    {
                        var comm = new OracleCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.MySql:
                    using (var conn = new MySqlConnection(ConnectionString))
                    {
                        var comm = new MySqlCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.Sqlite:
                    using (var conn = new SQLiteConnection(ConnectionString))
                    {
                        var comm = new SQLiteCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.PostGreSql:
                    using (var conn = new NpgsqlConnection(ConnectionString))
                    {
                        var comm = new NpgsqlCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.Teradata:
                    using (var conn = new TdConnection(ConnectionString))
                    {
                        var comm = new TdCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                case Type.Odbc:
                    using (var conn = new OdbcConnection(ConnectionString))
                    {
                        var comm = new OdbcCommand(sql, conn);
                        ExecSql(conn, comm);
                    }
                    break;
                default:
                    break;
            }
        }

        private void ExecSql(DbConnection conn, DbCommand comm)
        {
            conn.Open();
            _ = comm.ExecuteNonQuery();
        }
    }
}

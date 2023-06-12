using System.Data.SqlClient;

namespace Wexflow.Core.Db.SQLServer
{
    public class Helper
    {
        private readonly string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void CreateDatabaseIfNotExists(string server, bool trustedConnection, string userId, string password, string databaseName)
        {
            using (var conn = new SqlConnection("Server=" + server + (trustedConnection ? ";Trusted_Connection=True;" : ";User Id=" + userId + ";Password=" + password + ";")))
            {
                conn.Open();

                using (var command1 = new SqlCommand("SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = N'" + databaseName + "'", conn))
                {
                    var count = (int)command1.ExecuteScalar();

                    if (count == 0)
                    {
                        using (var command2 = new SqlCommand("CREATE DATABASE " + databaseName + ";", conn))
                        {
                            _ = command2.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SqlCommand("IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = '" + tableName + "') CREATE TABLE " + tableName + tableStruct + ";", conn))
                {
                    _ = command.ExecuteNonQuery();
                }
            }
        }
    }
}

using System.Data.SqlClient;

namespace Wexflow.Core.Db.SQLServer
{
    public class Helper(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public static void CreateDatabaseIfNotExists(string server, bool trustedConnection, string userId, string password, string databaseName)
        {
            using SqlConnection conn = new("Server=" + server + (trustedConnection ? ";Trusted_Connection=True;" : ";User Id=" + userId + ";Password=" + password + ";"));
            conn.Open();

            using SqlCommand command1 = new("SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = N'" + databaseName + "'", conn);

            var count = (int)command1.ExecuteScalar();

            if (count == 0)
            {
                using SqlCommand command2 = new("CREATE DATABASE " + databaseName + ";", conn);

                _ = command2.ExecuteNonQuery();
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            using SqlCommand command = new("IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = '" + tableName + "') CREATE TABLE " + tableName + tableStruct + ";", conn);

            _ = command.ExecuteNonQuery();
        }
    }
}

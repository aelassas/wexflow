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
            using (SqlConnection conn = new SqlConnection("Server=" + server + (trustedConnection ? ";Trusted_Connection=True;" : ";User Id=" + userId + ";Password=" + password + ";")))
            {
                conn.Open();

                using (SqlCommand command1 = new SqlCommand("SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = N'" + databaseName + "'", conn))
                {

                    int count = (int)command1.ExecuteScalar();

                    if (count == 0)
                    {
                        using (SqlCommand command2 = new SqlCommand("CREATE DATABASE " + databaseName + ";", conn))
                        {

                            command2.ExecuteNonQuery();
                        }
                    }
                }

            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand command = new SqlCommand("IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = '" + tableName + "') CREATE TABLE " + tableName + tableStruct + ";", conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}

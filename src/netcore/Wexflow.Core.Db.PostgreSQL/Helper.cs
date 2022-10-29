using Npgsql;

namespace Wexflow.Core.Db.PostgreSQL
{
    public class Helper
    {
        private string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateDatabaseIfNotExists(string server, string userId, string password, string databaseName)
        {
            using (var conn = new NpgsqlConnection("Server=" + server + ";User Id=" + userId + ";Password=" + password + ";"))
            {
                conn.Open();

                using (var command1 = new NpgsqlCommand("SELECT COUNT(*) FROM pg_database WHERE datname = '" + databaseName + "'", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    if (count == 0)
                    {
                        using (var command2 = new NpgsqlCommand("CREATE DATABASE " + databaseName + ";", conn))
                        {
                            command2.ExecuteNonQuery();
                        }
                    }
                }

            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}

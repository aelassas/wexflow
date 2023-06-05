using Npgsql;

namespace Wexflow.Core.Db.PostgreSQL
{
    public class Helper
    {
        private readonly string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void CreateDatabaseIfNotExists(string server, string userId, string password, string databaseName, int port)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection("Server=" + server + ";User Id=" + userId + ";Password=" + password + ";Database=postgres" + ";Port=" + port))
            {
                conn.Open();

                using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT COUNT(*) FROM pg_database WHERE datname = '" + databaseName + "'", conn))
                {

                    long count = (long)command1.ExecuteScalar();

                    if (count == 0)
                    {
                        using (NpgsqlCommand command2 = new NpgsqlCommand("CREATE DATABASE " + databaseName + ";", conn))
                        {
                            command2.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}

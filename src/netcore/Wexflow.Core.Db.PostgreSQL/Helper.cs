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
            using NpgsqlConnection conn = new("Server=" + server + ";User Id=" + userId + ";Password=" + password + ";Database=postgres" + ";Port=" + port + ";");
            conn.Open();

            using NpgsqlCommand command1 = new("SELECT COUNT(*) FROM pg_database WHERE datname = '" + databaseName + "'", conn);

            var count = (long)command1.ExecuteScalar()!;

            if (count == 0)
            {
                using NpgsqlCommand command2 = new("CREATE DATABASE " + databaseName + ";", conn);
                _ = command2.ExecuteNonQuery();
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using NpgsqlConnection conn = new(_connectionString);
            conn.Open();

            using NpgsqlCommand command = new("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn);
            _ = command.ExecuteNonQuery();
        }
    }
}

using MySqlConnector;

namespace Wexflow.Core.Db.MariaDB
{
    public class Helper(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public static void CreateDatabaseIfNotExists(string server, int port, string user, string password, string database)
        {
            using MySqlConnection conn = new("SERVER=" + server + ";PORT=" + port + ";USER=" + user + ";PASSWORD=" + password + ";");
            conn.Open();

            using MySqlCommand command = new("CREATE DATABASE IF NOT EXISTS " + database + ";", conn);

            _ = command.ExecuteNonQuery();
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using MySqlConnection conn = new(_connectionString);
            conn.Open();

            using MySqlCommand command = new("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn);

            _ = command.ExecuteNonQuery();
        }
    }
}

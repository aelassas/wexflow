using MySqlConnector;

namespace Wexflow.Core.Db.MariaDB
{
    public class Helper
    {
        private readonly string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void CreateDatabaseIfNotExists(string server, int port, string user, string password, string database)
        {
            using (var conn = new MySqlConnection("SERVER=" + server + ";PORT=" + port + ";USER=" + user + ";PASSWORD=" + password + ";"))
            {
                conn.Open();

                using (var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS " + database + ";", conn))
                {
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn))
                {
                    _ = command.ExecuteNonQuery();
                }
            }
        }
    }
}

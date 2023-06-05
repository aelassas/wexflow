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
            using (MySqlConnection conn = new MySqlConnection("SERVER=" + server + ";PORT=" + port + ";USER=" + user + ";PASSWORD=" + password + ";"))
            {
                conn.Open();

                using (MySqlCommand command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS " + database + ";", conn))
                {

                    command.ExecuteNonQuery();
                }

            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand command = new MySqlCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}

using MySqlConnector;

namespace Wexflow.Core.Db.MySQL
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
            using (var conn = new MySqlConnection("Server=" + server + ";Uid=" + userId + ";Pwd=" + password + ";"))
            {
                conn.Open();

                using (var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS " + databaseName + ";", conn))
                {

                    command.ExecuteNonQuery();
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

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}

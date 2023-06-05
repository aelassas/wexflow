using System.Data.SQLite;
using System.IO;

namespace Wexflow.Core.Db.SQLite
{
    public class Helper
    {
        private readonly string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void CreateDatabaseIfNotExists(string dataSource)
        {
            if (!File.Exists(dataSource))
            {
                SQLiteConnection.CreateFile(dataSource);
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}

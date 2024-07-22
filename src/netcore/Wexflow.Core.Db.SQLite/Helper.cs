using System.Data.SQLite;
using System.IO;

namespace Wexflow.Core.Db.SQLite
{
    public class Helper(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public static void CreateDatabaseIfNotExists(string dataSource)
        {
            if (!File.Exists(dataSource))
            {
                SQLiteConnection.CreateFile(dataSource);
            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using SQLiteConnection conn = new(_connectionString);
            conn.Open();

            using SQLiteCommand command = new("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn);

            _ = command.ExecuteNonQuery();
        }
    }
}

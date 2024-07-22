using FirebirdSql.Data.FirebirdClient;

namespace Wexflow.Core.Db.Firebird
{
    public class Helper(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using FbConnection conn = new(_connectionString);
            conn.Open();

            using FbCommand command = new("select 1 from rdb$relations where rdb$relation_name = '" + tableName.ToUpper() + "';", conn);
            var res = command.ExecuteScalar();
            if (res == null)
            {
                using FbCommand cmd = new("create table " + tableName + tableStruct + ";", conn);
                _ = cmd.ExecuteNonQuery();
            }
        }
    }
}

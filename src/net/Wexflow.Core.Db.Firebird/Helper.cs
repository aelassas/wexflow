using FirebirdSql.Data.FirebirdClient;

namespace Wexflow.Core.Db.Firebird
{
    public class Helper
    {
        private readonly string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new FbConnection(_connectionString))
            {
                conn.Open();

                using (var command = new FbCommand("select 1 from rdb$relations where rdb$relation_name = '" + tableName.ToUpper() + "';", conn))
                {
                    var res = command.ExecuteScalar();
                    if (res == null)
                    {
                        using (var cmd = new FbCommand("create table " + tableName + tableStruct + ";", conn))
                        {
                            _ = cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}

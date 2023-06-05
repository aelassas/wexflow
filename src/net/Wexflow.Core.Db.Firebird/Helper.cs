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
            using (FbConnection conn = new FbConnection(_connectionString))
            {
                conn.Open();

                using (FbCommand command = new FbCommand("select 1 from rdb$relations where rdb$relation_name = '" + tableName.ToUpper() + "';", conn))
                {
                    object res = command.ExecuteScalar();
                    if (res == null)
                    {
                        using (FbCommand cmd = new FbCommand("create table " + tableName + tableStruct + ";", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

    }
}

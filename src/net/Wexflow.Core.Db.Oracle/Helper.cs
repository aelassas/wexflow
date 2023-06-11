using Oracle.ManagedDataAccess.Client;

namespace Wexflow.Core.Db.Oracle
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
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                using (var command = new OracleCommand("declare begin execute immediate 'CREATE TABLE " + tableName + tableStruct + "'; exception when others then if SQLCODE = -955 then null; else raise; end if; end;", conn))
                {
                    _ = command.ExecuteNonQuery();
                }
            }
        }
    }
}

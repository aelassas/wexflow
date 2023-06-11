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
            using OracleConnection conn = new(_connectionString);
            conn.Open();

            using OracleCommand command = new("declare begin execute immediate 'CREATE TABLE " + tableName + tableStruct + "'; exception when others then if SQLCODE = -955 then null; else raise; end if; end;", conn);
            _ = command.ExecuteNonQuery();
        }
    }
}

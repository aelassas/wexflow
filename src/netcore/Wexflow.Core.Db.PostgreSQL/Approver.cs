namespace Wexflow.Core.Db.PostgreSQL
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_Approved = "APPROVED";
        public const string ColumnName_ApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, "
                                                        + ColumnName_UserId + " INT, "
                                                        + ColumnName_RecordId + " INT, "
                                                        + ColumnName_Approved + " BOOLEAN, "
                                                        + ColumnName_ApprovedOn + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

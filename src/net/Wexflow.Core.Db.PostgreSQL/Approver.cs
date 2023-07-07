namespace Wexflow.Core.Db.PostgreSQL
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameApproved = "APPROVED";
        public const string ColumnNameApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnNameId + " SERIAL PRIMARY KEY, "
                                                        + ColumnNameUserId + " INT, "
                                                        + ColumnNameRecordId + " INT, "
                                                        + ColumnNameApproved + " BOOLEAN, "
                                                        + ColumnNameApprovedOn + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

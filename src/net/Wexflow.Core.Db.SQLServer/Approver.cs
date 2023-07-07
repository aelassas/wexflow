namespace Wexflow.Core.Db.SQLServer
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameApproved = "APPROVED";
        public const string ColumnNameApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnNameUserId + " INT, "
                                                        + ColumnNameRecordId + " INT, "
                                                        + ColumnNameApproved + " BIT, "
                                                        + ColumnNameApprovedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

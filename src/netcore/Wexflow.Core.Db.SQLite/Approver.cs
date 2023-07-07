namespace Wexflow.Core.Db.SQLite
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameApproved = "APPROVED";
        public const string ColumnNameApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnNameUserId + " INTEGER, "
                                                        + ColumnNameRecordId + " INTEGER, "
                                                        + ColumnNameApproved + " INTEGER, "
                                                        + ColumnNameApprovedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

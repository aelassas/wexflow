namespace Wexflow.Core.Db.SQLite
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_Approved = "APPROVED";
        public const string ColumnName_ApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnName_UserId + " INTEGER, "
                                                        + ColumnName_RecordId + " INTEGER, "
                                                        + ColumnName_Approved + " INTEGER, "
                                                        + ColumnName_ApprovedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

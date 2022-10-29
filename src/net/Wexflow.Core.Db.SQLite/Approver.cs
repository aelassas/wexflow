namespace Wexflow.Core.Db.SQLite
{
    public class Approver : Core.Db.Approver
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_UserId = "USER_ID";
        public static readonly string ColumnName_RecordId = "RECORD_ID";
        public static readonly string ColumnName_Approved = "APPROVED";
        public static readonly string ColumnName_ApprovedOn = "APPROVED_ON";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
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

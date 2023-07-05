namespace Wexflow.Core.Db.SQLServer
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_Approved = "APPROVED";
        public const string ColumnName_ApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnName_UserId + " INT, "
                                                        + ColumnName_RecordId + " INT, "
                                                        + ColumnName_Approved + " BIT, "
                                                        + ColumnName_ApprovedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

namespace Wexflow.Core.Db.MySQL
{
    public class Approver : Core.Db.Approver
    {
        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_UserId = "USER_ID";
        public static readonly string ColumnName_RecordId = "RECORD_ID";
        public static readonly string ColumnName_Approved = "APPROVED";
        public static readonly string ColumnName_ApprovedOn = "APPROVED_ON";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnName_UserId + " INT, "
                                                        + ColumnName_RecordId + " INT, "
                                                        + ColumnName_Approved + " BIT(1), "
                                                        + ColumnName_ApprovedOn + " TIMESTAMP NULL DEFAULT NULL, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

namespace Wexflow.Core.Db.MariaDB
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_UserId = "USER_ID";
        public const string ColumnName_RecordId = "RECORD_ID";
        public const string ColumnName_Approved = "APPROVED";
        public const string ColumnName_ApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, "
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

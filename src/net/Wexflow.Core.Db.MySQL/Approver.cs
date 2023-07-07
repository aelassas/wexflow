namespace Wexflow.Core.Db.MySQL
{
    public class Approver : Core.Db.Approver
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameUserId = "USER_ID";
        public const string ColumnNameRecordId = "RECORD_ID";
        public const string ColumnNameApproved = "APPROVED";
        public const string ColumnNameApprovedOn = "APPROVED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnNameUserId + " INT, "
                                                        + ColumnNameRecordId + " INT, "
                                                        + ColumnNameApproved + " BIT(1), "
                                                        + ColumnNameApprovedOn + " TIMESTAMP NULL DEFAULT NULL, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnNameId + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

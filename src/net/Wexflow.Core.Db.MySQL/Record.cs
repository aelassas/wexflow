namespace Wexflow.Core.Db.MySQL
{
    public class Record : Core.Db.Record
    {
        public const string ColumnNameId = "ID";
        public const string ColumnNameName = "NAME";
        public const string ColumnNameDescription = "DESCRIPTION";
        public const string ColumnNameApproved = "APPROVED";
        public const string ColumnNameStartDate = "START_DATE";
        public const string ColumnNameEndDate = "END_DATE";
        public const string ColumnNameComments = "COMMENTS";
        public const string ColumnNameManagerComments = "MANAGER_COMMENTS";
        public const string ColumnNameCreatedBy = "CREATED_BY";
        public const string ColumnNameCreatedOn = "CREATED_ON";
        public const string ColumnNameModifiedBy = "MODIFIED_BY";
        public const string ColumnNameModifiedOn = "MODIFIED_ON";
        public const string ColumnNameAssignedTo = "ASSIGNED_TO";
        public const string ColumnNameAssignedOn = "ASSIGNED_ON";

        public const string TableStruct = "(" + ColumnNameId + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnNameName + " VARCHAR(512), "
                                                        + ColumnNameDescription + " LONGTEXT, "
                                                        + ColumnNameApproved + " BIT, "
                                                        + ColumnNameStartDate + " TIMESTAMP NULL DEFAULT NULL, "
                                                        + ColumnNameEndDate + " TIMESTAMP NULL DEFAULT NULL, "
                                                        + ColumnNameComments + " LONGTEXT, "
                                                        + ColumnNameManagerComments + " LONGTEXT, "
                                                        + ColumnNameCreatedBy + " INT, "
                                                        + ColumnNameCreatedOn + " TIMESTAMP, "
                                                        + ColumnNameModifiedBy + " INT, "
                                                        + ColumnNameModifiedOn + " TIMESTAMP NULL DEFAULT NULL, "
                                                        + ColumnNameAssignedTo + " INT, "
                                                        + ColumnNameAssignedOn + " TIMESTAMP NULL DEFAULT NULL, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnNameId + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

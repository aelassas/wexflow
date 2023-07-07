namespace Wexflow.Core.Db.SQLServer
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

        public const string TableStruct = "(" + ColumnNameId + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnNameName + " VARCHAR(512), "
                                                        + ColumnNameDescription + " VARCHAR(MAX), "
                                                        + ColumnNameApproved + " BIT, "
                                                        + ColumnNameStartDate + " DATETIME, "
                                                        + ColumnNameEndDate + " DATETIME, "
                                                        + ColumnNameComments + " VARCHAR(MAX), "
                                                        + ColumnNameManagerComments + " VARCHAR(MAX), "
                                                        + ColumnNameCreatedBy + " INT, "
                                                        + ColumnNameCreatedOn + " DATETIME, "
                                                        + ColumnNameModifiedBy + " INT, "
                                                        + ColumnNameModifiedOn + " DATETIME, "
                                                        + ColumnNameAssignedTo + " INT, "
                                                        + ColumnNameAssignedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

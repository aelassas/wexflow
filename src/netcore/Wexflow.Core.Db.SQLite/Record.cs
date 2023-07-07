namespace Wexflow.Core.Db.SQLite
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

        public const string TableStruct = "(" + ColumnNameId + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnNameName + " TEXT, "
                                                        + ColumnNameDescription + " TEXT, "
                                                        + ColumnNameApproved + " INTEGER, "
                                                        + ColumnNameStartDate + " TEXT, "
                                                        + ColumnNameEndDate + " TEXT, "
                                                        + ColumnNameComments + " TEXT, "
                                                        + ColumnNameManagerComments + " TEXT, "
                                                        + ColumnNameCreatedBy + " INTEGER, "
                                                        + ColumnNameCreatedOn + " TEXT, "
                                                        + ColumnNameModifiedBy + " INTEGER, "
                                                        + ColumnNameModifiedOn + " TEXT, "
                                                        + ColumnNameAssignedTo + " INTEGER, "
                                                        + ColumnNameAssignedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

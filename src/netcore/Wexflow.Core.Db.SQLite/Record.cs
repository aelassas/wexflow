namespace Wexflow.Core.Db.SQLite
{
    public class Record : Core.Db.Record
    {
        public const string ColumnName_Id = "ID";
        public const string ColumnName_Name = "NAME";
        public const string ColumnName_Description = "DESCRIPTION";
        public const string ColumnName_Approved = "APPROVED";
        public const string ColumnName_StartDate = "START_DATE";
        public const string ColumnName_EndDate = "END_DATE";
        public const string ColumnName_Comments = "COMMENTS";
        public const string ColumnName_ManagerComments = "MANAGER_COMMENTS";
        public const string ColumnName_CreatedBy = "CREATED_BY";
        public const string ColumnName_CreatedOn = "CREATED_ON";
        public const string ColumnName_ModifiedBy = "MODIFIED_BY";
        public const string ColumnName_ModifiedOn = "MODIFIED_ON";
        public const string ColumnName_AssignedTo = "ASSIGNED_TO";
        public const string ColumnName_AssignedOn = "ASSIGNED_ON";

        public const string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + ColumnName_Name + " TEXT, "
                                                        + ColumnName_Description + " TEXT, "
                                                        + ColumnName_Approved + " INTEGER, "
                                                        + ColumnName_StartDate + " TEXT, "
                                                        + ColumnName_EndDate + " TEXT, "
                                                        + ColumnName_Comments + " TEXT, "
                                                        + ColumnName_ManagerComments + " TEXT, "
                                                        + ColumnName_CreatedBy + " INTEGER, "
                                                        + ColumnName_CreatedOn + " TEXT, "
                                                        + ColumnName_ModifiedBy + " INTEGER, "
                                                        + ColumnName_ModifiedOn + " TEXT, "
                                                        + ColumnName_AssignedTo + " INTEGER, "
                                                        + ColumnName_AssignedOn + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

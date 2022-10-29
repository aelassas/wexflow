namespace Wexflow.Core.Db.SQLite
{
    public class Record : Core.Db.Record
    {

        public static readonly string ColumnName_Id = "ID";
        public static readonly string ColumnName_Name = "NAME";
        public static readonly string ColumnName_Description = "DESCRIPTION";
        public static readonly string ColumnName_Approved = "APPROVED";
        public static readonly string ColumnName_StartDate = "START_DATE";
        public static readonly string ColumnName_EndDate = "END_DATE";
        public static readonly string ColumnName_Comments = "COMMENTS";
        public static readonly string ColumnName_ManagerComments = "MANAGER_COMMENTS";
        public static readonly string ColumnName_CreatedBy = "CREATED_BY";
        public static readonly string ColumnName_CreatedOn = "CREATED_ON";
        public static readonly string ColumnName_ModifiedBy = "MODIFIED_BY";
        public static readonly string ColumnName_ModifiedOn = "MODIFIED_ON";
        public static readonly string ColumnName_AssignedTo = "ASSIGNED_TO";
        public static readonly string ColumnName_AssignedOn = "ASSIGNED_ON";

        public static readonly string TableStruct = "(" + ColumnName_Id + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
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

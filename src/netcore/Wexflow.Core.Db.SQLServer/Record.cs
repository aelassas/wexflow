namespace Wexflow.Core.Db.SQLServer
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

        public const string TableStruct = "(" + ColumnName_Id + " INT IDENTITY(1,1) PRIMARY KEY, "
                                                        + ColumnName_Name + " VARCHAR(512), "
                                                        + ColumnName_Description + " VARCHAR(MAX), "
                                                        + ColumnName_Approved + " BIT, "
                                                        + ColumnName_StartDate + " DATETIME, "
                                                        + ColumnName_EndDate + " DATETIME, "
                                                        + ColumnName_Comments + " VARCHAR(MAX), "
                                                        + ColumnName_ManagerComments + " VARCHAR(MAX), "
                                                        + ColumnName_CreatedBy + " INT, "
                                                        + ColumnName_CreatedOn + " DATETIME, "
                                                        + ColumnName_ModifiedBy + " INT, "
                                                        + ColumnName_ModifiedOn + " DATETIME, "
                                                        + ColumnName_AssignedTo + " INT, "
                                                        + ColumnName_AssignedOn + " DATETIME)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

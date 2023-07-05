namespace Wexflow.Core.Db.PostgreSQL
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

        public const string TableStruct = "(" + ColumnName_Id + " SERIAL PRIMARY KEY, "
                                                        + ColumnName_Name + " VARCHAR, "
                                                        + ColumnName_Description + " VARCHAR, "
                                                        + ColumnName_Approved + " BOOLEAN, "
                                                        + ColumnName_StartDate + " TIMESTAMP, "
                                                        + ColumnName_EndDate + " TIMESTAMP, "
                                                        + ColumnName_Comments + " VARCHAR, "
                                                        + ColumnName_ManagerComments + " VARCHAR, "
                                                        + ColumnName_CreatedBy + " INT, "
                                                        + ColumnName_CreatedOn + " TIMESTAMP, "
                                                        + ColumnName_ModifiedBy + " INT, "
                                                        + ColumnName_ModifiedOn + " TIMESTAMP, "
                                                        + ColumnName_AssignedTo + " INT, "
                                                        + ColumnName_AssignedOn + " TIMESTAMP)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

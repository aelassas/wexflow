namespace Wexflow.Core.Db.MySQL
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

        public static readonly string TableStruct = "(" + ColumnName_Id + " INT NOT NULL AUTO_INCREMENT, "
                                                        + ColumnName_Name + " VARCHAR(512), "
                                                        + ColumnName_Description + " LONGTEXT, "
                                                        + ColumnName_Approved + " BIT, "
                                                        + ColumnName_StartDate + " TIMESTAMP NULL DEFAULT NULL, "
                                                        + ColumnName_EndDate + " TIMESTAMP NULL DEFAULT NULL, "
                                                        + ColumnName_Comments + " LONGTEXT, "
                                                        + ColumnName_ManagerComments + " LONGTEXT, "
                                                        + ColumnName_CreatedBy + " INT, "
                                                        + ColumnName_CreatedOn + " TIMESTAMP, "
                                                        + ColumnName_ModifiedBy + " INT, "
                                                        + ColumnName_ModifiedOn + " TIMESTAMP NULL DEFAULT NULL, "
                                                        + ColumnName_AssignedTo + " INT, "
                                                        + ColumnName_AssignedOn + " TIMESTAMP NULL DEFAULT NULL, CONSTRAINT " + DocumentName + "_pk PRIMARY KEY (" + ColumnName_Id + "))";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

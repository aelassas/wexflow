namespace Wexflow.Core.Db.SQLite
{
    public class Record : Core.Db.Record
    {
        public const string COLUMN_NAME_ID = "ID";
        public const string COLUMN_NAME_NAME = "NAME";
        public const string COLUMN_NAME_DESCRIPTION = "DESCRIPTION";
        public const string COLUMN_NAME_APPROVED = "APPROVED";
        public const string COLUMN_NAME_START_DATE = "START_DATE";
        public const string COLUMN_NAME_END_DATE = "END_DATE";
        public const string COLUMN_NAME_COMMENTS = "COMMENTS";
        public const string COLUMN_NAME_MANAGER_COMMENTS = "MANAGER_COMMENTS";
        public const string COLUMN_NAME_CREATED_BY = "CREATED_BY";
        public const string COLUMN_NAME_CREATED_ON = "CREATED_ON";
        public const string COLUMN_NAME_MODIFIED_BY = "MODIFIED_BY";
        public const string COLUMN_NAME_MODIFIED_ON = "MODIFIED_ON";
        public const string COLUMN_NAME_ASSIGNED_TO = "ASSIGNED_TO";
        public const string COLUMN_NAME_ASSIGNED_ON = "ASSIGNED_ON";

        public const string TABLE_STRUCT = "(" + COLUMN_NAME_ID + " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, "
                                                        + COLUMN_NAME_NAME + " TEXT, "
                                                        + COLUMN_NAME_DESCRIPTION + " TEXT, "
                                                        + COLUMN_NAME_APPROVED + " INTEGER, "
                                                        + COLUMN_NAME_START_DATE + " TEXT, "
                                                        + COLUMN_NAME_END_DATE + " TEXT, "
                                                        + COLUMN_NAME_COMMENTS + " TEXT, "
                                                        + COLUMN_NAME_MANAGER_COMMENTS + " TEXT, "
                                                        + COLUMN_NAME_CREATED_BY + " INTEGER, "
                                                        + COLUMN_NAME_CREATED_ON + " TEXT, "
                                                        + COLUMN_NAME_MODIFIED_BY + " INTEGER, "
                                                        + COLUMN_NAME_MODIFIED_ON + " TEXT, "
                                                        + COLUMN_NAME_ASSIGNED_TO + " INTEGER, "
                                                        + COLUMN_NAME_ASSIGNED_ON + " TEXT)";

        public long Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}

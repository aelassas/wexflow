using System;

namespace Wexflow.Core.Db
{
    public enum UserProfile
    {
        SuperAdministrator,
        Administrator,
        Restricted
    }

    public class User
    {
        public const string DOCUMENT_NAME = "users";

        public string Username { get; set; }
        public string Password { get; set; }
        public UserProfile UserProfile { get; set; }
        public string Email { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}

using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    public enum UserProfile
    {
        SuperAdministrator,
        Administrator,
        Restricted,
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public UserProfile UserProfile { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        //public double CreatedOn { get; set; }
        public string CreatedOn { get; set; }
        [DataMember]
        //public double ModifiedOn { get; set; }
        public string ModifiedOn { get; set; }
    }
}

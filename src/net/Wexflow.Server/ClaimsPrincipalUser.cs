using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Wexflow.Server
{
    public class ClaimsPrincipalUser : ClaimsPrincipal
    {
        public ClaimsPrincipal Principal { get; }

        // Required by Nancy
        public string UserName => Principal.Identity?.Name;

        // Required by Nancy
        public new IEnumerable<string> Claims => Principal.Claims.Select(c => c.Type + ":" + c.Value);

        // Override Identity so Nancy sees it
        public override IIdentity Identity => Principal.Identity;

        public ClaimsPrincipalUser(ClaimsPrincipal principal) : base(principal)
        {
            Principal = principal;
        }
    }

}

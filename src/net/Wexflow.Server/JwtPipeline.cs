using Nancy;
using System;
using System.Runtime.Remoting.Contexts;

namespace Wexflow.Server
{
    public static class JwtPipeline
    {
        public static Func<NancyContext, Response> Before => ctx =>
        {
            var path = ctx.Request.Path.TrimEnd('/');
            var root = $"/{WexflowService.ROOT.Trim('/')}";

            if (string.Equals(path, $"{root}/hello", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, $"{root}/login", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, $"{root}/logout", StringComparison.OrdinalIgnoreCase)

                )
            {
                return null; // Skip JWT auth
            }


            var authHeader = ctx.Request.Headers.Authorization;
            if (string.IsNullOrEmpty(authHeader) && ctx.Request.Cookies.TryGetValue("wf-auth", out var cookieToken))
            {
                authHeader = $"Bearer {cookieToken}";
            }

            // Run authentication middleware
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var principal = JwtHelper.ValidateToken(token);

                if (principal != null)
                {
                    ctx.CurrentUser = new ClaimsPrincipalUser(principal);
                    return null; // Success
                }
            }

            return HttpStatusCode.Unauthorized;
        };
    }
}

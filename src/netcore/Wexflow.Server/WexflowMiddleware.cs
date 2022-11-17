using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Wexflow.Server
{
    public class WexflowMiddleware
    {
        private readonly RequestDelegate _next;

        public WexflowMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            await _next.Invoke(context);
        }
    }
}

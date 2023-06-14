using Owin;

namespace Wexflow.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            // Configure diagnostics
            app.UseErrorPage();
#endif
            _ = app.UseNancy();
        }
    }
}

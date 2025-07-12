using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Wexflow.Server
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var https = bool.TryParse(_config["HTTPS"], out var res) && res;
            if (https)
            {
                app.UseHttpsRedirection();
            }

            var path = Path.Combine(env.ContentRootPath, "swagger-ui");

            var extensionProvider = new FileExtensionContentTypeProvider();
            extensionProvider.Mappings.Add(".yaml", "application/x-yaml");
            extensionProvider.Mappings.Add(".yml", "application/x-yaml");

            var fileProvider = new PhysicalFileProvider(path);

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = ""
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = extensionProvider,
                RequestPath = ""
            });

            app.UseMiddleware<WexflowMiddleware>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors(policyBuilder => policyBuilder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials()
            );

            app.UseEndpoints(endpoints =>
            {
                var wexflowService = new WexflowService(endpoints);
                wexflowService.Map();
            });
        }


        public void ConfigureServices(IServiceCollection services)
        {
            var port = _config.GetValue<int>("WexflowServicePort");

            _ = services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = port;
            });
            _ = services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            _ = services.AddCors();
            _ = services.AddControllers();
        }
    }
}

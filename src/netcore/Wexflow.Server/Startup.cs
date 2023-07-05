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
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .SetBasePath(env.ContentRootPath);

            var config = builder.Build();

            AppConfiguration appConfig = new();
            config.Bind(appConfig);

            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }

            //
            // Swagger UI
            //
            var webBuilder = WebApplication.CreateBuilder();
            var path = Path.Combine(webBuilder.Environment.ContentRootPath, "swagger-ui");

            FileExtensionContentTypeProvider extensionProvider = new();
            extensionProvider.Mappings.Add(".yaml", "application/x-yaml");
            extensionProvider.Mappings.Add(".yml", "application/x-yaml");

            PhysicalFileProvider fileProvider = new(path);
            _ = app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = new PathString("")
            });
            _ = app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = extensionProvider,
                RequestPath = new PathString("")
            });

            //
            // Wexflow Service
            //
            _ = app.UseMiddleware<WexflowMiddleware>();
            _ = app.UseRouting();
            _ = app.UseAuthorization();
            _ = app.UseCors(policyBuilder => policyBuilder
                                                        .AllowAnyHeader()
                                                        .AllowAnyMethod()
                                                        .SetIsOriginAllowed(_ => true)
                                                        .AllowCredentials()
            );
            _ = app.UseEndpoints(endpoints =>
            {
                WexflowService wexflowService = new(endpoints);
                wexflowService.Map();
            });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            _ = services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            _ = services.AddCors();
            _ = services.AddControllers();
        }
    }
}

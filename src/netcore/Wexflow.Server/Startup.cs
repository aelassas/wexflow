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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .SetBasePath(env.ContentRootPath);

            var config = builder.Build();

            var appConfig = new AppConfiguration();
            ConfigurationBinder.Bind(config, appConfig);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //
            // swagger-ui
            //
            var webBuilder = WebApplication.CreateBuilder();
            var path = Path.Combine(webBuilder.Environment.ContentRootPath, "swagger-ui");

            var extensionProvider = new FileExtensionContentTypeProvider();
            extensionProvider.Mappings.Add(".yaml", "application/x-yaml");
            extensionProvider.Mappings.Add(".yml", "application/x-yaml");

            var fileProvider = new PhysicalFileProvider(path);
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = new PathString("")
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = extensionProvider,
                RequestPath = new PathString("")
            });

            //
            // Wexflow Service
            //
            app.UseMiddleware<WexflowMiddleware>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors(builder => builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed((host) => true)
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
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddCors();
            services.AddControllers();

        }
    }
}

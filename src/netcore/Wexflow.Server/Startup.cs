using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Linq;
using System.Text;

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

            // https
            var https = bool.TryParse(_config["HTTPS"], out var res) && res;
            if (https)
            {
                app.UseHttpsRedirection();
            }

            // redirects
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value;

                if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("/admin");
                    return;
                }

                if (string.Equals(path, "/swagger", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("/swagger-ui");
                    return;
                }

                await next();
            });

            // admin
            var adminFolder = Path.GetFullPath(_config["AdminFolder"]);

            if (!Directory.Exists(adminFolder))
            {
                throw new DirectoryNotFoundException($"Admin folder not found: {adminFolder}");
            }

            var extensionProvider = new FileExtensionContentTypeProvider();
            var fileProvider = new PhysicalFileProvider(adminFolder);

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = "/admin"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = extensionProvider,
                RequestPath = "/admin"
            });

            // swagger
            var path = Path.Combine(env.ContentRootPath, "swagger-ui");

            extensionProvider = new FileExtensionContentTypeProvider();
            extensionProvider.Mappings.Add(".yaml", "application/x-yaml");
            extensionProvider.Mappings.Add(".yml", "application/x-yaml");

            fileProvider = new PhysicalFileProvider(path);

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider,
                RequestPath = "/swagger-ui"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = extensionProvider,
                RequestPath = "/swagger-ui"
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

            // jwt middleware
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.TrimEnd('/');
                var root = $"/{WexflowService.ROOT.Trim('/')}";

                if (string.Equals(path, $"{root}/hello", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(path, $"{root}/login", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(path, $"{root}/logout", StringComparison.OrdinalIgnoreCase)
                    )
                {
                    // Skip JWT validation
                    await next();
                    return;
                }

                // Run authentication middleware
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) && context.Request.Cookies.TryGetValue("wf-auth", out var cookieToken))
                {
                    authHeader = $"Bearer {cookieToken}";
                }

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();

                    var principal = JwtHelper.ValidateToken(token);
                    if (principal != null)
                    {
                        context.User = principal;
                        await next();
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");

            });

            // map endpoints
            app.UseEndpoints(endpoints =>
            {
                var wexflowService = new WexflowService(endpoints, _config);
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

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace Wexflow.Server
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static NameValueCollection Config = ConfigurationManager.AppSettings;

        private readonly IAppConfiguration _appConfig;

        public Bootstrapper()
        {
        }

        public Bootstrapper(IAppConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            _ = container.Register(_appConfig);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // Register the global JWT pipeline
            pipelines.BeforeRequest += JwtPipeline.Before;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.AfterRequest += ctx =>
            {
                _ = ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "POST,GET,PUT,DELETE,OPTIONS,HEAD,PATCH")
                    .WithHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
            };
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Config["AdminFolder"]; // e.g., ".\\Admin" or "C:\\Some\\Folder"
            string adminFolder;

            if (Path.IsPathRooted(configPath))
            {
                adminFolder = configPath;
            }
            else
            {
                adminFolder = Path.Combine(baseDir, configPath);
            }

            if (!Directory.Exists(adminFolder))
            {
                throw new DirectoryNotFoundException($"Admin folder not found: {adminFolder}");
            }

            conventions.StaticContentsConventions.Clear();
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/swagger-ui", "swagger-ui"));

            conventions.StaticContentsConventions.Add((ctx, rootPath) =>
            {
                const string mountPath = "/admin";
                var requestPath = ctx.Request.Path;

                if (!requestPath.StartsWith(mountPath, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Map to your external admin folder
                var relativePath = requestPath.Substring(mountPath.Length).TrimStart('/');
                if (string.IsNullOrEmpty(relativePath))
                {
                    relativePath = "index.html";
                }

                var filePath = Path.Combine(adminFolder, relativePath);

                // Prevent directory traversal
                var normalizedAdminFolder = Path
                .GetFullPath(adminFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

                var fullPath = Path.GetFullPath(filePath);

                if (!fullPath.StartsWith(normalizedAdminFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return HttpStatusCode.Forbidden;
                }

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                var ext = Path.GetExtension(fullPath).ToLowerInvariant();
                var contentType = "application/octet-stream";

                switch (ext)
                {
                    case ".html": contentType = "text/html"; break;
                    case ".js": contentType = "application/javascript"; break;
                    case ".css": contentType = "text/css"; break;
                    case ".json": contentType = "application/json"; break;
                    case ".png": contentType = "image/png"; break;
                    case ".jpg":
                    case ".jpeg": contentType = "image/jpeg"; break;
                    case ".svg": contentType = "image/svg+xml"; break;
                }

                var bytes = File.ReadAllBytes(fullPath);

                return new Response
                {
                    StatusCode = HttpStatusCode.OK,
                    ContentType = contentType,
                    Contents = stream => stream.Write(bytes, 0, bytes.Length)
                };
            });

        }
    }
}

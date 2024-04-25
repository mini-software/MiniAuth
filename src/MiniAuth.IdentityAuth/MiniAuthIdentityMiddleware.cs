using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniAuth.Identity;
using System.Diagnostics;
using System.Reflection;

namespace MiniAuth
{
    public partial class MiniAuthIdentityMiddleware
    {
        private const string EmbeddedFileNamespace = "MiniAuth.IdentityAuth.wwwroot";
        private readonly RequestDelegate _next;
        private readonly ILogger<MiniAuthIdentityMiddleware> _logger;
        private readonly MiniAuthOptions _options;
        private readonly StaticFileMiddleware _staticFileMiddleware;
        public MiniAuthIdentityMiddleware(RequestDelegate next,
            ILogger<MiniAuthIdentityMiddleware> logger,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment hostingEnv,
            MiniAuthOptions options = null
        )
        {
            this._logger = logger;
            this._next = next;
            if (options == null)
                _options = new MiniAuthOptions();
            else
                _options = options;
            this._staticFileMiddleware = CreateStaticFileMiddleware(next, loggerFactory, hostingEnv); ;
        }
        public async Task Invoke(HttpContext context)
        {
#if DEBUG
            Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}, Path: {context.Request.Path}");
#endif
            await _next(context);
        }
        private StaticFileMiddleware CreateStaticFileMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnv)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(_options.RoutePrefix) ? string.Empty : $"/{_options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(MiniAuthIdentityMiddleware).GetTypeInfo().Assembly, "MiniAuth.Identity.wwwroot"),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }
    }
}

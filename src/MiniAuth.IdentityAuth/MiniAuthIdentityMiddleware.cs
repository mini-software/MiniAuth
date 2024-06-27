using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniAuth;
using System.Diagnostics;
using System.Reflection;


public partial class MiniAuthIdentityMiddleware
{
    private const string EmbeddedFileNamespace = "MiniAuth.IdentityAuth.wwwroot";
    private readonly RequestDelegate _next;
    private readonly ILogger<MiniAuthIdentityMiddleware> _logger;
    private readonly StaticFileMiddleware _staticFileMiddleware;
    private static bool FirstRun = true;
    public MiniAuthIdentityMiddleware(RequestDelegate next,
        ILogger<MiniAuthIdentityMiddleware> logger,
        ILoggerFactory loggerFactory,
        IWebHostEnvironment hostingEnv
    )
    {
        this._logger = logger;
        this._next = next;
        this._staticFileMiddleware = CreateStaticFileMiddleware(next, loggerFactory, hostingEnv); ;
    }
    public async Task Invoke(HttpContext context)
    {
        if (FirstRun)
        {
            FirstRun = false;
            var server = context.RequestServices.GetService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            foreach (var address in addressFeature.Addresses)
            {
                _logger.LogInformation($"MiniAuth management is listening on address: {address}/miniauth/index.html");
            }
        }
#if DEBUG
        Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}, Path: {context.Request.Path}");
#endif
        await _next(context);
    }
    private StaticFileMiddleware CreateStaticFileMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnv)
    {
        var staticFileOptions = new StaticFileOptions
        {
            RequestPath = string.IsNullOrEmpty(MiniAuthOption.RoutePrefix) ? string.Empty : $"/{MiniAuthOption.RoutePrefix}",
            FileProvider = new EmbeddedFileProvider(typeof(MiniAuthIdentityMiddleware).GetTypeInfo().Assembly, "MiniAuth.Identity.wwwroot"),
        };

        return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
    }
}


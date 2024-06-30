using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


public partial class MiniAuthIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiniAuthIdentityMiddleware> _logger;
    private static bool FirstRun = true;
    public MiniAuthIdentityMiddleware(RequestDelegate next,
        ILogger<MiniAuthIdentityMiddleware> logger
    )
    {
        this._logger = logger;
        this._next = next;
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
        Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}, Path: {context.Request.Path}");
        await _next(context);
    }
}



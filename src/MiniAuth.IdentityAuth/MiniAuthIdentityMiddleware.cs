using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

#if DEBUG
public partial class MiniAuthIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiniAuthIdentityMiddleware> _logger;
    public MiniAuthIdentityMiddleware(RequestDelegate next,
        ILogger<MiniAuthIdentityMiddleware> logger
    )
    {
        this._logger = logger;
        this._next = next;
    }
    public async Task Invoke(HttpContext context)
    {
        Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}, Path: {context.Request.Path}");

        await _next(context);
    }
}
#endif


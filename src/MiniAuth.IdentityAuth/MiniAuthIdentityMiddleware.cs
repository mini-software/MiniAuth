using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MiniAuth.Identity
{
    public partial class MiniAuthIdentityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiniAuthIdentityMiddleware> _logger;
        public MiniAuthIdentityMiddleware(RequestDelegate next,
            ILogger<MiniAuthIdentityMiddleware> logger,
            IWebHostEnvironment hostingEnv
        )
        {
            this._logger = logger;
            this._next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            await _next(context);
        }
    }
}

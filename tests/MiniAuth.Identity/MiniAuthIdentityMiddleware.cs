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
            _logger.LogInformation("MiniAuthIdentityMiddleware executing..");
            await _next(context);
        }
    }

    public static class MiniAuthIdentityBuilderExtensions
    {
        public static IApplicationBuilder UseMiniIdentityAuth(this IApplicationBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            return builder.UseMiddleware<MiniAuthIdentityMiddleware>();
        }
    }
}

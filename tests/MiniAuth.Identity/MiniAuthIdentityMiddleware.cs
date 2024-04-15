using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

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

    public static class MiniAuthIdentityServiceExtensions
    {
        public static IServiceCollection AddMiniIdentityAuth(this IServiceCollection services, Action<IdentityOptions> configureOptions)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            var connectionString = "Data Source=miniauth_identity.db";
            services.AddDbContext<MiniAuthIdentityDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddIdentityCookies(o => { });

            services.AddIdentityCore<MiniAuthIdentityUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                configureOptions?.Invoke(o);
                o.SignIn.RequireConfirmedAccount = false;
            }).AddDefaultTokenProviders().AddEntityFrameworkStores<MiniAuthIdentityDbContext>();

            return services;
        }
    }

    [Table("AspNetUsers")]
    public class MiniAuthIdentityUser : IdentityUser
    {
    }

    public class MiniAuthIdentityDbContext : IdentityDbContext<MiniAuthIdentityUser>
    {
        public MiniAuthIdentityDbContext(DbContextOptions<MiniAuthIdentityDbContext> options) : base(options)
        {
        }
    }
}

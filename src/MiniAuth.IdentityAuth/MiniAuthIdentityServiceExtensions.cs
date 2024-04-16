using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniAuth.IdentityAuth.Models;

namespace MiniAuth.Identity
{
    public static class MiniAuthIdentityServiceExtensions
    {
        public static IServiceCollection AddMiniIdentityAuth(this IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            var connectionString = "Data Source=miniauth_identity.db";
            services.AddDbContext<MiniAuthIdentityDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            // if services AddAuthentication not already added then call AddAuthentication
            if (services.All(o => o.ServiceType != typeof(IAuthenticationService)))
            {
                services.AddAuthorization();
                services.AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityConstants.ApplicationScheme;
                    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                }).AddIdentityCookies(o => { });

                services.AddIdentityCore<MiniAuthIdentityUser>(o =>
                {
                    o.SignIn.RequireConfirmedAccount = false;
                }).AddDefaultTokenProviders()
                  .AddEntityFrameworkStores<MiniAuthIdentityDbContext>();
            }
            return services;
        }
    }
}

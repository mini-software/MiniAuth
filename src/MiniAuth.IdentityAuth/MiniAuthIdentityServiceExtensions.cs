using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniAuth.IdentityAuth.Models;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

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
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("miniauth_admin", policy =>
                    {
                        policy.RequireClaim("role", "miniauth_admin");
                    });
                });
                services.AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityConstants.ApplicationScheme;
                    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                }).AddIdentityCookies(o => { 
                    o.ApplicationCookie.Configure(o => o.LoginPath = "/miniauth/login.html");
                });

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

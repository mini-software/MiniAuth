using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniAuth.IdentityAuth.Models;
using System.Diagnostics.CodeAnalysis;
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
                services
                    .AddMiniAuthIdentity<MiniAuthIdentityUser, IdentityRole>()
                    .AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<MiniAuthIdentityDbContext>();
            }
            return services;
        }
        public static IdentityBuilder AddMiniAuthIdentity<TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRole>(
            this IServiceCollection services)
            where TUser : class
            where TRole : class
            => services.AddMiniAuthIdentity<TUser, TRole>(setupAction: null!);
        public static IdentityBuilder AddMiniAuthIdentity<TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRole>(
             this IServiceCollection services,
             Action<IdentityOptions> setupAction)
             where TUser : class
             where TRole : class
        {
            // Services used by identity
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies(o => {
                o.ApplicationCookie.Configure(o => o.LoginPath = "/miniauth/login.html");
            });
            //.AddCookie(IdentityConstants.ApplicationScheme, o =>
            //{
            //    o.LoginPath = new PathString("/miniauth/login.html");
            //    o.Events = new CookieAuthenticationEvents
            //    {
            //        OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
            //    };
            //})
            //.AddCookie(IdentityConstants.ExternalScheme, o =>
            //{
            //    o.Cookie.Name = IdentityConstants.ExternalScheme;
            //    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //})
            //.AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
            //{
            //    o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
            //    o.Events = new CookieAuthenticationEvents
            //    {
            //        OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
            //    };
            //})
            //.AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
            //{
            //    o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
            //    o.Events = new CookieAuthenticationEvents
            //    {
            //        OnRedirectToReturnUrl = _ => Task.CompletedTask
            //    };
            //    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //});

            // Hosting doesn't add IHttpContextAccessor by default
            services.AddHttpContextAccessor();
            // Identity services
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SecurityStampValidatorOptions>, PostConfigureSecurityStampValidatorOptions>());
            services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
            services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
            services.TryAddScoped<UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }

        private sealed class PostConfigureSecurityStampValidatorOptions : IPostConfigureOptions<SecurityStampValidatorOptions>
        {
            public PostConfigureSecurityStampValidatorOptions(TimeProvider timeProvider)
            {
                TimeProvider = timeProvider;
            }

            private TimeProvider TimeProvider { get; }

            public void PostConfigure(string? name, SecurityStampValidatorOptions options)
            {
                options.TimeProvider ??= TimeProvider;
            }
        }
    }
}

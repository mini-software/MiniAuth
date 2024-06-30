using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniAuth;
using MiniAuth.Identity;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


public static class MiniAuthIdentityServiceExtensions
{
    private static IServiceCollection AddMiniIdentityAuth(this IServiceCollection services, Action<MiniAuthOptions> options = null, bool autoUse = true)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        var connectionString = MiniAuthOption.SqliteConnectionString;
        services.AddDbContext<MiniAuthIdentityDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });
        services.AddMiniAuth<MiniAuthIdentityDbContext, IdentityUser, IdentityRole>(options: options, autoUse: autoUse);
        return services;
    }
    public static IServiceCollection AddMiniAuth(this IServiceCollection services, Action<MiniAuthOptions> options = null, bool autoUse = true)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        services.AddMiniIdentityAuth(options: options, autoUse: autoUse);  //TODO: auto use issue : https://github.com/mini-software/MiniAuth/issues/151         
        return services;
    }
    public static IServiceCollection AddMiniAuth<TDbContext, TIdentityUser, TIdentityRole>(this IServiceCollection services, Action<MiniAuthOptions> options = null, bool autoUse = true)
        where TDbContext : IdentityDbContext
        where TIdentityUser : IdentityUser
        where TIdentityRole : IdentityRole
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        if (options != null)
        {
            options(new MiniAuthOptions());
        }

        // if not exist AddAuthorization then add default policy
        var existAuthorization = services.Any(o => o.ServiceType == typeof(IAuthorizationService));
        if (!existAuthorization)
        {
            Debug.WriteLine("* Use MiniAuth default AddAuthorization");
            services.AddAuthorization();
        }
        else
        {
            Debug.WriteLine("* Use exist Authorization");
        }

        // service add MiniAuthOptions 
        services.TryAddSingleton<MiniAuthOptions>();


        if (services.All(o => o.ServiceType != typeof(IAuthenticationService)))
        {
            Debug.WriteLine("* Use MiniAuth default AddAuthentication");
            if (MiniAuthOption.AuthenticationType == AuthType.Cookie)
            {
                services
                    .AddMiniAuth<TIdentityUser, TIdentityRole>()
                    .AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<TDbContext>();
                //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                //    .AddCookie(options =>
                //    {
                //        options.LoginPath = $"/{MiniAuthOption.RoutePrefix}/login.html"; 
                //        options.LogoutPath = $"/{MiniAuthOption.RoutePrefix}/logout"; 
                //        options.AccessDeniedPath = $"/{MiniAuthOption.RoutePrefix}/AccessDenied"; 
                //    });
                //// add identity
                //services.AddIdentity<TIdentityUser, TIdentityRole>()
                //    .AddEntityFrameworkStores<TDbContext>()
                //    .AddDefaultTokenProviders();
            }
            if (MiniAuthOption.AuthenticationType == AuthType.BearerJwt)
            {

                services.AddIdentity<TIdentityUser, TIdentityRole>()
                    .AddEntityFrameworkStores<TDbContext>()
                    .AddDefaultTokenProviders();

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                    //.AddJwtBearer()
                    .AddJwtBearer(options =>
                    {
                        options.IncludeErrorDetails = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = MiniAuthOption.Issuer,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = MiniAuth.MiniAuthOption.JWTKey
                        };
                    })
                    ;
            }
        }
        else
        {
            Debug.WriteLine("* Use exist Authentication");
        }

        if (autoUse)
            services.AddTransient<IStartupFilter, MiniAuthStartupFilter>();
        else
            services.AddTransient<IStartupFilter, EmptyStartupFilter>();

        return services;
    }

    public static IdentityBuilder AddMiniAuth<TUser,
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    TRole>(
        this IServiceCollection services)
        where TUser : class
        where TRole : class
        => services.AddMiniAuth<TUser, TRole>(setupAction: null!);
    public static IdentityBuilder AddMiniAuth<TUser,
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    TRole>(
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
        .AddMiniAuthIdentityCookies(o =>
        {
            o.ApplicationCookie.Configure(o =>
            {
                o.LoginPath = $"/{MiniAuthOption.RoutePrefix}/login.html";
                o.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        var routeEndpoint = ctx.HttpContext.GetEndpoint();
                        Debug.WriteLine($"* CookieAuthenticationEvents : {routeEndpoint.ToString()}");

                        var isJsonApi = ctx.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                            routeEndpoint.Metadata?.GetMetadata<Microsoft.AspNetCore.Mvc.ApiControllerAttribute>() != null;
                        if (isJsonApi)
                        {
                            Debug.WriteLine($"IsXMLHttpRequest Path: {ctx.Request.Path}");
                            if (ctx.Request.Path.StartsWithSegments($"/{MiniAuthOption.RoutePrefix}/api"))
                            {
                                ctx.Response.Headers.Append("ReturnUrl", $"/{MiniAuthOption.RoutePrefix}/index.html");
                                ctx.RedirectUri = $"/{MiniAuthOption.RoutePrefix}/index.html";
                                ctx.Response.StatusCode = 401;
                            }
                            else
                            {
                                ctx.Response.Headers.Append("ReturnUrl", ctx.RedirectUri);
                                ctx.RedirectUri = "";
                                ctx.Response.StatusCode = 401;
                            }
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }

                        return Task.CompletedTask;
                    }
                };
            });
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
#if NET8_0_OR_GREATER
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SecurityStampValidatorOptions>, PostConfigureSecurityStampValidatorOptions>());
#endif
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
    private static IdentityCookiesBuilder AddMiniAuthIdentityCookies(this AuthenticationBuilder builder, Action<IdentityCookiesBuilder> configureCookies)
    {
        var cookieBuilder = new IdentityCookiesBuilder();
        cookieBuilder.ApplicationCookie = builder.AddMiniAuthApplicationCookie();
        cookieBuilder.ExternalCookie = builder.AddExternalCookie();
        cookieBuilder.TwoFactorRememberMeCookie = builder.AddTwoFactorRememberMeCookie();
        cookieBuilder.TwoFactorUserIdCookie = builder.AddTwoFactorUserIdCookie();
        configureCookies?.Invoke(cookieBuilder);
        return cookieBuilder;
    }
    private static OptionsBuilder<CookieAuthenticationOptions> AddMiniAuthApplicationCookie(this AuthenticationBuilder builder)
    {
        builder.AddCookie(IdentityConstants.ApplicationScheme, o =>
        {
            o.LoginPath = new PathString("/Account/Login");
            o.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
            };
        });
        return new OptionsBuilder<CookieAuthenticationOptions>(builder.Services, IdentityConstants.ApplicationScheme);
    }
#if NET8_0_OR_GREATER
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
#endif
}


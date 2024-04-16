using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
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
    public static class MiniAuthIdentityBuilderExtensions
    {
        public static IApplicationBuilder UseMiniIdentityAuth(this IApplicationBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.UseMiddleware<MiniAuthIdentityMiddleware>();

            if (!builder.Properties.TryGetValue("__UseRouting",out var _))
                builder.UseRouting();
            if (!builder.Properties.TryGetValue("__UseAuthorization", out var _)) 
                builder.UseAuthorization();
           

            // builder.UseEndpoints by class MiniAuthIdentityEndpoints
            var miniauthEndpoints = new MiniAuthIdentityEndpoints();
            miniauthEndpoints.MapEndpoints(builder);

            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<MiniAuthIdentityDbContext>();
                if (ctx.Database.EnsureCreated())
                {
                    var user = new MiniAuthIdentityUser
                    {
                        UserName = "miniauth",
                        Email = "",
                        EmailConfirmed = true,
                        PhoneNumber = "",
                        PhoneNumberConfirmed = true,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                    };
                    // Add default password
                    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<MiniAuthIdentityUser>>();
                    user.PasswordHash = hasher.HashPassword(user, "miniauth");

                    // add IdentityUserRole and IdentityRole
                    ctx.Add(new IdentityRole { Id = "miniauth_admin", Name = "miniauth_admin", NormalizedName = "miniauth_admin" });
                    ctx.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = "miniauth_admin" });

                    ctx.Add(user);
                    ctx.SaveChanges();
                }

            }

            var option = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty("MiniAuth") ? string.Empty : $"/MiniAuth",
                FileProvider = new EmbeddedFileProvider(typeof(MiniAuthIdentityServiceExtensions).GetTypeInfo().Assembly, "MiniAuth.IdentityAuth.wwwroot"),
            };
            builder.UseStaticFiles(option);

            return builder;
        }


    }
}

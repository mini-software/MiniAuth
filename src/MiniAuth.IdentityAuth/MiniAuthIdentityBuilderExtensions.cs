using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MiniAuth.IdentityAuth.Models;
using System.Reflection;

namespace MiniAuth.Identity
{
    public static class MiniAuthIdentityBuilderExtensions
    {
        public static IApplicationBuilder UseMiniIdentityAuth(this IApplicationBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            builder.UseMiniIdentityAuth<MiniAuthIdentityDbContext>();
            return builder;
        }
        public static IApplicationBuilder UseMiniIdentityAuth<TDbContext> 
            (this IApplicationBuilder builder) 
            where TDbContext : IdentityDbContext
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.UseMiddleware<MiniAuthIdentityMiddleware>();

            if (!builder.Properties.TryGetValue("__UseRouting", out var _))
                builder.UseRouting();
            if (!builder.Properties.TryGetValue("__UseAuthorization", out var _))
                builder.UseAuthorization();

            var miniauthEndpoints = new MiniAuthIdentityEndpoints<TDbContext>();
            miniauthEndpoints.MapEndpoints(builder);

            Task.Run(async () =>
            {
                using (var scope = builder.ApplicationServices.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<MiniAuthIdentityDbContext>();
                    if (ctx.Database.EnsureCreated())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                        
                        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<IdentityUser>>();
                        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                        {
                            var user = Activator.CreateInstance<IdentityUser>();
                            await userStore.SetUserNameAsync(user, "miniauth", CancellationToken.None);
                            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
                            await roleManager.CreateAsync(new IdentityRole("miniauth-admin") );
                            await userManager.AddToRoleAsync(user, "miniauth-admin");
                        }
#if DEBUG
                        foreach (var item in new[] { "HR","IT","RD"})
                        {
                            var user = Activator.CreateInstance<IdentityUser>();
                            await userStore.SetUserNameAsync(user, $"miniauth-{item.ToLower()}", CancellationToken.None);
                            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
                            await roleManager.CreateAsync(new IdentityRole(item) );
                            await userManager.AddToRoleAsync(user, item);
                        }   
#endif
                    }
                }
            }).GetAwaiter().GetResult();


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

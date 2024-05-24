using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Reflection;

namespace MiniAuth.Identity
{
    public static class MiniAuthIdentityBuilderExtensions
    {
        public static IApplicationBuilder UseMiniAuth(this IApplicationBuilder builder)
        {
            Debug.WriteLine("* start UseMiniIdentityAuth");
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            builder.UseMiniAuth<MiniAuthIdentityDbContext, IdentityUser, IdentityRole>();
            return builder;
        }
        public static IApplicationBuilder UseMiniAuth<TDbContext, TIdentityUser, TIdentityRole>
            (this IApplicationBuilder builder)
            where TDbContext : IdentityDbContext
            where TIdentityUser : IdentityUser,new()
            where TIdentityRole : IdentityRole, new()
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.UseMiddleware<MiniAuthIdentityMiddleware>();

            if (!builder.Properties.TryGetValue("__UseRouting", out var _))
                builder.UseRouting();
            if (!builder.Properties.TryGetValue("__UseAuthorization", out var _))
                builder.UseAuthorization();

            var miniauthEndpoints = new MiniAuthIdentityEndpoints<TDbContext, TIdentityUser, TIdentityRole>();
            miniauthEndpoints.MapEndpoints(builder);


            //TODO: only for testing to create default user and roles
            Task.Run(async () =>
            {
                using (var scope = builder.ApplicationServices.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<TDbContext>();
                    if (ctx.Database.EnsureCreated())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TIdentityUser>>();

                        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<TIdentityUser>>();
                        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<TIdentityRole>>();
                        {
                            var user = Activator.CreateInstance<TIdentityUser>();
                            await userStore.SetUserNameAsync(user, "miniauth", CancellationToken.None);
                            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
                            await roleManager.CreateAsync(new TIdentityRole() { Name= "miniauth-admin"  });
                            await userManager.AddToRoleAsync(user, "miniauth-admin");
                        }
#if DEBUG
                        foreach (var item in new[] { "HR", "IT", "RD" })
                        {
                            var user = Activator.CreateInstance<TIdentityUser>();
                            await userStore.SetUserNameAsync(user, $"miniauth-{item.ToLower()}", CancellationToken.None);
                            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
                            await roleManager.CreateAsync(new TIdentityRole() { Name= item });
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

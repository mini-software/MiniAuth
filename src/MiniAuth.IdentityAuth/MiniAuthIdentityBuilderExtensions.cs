using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
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

            builder.UseMiddleware<MiniAuthIdentityMiddleware>();

            if (!builder.Properties.TryGetValue("__UseRouting", out var _))
                builder.UseRouting();
            if (!builder.Properties.TryGetValue("__UseAuthorization", out var _))
                builder.UseAuthorization();

            var miniauthEndpoints = new MiniAuthIdentityEndpoints();
            miniauthEndpoints.MapEndpoints(builder);

            Task.Run(async () =>
            {
                using (var scope = builder.ApplicationServices.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<MiniAuthIdentityDbContext>();
                    if (ctx.Database.EnsureCreated())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MiniAuthIdentityUser>>();
                        
                        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<MiniAuthIdentityUser>>();
                        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MiniAuthIdentityRole>>();
                        {
                            var user = Activator.CreateInstance<MiniAuthIdentityUser>();
                            await userStore.SetUserNameAsync(user, "miniauth", CancellationToken.None);
                            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
                            await roleManager.CreateAsync(new MiniAuthIdentityRole("miniauth-admin") { Type = "miniauth" });
                            await userManager.AddToRoleAsync(user, "miniauth-admin");
                        }
#if DEBUG
                        foreach (var item in new[] { "HR","IT","RD"})
                        {
                            var user = Activator.CreateInstance<MiniAuthIdentityUser>();
                            await userStore.SetUserNameAsync(user, $"miniauth-{item.ToLower()}", CancellationToken.None);
                            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
                            await roleManager.CreateAsync(new MiniAuthIdentityRole(item) { Type = null });
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;


public static class MiniAuthIdentityBuilderExtensions
{

    public static IApplicationBuilder UseMiniAuth<TDbContext, TIdentityUser, TIdentityRole>
        (this IApplicationBuilder builder)
        where TDbContext : IdentityDbContext
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : IdentityRole, new()
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.UseMiddleware<MiniAuthIdentityMiddleware>();

         if (!builder.Properties.TryGetValue("__UseRouting", out var _))
            builder.UseRouting();
        if (!builder.Properties.TryGetValue("__AuthenticationMiddlewareSet", out var _))
            builder.UseAuthentication();
        if (!builder.Properties.TryGetValue("__AuthorizationMiddlewareSet", out var _))
            builder.UseAuthorization();
        // {[__AuthorizationMiddlewareSet, true]}
        // check AuthorizationMiddleware exist 
        //if (!builder.Properties.TryGetValue(AuthorizationAppBuilderExtensions.AuthorizationMiddlewareSetKey, out var _))
        //    throw new InvalidOperationException("UseAuthorization must be called before UseMiniAuth");

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
                    await CreateUserAndRoles<TDbContext,TIdentityUser, TIdentityRole>(ctx,scope);
                }
                else
                {
                    var miniauthUser = await ctx.Set<TIdentityUser>().FirstOrDefaultAsync(o => o.UserName == "admin@mini-software.github.io");
                    if (miniauthUser == null)
                        await CreateUserAndRoles<TDbContext,TIdentityUser, TIdentityRole>(ctx,scope);
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

    private static async Task CreateUserAndRoles<TDbContext,TIdentityUser, TIdentityRole>(TDbContext ctx,IServiceScope scope)
        where TDbContext : IdentityDbContext
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : IdentityRole, new()
    {
        Debug.WriteLine("* Create miniauth user and role");
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<TIdentityUser>>();

        {
            var user = Activator.CreateInstance<TIdentityUser>();
            // private readonly IUserEmailStore<IdentityUser> _emailStore;
            var emailStore = userStore as IUserEmailStore<TIdentityUser>;
            var email = "admin@mini-software.github.io";
            var account = email;
            var roleName = "miniauth-admin";
            await userStore.SetUserNameAsync(user, account, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
            // user complete email and phone and two-factor
            user = await userManager.FindByNameAsync(account);
            user.PhoneNumberConfirmed= true;
            user.EmailConfirmed = true;
            //user.TwoFactorEnabled = true; // ![image](https://github.com/mini-software/MiniExcel/assets/12729184/9acd3900-f76c-4322-aabf-0d5ec2131fcb)
            user.Email = email;
            user.NormalizedEmail = email.ToUpper();
            await userManager.UpdateAsync(user);
            ctx.Roles.Add(new TIdentityRole {  Name = roleName, NormalizedName= roleName.ToUpper() });
            ctx.SaveChanges();
            var role = ctx.Roles.FirstOrDefault(c => c.Name == roleName);
            ctx.UserRoles.Add(new IdentityUserRole<string> { RoleId = role.Id, UserId = user.Id });
        }
#if DEBUG
        foreach (var roleKey in new[] { "HR", "IT", "RD" })
        {
            var user = Activator.CreateInstance<TIdentityUser>();
            var account = $"{roleKey.ToLower()}@mini-software.github.io";
            await userStore.SetUserNameAsync(user, account, CancellationToken.None);
            await userManager.CreateAsync(user, "E7c4f679-f379-42bf-b547-684d456bc37f");
            user = await userManager.FindByNameAsync(account);
            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;
            //user.TwoFactorEnabled = true; // ![image](https://github.com/mini-software/MiniExcel/assets/12729184/9acd3900-f76c-4322-aabf-0d5ec2131fcb)
            user.Email = account;
            user.NormalizedEmail = account.ToUpper();
            await userManager.UpdateAsync(user);
            ctx.Roles.Add(new TIdentityRole {  Name = roleKey, NormalizedName = roleKey.ToUpper() });
            ctx.SaveChanges();
            var role = ctx.Roles.FirstOrDefault(ctx => ctx.Name == roleKey);
            ctx.UserRoles.Add(new IdentityUserRole<string> { RoleId = role.Id, UserId = user.Id });
            ctx.SaveChanges();
        } 
#endif
    }

    private static async Task CreateUserAndRole<TIdentityUser, TIdentityRole>(UserManager<TIdentityUser> userManager, IUserStore<TIdentityUser> userStore, RoleManager<TIdentityRole> roleManager)
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : IdentityRole, new()
    {

    }

}


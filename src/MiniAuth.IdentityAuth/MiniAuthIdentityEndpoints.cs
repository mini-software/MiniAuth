using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniAuth.IdentityAuth.Helpers;
using MiniAuth.IdentityAuth.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace MiniAuth.Identity
{
    public class MiniAuthIdentityEndpoints<TDbContext, TIdentityUser, TIdentityRole>
        where TDbContext : IdentityDbContext
        where TIdentityUser : IdentityUser,new()
        where TIdentityRole : IdentityRole,new()
    {
        public static ConcurrentDictionary<string, RoleEndpointEntity> _endpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>();
        public void MapEndpoints(IApplicationBuilder builder)
        {
            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/miniauth/api/getAllEndpoints", async (HttpContext context,
                    TDbContext _dbContext
                ) =>
                {
                    await OkResult(context, _endpointCache.Values.OrderByDescending<RoleEndpointEntity, string>(o => o.Id).ToJson());
                })
                .RequireAuthorization("miniauth-admin")
                ;

                endpoints.MapPost("/miniauth/login", async (HttpContext context
                    , TDbContext _dbContext
                    , SignInManager<TIdentityUser> signInManager
                ) =>
                {
                    JsonDocument bodyJson = await GetBodyJson(context);
                    var root = bodyJson.RootElement;
                    var userName = root.GetProperty<string>("username");
                    var password = root.GetProperty<string>("password");
                    var remember = root.GetProperty<bool>("remember");
                    var result = await signInManager.PasswordSignInAsync(userName, password, remember, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var newToken = Guid.NewGuid().ToString();
                        //context.Response.Cookies.Append("X-MiniAuth-Token", newToken);
                        await OkResult(context, $"{{\"X-MiniAuth-Token\":\"{newToken}\"}}");
                        return;
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                });

                endpoints.MapGet("/miniauth/api/getRoles", async (HttpContext context
                    , TDbContext _dbContext
                ) =>
                {
                    var roles = (_dbContext.Roles.ToArray());
                    await OkResult(context, roles.ToJson());
                }).RequireAuthorization("miniauth-admin");

                endpoints.MapPost("/miniauth/api/saveRole", async (HttpContext context
                    , TDbContext _dbContext
                ) =>
                {
                    JsonDocument bodyJson = await GetBodyJson(context);
                    var root = bodyJson.RootElement;
                    var id = root.GetProperty<string>("Id");
                    var name = root.GetProperty<string>("Name");
                    var enable = root.GetProperty<bool>("Enable");
                    var type = root.GetProperty<string>("Type");
                    var role = await _dbContext.Roles.FindAsync(id);
                    if (role == null)
                    {
                        role = new TIdentityRole() { Name= name };
                        role.Id = id;
                        await _dbContext.Roles.AddAsync(role);
                    }
                    else
                    {
                        role.Name = name;
                    }
                    await _dbContext.SaveChangesAsync();
                    await OkResult(context, "".ToJson(code: 200, message: ""));
                }).RequireAuthorization("miniauth-admin");


                endpoints.MapPost("/miniauth/api/deleteRole", async (HttpContext context
                    , TDbContext _dbContext
                ) =>
                {
                    JsonDocument bodyJson = await GetBodyJson(context);
                    var root = bodyJson.RootElement;
                    var id = root.GetProperty<string>("Id");
                    var role = await _dbContext.Roles.FindAsync(id);
                    if (role != null)
                    {
                        _dbContext.Roles.Remove(role);
                        await _dbContext.SaveChangesAsync();
                    }
                    await OkResult(context, "".ToJson(code: 200, message: ""));
                }).RequireAuthorization("miniauth-admin");


                endpoints.MapPost("/miniauth/api/getUsers", async (HttpContext context
                    , TDbContext _dbContext
                ) =>
                {
                    JsonDocument bodyJson = await GetBodyJson(context);
                    var root = bodyJson.RootElement;
                    var pageIndex = root.GetProperty<int>("pageIndex");
                    var pageSize = root.GetProperty<int>("pageSize");
                    var offset = pageIndex * pageSize;
                    var users = _dbContext.Users.Skip(offset).Take(pageSize)
                        .Select(s => new
                        {
                            Id = s.Id,
                            Username = s.UserName,
                            Mail = s.Email,
                            Roles = _dbContext.UserRoles.Where(w => w.UserId == s.Id).Select(s => s.RoleId).ToArray(),
                        });
                    var totalItems = _dbContext.Users.Count();
                    await OkResult(context, new { users, totalItems }.ToJson());
                }).RequireAuthorization("miniauth-admin");

                endpoints.MapPost("/miniauth/api/saveUser", async (HttpContext context
                    , TDbContext _dbContext
                    , UserManager<TIdentityUser> userManager
                ) =>
                {
                    JsonDocument bodyJson = await GetBodyJson(context);
                    var root = bodyJson.RootElement;
                    var id = root.GetProperty<string>("Id");
                    var username = root.GetProperty<string>("Username");
                    var first_name = root.GetProperty<string>("First_name");
                    var last_name = root.GetProperty<string>("Last_name");
                    var emp_no = root.GetProperty<string>("Emp_no");
                    var mail = root.GetProperty<string>("Mail");
                    var enable = root.GetProperty<bool>("Enable");
                    var roles = root.GetProperty<string[]>("Roles");
                    var type = root.GetProperty<string>("Type");
                    TIdentityUser user = (TIdentityUser)await _dbContext.Users.FindAsync(id);
                    var isUserExist = user == null;
                    if (isUserExist)
                    {
                        user = new TIdentityUser
                        {
                            UserName = username,
                            Email = mail,
                        };
                        await _dbContext.Users.AddAsync(user);
                    }
                    else
                    {
                        user.UserName = username;
                        user.Email = mail;
                    }
                    await _dbContext.SaveChangesAsync();
                    var userRoles = _dbContext.UserRoles.Where(w => w.UserId == user.Id).ToArray();
                    foreach (var userRole in userRoles)
                    {
                        _dbContext.UserRoles.Remove(userRole);
                    }
                    foreach (var role in roles)
                    {
                        var userRole = new IdentityUserRole<string>
                        {
                            UserId = user.Id,
                            RoleId = role
                        };
                        await _dbContext.UserRoles.AddAsync(userRole);
                    }

                    if (isUserExist)
                    {
                        string newPassword = GetNewPassword();
                        await userManager.AddPasswordAsync(user, newPassword);
                        await _dbContext.SaveChangesAsync();
                        await OkResult(context, new { newPassword }.ToJson(code: 200, message: ""));
                    }
                    else
                    {
                        await _dbContext.SaveChangesAsync();
                        await OkResult(context, "".ToJson(code: 200, message: ""));
                    }
                }).RequireAuthorization("miniauth-admin");

                endpoints.MapPost("/miniauth/api/resetPassword", async (HttpContext context
                    , TDbContext _dbContext
                    , UserManager<TIdentityUser> userManager
                ) =>
                {
                    JsonDocument bodyJson = await GetBodyJson(context);
                    var root = bodyJson.RootElement;
                    var id = root.GetProperty<string>("Id");
                    var password = root.GetProperty<string>("Password");
                    TIdentityUser user = (TIdentityUser)await _dbContext.Users.FindAsync(id);
                    if (user != null)
                    {
                        string newPassword = GetNewPassword();
                        var result = await userManager.RemovePasswordAsync(user);
                        if (result.Succeeded)
                        {
                            result = await userManager.AddPasswordAsync(user, newPassword);
                            if (result.Succeeded)
                            {
                                await OkResult(context, new { newPassword }.ToJson(code: 200, message: ""));
                            }
                            else
                            {
                                await OkResult(context, "".ToJson(code: 500, message: result.Errors.Select(s => s.Description).ToJson()));
                            }
                        }
                        else
                        {
                            await OkResult(context, "".ToJson(code: 500, message: result.Errors.Select(s => s.Description).ToJson()));
                        }
                    }
                    else
                    {
                        await OkResult(context, "".ToJson(code: 404, message: "User not found"));
                    }
                }).RequireAuthorization("miniauth-admin");

            });

            // init cache
            var endpointDataSource = builder.ApplicationServices.GetRequiredService<EndpointDataSource>();
            var endpoints = endpointDataSource.Endpoints;
            foreach (var endpoint in endpoints)
            {
                var routeEndpoint = endpoint as RouteEndpoint;
                if (routeEndpoint != null)
                {
                    var id = routeEndpoint.DisplayName; //TODO
                    var methods = routeEndpoint.Metadata?.GetMetadata<HttpMethodMetadata>()?.HttpMethods.ToArray();
                    var route = routeEndpoint?.RoutePattern.RawText;
                    var isApi = routeEndpoint.Metadata?.GetMetadata<Microsoft.AspNetCore.Mvc.ApiControllerAttribute>() != null;
                    var roleEndpoint = new RoleEndpointEntity
                    {
                        Id = routeEndpoint.DisplayName,
                        Type = "system",
                        Name = routeEndpoint.DisplayName,
                        Route = route,
                        Methods = methods,
                        Enable = true,
                        RedirectToLoginPage = !isApi
                    };
                    MiniAuthIdentityEndpoints<TDbContext,TIdentityUser,TIdentityRole>._endpointCache.TryAdd(id, roleEndpoint);
                }
            }
        }

        private static string GetNewPassword()
        {
            return $"{Guid.NewGuid().ToString().Substring(0, 10).ToUpper()}@{Guid.NewGuid().ToString().Substring(0, 5)}";
        }

        private async Task<JsonDocument> GetBodyJson(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var bodyJson = JsonDocument.Parse(body);
            return bodyJson;
        }

        private static async Task OkResult(HttpContext context, string result, string contentType = "application/json")
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = contentType;
            context.Response.ContentLength = result != null ? Encoding.UTF8.GetByteCount(result) : 0;
            await context.Response.WriteAsync(result).ConfigureAwait(false);
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniAuth.IdentityAuth.Helpers;
using MiniAuth.IdentityAuth.Models;
using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MiniAuth.Identity
{
    public class MiniAuthIdentityEndpoints
    {
        public static ConcurrentDictionary<string, RoleEndpointEntity> _endpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>();
        public void MapEndpoints(IApplicationBuilder builder)
        {
            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/miniauth/api/getAllEndpoints", async (HttpContext context,
                    ILogger<object> _logger,
                    MiniAuthIdentityDbContext _dbContext
                ) =>
                {
                    await OkResult(context, _endpointCache.Values.OrderByDescending<RoleEndpointEntity, string>(o => o.Id).ToJson());
                })
                .RequireAuthorization("miniauth-admin")
                ;

                endpoints.MapPost("/miniauth/login", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    await Login(context, _logger, _dbContext, signInManager, userManager).ConfigureAwait(false);
                });

                endpoints.MapGet("/miniauth/api/getRoles", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    var roles = (_dbContext.Roles.ToArray<MiniAuthIdentityRole>());
                    await OkResult(context, roles.ToJson());
                }).RequireAuthorization("miniauth-admin");

                endpoints.MapPost("/miniauth/api/saveRole", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    await SaveRole(context, _dbContext);
                }).RequireAuthorization("miniauth-admin");


                endpoints.MapPost("/miniauth/api/deleteRole", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    await deleteRole(context, _dbContext);
                }).RequireAuthorization("miniauth-admin");


                endpoints.MapPost("/miniauth/api/getUsers", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    await GetUsers(context, _dbContext);
                }).RequireAuthorization("miniauth-admin");

                endpoints.MapPost("/miniauth/api/saveUser", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    await SaveUser(context, _dbContext);
                }).RequireAuthorization("miniauth-admin");

                // /api/resetPassword
                endpoints.MapPost("/miniauth/api/resetPassword", async (HttpContext context
                    , ILogger<MiniAuthIdentityEndpoints> _logger
                    , MiniAuthIdentityDbContext _dbContext
                    , SignInManager<MiniAuthIdentityUser> signInManager
                    , UserManager<MiniAuthIdentityUser> userManager
                ) =>
                {
                    await ResetPassword(context, _dbContext, userManager);
                }).RequireAuthorization("miniauth-admin");

            });
            InitEndpointsCache(builder);

        }

        private static string GetNewPassword()
        {
            return $"MiniAuth@{Guid.NewGuid().ToString().Substring(0, 10)}";
        }
        private async Task ResetPassword(HttpContext context, MiniAuthIdentityDbContext _dbContext, UserManager<MiniAuthIdentityUser> userManager)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var id = root.GetProperty<string>("Id");
            var password = root.GetProperty<string>("Password");
            var user = await _dbContext.Users.FindAsync(id);
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
        }

        private async Task SaveUser(HttpContext context, MiniAuthIdentityDbContext _dbContext)
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
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                user = new MiniAuthIdentityUser
                {
                    UserName = username,
                    First_name = first_name,
                    Last_name = last_name,
                    Emp_no = emp_no,
                    Email = mail,
                    Enable = enable,
                    Type = type
                };
                await _dbContext.Users.AddAsync(user);
            }
            else
            {
                user.UserName = username;
                user.First_name = first_name;
                user.Last_name = last_name;
                user.Emp_no = emp_no;
                user.Email = mail;
                user.Enable = enable;
                user.Type = type;
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
            await _dbContext.SaveChangesAsync();
            await OkResult(context, "".ToJson(code: 200, message: ""));
        }


        private async Task GetUsers(HttpContext context, MiniAuthIdentityDbContext _dbContext)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var pageIndex = root.GetProperty<int>("pageIndex");
            var pageSize = root.GetProperty<int>("pageSize");
            var offset = pageIndex * pageSize;
            var users = _dbContext.Users.Skip(offset).Take(pageSize)
                .Select(s => new MiniAuthUserVo
                {
                    Id = s.Id,
                    Username = s.UserName,
                    First_name = s.First_name,
                    Last_name = s.Last_name,
                    Emp_no = s.Emp_no,
                    Mail = s.Email,
                    Enable = s.Enable,
                    Roles = _dbContext.UserRoles.Where(w => w.UserId == s.Id).Select(s => s.RoleId).ToArray(),
                    Type = s.Type
                });
            var totalItems = _dbContext.Users.Count();
            await OkResult(context, new { users, totalItems }.ToJson());
        }

        private async Task deleteRole(HttpContext context, MiniAuthIdentityDbContext _dbContext)
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
        }

        private async Task SaveRole(HttpContext context, MiniAuthIdentityDbContext _dbContext)
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
                role = new MiniAuthIdentityRole(name);
                role.Id = id;
                role.Enable = enable;
                role.Type = type;
                await _dbContext.Roles.AddAsync(role);
            }
            else
            {
                role.Name = name;
                role.Enable = enable;
                role.Type = type;
            }
            await _dbContext.SaveChangesAsync();
            await OkResult(context, "".ToJson(code: 200, message: ""));
        }

        private async Task Login(HttpContext context, ILogger<MiniAuthIdentityEndpoints> _logger, MiniAuthIdentityDbContext _dbContext, SignInManager<MiniAuthIdentityUser> signInManager, UserManager<MiniAuthIdentityUser> userManager)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var userName = root.GetProperty<string>("username");
            var password = root.GetProperty<string>("password");
            var remember = root.GetProperty<bool>("remember");
            var result = await signInManager.PasswordSignInAsync(userName, password, remember, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                var newToken = Guid.NewGuid().ToString();
                context.Response.Cookies.Append("X-MiniAuth-Token", newToken);
                await OkResult(context, $"{{\"X-MiniAuth-Token\":\"{newToken}\"}}");
                return;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        private async Task<JsonDocument> GetBodyJson(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var bodyJson = JsonDocument.Parse(body);
            return bodyJson;
        }

        private static void InitEndpointsCache(IApplicationBuilder builder)
        {
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
                    MiniAuthIdentityEndpoints._endpointCache.TryAdd(id, roleEndpoint);
                }
            }
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

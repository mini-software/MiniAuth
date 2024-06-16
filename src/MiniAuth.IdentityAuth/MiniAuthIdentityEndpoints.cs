using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniAuth;
using MiniAuth.IdentityAuth.Helpers;
using MiniAuth.IdentityAuth.Models;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


internal class MiniAuthIdentityEndpoints<TDbContext, TIdentityUser, TIdentityRole>
    where TDbContext : IdentityDbContext
    where TIdentityUser : IdentityUser, new()
    where TIdentityRole : IdentityRole, new()
{
    public static ConcurrentDictionary<string, RoleEndpointEntity> _endpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>();
    public void MapEndpoints(IApplicationBuilder builder)
    {
        builder.UseEndpoints(endpoints =>
        {
            endpoints.MapGet($"/{MiniAuthOptions.RoutePrefix}/api/getAllEndpoints", async (HttpContext context,
                TDbContext _dbContext
            ) =>
            {
                await OkResult(context, _endpointCache.Values.OrderByDescending<RoleEndpointEntity, string>(o => o.Id).ToJson());
            })
                .RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });
            endpoints.MapGet($"/{MiniAuthOptions.RoutePrefix}/logout", async (HttpContext context
                                   , SignInManager<TIdentityUser> signInManager
                                   , IOptions<IdentityOptions> identityOptionsAccessor
                                                  ) =>
            {

                await signInManager.SignOutAsync();
                context.Response.Redirect(MiniAuthOptions.LoginPath);

            });

            if (!MiniAuthOptions.DisableMiniAuthLogin)
            {
                endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/login", async (
                   [FromBody] LoginRequest login
                    , [FromServices] IServiceProvider sp
                    , HttpContext context
                ) =>
                {
                    UserManager<TIdentityUser> _userManager = sp.GetRequiredService<UserManager<TIdentityUser>>();
                    TDbContext _dbContext = sp.GetRequiredService<TDbContext>();
                    SignInManager<TIdentityUser> signInManager = sp.GetRequiredService<SignInManager<TIdentityUser>>();

                    if (MiniAuth.MiniAuthOptions.AuthenticationType == MiniAuthOptions.AuthType.BearerJwt)
                    {
                        var user = await _dbContext.Users.FirstOrDefaultAsync(f => f.UserName == login.username);
                        if (!(user != null && await _userManager.CheckPasswordAsync((TIdentityUser)user, login.password)))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }
                        // Payload issuer
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Name, user.UserName)
                        };
                        var userRoles = _dbContext.UserRoles.Where(w => w.UserId == user.Id).Select(s => s.RoleId).ToArray();
                        var rolesName = _dbContext.Roles.Where(w => userRoles.Contains(w.Id)).Select(s => s.Name).ToArray();
                        foreach (var item in rolesName)
                            claims.Add(new Claim(ClaimTypes.Role, item));
                        claims.Add(new Claim("sub", user.UserName));

                        var secretkey = MiniAuthOptions.JWTKey;
                        var credentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);
                        var tokenDescriptor = new SecurityTokenDescriptor()
                        {
                            Subject = new ClaimsIdentity(claims),
                            Expires = DateTime.UtcNow.AddSeconds(MiniAuthOptions.TokenExpiresIn),
                            Issuer = MiniAuthOptions.Issuer,
                            SigningCredentials = credentials

                        };
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenJwt =  tokenHandler.CreateToken(tokenDescriptor);
                        var token = tokenHandler.WriteToken(tokenJwt);
                        var result = new
                        {
                            tokenType = "Bearer",
                            accessToken = token,
                            expiresIn = MiniAuthOptions.TokenExpiresIn,
                        };

                        await OkResult(context, result.ToJson());
                        return;
                    }
                    else
                    {
                        var result = await signInManager.PasswordSignInAsync(login.username, login.password, login.remember, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            var newToken = Guid.NewGuid().ToString();
                            var jsonResult = new
                            {
                                token = newToken,
                                expiration = null as DateTime?
                            };
                            await OkResult(context, jsonResult.ToJson());
                            return;
                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }
                    }
                });
            }


            endpoints.MapGet($"/{MiniAuthOptions.RoutePrefix}/api/getRoles", async (HttpContext context
                , TDbContext _dbContext
            ) =>
            {
                var roles = (_dbContext.Roles.ToArray().Select(s =>
                {
                    var claims = _dbContext.RoleClaims.Where(w => w.RoleId == s.Id).ToArray();
                    return new
                    {
                        s.Id,
                        s.Name,
                        Enable = claims.FirstOrDefault(f => f.ClaimType == "Enable")?.ClaimValue != "False",
                        Remark = claims.FirstOrDefault(f => f.ClaimType == "Remark")?.ClaimValue,
                    };
                }));
                await OkResult(context, roles.ToJson());
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });

            endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/api/saveRole", async (HttpContext context
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
                    role = new TIdentityRole() { Name = name };
                    role.Id = Guid.NewGuid().ToString();
                    role.NormalizedName = name.ToUpper();
                    await _dbContext.Roles.AddAsync(role);
                }
                else
                {
                    role.Name = name;
                    role.NormalizedName = name.ToUpper();
                }

                // save role claims for enable and remark
                var roleClaims = _dbContext.RoleClaims.Where(w => w.RoleId == role.Id).ToArray();
                {
                    string[] keys = new[] { "Enable", "Remark" };
                    foreach (var item in keys)
                    {
                        var roleClaim = roleClaims.FirstOrDefault(f => f.ClaimType == item);
                        var val = root.GetProperty<object>(item);
                        if (val == null)
                            continue;
                        if (roleClaim == null)
                        {
                            roleClaim = new IdentityRoleClaim<string>
                            {
                                RoleId = role.Id,
                                ClaimType = item,
                                ClaimValue = val?.ToString(),
                            };
                            await _dbContext.RoleClaims.AddAsync(roleClaim);
                        }
                        else
                        {
                            roleClaim.ClaimValue = val?.ToString();
                        }
                    }
                };


                await _dbContext.SaveChangesAsync();
                await OkResult(context, "".ToJson(code: 200, message: ""));
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });


            endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/api/deleteRole", async (HttpContext context
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
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });


            endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/api/getUsers", async (HttpContext context
                , TDbContext _dbContext
            ) =>
            {
                JsonDocument bodyJson = await GetBodyJson(context);
                var root = bodyJson.RootElement;
                var pageIndex = root.GetProperty<int>("pageIndex");
                var pageSize = root.GetProperty<int>("pageSize");
                //search value filter
                var search = root.GetProperty<string>("search");
                var users = _dbContext.Users.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                    users = users.Where(w => w.UserName.Contains(search) || w.Email.Contains(search));
                var offset = pageIndex * pageSize;
                var userVo = users.Skip(offset).Take(pageSize).ToArray()
                    .Select(s =>
                    {
                        var claims = _dbContext.UserClaims.Where(w => w.UserId == s.Id).ToArray();
                        var result = new
                        {
                            Id = s.Id,
                            Username = s.UserName,
                            Mail = s.Email,
                            EmailConfirmed = s.EmailConfirmed,
                            PhoneNumber = s.PhoneNumber,
                            PhoneNumberConfirmed = s.PhoneNumberConfirmed,
                            TwoFactorEnabled = s.TwoFactorEnabled,
                            LockoutEnd = s.LockoutEnd?.ToString("yyyy-MM-ddTHH:mm"),
                            LockoutEnabled = s.LockoutEnabled,
                            AccessFailedCount = s.AccessFailedCount,

                            Roles = _dbContext.UserRoles.Where(w => w.UserId == s.Id).Select(s => s.RoleId).ToArray(),

                            First_name = claims.FirstOrDefault(f => f.ClaimType == "First_name")?.ClaimValue,
                            Last_name = claims.FirstOrDefault(f => f.ClaimType == "Last_name")?.ClaimValue,
                            // enable null = true
                            Enable = claims.FirstOrDefault(f => f.ClaimType == "Enable")?.ClaimValue != "False",
                            Emp_no = claims.FirstOrDefault(f => f.ClaimType == "Emp_no")?.ClaimValue,
                        };
                        return result;
                    });
                var totalItems = _dbContext.Users.Count();
                await OkResult(context, new { users = userVo, totalItems }.ToJson());
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });

            endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/api/deleteUser", async (HttpContext context
                               , TDbContext _dbContext
                                          ) =>
            {
                JsonDocument bodyJson = await GetBodyJson(context);
                var root = bodyJson.RootElement;
                var id = root.GetProperty<string>("Id");
                var user = await _dbContext.Users.FindAsync(id);
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
                }
                await OkResult(context, "".ToJson(code: 200, message: ""));
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });

            endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/api/saveUser", async (HttpContext context
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
                var EmailConfirmed = root.GetProperty<bool>("EmailConfirmed");
                var PhoneNumberConfirmed = root.GetProperty<bool>("PhoneNumberConfirmed");
                var PhoneNumber = root.GetProperty<string>("PhoneNumber");
                var TwoFactorEnabled = root.GetProperty<bool>("TwoFactorEnabled");
                var LockoutEnd = root.GetProperty<DateTimeOffset?>("LockoutEnd");
                var LockoutEnabled = root.GetProperty<bool>("LockoutEnabled");
                var roles = root.GetProperty<string[]>("Roles");
                TIdentityUser user = (TIdentityUser)await _dbContext.Users.FindAsync(id);

                var isUserExist = user == null;
                if (isUserExist)
                {
                    user = new TIdentityUser
                    {
                        UserName = username,
                        NormalizedUserName = username?.ToUpper(),
                        Email = mail,
                        NormalizedEmail = mail?.ToUpper(),
                        EmailConfirmed = EmailConfirmed,
                        PhoneNumberConfirmed = PhoneNumberConfirmed,
                        PhoneNumber = PhoneNumber,
                        TwoFactorEnabled = TwoFactorEnabled,
                        LockoutEnd = LockoutEnd,
                        LockoutEnabled = LockoutEnabled,

                    };
                    await _dbContext.Users.AddAsync(user);
                }
                else
                {
                    user.UserName = username;
                    user.NormalizedUserName = username?.ToUpper();
                    user.Email = mail;
                    user.NormalizedEmail = mail?.ToUpper();
                    user.EmailConfirmed = EmailConfirmed;
                    user.PhoneNumberConfirmed = PhoneNumberConfirmed;
                    user.PhoneNumber = PhoneNumber;
                    user.TwoFactorEnabled = TwoFactorEnabled;
                    user.LockoutEnd = LockoutEnd;
                    user.LockoutEnabled = LockoutEnabled;
                }

                var userRoles = _dbContext.UserRoles.Where(w => w.UserId == user.Id).ToArray();
                {
                    var roleIds = userRoles.Select(s => s.RoleId).ToArray();
                    foreach (var role in roles)
                    {
                        var userRole = userRoles.FirstOrDefault(f => f.RoleId == role);
                        if (userRole == null)
                        {
                            userRole = new IdentityUserRole<string>
                            {
                                UserId = user.Id,
                                RoleId = role,
                            };
                            await _dbContext.UserRoles.AddAsync(userRole);
                        }
                    }
                    foreach (var item in userRoles)
                    {
                        if (!roles.Contains(item.RoleId))
                        {
                            _dbContext.UserRoles.Remove(item);
                        }
                    }
                }

                var userClaims = _dbContext.UserClaims.Where(w => w.UserId == user.Id).ToArray();
                {
                    string[] keys = new[] { "First_name", "Last_name", "Emp_no", "Enable" };
                    foreach (var item in keys)
                    {
                        var userClaim = userClaims.FirstOrDefault(f => f.ClaimType == item);
                        var val = root.GetProperty<object>(item);
                        if (val == null)
                            continue;
                        if (userClaim == null)
                        {
                            userClaim = new IdentityUserClaim<string>
                            {
                                UserId = user.Id,
                                ClaimType = item,
                                ClaimValue = val?.ToString(),
                            };
                            await _dbContext.UserClaims.AddAsync(userClaim);
                        }
                        else
                        {
                            userClaim.ClaimValue = val?.ToString();
                        }
                    }
                };

                if (isUserExist)
                {
                    string newPassword = GetNewPassword();
                    if (!await userManager.HasPasswordAsync(user))
                    {
                        await _dbContext.SaveChangesAsync();
                        await userManager.AddPasswordAsync(user, newPassword);
                    }
                    await _dbContext.SaveChangesAsync();
                    await OkResult(context, new { newPassword }.ToJson(code: 200, message: ""));
                }
                else
                {
                    await _dbContext.SaveChangesAsync();
                    await OkResult(context, "".ToJson(code: 200, message: ""));
                }
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });

            endpoints.MapPost($"/{MiniAuthOptions.RoutePrefix}/api/resetPassword", async (HttpContext context
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
            }).RequireAuthorization(new AuthorizeAttribute() { Roles = "miniauth-admin" });

            endpoints.MapGet($"/{MiniAuthOptions.RoutePrefix}/api/getUserInfo", async (HttpContext context
            , TDbContext _dbContext
            ) =>
            {
                var user = context.User;
                if (!user.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                var userEntity = await _dbContext.Users.FindAsync(user.FindFirst(ClaimTypes.NameIdentifier).Value);
                await OkResult(context, userEntity.ToJson());
            });

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
                MiniAuthIdentityEndpoints<TDbContext, TIdentityUser, TIdentityRole>._endpointCache.TryAdd(id, roleEndpoint);
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

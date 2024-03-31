using JWT.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniAuth.Configs;
using MiniAuth.Exceptions;
using MiniAuth.Helpers;
using MiniAuth.Managers;
using MiniAuth.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MiniAuth.Managers.RoleEndpointManager;

namespace MiniAuth
{
    public partial class MiniAuthMiddleware
    {
        private const string EmbeddedFileNamespace = "MiniAuth.wwwroot";
        private readonly RequestDelegate _next;
        private readonly IEnumerable<EndpointDataSource> _endpointSources;
        private readonly IMiniAuthDB _db;
        private readonly StaticFileMiddleware _staticFileMiddleware;
        private readonly MiniAuthOptions _options;
        private readonly IJWTManager _jwtManager;
        private readonly IUserManager _userManer;
        private readonly IRoleEndpointManager _endpointManager;
        private readonly ILogger<MiniAuthMiddleware> _logger;
        private readonly ConcurrentDictionary<string, RoleEndpointEntity> _endpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>();
        private RoleEndpointEntity _routeEndpoint;
        private bool _isMiniAuthPath;

        public MiniAuthMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment hostingEnv,
            ILogger<MiniAuthMiddleware> logger,
            IJWTManager jwtManager = null,
            MiniAuthOptions options = null,
            IRoleEndpointManager endpointManager = null,
            IUserManager userManager = null,
            IEnumerable<EndpointDataSource> endpointSources = null,
            IMiniAuthDB db = null
        )
        {
            var host = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000";
            this._logger = logger;
            this._logger.LogInformation($"MiniAuth management page : {host}/miniauth/index.html");
            this._next = next;
            if (db == null)
                this._db = new MiniAuthDB<SQLiteConnection>("Data Source=miniauth.db;Version=3;");
            else
                this._db = db;
            if (options == null)
                _options = new MiniAuthOptions();
            else
                _options = options;
            if (jwtManager == null)
                _jwtManager = new JWTManager(_options.SubjectName, _options.Password, _options.CerPath);
            else
                _jwtManager = jwtManager;
            if (userManager == null)
                _userManer = new UserManager(this._db);
            else
                _userManer = userManager;
            if (endpointManager == null)
                _endpointManager = new RoleEndpointManager(this._db);
            else
                _endpointManager = endpointManager;
            this._endpointSources = endpointSources;
            this._staticFileMiddleware = CreateStaticFileMiddleware(next, loggerFactory, hostingEnv); ;

            {
                var systemEndpoints = _endpointManager.GetEndpointsAsync(_endpointSources).GetAwaiter().GetResult();
                var cache = systemEndpoints.ToDictionary(p => p.Id);
                var endpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>(cache);
                _endpointCache = endpointCache;
            }
        }

        private StaticFileMiddleware CreateStaticFileMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnv)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(_options.RoutePrefix) ? string.Empty : $"/{_options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(MiniAuthMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                _ = context ?? throw new ArgumentNullException(nameof(context));
                _isMiniAuthPath = context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}", out PathString subPath);

                _routeEndpoint = GetEndpoint(context);
                if (_routeEndpoint == null && !_isMiniAuthPath)
                {
                    await _next(context);
                    return;
                }

                var isAuth = IsAuth(context);
                if (!isAuth)
                    return;

#if DEBUG
                _logger.LogInformation($"Path: {context.Request.Path}, isAuth: {isAuth}, _isMiniAuthPath: {_isMiniAuthPath}, _routeEndpoint: {_routeEndpoint?.Id} {_routeEndpoint?.Name} {_routeEndpoint?.Route} {_routeEndpoint?.Type}");
#endif

                if (_isMiniAuthPath)
                {
                    if (context.Request.Path.Equals($"/{_options.RoutePrefix}/login.html"))
                    {
                        await _staticFileMiddleware.Invoke(context);
                        return;
                    }
                    if (context.Request.Path.Equals($"/{_options.RoutePrefix}/login"))
                    {
                        if (context.Request.Method == "POST")
                        {
                            await Login(context).ConfigureAwait(false);
                            return;
                        }
                    }
                    if (context.Request.Path.Equals($"/{_options.RoutePrefix}/logout", StringComparison.OrdinalIgnoreCase))
                    {
                        Logout(context);
                        return;
                    }
                    if (context.Request.Path.Value.EndsWith(".js")
                        || context.Request.Path.Value.EndsWith("css")
                        || context.Request.Path.Value.EndsWith(".ico"))
                    {
                        await _staticFileMiddleware.Invoke(context);
                        return;
                    }
                    if (subPath.StartsWithSegments("/api/getAllEndpoints"))
                    {
                        await OkResult(context, _endpointCache.Values.OrderBy(_ => _.Route).ToJson());
                        return;
                    }
                    if (subPath.StartsWithSegments("/api/getRoles"))
                    {
                        await GetRoles(context);
                        return;
                    }

                    if (subPath.StartsWithSegments("/api/getUsers"))
                    {
                        await GetUsers(context);
                        return;
                    }

                    if (subPath.StartsWithSegments("/api/resetPassword"))
                    {
                        await ResetPassword(context);
                        return;
                    }

                    if (subPath.StartsWithSegments("/api/saveUser"))
                    {
                        await SaveUser(context);
                        return;
                    }
                    if (subPath.StartsWithSegments("/api/deleteUser"))
                    {
                        await DeleteUser(context);
                        return;
                    }
                    if (subPath.StartsWithSegments("/api/deleteRole"))
                    {
                        await DeleteRole(context);
                        return;
                    }
                    if (subPath.StartsWithSegments("/api/saveRole"))
                    {
                        await SaveRole(context);
                        return;
                    }
                    if (subPath.StartsWithSegments("/api/saveEndpoint"))
                    {
                        await SaveEndpoint(context);
                        return;
                    }
                    if (context.Request.Path.Value.EndsWith(".html"))
                    {
                        await _staticFileMiddleware.Invoke(context);
                        return;
                    }
                }
                await _next(context);
                return;
            }
            catch (Exception e)
            {
                await NotOkResult(context, "".ToJson(code: 500, message: e.Message));
                _logger.LogError(e, e.StackTrace);
                _logger.LogError(e, e.Message);
#if DEBUG
                throw;
#endif
                return;
            }
        }

        private async Task SaveEndpoint(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var id = root.GetProperty("Id").GetString();
            var roles = root.GetProperty("Roles").Deserialize<string[]>();
            var enable = root.GetProperty("Enable").GetBoolean();
            var redirectToLoginPage = root.GetProperty("RedirectToLoginPage").GetBoolean();
            var cacheEndpoint = _endpointCache.Values.Single(w => w.Id == id);
            cacheEndpoint.Enable = enable;
            cacheEndpoint.Roles = roles;
            cacheEndpoint.RedirectToLoginPage = redirectToLoginPage;
            await _endpointManager.UpdateEndpoint(cacheEndpoint);
            await OkResult(context, "".ToJson(code: 200, message: ""));
        }

        private async Task SaveRole(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var id = root.GetProperty("Id").GetString();
            var name = root.GetProperty("Name").GetString();
            var enable = root.GetProperty("Enable").GetBoolean();
            using (var cn = this._db.GetConnection())
            {
                using (var command = cn.CreateCommand())
                {
                    if (id == null)
                    {
                        command.CommandText = @"insert into roles (id,name,enable) values (@id,@name,@enable)";
                        command.AddParameters(new Dictionary<string, object>()
                                    {
                                        { "@id", Helpers.IdHelper.NewId() },
                                        { "@name", name },
                                        { "@enable", enable ? 1 : 0 },
                                    });
                    }
                    else
                    {
                        command.CommandText = @"update roles set name = @name,enable=@enable where id = @id";
                        command.AddParameters(new Dictionary<string, object>()
                                {
                                    { "@id", id },
                                    { "@name", name },
                                    { "@enable", enable ? 1 : 0 },
                                });
                    }

                    command.ExecuteNonQuery();
                }
            }
            await OkResult(context, "".ToJson(code: 200, message: ""));
        }

        private async Task DeleteRole(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            if (!root.TryGetProperty("Id", out var _id))
                throw new MiniAuthException("Without Id key");
            var id = _id.GetString();
            using (var cn = this._db.GetConnection())
            {
                using (var command = cn.CreateCommand())
                {
                    if (id != null)
                    {
                        command.CommandText = @"delete from Roles where id = @id";
                        command.AddParameters(new Dictionary<string, object>()
                                    {
                                        { "@id", _id },
                                    });
                        command.ExecuteNonQuery();
                        await OkResult(context, "".ToJson(code: 200, message: ""));
                    }
                    else
                    {
                        throw new MiniAuthException("Id is null");
                    }
                }
            }
        }

        private async Task DeleteUser(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            if (!root.TryGetProperty("Id", out var _id))
                throw new MiniAuthException("Without Id key");
            var id = _id.GetString();
            using (var cn = this._db.GetConnection())
            {
                using (var command = cn.CreateCommand())
                {
                    if (id != null)
                    {
                        command.CommandText = @"delete from users where id = @id";
                        command.AddParameters(new Dictionary<string, object>()
                                    {
                                        { "@id", _id },
                                    });
                        command.ExecuteNonQuery();
                        await OkResult(context, "".ToJson(code: 200, message: ""));
                    }
                    else
                    {
                        throw new MiniAuthException("Id is null");
                    }
                }
            }
        }

        private async Task ResetPassword(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var id = root.GetProperty("Id").GetString();
            var newPassword = Guid.NewGuid().ToString("N").Substring(0, 10);
            _userManer.UpdatePassword(id, newPassword);
            await OkResult(context, new { newPassword }.ToJson(code: 200, message: ""));
        }

        private async Task GetUsers(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var pageIndex = root.GetProperty("pageIndex").GetInt32();
            var pageSize = root.GetProperty("pageSize").GetInt32();
            var offset = pageIndex * pageSize;
            var totalItems = default(long);
            var users = new List<dynamic>();
            using (var cn = this._db.GetConnection())
            {
                using (var command = cn.CreateCommand())
                {
                    command.CommandText = @"select id,username,first_name,
last_name,emp_no,mail,Enable,roles ,type
from users u 
order by id
LIMIT @pageSize OFFSET @offset;
";
                    command.AddParameters(new Dictionary<string, object>()
                    {
                        { "@pageSize", pageSize },
                        { "@offset", offset },
                    });
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var e = new MiniAuthUser
                            {
                                Id = reader.GetString(0),
                                Username = reader.IsDBNull(1) ? null : reader.GetString(1),
                                First_name = reader.IsDBNull(2) ? null : reader.IsDBNull(2) ? null : reader.GetString(2),
                                Last_name = reader.IsDBNull(3) ? null : reader.IsDBNull(3) ? null : reader.GetString(3),
                                Emp_no = reader.IsDBNull(4) ? null : reader.IsDBNull(4) ? null : reader.GetString(4),
                                Mail = reader.IsDBNull(5) ? null : reader.IsDBNull(5) ? null : reader.GetString(5),
                                Enable = reader.GetInt32(6) == 1,
                                Roles = reader.IsDBNull(7) ? null : reader.GetString(7)?.Split(','),
                                Type = reader.IsDBNull(8) ? null : reader.GetString(8)
                            };
                            users.Add(e);
                        }
                    }
                }
                totalItems = cn.ExecuteScalar<long>("select count(*) from users");
            }

            await OkResult(context, new { users, totalItems }.ToJson());
        }

        private async Task GetRoles(HttpContext context)
        {
            var roles = new List<dynamic>();
            using (var cn = this._db.GetConnection())
            {
                using (var command = cn.CreateCommand())
                {
                    command.CommandText = @"select id,name,enable,type from roles r";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var endpoint = new
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Enable = reader.GetInt32(2) == 1,
                                Type = reader.IsDBNull(3)?null: reader.GetString(3)
                            };
                            roles.Add(endpoint);
                        }
                    }
                }
            }

            await OkResult(context, roles.ToJson());
        }

        private void Logout(HttpContext context)
        {
            context.Response.Cookies.Delete("X-MiniAuth-Token");
            context.Response.Redirect($"/{_options.RoutePrefix}/login.html");
        }

        private async Task Login(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var userName = root.GetProperty("username").GetString();
            var password = root.GetProperty("password").GetString();
            var remember = default(Boolean);
            if (root.TryGetProperty("remember", out var _))
                remember = root.GetProperty("remember").GetBoolean();
            if (_userManer.ValidateUser(userName, password))
            {
                var user = _userManer.GetUser(userName);
                var roles = user["roles"] as string[];
                var newToken = _jwtManager.GetToken(userName, userName, _options.ExpirationMinuteTime, roles);
                context.Response.Headers.Add("X-MiniAuth-Token", newToken);


                if (remember)
                {
                    context.Response.Cookies.Append("X-MiniAuth-Token", newToken, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(_options.ExpirationMinuteTime),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }
                else
                {
                    context.Response.Cookies.Append("X-MiniAuth-Token", newToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }

                await OkResult(context, $"{{\"X-MiniAuth-Token\":\"{newToken}\"}}").ConfigureAwait(false);

            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
        }

        private async Task SaveUser(HttpContext context)
        {
            JsonDocument bodyJson = await GetBodyJson(context);
            var root = bodyJson.RootElement;
            var id = root.GetProperty("Id").GetString();
            var roles = root.GetProperty("Roles").Deserialize<string[]>();
            var parameters = new Dictionary<string, object>()
                        {
                            { "@id", id },
                            { "@username", root.GetProperty("Username").GetString() },
                            { "@enable", root.GetProperty("Enable").GetBoolean() ? 1 : 0 },
                            { "@First_name", root.GetProperty("First_name").GetString() },
                            { "@Last_name", root.GetProperty("Last_name").GetString() },
                            { "@Mail", root.GetProperty("Mail").GetString() },
                            { "@Roles", roles==null?null:string.Join(",",roles) },
                        };
            using (var cn = this._db.GetConnection())
            {
                using (var command = cn.CreateCommand())
                {
                    if (id == null)
                    {
                        command.CommandText = @"insert into users (username,enable,Roles,First_name,Last_name,Mail) 
values (@username,@enable,@Roles,@First_name,@Last_name,@Mail)";
                        command.AddParameters(parameters);
                    }
                    else
                    {
                        command.CommandText = @"update users set username = @username,
enable=@enable , Roles=@Roles,First_name=@First_name,Last_name=@Last_name,Mail=@Mail
where id = @id";
                        command.AddParameters(parameters);

                        var newPassword = root.GetProperty("NewPassword").GetString();
                        if (!string.IsNullOrEmpty(newPassword))
                        {
                            _userManer.UpdatePassword(id, newPassword);
                        }
                    }

                    command.ExecuteNonQuery();
                }


            }
            await OkResult(context, "".ToJson(code: 200, message: ""));
        }

        private static async Task<JsonDocument> GetBodyJson(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var bodyJson = JsonDocument.Parse(body);
            return bodyJson;
        }

        private bool IsAuth(HttpContext context)
        {
            var isAuth = true;
            if (this._routeEndpoint == null)
                return isAuth;
            var message = string.Empty;
            var messageCode = default(int);
            var token = context.Request.Headers["X-MiniAuth-Token"].FirstOrDefault() ?? context.Request.Cookies["X-MiniAuth-Token"];
            var needCheckAuth = this._routeEndpoint.Enable;
            if (needCheckAuth)
            {
                if (token == null)
                {
                    isAuth = false;
                    messageCode = 401;
                    message = "Unauthorized";
                    goto End;
                }
                try
                {
                    var json = _jwtManager.DecodeToken(token);
                    var sub = JsonDocument.Parse(json).RootElement.GetProperty("sub").GetString();
                    if (sub == null)
                        throw new Exception("sub can't null");
                    var user = _userManer.GetUser(sub);
                    var roles = user["roles"] as string[];
                    if (this._routeEndpoint.Roles != null && !(this._routeEndpoint.Roles.Length == 0))
                    {
                        bool hasRole = roles.Any(value => this._routeEndpoint.Roles.Contains(value));
                        if (!hasRole)
                        {
                            isAuth = false;
                            messageCode = 401;
                            message = "Unauthorized";
                        }
#if DEBUG
                        _logger.LogInformation($"hasRole: {hasRole}");
#endif
                    }
#if DEBUG
                    _logger.LogInformation($"Endpoint.Roles {JsonConvert.SerializeObject(new { _routeEndpoint.Roles })}");
                    _logger.LogInformation($"JWT User {JsonConvert.SerializeObject(user)}");
#endif
                }
                catch (TokenNotYetValidException)
                {
                    isAuth = false;
                    messageCode = 401;
                    message = "Token is not valid yet";
                }
                catch (TokenExpiredException)
                {
                    isAuth = false;
                    messageCode = 401;
                    message = "Token is expired";
                }
                catch (SignatureVerificationException)
                {
                    isAuth = false;
                    messageCode = 401;
                    message = "Token signature is not valid";
                }
            }

        End:
            if (!isAuth)
                DeniedEndpoint(context, new ResponseVo { code = messageCode, message = message });
#if DEBUG
            _logger.LogInformation($"messageCode: {messageCode} message: {message}");
#endif
            return isAuth;
        }
        private RoleEndpointEntity GetEndpoint(HttpContext context)
        {

            if (_isMiniAuthPath)
            {
                if (_endpointCache.ContainsKey(context.Request.Path.Value.ToLowerInvariant()))
                    return _endpointCache[context.Request.Path.Value.ToLowerInvariant()];
                else
                    return null;
            }
            var ctxEndpoint = context.GetEndpoint();
            if (ctxEndpoint != null)
            {
                return _endpointCache[ctxEndpoint.DisplayName];
            }
            return null;
        }

        private void DeniedEndpoint(HttpContext context, ResponseVo messageInfo, int status = StatusCodes.Status401Unauthorized)
        {
            if (_routeEndpoint == null)
            {
                JsonResponse(context, messageInfo, status);
                return;
            }
            if (_routeEndpoint.RedirectToLoginPage)
            {
                context.Response.Redirect($"/{_options.RoutePrefix}/login.html?returnUrl=" + context.Request.Path);
                return;
            }
            if (_isMiniAuthPath)
            {
                JsonResponse(context, messageInfo, status);
                return;
            }
            JsonResponse(context, messageInfo, status);
        }

        private static void JsonResponse(HttpContext context, ResponseVo messageInfo, int status)
        {
            var message = messageInfo != null ? JsonConvert.SerializeObject(messageInfo) : "Unauthorized";
            if (status == StatusCodes.Status401Unauthorized)
            {
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(message);
                context.Response.WriteAsync(message);
            }
        }

        private static async Task OkResult(HttpContext context, string result, string contentType = "application/json")
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = contentType;
            context.Response.ContentLength = result != null ? Encoding.UTF8.GetByteCount(result) : 0;
            await context.Response.WriteAsync(result).ConfigureAwait(false);
        }
        private static async Task NotOkResult(HttpContext context, string result, string contentType = "application/json")
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = contentType;
            context.Response.ContentLength = result != null ? Encoding.UTF8.GetByteCount(result) : 0;
            await context.Response.WriteAsync(result).ConfigureAwait(false);
        }
    }
}

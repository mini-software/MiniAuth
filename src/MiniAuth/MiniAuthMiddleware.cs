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
using MiniAuth.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MiniAuth.Managers.RolePermissionManager;

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
        private readonly IRolePermissionManager _permissionManager;
        private readonly ILogger<MiniAuthMiddleware> _logger;
        private readonly ConcurrentDictionary<string, RolePermissionEntity> _routePermissionCache = new ConcurrentDictionary<string, RolePermissionEntity>();
        private RolePermissionEntity routePermission;

        public MiniAuthMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment hostingEnv,
            ILogger<MiniAuthMiddleware> logger,
            IJWTManager jwtManager = null,
            MiniAuthOptions options = null,
            IRolePermissionManager permissionManager = null,
            IUserManager userManager = null,
            IEnumerable<EndpointDataSource> endpointSources = null,
            IMiniAuthDB db = null
        )
        {
            this._logger = logger;
            this._next = next;
            if (db == null)
                this._db = new MiniAuthDB<SQLiteConnection>("Data Source=miniauth.db;Version=3;");
            if (jwtManager == null)
                _jwtManager = new JWTManager("miniauth", "miniauth", "miniauth.pfx");
            if (options == null)
                _options = new MiniAuthOptions();
            if (userManager == null)
                _userManer = new UserManager(this._db);
            if (permissionManager == null)
                _permissionManager = new RolePermissionManager(this._db);
            this._endpointSources = endpointSources;
            this._staticFileMiddleware = CreateStaticFileMiddleware(next, loggerFactory, hostingEnv); ;
            // first time load route cache
            _routePermissionCache = new ConcurrentDictionary<string, RolePermissionEntity>(_permissionManager.GetPermissions().ToDictionary(p => p.Route.ToLowerInvariant()));
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
            _ = context ?? throw new ArgumentNullException(nameof(context));
            routePermission = null;
            if (_routePermissionCache.ContainsKey(context.Request.Path.Value.ToLowerInvariant()))
                routePermission = _routePermissionCache[context.Request.Path.Value.ToLowerInvariant()];
            var isMiniAuthPath = context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}");
            var endpoint = context.GetEndpoint();
            if(routePermission==null && endpoint == null && !isMiniAuthPath) // if routePermission is null, it's not a controled route
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.Equals($"/{_options.RoutePrefix}/login.html"))
            {
                if (context.Request.Method == "GET")
                {
                    await _staticFileMiddleware.Invoke(context);
                    return;
                }
            }
            if (context.Request.Path.Equals($"/{_options.RoutePrefix}/login"))
            {
                if (context.Request.Method == "POST")
                {
                    var reader = new StreamReader(context.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var bodyJson = JsonDocument.Parse(body);
                    var root = bodyJson.RootElement;
                    var userName = root.GetProperty("username").GetString();
                    var password = root.GetProperty("password").GetString();
                    if (_userManer.ValidateUser(userName, password))
                    {
                        var roles = _userManer.GetUserRoles(userName);
                        var newToken = _jwtManager.GetToken(userName, userName, _options.ExpirationMinuteTime, roles);
                        context.Response.Headers.Add("X-MiniAuth-Token", newToken);
                        context.Response.Cookies.Append("X-MiniAuth-Token", newToken);

                        await ResponseWriteAsync(context, $"{{\"X-MiniAuth-Token\":\"{newToken}\"}}").ConfigureAwait(false);
                        return;
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                }
            }

            if (context.Request.Path.Equals($"/{_options.RoutePrefix}/logout", StringComparison.OrdinalIgnoreCase))
            {
                if (context.Request.Method == "GET")
                {
                    context.Response.Cookies.Delete("X-MiniAuth-Token");
                    context.Response.Redirect($"/{_options.RoutePrefix}/login.html");
                    return;
                }
            }

            // js or css doesn't need auth
            {
                
                if (context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}", out PathString subPath))
                {
                    if (subPath.Value.EndsWith("js") || subPath.Value.EndsWith("css"))
                    {
                        await _staticFileMiddleware.Invoke(context);
                        return;
                    }
                }
            }



            var token = context.Request.Headers["X-MiniAuth-Token"].FirstOrDefault() ?? context.Request.Cookies["X-MiniAuth-Token"];

            // check route auth
            {
                //RolePermissionEntity routePermission = null;

                var checkRouteAuth = false;
                if (_options.AuthAllRoutes)
                {
                    if (routePermission != null && routePermission.Enable == 0)
                        checkRouteAuth = false;
                    else
                        checkRouteAuth = true;
                }
                else
                {
                    if (routePermission != null && routePermission.Enable == 1)
                        checkRouteAuth = true;
                    else
                        checkRouteAuth = false;
                }

                if (checkRouteAuth)
                {
                    if (token == null)
                    {
                        DeniedPermission(context, new ResponseVo { code = 401, message = "Unauthorized" });
                        return;
                    }
                    try
                    {
                        var json = _jwtManager.DecodeToken(token);
                        var sub = JsonDocument.Parse(json).RootElement.GetProperty("sub").GetString();
                        if (sub == null)
                            throw new Exception("sub can't null");
                        var usersPermission = _userManer.GetUserRoleAndPermissions(sub);

                        // if user doesn't have permission to access this route
                        //if it's null, permission is basic
                        if (routePermission == null)
                        {
                            // only admin role can access /miniauth now
                            // TODO: dynamic route permission feature
                            if (isMiniAuthPath)
                            {
                                if (!usersPermission.Any(_ => _.RoleId == 1))
                                {
                                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                    await ResponseWriteAsync(context, "insufficient rights to a resource");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (routePermission.Enable == 1 && usersPermission.Any(_ => _.PermissionId == routePermission.Id))
                            {
                                // pass
                            }
                            else
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                await ResponseWriteAsync(context, "insufficient rights to a resource");
                                return;
                            }
                        }
                    }
                    catch (TokenNotYetValidException)
                    {
                        _logger.LogDebug("Token is not valid yet");
                        DeniedPermission(context, new ResponseVo { code = 401, message = "Token is not valid yet" });
                        return;
                    }
                    catch (TokenExpiredException)
                    {
                        _logger.LogDebug("Token is expired");
                        DeniedPermission(context, new ResponseVo { code = 401, message = "Token is expired" });
                        return;
                    }
                    catch (SignatureVerificationException)
                    {
                        _logger.LogDebug("Token signature is not valid");
                        DeniedPermission(context, new ResponseVo { code = 400, message = "Token signature is not valid" });
                        return;
                    }


                    if (context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}", out PathString subPath))
                    {
                        if (subPath.StartsWithSegments("/api/getAllEnPoints"))
                        {
                            await GetAllEnPointsApi(context);
                            return;
                        }
                        if (subPath.Value.ToLowerInvariant().EndsWith(".html"))
                        {
                            await _staticFileMiddleware.Invoke(context);
                            return;
                        }
                    }
                }
            }

            await _next(context);
            return;
        }
        private void DeniedPermission(HttpContext context, ResponseVo messageInfo, int status = StatusCodes.Status401Unauthorized)
        {
            if(routePermission == null)
                context.Response.Redirect($"/{_options.RoutePrefix}/login.html?returnUrl=" + context.Request.Path);

            if (routePermission.IsAjax)
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
            else
            {
                context.Response.Redirect($"/{_options.RoutePrefix}/login.html?returnUrl=" + context.Request.Path);
            }
        }

        private async Task GetAllEnPointsApi(HttpContext context)
        {
            var urlList = new List<Dictionary<string, object>>();
            foreach (var item in _endpointSources.SelectMany(source => source.Endpoints))
            {
                var routeEndpoint = item as RouteEndpoint;
                if (routeEndpoint == null)
                    continue;
                var methods = item.Metadata?.GetMetadata<HttpMethodMetadata>()
                    ?.HttpMethods;
                var route = routeEndpoint?.RoutePattern.RawText;

                urlList.Add(new Dictionary<string, object>{
                    { "methods",methods}, { "route",route}, { "status","On"}, { "type","system"}
                });
            }
            await ResponseWriteAsync(context, urlList.ToJson());
        }

        private async Task RespondWithLoginHtml(HttpResponse response)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "text/html";
            using (var stream = _options.LoginHtmlStream())
            {
                var htmlBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }
        private static async Task ResponseWriteAsync(HttpContext context, string result, string contentType = "application/json")
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = contentType;
            context.Response.ContentLength = result != null ? Encoding.UTF8.GetByteCount(result) : 0;
            await context.Response.WriteAsync(result).ConfigureAwait(false);
        }
    }
}

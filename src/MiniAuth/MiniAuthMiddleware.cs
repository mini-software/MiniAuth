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
        private readonly ConcurrentDictionary<string, RoleEndpointEntity> _routeEndpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>();
        private RoleEndpointEntity routeEndpoint;

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
            if (endpointManager == null)
                _endpointManager = new RoleEndpointManager(this._db);
            this._endpointSources = endpointSources;
            this._staticFileMiddleware = CreateStaticFileMiddleware(next, loggerFactory, hostingEnv); ;
            
            _routeEndpointCache = new ConcurrentDictionary<string, RoleEndpointEntity>(_endpointManager.GetEndpoints().ToDictionary(p => p.Route.ToLowerInvariant()));
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
            routeEndpoint = null;
            if (_routeEndpointCache.ContainsKey(context.Request.Path.Value.ToLowerInvariant()))
                routeEndpoint = _routeEndpointCache[context.Request.Path.Value.ToLowerInvariant()];
            var isMiniAuthPath = context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}");
            var endpoint = context.GetEndpoint();
            if(routeEndpoint==null && endpoint == null && !isMiniAuthPath) // if routeEndpoint is null, it's not a controled route
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
                //RoleEndpointEntity routeEndpoint = null;

                var checkRouteAuth = false;
                if (_options.AuthAllRoutes)
                {
                    if (routeEndpoint != null && routeEndpoint.Enable == 0)
                        checkRouteAuth = false;
                    else
                        checkRouteAuth = true;
                }
                else
                {
                    if (routeEndpoint != null && routeEndpoint.Enable == 1)
                        checkRouteAuth = true;
                    else
                        checkRouteAuth = false;
                }

                if (checkRouteAuth)
                {
                    if (token == null)
                    {
                        DeniedEndpoint(context, new ResponseVo { code = 401, message = "Unauthorized" });
                        return;
                    }
                    try
                    {
                        var json = _jwtManager.DecodeToken(token);
                        var sub = JsonDocument.Parse(json).RootElement.GetProperty("sub").GetString();
                        if (sub == null)
                            throw new Exception("sub can't null");
                        var usersEndpoint = _userManer.GetUserRoleAndEndpoints(sub);

                        // if user doesn't have endpoint to access this route
                        //if it's null, endpoint is basic
                        if (routeEndpoint == null)
                        {
                            // only admin role can access /miniauth now
                            // TODO: dynamic route endpoint feature
                            if (isMiniAuthPath)
                            {
                                if (!usersEndpoint.Any(_ => _.RoleId == 1))
                                {
                                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                    await ResponseWriteAsync(context, "insufficient rights to a resource");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (routeEndpoint.Enable == 1 && usersEndpoint.Any(_ => _.EndpointId == routeEndpoint.Id))
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
                        DeniedEndpoint(context, new ResponseVo { code = 401, message = "Token is not valid yet" });
                        return;
                    }
                    catch (TokenExpiredException)
                    {
                        _logger.LogDebug("Token is expired");
                        DeniedEndpoint(context, new ResponseVo { code = 401, message = "Token is expired" });
                        return;
                    }
                    catch (SignatureVerificationException)
                    {
                        _logger.LogDebug("Token signature is not valid");
                        DeniedEndpoint(context, new ResponseVo { code = 400, message = "Token signature is not valid" });
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
        private void DeniedEndpoint(HttpContext context, ResponseVo messageInfo, int status = StatusCodes.Status401Unauthorized)
        {
            if(routeEndpoint == null)
            {
                context.Response.Redirect($"/{_options.RoutePrefix}/login.html?returnUrl=" + context.Request.Path);
                return;
            }

            if (routeEndpoint.IsAjax)
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
                //get endpoint namespace class name and method name
                var routeEndpoint = item as RouteEndpoint;
                if (routeEndpoint == null)
                    continue;
                var methods = item.Metadata?.GetMetadata<HttpMethodMetadata>()
                    ?.HttpMethods;
                var route = routeEndpoint?.RoutePattern.RawText;
                var isAjax = item.Metadata?.GetMetadata<Microsoft.AspNetCore.Mvc.ApiControllerAttribute>()!=null;

                urlList.Add(new Dictionary<string, object>{
                    { "id",item.DisplayName},{ "isAjax",isAjax},
                    { "methods",methods}, { "route",route}, { "status","On"}, { "type","system"}
                });
            }
            await ResponseWriteAsync(context, urlList.ToJson());
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

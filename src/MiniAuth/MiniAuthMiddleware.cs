using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace MiniAuth
{
    public class MiniAuthMiddleware
    {
        private const string EmbeddedFileNamespace = "MiniAuth.wwwroot";
        private readonly RequestDelegate _next;
        private readonly IEnumerable<EndpointDataSource> _endpointSources;
        private readonly StaticFileMiddleware _staticFileMiddleware;
        private readonly MiniAuthOptions _options;
        private readonly IJWTManager _jwtManager;
        private readonly IAccountManager _accountManer;
        public MiniAuthMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment hostingEnv,
            IJWTManager jwtManager = null,
            MiniAuthOptions options = null,
            IAccountManager accountManager =null,
            IEnumerable<EndpointDataSource> endpointSources = null
        )
        {
            this._next = next;
            this._endpointSources = endpointSources;
            if (jwtManager == null)
                _jwtManager = new JWTManager("miniauth", "miniauth", "miniauth.pfx");
            if (options == null)
                _options = new MiniAuthOptions();
            if (accountManager == null)
                _accountManer = new AccountManager("miniauth.db");
            this._staticFileMiddleware = CreateStaticFileMiddleware(next, loggerFactory, hostingEnv); ;
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
            var endPoint = context.GetEndpoint();
            var authorizeAttribute = new Object();//= endPoint?.Metadata.GetMetadata<AuthorizeAttribute>();
            if (context.Request.Path.Equals($"/{_options.RoutePrefix}/login"))
            {
                if (context.Request.Method == "GET")
                {
                    await RespondWithLoginHtml(context.Response);
                    return;
                }
                if (context.Request.Method == "POST")
                {
                    var reader = new StreamReader(context.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var json = JsonDocument.Parse(body);
                    var root = json.RootElement;
                    var account = root.GetProperty("username").GetString();
                    var password = root.GetProperty("password").GetString();
                    if (_accountManer.ValidateAccount(account, password))
                    {
                        var token = _jwtManager.GetToken(account, account, _options.ExpirationMinuteTime);
                        context.Response.Headers.Add("X-MiniAuth-Token", token);
                        context.Response.Cookies.Append("X-MiniAuth-Token", token);
                        await ResponseWriteAsync(context, $"{{\"X-MiniAuth-Token\":\"{token}\"}}").ConfigureAwait(false);
                        return;
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                }

            }
            if (authorizeAttribute == null)
            {
                if (context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}", out PathString subPath))
                {
                    await _staticFileMiddleware.Invoke(context);
                }
                else
                {
                    await _next(context);
                }
                return;
            }
            if (authorizeAttribute != null)
            {
                // TODO: remove
                if (context.Request.Path.StartsWithSegments($"/{_options.RoutePrefix}", out PathString subPath))
                {
                    if (subPath == "/getAllEnPoints")
                    {
                        var urlList = new List<Dictionary<string,object>>();
                        foreach (var item in _endpointSources.SelectMany(source => source.Endpoints))
                        {
                            var rE = item as RouteEndpoint;
                            var methods = item?.Metadata?.GetMetadata<HttpMethodMetadata>()
                                ?.HttpMethods;
                            var route = rE?.RoutePattern.RawText;
  
                            urlList.Add(new Dictionary<string, object>{
                                { "methods",methods}, { "route",route} 
                            });
                        }
                        await ResponseWriteAsync(context, JsonConvert.SerializeObject(urlList));
                        return;
                    }
                    await _staticFileMiddleware.Invoke(context);
                    return;
                }

                var token = context.Request.Headers["X-MiniAuth-Token"].FirstOrDefault() ?? context.Request.Cookies["X-MiniAuth-Token"];
                if (token == null)
                {
                    context.Response.Redirect($"/{_options.RoutePrefix}/login?returnUrl=" + context.Request.Path);
                    return;
                }

                try
                {
                    var json = _jwtManager.DecodeToken(token);
                    //TODO:check time
                    await _next(context);
                    return;
                }
                catch (Exception)
                {

                    throw;
                }
            }

            await _next(context);
        }
        private async Task RespondWithLoginHtml(HttpResponse response)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "text/html";
            using (var stream = _options.IndexStream())
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

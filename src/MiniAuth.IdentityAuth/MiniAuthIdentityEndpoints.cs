using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniAuth.IdentityAuth.Helpers;
using MiniAuth.IdentityAuth.Models;
using System.Collections.Concurrent;
using System.Text;

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
                    await OkResult(context, _endpointCache.Values.OrderByDescending(o => o.Id).ToJson());
                })
                .RequireAuthorization("miniauth_admin");
            });
            InitEndpointsCache(builder);
            
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

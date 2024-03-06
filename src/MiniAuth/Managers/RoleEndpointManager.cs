using Microsoft.AspNetCore.Routing;
using MiniAuth.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace MiniAuth.Managers
{
    public interface IRoleEndpointManager
    {
        Task<List<RoleEndpointManager.RoleEndpointEntity>> GetEndpointsAsync(IEnumerable<Microsoft.AspNetCore.Routing.EndpointDataSource> _endpointSources);
    }

    public class RoleEndpointManager : IRoleEndpointManager
    {
        private readonly IMiniAuthDB _db;

        public RoleEndpointManager(IMiniAuthDB db)
        {
            _db = db;
        }

        public async Task<List<RoleEndpointEntity>> GetEndpointsAsync(IEnumerable<Microsoft.AspNetCore.Routing.EndpointDataSource> _endpointSources)
        {
            var endpoints = new List<RoleEndpointEntity>();
            foreach (var item in _endpointSources.SelectMany(source => source.Endpoints))
            {
                //get endpoint namespace class name and method name
                var routeEndpoint = item as RouteEndpoint;
                if (routeEndpoint == null)
                    continue;
                var methods = item.Metadata?.GetMetadata<HttpMethodMetadata>()
                    ?.HttpMethods.ToArray();
                var route = routeEndpoint?.RoutePattern.RawText;
                var isApi = item.Metadata?.GetMetadata<Microsoft.AspNetCore.Mvc.ApiControllerAttribute>() != null;
                endpoints.Add(new RoleEndpointEntity
                {
                    Id = item.DisplayName,
                    Type = "system",
                    Name = item.DisplayName,
                    Route = route,
                    Methods = methods,
                    Enable = true,
                    RoleIds = new string[] { "1" },
                    RedirectToLoginPage = !isApi
                });
            }
            var dbEndpoints = new List<RoleEndpointEntity>();
            using (var connection = _db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT id,name,route,methods,enable,RedirectToLoginPage FROM endpoints p";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var endpoint = new RoleEndpointEntity
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Route = reader.GetString(2),
                                Methods = reader.GetString(3).Split(","),
                                Enable = reader.GetInt32(4) == 1,
                                RedirectToLoginPage = reader.GetInt32(5) == 1,
                                //RoleIds = reader.GetString(5) //TOOD: get role ids
                            };
                            dbEndpoints.Add(endpoint);
                        }
                    }
                }

                // if db endpoints is empty, insert the missing endpoints to db by transaction
                if (dbEndpoints.Count == 0)
                {
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        foreach (var endpoint in endpoints)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"INSERT INTO endpoints (id,name,route,methods,enable,RedirectToLoginPage) VALUES (@id,@name,@route,@methods,@enable,@RedirectToLoginPage)";
                                command.AddParameters(new Dictionary<string, object>()
                                    {
                                        { "@id", endpoint.Id },
                                        { "@name", endpoint.Name },
                                        { "@route", endpoint.Route },
                                        { "@methods", string.Join(",", endpoint.Methods??new[]{ ""}) },
                                        { "@enable", endpoint.Enable ? 1 : 0 },
                                        { "@RedirectToLoginPage", endpoint.RedirectToLoginPage ? 1 : 0 }
                                    });
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        transaction.Commit();
                    }
                }
                else
                {
                    // check missing endpoints and insert to db
                    var missingEndpoints = endpoints.Where(e => !dbEndpoints.Any(d => d.Id == e.Id)).ToList();
                    if (missingEndpoints.Count > 0)
                    {
                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            foreach (var endpoint in missingEndpoints)
                            {
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = @"INSERT INTO endpoints (id,name,route,methods,enable,RedirectToLoginPage) VALUES (@id,@name,@route,@methods,@enable,@RedirectToLoginPage)";
                                    command.AddParameters(new Dictionary<string, object>()
                                    {
                                        { "@id", endpoint.Id },
                                        { "@name", endpoint.Name },
                                        { "@route", endpoint.Route },
                                        { "@methods", string.Join(",", endpoint.Methods??new[]{ ""}) },
                                        { "@enable", endpoint.Enable ? 1 : 0 },
                                        { "@RedirectToLoginPage", endpoint.RedirectToLoginPage ? 1 : 0 }
                                    });
                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            return endpoints;
        }

        public class RoleEndpointEntity
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Route { get; set; }
            public string[] Methods { get; set; }
            public bool Enable { get; set; }
            public string[] RoleIds { get; set; }
            public bool RedirectToLoginPage { get; set; }
        }
    }
}

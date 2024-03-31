using Microsoft.AspNetCore.Routing;
using MiniAuth.Helpers;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace MiniAuth.Managers
{
    public interface IRoleEndpointManager
    {
        Task<List<RoleEndpointManager.RoleEndpointEntity>> GetEndpointsAsync(IEnumerable<Microsoft.AspNetCore.Routing.EndpointDataSource> _endpointSources);
        Task<bool> UpdateEndpoint(RoleEndpointManager.RoleEndpointEntity endpoint);
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
            using (var connection = _db.GetConnection())
            {
                var dbEndpoints = await GetDbEndpoints(connection);
                List<RoleEndpointEntity> endpoints = GetSystemEndpoints(_endpointSources);
                // if db endpoints is empty, insert the missing endpoints to db by transaction
                if (dbEndpoints.Count == 0)
                {
                    await InsertNewEndpoint(connection, endpoints);
                    return endpoints;
                }
                else
                {
                    // check missing endpoints and insert to db
                    var missingEndpoints = endpoints.Where(e => !dbEndpoints.Any(d => d.Id == e.Id)).ToList();
                    if (missingEndpoints.Count > 0)
                    {
                        await InsertNewEndpoint(connection, missingEndpoints);
                        return await GetDbEndpoints(connection); 
                    }
                        
                    return dbEndpoints;
                }
            }
        }

        public async Task<bool> UpdateEndpoint(RoleEndpointEntity endpoint)
        {
            using (var connection = _db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"update endpoints 
set enable = @enable,RedirectToLoginPage=@RedirectToLoginPage,roles=@roles
where id = @id";
                    command.AddParameters(new Dictionary<string, object>()
                    {
                        { "@id", endpoint.Id },
                        { "@route", endpoint.Route },
                        { "@methods", string.Join(",", endpoint.Methods??new[]{ ""}) },
                        { "@enable", endpoint.Enable ? 1 : 0 },
                        { "@RedirectToLoginPage", endpoint.RedirectToLoginPage ? 1 : 0 },
                        { "@roles", string.Join(",",endpoint.Roles) },
                    });
                    await command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }
        private static async Task InsertNewEndpoint(System.Data.Common.DbConnection connection, List<RoleEndpointEntity> endpoints)
        {
            using (var transaction = await connection.BeginTransactionAsync())
            {
                foreach (var endpoint in endpoints)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"INSERT INTO endpoints (id,type,name,route,methods,enable,RedirectToLoginPage,roles) 
VALUES (@id,@type,@name,@route,@methods,@enable,@RedirectToLoginPage,@roles)";
                        var ps = new Dictionary<string, object>
                        {
                            { "@id", endpoint.Id },
                            { "@type", endpoint.Type },
                            { "@name", endpoint.Name },
                            { "@route", endpoint.Route },
                            { "@methods", string.Join(",", endpoint.Methods??new[]{ ""}) },
                            { "@enable", endpoint.Enable ? 1 : 0 },
                            { "@RedirectToLoginPage", endpoint.RedirectToLoginPage ? 1 : 0 },
                            { "@roles", endpoint.Roles==null?null:string.Join(",",endpoint.Roles) },
                        };
                        command.AddParameters(ps);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                transaction.Commit();
            }
        }

        private static List<RoleEndpointEntity> GetSystemEndpoints(IEnumerable<EndpointDataSource> _endpointSources)
        {
            var endpoints = new List<RoleEndpointEntity>();
            foreach (var item in _endpointSources.SelectMany(source => source.Endpoints))
            {
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
                    RedirectToLoginPage = !isApi
                });
            }
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/getallenpoints",
                Type = "miniauth",
                Name = "/miniauth/api/getallenpoints",
                Route = "/miniauth/api/getallenpoints",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/getroles",
                Type = "miniauth",
                Name = "/miniauth/api/getroles",
                Route = "/miniauth/api/getroles",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/getusers",
                Type = "miniauth",
                Name = "/miniauth/api/getusers",
                Route = "/miniauth/api/getusers",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/resetpassword",
                Type = "miniauth",
                Name = "/miniauth/api/resetpassword",
                Route = "/miniauth/api/resetpassword",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/saveuser",
                Type = "miniauth",
                Name = "/miniauth/api/saveuser",
                Route = "/miniauth/api/saveuser",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/deleteuser",
                Type = "miniauth",
                Name = "/miniauth/api/deleteuser",
                Route = "/miniauth/api/deleteuser",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/deleterole",
                Type = "miniauth",
                Name = "/miniauth/api/deleterole",
                Route = "/miniauth/api/deleterole",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/saverole",
                Type = "miniauth",
                Name = "/miniauth/api/saverole",
                Route = "/miniauth/api/saverole",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/api/saveendpoint",
                Type = "miniauth",
                Name = "/miniauth/api/saveendpoint",
                Route = "/miniauth/api/saveendpoint",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = false
            });
            endpoints.Add(new RoleEndpointEntity
            {
                Id = "/miniauth/index.html",
                Type = "miniauth",
                Name = "/miniauth/index.html",
                Route = "/miniauth/index.html",
                Roles = new[] { "13414618672271360" },
                Enable = true,
                RedirectToLoginPage = true
            });

            return endpoints;
        }

        private static async Task<List<RoleEndpointEntity>> GetDbEndpoints(DbConnection connection)
        {
            var dbEndpoints = new List<RoleEndpointEntity>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT id,name,route,methods,enable,RedirectToLoginPage,type,roles
FROM endpoints p";
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
                            Type = reader.GetString(6),
                            Roles = reader.IsDBNull(7)?null:reader.GetString(7)?.Split(',')
                        };
                        dbEndpoints.Add(endpoint);
                    }
                }
            }
            return dbEndpoints;
        }

        public class RoleEndpointEntity
        {
            public string Id { get; set; }
            public string Type { get; set; } = "system";
            public string Name { get; set; }
            public string Route { get; set; }
            public string[] Methods { get; set; }
            public bool Enable { get; set; }
            public string[] Roles { get; set; }
            public bool RedirectToLoginPage { get; set; }
        }
    }
}

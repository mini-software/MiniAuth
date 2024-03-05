using System.Collections.Generic;
namespace MiniAuth.Managers
{
    public interface IRoleEndpointManager
    {
        List<RoleEndpointManager.RoleEndpointEntity> GetEndpoints();
    }

    public class RoleEndpointManager : IRoleEndpointManager
    {
        private readonly IMiniAuthDB _db;

        public RoleEndpointManager(IMiniAuthDB db)
        {
            _db = db;
        }

        public List<RoleEndpointEntity> GetEndpoints()
        {
            var endpoints = new List<RoleEndpointEntity>();
            using (var connection = _db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"  
                    SELECT p.id,p.name,p.route,p.enable,p.isajax, rp.role_id  
                    FROM endpoints p  
                    LEFT JOIN role_endpoints rp ON rp.endpoint_id = p.id  ";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var endpoint = new RoleEndpointEntity
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Route = reader.GetString(2),
                                Enable = reader.GetInt32(3),
                                IsAjax = reader.GetBoolean(4),
                                RoleId = reader.GetInt32(5)
                            };
                            endpoints.Add(endpoint);
                        }
                    }
                }
            }
            return endpoints;
        }

        public class RoleEndpointEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Route { get; set; }
            public int Enable { get; set; }
            public int RoleId { get; set; }
            public bool IsAjax { get; set; }
        }
    }
}

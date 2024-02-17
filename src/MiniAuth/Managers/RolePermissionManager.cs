using System.Collections.Generic;
namespace MiniAuth.Managers
{
    public interface IRolePermissionManager
    {
        List<RolePermissionManager.PermissionDto> GetPermissions();
    }

    public class RolePermissionManager : IRolePermissionManager
    {
        private readonly IMiniAuthDB _db;

        public RolePermissionManager(IMiniAuthDB db)
        {
            _db = db;
        }

        public List<PermissionDto> GetPermissions()
        {
            var permissions = new List<PermissionDto>();
            using (var connection = _db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"  
                    SELECT p.*, rp.role_id  
                    FROM permissions p  
                    LEFT JOIN role_permissions rp ON rp.permission_id = p.id  ";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var permission = new PermissionDto
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Route = reader.GetString(2),
                                Enable = reader.GetInt32(3),
                                RoleId = reader.GetInt32(4)
                            };
                            permissions.Add(permission);
                        }
                    }
                }
            }
            return permissions;
        }

        public class PermissionDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Route { get; set; }
            public int Enable { get; set; }
            public int RoleId { get; set; }
        }
    }
}

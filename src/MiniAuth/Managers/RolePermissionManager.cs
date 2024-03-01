using System.Collections.Generic;
namespace MiniAuth.Managers
{
    public interface IRolePermissionManager
    {
        List<RolePermissionManager.RolePermissionEntity> GetPermissions();
    }

    public class RolePermissionManager : IRolePermissionManager
    {
        private readonly IMiniAuthDB _db;

        public RolePermissionManager(IMiniAuthDB db)
        {
            _db = db;
        }

        public List<RolePermissionEntity> GetPermissions()
        {
            var permissions = new List<RolePermissionEntity>();
            using (var connection = _db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"  
                    SELECT p.id,p.name,p.route,p.enable,p.isajax, rp.role_id  
                    FROM permissions p  
                    LEFT JOIN role_permissions rp ON rp.permission_id = p.id  ";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var permission = new RolePermissionEntity
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Route = reader.GetString(2),
                                Enable = reader.GetInt32(3),
                                IsAjax = reader.GetBoolean(4),
                                RoleId = reader.GetInt32(5)
                            };
                            permissions.Add(permission);
                        }
                    }
                }
            }
            return permissions;
        }

        public class RolePermissionEntity
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

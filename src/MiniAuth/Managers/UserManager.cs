using System;
using System.Collections.Generic;
using MiniAuth.Helpers;
namespace MiniAuth.Managers
{
    public interface IUserManager
    {
        void CreateUser(string username, string password, string roles);
        List<UserManager.UserPermissionDto> GetUserRoleAndPermissions(string username);
        List<string> GetUserRoles(string username);
        void UpdatePassword(string username, string newPassword);
        bool ValidateUser(string username, string password);
    }

    public class UserManager : IUserManager
    {
        private readonly IMiniAuthDB _db;

        public UserManager(IMiniAuthDB db)
        {
            _db = db;
        }

        public void CreateUser(string username, string password, string roles)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "INSERT INTO Users (Username, Password) VALUES (@username, @password);";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                {
                    { "@username", username },
                    { "@password", HashGenerator.GetHashPassword(password) },
                    { "@roles", roles },
                });
                command.ExecuteNonQuery();
            }
        }
        public class UserPermissionDto
        {
            public string RoleName { get; set; }
            public int? RoleId { get; set; }
            public int? PermissionId { get; set; }
            public string Route { get; set; }
            public string PermissionName { get; set; }
        }
        public List<string> GetUserRoles(string username)
        {
            var result = new List<string>(); ;
            using (var connection = this._db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"  
                    SELECT r.name AS role_name 
                    FROM users u  
                    LEFT JOIN users_roles ur ON u.id = ur.user_id  
                    LEFT JOIN roles r ON ur.role_id = r.id   
                    WHERE u.username = @username";

                    command.AddParameters(new Dictionary<string, object>() { { "@username", username } });

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader["role_name"].ToString());
                        }
                    }
                }
                return result;
            }
        }
        public List<UserPermissionDto> GetUserRoleAndPermissions(string username)
        {
            var userPermissions = new List<UserPermissionDto>(); ;
            using (var connection = this._db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"  
                    SELECT r.name AS role_name, r.id AS role_id, p.id AS permission_id, p.route, p.name AS permission_name  
                    FROM users u  
                    LEFT JOIN users_roles ur ON u.id = ur.user_id  
                    LEFT JOIN roles r ON ur.role_id = r.id  
                    LEFT JOIN role_permissions rp ON rp.role_id = r.id  
                    LEFT JOIN permissions p ON rp.permission_id = p.id  
                    WHERE u.username = @username";

                    command.AddParameters(new Dictionary<string, object>() { { "@username", username } });

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //check role_name is DBNull




                            var userPermission = new UserPermissionDto
                            {
                                RoleName = reader["role_name"] == DBNull.Value ? null : reader["role_name"].ToString(),
                                RoleId = reader["role_id"]==DBNull.Value?null: Convert.ToInt32(reader["role_id"]),
                                PermissionId = reader["permission_id"] == DBNull.Value ? null : Convert.ToInt32(reader["permission_id"]),
                                Route = reader["route"] == DBNull.Value ? null : reader["route"].ToString(),
                                PermissionName = reader["permission_name"] == DBNull.Value ? null : reader["permission_name"].ToString()
                            };
                            userPermissions.Add(userPermission);
                        }
                    }
                }
                return userPermissions;
            }
        }
        public bool ValidateUser(string username, string password)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "SELECT * FROM Users WHERE Username = @username;";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                {
                    { "@username", username }
                });
                var reader = command.ExecuteReader();
                if (reader.Read())
                    return reader["Password"].ToString() == HashGenerator.GetHashPassword(password);
                return false;
            }
        }

        public void UpdatePassword(string username, string newPassword)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "UPDATE Users SET Password = @newPassword WHERE username = @username;";
                connection.ExecuteNonQuery(sql, new Dictionary<string, object>
                {
                    { "@username", username },
                    { "@newPassword", HashGenerator.GetHashPassword(newPassword) }
                });
            }
        }

    }
}

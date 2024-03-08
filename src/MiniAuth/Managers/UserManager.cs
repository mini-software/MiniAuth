using System;
using System.Collections.Generic;
using System.Data;
using MiniAuth.Helpers;
namespace MiniAuth.Managers
{
    public interface IUserManager
    {
        void CreateUser(string username, string password, string roles);
        List<string> GetUserRoleIds(string username);
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
        public class UserEndpointDto
        {
            public string RoleName { get; set; }
            public int? RoleId { get; set; }
            public string? EndpointId { get; set; }
            public string Route { get; set; }
            public string EndpointName { get; set; }
        }
        public List<string> GetUserRoleIds(string username)
        {
            var result = new List<string>(); ;
            using (var connection = this._db.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"  
                    SELECT ur.id 
                    FROM users u  
                    LEFT JOIN users_roles ur ON u.id = ur.user_id   
                    WHERE u.username = @username";

                    command.AddParameters(new Dictionary<string, object>() { { "@username", username } });

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetInt32(0).ToString());
                        }
                    }
                }
                return result;
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

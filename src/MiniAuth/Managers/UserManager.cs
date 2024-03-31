using System;
using System.Collections.Generic;
using System.Data;
using MiniAuth.Exceptions;
using MiniAuth.Helpers;
namespace MiniAuth.Managers
{
    public interface IUserManager
    {
        void CreateUser(string username, string password, string[] roles = null, string first_name = null, string last_name = null, string mail = null);
        Dictionary<string, object> GetUser(string userName);
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
        public void CreateUser(string username, string password, string[] roles = null,
            string first_name = null, string last_name = null, string mail = null)
        {

            using (var connection = _db.GetConnection())
            {
                string sql = @"insert into users (id,username,enable,Roles,First_name,Last_name,Mail,password) 
values (@id,@username,@enable,@Roles,@First_name,@Last_name,@Mail,@newpassword)";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                    {
                        { "@id", Helpers.IdHelper.NewId() },
                        { "@username", username },
                        { "@enable", true },
                        { "@Roles", roles == null ? null : string.Join(",", roles) },
                        { "@First_name", first_name },
                        { "@Last_name", last_name },
                        { "@Mail", mail },
                        { "@newpassword", HashGenerator.GetHashPassword(password) },
                    });
                command.ExecuteNonQuery();
            }

        }

        public bool ValidateUser(string username, string password)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "SELECT * FROM Users WHERE Username = @username and enable = 1;";
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

        public void UpdatePassword(string id, string newPassword)
        {
            if (newPassword.Length < 8)
                throw new MiniAuthException("Password must be at least 8 characters");
            if (newPassword.Length > 100)
                throw new MiniAuthException("Password must be less than 100 characters");
            using (var connection = _db.GetConnection())
            {
                string sql = "UPDATE Users SET Password = @newPassword WHERE id = @id;";
                connection.ExecuteNonQuery(sql, new Dictionary<string, object>
                {
                    { "@id", id },
                    { "@newPassword", HashGenerator.GetHashPassword(newPassword) }
                });
            }
        }

        public Dictionary<string, object> GetUser(string userName)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "SELECT * FROM Users WHERE Username = @username and enable = 1;";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                {
                    { "@username", userName }
                });
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Dictionary<string, object>
                    {
                        { "id", reader["id"] },
                        { "username", reader["username"] },
                        { "roles", reader["roles"]?.ToString().Split(',') }
                    };
                }
                return null;
            }
        }
    }
}

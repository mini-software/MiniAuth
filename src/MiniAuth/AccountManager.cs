using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
namespace MiniAuth
{
    public interface IAccountManager
    {
        void CreateAccount(string username, string password, string roles);
        void UpdateAccount(string username, string newPassword, string roles);
        bool ValidateAccount(string username, string password);
    }

    public class AccountManager : IAccountManager
    {
        private readonly IMiniAuthDB _db;

        public AccountManager(IMiniAuthDB db)
        {
            this._db = db;
        }

        public void CreateAccount(string username, string password, string roles)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "INSERT INTO Accounts (Username, Password) VALUES (@username, @password);";
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

        public bool ValidateAccount(string username, string password)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "SELECT Password FROM Accounts WHERE Username = @username;";
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

        public void UpdateAccount(string username, string newPassword, string roles)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "UPDATE Accounts SET Password = @newPassword WHERE username = @username;";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                {
                    { "@Username", username },
                    { "@newPassword", HashGenerator.GetHashPassword(newPassword) },
                    { "@roles", roles },
                });
                command.ExecuteNonQuery();
            }
        }

    }
}

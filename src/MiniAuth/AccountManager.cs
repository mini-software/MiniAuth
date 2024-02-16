using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
namespace MiniAuth
{
    public interface IAccountManager
    {
        void CreateAccount(string username, string password);
        void UpdateAccount(int id, string newUsername, string newPassword);
        bool ValidateAccount(string username, string password);
    }

    public class AccountManager : IAccountManager
    {
        private readonly IMiniAuthDB _db;

        public AccountManager(IMiniAuthDB db)
        {
            this._db = db;
        }

        public void CreateAccount(string username, string password)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "INSERT INTO Accounts (Username, Password) VALUES (@username, @password);";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                {
                    { "@username", username },
                    { "@password", HashGenerator.GetHashPassword(password) }
                });
                command.ExecuteNonQuery();
            }
        }

        public bool ValidateAccount(string username, string password)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "SELECT * FROM Accounts WHERE Username = @username;";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new SQLiteParameter("@username", username));
                var reader = command.ExecuteReader();
                if (reader.Read())
                    return reader["Password"].ToString() == HashGenerator.GetHashPassword(password);
                return false;
            }
        }

        public void UpdateAccount(int id, string newUsername, string newPassword)
        {
            using (var connection = _db.GetConnection())
            {
                string sql = "UPDATE Accounts SET Username = @newUsername, Password = @newPassword WHERE Id = @id;";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.AddParameters(new Dictionary<string, object>
                {
                    { "@id", id },
                    { "@newUsername", newUsername },
                    { "@newPassword", HashGenerator.GetHashPassword(newPassword) }
                });
                command.ExecuteNonQuery();
            }
        }

    }
}

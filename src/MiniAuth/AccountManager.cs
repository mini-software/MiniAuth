using System;
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
        private string connectionString;

        public AccountManager(string databasePath)
        {
            connectionString = $"Data Source={databasePath};Version=3;";
            if (!System.IO.File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"CREATE TABLE IF NOT EXISTS Accounts (
Id INTEGER PRIMARY KEY AUTOINCREMENT,
Username TEXT NOT NULL UNIQUE,
Password TEXT NOT NULL
);";
                    SQLiteCommand command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQuery();
                    CreateAccount("miniauth", "miniauth");
                }
            }
        }

        public void CreateAccount(string username, string password)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO Accounts (Username, Password) VALUES (@username, @password);";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", HashGenerator.GetHashPassword(password));
                command.ExecuteNonQuery();
            }
        }

        public bool ValidateAccount(string username, string password)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Accounts WHERE Username = @username;";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@username", username);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return reader["Password"].ToString() == HashGenerator.GetHashPassword(password);
                return false;
            }
        }

        public void UpdateAccount(int id, string newUsername, string newPassword)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE Accounts SET Username = @newUsername, Password = @newPassword WHERE Id = @id;";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@newUsername", newUsername);
                command.Parameters.AddWithValue("@newPassword", HashGenerator.GetHashPassword(newPassword));
                command.ExecuteNonQuery();
            }
        }

    }
}

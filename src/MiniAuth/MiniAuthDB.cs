using System;
using System.Data.Common;
using System.Data.SQLite;
namespace MiniAuth
{
    public interface IMiniAuthDB
    {
        DbConnection GetConnection();
    }
    public class MiniAuthDB<T> : IMiniAuthDB
        where T : DbConnection, new()
    {
        public string ConnectionString;
        public Func<DbConnection> _GetConnection;
        public DbConnection GetConnection()
        {
            return _GetConnection();
        }
        public MiniAuthDB(string connectionString)
        {
            _GetConnection = () => {
                var cn = new T();
                cn.ConnectionString = connectionString;
                if(cn.State==System.Data.ConnectionState.Closed )
                    cn.Open();
                return cn;
            };
            if (typeof(T).Name.ToUpper().Contains("SQLITE"))
            {
                if (!System.IO.File.Exists("miniauth.db"))
                {
                    SQLiteConnection.CreateFile(connectionString);
                    string sql = @"CREATE TABLE Accounts (
Id INTEGER PRIMARY KEY AUTOINCREMENT,
Username TEXT NOT NULL UNIQUE,
Password TEXT NOT NULL
);";
                    using (var connection = _GetConnection())
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        new AccountManager(this).CreateAccount("miniauth", "miniauth");
                    }

                }
            }
        }


    }
}

using MiniAuth.Helpers;
using MiniAuth.Managers;
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
                    SQLiteConnection.CreateFile("miniauth.db");
                    string sql = @"
DROP TABLE IF EXISTS users;
CREATE TABLE users (  
    id INTEGER PRIMARY KEY AUTOINCREMENT,  
    username TEXT NOT NULL UNIQUE,  
    password TEXT NOT NULL,  
    extension TEXT NULL
);
DROP TABLE IF EXISTS roles;
CREATE TABLE roles (  
    id INTEGER PRIMARY KEY AUTOINCREMENT,  
    name TEXT NOT NULL UNIQUE  
);

DROP TABLE IF EXISTS users_roles;
CREATE TABLE users_roles (  
    id INTEGER PRIMARY KEY AUTOINCREMENT,  
    user_id INTEGER,  
    role_id INTEGER,  
    FOREIGN KEY(user_id) REFERENCES users(id),  
    FOREIGN KEY(role_id) REFERENCES roles(id)  
);
DROP TABLE IF EXISTS endpoints;
CREATE TABLE endpoints (  
    id string PRIMARY KEY,
    type text not null,
    name TEXT NOT NULL,  
    route TEXT NOT NULL,
    methods TEXT,
    enable INTEGER NOT NULL,
    redirectToLoginPage INTEGER NOT NULL,
    roles TEXT NOT NULL
);
DROP TABLE IF EXISTS role_endpoints;
CREATE TABLE role_endpoints (  
    role_id INTEGER NOT NULL,  
    endpoint_id string NOT NULL,  
    PRIMARY KEY (role_id, endpoint_id)  
);

-- Insert users
INSERT INTO users (username,password) VALUES ('miniauth','');

-- Insert roles
INSERT INTO roles (name) VALUES ('admin');
INSERT INTO roles (name) VALUES ('user');

-- Assign roles to users
INSERT INTO users_roles (user_id, role_id) VALUES (1, 1); 
";
                    using (var connection = _GetConnection())
                    {
                        connection.ExecuteNonQuery(sql);
                        new UserManager(this).UpdatePassword("miniauth", "miniauth");
                    }
                }
            }
        }


    }
}

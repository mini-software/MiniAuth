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
DROP TABLE IF EXISTS permissions;
CREATE TABLE permissions (  
    id INTEGER PRIMARY KEY AUTOINCREMENT,  
    name TEXT NOT NULL UNIQUE,  
    route TEXT NOT NULL UNIQUE ,
    enable INTEGER NOT NULL DEFAULT 1
);
DROP TABLE IF EXISTS role_permissions;
CREATE TABLE role_permissions (  
    role_id INTEGER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,  
    permission_id INTEGER NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,  
    PRIMARY KEY (role_id, permission_id)  
);

-- Insert users
INSERT INTO users (username,password) VALUES ('miniauth','4d338c92-d9f3-4b66-b542-1f2931439870');

-- Insert roles
INSERT INTO roles (name) VALUES ('admin');
INSERT INTO roles (name) VALUES ('user');

-- Insert permissions
INSERT INTO permissions (name, route) VALUES ('Access Homepage', '/');
INSERT INTO permissions (name, route) VALUES ('Access Settings', '/miniauth/management');

-- Assign roles to users
INSERT INTO users_roles (user_id, role_id) VALUES (1, 1); 

-- Assign permissions to roles
INSERT INTO role_permissions (role_id, permission_id) VALUES (1, 1); 
INSERT INTO role_permissions (role_id, permission_id) VALUES (1, 2); 
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

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
            _GetConnection = () =>
            {
                var cn = new T();
                cn.ConnectionString = connectionString;
                if (cn.State == System.Data.ConnectionState.Closed)
                    cn.Open();
                return cn;
            };
            if (typeof(T).Name.ToUpper().Contains("SQLITE"))
            {
                if (!System.IO.File.Exists("miniauth.db"))
                {
                    SQLiteConnection.CreateFile("miniauth.db");
                    string sql = @"
create table users (  
    id text not null primary key,  
    username text not null unique,  
    password text not null, 
    roles text,
    enable integer default 1,
    first_name text,
    last_name text,
    mail text,
    emp_no text 
);

create table roles (  
    id text primary key,  
    name text not null unique,
    enable integer default (1) not null
);

create table endpoints (  
    id text primary key,
    type text not null,
    name text not null,  
    route text not null,
    methods text,
    enable integer default (1) not null,
    redirecttologinpage integer not null,
    roles text 
);

insert into roles (id,name) values ('141f6722-b2d2-4d2b-81a8-a889335e2acd','miniauth-admin');
insert into roles (id,name) values ('3ab21f79-fa49-498b-aa3d-e57188d3b0d2','miniauth-user');
insert into roles (id,name) values ('25347851-cdfe-4456-b525-52dc8cb95f10','miniauth-hr');
insert into roles (id,name) values ('b783d347-30cc-46b5-b2fc-d0b2b41684ad','miniauth-it');

insert into users (id,username,password,roles) values ('d8eb1139-7ee0-4dbd-b8a1-3c979543b982','miniauth','','141f6722-b2d2-4d2b-81a8-a889335e2acd');
insert into users (id,username,password,roles) values ('a564df6f-705f-4361-a346-b578d7a711a8','miniauth-hr','','25347851-cdfe-4456-b525-52dc8cb95f10');


";
                    using (var connection = _GetConnection())
                    {
                        connection.ExecuteNonQuery(sql);
                        new UserManager(this).UpdatePassword("d8eb1139-7ee0-4dbd-b8a1-3c979543b982", "miniauth");
                    }
                }
            }
        }


    }
}

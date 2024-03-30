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

insert into roles (id,name) values ('141f6722-b2d2-4d2b-81a8-a889335e2acd','miniauth-ADMIN');
insert into roles (id,name) values ('25347851-cdfe-4456-b525-52dc8cb95f10','miniauth-HR');
insert into roles (id,name) values ('b783d347-30cc-46b5-b2fc-d0b2b41684ad','miniauth-IT');
insert into roles (id,name) values ('9183c4d6-54e8-4758-b378-19b2d019c043','miniauth-RD');

insert into users (id,username,password,roles) values ('d8eb1139-7ee0-4dbd-b8a1-3c979543b982','miniauth','','141f6722-b2d2-4d2b-81a8-a889335e2acd');
insert into users (id,username,password,roles) values ('a564df6f-705f-4361-a346-b578d7a711a8','miniauth-user','',null);
insert into users (id,username,password,roles) values ('b23d96e6-7b3c-43bd-8723-7720315d4ad8','miniauth-hr','','25347851-cdfe-4456-b525-52dc8cb95f10');
insert into users (id,username,password,roles) values ('170d03a6-b698-4862-9390-59d5f67f770e','miniauth-it','','b783d347-30cc-46b5-b2fc-d0b2b41684ad,17c7084a-3e45-40c1-afb9-73e35ba71314');


";
                    using (var connection = _GetConnection())
                    {
                        connection.ExecuteNonQuery(sql);
                        new UserManager(this).UpdatePassword("d8eb1139-7ee0-4dbd-b8a1-3c979543b982", "miniauth");
                        new UserManager(this).UpdatePassword("a564df6f-705f-4361-a346-b578d7a711a8", "miniauth-user");
                        new UserManager(this).UpdatePassword("b23d96e6-7b3c-43bd-8723-7720315d4ad8", "miniauth-hr");
                        new UserManager(this).UpdatePassword("170d03a6-b698-4862-9390-59d5f67f770e", "miniauth-it");
                    }
                }
            }
        }


    }
}

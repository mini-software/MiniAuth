using MiniAuth.Exceptions;
using MiniAuth.Helpers;
using MiniAuth.Managers;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;
namespace MiniAuth
{
    public interface IMiniAuthDB
    {
        DbConnection GetConnection();
        DBType GetCurrentDBType();
    }

    // enum for sqlite or sqlserver
    public enum DBType
    {
        SQLite,
        SQLServer,
        Unknown
    }
    public class MiniAuthDB<T> : IMiniAuthDB
        where T : DbConnection, new()
    {
        public string ConnectionString;
        public Func<DbConnection> _GetConnection;

        public DBType DBType = DBType.Unknown;
        public DBType GetCurrentDBType()
        {
            return DBType;
        }

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
                DBType = DBType.SQLite;
            if (typeof(T).Name.ToUpper() == ("SQLCONNECTION"))
                DBType = DBType.SQLServer;
            if (DBType == DBType.Unknown)
                throw new MiniAuthException($"{typeof(T).Name} DB Type doesn't support.");

            {
                string initSql = string.Empty;
                if (DBType == DBType.SQLite)
                {
                    if (!System.IO.File.Exists("miniauth.db"))
                    {
                        SQLiteConnection.CreateFile("miniauth.db");
                        initSql = @"
create table users (  
    id text not null primary key,  
    username text not null unique, 
    password text not null, 
    roles text,
    enable integer default 1,
    first_name text,
    last_name text,
    mail text,
    emp_no text ,
    type text  
);

create table roles (  
    id text primary key,  
    name text not null unique,
    enable integer default (1) not null,
    type text  
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

insert into roles (id,type,name) values ('13414618672271360','miniauth','miniauth-admin');
insert into users (id,type,username,password,roles) values ('13414618672271350','miniauth','miniauth','','13414618672271360');
";
                        using (var connection = _GetConnection())
                        {
                            connection.ExecuteNonQuery(initSql);
                            var manager = new UserManager(this);
                            manager.UpdatePassword("13414618672271350", "miniauth");

#if DEBUG
                            connection.ExecuteNonQuery(@"
insert into roles (id,type,name) values ('13414618672271361',null,'HR');
insert into roles (id,type,name) values ('13414618672271362',null,'IT');
insert into roles (id,type,name) values ('13414618672271363',null,'RD');
insert into users (id,type,username,password,roles) values ('13414618672271351',null,'miniauth-user','',null);
insert into users (id,type,username,password,roles) values ('13414618672271352',null,'miniauth-hr','','13414618672271361');
insert into users (id,type,username,password,roles) values ('13414618672271353',null,'miniauth-it','','13414618672271362,13414618672271363');
");
                            manager.UpdatePassword("13414618672271351", "miniauth-user");
                            manager.UpdatePassword("13414618672271352", "miniauth-hr");
                            manager.UpdatePassword("13414618672271353", "miniauth-it");
#endif
                        }
                    }
                }

                if (DBType == DBType.SQLServer)
                {
                    try
                    {
                        using (var connection = _GetConnection())
                        {
                            var existDb = connection.ExecuteScalar<int?>("select 1 from sys.databases where name = 'miniauth' ") == 1;
                            if (!existDb)
                                throw new MiniAuthException("Please create sql server db by script first");

                            var adminPassword = connection.ExecuteScalar<string>("select password from miniauth..users where id = '13414618672271350' ");
                            if (string.IsNullOrWhiteSpace(adminPassword))
                            {
                                new UserManager(this).UpdatePassword("13414618672271350", "miniauth");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}

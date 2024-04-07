create database miniauth; /*Following your env to change sql*/

create table miniauth..users (  
    id nvarchar(20) not null primary key,  
    username nvarchar(20) not null unique, 
    password nvarchar(100) not null, 
    roles nvarchar(2000),
    enable int default 1,
    first_name nvarchar(200),
    last_name nvarchar(200),
    mail nvarchar(200),
    emp_no nvarchar(50) ,
    type nvarchar(20)  
);

create table miniauth..roles (  
    id nvarchar(20) primary key,  
    name nvarchar(200) not null unique,
    enable int default (1) not null,
    type nvarchar(20)  
);

create table miniauth..endpoints (  
    id nvarchar(400) primary key,
    type nvarchar(20) not null,
    name nvarchar(400) not null,  
    route nvarchar(400) not null,
    methods nvarchar(80),
    enable int default (1) not null,
    redirecttologinpage int not null,
    roles nvarchar(2000) 
);

insert into miniauth..roles (id,type,name) values ('13414618672271360','miniauth','miniauth-admin');
insert into miniauth..users (id,type,username,password,roles) values ('13414618672271350','miniauth','miniauth','','13414618672271360');

/*
select * from miniauth..endpoints
select * from miniauth..roles
select * from miniauth..users
*/

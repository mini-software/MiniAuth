<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/shps951023/MiniAuth"><img src="https://gitee.com/shps951023/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
</p>
</div>

---

<div align="center">
<p><strong><a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a></strong></p>
</div>


---

<div align="center">
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">赞助</a>、<a href="https://edu.51cto.com/course/32914.html">购买视频</a>、<a href="https://www.linkedin.com/in/itweihan/">Linkedin 推荐</a> 能帮助 MiniAuth 成长 </p>
</div>


---


### 简介

「一行代码」为「现有新旧项目」添加JWT账号、动态路由权限管理系统。

开箱即用，渐进式添加功能，避免需要打掉重写或是严重耦合情况。

### 特点

- 简单、拔插设计 : SPA、SSR、API、MVC、Razor Page 开箱即用，不使用注解代码即可
- 多平台 : 支持 Linux, macos
- 动态 : 运行时动态路由权限设置
- 渐进式 : 可按照业务需求配置功能
- 兼容 : 不对现有系统做侵入式修改，能搭配其他权限框架使用
- 支持多数据库

### 安装

从 [NuGet](https://www.nuget.org/packages/MiniAuth) 安装套件


### 快速开始

在 Startup 添加一行代码并运行项目即可

```csharp
app.UseMiniAuth();    
```

预设 admin 管理账号为 miniauth 密码为 miniauth (注意记得修改密码)
管理页面为 `http(s)://yourhost/miniauth/index.html`

#### 登入

api 接口 `Post /MiniAuth/login` 
传入 json body

```json
{
	"username":"username",
	"password":"password"
}
```
可以在 Headers 或是 Response Body 获取Key 为 X-MiniAuth-Token 的 JWT Token

预设同一个域会自动添加 cookie。

#### 登出

删除 Cookie X-MiniAuth-Token 系统即可登出

也可以使用 api 接口 `Get /MiniAuth/logout` 删除 cookie 跟跳转到登入页面

#### 获取当前用户数据

注意 : 从 Request 读取 JWT Token 的用户数据，而不是再一次读取DB数据

```C#
    public class YourController : Controller
    {
        public ActionResult UserInfo()
        {
            var user = this.GetMiniAuthUser();  
            //..
        }
    }
```

### 更换数据库

#### SQLite

系统预设使用 SQLite，无需做任何设定代码。

#### SQL Server 

目前支持版本 `SQL Server 2012 (版本 11.x) 及更高版本`

请先按照环境更改执行以下 script 先
```sql
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

-- hashed password will update on first run time 
insert into miniauth..roles (id,type,name) values ('13414618672271360','miniauth','miniauth-admin');
insert into miniauth..users (id,type,username,password,roles) values ('13414618672271350','miniauth','miniauth','','13414618672271360');
```

在 startup 添加注入

```csharp
builder.Services.AddSingleton<IMiniAuthDB>(
     new MiniAuthDB<System.Data.SqlClient.SqlConnection>("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=SSPI;Initial Catalog=miniauth;app=MiniAuth")
);
```





### 设定、选项

#### 预设模式

- MiniAuth 预设模式为IT Admin 集中用户管理，用户注册、密码重置等操作需要 Admin 权限账号操作。
- 第一次会将现有所有的 endpoint 加入系统权限管控，预设需要登入后才能访问。
- 新的 endpoint 系统会在重新启动时检查并添加。
- 用户密码预设以点击重置按钮得到随机密码
- 创建完用户后请记得按重置密码

#### 登入、用户验证
非 ApiController 预设登入导向 login.html 页面
ApiController 的 Controller 预设不会导向登入页面，而是返回 401 status code

#### 预设过期时间

MiniAuthOptions.ExpirationMinuteTime 预设过期时间为7天，如要修改请参考，注意单位是分钟

```C#
services.AddSingleton<MiniAuthOptions>(new MiniAuthOptions {ExpirationMinuteTime=12*24*60 });
```

#### 自定义 Login - js, css

添加 `app.UseStaticFiles()` 在UseMiniAuth之前，并新增  `wwwroot\MiniAuth\login.css`,  `wwwroot\MiniAuth\login.js`

### 安全

#### 加密、密钥
预设 JWT 的处理方式为 RS256 + X509，第一次运行时会生成新的凭证在本地 `miniauth.pfx`, `miniauthsalt.cer` 请妥善管理

### 分布式系统

- 数据库来源请换成 SQL Server、MySQL、PostgreSQL 等数据库，系统预设使用 SQLite
- 请确认每个机器上的 `miniauth.pfx`, `miniauthsalt.cer`使用同一份，否则会导致验证失败。


### 更新日志

请查看 [Release Notes](releases)
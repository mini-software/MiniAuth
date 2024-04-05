<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/shps951023/MiniAuth"><img src="https://gitee.com/shps951023/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
</p>
</div>

---

<div align="center">
<p><strong>
    <a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-Hant.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.es.md">Español</a>  
</strong></p>
</div>


---

<div align="center">
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">赞助</a>、<a href="https://edu.51cto.com/course/32914.html">购买视频</a>、<a href="https://www.linkedin.com/in/itweihan/">推荐</a> 能帮助 MiniAuth 成长 </p>
</div>


---


### QQ群(1群) : [813100564](https://qm.qq.com/cgi-bin/qm/qr?k=3OkxuL14sXhJsUimWK8wx_Hf28Wl49QE&jump_from=webapi) / QQ群(2群) : [579033769](https://jq.qq.com/?_wv=1027&k=UxTdB8pR) / QQ群(3群) : [625362917](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=ZFudsVhvZSNkHyt0ljbfTqZfMFO9AoFH&authKey=G5zGjiUNHjZ3efr7GzR43lESp3e3mYL2fczPALvEsUduZD2zWk9y%2BGXBJ0egt0%2FE&noverify=0&group_code=625362917)

###  [视频教学店铺](https://edu.51cto.com/course/32914.html)  |  [咸鱼店铺](https://m.tb.cn/h.5yxd1XY?tk=98krWpVNBzR)  |   [淘宝店铺](https://minisoftware.taobao.com/)

---


### 简介

「一行代码」为「现有新旧项目」添加JWT账号、动态路由权限管理系统。

开箱即用，渐进式添加功能，避免需要打掉重写或是严重耦合情况。

<table>
    <tr>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/fd26b9d8-f0e9-48eb-87c7-9d54beb56256" alt="Image 1"></td>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/3c7e8b76-d16b-4f35-a8a3-5a47b9540db9" alt="Image 2"></td>
    </tr>
    <tr>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/dc2069ef-96df-47d9-8ea7-5d10b337d2d5" alt="Image 3"></td>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/08d2404a-c494-43f4-9ce8-d563cd063ab5" alt="Image 4"></td>
    </tr>
</table>



### 特点

- 简单、拔插设计 : SPA、SSR、API、MVC、Razor Page 开箱即用
- 多平台 : 支持 linux, macos
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

注意 : 请将 UseMiniAuth 放在路由生成之后，否则系统无法获取路由数据作权限判断，如 :

```c#
app.UseRouting();
app.UseMiniAuth();
```



#### 登录

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

也可以使用 api 接口 `Get /MiniAuth/logout` 删除 cookie 跟跳转到登录页面

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
- 第一次会将现有所有的 endpoint 加入系统权限管控，预设需要登录后才能访问。
- 新的 endpoint 系统会在重新启动时检查并添加。
- 用户密码预设以点击重置按钮得到随机密码
- 创建完用户后请记得按重置密码

#### 登录、用户验证
非 ApiController 预设登录导向 login.html 页面
ApiController 的 Controller 预设不会导向登录页面，而是返回 401 status code

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
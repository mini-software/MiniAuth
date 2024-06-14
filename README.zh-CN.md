<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/shps951023/MiniAuth"><img src="https://gitee.com/shps951023/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/badge/.NET-%3E%3D%206.0-red.svg" alt="version"></a>
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
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">赞助</a>、<a href="https://www.linkedin.com/in/itweihan/">推荐</a> 能帮助 MiniAuth 成长 </p>
</div>



---


### QQ群(1群) : [813100564](https://qm.qq.com/cgi-bin/qm/qr?k=3OkxuL14sXhJsUimWK8wx_Hf28Wl49QE&jump_from=webapi) / QQ群(2群) : [579033769](https://jq.qq.com/?_wv=1027&k=UxTdB8pR) / QQ群(3群) : [625362917](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=ZFudsVhvZSNkHyt0ljbfTqZfMFO9AoFH&authKey=G5zGjiUNHjZ3efr7GzR43lESp3e3mYL2fczPALvEsUduZD2zWk9y%2BGXBJ0egt0%2FE&noverify=0&group_code=625362917)



---

### 简介

MiniAuth 一个轻量 ASP.NET Core Identity Web 后台管理中间插件

「一行代码」为「新、旧项目」 添加 Identity 系统跟用户、权限管理后台 Web UI

开箱即用，避免打掉重写或是严重耦合情况

<table>
    <tr>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/bd744b76-6a7d-4cc4-95fa-2400c81ada00" alt="Image 1"></td>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/f5377c42-98e9-4a12-b4df-3852bef01a3a" alt="Image 2"></td>
    </tr>
    <tr>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/af7b445a-2ebb-4ed6-9d0c-376c06a00fb5" alt="Image 3"></td>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/26007b39-7ec5-4f72-b714-4e5a8a4e124a" alt="Image 4"></td>
    </tr>
</table>




### 特点

- 兼容 : 支持 .NET identity Based on JWT, Cookie, Session 等 
- 简单 : 拔插设计，API、MVC、Razor Page 等开箱即用
- 支持多数据库 : 支持 Oracle, SQL Server, MySQL 等 EF Core
- 渐进、非侵入式 : 不影响现有数据库、项目结构
- 多平台 : 支持 Linux, macOS 环境


### 安装

从 [NuGet](https://www.nuget.org/packages/MiniAuth) 安装套件


### 快速开始

在 Startup 添加一行代码 `services.AddMiniAuth()` 并运行项目，例子: 

```csharp
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMiniAuth(); // <= ❗❗❗

            var app = builder.Build();
            app.Run();
        }
    }
```



接着访问管理网页，链接为 `http(s)://yourhost/miniauth/index.html`，预设 admin 管理账号为 `admin@mini-software.github.io` 密码为 `E7c4f679-f379-42bf-b547-684d456bc37f` (请记得修改密码)，即可管理你的 Identity 用户、角色、端点。

其他跟 asp.net core identity 使用方式一样，在需要权限管理的类别或方法上加上 `[Authorize]` 或是角色管控 `[Authorize(Roles = "角色")]`，假设没登入返回 401 状态, 没权限返回 403 状态。



注意: 如有自己的 identity auth 请看以下注意点

### 使用在现有的 identity 项目

把 AddMiniAuth autoUse 关闭，将 UseMiniAuth 并在泛型参数换上自己的 IdentityDBContext、用户、权限认证，放在自己的 Auth 之后，例子: 
```csharp
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddMiniAuth(autoUse: false); // <= ❗❗❗


            var app = builder.Build();

            app.UseMiniAuth<ApplicationDbContext, IdentityUser, IdentityRole>();  // <= ❗❗❗ 
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
```



### 使用 MiniAuth  JWT  Identity

指定 AuthenticationType 为 BearerJwt

```C#
MiniAuthOptions.AuthenticationType = MiniAuthOptions.AuthType.BearerJwt;
builder.Services.AddMiniAuth();
```

请记得自定义 JWT Security Key，如:

```C#
MiniAuthOptions.JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6ee3edbf-488e-4484-9c2c-e3ffa6dcbc09"));
builder.Services.AddMiniAuth();
```

#### 获取用户 token 方式

前端 Javascript XHR 例子

```javascript
var data = JSON.stringify({
  "username": "admin@mini-software.github.io",
  "password": "E7c4f679-f379-42bf-b547-684d456bc37f",
  "remember": false
});
var xhr = new XMLHttpRequest();
xhr.withCredentials = true;
xhr.addEventListener("readystatechange", function() {
  if(this.readyState === 4) {
    console.log(this.responseText);
  }
});
xhr.open("POST", "http://yourhost/miniauth/login");
xhr.setRequestHeader("Content-Type", "application/json");
xhr.send(data);
```

返回结果

```json
{
    "ok": true,
    "code": 200,
    "message": null,
    "data": {
        "tokenType": "Bearer",
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxZTIxOGY4My1iZjE3LTRhY2YtODhmOS1iOTQ3NjhjOWUwMGMiLCJuYW1lIjoiYWRtaW5AbWluaS1zb2Z0d2FyZS5naXRodWIuaW8iLCJyb2xlIjoibWluaWF1dGgtYWRtaW4iLCJzdWIiOiJhZG1pbkBtaW5pLXNvZnR3YXJlLmdpdGh1Yi5pbyIsIm5iZiI6MTcxODIwNDg5NSwiZXhwIjoxNzE4MjA1Nzk1LCJpYXQiOjE3MTgyMDQ4OTUsImlzcyI6Ik1pbmlBdXRoIn0._-DQ_rcbeju8_nrK2lD5we0rre04_xdDZNF6NhM0Rg0",
        "expiresIn": 900
    }
}
```

将 accessToken 保存在 localstorage 或是 cookie 内，呼叫你的 [Authorize] api 时设定 `Header Authorization : Bearer + 空格 +  accessToken`，系统会自动验证。

举例:   

```js
var xhr = new XMLHttpRequest();
xhr.withCredentials = true;
xhr.addEventListener("readystatechange", function() {
  if(this.readyState === 4) {
    console.log(this.responseText);
  }
});
xhr.open("GET", "http://yourhost:5014/your/api");
xhr.setRequestHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxZTIxOGY4My1iZjE3LTRhY2YtODhmOS1iOTQ3NjhjOWUwMGMiLCJuYW1lIjoiYWRtaW5AbWluaS1zb2Z0d2FyZS5naXRodWIuaW8iLCJyb2xlIjoibWluaWF1dGgtYWRtaW4iLCJzdWIiOiJhZG1pbkBtaW5pLXNvZnR3YXJlLmdpdGh1Yi5pbyIsIm5iZiI6MTcxODIwNDg5NSwiZXhwIjoxNzE4MjA1Nzk1LCJpYXQiOjE3MTgyMDQ4OTUsImlzcyI6Ik1pbmlBdXRoIn0._-DQ_rcbeju8_nrK2lD5we0rre04_xdDZNF6NhM0Rg0");
xhr.send();
```

#### 设定过期时间

```c#
 MiniAuthOptions.TokenExpiresIn = 30 * 60; 
```

单位为秒，预设30分钟，另外注意 .NET JWT ClockSkew JwtBearerOptions 预设要额外加上5分钟 [原因](https://stackoverflow.com/questions/43045035/jwt-token-authentication-expired-tokens-still-working-net-core-web-api)




### 设定、选项、自定义

#### 预设模式

- MiniAuth 预设模式为IT Admin 集中用户管理，用户注册、密码重置等操作需要 Admin 权限账号操作，预设 Role = miniauth-admin

#### 关闭 MiniAuth Login

如果你只想用自己的登录逻辑、页面、API，可以指定登录路径，关闭开关

```C#
// 放在 service 注册之前
MiniAuthOptions.LoginPath = "/Identity/Account/Login";
MiniAuthOptions.DisableMiniAuthLogin = true;
```





#### 更换数据库

MiniAuth 系统预设使用 SQLite，无需做任何设定代码
如果需要切换请在 `app.UseMiniAuth` 泛型指定不同的数据库型别。

#### 登录、用户验证

非 ApiController 预设登录导向 login.html 页面 (判断方式Headers["X-Requested-With"] == "XMLHttpRequest" 或是 ApiControllerAttribute)
ApiController 的 Controller 预设不会导向登录页面，而是返回 401 status code

#### 自定义前端

- 管理后台前端在 `/src/Frontend_Identity` 主体使用 Vue3 + Vite，使用 npm run build 后即可更新 miniauth 的 UI
- 登录页面不想使用 miniauth 预设， mvc可以使用 identity 自带的Scaffolded Login.cshtml ，或是更改 miniauth 前端的 login.html, js, css

### 自定路由前缀

```
MiniAuthOptions.RoutePrefix = "YourName";
```

预设为 `MiniAuth`



### 注意事项

#### 注意顺序

请将 UseMiniAuth 放在路由生成之后，否则系统无法获取路由数据作权限判断，如 :

```c#
app.UseRouting();
app.UseMiniAuth();
```

#### 注意: 请添加 Role 规则

请添加 `AddRoles<IdentityRole>()`，否则 `[Authorize(Roles = "权限")]` 不会生效

```C#
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // ❗❗❗ 
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

#### 分布式系统

- 数据库来源请换成 SQL Server、MySQL、PostgreSQL 等数据库
- 建议更换 JWT 等 auth 方式

### 更新日志

请查看 [Release Notes](releases)

### TODO
Link : [MiniAuth.Identify project
](https://github.com/orgs/mini-software/projects/7/views/1)
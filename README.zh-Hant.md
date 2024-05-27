<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/shps951023/MiniAuth"><img src="https://gitee.com/shps951023/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/badge/.NET-%3E%3D%206.0-red.svg" alt="version"></a>
</p>
</div>


---

<div align="center">
<p><strong>
    <a href="README.md">English</a> | <a href="README.zh-CN.md">簡體中文</a> | <a href="README.zh-Hant.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.es.md">Español</a>  
</strong></p>
</div>


---

<div align="center">
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">贊助</a>、<a href="https://www.linkedin.com/in/itweihan/">推薦</a> 能幫助 MiniAuth 成長 </p>
</div>


---

### 簡介

MiniAuth 一個輕量 ASP.NET Core Identity Web 後台管理插件

「一行代碼」為「新、舊專案」 添加 Identity 系統跟用戶、權限管理後台 Web UI

開箱即用，避免打掉重寫或是嚴重耦合情況

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




### 特點

- 兼容 :  Based on JWT, Cookie, Session 只要符合 .NET identity 規格都能使用。
- 簡單 : 拔插設計，API、MVC、Razor Page 等，都能開箱即用
- 多平台 : 支持 Linux, macOS 
- 支持多資料庫 : 符合 Identity  EF Core 規格的資料庫都能使用

### 安裝

從 [NuGet](https://www.nuget.org/packages/MiniAuth) 安裝套件

```
dotnet add package MiniAuth
// or
NuGet\Install-Package MiniAuth
```


### 快速開始

在 Startup 添加一行代碼 `services.AddMiniAuth()` 並運行專案，例子: 

```csharp
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMiniAuth();

            var app = builder.Build();
            app.Run();
        }
    }
```

預設 admin 管理賬號為 `admin@mini-software.github.io` 密碼為 `E7c4f679-f379-42bf-b547-684d456bc37f` (注意記得修改密碼)
管理頁面為 `http(s)://yourhost/miniauth/index.html`

注意: 如有自己的 identiy auth 請看以下注意點

### 已有自己的 identity 情況

把 AddMiniAuth autoUse 關閉，將 UseMiniAuth 並在泛型參數換上自己的 IdentityDBContext、用戶、權限認證，放在自己的 Auth 之後，例子: 
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

#### 注意順序
請將 UseMiniAuth 放在路由生成之後，否則系統無法獲取路由數據作權限判斷，如 :

```c#
app.UseRouting();
app.UseMiniAuth();
```

#### 注意: 請添加 Role 規則

請添加 `AddRoles<IdentityRole>()`，否則 `[Authorize(Roles = "權限")]` 不會生效
```C#
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // ❗❗❗ 
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

#### 關閉 MiniAuth Login
如果你只想用自己的登錄邏輯、頁面、API，可以指定登錄路徑，關閉開關
```C#
// 放在 service 註冊之前
MiniAuthOptions.LoginPath = "/Identity/Account/Login";
MiniAuthOptions.DisableMiniAuthLogin = true;
```



### 更換資料庫

MiniAuth 系統預設使用 SQLite，無需做任何設定代碼
如果需要切換請在 `app.UseMiniAuth` 泛型指定不同的資料庫型別。


### 設定、選項

#### 預設模式

- MiniAuth 預設模式為IT Admin 集中用戶管理，用戶註冊、密碼重置等操作需要 Admin 權限賬號操作，預設 Role = miniauth-admin

#### 登錄、用戶驗證

非 ApiController 預設登錄導向 login.html 頁面 (判斷方式Headers["X-Requested-With"] == "XMLHttpRequest" 或是 ApiControllerAttribute)
ApiController 的 Controller 預設不會導向登錄頁面，而是返回 401 status code


### 分布式系統

- 資料庫來源請換成 SQL Server、MySQL、PostgreSQL 等資料庫

### 修改UI

### 修改前端

- 管理後台前端在 `/src/Frontend_Identity` 主體使用 Vue3 + Vite，使用 npm run build 後即可更新 miniauth 的 UI
- 登錄頁面不想使用 miniauth 預設， mvc可以使用 identity 自帶的Scaffolded Login.cshtml ，或是更改 miniauth 前端的 login.html, js, css

### 更新日誌

請查看 [Release Notes](releases)

### TODO
Link : [MiniAuth.Identify project
](https://github.com/orgs/mini-software/projects/7/views/1)
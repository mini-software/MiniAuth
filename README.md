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
 Your <a href="https://github.com/mini-software/miniauth">Star</a>, <a href="https://miniexcel.github.io/">Donate</a>, <a href="https://www.linkedin.com/in/itweihan/">Recomm.</a> can make MiniAuth better 
</div>





### Introduction

"One-line code" adds identity management web for your new/old projects

<table>
    <tr>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/d2aec694-158d-4ebc-bd8b-0e9ae1f855ac" alt="Image 1"></td>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/2f791e25-9158-495e-9383-4fbedf9b982b" alt="Image 2"></td>
    </tr>
    <tr>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/03d72ed7-8fb9-465f-9093-24b00279eeb2" alt="Image 3"></td>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/841df179-7e5f-481a-9039-46738b20aa2e" alt="Image 4"></td>
    </tr>
</table>


### Features

- Compatibility: Based JWT, Cookie, and Session that follow the .NET Identity standard.
- Out of the box: Easy integration, MiniAuth works with APIs, MVC, Razor Pages.
- Multi-platform: Supports Linux, macOS environments.
- Multiple Database Support: Compatible with any database that follow the Identity EF Core standard.

### Installation

Install the package from [NuGet](https://www.nuget.org/packages/MiniAuth):

```
dotnet add package MiniAuth
// or
NuGet\Install-Package MiniAuth
```

### Quick Start

Add a single line of code `services.AddMiniAuth()` in Startup, then run your project. Example:

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

The default admin account is `admin@mini-software.github.io` with the password `E7c4f679-f379-42bf-b547-684d456bc37f` (remember to change the password). The admin page can be accessed at `http(s)://yourhost/miniauth/index.html`.

Note: If you already have your own identity auth, please follow the instructions below.

### Existing Identity Setup

Turn off autoUse for `AddMiniAuth`, and replace it with your own IdentityDBContext, user, and permission authentication in UseMiniAuth, placing it after your own Auth. Example:

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

#### Order Matters
Please place UseMiniAuth after routing generation; otherwise, the system won't be able to obtain routing data for permission checks, like:

```csharp
app.UseRouting();
app.UseMiniAuth();
```

#### Note: Adding Role Rules

Please add `AddRoles<IdentityRole>()`; otherwise `[Authorize(Roles = "YourRole")]` won't work.
```C#
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // ❗❗❗ 
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

### Changing Databases

MiniAuth system defaults to using SQLite without any code settings required. If you need to switch, specify a different database type in `app.UseMiniAuth`.

### Configuration and Options

#### Default Mode

- MiniAuth operates in a default mode where IT Admin centrally manages user operations like registration and password reset, requiring an Admin privilege account to perform these actions. Default Role = miniauth-admin.

#### Login and User Validation

For non-ApiController, login redirects to the login.html page (determined by Headers["X-Requested-With"] == "XMLHttpRequest" or ApiControllerAttribute).
ApiController Controllers do not redirect to the login page by default but return a 401 status code.

### Distributed Systems

- Please switch the database source to SQL Server, MySQL, PostgreSQL, etc.

### Release Notes

Refer to the [Release Notes](releases) for updates.

### TODO

Link: [MiniAuth.Identify project](https://github.com/orgs/mini-software/projects/7/views/1)
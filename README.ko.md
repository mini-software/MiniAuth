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
  너의 <a href="https://github.com/mini-software/miniauth">Star</a>, <a href="https://miniexcel.github.io/">Donate</a>, <a href="https://www.linkedin.com/in/itweihan/">Recomm.</a> 가 MiniAuth를 더 좋게 만들 수 있어!
</div>





### 소개

"한 줄 코드"는 신/구 프로젝트에 대한 신원 관리 웹을 추가합니다.

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


### 특징

- 호환성: .NET Identity 표준을 따르는 JWT, Cookie 및 Session을 기반으로 합니다.
- 즉시 사용 가능: 손쉬운 통합, MiniAuth는 API, MVC, Razer Pages와 함께 작동합니다.
- 멀티 플랫폼: 리눅스, macOS 환경을 지원합니다.
- 다중 데이터베이스 지원: Identity EF Core 표준을 따르는 모든 데이터베이스와 호환됩니다.

### 설치

패키지를 설치하세요 [NuGet](https://www.nuget.org/packages/MiniAuth):

```
dotnet add package MiniAuth
// or
NuGet\Install-Package MiniAuth
```

### 빠른 시

한줄의 코드를 추가해라 `services.AddMiniAuth()` Startup에, 그리고나서 프로젝트를 실행해 예:

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

기본 관리자 계정은 `admin@mini-software.github.io` 이며 비밀번호는 `E7c4f679-f379-42bf-b547-684d456bc37f`(비밀번호 변경 remember)입니다. 관리자 페이지는 `http(s)://yourhost/miniauth/index.html`에서 접속할 수 있습니다.

참고: 이미 본인 인증이 있는 경우 아래 지침을 따르시기 바랍니다.

### 기존 Identity Setup

`Add MiniAuth`에 대해 autoUse를 해제하고 Use MiniAuth에서 컨텍스트, 사용자 및 사용 권한 인증으로 교체하여 자신의 인증 뒤에 배치합니다. 예:
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

#### 주문 사항
Use MiniAuth를 라우팅 생성 후에 배치하십시오. 그렇지 않으면 시스템이 다음과 같은 권한 검사를 위한 라우팅 데이터를 얻을 수 없습니다:

```csharp
app.UseRouting();
app.UseMiniAuth();
```

#### 참고: 역할 규칙 추가

`AddRoles<IdentityRole>()`; 을 추가해주세요 그렇지 않으면 `[Authorize(Roles = "YourRole")]`가 작동하지 않습니다.
```C#
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // ❗❗❗ 
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

#### MiniAuth 로그인 사용 안 함
자신만의 로그인 로직, 페이지, API만 사용하고 싶다면 로그인 경로를 커스터마이징하고 스위치를 끌 수 있습니다.
```C#
// before add service
MiniAuthOptions.LoginPath = "/Identity/Account/Login";
MiniAuthOptions.DisableMiniAuthLogin = true;
```

### 데이터베이스 변경

MiniAuth 시스템은 기본적으로 코드 설정 없이 SQLite를 사용합니다. 전환이 필요한 경우 `app.Use MiniAuth`에서 다른 데이터베이스 유형을 지정합니다.

### 구성 및 옵션

#### 기본 모드

- MiniAuth는 등록 및 암호 재설정과 같은 사용자 작업을 IT 관리자가 중앙에서 관리하는 기본 모드로 작동하므로 이러한 작업을 수행하려면 관리자 권한 계정이 필요합니다. Default Role = miniauth-admin.

#### 로그인 및 사용자 유효성 검사

ApiController가 아닌 경우 로그인은 login.html 페이지(Headers["X-Requested-With"] == "XMLHtpRequest" 또는 ApiControllerAttribute로 리디렉션됩니다.
ApiController는 기본적으로 로그인 페이지로 리디렉션하지 않고 401 상태 코드를 반환합니다.

### 분산 시스템

- 데이터베이스 소스를 SQL Server, MySQL, PostgreSQL 등으로 전환해주세요.

### 사용자 정의 프론트엔드

- 관리 백엔드 프론트엔드는 `/src/Frontend_Identity`에서 Vue3 + Vite를 사용하며, `npm run build`를 사용한 후 miniauth UI를 업데이트할 수 있습니다.
- 로그인 페이지에 miniauth 기본값을 사용하지 않으려면 mvc에서 ID 스캐폴드 Login.cshtml을 사용하거나 miniauth 프론트엔드의 login.html, js, css를 변경할 수 있습니다.

### 릴리스 노트

업데이트 내용은 [Release Notes](releases)를 참고하세요.

### TODO

Link: [MiniAuth.Identify project](https://github.com/orgs/mini-software/projects/7/views/1)
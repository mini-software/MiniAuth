<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/shps951023/MiniAuth"><img src="https://gitee.com/shps951023/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
</p>
</div>

---

<div align="center">
<p><strong>
    <a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-Hant">繁體中文</a> | <a href="README.ja">日本語</a> | <a href="README.ko">한국어</a> | <a href="README.es">Español</a>  
</strong></p>
</div>


---

<div align="center">
 Your <a href="https://github.com/mini-software/miniauth">Star</a>, <a href="https://miniexcel.github.io/">Donate</a>, <a href="https://www.linkedin.com/in/itweihan/">Recommendations</a> can make MiniAuth better 
</div>




### Introduction

"One-line code" adds a JWT account and dynamic routing permission management system to "existing new or old projects."

<table>
    <tr>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/d2aec694-158d-4ebc-bd8b-0e9ae1f855ac" alt="Image 1"></td>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/fc141e95-502f-4d27-a47a-1943c815a7d0" alt="Image 2"></td>
    </tr>
    <tr>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/c24b4a70-1e5e-4d00-ac6e-cfd0685eeee9" alt="Image 3"></td>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/072d86c4-4a0f-4573-aad6-c7e6680af4f3" alt="Image 4"></td>
    </tr>
</table>

### Features

- Simple: Out of the box for SPA, SSR, API, MVC, and Razor Page.
- Multi-platform: Supports Linux and macOS.
- Dynamic: Runtime dynamic endpoint permission management .
- Incremental: Configuration based on business needs.
- Compatible: Doesn't require intrusive modifications to existing systems and can be used with other permission frameworks.
- Supports multiple databases.

### Installation

Install the package from [NuGet](https://www.nuget.org/packages/MiniAuth).

### Quick Start

Add one line of code to Startup and run the project:

```csharp
app.UseMiniAuth();
```

The default admin account "miniauth"  and  password "miniauth" `(remember to change the password)`.
The admin page:  `http(s)://yourhost/miniauth/index.html`.

Note : Please put UseMiniAuth after route creating for system get endpoint data, e.g.

```c#
app.UseRouting();
app.UseMiniAuth();
```

#### Login

Use the API endpoint `Post /MiniAuth/login` and pass the JSON body:

```json
{
    "username":"username",
    "password":"password"
}
```
You can retrieve the JWT Token with the key `X-MiniAuth-Token` from the Headers or Response Body.
By default, the same domain will automatically add token cookie.

#### Logout

Delete the `X-MiniAuth-Token` cookie to log out of the system.
You can also use the API endpoint `Get /MiniAuth/logout` to delete the cookie and redirect to the login page.

#### Get Current User Data

Note: Read JWT Token user data from the Request, not from the DB.

```C#
public class YourController : Controller
{
    public ActionResult UserInfo()
    {
    	var user = this.GetMiniAuthUser(); 
    	//...
    }
}
```

### Changing the Database

#### SQLite

SQLite is used by default, no additional configuration required.

#### SQL Server

Currently supports `SQL Server 2012 (version 11.x) and higher`.
Run the following script based on your environment:

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
In Startup, add the injection code:

```csharp
builder.Services.AddSingleton<IMiniAuthDB>(
	new MiniAuthDB<System.Data.SqlClient.SqlConnection>("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=SSPI;Initial Catalog=miniauth;app=MiniAuth")
);
```

### Settings and Options

#### Default Mode
- MiniAuth's default mode is centralized user management by IT Admin, requiring an Admin account for operations like user registration and password reset.
- Initially, all existing endpoints are added to the system's permission management
- New endpoints are automatically detected and added during system restarts.
- User passwords reset by the reset button to generate a random password.
- Remember to reset the password after creating a user.

#### Login and User Authentication
Non-ApiController defaults to redirecting to the login.html page for login.
ApiController-based controllers default to returning a 401 status code.

#### Default Expiration Time
`MiniAuthOptions.ExpirationMinuteTime` has a default expiration time of 7 days. You can change like following code (note the unit is `minutes`):

```C#
services.AddSingleton<MiniAuthOptions>(new MiniAuthOptions { ExpirationMinuteTime = 12 * 24 * 60 });
```

#### Custom Login - js, css
Add `app.UseStaticFiles()` before `UseMiniAuth` and create `wwwroot\MiniAuth\login.css` and `wwwroot\MiniAuth\login.js` for customization.

### Security
#### Encryption and Keys
The default JWT handling method is `RS256 + X509`. During the first run, new certificates (`miniauth.pfx` and `miniauthsalt.cer`) are generated locally. Please manage these securely.

### Distributed Systems
- For distributed systems, use databases like SQL Server, MySQL, or PostgreSQL instead of the default SQLite.
- Ensure that `miniauth.pfx` and `miniauthsalt.cer` are the same across all machines; 

### Release Notes
Please refer to the [Release Notes](releases) for update details.

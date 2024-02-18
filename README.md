<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/mini-software/MiniAuth"><img src="https://gitee.com/mini-software/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
</p>
</div>

---

<div align="center">
<p><strong><a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-Hant.md">繁體中文</a></strong></p>
</div>

---

<div align="center">
 Your <a href="https://github.com/mini-software/miniauth">Star</a> and <a href="https://edu.51cto.com/course/32914.html">Donate</a> can make MiniAuth better 
</div>


---


### Introduction

Login RBAC System in ONE Line of Code for your existing project


### Features

* Easy: support SPA, SSR, API, MVC, Razor Page 
* Dynamic: Runtime dynamic routing permission settings
* Progressive: Configurable functionality based on business requirements
* Compatible: Non-intrusive modifications to existing systems, able to work with other permission frameworks
* Supports multiple databases


### Installation

Install the package from [NuGet](https://www.nuget.org/packages/MiniAuth)


### Quick Start

Add the following code in Startup and run the project:

```csharp
app.UseMiniAuth();    // using MiniAuth; 
```

The default admin User is "miniauth" with the password "miniauth". You will need to change password when first login.


### Changelog

Please see [Release Notes](releases)


### Security

* The default JWT is RS256 + X509. When running for the first time, new credentials will be generated locally in `miniauth.pfx` and `miniauthsalt.cer`.

### API

#### Login

If there is no cookie environment, you can call the api endpoint`Post /MiniAuth/login`and pass the json body

```json
{  
 "username":"username",  
 "password":"password"  
}
```

You can obtain the JWT Value with the Key `X-MiniAuth-Token` in the Headers or Response Body.

#### Logout

If there is no cookie environment, you can call the api endpoint `Get /MiniAuth/login`

### Settings

#### Custom Login css and js

Add `wwwroot\MiniAuth\custom.css` or `wwwroot\MiniAuth\custom.js` and call `app.UseStaticFiles();`

### Distributed Systems

- Change the database source to MySQL, PostgreSQL, etc.
- Ensure that `miniauth.pfx` and `miniauthsalt.cer` are the same on each machine, otherwise authentication will fail.

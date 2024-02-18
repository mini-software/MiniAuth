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

"One line code" to add a login User management system to existing project.


### Features

* Support SPA, SSR, API, MVC, Razor Pages, etc.
* Lightweight, no need for complex dependency setup
* Makes non-intrusive modifications to existing systems
* Without SQL Server etc. DBMS dependency 


### Installation

Install the package from [NuGet](https://www.nuget.org/packages/MiniAuth)


### Quick Start

Add the following code in Startup and run the project:

```csharp
app.UseMiniAuth();    // using MiniAuth; //namespace
```

The default admin User is "miniauth" with the password "miniauth". You will need to change password when first login.


### Changelog

Please see [Release Notes](releases)


### Security

* The default JWT is RS256 + X509. When running for the first time, new credentials will be generated locally in `miniauth.pfx` and `miniauthsalt.cer`.


### Limitations and Warnings

* Currently does not support distributed systems.


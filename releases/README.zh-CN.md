## Release  Notes

<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://ci.appveyor.com/project/Mini-Software/MiniAuth/branch/master"><img src="https://ci.appveyor.com/api/projects/status/b2vustrwsuqx45f4/branch/master?svg=true" alt="Build status"></a>
<a href="https://gitee.com/mini-software/MiniAuth"><img src="https://gitee.com/mini-software/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
<a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/badge/.NET-%3E%3D%204.5-red.svg" alt="version"></a>
</p>
</div>

---

<div align="center">
<p><strong><a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a></strong></p>
</div>


---

<div align="center">
 Your <a href="https://github.com/Mini-Software/MiniAuth">Star</a> and <a href="https://MiniExcel.github.io">Donate</a> can make MiniAuth better 
</div>

---

### 0.1.0-0.7.0
- Feat: Support sql server #21
- Fix: endpoint roles null exception #116
- Fix: Avoid init error to shut down app #115
- New: Support Mobile Layout #114
- New: JWT payload mail, first name, last name #113
- Fix: SQLite asp.net core multiple thread mode database lock
- New: Basic requirements for password complexity, at least 8-100 digits #111
- New: miniatuh role, user, endpoint disable change #110
- New: MiniAuth API require miniauth role #108
- New: Support Remember #16
- New: Support GetCurrentUser entity #97
- New: DB ID by snowflake#107
- New : JWT Cookie security #106
- Feat : Support user pagination #80
- New: Endpoint page support roles update
- New: Add bootstrap 5.3

### 0.0.1
- [New] 支持login api and default page
- [New] 预设 JWT RS256 + X509 Certification

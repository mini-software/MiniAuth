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
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">赞助</a> 和 <a href="https://edu.51cto.com/course/32914.html">购买视频</a> 能帮助 MiniAuth 成长 </p>
</div>


---


### 简介

「一行代码」为现有项目添加登录账号管理系统，开箱即用。


### 特点

- SPA、SSR、API、MVC、Razor Page等都能轻易使用
- 轻量，无需做复杂依赖设定
- 不对现有系统做侵入式修改
- 不依赖 SQL Server 等数据库


### 安装

从 [NuGet](https://www.nuget.org/packages/MiniAuth) 安装套件


### 快速开始

在 Startup 添加以下代码并运行项目即可

```csharp
app.UseMiniAuth();    // using MiniAuth; //namespace
```

预设admin管理账号为 miniauth 密码为 miniauth，第一次登入后会要求修改密码

### 更新日志

请查看 [Release Notes](releases)

### 安全

- 预设 JWT 的处理方式为 RS256 + X509，第一次运行时会生成新的凭证在本地 `miniauth.pfx`, `miniauthsalt.cer` 请妥善管理


### 局限与警告

- 目前不支持分布式系统


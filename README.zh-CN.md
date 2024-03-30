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

「一行代码」为「现有新旧项目」添加JWT账号、动态路由权限管理系统。

开箱即用，渐进式添加功能，避免需要打掉重写或是严重耦合情况。

### 特点

- 简单 : SPA、SSR、API、MVC、Razor Page 都能使用
- 动态 : 运行时动态路由权限设置
- 渐进式 : 可按照业务需求配置功能
- 兼容 : 不对现有系统做侵入式修改，能搭配其他权限框架使用
- 支持多数据库

### 安装

从 [NuGet](https://www.nuget.org/packages/MiniAuth) 安装套件


### 快速开始

在 Startup 添加以下代码并运行项目即可

```csharp
app.UseMiniAuth();    // using MiniAuth; 
```

预设admin管理账号为 miniauth 密码为 miniauth，第一次登入后会要求修改密码

### 更新日志

请查看 [Release Notes](releases)

### 安全

- 预设 JWT 的处理方式为 RS256 + X509，第一次运行时会生成新的凭证在本地 `miniauth.pfx`, `miniauthsalt.cer` 请妥善管理

### API

#### 登入

如没有 cookie 环境，可以 call api 接口 `Post /MiniAuth/login` 
传入 json body

```json
{
	"username":"username",
	"password":"password"
}
```
可以在 Headers 或是 Response Body 获取Key 为 X-MiniAuth-Token 的 JWT Value。

#### 登出
如没有 cookie 环境，可以 call api 接口 `Get /MiniAuth/login` 



### 设定

#### 自定义 Login - js, css

添加 `app.UseStaticFiles()` 在UseMiniAuth之前，并新增  `wwwroot\MiniAuth\login.css`,  `wwwroot\MiniAuth\login.js`



### 选项

设定密钥生成账号、密码、路径





### 分布式系统

- 数据库来源请换成 MySQL、PostgreSQL 等数据库。
- 请确认每个机器上的 `miniauth.pfx`, `miniauthsalt.cer`都是同一个，否则会导致验证失败。

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

在 Startup 添加一行代码并运行项目即可

```csharp
app.UseMiniAuth();    
```

预设 admin 管理账号为 miniauth 密码为 miniauth (注意记得修改密码)
管理页面为 `http(s)://yourhost/miniauth/index.html`

### 更新日志

请查看 [Release Notes](releases)

### API

#### 登入

api 接口 `Post /MiniAuth/login` 
传入 json body

```json
{
	"username":"username",
	"password":"password"
}
```
可以在 Headers 或是 Response Body 获取Key 为 X-MiniAuth-Token 的 JWT Token

预设同一个域会自动添加 cookie。



#### 登出

删除 Cookie X-MiniAuth-Token 系统即可登出

也可以使用 api 接口 `Get /MiniAuth/logout` 删除 cookie 跟跳转到登入页面



### 工具

#### 获取当前用户数据

注意 : 从 Request 读取 JWT Token 的用户数据，而不是再一次读取DB数据

```C#
    public class YourController : Controller
    {
        public ActionResult UserInfo()
        {
            var user = this.GetMiniAuthUser();  
            //..
        }
    }
```



### 设定、选项

#### 预设过期时间

MiniAuthOptions.ExpirationMinuteTime 预设过期时间为7天，如要修改请参考，注意单位是分钟

```C#
services.AddSingleton<MiniAuthOptions>(new MiniAuthOptions {ExpirationMinuteTime=12*24*60 });
```

#### 自定义 Login - js, css

添加 `app.UseStaticFiles()` 在UseMiniAuth之前，并新增  `wwwroot\MiniAuth\login.css`,  `wwwroot\MiniAuth\login.js`

### 安全

#### 密钥
预设 JWT 的处理方式为 RS256 + X509，第一次运行时会生成新的凭证在本地 `miniauth.pfx`, `miniauthsalt.cer` 请妥善管理

### 分布式系统

- 数据库来源请换成 SQL Server、MySQL、PostgreSQL 等数据库，系统预设使用 SQLite
- 请确认每个机器上的 `miniauth.pfx`, `miniauthsalt.cer`都是同一个，否则会导致验证失败。

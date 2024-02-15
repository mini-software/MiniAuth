<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/mini-software/MiniAuth"><img src="https://gitee.com/mini-software/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
</p>
</div>

---

<div align="center">
<p><strong><a href="README.md">English</a> | <a href="README.zh-CN.md">簡體中文</a> | <a href="README.zh-Hant.md">繁體中文</a></strong></p>
</div>

---

<div align="center">
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">贊助</a> 和 <a href="https://edu.51cto.com/course/32914.html">購買視頻</a> 能幫助 MiniAuth 成長 </p>
</div>


---


### 簡介

「一行代碼」為現有項目添加登錄賬號管理系統，開箱即用。


### 特點

- SPA、SSR、API、MVC、Razor Page等都能輕易使用
- 輕量，無需做複雜依賴設定
- 不對現有系統做侵入式修改


### 安裝

從 [NuGet](https://www.nuget.org/packages/MiniAuth) 安裝套件


### 快速開始

在 Startup 添加以下代碼並運行項目即可

```csharp
app.UseMiniAuth();    // using MiniAuth; //namespace
```

預設admin管理賬號為 miniauth 密碼為 miniauth，第一次登入後會要求修改密碼

### 更新日誌

請查看 [Release Notes](releases)

### 安全

- 預設 JWT 的處理方式為 RS256 + X509，第一次運行時會生成新的憑證在本地 `miniauth.pfx`, `miniauthsalt.cer` 請妥善管理


### 侷限與警告

- 目前不支持分布式系統
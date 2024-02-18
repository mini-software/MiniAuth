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

「一行代碼」為「現有新舊項目」添加登錄賬號管理系統。

開箱即用，漸進式添加功能，避免需要打掉重寫或是嚴重耦合情況。

### 特點

- 簡單 : SPA、SSR、API、MVC、Razor Page 都能使用
- 動態 : 運行時動態路由權限設置
- 漸進式 : 可按照業務需求配置功能
- 兼容 : 不對現有系統做侵入式修改，能搭配其他權限框架使用
- 支持多數據庫

### 安裝

從 [NuGet](https://www.nuget.org/packages/MiniAuth) 安裝套件


### 快速開始

在 Startup 添加以下代碼並運行項目即可

```csharp
app.UseMiniAuth();    // using MiniAuth; 
```

預設admin管理賬號為 miniauth 密碼為 miniauth，第一次登入後會要求修改密碼

### 更新日誌

請查看 [Release Notes](releases)

### 安全

- 預設 JWT 的處理方式為 RS256 + X509，第一次運行時會生成新的憑證在本地 `miniauth.pfx`, `miniauthsalt.cer` 請妥善管理

### API

#### 登入

如沒有 cookie 環境，可以 call api 接口 `Post /MiniAuth/login` 
傳入 json body

```json
{
	"username":"username",
	"password":"password"
}
```
可以在 Headers 或是 Response Body 獲取Key 為 X-MiniAuth-Token 的 JWT Value。

#### 登出
如沒有 cookie 環境，可以 call api 接口 `Get /MiniAuth/login` 



### 設定

#### 自定義 Login css 跟 js

新增 `wwwroot\MiniAuth\custom.css` 或 `wwwroot\MiniAuth\custom.js`



### 分布式系統

- 數據庫來源請換成 MySQL、PostgreSQL 等數據庫。
- 請確認每個機器上的 `miniauth.pfx`, `miniauthsalt.cer`都是同一個，否則會導致驗證失敗。

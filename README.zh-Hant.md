<div align="center">
<p><a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/v/MiniAuth.svg" alt="NuGet"></a>  <a href="https://www.nuget.org/packages/MiniAuth"><img src="https://img.shields.io/nuget/dt/MiniAuth.svg" alt=""></a>  
<a href="https://gitee.com/shps951023/MiniAuth"><img src="https://gitee.com/shps951023/MiniAuth/badge/star.svg" alt="star"></a> <a href="https://github.com/Mini-Software/MiniAuth" rel="nofollow"><img src="https://img.shields.io/github/stars/Mini-Software/MiniAuth?logo=github" alt="GitHub stars"></a> 
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
<p> 您的 <a href="https://github.com/mini-software/miniauth">Star</a>、<a href="https://miniexcel.github.io">贊助</a>、<a href="https://www.udemy.com/course/miniexcel-tutorial/?referralCode=732E11323F1E8064F96C">購買影片</a>、<a href="https://www.linkedin.com/in/itweihan/">推薦</a> 能幫助 MiniAuth 成長 </p>
</div>

###  

---


### 簡介

「一行代碼」為「現有新舊專案」添加JWT帳號、動態路由權限管理系統。

開箱即用，漸進式添加功能，避免需要打掉重寫或是嚴重耦合情況。

<table>
    <tr>
        <td><img src="https://github.com/mini-software/MiniExcel/assets/12729184/fd26b9d8-f0e9-48eb-87c7-9d54beb56256" alt="Image 1"></td>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/74d4b089-355c-49af-a532-54796b5d2cac" alt="Image 2"></td>
    </tr>
    <tr>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/523b0b92-b896-4bc5-8984-06488bb15525" alt="Image 3"></td>
        <td><img src="https://github.com/mini-software/MiniAuth/assets/12729184/c6e49ab1-b1cf-4042-a430-edf86518175d" alt="Image 4"></td>
    </tr>
</table>



### 特點

- 簡單、拔插設計 : SPA、SSR、API、MVC、Razor Page 開箱即用
- 多平台 : 支持 linux, macos
- 動態 : 運行時動態路由權限設置
- 漸進式 : 可按照業務需求配置功能
- 兼容 : 不對現有系統做侵入式修改，能搭配其他權限框架使用
- 支持多資料庫

### 安裝

從 [NuGet](https://www.nuget.org/packages/MiniAuth) 安裝套件

### 快速開始-影片

[Video Link](https://www.youtube.com/watch?v=MBaDJVZI-ik)


### 快速開始

在 Startup 添加一行代碼並運行專案即可

```csharp
app.UseMiniAuth();    
```

預設 admin 管理帳號為 miniauth 密碼為 miniauth (注意記得修改密碼)
管理頁面為 `http(s)://yourhost/miniauth/index.html`

注意 : 請將 UseMiniAuth 放在路由生成之後，否則系統無法獲取路由數據作權限判斷，如 :

```c#
app.UseRouting();
app.UseMiniAuth();
```



#### 登入

api 接口 `Post /MiniAuth/login` 
傳入 json body

```json
{
	"username":"username",
	"password":"password"
}
```
可以在 Headers 或是 Response Body 獲取Key 為 X-MiniAuth-Token 的 JWT Token

預設同一個域會自動添加 cookie。

#### 登出

刪除 Cookie X-MiniAuth-Token 系統即可登出

也可以使用 api 接口 `Get /MiniAuth/logout` 刪除 cookie 跟跳轉到登入頁面

#### 獲取當前用戶數據

注意 : 從 Request 讀取 JWT Token 的用戶數據，而不是再一次讀取DB數據

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

### 更換資料庫

#### SQLite

系統預設使用 SQLite，無需做任何設定代碼。

#### SQL Server 

目前支持版本 `SQL Server 2012 (版本 11.x) 及更高版本`

請先按照環境更改執行以下 script 先
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

在 startup 添加注入

```csharp
builder.Services.AddSingleton<IMiniAuthDB>(
     new MiniAuthDB<System.Data.SqlClient.SqlConnection>("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=SSPI;Initial Catalog=miniauth;app=MiniAuth")
);
```





### 設定、選項

#### 預設模式

- MiniAuth 預設模式為IT Admin 集中用戶管理，用戶註冊、密碼重置等操作需要 Admin 權限帳號操作。
- 第一次會將現有所有的 endpoint 加入系統權限管控，預設需要登入後才能訪問。
- 新的 endpoint 系統會在重新啟動時檢查並添加。
- 用戶密碼預設以點擊重置按鈕得到隨機密碼
- 創建完用戶後請記得按重置密碼

#### 登入、用戶驗證
非 ApiController 預設登入導向 login.html 頁面
ApiController 的 Controller 預設不會導向登入頁面，而是返回 401 status code

#### 預設過期時間

MiniAuthOptions.ExpirationMinuteTime 預設過期時間為7天，如要修改請參考，注意單位是分鐘

```C#
services.AddSingleton<MiniAuthOptions>(new MiniAuthOptions {ExpirationMinuteTime=12*24*60 });
```

#### 自定義 Login - js, css

添加 `app.UseStaticFiles()` 在UseMiniAuth之前，並新增  `wwwroot\MiniAuth\login.css`,  `wwwroot\MiniAuth\login.js`

### 安全

#### 加密、密鑰
預設 JWT 的處理方式為 RS256 + X509，第一次運行時會生成新的憑證在本地 `miniauth.pfx`, `miniauthsalt.cer` 請妥善管理

### 分布式系統

- 資料庫來源請換成 SQL Server、MySQL、PostgreSQL 等資料庫，系統預設使用 SQLite
- 請確認每個機器上的 `miniauth.pfx`, `miniauthsalt.cer`使用同一份，否則會導致驗證失敗。


### 更新日誌

請查看 [Release Notes](releases)
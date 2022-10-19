<div align="center">
<img src="https://os.scighost.com/pica3/repo/logo.png" />
</div>

<div align="center">

# 哔咔 3

不主动更新 &nbsp; | &nbsp; 不拒绝使用 &nbsp; | &nbsp; 不负责维护

[![LICENSE](https://img.shields.io/github/license/Scighost/Pica3)](https://github.com/Scighost/Pica3/blob/main/LICENSE)
[![releases](https://img.shields.io/github/downloads/Scighost/Pica3/total)](https://github.com/Scighost/Pica3/releases)
[![releases/latest](https://img.shields.io/github/downloads/Scighost/Pica3/latest/total)](https://github.com/Scighost/Pica3/releases/latest)
[![build](https://img.shields.io/github/workflow/status/Scighost/Pica3/Build%20Dev%20Version)](https://github.com/Scighost/Pica3/actions)

</div>

**哔咔 3** 是 Windows 平台上的第三方 [哔咔漫画](http://picacgp.com/) 客户端，本项目仅用于学习和交流，不可闭源，严禁商用。

## 开始

- 仅支持 Windows 10 1809 (17763) 及以上版本
- 下载发行版并解压
- 运行 `bika3.exe`

如果你使用的是 Windows 10，安装以下内容以改善体验：

- 字体：[Segoe Fluent Icons](https://os.scighost.com/pica3/app/Segoe-Fluent-Icons.zip)
- 浏览器环境：[WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703)

不要随意删除文件夹内的任何内容，否则可能会出现无法运行的问题。  
如遇网络问题，请使用代理。

## 功能

本应用处于早期开发阶段，功能尚不完善。

- 账号
  - [x] 登录
  - [x] 注册
  - [x] 忘记密码
- 用户
  - [ ] 个人信息
  - [x] 打哔咔
  - [x] 收藏
- 漫画
  - [x] 分类
  - [x] 搜索
  - [x] 排行榜
  - [x] 推荐
  - [x] 随机
  - [ ] 评论
  - [ ] 历史记录
- 阅读
  - [x] 上下滚动
  - [ ] 左右切换
  - [x] 主题切换
  - [x] 缩放
  - [ ] 复制和保存指定图片
- 网络
  - [x] HTTP 代理
  - [x] Api 分流
- 其他
  - [ ] 黑白名单
  - [ ] 下载
  - [ ] ~~游戏（不支持）~~

## 截图

![LoginPage](https://os.scighost.com/pica3/repo/LoginPage.webp)

## 致谢

感谢以下项目：

- [AnkiKong/picacomic](https://github.com/AnkiKong/picacomic)
- [tonquer/picacg-qt](https://github.com/tonquer/picacg-qt)
- [Yoroion/Picsharp](https://github.com/Yoroion/Picsharp)

## 关于 CoreApi

[![NuGet](https://img.shields.io/nuget/v/Pica3.CoreApi)](https://www.nuget.org/packages/Pica3.CoreApi)

CoreApi is a PicACG api wrapper for dotnet.

``` cs
var client = new Pica3.CoreApi.PicaClient();
await client.LoginAsync("account", "password");
// Then do any other thing, see method comment for more information.
```

## 关于分流

分流是以 IP 代替域名的方式达到绕过 SNI 阻断的效果，PicaClient 内置了分流功能。

### 如何使用

Api 分流

``` cs
// 获取分流 IP
List<string> ipList = client.GetIpListAsync();
// 以上方法的返回值只有 IP，不包含 https:// 前缀
Uri uri = new Uri("https://172.67.7.24");
// 构造时设置分流
var client = new Pica3.CoreApi.PicaClient(proxy: null, address: uri);
// 也可以在构造后修改
client.ChangeProxyAndBaseAddress(proxy: null, address: uri);
// 后续的请求均发送至指定的 IP
```

图片分流

``` cs
// 图片链接
const string url = "...";
// 访问图片不需要鉴权，此方法发出的请求不会携带账号信息
HttpResponseMessage response = await client.GetImageResponseAsync(url);
byte[] bytes = await response.Content.ReadAsByteArrayAsync();
```

### 如何实现

分流功能实现起来非常容易，把链接中的域名替换为 IP，然后手动把域名加入到 Header 即可。不过需要注意的是部分图片链接可能会重定向到一个新地址，原链接就无法使用分流，需要用重定向后的新地址，好在重定向的规律非常明显。

下面的几个例子已满足目前的需求，可能还存在其他的规律，碰到后再做补充。

``` yaml
# 登录
url: https://picaapi.picacomic.com/auth/sign-in
--->
url: https://172.67.7.24/auth/sign-in
header:
    Host: picaapi.picacomic.com
```

``` yaml
# 漫画正文图片（无重定向）
url: https://storage1.picacomic.com/static/ed8caaeb-fe13-4363-9387-d5c6f2867cb1.jpg
--->
url: https://172.67.7.24/static/ed8caaeb-fe13-4363-9387-d5c6f2867cb1.jpg
header:
    Host: storage1.picacomic.com
```

``` yaml
# 漫画封面图片
url: https://storage1.picacomic.com/static/tobeimg/6YdZfey4tqYNaWfyZzOiRXssvxY8yZDoTXVH2BrL62Q/rs:fill:300:400:0/g:sm/aHR0cHM6Ly9zdG9yYWdlMS5waWNhY29taWMuY29tL3N0YXRpYy8wMDJhNzZmYi0wY2Q1LTQ4ODktOWQwMC01ODFhMDUyN2Q3YzkuanBn.jpg
# 重定向到不同域名
# -- https://img.picacomic.com/6YdZfey4tqYNaWfyZzOiRXssvxY8yZDoTXVH2BrL62Q/rs:fill:300:400:0/g:sm/aHR0cHM6Ly9zdG9yYWdlMS5waWNhY29taWMuY29tL3N0YXRpYy8wMDJhNzZmYi0wY2Q1LTQ4ODktOWQwMC01ODFhMDUyN2Q3YzkuanBn.jpg
--->
url: https://172.67.7.24/6YdZfey4tqYNaWfyZzOiRXssvxY8yZDoTXVH2BrL62Q/rs:fill:300:400:0/g:sm/aHR0cHM6Ly9zdG9yYWdlMS5waWNhY29taWMuY29tL3N0YXRpYy8wMDJhNzZmYi0wY2Q1LTQ4ODktOWQwMC01ODFhMDUyN2Q3YzkuanBn.jpg
header:
    Host: img.picacomic.com
```

``` yaml
# 个人头像图片
url: https://storage-b.picacomic.com/static/tobs/c86fa457-7843-456a-8572-c77adbaedc5b.jpg
# 重定向到相同域名
# -- https://storage-b.picacomic.com/static/c86fa457-7843-456a-8572-c77adbaedc5b.jpg
--->
url: https://172.67.7.24/static/c86fa457-7843-456a-8572-c77adbaedc5b.jpg
header:
    Host: storage-b.picacomic.com
```


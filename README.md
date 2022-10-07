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

![https://www.nuget.org/packages/Pica3.CoreApi](https://img.shields.io/nuget/v/Pica3.CoreApi)

CoreApi is a PicACG api wrapper for dotnet.

``` cs
var client = new Pica3.CoreApi.PicaClient();
await client.LoginAsync("account", "password");
// Then do any other thing, see method comment for more information.
```

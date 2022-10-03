using Pica3.CoreApi.Account;
using Pica3.CoreApi.App;
using Pica3.CoreApi.Comic;
using Pica3.CoreApi.Comment;
using Pica3.CoreApi.Game;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Pica3.CoreApi;


/// <summary>
/// Pica Api 请求类
/// </summary>
/// <remarks>如有账号，先登录 <see cref="LoginAsync(string, string)"/>，然后可调用其他 api；
/// <para />
/// 没有账号，先注册 <see cref="RegisterAccountAsync(RegisterAccountRequest)"/>；
/// <para />
/// 忘记密码，先获取账号安全问题 <see cref="GetAccountSecurityQuestionAsync(string)"/>，然后重置密码 <see cref="ResetPasswordAsync(string, int, string)"/>。
/// <para />
/// Api 请求失败，即非网络问题下返回值 code != 200 时，抛出异常 <see cref="PicaApiException"/>。</remarks>
public class PicaClient
{


    #region Private Property

    private const string InitUrl = "http://68.183.234.72/init";

    private const string BaseUrl = "https://picaapi.picacomic.com/";

    private const string ApiKey = "C69BAF41DA5ABD1FFEDC6D2FEA56B";

    private const string AppVersion = "2.2.1.2.3.4";

    private const string AppBuildVersion = "45";

    private const string Accept = "application/vnd.picacomic.com.v1+json";

    private const string AppPlatform = "android";

    private const string AppUuid = "defaultUuid";

    private const string UserAgent = "okhttp/3.8.1";

    private const string AppChannel = "1"; //哔咔线路

    private readonly string Nonce = Guid.NewGuid().ToString("N");

    private static readonly byte[] SignatureKey = "~d}$Q7$eIni=V)9\\RK/P.RM4;9[7|@/CA}b~OW!3?EV`:<>M7pddUBL5n|0/*Cn"u8.ToArray();

    private static string TimeStamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    private Uri baseAddress;

    private IWebProxy? proxy;

    private string authorization;

    private HttpClient _httpClient;

    private List<string> ipList;


    #endregion


    /// <summary>
    /// 图片质量，默认原图
    /// </summary>
    public ImageQuality ImageQuality { get; set; } = ImageQuality.Original;


    /// <summary>
    /// 已登录
    /// </summary>
    public bool IsLogin => !string.IsNullOrWhiteSpace(authorization);


    /// <summary>
    /// 构造一个 client
    /// </summary>
    /// <param name="proxy">HTTP 代理</param>
    /// <param name="address">分流 IP，通过 <see cref="GetIpListAsync"/> 获取，参数模板 new Uri("https://0.0.0.0")，http 也可以</param>
    public PicaClient(IWebProxy? proxy = null, Uri? address = null)
    {
        ChangeProxyAndBaseAddress(proxy, address);
    }


    /// <summary>
    /// 更改代理和分流，参数为 null 时重置相应设置
    /// </summary>
    /// <param name="proxy">HTTP 代理</param>
    /// <param name="address">分流 IP，通过 <see cref="GetIpListAsync"/> 获取，参数模板 new Uri("https://0.0.0.0")，http 也可以</param>
    public void ChangeProxyAndBaseAddress(IWebProxy? proxy = null, Uri? address = null)
    {
        if (address is null)
        {
            baseAddress = new Uri(BaseUrl);
        }
        else
        {
            baseAddress = address;
        }
        this.proxy = proxy;
        CreateHttpClient();
    }


    private void CreateHttpClient()
    {
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            Proxy = proxy,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });
        _httpClient.BaseAddress = baseAddress;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _httpClient.DefaultRequestHeaders.Add("api-key", ApiKey);
        _httpClient.DefaultRequestHeaders.Add("accept", Accept);
        _httpClient.DefaultRequestHeaders.Add("app-channel", AppChannel);
        _httpClient.DefaultRequestHeaders.Add("app-version", AppVersion);
        _httpClient.DefaultRequestHeaders.Add("app-build-version", AppBuildVersion);
        _httpClient.DefaultRequestHeaders.Add("nonce", Nonce);
        _httpClient.DefaultRequestHeaders.Add("app-platform", AppPlatform);
        _httpClient.DefaultRequestHeaders.Add("app-uuid", AppUuid);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        _httpClient.DefaultRequestHeaders.Add("Host", "picaapi.picacomic.com");
    }




    #region Common Method




    private async Task<T> CommonSendAsync<T>(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.Add("authorization", authorization);
        }
        if (request.Content?.Headers.ContentType != null)
        {
            // 惊呆了，小写不行，必须用大写
            request.Content.Headers.ContentType.CharSet = "UTF-8";
        }
        request.Headers.Add("time", TimeStamp);
        request.Headers.Add("signature", GetSignature(request));
        request.Headers.Add("image-quality", ImageQuality.ToString().ToLower());

        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        ResponseBase<T>? wrapper = null;
        try
        {
#if DEBUG
            var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Debug.WriteLine(request.RequestUri);
            Debug.WriteLine(str);

            wrapper = JsonSerializer.Deserialize<ResponseBase<T>>(str);
#else
            wrapper = await response.Content.ReadFromJsonAsync<ResponseBase<T>>().ConfigureAwait(false);
#endif  
        }
        catch (JsonException)
        {
            response.EnsureSuccessStatusCode();
        }
        if (wrapper is null)
        {
            response.EnsureSuccessStatusCode();
            throw new PicaApiException(request.RequestUri?.ToString(), "Response data is null.");
        }
        if (wrapper.Success)
        {
            return wrapper.Data;
        }
        else
        {
            throw new PicaApiException(request.RequestUri?.ToString(), wrapper.Message, wrapper.Error, wrapper.Detail);
        }
    }



    private async Task<T> CommonSendAsync<T>(HttpRequestMessage request, string objPropertyName, bool allowReturnNull = false)
    {
        var data = await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
        var obj = JsonSerializer.Deserialize<T>(data?[objPropertyName]);
        if (obj != null)
        {
            return obj;
        }
        else
        {
            if (allowReturnNull)
            {
                return obj!;
            }
            throw new PicaApiException(request.RequestUri?.ToString(), "Response data is null.");
        }
    }




    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private string GetSignature(HttpRequestMessage request)
    {
        var data = $"{request.RequestUri?.OriginalString}{TimeStamp}{Nonce}{request.Method}{ApiKey}".ToLower();
        using var hmacsha256 = new HMACSHA256(SignatureKey);
        byte[] hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLower();
    }



    #endregion




    #region App


    /// <summary>
    /// 获取分流 IP
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetIpListAsync()
    {
        if (ipList is null)
        {
            Debug.WriteLine(InitUrl);
            var node = await _httpClient.GetFromJsonAsync<JsonNode>(InitUrl).ConfigureAwait(false);
            Debug.WriteLine(node);
            if (node?["addresses"] is JsonArray array)
            {
                ipList = array.Select(x => x!.ToString()).ToList();
            }
        }
        if (ipList?.Any() ?? false)
        {
            return ipList;
        }
        else
        {
            throw new PicaApiException(InitUrl, "Cannot get ip list.");
        }
    }



    /// <summary>
    /// 应用信息
    /// </summary>
    /// <returns></returns>
    public async Task<AppInfo> GetAppInfoAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "init?platform=android");
        return await CommonSendAsync<AppInfo>(request).ConfigureAwait(false);
    }





    /// <summary>
    /// 聊天室信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<ChatRoom>> GetChatRoomsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "chat");
        return await CommonSendAsync<List<ChatRoom>>(request, "chatList").ConfigureAwait(false);
    }





    /// <summary>
    /// 主页分类
    /// </summary>
    /// <returns></returns>
    public async Task<List<HomeCategory>> GetHomeCategoriesAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "categories");
        return await CommonSendAsync<List<HomeCategory>>(request, "categories").ConfigureAwait(false);
    }




    /// <summary>
    /// 骑士榜
    /// </summary>
    /// <returns></returns>
    public async Task<List<KnightProfile>> GetRankKnightsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "comics/knight-leaderboard");
        return await CommonSendAsync<List<KnightProfile>>(request, "users").ConfigureAwait(false);
    }








    #endregion




    #region Account



    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    /// <returns>是否获取到 token</returns>
    public async Task<bool> LoginAsync(string account, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "auth/sign-in")
        {
            Content = JsonContent.Create(new { email = account, password })
        };
        var token = await CommonSendAsync<string>(request, "token", true).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token))
        {
            authorization = token;
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 注销，即删除认证
    /// </summary>
    public void Logout()
    {
        authorization = null!;
    }


    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="register"></param>
    /// <returns>无返回值</returns>
    public async Task RegisterAccountAsync(RegisterAccountRequest register)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "auth/register")
        {
            Content = JsonContent.Create(register),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }


    /// <summary>
    /// 忘记密码，获取安全问题，然后调用 <see cref="ResetPasswordAsync(string, int, string)"/>
    /// </summary>
    /// <param name="account">忘记密码的账号</param>
    /// <returns>安全问题</returns>
    public async Task<AccountSecurityQuestion> GetAccountSecurityQuestionAsync(string account)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "auth/forgot-password")
        {
            Content = JsonContent.Create(new { email = account }),
        };
        return await CommonSendAsync<AccountSecurityQuestion>(request).ConfigureAwait(false);
    }


    /// <summary>
    /// 重置密码，返回新密码
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="questionIndex">第几个安全问题，1 2 3</param>
    /// <param name="answer">安全问题的答案</param>
    /// <returns>新密码</returns>
    public async Task<string> ResetPasswordAsync(string account, int questionIndex, string answer)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "auth/reset-password")
        {
            Content = JsonContent.Create(new { email = account, questionNo = questionIndex, answer }),
        };
        return await CommonSendAsync<string>(request, "password").ConfigureAwait(false);
    }


    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>无返回值</returns>
    public async Task ChangePasswordAsync(string oldPassword, string newPassword)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "users/password")
        {
            Content = JsonContent.Create(new { old_password = oldPassword, new_password = newPassword }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }


    /// <summary>
    /// 用户信息
    /// </summary>
    /// <param name="id">为 null 时获取自己的信息</param>
    /// <returns>用户信息</returns>
    public async Task<UserProfile> GetUserProfileAsync(string? id = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"users/{(string.IsNullOrWhiteSpace(id) ? "" : id + "/")}profile");
        return await CommonSendAsync<UserProfile>(request, "user").ConfigureAwait(false);
    }


    /// <summary>
    /// 修改个性签名
    /// </summary>
    /// <param name="slogan">个性签名</param>
    /// <returns>无返回值</returns>
    public async Task ChangeSloganAsync(string slogan)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "users/profile")
        {
            Content = JsonContent.Create(new { slogan }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }


    /// <summary>
    /// 修改个人头像
    /// </summary>
    /// <param name="bytes">图片文件字节数组</param>
    /// <param name="format">文件格式，仅支持 jpg 和 png</param>
    /// <returns>无返回值</returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task ChangeUserAvatarAsync(byte[] bytes, string format)
    {
        format = format?.ToString() switch
        {
            "jpg" => "jpeg",
            "jpeg" => "jpeg",
            "png" => "png",
            _ => throw new NotSupportedException($"Format '{format}' is not supported.")
        };
        var avatar = $"data:image/{format};base64,{Convert.ToBase64String(bytes)}";
        var request = new HttpRequestMessage(HttpMethod.Put, "users/avatar")
        {
            Content = JsonContent.Create(new { avatar }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }



    /// <summary>
    /// 打哔咔
    /// </summary>
    /// <returns>已打哔咔/之前已打过哔咔</returns>
    public async Task<bool> PunchAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "users/punch-in");
        var res = await CommonSendAsync<JsonNode>(request, "res").ConfigureAwait(false);
        return res["status"]?.ToString() switch
        {
            "ok" => true,
            "fail" => false,
            _ => throw new PicaApiException("users/punch-in", "Result is invalid."),
        };
    }








    #endregion





    #region Search


    /// <summary>
    /// 分类搜索
    /// </summary>
    /// <param name="category">分类名，调用 <see cref="GetHomeCategoriesAsync"/>，然后填入 <see cref="HomeCategory.Title"/></param>
    /// <param name="page"></param>
    /// <param name="sort">排序</param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> CategorySearchAsync(string category, int page = 1, SortType sort = SortType.ua)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics?page={page}&c={Uri.EscapeDataString(category)}&s={sort}");
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics").ConfigureAwait(false);
    }


    /// <summary>
    /// 关键词搜索
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <param name="page"></param>
    /// <param name="sort"></param>
    /// <param name="categories">分类，<see cref="HomeCategory.Title"/></param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> AdvanceSearchAsync(string keyword, int page = 1, SortType sort = SortType.ua, IEnumerable<string>? categories = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comics/advanced-search?page={page}")
        {
            Content = JsonContent.Create(new { keyword, sort, categories }),
        };
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics").ConfigureAwait(false);
    }



    #endregion





    #region Comic



    /// <summary>
    /// 收藏的漫画
    /// </summary>
    /// <param name="sort">排序</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> GetFavouriteComicAsync(SortType sort = SortType.ua, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"users/favourite?s={sort}&page={page}");
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics").ConfigureAwait(false);
    }


    /// <summary>
    /// 添加或取消收藏
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns>已收藏/已取消收藏</returns>
    public async Task<bool> FavoriteComicAsync(string comicId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comics/{comicId}/favourite");
        var result = await CommonSendAsync<string>(request, "action").ConfigureAwait(false);
        return result switch
        {
            "favourite" => true,
            "un_favourite" => false,
            _ => throw new PicaApiException($"comics/{comicId}/favourite", "Result is invalid."),
        };
    }



    /// <summary>
    /// 点赞或取消点赞
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns>已点赞/已取消点赞</returns>
    public async Task<bool> LikeComicAsync(string comicId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comics/{comicId}/like");
        var result = await CommonSendAsync<string>(request, "action").ConfigureAwait(false);
        return result switch
        {
            "like" => true,
            "unlike" => false,
            _ => throw new PicaApiException($"comics/{comicId}/like", "Result is invalid."),
        };
    }



    /// <summary>
    /// 排行榜漫画
    /// </summary>
    /// <param name="type">排行榜类型，日周月</param>
    /// <returns></returns>
    public async Task<List<RankComic>> GetRankComicsAsync(RankType type)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/leaderboard?tt={type}&ct=VC");
        return await CommonSendAsync<List<RankComic>>(request, "comics").ConfigureAwait(false);
    }



    /// <summary>
    /// 漫画详情
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns></returns>
    public async Task<ComicDetail> GetComicDetailAsync(string comicId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}");
        return await CommonSendAsync<ComicDetail>(request, "comic").ConfigureAwait(false);
    }




    /// <summary>
    /// 漫画章节列表
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicEpisodeProfile>> GetComicEpisodeListAsync(string comicId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}/eps?page={page}");
        return await CommonSendAsync<PicaPageResult<ComicEpisodeProfile>>(request, "eps").ConfigureAwait(false);
    }



    /// <summary>
    /// 看了这本的人也在看
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns></returns>
    public async Task<List<ComicProfile>> GetRecommendComicsAsync(string comicId)
    {

        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}/recommendation");
        return await CommonSendAsync<List<ComicProfile>>(request, "comics").ConfigureAwait(false);
    }



    /// <summary>
    /// 漫画章节图片内容
    /// </summary>
    /// <param name="bookId">漫画 id <see cref="ComicProfile.Id"/></param>
    /// <param name="episodeOrderId">漫画章节顺序 <see cref="ComicEpisodeProfile.Order"/></param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<ComicEpisodeDetail> GetComicEpisodeImagesAsync(string bookId, int episodeOrderId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{bookId}/order/{episodeOrderId}/pages?page={page}");
        var node = await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
        var detail = JsonSerializer.Deserialize<ComicEpisodeDetail>(node);
        detail!.Order = episodeOrderId;
        detail.Id = node["ep"]?["_id"]?.ToString()!;
        detail.Title = node["ep"]?["title"]?.ToString()!;
        return detail;
    }


    /// <summary>
    /// 获取漫画评论
    /// </summary>
    /// <param name="comicId"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<CommentCollection> GetComicCommentAsync(string comicId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}/comments?page={page}");
        return await CommonSendAsync<CommentCollection>(request).ConfigureAwait(false);
    }


    /// <summary>
    /// 发表漫画评论
    /// </summary>
    /// <param name="comicId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task PublishComicCommentAsync(string comicId, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comics/{comicId}/comments")
        {
            Content = JsonContent.Create(new { content }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }



    /// <summary>
    /// 大家都在搜
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetKeywordsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "keywords");
        return await CommonSendAsync<List<string>>(request, "keywords").ConfigureAwait(false);
    }



    /// <summary>
    /// 随机本子
    /// </summary>
    /// <returns></returns>
    public async Task<List<ComicProfile>> GetRandomComicsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "comics/random");
        return await CommonSendAsync<List<ComicProfile>>(request, "comics").ConfigureAwait(false);
    }



    /// <summary>
    /// 本子推荐
    /// </summary>
    /// <returns></returns>
    public async Task<List<RecommendComic>> GetRecommendComicsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "collections");
        return await CommonSendAsync<List<RecommendComic>>(request, "collections").ConfigureAwait(false);
    }



    /// <summary>   
    /// 大家都在看
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> GetEveryoneLovesAsync(int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics?page={page}&c={Uri.EscapeDataString("大家都在看")}&s=ld");
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics").ConfigureAwait(false);
    }



    #endregion





    #region Comment



    /// <summary>
    /// 我的评论
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<MyComment>> GetMyCommentAsync(int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"users/my-comments?page={page}");
        return await CommonSendAsync<PicaPageResult<MyComment>>(request, "comments").ConfigureAwait(false);
    }



    /// <summary>
    /// 给评论点赞
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns>已点赞</returns>
    /// <exception cref="PicaApiException"></exception>
    public async Task<bool> LikeCommentAsync(string commentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comments/{commentId}/like");
        var result = await CommonSendAsync<string>(request, "action").ConfigureAwait(false);
        return result switch
        {
            "like" => true,
            "unlike" => false,
            _ => throw new PicaApiException($"comments/{commentId}/like", "Result is invalid."),
        };
    }


    /// <summary>
    /// 获取子评论
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<SubCommentItem>> GetSubCommentAsync(string commentId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comments/{commentId}/childrens?page={page}");
        return await CommonSendAsync<PicaPageResult<SubCommentItem>>(request, "comments").ConfigureAwait(false);
    }


    /// <summary>
    /// 发表子评论
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task PublishSubCommentAsync(string commentId, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comments/{commentId}")
        {
            Content = JsonContent.Create(new { content }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }



    /// <summary>
    /// 获取留言板评论
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<CommentCollection> GetSquareCommentAsync(int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/5822a6e3ad7ede654696e482/comments?page={page}");
        return await CommonSendAsync<CommentCollection>(request).ConfigureAwait(false);
    }


    /// <summary>
    /// 发表留言板评论
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task PublishSquareCommentAsync(string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comics/5822a6e3ad7ede654696e482/comments")
        {
            Content = JsonContent.Create(new { content }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }




    #endregion





    #region Game



    /// <summary>
    /// 游戏列表
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<List<GameProfile>> GetGameListAsync(int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"games?page={page}");
        return await CommonSendAsync<List<GameProfile>>(request, "games").ConfigureAwait(false);
    }




    /// <summary>
    /// 游戏详情
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<GameDetail> GetGameDetailAsync(string gameId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"games/{gameId}");
        return await CommonSendAsync<GameDetail>(request, "game").ConfigureAwait(false);
    }




    /// <summary>
    /// 游戏评论
    /// </summary>
    /// <param name="gameId">游戏 id</param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<CommentCollection>> GetGameCommentsAsync(string gameId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"games/{gameId}/comments?page={page}");
        return await CommonSendAsync<PicaPageResult<CommentCollection>>(request, "comments").ConfigureAwait(false);
    }




    /// <summary>
    /// 发布游戏评论
    /// </summary>
    /// <param name="gameId">游戏 id</param>
    /// <param name="content">评论内容</param>
    /// <returns>无返回值</returns>
    public async Task PublishGameCommentsAsync(string gameId, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"games/{gameId}/comments")
        {
            Content = JsonContent.Create(new { content }),
        };
        await CommonSendAsync<JsonNode>(request).ConfigureAwait(false);
    }




    #endregion




}
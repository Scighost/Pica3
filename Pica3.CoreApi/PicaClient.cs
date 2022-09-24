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

public class PicaClient
{


    #region Private Property


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


    private string authorization;

    private HttpClient _httpClient;


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
    /// <param name="proxy">代理</param>
    /// <remarks>如有账号，先登录 <see cref="LoginAsync(string, string)"/>，然后可调用其他 api；
    /// <para />
    /// 没有账号，先注册 <see cref="RegisterAccountAsync(RegisterAccountRequest)"/>；
    /// <para />
    /// 忘记密码，先获取账号安全问题 <see cref="GetAccountSecurityQuestionAsync(string)"/>，然后重置密码 <see cref="ResetPasswordAsync(string, int, string)"/>。
    /// <para />
    /// Api 请求失败，即非网络问题下返回值 code != 200 时，抛出异常 <see cref="PicaApiException"/>。</remarks>
    public PicaClient(IWebProxy? proxy = null)
    {
        CreateHttpClient(proxy);
    }


    /// <summary>
    /// 更改代理
    /// </summary>
    /// <param name="proxy"></param>
    public void ChangeProxy(IWebProxy? proxy = null)
    {
        CreateHttpClient(proxy);
    }


    private void CreateHttpClient(IWebProxy? proxy = null)
    {
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            Proxy = proxy,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _httpClient.DefaultRequestHeaders.Add("api-key", ApiKey);
        _httpClient.DefaultRequestHeaders.Add("accept", Accept);
        _httpClient.DefaultRequestHeaders.Add("app-channel", AppChannel);
        _httpClient.DefaultRequestHeaders.Add("app-version", AppVersion);
        _httpClient.DefaultRequestHeaders.Add("app-build-version", AppBuildVersion);
        _httpClient.DefaultRequestHeaders.Add("nonce", Nonce);
        _httpClient.DefaultRequestHeaders.Add("app-platform", AppPlatform);
        _httpClient.DefaultRequestHeaders.Add("app-uuid", AppUuid);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
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

        var response = await _httpClient.SendAsync(request);
#if DEBUG
        var str = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(request.RequestUri);
        Debug.WriteLine(str);
        var wrapper = JsonSerializer.Deserialize<ResponseBase<T>>(str);
#else
        var obj = await response.Content.ReadFromJsonAsync<ResponseBase<T>>();
#endif
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

        var response = await _httpClient.SendAsync(request);
#if DEBUG
        var str = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(request.RequestUri);
        Debug.WriteLine(str);
        var wrapper = JsonSerializer.Deserialize<ResponseBase<JsonNode>>(str);
#else
        var wrapper = await response.Content.ReadFromJsonAsync<ResponseBase<JsonNode>>();
#endif
        if (wrapper is null)
        {
            response.EnsureSuccessStatusCode();
            throw new PicaApiException(request.RequestUri?.ToString(), "Response data is null.");
        }
        if (wrapper.Success)
        {
            var obj = JsonSerializer.Deserialize<T>(wrapper.Data?[objPropertyName]);
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
        else
        {
            throw new PicaApiException(request.RequestUri?.ToString(), wrapper.Message, wrapper.Error, wrapper.Detail);
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
        using (var hmacsha256 = new HMACSHA256(SignatureKey))
        {
            byte[] hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));

            return Convert.ToHexString(hash).ToLower();
        }
    }



    #endregion




    #region App



    /// <summary>
    /// 应用信息
    /// </summary>
    /// <returns></returns>
    public async Task<AppInfo> GetAppInfoAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "init?platform=android");
        return await CommonSendAsync<AppInfo>(request);
    }





    /// <summary>
    /// 聊天室信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<ChatRoom>> GetChatRoomsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "chat");
        return await CommonSendAsync<List<ChatRoom>>(request, "chatList");
    }





    /// <summary>
    /// 主页分类
    /// </summary>
    /// <returns></returns>
    public async Task<List<HomeCategory>> GetHomeCategoriesAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "categories");
        return await CommonSendAsync<List<HomeCategory>>(request, "categories");
    }




    /// <summary>
    /// 骑士榜
    /// </summary>
    /// <returns></returns>
    public async Task<List<KnightProfile>> GetRankKnightsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "comics/knight-leaderboard");
        return await CommonSendAsync<List<KnightProfile>>(request, "users");
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
        var token = await CommonSendAsync<string>(request, "token", true);
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
        await CommonSendAsync<JsonNode>(request);
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
        return await CommonSendAsync<AccountSecurityQuestion>(request);
    }


    /// <summary>
    /// 重置密码，返回新密码
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="questionNo">第几个安全问题，1 2 3</param>
    /// <param name="answer">安全问题的答案</param>
    /// <returns>新密码</returns>
    public async Task<string> ResetPasswordAsync(string account, int questionNo, string answer)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "auth/reset-password")
        {
            Content = JsonContent.Create(new { email = account, questionNo, answer }),
        };
        return await CommonSendAsync<string>(request, "password");
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
        await CommonSendAsync<JsonNode>(request);
    }


    /// <summary>
    /// 用户信息
    /// </summary>
    /// <param name="id">为 null 时获取自己的信息</param>
    /// <returns>用户信息</returns>
    public async Task<UserProfile> GetUserProfileAsync(string? id = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"users/{(string.IsNullOrWhiteSpace(id) ? "" : id + "/")}profile");
        return await CommonSendAsync<UserProfile>(request, "user");
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
        await CommonSendAsync<JsonNode>(request);
    }



    /// <summary>
    /// 打哔咔
    /// </summary>
    /// <returns>已打哔咔/无需打哔咔</returns>
    public async Task<bool> PunchAsync()
    {
        var profile = await GetUserProfileAsync();
        if (profile.IsPunched)
        {
            return false;
        }
        var request = new HttpRequestMessage(HttpMethod.Post, "users/punch-in");
        var res = await CommonSendAsync<JsonNode>(request, "res");
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
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics");
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
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics");
    }



    #endregion





    #region Comic



    /// <summary>
    /// 收藏的漫画
    /// </summary>
    /// <param name="sort">排序</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> GetFavouriteAsync(SortType sort = SortType.ua, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"users/favourite?s={sort}&page={page}");
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics");
    }


    /// <summary>
    /// 添加或取消收藏
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns>已收藏/已取消收藏</returns>
    public async Task<bool> AddFavoriteAsync(string comicId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"comics/{comicId}/favourite");
        var result = await CommonSendAsync<string>(request, "action");
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
        var result = await CommonSendAsync<string>(request, "action");
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
        return await CommonSendAsync<List<RankComic>>(request, "comics");
    }



    /// <summary>
    /// 漫画详情
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns></returns>
    public async Task<ComicDetail> GetComicDetailAsync(string comicId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}");
        return await CommonSendAsync<ComicDetail>(request, "comic");
    }




    /// <summary>
    /// 漫画章节列表
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicEpisodeProfile>> GetComicEpisodeAsync(string comicId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}/eps?page={page}");
        return await CommonSendAsync<PicaPageResult<ComicEpisodeProfile>>(request, "eps");
    }



    /// <summary>
    /// 看了这本的人也在看
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns></returns>
    public async Task<List<ComicProfile>> GetRecommendComicsAsync(string comicId)
    {

        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{comicId}/recommendation");
        return await CommonSendAsync<List<ComicProfile>>(request, "comics");
    }



    /// <summary>
    /// 漫画章节图片内容
    /// </summary>
    /// <param name="bookId">漫画 id <see cref="ComicProfile.Id"/></param>
    /// <param name="episodeOrderId">漫画章节顺序 <see cref="ComicEpisodeProfile.Order"/></param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<ComicEpisodeDetail> GetComicEpisodePageAsync(string bookId, int episodeOrderId, int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/{bookId}/order/{episodeOrderId}/pages?page={page}");
        var node = await CommonSendAsync<JsonNode>(request);
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
        return await CommonSendAsync<CommentCollection>(request);
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
        await CommonSendAsync<JsonNode>(request);
    }



    /// <summary>
    /// 大家都在搜
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetKeywordsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "keywords");
        return await CommonSendAsync<List<string>>(request, "keywords");
    }



    /// <summary>
    /// 随机本子
    /// </summary>
    /// <returns></returns>
    public async Task<List<ComicProfile>> GetRandomComicsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "comics/random");
        return await CommonSendAsync<List<ComicProfile>>(request, "comics");
    }



    /// <summary>
    /// 本子神推荐
    /// </summary>
    /// <returns></returns>
    public async Task<List<RecommendComic>> GetRecommendComicsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "collections");
        return await CommonSendAsync<List<RecommendComic>>(request, "collections");
    }



    /// <summary>   
    /// 大家都在看
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> GetEveryoneLovesAsync(int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics?page={page}&c={Uri.EscapeDataString("大家都在看")}&s=ld");
        return await CommonSendAsync<PicaPageResult<ComicProfile>>(request, "comics");
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
        return await CommonSendAsync<PicaPageResult<MyComment>>(request, "comments");
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
        var result = await CommonSendAsync<string>(request, "action");
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
        return await CommonSendAsync<PicaPageResult<SubCommentItem>>(request, "comments");
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
        await CommonSendAsync<JsonNode>(request);
    }



    /// <summary>
    /// 获取留言板评论
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<CommentCollection> GetSquareCommentAsync(int page = 1)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"comics/5822a6e3ad7ede654696e482/comments?page={page}");
        return await CommonSendAsync<CommentCollection>(request);
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
        await CommonSendAsync<JsonNode>(request);
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
        return await CommonSendAsync<List<GameProfile>>(request, "games");
    }




    /// <summary>
    /// 游戏详情
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<GameDetail> GetGameDetailAsync(string gameId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"games/{gameId}");
        return await CommonSendAsync<GameDetail>(request, "game");
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
        return await CommonSendAsync<PicaPageResult<CommentCollection>>(request, "comments");
    }




    /// <summary>
    /// 发布游戏评论
    /// </summary>
    /// <param name="gameId">游戏 id</param>
    /// <param name="content">评论内容</param>
    /// <returns>无返回值</returns>
    public async Task GetGameCommentsAsync(string gameId, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"games/{gameId}/comments")
        {
            Content = JsonContent.Create(new { content }),
        };
        await CommonSendAsync<JsonNode>(request);
    }




    #endregion




}
using Dapper;
using Pica3.CoreApi;
using Pica3.CoreApi.Account;
using Pica3.CoreApi.App;
using Pica3.CoreApi.Comic;
using Pica3.CoreApi.Comment;
using Pica3.CoreApi.Game;
using System.Net;

namespace Pica3.Services;





public class PicaService
{


    private readonly PicaClient picaClient;

    public PicaService(PicaClient picaClient)
    {
        this.picaClient = picaClient;
    }




    /// <summary>
    /// 图片质量，默认原图
    /// </summary>
    public ImageQuality ImageQuality { get => picaClient.ImageQuality; set => picaClient.ImageQuality = value; }



    /// <summary>
    /// 已登录
    /// </summary>
    public bool IsLogin => picaClient.IsLogin;



    /// <summary>
    /// 更改代理和分流，参数为 null 时重置相应设置
    /// </summary>
    /// <param name="proxy">HTTP 代理</param>
    /// <param name="address">分流 IP，通过 <see cref="GetIpListAsync"/> 获取，参数模板 new Uri("https://0.0.0.0")，http 也可以</param>
    public void ChangeProxyAndBaseAddress(IWebProxy? proxy = null, Uri? address = null)
    {
        picaClient.ChangeProxyAndBaseAddress(proxy, address);
    }






    #region Database



    /// <summary>
    /// 获取账号信息
    /// </summary>
    /// <returns></returns>
    public static List<PicaAccount> GetAccounts()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var accounts = dapper.Query<PicaAccount>("SELECT * FROM PicaAccount;").ToList();
        List<PicaAccount> result = new(accounts.Count);
        foreach (var account in accounts)
        {
            var id = account.Account;
            try
            {
                if (!string.IsNullOrWhiteSpace(account.Account))
                {
                    account.Account = AesHelper.Decrypt(Convert.FromHexString(account.Account));
                }
                if (!string.IsNullOrWhiteSpace(account.Password))
                {
                    account.Password = AesHelper.Decrypt(Convert.FromHexString(account.Password));
                }
                result.Add(account);
            }
            catch
            {
                dapper.Execute("DELETE FROM PicaAccount WHERE Account=@Id;", new { Id = id });
            }
        }
        return result;
    }



    /// <summary>
    /// 保存账号信息
    /// </summary>
    /// <param name="account"></param>
    public static void SaveAccount(PicaAccount account)
    {
        if (!string.IsNullOrWhiteSpace(account.Account))
        {
            account.Account = Convert.ToHexString(AesHelper.Encrypt(account.Account));
        }
        if (!string.IsNullOrWhiteSpace(account.Password))
        {
            account.Password = Convert.ToHexString(AesHelper.Encrypt(account.Password));
        }
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute("UPDATE PicaAccount SET Selected=False WHERE True;");
        dapper.Execute("INSERT OR REPLACE INTO PicaAccount VALUES (@Account, @Password, @Selected, @AutoLogin);", account);
    }


    /// <summary>
    /// 保存漫画简单信息
    /// </summary>
    /// <param name="comic"></param>
    private static void SaveComicProfile(IEnumerable<ComicProfile> comics, bool isFavorite = false)
    {
        Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                using var t = dapper.BeginTransaction();
                dapper.Execute("""
                    INSERT INTO ComicDetail (Id, Title, Author, EpisodeCount, PagesCount, Finished, Cover, Categories, TotalViews, TotalLikes, LikesCount, LastUpdateTime)
                    VALUES (@Id, @Title, @Author, @EpisodeCount, @PagesCount, @Finished, @Cover, @Categories, @TotalViews, @TotalLikes, @LikesCount, DATETIME())
                    ON CONFLICT DO UPDATE SET
                    EpisodeCount=@EpisodeCount, PagesCount=@PagesCount, Finished=@Finished, TotalViews=@TotalViews, TotalLikes=@TotalLikes, LikesCount=@LikesCount, LastUpdateTime=DATETIME();
                    """, comics, t);
                if (isFavorite)
                {
                    dapper.Execute("UPDATE ComicDetail SET IsFavourite = TRUE WHERE Id IN @Ids;", new { Ids = comics.Select(x => x.Id) }, t);
                }
                dapper.Execute("INSERT OR IGNORE INTO ComicCategory (ComicId, Category) VALUES (@ComicId, @Category);", comics.SelectMany(x => x.Categories.Select(y => new { ComicId = x.Id, Category = y })), t);
                t.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
    }


    /// <summary>
    /// 保存漫画详细信息
    /// </summary>
    /// <param name="comic"></param>
    private static void SaveComicDetail(ComicDetail comic)
    {
        Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                using var t = dapper.BeginTransaction();
                dapper.Execute("""
                    INSERT OR REPLACE INTO ComicDetail (Id, Title, Author, ChineseTeam, Description, EpisodeCount, PagesCount, CreatedAt, UpdatedAt,
                                                        Finished, Cover, Creator, Categories, Tags, TotalViews, TotalLikes, LikesCount, ViewsCount,
                                                        CommentsCount, AllowDownload, AllowComment, IsLiked, IsFavourite, LastUpdateTime)
                    VALUES (@Id, @Title, @Author, @ChineseTeam, @Description, @EpisodeCount, @PagesCount, @CreatedAt, @UpdatedAt, @Finished,
                            @Cover, @Creator, @Categories, @Tags, @TotalViews, @TotalLikes, @LikesCount, @ViewsCount, @CommentsCount,
                            @AllowDownload, @AllowComment, @IsLiked, @IsFavourite, DATETIME());
                    """, comic, t);
                dapper.Execute("INSERT OR IGNORE INTO ComicCategory (ComicId, Category) VALUES (@ComicId, @Category);", comic.Categories.Select(x => new { ComicId = comic.Id, Category = x }), t);
                dapper.Execute("INSERT OR IGNORE INTO ComicTag (ComicId, Tag) VALUES (@ComicId, @Tag);", comic.Tags.Select(x => new { ComicId = comic.Id, Tag = x }), t);
                t.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
    }



    /// <summary>
    /// 保存漫画章节信息
    /// </summary>
    /// <param name="episodes"></param>
    private static void SaveComicEpisode(string comicId, IEnumerable<ComicEpisodeProfile> episodes)
    {
        Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                dapper.Execute("""
                    INSERT OR REPLACE INTO ComicEpisode (Id, ComicId, Title, "Order", UpdatedAt) VALUES (@Id, @ComicId, @Title, @Order, @UpdatedAt);
                    """, episodes.Select(x => new { x.Id, ComicId = comicId, x.Title, x.Order, x.UpdatedAt }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
    }




    /// <summary>
    /// 保存漫画章节图片信息
    /// </summary>
    /// <param name="episodes"></param>
    private static void SaveComicEpisodeImage(ComicEpisodeDetail detail)
    {
        Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                dapper.Execute("UPDATE ComicEpisode SET PageCount = @PageCount WHERE Id = @Id;", new { detail.Id, PageCount = detail.Images.Total });
                dapper.Execute("""
                    INSERT OR REPLACE INTO ComicEpisodeImage (Id, EpisodeId, ImageIndex, Image) VALUES (@Id, @EpisodeId, @ImageIndex, @Image);
                    """, detail.Images.List.Zip(Enumerable.Range(detail.Images.Limit * (detail.Images.Page - 1) + 1, detail.Images.List.Count))
                                           .Select(x => new { x.First.Id, EpisodeId = detail.Id, ImageIndex = x.Second, x.First.Image }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
    }



    /// <summary>
    /// 保存阅读记录
    /// </summary>
    /// <param name="comicId"></param>
    /// <param name="episodeId"></param>
    /// <param name="episodeIndex"></param>
    /// <param name="imageIndex"></param>
    /// <param name="type"></param>
    public static void SaveReadHistory(string comicId, string episodeId, int episodeIndex, int imageIndex, HistoryType type)
    {
        Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                dapper.Execute("""
                    INSERT INTO ReadHistory (ComicId, EpisodeId, EpisodeIndex, ImageIndex, DateTime, HistoryType) VALUES (@comicId, @episodeId, @episodeIndex, @imageIndex, @time, @type);
                    """, new { comicId, episodeId, episodeIndex, imageIndex, time = DateTimeOffset.Now, type });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
    }



    /// <summary>
    /// 保存阅读记录
    /// </summary>
    /// <param name="comicId"></param>
    /// <param name="type"></param>
    public static void SaveReadHistory(string comicId, HistoryType type)
    {
        SaveReadHistory(comicId, null!, 0, 0, type);
    }



    /// <summary>
    /// 保存搜索记录
    /// </summary>
    /// <param name="keyword"></param>
    public static void SaveSearchHistory(string keyword)
    {
        Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                dapper.Execute("INSERT INTO SearchHistory (Keyword, DateTime) VALUES (@keyword, @Now);", new { keyword, DateTimeOffset.Now });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
    }




    #endregion




    #region App


    /// <summary>
    /// 获取分流 IP
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetIpListAsync()
    {
        return await picaClient.GetIpListAsync();
    }



    /// <summary>
    /// 应用信息
    /// </summary>
    /// <returns></returns>
    public async Task<AppInfo> GetAppInfoAsync()
    {
        return await picaClient.GetAppInfoAsync();
    }





    /// <summary>
    /// 聊天室信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<ChatRoom>> GetChatRoomsAsync()
    {
        return await picaClient.GetChatRoomsAsync();
    }





    /// <summary>
    /// 主页分类
    /// </summary>
    /// <returns></returns>
    public async Task<List<HomeCategory>> GetHomeCategoriesAsync()
    {
        return await picaClient.GetHomeCategoriesAsync();
    }




    /// <summary>
    /// 骑士榜
    /// </summary>
    /// <returns></returns>
    public async Task<List<KnightProfile>> GetRankKnightsAsync()
    {
        return await picaClient.GetRankKnightsAsync();
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
        return await picaClient.LoginAsync(account, password);
    }


    /// <summary>
    /// 注销，即删除认证
    /// </summary>
    public void Logout()
    {
        picaClient.Logout();
    }


    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="register"></param>
    /// <returns>无返回值</returns>
    public async Task RegisterAccountAsync(RegisterAccountRequest register)
    {
        await picaClient.RegisterAccountAsync(register);
    }


    /// <summary>
    /// 忘记密码，获取安全问题，然后调用 <see cref="ResetPasswordAsync(string, int, string)"/>
    /// </summary>
    /// <param name="account">忘记密码的账号</param>
    /// <returns>安全问题</returns>
    public async Task<AccountSecurityQuestion> GetAccountSecurityQuestionAsync(string account)
    {
        return await picaClient.GetAccountSecurityQuestionAsync(account);
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
        return await picaClient.ResetPasswordAsync(account, questionIndex, answer);
    }


    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <returns>无返回值</returns>
    public async Task ChangePasswordAsync(string oldPassword, string newPassword)
    {
        await picaClient.ChangePasswordAsync(oldPassword, newPassword);
    }


    /// <summary>
    /// 用户信息
    /// </summary>
    /// <param name="id">为 null 时获取自己的信息</param>
    /// <returns>用户信息</returns>
    public async Task<UserProfile> GetUserProfileAsync(string? id = null)
    {
        return await picaClient.GetUserProfileAsync(id);
    }


    /// <summary>
    /// 修改个性签名
    /// </summary>
    /// <param name="slogan">个性签名</param>
    /// <returns>无返回值</returns>
    public async Task ChangeSloganAsync(string slogan)
    {
        await picaClient.ChangeSloganAsync(slogan);
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
        await picaClient.ChangeUserAvatarAsync(bytes, format);
    }



    /// <summary>
    /// 打哔咔
    /// </summary>
    /// <returns>已打哔咔/之前已打过哔咔</returns>
    public async Task<bool> PunchAsync()
    {
        return await picaClient.PunchAsync();
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
        var result = await picaClient.CategorySearchAsync(category, page, sort);
        SaveComicProfile(result.List);
        return result;
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
        var result = await picaClient.AdvanceSearchAsync(keyword, page, sort, categories);
        SaveComicProfile(result.List);
        SaveSearchHistory(keyword);
        return result;
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
        var result = await picaClient.GetFavouriteComicAsync(sort, page);
        SaveComicProfile(result.List, true);
        return result;
    }


    /// <summary>
    /// 添加或取消收藏
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns>已收藏/已取消收藏</returns>
    public async Task<bool> FavoriteComicAsync(string comicId)
    {
        var result = await picaClient.FavoriteComicAsync(comicId);
        _ = Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                dapper.Execute("UPDATE ComicDetail SET IsFavourite=@result WHERE Id = @comicId;", new { comicId, result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
        return result;
    }




    /// <summary>
    /// 喜欢或取消喜欢
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns>已喜欢/已取消喜欢</returns>
    public async Task<bool> LikeComicAsync(string comicId)
    {
        var result = await picaClient.LikeComicAsync(comicId);
        _ = Task.Run(() =>
        {
            try
            {
                using var dapper = DatabaseProvider.CreateConnection();
                dapper.Execute("UPDATE ComicDetail SET IsLiked=@result WHERE Id = @comicId;", new { comicId, result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        });
        return result;
    }



    /// <summary>
    /// 排行榜漫画
    /// </summary>
    /// <param name="type">排行榜类型，日周月</param>
    /// <returns></returns>
    public async Task<List<RankComic>> GetRankComicsAsync(RankType type)
    {
        var list = await picaClient.GetRankComicsAsync(type);
        SaveComicProfile(list);
        return list;
    }



    /// <summary>
    /// 漫画详情
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns></returns>
    public async Task<ComicDetail> GetComicDetailAsync(string comicId)
    {
        var detail = await picaClient.GetComicDetailAsync(comicId);
        SaveComicDetail(detail);
        return detail;
    }



    /// <summary>
    /// 漫画章节列表
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicEpisodeProfile>> GetComicEpisodeListAsync(string comicId, int page = 1)
    {
        var episodes = await picaClient.GetComicEpisodeListAsync(comicId, page);
        SaveComicEpisode(comicId, episodes.List);
        return episodes;
    }



    /// <summary>
    /// 看了这本的人也在看
    /// </summary>
    /// <param name="comicId">漫画 id</param>
    /// <returns></returns>
    public async Task<List<ComicProfile>> GetRecommendComicsAsync(string comicId)
    {
        var list = await picaClient.GetRecommendComicsAsync(comicId);
        SaveComicProfile(list);
        return list;
    }



    /// <summary>
    /// 漫画章节图片内容
    /// </summary>
    /// <param name="comicId">漫画 id <see cref="ComicProfile.Id"/></param>
    /// <param name="episodeOrderId">漫画章节顺序 <see cref="ComicEpisodeProfile.Order"/></param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public async Task<ComicEpisodeDetail> GetComicEpisodeImagesAsync(string comicId, int episodeOrderId, int page = 1)
    {
        var detail = await picaClient.GetComicEpisodeImagesAsync(comicId, episodeOrderId, page);
        SaveComicEpisodeImage(detail);
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
        return await picaClient.GetComicCommentAsync(comicId, page);
    }


    /// <summary>
    /// 发表漫画评论
    /// </summary>
    /// <param name="comicId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task PublishComicCommentAsync(string comicId, string content)
    {
        await picaClient.PublishComicCommentAsync(comicId, content);
    }



    /// <summary>
    /// 大家都在搜
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetKeywordsAsync()
    {
        return await picaClient.GetKeywordsAsync();
    }



    /// <summary>
    /// 随机本子
    /// </summary>
    /// <returns></returns>
    public async Task<List<ComicProfile>> GetRandomComicsAsync()
    {
        var list = await picaClient.GetRandomComicsAsync();
        SaveComicProfile(list);
        return list;
    }



    /// <summary>
    /// 本子推荐
    /// </summary>
    /// <returns></returns>
    public async Task<List<RecommendComic>> GetRecommendComicsAsync()
    {
        var recommend = await picaClient.GetRecommendComicsAsync();
        SaveComicProfile(recommend.SelectMany(x => x.Comics));
        return recommend;
    }



    /// <summary>   
    /// 大家都在看
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<ComicProfile>> GetEveryoneLovesAsync(int page = 1)
    {
        var result = await picaClient.GetEveryoneLovesAsync(page);
        SaveComicProfile(result.List);
        return result;
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
        return await picaClient.GetMyCommentAsync(page);
    }



    /// <summary>
    /// 给评论喜欢
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns>已喜欢</returns>
    /// <exception cref="PicaApiException"></exception>
    public async Task<bool> LikeCommentAsync(string commentId)
    {
        return await picaClient.LikeCommentAsync(commentId);
    }


    /// <summary>
    /// 获取子评论
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<SubCommentItem>> GetSubCommentAsync(string commentId, int page = 1)
    {
        return await picaClient.GetSubCommentAsync(commentId, page);
    }


    /// <summary>
    /// 发表子评论
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task PublishSubCommentAsync(string commentId, string content)
    {
        await picaClient.PublishSubCommentAsync(commentId, content);
    }



    /// <summary>
    /// 获取留言板评论
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<CommentCollection> GetSquareCommentAsync(int page = 1)
    {
        return await picaClient.GetSquareCommentAsync(page);
    }


    /// <summary>
    /// 发表留言板评论
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task PublishSquareCommentAsync(string content)
    {
        await picaClient.PublishSquareCommentAsync(content);
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
        return await picaClient.GetGameListAsync(page);
    }




    /// <summary>
    /// 游戏详情
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<GameDetail> GetGameDetailAsync(string gameId)
    {
        return await picaClient.GetGameDetailAsync(gameId);
    }




    /// <summary>
    /// 游戏评论
    /// </summary>
    /// <param name="gameId">游戏 id</param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<PicaPageResult<CommentCollection>> GetGameCommentsAsync(string gameId, int page = 1)
    {
        return await picaClient.GetGameCommentsAsync(gameId, page);
    }




    /// <summary>
    /// 发布游戏评论
    /// </summary>
    /// <param name="gameId">游戏 id</param>
    /// <param name="content">评论内容</param>
    /// <returns>无返回值</returns>
    public async Task PublishGameCommentsAsync(string gameId, string content)
    {
        await picaClient.PublishGameCommentsAsync(gameId, content);
    }




    #endregion



}

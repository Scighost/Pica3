using Pica3.CoreApi.Account;

namespace Pica3.CoreApi.Comic;

/// <summary>
/// 漫画详细信息
/// </summary>
public class ComicDetail : ComicProfile
{

    /// <summary>
    /// 创建者
    /// </summary>
    [JsonPropertyName("_creator")]
    public UserProfile Creator { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// 团队
    /// </summary>
    [JsonPropertyName("chineseTeam")]
    public string ChineseTeam { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 允许下载
    /// </summary>
    [JsonPropertyName("allowDownload")]
    public bool AllowDownload { get; set; }

    /// <summary>
    /// 允许评论
    /// </summary>
    [JsonPropertyName("allowComment")]
    public bool AllowComment { get; set; }

    /// <summary>
    /// 阅读数，好像比 <see cref="ComicProfile.TotalViews"/> 大
    /// </summary>
    [JsonPropertyName("viewsCount")]
    public int ViewsCount { get; set; }

    /// <summary>
    /// 评论数
    /// </summary>
    [JsonPropertyName("commentsCount")]
    public int CommentsCount { get; set; }

    /// <summary>
    /// 已收藏
    /// </summary>
    [JsonPropertyName("isFavourite")]
    public bool IsFavourite { get; set; }

    /// <summary>
    /// 已喜欢
    /// </summary>
    [JsonPropertyName("isLiked")]
    public bool IsLiked { get; set; }


}



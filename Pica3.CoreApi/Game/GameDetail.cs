namespace Pica3.CoreApi.Game;


/// <summary>
/// 游戏详细信息
/// </summary>
public class GameDetail : GameProfile
{

    /// <summary>
    /// 简介
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// iOS 链接
    /// </summary>
    [JsonPropertyName("iosLinks")]
    public List<string> IosLinks { get; set; }

    /// <summary>
    /// 安卓链接
    /// </summary>
    [JsonPropertyName("androidLinks")]
    public List<string> AndroidLinks { get; set; }

    /// <summary>
    /// 下载数
    /// </summary>
    [JsonPropertyName("downloadsCount")]
    public int DownloadsCount { get; set; }

    /// <summary>
    /// 截图
    /// </summary>
    [JsonPropertyName("screenshots")]
    public List<PicaImage> Screenshots { get; set; }

    /// <summary>
    /// 安卓端大小 MB
    /// </summary>
    [JsonPropertyName("androidSize")]
    public int AndroidSize { get; set; }

    /// <summary>
    /// iOS 大小 MB
    /// </summary>
    [JsonPropertyName("iosSize")]
    public int IosSize { get; set; }

    /// <summary>
    /// 视频链接
    /// </summary>
    [JsonPropertyName("videoLink")]
    public string VideoLink { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 点赞数据
    /// </summary>
    [JsonPropertyName("likesCount")]
    public int LikesCount { get; set; }

    /// <summary>
    /// 我已点赞
    /// </summary>
    [JsonPropertyName("isLiked")]
    public bool IsLiked { get; set; }

    /// <summary>
    /// 评论数
    /// </summary>
    [JsonPropertyName("commentsCount")]
    public int CommentsCount { get; set; }


}

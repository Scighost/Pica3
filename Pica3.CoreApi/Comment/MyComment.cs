using Pica3.CoreApi.Comic;

namespace Pica3.CoreApi.Comment;

/// <summary>
/// 我的评论
/// </summary>
public class MyComment
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    /// <summary>
    /// 评论内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// 漫画，仅 <see cref="ComicProfile.Id"/> <see cref="ComicProfile.Title"/> 有值
    /// </summary>
    [JsonPropertyName("_comic")]
    public ComicProfile Comic { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [JsonPropertyName("hide")]
    public bool Hide { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    [JsonPropertyName("likesCount")]
    public int LikesCount { get; set; }

    /// <summary>
    /// 子评论数
    /// </summary>
    [JsonPropertyName("commentsCount")]
    public int CommentsCount { get; set; }

    /// <summary>
    /// 我已点赞
    /// </summary>
    [JsonPropertyName("isLiked")]
    public bool IsLiked { get; set; }
}

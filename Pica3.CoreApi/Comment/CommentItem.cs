using Pica3.CoreApi.Account;

namespace Pica3.CoreApi.Comment;

/// <summary>
/// 一条评论
/// </summary>
public class CommentItem
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// 发布评论的用户
    /// </summary>
    [JsonPropertyName("_user")]
    public UserProfile User { get; set; }

    /// <summary>
    /// 漫画 id
    /// </summary>
    [JsonPropertyName("_comic")]
    public string ComicId { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    [JsonPropertyName("isTop")]
    public bool IsTop { get; set; }

    [JsonPropertyName("hide")]
    public bool Hide { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("likesCount")]
    public int LikesCount { get; set; }

    [JsonPropertyName("commentsCount")]
    public int SubCommentsCount { get; set; }

    [JsonPropertyName("isLiked")]
    public bool IsLiked { get; set; }
}

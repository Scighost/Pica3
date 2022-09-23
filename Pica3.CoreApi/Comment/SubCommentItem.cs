namespace Pica3.CoreApi.Comment;

/// <summary>
/// 一条子评论
/// </summary>
public class SubCommentItem : CommentItem
{
    /// <summary>
    /// 回复的评论id
    /// </summary>
    [JsonPropertyName("_parent")]
    public string ParentId { get; set; }
}
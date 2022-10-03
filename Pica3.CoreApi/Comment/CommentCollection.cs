namespace Pica3.CoreApi.Comment;



/// <summary>
/// 一组评论
/// </summary>
public class CommentCollection
{
    /// <summary>
    /// 评论，时间倒序，可通过 <see cref="PicaPageResult{T}.Total"/> 算出评论楼层
    /// </summary>
    [JsonPropertyName("comments")]
    public PicaPageResult<CommentItem> Comments { get; set; }

    /// <summary>
    /// 置顶评论
    /// </summary>
    [JsonPropertyName("topComments")]
    public List<CommentItem> TopComments { get; set; }
}

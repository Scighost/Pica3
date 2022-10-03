namespace Pica3.CoreApi.Comic;


/// <summary>
/// 漫画简单信息
/// </summary>
public class ComicProfile
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; }

    /// <summary>
    /// 观看数
    /// </summary>
    [JsonPropertyName("totalViews")]
    public int TotalViews { get; set; }

    /// <summary>
    /// 点赞数，同 <see cref="LikesCount"/>
    /// </summary>
    [JsonPropertyName("totalLikes")]
    public int TotalLikes { get; set; }

    /// <summary>
    /// 图片页数
    /// </summary>
    [JsonPropertyName("pagesCount")]
    public int PagesCount { get; set; }

    /// <summary>
    /// 章节数
    /// </summary>
    [JsonPropertyName("epsCount")]
    public int EpisodeCount { get; set; }

    /// <summary>
    /// 完结
    /// </summary>
    [JsonPropertyName("finished")]
    public bool Finished { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; }

    /// <summary>
    /// 封面图
    /// </summary>
    [JsonPropertyName("thumb")]
    public PicaFile Cover { get; set; }

    /// <summary>
    /// 点赞数，同 <see cref="TotalLikes"/>
    /// </summary>
    [JsonPropertyName("likesCount")]
    public int LikesCount { get; set; }


}



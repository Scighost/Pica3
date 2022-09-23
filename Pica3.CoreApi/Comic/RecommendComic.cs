namespace Pica3.CoreApi.Comic;


/// <summary>
/// 本子神推荐
/// </summary>
public class RecommendComic
{

    /// <summary>
    /// 本子 神/妹/母 推荐
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }


    [JsonPropertyName("comics")]
    public List<ComicProfile> Comics { get; set; }

}

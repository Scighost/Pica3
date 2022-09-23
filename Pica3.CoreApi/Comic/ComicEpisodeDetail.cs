namespace Pica3.CoreApi.Comic;

/// <summary>
/// 漫画章节图片信息
/// </summary>
public class ComicEpisodeDetail : ComicEpisodeProfile
{

    [JsonPropertyName("pages")]
    public PicaPageResult<ComicEpisodePage> Pages { get; set; }

}


/// <summary>
/// 漫画章节图片
/// </summary>
public class ComicEpisodePage
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }


    [JsonPropertyName("media")]
    public PicaImage Image { get; set; }

}
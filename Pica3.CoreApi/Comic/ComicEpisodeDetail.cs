namespace Pica3.CoreApi.Comic;

/// <summary>
/// 漫画章节图片信息
/// </summary>
public class ComicEpisodeDetail : ComicEpisodeProfile
{

    [JsonPropertyName("pages")]
    public PicaPageResult<ComicEpisodeImage> Images { get; set; }

}


/// <summary>
/// 漫画章节图片
/// </summary>
public class ComicEpisodeImage
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }


    [JsonPropertyName("media")]
    public PicaFile Image { get; set; }

}
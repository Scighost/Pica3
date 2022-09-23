namespace Pica3.CoreApi.Comic;

/// <summary>
/// 漫画章节简单信息
/// </summary>
public class ComicEpisodeProfile
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

}






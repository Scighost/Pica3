namespace Pica3.CoreApi.App;


/// <summary>
/// 主页分类
/// </summary>
public class HomeCategory
{

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 图片
    /// </summary>
    [JsonPropertyName("thumb")]
    public PicaFile Thumb { get; set; }

    /// <summary>
    /// 是网页
    /// </summary>
    [JsonPropertyName("isWeb")]
    public bool IsWeb { get; set; }

    /// <summary>
    /// 可用
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// 网页链接
    /// </summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}



namespace Pica3.CoreApi.Game;

/// <summary>
/// 游戏简单信息
/// </summary>
public class GameProfile
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    /// <summary>
    /// 游戏名
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// 版本更新内容
    /// </summary>
    [JsonPropertyName("updateContent")]
    public string? UpdateContent { get; set; }

    /// <summary>
    /// 发布商
    /// </summary>
    [JsonPropertyName("publisher")]
    public string Publisher { get; set; }

    /// <summary>
    /// 推荐
    /// </summary>
    [JsonPropertyName("suggest")]
    public bool Suggest { get; set; }

    /// <summary>
    /// 18+
    /// </summary>
    [JsonPropertyName("adult")]
    public bool Adult { get; set; }

    /// <summary>
    /// 安卓平台
    /// </summary>
    [JsonPropertyName("android")]
    public bool Android { get; set; }

    /// <summary>
    /// iOS 平台
    /// </summary>
    [JsonPropertyName("ios")]
    public bool Ios { get; set; }

    /// <summary>
    /// 封面
    /// </summary>
    [JsonPropertyName("icon")]
    public PicaImage Icon { get; set; }


}

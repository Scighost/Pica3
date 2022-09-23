namespace Pica3.CoreApi.App;

/// <summary>
/// 哔咔应用信息
/// </summary>
public class AppInfo
{
    /// <summary>
    /// 已签到
    /// </summary>
    [JsonPropertyName("isPunched")]
    public bool IsPunched { get; set; }

    /// <summary>
    /// 最新版本信息
    /// </summary>
    [JsonPropertyName("latestApplication")]
    public LatestApplication LatestApplication { get; set; }

    /// <summary>
    /// 图片服务器
    /// </summary>
    [JsonPropertyName("imageServer")]
    public string ImageServer { get; set; }

    [JsonPropertyName("apiLevel")]
    public int ApiLevel { get; set; }

    [JsonPropertyName("minApiLevel")]
    public int MinApiLevel { get; set; }

    /// <summary>
    /// 主页分类
    /// </summary>
    [JsonPropertyName("categories")]
    public List<HomeCategory> Categories { get; set; }

    [JsonPropertyName("notification")]
    public object Notification { get; set; }

    [JsonPropertyName("isIdUpdated")]
    public bool IsIdUpdated { get; set; }

}


/// <summary>
/// 哔咔 Apk 最后版本
/// </summary>
public class LatestApplication
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    /// <summary>
    /// 下载链接
    /// </summary>
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; }

    /// <summary>
    /// 更新内容
    /// </summary>
    [JsonPropertyName("updateContent")]
    public string UpdateContent { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("apk")]
    public PicaImage Apk { get; set; }
}






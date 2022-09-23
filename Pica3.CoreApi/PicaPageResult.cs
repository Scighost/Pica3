namespace Pica3.CoreApi;


/// <summary>
/// 哔咔分页结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class PicaPageResult<T>
{
    /// <summary>
    /// 总计多少页
    /// </summary>
    [JsonPropertyName("pages")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Pages { get; set; }

    /// <summary>
    /// 总数
    /// </summary>
    [JsonPropertyName("total")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Total { get; set; }

    /// <summary>
    /// list of <see cref="T"/>
    /// </summary>
    [JsonPropertyName("docs")]
    public List<T> TList { get; set; }

    /// <summary>
    /// 这是第几页
    /// </summary>
    [JsonPropertyName("page")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Page { get; set; }

    /// <summary>
    /// 每页有多少
    /// </summary>
    [JsonPropertyName("limit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Limit { get; set; }
}

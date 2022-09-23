namespace Pica3.CoreApi.Comic;

/// <summary>
/// 排行榜漫画简单信息
/// </summary>
public class RankComic : ComicProfile
{
    /// <summary>
    /// 榜内绅士指数
    /// </summary>
    [JsonPropertyName("leaderboardCount")]
    public int LeaderboardCount { get; set; }
}



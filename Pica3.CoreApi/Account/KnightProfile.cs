namespace Pica3.CoreApi.Account;

/// <summary>
/// 骑士榜用户信息
/// </summary>
public class KnightProfile : UserProfile
{

    /// <summary>
    /// 上传本子数
    /// </summary>
    [JsonPropertyName("comicsUploaded")]
    public int UploadedComicCount { get; set; }

}

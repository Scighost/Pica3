namespace Pica3.CoreApi.Account;

/// <summary>
/// 用户信息
/// </summary>
public class UserProfile
{

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    [JsonPropertyName("email")]
    public string Account { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    [JsonPropertyName("slogan")]
    public string Slogan { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    [JsonPropertyName("birthday")]
    public DateTimeOffset Birthday { get; set; }

    /// <summary>
    /// 性别，m/f/bot
    /// </summary>
    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    /// <summary>
    /// 等级称号
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public PicaFile? Avatar { get; set; }

    /// <summary>
    /// 是否验证，未验证无法发布评论
    /// </summary>
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    /// <summary>
    /// 经验
    /// </summary>
    [JsonPropertyName("exp")]
    public int Exp { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 打不开 https://pica-web.wakamoment.tk/images/halloween_m.png
    /// </summary>
    [JsonPropertyName("character")]
    public string Character { get; set; }

    /// <summary>
    /// 角色，骑士，和 <see cref="Role"/> 对应
    /// </summary>
    [JsonPropertyName("characters")]
    public List<string> Characters { get; set; }

    /// <summary>
    /// 创建账号的时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 账号激活时间
    /// </summary>
    [JsonPropertyName("activation_date")]
    public DateTimeOffset ActivationDate { get; set; }

    /// <summary>
    /// 打过哔咔
    /// </summary>
    [JsonPropertyName("isPunched")]
    public bool IsPunched { get; set; }

    /// <summary>
    /// member/knight
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }
}


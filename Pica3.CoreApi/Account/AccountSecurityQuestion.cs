namespace Pica3.CoreApi.Account;

/// <summary>
/// 账号安全问题
/// </summary>
public class AccountSecurityQuestion
{

    [JsonPropertyName("question1")]
    public string Question1 { get; set; }

    [JsonPropertyName("question2")]
    public string Question2 { get; set; }

    [JsonPropertyName("question3")]
    public string Question3 { get; set; }

}

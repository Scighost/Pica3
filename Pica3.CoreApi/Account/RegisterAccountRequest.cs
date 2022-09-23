namespace Pica3.CoreApi.Account;

/// <summary>
/// 注册账号
/// </summary>
public class RegisterAccountRequest
{

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Account { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    /// <summary>
    /// m/f/bot
    /// </summary>
    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    /// <summary>
    /// yyyy/mm/dd
    /// </summary>
    [JsonPropertyName("birthday")]
    public string Birthday { get; set; }

    [JsonPropertyName("question1")]
    public string Question1 { get; set; }

    [JsonPropertyName("question2")]
    public string Question2 { get; set; }

    [JsonPropertyName("question3")]
    public string Question3 { get; set; }

    [JsonPropertyName("answer1")]
    public string Answer1 { get; set; }

    [JsonPropertyName("answer2")]
    public string Answer2 { get; set; }

    [JsonPropertyName("answer3")]
    public string Answer3 { get; set; }

}

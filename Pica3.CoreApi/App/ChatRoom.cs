namespace Pica3.CoreApi.App;

/// <summary>
/// 聊天室
/// </summary>
public class ChatRoom
{

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

}



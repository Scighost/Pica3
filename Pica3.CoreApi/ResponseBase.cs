namespace Pica3.CoreApi;

internal class ResponseBase<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("error")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Error { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("data")]
    public T Data { get; set; }

    public bool Success => Code == 200;

}



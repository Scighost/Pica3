namespace Pica3.CoreApi;

/// <summary>
/// 哔咔图片或文件
/// </summary>
public class PicaImage
{

    [JsonPropertyName("originalName")]
    public string OriginalName { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("fileServer")]
    public string FileServer { get; set; }

    [JsonIgnore]
    public string Url => FileServer.Contains("static") ? FileServer + Path : $"{FileServer}/static/{Path}";

}

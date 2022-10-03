namespace Pica3.CoreApi;

/// <summary>
/// 哔咔图片或文件，使用 <see cref="Url"/> 获取完整链接
/// </summary>
public class PicaFile
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

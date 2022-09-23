namespace Pica3.CoreApi;


/// <summary>
/// Api 返回值 code != 200 时，抛出此错误
/// </summary>
public class PicaApiException : Exception
{

    public string? Url { get; set; }

    public int Code { get; set; }

    public string? Detail { get; set; }


    public PicaApiException(string? url, string? message, int code = 0, string? detail = null) : base(message)
    {
        this.Url = url;
        this.Code = code;
        this.Detail = detail;
    }

}

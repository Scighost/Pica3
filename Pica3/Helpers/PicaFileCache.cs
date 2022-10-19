using Scighost.WinUILib.Cache;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.Storage;

namespace Pica3.Helpers;

internal class PicaFileCache : CacheBase<StorageFile>
{


    private static PicaFileCache instance;


    public static PicaFileCache Instance => instance ??= new PicaFileCache
    {
        CacheDuration = TimeSpan.FromDays(30),
        RetryCount = 3,
    };




    private Uri? overrideBaseAddress;


    public void ChangeProxyAndBaseAddress(IWebProxy? proxy = null, Uri? baseAddress = null)
    {
        _httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy });
        overrideBaseAddress = baseAddress;
    }



    protected override HttpRequestMessage GetHttpRequestMessage(Uri uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (overrideBaseAddress != null)
        {
            if (uri.ToString().Contains("tobeimg"))
            {
                request.RequestUri = new Uri(overrideBaseAddress, uri.PathAndQuery.Replace("/static/tobeimg", ""));
                request.Headers.Add("Host", "img.picacomic.com");
            }
            else if (uri.ToString().Contains("tobs"))
            {
                request.RequestUri = new Uri(overrideBaseAddress, uri.PathAndQuery.Replace("/tobs", ""));
                request.Headers.Add("Host", uri.Host);
            }
            else
            {
                request.RequestUri = new Uri(overrideBaseAddress, uri.PathAndQuery);
                request.Headers.Add("Host", uri.Host);
            }
        }
        return request;
    }



    protected override Task<StorageFile> InitializeTypeAsync(Stream stream)
    {
        throw new NotImplementedException();
    }

    protected override Task<StorageFile> InitializeTypeAsync(StorageFile baseFile)
    {
        return Task.FromResult(baseFile);
    }

}

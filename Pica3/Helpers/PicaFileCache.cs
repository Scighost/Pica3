using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Windows.Storage;

namespace Pica3.Helpers;

internal class PicaFileCache : PicaFileCacheBase<StorageFile>
{


    private static PicaFileCache instance;


    public static PicaFileCache Instance => instance ??= new PicaFileCache
    {
        CacheDuration = TimeSpan.FromDays(30),
        RetryCount = 3
    };




    private Uri? overrideBaseAddress;


    public void ChangeProxyAndBaseAddress(IWebProxy? proxy = null, Uri? baseAddress = null)
    {
        _httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy });
        overrideBaseAddress = baseAddress;
    }



    protected override async Task<StorageFile?> DownloadFileAsync(Uri uri, StorageFile baseFile, bool preCacheOnly, CancellationToken cancellationToken, List<KeyValuePair<string, object>>? initializerKeyValues = null)
    {
        HttpRequestMessage request;
        if (overrideBaseAddress is null)
        {
            request = new HttpRequestMessage(HttpMethod.Get, uri);
        }
        else
        {
            request = new HttpRequestMessage(HttpMethod.Get, new Uri(overrideBaseAddress, uri.PathAndQuery));
            request.Headers.Add("Host", uri.Host);
        }
        var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var hs = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var ms = new MemoryStream();
        await hs.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        await ms.FlushAsync().ConfigureAwait(false);
        ms.Position = 0;

        using var fs = await baseFile.OpenStreamForWriteAsync();
        await ms.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
        await fs.FlushAsync().ConfigureAwait(false);

        return baseFile;
    }



    protected override Task<StorageFile> InitializeTypeAsync(Stream stream, List<KeyValuePair<string, object>>? initializerKeyValues = null)
    {
        throw new NotImplementedException();
    }

    protected override Task<StorageFile> InitializeTypeAsync(StorageFile baseFile, List<KeyValuePair<string, object>>? initializerKeyValues = null)
    {
        return Task.FromResult(baseFile);
    }

}

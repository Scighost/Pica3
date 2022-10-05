using Microsoft.Extensions.DependencyInjection;
using Pica3.CoreApi;
using Pica3.Services;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.Storage;

namespace Pica3.Helpers;

internal static class ServiceProvider
{


    private static Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider;

    private static bool _initialized;


    public static HttpClient HttpClient { get; private set; }




    public static void Initialize()
    {
        if (!_initialized)
        {
            var sc = new ServiceCollection();
            ConfigureHttpClient(sc);
            ConfigureService(sc);
            _serviceProvider = sc.BuildServiceProvider();
            _initialized = true;
        }
    }




    private static void ConfigureHttpClient(ServiceCollection sc)
    {
        WebProxy? proxy = null;
        Uri? address = null;
        if (IPEndPoint.TryParse(AppSetting.GetValue<string>(SettingKeys.WebProxy)!, out var ipEndPoint))
        {
            proxy = new WebProxy(ipEndPoint.ToString());
        }
        if (IPAddress.TryParse(AppSetting.GetValue<string>(SettingKeys.OverrideApiAddress), out var ipAddress))
        {
            address = new Uri("https://" + ipAddress.ToString());
        }
        sc.AddSingleton(new PicaClient(proxy, address));

        var folder = AppSetting.GetValue<string>(SettingKeys.CacheFolder) ?? Path.Combine(AppContext.BaseDirectory, "Cache");
        if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }
        PicaFileCache.Instance.Initialize(StorageFolder.GetFolderFromPathAsync(folder).GetAwaiter().GetResult());
        PicaFileCache.Instance.ChangeProxyAndBaseAddress(proxy, address);

        HttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        });
    }



    private static void ConfigureService(ServiceCollection sc)
    {
        sc.AddSingleton<PicaService>();
    }





    /// <summary>
    /// 修改代理设置
    /// </summary>
    /// <param name="proxy"></param>
    public static void ChangeProxyAndBaseAddress(IWebProxy? proxy = null, Uri? address = null)
    {
        GetService<PicaClient>()?.ChangeProxyAndBaseAddress(proxy, address);
        PicaFileCache.Instance.ChangeProxyAndBaseAddress(proxy, address);
        HttpClient = new HttpClient(new HttpClientHandler
        {
            Proxy = proxy,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        });
    }





    public static T? GetService<T>()
    {
        if (!_initialized)
        {
            Initialize();
        }
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider);
    }



    public static void Dispose()
    {
        _serviceProvider?.Dispose();
    }




}

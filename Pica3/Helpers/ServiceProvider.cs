using Microsoft.Extensions.DependencyInjection;
using Pica3.Controls;
using Pica3.CoreApi;
using Pica3.ViewModels;
using System.Net;
using System.Net.Http;

namespace Pica3.Helpers;

internal static class ServiceProvider
{


    private static Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider;

    private static bool _isInitialized;


    private static HttpClient _httpClient;

    public static HttpClient HttpClient
    {
        get
        {
            if (_httpClient is null)
            {
                InitializeHttpClient();
            }
            return _httpClient!;
        }
    }



    private static void Initialize()
    {
        var sc = new ServiceCollection();
        ConfigureHttpClient(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _isInitialized = true;
    }




    private static void ConfigureHttpClient(ServiceCollection sc)
    {
        if (IPEndPoint.TryParse(AppSetting.GetValue<string>(SettingKeys.WebProxy)!, out var address))
        {
            sc.AddSingleton(new PicaClient(proxy: new WebProxy(address.ToString())));
        }
        else
        {
            sc.AddSingleton(new PicaClient());
        }
    }


    private static void ConfigureService(ServiceCollection sc)
    {

    }



    private static void InitializeHttpClient()
    {
        if (IPEndPoint.TryParse(AppSetting.GetValue<string>(SettingKeys.WebProxy)!, out var address))
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                Proxy = new WebProxy(address.ToString()),
                AutomaticDecompression = DecompressionMethods.All,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            });

        }
        else
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            });
        }
    }



    /// <summary>
    /// 修改代理设置
    /// </summary>
    /// <param name="proxy"></param>
    public static void ChangeProxyAndBaseAddress(IWebProxy? proxy = null, Uri? address = null)
    {
        var client = GetService<PicaClient>();
        client?.ChangeProxyAndBaseAddress(proxy, address);
        _httpClient = new HttpClient(new HttpClientHandler
        {
            Proxy = proxy,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        });
        PicaFileCache.Instance.ChangeProxyAndBaseAddress(proxy, address);
    }





    public static T? GetService<T>()
    {
        if (!_isInitialized)
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

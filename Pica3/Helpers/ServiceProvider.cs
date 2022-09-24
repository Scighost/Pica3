using Microsoft.Extensions.DependencyInjection;
using Pica3.CoreApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pica3.Helpers;

internal static class ServiceProvider
{


    private static Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider;

    private static bool _isInitialized;


    private static void Initialize()
    {
        var sc = new ServiceCollection();
        ConfigureService(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _isInitialized = true;
    }




    private static void ConfigureService(ServiceCollection sc)
    {
        if (IPEndPoint.TryParse(AppSetting.GetValue<string>(SettingKeys.WebProxy)!, out var address))
        {
            sc.AddSingleton(new PicaClient(new WebProxy(address.ToString())));
            sc.AddSingleton(new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                Proxy = new WebProxy(address.ToString()),
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            }));

        }
        else
        {
            sc.AddSingleton(new PicaClient());
            sc.AddSingleton(new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            }));
        }
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

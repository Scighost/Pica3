using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Scighost.WinUILib.Cache;
using System.IO;
using System.Net.Http;
using System.Threading;
using Windows.Storage;

namespace Pica3.Controls;

internal class CachedImage : ImageEx
{

    private static bool initialized;


    static CachedImage()
    {
        ImageCache.Instance.CacheDuration = TimeSpan.FromDays(30);
        ImageCache.Instance.RetryCount = 5;
        Initialize();
    }



    private static void Initialize()
    {
        if (initialized)
        {
            return;
        }
        try
        {
            var folder = AppSetting.GetValue<string>(SettingKeys.CacheFolder) ?? Path.Combine(AppContext.BaseDirectory, "Cache");
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }
            ImageCache.Instance.Initialize(StorageFolder.GetFolderFromPathAsync(folder).GetAwaiter().GetResult(), ServiceProvider.GetService<HttpClient>());
            initialized = true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }




    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "file" or "ms-appx")
            {
                return new BitmapImage(imageUri);
            }
            var image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Image source has changed.");
            }
            if (image is null)
            {
                throw new FileNotFoundException(imageUri.ToString());
            }
            return image;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            await ImageCache.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }

    }


}

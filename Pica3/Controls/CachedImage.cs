using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Threading;
using Windows.Storage;

namespace Pica3.Controls;

internal class CachedImage : ImageEx
{


    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "file" or "ms-appx")
            {
                return new BitmapImage(imageUri);
            }
            var file = await PicaFileCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Image source has changed.");
            }
            if (file is null)
            {
                throw new FileNotFoundException(imageUri.ToString());
            }
            return new BitmapImage(new Uri(file.Path));
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            await PicaFileCache.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }
    }


}

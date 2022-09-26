using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Scighost.WinUILib.Cache;
using System.IO;
using System.Threading;

namespace Pica3.Controls;

internal class ComicImage : ImageEx
{



    public string ImageOrder
    {
        get { return (string)GetValue(ImageOrderProperty); }
        set { SetValue(ImageOrderProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ImageOrder.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageOrderProperty =
        DependencyProperty.Register("ImageOrder", typeof(string), typeof(ComicImage), new PropertyMetadata(null));





    protected override void OnImageOpened(object sender, RoutedEventArgs e)
    {
        base.OnImageOpened(sender, e);
        ClearValue(HeightProperty);
        ClearValue(WidthProperty);
    }




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

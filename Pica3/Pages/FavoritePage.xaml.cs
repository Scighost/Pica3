using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pica3.CoreApi.Comic;
using Pica3.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class FavoritePage : Page
{


    [ObservableProperty]
    private FavoritePageModel? _VM;


    public FavoritePage()
    {
        this.InitializeComponent();
        VM = ServiceProvider.GetService<FavoritePageModel>();
        if (VM != null)
        {
            VM.Initialize();
            Loaded += VM.Loaded;
        }
    }

    private async void ComicProfileGridView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverBackAnimation");
            if (ani != null && VM != null)
            {
                if (VM.LastClickedComic != null && (VM.ComicList?.Contains(VM.LastClickedComic) ?? false) && sender is GridView gridView)
                {
                    gridView.ScrollIntoView(VM.LastClickedComic);
                    gridView.UpdateLayout();
                    ani.Configuration = new BasicConnectedAnimationConfiguration();
                    await gridView.TryStartConnectedAnimationAsync(ani, VM.LastClickedComic, "c_Image_ComicCover");
                }
                else
                {
                    ani.Cancel();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    private void ComicProfileGridView_ItemClicked(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is ComicProfile comic && sender is GridView gridView)
            {
                if (VM != null)
                {
                    VM.LastClickedComic = comic;
                }
                var ani = gridView.PrepareConnectedAnimation("ComicCoverAnimation", comic, "c_Image_ComicCover");
                ani.Configuration = new BasicConnectedAnimationConfiguration();
                MainPage.Current.Navigate(typeof(ComicDetailPage), comic, new SuppressNavigationTransitionInfo());
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class HomePage : Page
{


    private readonly PicaClient picaClient;


    public HomePage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += HomePage_Loaded;
    }







    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin)
            {
                if (RecommendComics is null)
                {
                    RecommendComics = await picaClient.GetRecommendComicsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }





    [ObservableProperty]
    private List<RecommendComic> recommendComics;




    private ComicProfile? lastClickedComic = null;



    private async void c_GridView_Recommend_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is GridView gridView && lastClickedComic != null && gridView.ContainerFromItem(lastClickedComic) != null)
            {
                gridView.ScrollIntoView(lastClickedComic);
                gridView.UpdateLayout();
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverBackAnimation");
                if (ani != null)
                {
                    ani.Configuration = new BasicConnectedAnimationConfiguration();
                    await gridView.TryStartConnectedAnimationAsync(ani, lastClickedComic, "c_Image_ComicCover");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    private void c_GridView_Recommend_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (sender is GridView gridView && e.ClickedItem is ComicProfile comic)
            {
                lastClickedComic = comic;
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

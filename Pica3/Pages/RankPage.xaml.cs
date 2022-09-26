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
public sealed partial class RankPage : Page
{


    private readonly PicaClient picaClient;


    public RankPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += RankPage_Loaded;
    }



    private async void RankPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin)
            {
                DayRanks ??= await picaClient.GetRankComicsAsync(RankType.H24);
                WeekRanks ??= await picaClient.GetRankComicsAsync(RankType.D7);
                MonthRanks ??= await picaClient.GetRankComicsAsync(RankType.D30);
                SelectedRanks ??= DayRanks;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    [ObservableProperty]
    private List<RankComic> dayRanks;


    [ObservableProperty]
    public List<RankComic> weekRanks;


    [ObservableProperty]
    private List<RankComic> monthRanks;


    [ObservableProperty]
    private List<RankComic> selectedRanks;


    private ComicProfile? lastClickedComic = null;


    private void c_Pivot_RankType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedRanks = c_Pivot_RankType.SelectedIndex switch
        {
            0 => DayRanks,
            1 => WeekRanks,
            2 => MonthRanks,
            _ => SelectedRanks,
        };
    }


    private async void c_GridView_RankList_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverBackAnimation");
            if (ani != null)
            {
                if (lastClickedComic != null && (SelectedRanks?.Contains(lastClickedComic) ?? false) && sender is GridView gridView)
                {
                    gridView.ScrollIntoView(lastClickedComic);
                    gridView.UpdateLayout();
                    ani.Configuration = new BasicConnectedAnimationConfiguration();
                    await gridView.TryStartConnectedAnimationAsync(ani, lastClickedComic, "c_Image_ComicCover");
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


    private void c_GridView_RankList_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is ComicProfile comic && sender is GridView gridView)
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

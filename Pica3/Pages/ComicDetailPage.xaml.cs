using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pica3.Controls;
using Pica3.CoreApi.Comic;
using Pica3.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class ComicDetailPage : Page
{



    private static Stack<ComicDetailPageModel?> _vmCaches = new();

    [ObservableProperty]
    private ComicDetailPageModel? _VM;


    public ComicDetailPage()
    {
        this.InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (_vmCaches.TryPop(out var cache))
                {
                    VM = cache;
                }
            }
            else if (e.Parameter is ComicProfile comic)
            {
                c_TextBlock_Title.Text = comic.Title;
                c_HyperlinkButton_Author.Content = comic.Author;
                c_ItemsRepeater_Categories.ItemsSource = comic.Categories;
                c_TextBlock_Views.Text = comic.TotalViews.ToString();
                c_TextBlock_Likes.Text = comic.TotalLikes.ToString();
                if (comic.EpisodeCount + comic.PagesCount != 0)
                {
                    c_TextBlock_EP.Text = $"{comic.EpisodeCount}E / {comic.PagesCount}P";
                }

                VM = ServiceProvider.GetService<ComicDetailPageModel>();
                if (VM != null)
                {
                    VM.Initialize(comic);
                }

                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverAnimation");
                ani?.TryStart(c_Image_ComicCover);
            }
            if (VM != null)
            {
                Loaded += VM.Loaded;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        try
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ComicCoverBackAnimation", c_Image_ComicCover);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (e.NavigationMode != NavigationMode.Back)
        {
            _vmCaches.Push(VM);
        }
        if (VM != null)
        {
            Loaded -= VM.Loaded;
        }
    }



    /// <summary>
    /// 搜索相关内容
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenSearchPage(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is HyperlinkButton button && button.Content is string keyword)
            {
                MainPage.Current.Navigate(typeof(SearchPage), keyword, new DrillInNavigationTransitionInfo());
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    /// <summary>
    /// 查看相关分类
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenCategoryDetailPage(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is HyperlinkButton button && button.Content is string category)
            {
                MainPage.Current.Navigate(typeof(CategoryDetailPage), category, new DrillInNavigationTransitionInfo());
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }







    /// <summary>
    /// 开始阅读
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Button_ComicEpisode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is ComicEpisodeProfile episode)
            {
                MainWindow.Current.SetFullWindowContent(new ComicViewer(VM!.ComicDetailInfo, episode), true);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }









    #region 点击卡片



    private async void ComicProfileGridView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverBackAnimation");
            if (ani != null && VM != null)
            {
                if (VM.LastClickedComic != null && (VM.RecommendComics?.Contains(VM.LastClickedComic) ?? false) && sender is GridView gridView)
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


    #endregion






}

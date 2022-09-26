using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Pica3.Controls;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;
using Scighost.WinUILib.Cache;
using System.Collections.ObjectModel;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class ComicDetailPage : Page
{


    private readonly PicaClient picaClient;

    private string comicId;

    private string? animateCover;

    public ComicDetailPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += ComicDetailPage_Loaded;
    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ComicProfile comic)
        {
            comicId = comic.Id;
            animateCover = comic.Cover?.Url;
            c_TextBlock_Title.Text = comic.Title;
            c_HyperlinkButton_Author.Content = comic.Author;
            c_ItemsRepeater_Categories.ItemsSource = comic.Categories;
            c_TextBlock_Views.Text = comic.TotalViews.ToString();
            c_TextBlock_Likes.Text = comic.TotalLikes.ToString();
            c_TextBlock_EP.Text = $"{comic.EpisodeCount}E / {comic.PagesCount}P";
        }
        var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverAnimation");
        ani?.TryStart(c_Image_ComicCover);
    }


    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.Back && e.SourcePageType != GetType())
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ComicCoverBackAnimation", c_Image_ComicCover);
        }
    }



    private async void ComicDetailPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            Focus(FocusState.Programmatic);
            if (Uri.TryCreate(animateCover, UriKind.RelativeOrAbsolute, out var uri))
            {
                var file = await PicaFileCache.Instance.GetFileFromCacheAsync(uri);
                if (file != null)
                {
                    c_Image_ComicCover.PlaceholderSource = new BitmapImage(new Uri(file.Path));
                }
            }
            if (ComicDetailInfo?.Id != comicId)
            {
                ComicDetailInfo = await picaClient.GetComicDetailAsync(comicId);
                if (ComicDetailInfo.IsLiked)
                {
                    c_FontIcon_Like.Glyph = "\uEB52";
                }
                if (ComicDetailInfo.IsFavourite)
                {
                    c_FontIcon_Star.Glyph = "\uE1CF";
                }
                var pageResult = await picaClient.GetComicEpisodeAsync(comicId, 1);
                EpisodeProfiles = new(pageResult.TList);
                TotalEpisodePage = pageResult.Pages;
                CurrentEpisodePage = pageResult.Page;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }






    /// <summary>
    /// 漫画详情
    /// </summary>
    [ObservableProperty]
    private ComicDetail comicDetailInfo;

    /// <summary>
    /// 漫画章节
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ComicEpisodeProfile> episodeProfiles;


    /// <summary>
    /// 相关推荐
    /// </summary>
    [ObservableProperty]
    private List<ComicProfile> recommendComics;




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
    /// 切换章节评论推荐内容
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void c_Pivot_Section_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var index = c_Pivot_Section.SelectedIndex;
            if (index == 1)
            {
                // 首次加载评论
            }
            if (index == 2 && RecommendComics is null)
            {
                // 首次加载推荐漫画
                RecommendComics = await picaClient.GetRecommendComicsAsync(comicId);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }







    #region 点赞收藏



    /// <summary>
    /// 给漫画点赞
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LikeComicAsync()
    {
        try
        {
            if (await picaClient.LikeComicAsync(comicId))
            {
                c_FontIcon_Like.Glyph = "\uEB52";
            }
            else
            {
                c_FontIcon_Like.Glyph = "\uEB51";
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



    /// <summary>
    /// 收藏漫画
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task StarComicAsync()
    {
        try
        {
            if (await picaClient.AddFavoriteAsync(comicId))
            {
                c_FontIcon_Star.Glyph = "\uE1CF";
            }
            else
            {
                c_FontIcon_Star.Glyph = "\uE1CE";
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }


    #endregion





    #region 章节



    /// <summary>
    /// 全部章节页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextPageVisibility))]
    private int totalEpisodePage;

    /// <summary>
    /// 已加载章节页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextPageVisibility))]
    private int currentEpisodePage;

    /// <summary>
    /// 下一页按键是否可见
    /// </summary>
    public Visibility NextPageVisibility => CurrentEpisodePage < TotalEpisodePage ? Visibility.Visible : Visibility.Collapsed;


    /// <summary>
    /// 加载下一页章节
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LoadNextEpisodePageAsync()
    {
        try
        {
            if (TotalEpisodePage == 0)
            {
                var pageResult = await picaClient.GetComicEpisodeAsync(comicId, 1);
                EpisodeProfiles = new(pageResult.TList);
                TotalEpisodePage = pageResult.Pages;
                CurrentEpisodePage = pageResult.Page;
            }
            else
            {
                if (CurrentEpisodePage < TotalEpisodePage)
                {
                    var pageResult = await picaClient.GetComicEpisodeAsync(comicId, CurrentEpisodePage + 1);
                    pageResult.TList.ForEach(x => EpisodeProfiles?.Add(x));
                    CurrentEpisodePage = pageResult.Page;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    /// <summary>
    /// 加载全部章节
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LoadAllEpisodePageAsync()
    {
        try
        {
            if (TotalEpisodePage == 0)
            {
                var pageResult = await picaClient.GetComicEpisodeAsync(comicId, 1);
                EpisodeProfiles = new(pageResult.TList);
                TotalEpisodePage = pageResult.Pages;
                CurrentEpisodePage = pageResult.Page;
            }
            while (CurrentEpisodePage < TotalEpisodePage)
            {
                var pageResult = await picaClient.GetComicEpisodeAsync(comicId, CurrentEpisodePage + 1);
                pageResult.TList.ForEach(x => EpisodeProfiles?.Add(x));
                CurrentEpisodePage = pageResult.Page;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
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
                MainWindow.Current.SetFullWindowContent(new ComicViewer(comicDetailInfo, episode.Order), true);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }




    #endregion






    #region 评论



    /// <summary>
    /// 全部评论页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextPageVisibility))]
    private int totalCommentPage;

    /// <summary>
    /// 已加载评论页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextPageVisibility))]
    private int currentCommentPage;




















    #endregion






    #region 推荐


    private ComicProfile? lastClickedComic = null;


    private async void c_GridView_RecommendComics_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (lastClickedComic != null)
            {
                c_GridView_RecommendComics.ScrollIntoView(lastClickedComic);
                c_GridView_RecommendComics.UpdateLayout();
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverBackAnimation");
                if (ani != null)
                {
                    ani.Configuration = new BasicConnectedAnimationConfiguration();
                    await c_GridView_RecommendComics.TryStartConnectedAnimationAsync(ani, lastClickedComic, "c_Image_ComicCover");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    private void c_GridView_RecommendComics_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is ComicProfile comic)
            {
                lastClickedComic = comic;
                var ani = c_GridView_RecommendComics.PrepareConnectedAnimation("ComicCoverAnimation", comic, "c_Image_ComicCover");
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

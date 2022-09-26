using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class SearchPage : Page
{


    private static readonly Stack<PageCache> pageCaches = new();


    private readonly PicaClient picaClient;



    public SearchPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += SearchPage_Loaded;
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (pageCaches.TryPop(out var cache))
                {
                    SortTypeIndex = cache.SortTypeIndex;
                    Categories = cache.Categories;
                    ComicList = cache.ComicList;
                    CurrentPage = cache.CurrentPage;
                    keyword = cache.Keyword;
                    lastClickedComic = cache.LastClickedComic;
                    TotalPage = cache.TotalPage;
                }
            }
            else
            {
                if (e.Parameter is string str)
                {
                    keyword = str;
                    ComicList = null;
                    Categories?.ForEach(x => x.IsSelcted = false);
                    c_AutoSuggestBox_Search.Text = keyword;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        try
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                pageCaches.Push(new PageCache
                {
                    SortTypeIndex = this.SortTypeIndex,
                    Categories = this.Categories,
                    ComicList = this.ComicList,
                    CurrentPage = this.CurrentPage,
                    Keyword = this.keyword,
                    LastClickedComic = this.lastClickedComic,
                    TotalPage = this.TotalPage,
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    private async void SearchPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin && ComicList is null)
            {
                SearchAsync();
            }
            if (picaClient.IsLogin && suggestionKeyword is null)
            {
                suggestionKeyword = await picaClient.GetKeywordsAsync();
            }
            if (picaClient.IsLogin && Categories is null)
            {
                var info = await picaClient.GetAppInfoAsync();
                Categories = info.Categories.Select(x => new SearchCategory(x.Title)).ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    private List<string> suggestionKeyword;

    [ObservableProperty]
    private List<SearchCategory> categories;


    [ObservableProperty]
    private int sortTypeIndex;


    [ObservableProperty]
    private int totalPage = 1;



    [ObservableProperty]
    private int currentPage = 1;


    [ObservableProperty]
    private List<ComicProfile>? comicList;


    private ComicProfile? lastClickedComic = null;


    private string? keyword = null;



    partial void OnSortTypeIndexChanged(int value)
    {
        SearchAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        if (value > 0)
        {
            SearchAsync();
        }
    }



    private int randomId;

    private async void SearchAsync()
    {
        try
        {
            if (picaClient.IsLogin)
            {
                var id = Random.Shared.Next();
                randomId = id;
                var cats = Categories?.Where(x => x.IsSelcted).Select(x => x.Category).ToList();
                if (string.IsNullOrWhiteSpace(keyword) && !(cats?.Any() ?? false))
                {
                    var list = await picaClient.GetRandomComicsAsync();
                    if (randomId == id)
                    {
                        TotalPage = 1;
                        CurrentPage = 1;
                        ComicList = list;
                    }
                }
                else
                {
                    var pageResult = await picaClient.AdvanceSearchAsync(keyword?.Trim() ?? "", CurrentPage, (SortType)SortTypeIndex, cats);
                    if (randomId == id)
                    {
                        TotalPage = pageResult.Pages;
                        CurrentPage = pageResult.Page;
                        ComicList = pageResult.TList;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }




    private void c_AutoSuggestBox_Search_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is AutoSuggestBox suggestBox)
            {
                if (string.IsNullOrWhiteSpace(suggestBox.Text))
                {
                    suggestBox.ItemsSource = suggestionKeyword;
                    suggestBox.IsSuggestionListOpen = true;
                }
                else
                {
                    suggestBox.ItemsSource = null;
                    if (VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(suggestBox, 0), 0) is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    private void c_AutoSuggestBox_Search_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        try
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(sender.Text))
                {
                    sender.ItemsSource = suggestionKeyword;
                    sender.IsSuggestionListOpen = true;
                }
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private void c_AutoSuggestBox_Search_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        try
        {
            if (args.SelectedItem?.ToString() is string str)
            {
                keyword = str;
                sender.Text = keyword;
                SearchAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    private void c_AutoSuggestBox_Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        try
        {
            if (args.ChosenSuggestion is null)
            {
                keyword = args.QueryText;
                sender.Text = keyword;
                SearchAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }






    private async void c_GridView_Comics_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("ComicCoverBackAnimation");
            if (ani != null)
            {
                if (lastClickedComic != null && (ComicList?.Contains(lastClickedComic) ?? false) && sender is GridView gridView)
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

    private void c_GridView_Comics_ItemClick(object sender, ItemClickEventArgs e)
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




    private class PageCache
    {

        public int SortTypeIndex { get; set; }

        public List<SearchCategory> Categories { get; set; }


        public int TotalPage { get; set; }


        public int CurrentPage { get; set; }

        public List<ComicProfile>? ComicList { get; set; }

        public ComicProfile? LastClickedComic { get; set; }


        public string? Keyword { get; set; }
    }






}


[INotifyPropertyChanged]
public partial class SearchCategory
{

    [ObservableProperty]
    public bool isSelcted;

    public string Category { get; set; }

    public SearchCategory(string category)
    {
        Category = category;
    }
}
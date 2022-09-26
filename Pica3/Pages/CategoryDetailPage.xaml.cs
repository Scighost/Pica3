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
public sealed partial class CategoryDetailPage : Page
{

    private static readonly Stack<PageCache> pageCaches = new();


    private record PageCache(string CategoryName, int SortTypeIndex, int TotalPage, int CurrentPage, List<ComicProfile>? ComicList, ComicProfile? LastClickedComic);


    private bool dotNotRefresh;


    private readonly PicaClient picaClient;


    [ObservableProperty]
    private string categoryName;




    public CategoryDetailPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += CategoryDetailPage_Loaded;
    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            dotNotRefresh = true;
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (pageCaches.TryPop(out var cache))
                {
                    CategoryName = cache.CategoryName;
                    SortTypeIndex = cache.SortTypeIndex;
                    TotalPage = cache.TotalPage;
                    CurrentPage = cache.CurrentPage;
                    ComicList = cache.ComicList;
                    lastClickedComic = cache.LastClickedComic;
                }
            }
            else
            {
                if (e.Parameter is string str && str != CategoryName)
                {
                    CategoryName = str;
                    TotalPage = 1;
                    CurrentPage = 1;
                    ComicList = null;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        finally
        {
            dotNotRefresh = false;
        }
    }


    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        try
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                pageCaches.Push(new PageCache(CategoryName, SortTypeIndex, TotalPage, CurrentPage, ComicList, lastClickedComic));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    private void CategoryDetailPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (ComicList is null)
        {
            ChangePageAsync();
        }
    }



    [ObservableProperty]
    private int sortTypeIndex;


    [ObservableProperty]
    private int totalPage = 1;



    [ObservableProperty]
    private int currentPage = 1;


    [ObservableProperty]
    private List<ComicProfile>? comicList;


    private ComicProfile? lastClickedComic = null;



    partial void OnSortTypeIndexChanged(int value)
    {
        ChangePageAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        if (value > 0)
        {
            ChangePageAsync();
        }
    }



    private int randomId;

    private async void ChangePageAsync()
    {
        if (dotNotRefresh)
        {
            return;
        }
        try
        {
            if (picaClient.IsLogin)
            {
                var id = Random.Shared.Next();
                randomId = id;
                var pageResult = await picaClient.CategorySearchAsync(CategoryName, CurrentPage, (SortType)SortTypeIndex);
                if (randomId == id)
                {
                    TotalPage = pageResult.Pages;
                    CurrentPage = pageResult.Page;
                    ComicList = pageResult.TList;
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
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






}

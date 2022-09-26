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
public sealed partial class StartPage : Page
{


    private readonly PicaClient picaClient;



    public StartPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += StartPage_Loaded;
    }

    private void StartPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (StarComics is null)
        {
            ChangePage();
        }
    }


    [ObservableProperty]
    private int sortTypeIndex;


    [ObservableProperty]
    private int totalPage = 1;



    [ObservableProperty]
    private int currentPage = 1;


    [ObservableProperty]
    private List<ComicProfile> starComics;


    private ComicProfile? lastClickedComic = null;


    partial void OnSortTypeIndexChanged(int value)
    {
        ChangePage();
    }

    partial void OnCurrentPageChanged(int value)
    {
        ChangePage();
    }

    private int randomId;

    private async void ChangePage()
    {
        try
        {
            if (picaClient.IsLogin)
            {
                var id = Random.Shared.Next();
                randomId = id;
                var pageResult = await picaClient.GetFavouriteAsync((SortType)SortTypeIndex + 1, CurrentPage);
                if (randomId == id)
                {
                    TotalPage = pageResult.Pages;
                    CurrentPage = pageResult.Page;
                    StarComics = pageResult.TList;
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
            if (lastClickedComic != null && sender is GridView gridView)
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
                //MainPage.Current.Navigate(typeof(ComicDetailPage), comic, new DrillInNavigationTransitionInfo());
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }
}

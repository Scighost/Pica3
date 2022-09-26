using CommunityToolkit.Common.Collections;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Controls;

[INotifyPropertyChanged]
public sealed partial class ComicViewer : UserControl
{


    private readonly PicaClient picaClient;

    /// <summary>
    /// 漫画信息
    /// </summary>
    [ObservableProperty]
    private ComicDetail initComic;


    private int initEpisodeId;


    /// <summary>
    /// ScrollView of <see cref="c_ListView_Comics"/>
    /// </summary>
    private ScrollViewer? c_ScrolViewer_ListView = null;



    /// <summary>
    /// 当前窗口图片是第几张
    /// </summary>
    [ObservableProperty]
    private int currentPage;


    [ObservableProperty]
    private int totalPage;


    [ObservableProperty]
    private ObservableCollection<IndexComicImage> comicImageList;





    public ComicViewer(ComicDetail comic, int episodeOrderId)
    {
        this.InitializeComponent();
        initComic = comic;
        initEpisodeId = episodeOrderId;
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += ComicViewer_Loaded;
    }





    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComicViewer_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            InitializeScrollViewerOfListView();
            Focus(FocusState.Programmatic);
            GetMorePagesAsync();
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }




    /// <summary>
    /// 上下滚动控件
    /// </summary>
    private void InitializeScrollViewerOfListView()
    {
        c_ScrolViewer_ListView = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(c_ListView_Comics, 0), 0) as ScrollViewer;
        if (c_ScrolViewer_ListView != null)
        {
            c_ScrolViewer_ListView.ViewChanged += C_ScrolViewer_ListView_ViewChanged;
        }

    }


    /// <summary>
    /// 上下滚动，计算当前是第几张图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void C_ScrolViewer_ListView_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        try
        {
            if (c_ScrolViewer_ListView!.Content is ItemsPresenter presenter)
            {
                if (VisualTreeHelper.GetChildrenCount(presenter) > 1)
                {
                    if (VisualTreeHelper.GetChild(presenter, 1) is ItemsStackPanel panel)
                    {
                        CurrentPage = panel.FirstVisibleIndex + 1;
                        Debug.WriteLine(CurrentPage);
                        return;
                    }
                }
            }
        }
        catch { }
    }



    partial void OnCurrentPageChanged(int value)
    {
        if (value > ComicImageList?.Count - 10)
        {
            GetMorePagesAsync();
        }
    }



    /// <summary>
    /// 正在刷新
    /// </summary>
    private bool isRefresh;


    /// <summary>
    /// 获取更多图片
    /// </summary>
    /// <returns></returns>
    private async void GetMorePagesAsync()
    {
        if (isRefresh)
        {
            return;
        }
        try
        {
            isRefresh = true;
            if (ComicImageList is null)
            {
                var episodeDetail = await picaClient.GetComicEpisodePageAsync(initComic.Id, initEpisodeId);
                TotalPage = episodeDetail.Pages.Total;
                var list = episodeDetail.Pages.TList.Zip(Enumerable.Range(1, episodeDetail.Pages.TList.Count)).Select(x => new IndexComicImage(x.Second, x.First.Image.Url));
                ComicImageList = new(list);
            }
            else
            {
                if (ComicImageList.Count == TotalPage)
                {
                    return;
                }
                else
                {
                    var getCount = ComicImageList.Count / 40 + 1;
                    var episodeDetail = await picaClient.GetComicEpisodePageAsync(initComic.Id, initEpisodeId, getCount);
                    episodeDetail.Pages.TList.ForEach(x => ComicImageList.Add(new IndexComicImage(ComicImageList.Count + 1, x.Image.Url)));
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
        finally
        {
            isRefresh = false;
        }
    }






    [ObservableProperty]
    private bool operationAreaHitTest;


    [ObservableProperty]
    private double operationAreaOpacity;



    /// <summary>
    /// 打开工具栏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_ListView_Comics_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ChangeOperationAreaState();
    }



    /// <summary>
    /// 打开或关闭工具栏
    /// </summary>
    private void ChangeOperationAreaState()
    {
        if (OperationAreaHitTest)
        {
            OperationAreaHitTest = false;
            OperationAreaOpacity = 0;
        }
        else
        {
            c_Grid_ComicInfoCard.Visibility = Visibility.Visible;
            OperationAreaHitTest = true;
            OperationAreaOpacity = 1;
        }
    }





    /// <summary>
    /// 关闭
    /// </summary>
    [RelayCommand]
    private void CloseComicViewer()
    {
        MainWindow.Current.CloseFullWindowContent();
    }


    /// <summary>
    /// 快捷键关闭
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void UserControl_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
    {
        if (args.Key == Windows.System.VirtualKey.Escape && args.Modifiers == Windows.System.VirtualKeyModifiers.None)
        {
            MainWindow.Current.CloseFullWindowContent();
        }
    }

}


public class IndexComicImage
{
    public IndexComicImage(int index, string url)
    {
        Index = index;
        Url = url;
    }

    public int Index { get; set; }

    public string Url { get; set; }

}
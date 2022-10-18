using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pica3.CoreApi.Comic;
using Pica3.Services;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Timer = System.Timers.Timer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Controls;

[INotifyPropertyChanged]
public sealed partial class ComicViewer : UserControl
{


    private readonly PicaService picaService;

    private readonly Timer timer = new(60000);

    /// <summary>
    /// 漫画信息
    /// </summary>
    [ObservableProperty]
    private ComicDetail initComic;

    [ObservableProperty]
    private ComicEpisodeProfile initEpisode;


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


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequestThemeString))]
    private int _ComicViewerTheme = AppSetting.GetValue<int>(SettingKeys.ComicViewerTheme);
    partial void OnComicViewerThemeChanged(int value)
    {
        AppSetting.TrySetValue(SettingKeys.ComicViewerTheme, value);
        this.RequestedTheme = value switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }



    public string RequestThemeString => ComicViewerTheme switch
    {
        0 => "跟随系统",
        1 => "浅色模式",
        2 => "深色模式",
        _ => ""
    };



    [RelayCommand]
    private void ChangeRequestTheme(string value)
    {
        if (int.TryParse(value, out int index))
        {
            ComicViewerTheme = index;
        }
    }




    [ObservableProperty]
    private double _ImageMaxWidth = AppSetting.GetValue<double>(SettingKeys.ComicViewerVerticalScrollMaxWidth, 1000);
    partial void OnImageMaxWidthChanged(double value)
    {
        AppSetting.TrySetValue(SettingKeys.ComicViewerVerticalScrollMaxWidth, value);
        try
        {
            var width = c_ListView_Comics.ActualWidth;
            if (width <= value || value == 0)
            {
                c_ListView_Comics.Padding = new Thickness(0);
            }
            else
            {
                var padding = (width - value) / 2;
                c_ListView_Comics.Padding = new Thickness(padding, 0, padding, 0);
            }
        }
        catch { }
    }



    private void Slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        if (sender is Slider slider)
        {
            ImageMaxWidth = slider.Value;
        }
    }



    public ComicViewer(ComicDetail comic, ComicEpisodeProfile episode)
    {
        this.InitializeComponent();
        this.RequestedTheme = ComicViewerTheme switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
        InitComic = comic;
        InitEpisode = episode;
        picaService = ServiceProvider.GetService<PicaService>()!;
        Loaded += ComicViewer_Loaded;
        Unloaded += ComicViewer_Unloaded;
        timer.Elapsed += Timer_Elapsed;
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
            Focus(FocusState.Programmatic);
            InitializeScrollViewerOfListView();
            GetMorePagesAsync();
            PicaService.SaveReadHistory(initComic.Id, initEpisode.Id, initEpisode.Order, CurrentPage, HistoryType.ReadBegin);
            timer.Start();
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }


    private void ComicViewer_Unloaded(object sender, RoutedEventArgs e)
    {
        PicaService.SaveReadHistory(initComic.Id, initEpisode.Id, initEpisode.Order, CurrentPage, HistoryType.ReadEnd);
        timer.Stop();
    }


    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        PicaService.SaveReadHistory(initComic.Id, initEpisode.Id, initEpisode.Order, CurrentPage, HistoryType.Reading);
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
                        return;
                    }
                }
            }
        }
        catch { }
    }



    private void c_ListView_Comics_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            if (sender is ListView listView)
            {
                var width = e.NewSize.Width;
                if (width <= ImageMaxWidth || ImageMaxWidth == 0)
                {
                    listView.Padding = new Thickness(0);
                }
                else
                {
                    var padding = (width - ImageMaxWidth) / 2;
                    listView.Padding = new Thickness(padding, 0, padding, 0);
                }
            }
        }
        catch { }
    }



    private bool canImageMoved;

    private Point oldPosition;


    private void c_ListView_Comics_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            canImageMoved = true;
            oldPosition = e.GetCurrentPoint(c_ListView_Comics).Position;
        }
        catch { }
    }

    /// <summary>
    /// 鼠标拖动图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_ListView_Comics_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            if (c_ScrolViewer_ListView != null)
            {
                if (canImageMoved)
                {

                    var pointer = e.GetCurrentPoint(c_ListView_Comics);
                    if (pointer.Properties.IsLeftButtonPressed)
                    {
                        var deltaX = pointer.Position.X - oldPosition.X;
                        var deltaY = pointer.Position.Y - oldPosition.Y;
                        oldPosition = pointer.Position;
                        // offset 的方向应与鼠标移动的方向相反
                        // 不要使用 ChangeView，会出现图片无法跟随鼠标的情况
                        c_ScrolViewer_ListView.ScrollToHorizontalOffset(c_ScrolViewer_ListView.HorizontalOffset - deltaX);
                        c_ScrolViewer_ListView.ScrollToVerticalOffset(c_ScrolViewer_ListView.VerticalOffset - deltaY);
                    }
                }
            }
        }
        catch { }
    }

    private void c_ListView_Comics_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            canImageMoved = false;
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
                var episodeDetail = await picaService.GetComicEpisodeImagesAsync(initComic.Id, initEpisode.Order);
                TotalPage = episodeDetail.Images.Total;
                var list = episodeDetail.Images.List.Zip(Enumerable.Range(1, episodeDetail.Images.List.Count)).Select(x => new IndexComicImage(x.Second, x.First.Image.Url));
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
                    var episodeDetail = await picaService.GetComicEpisodeImagesAsync(initComic.Id, initEpisode.Order, getCount);
                    episodeDetail.Images.List.ForEach(x => ComicImageList.Add(new IndexComicImage(ComicImageList.Count + 1, x.Image.Url)));
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
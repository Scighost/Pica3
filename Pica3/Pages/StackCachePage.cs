using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi.Comic;
using Pica3.ViewModels;

namespace Pica3.Pages;


/// <summary>
/// <see cref="Page"/> 不能使用泛型，这个类只是一个参考，真正使用时需要复制其中的代码
/// </summary>
/// <typeparam name="ViewModel"></typeparam>
[INotifyPropertyChanged, Obsolete("不能使用泛型", true)]
public abstract partial class StackCachePage<ViewModel> : Page where ViewModel : ComicProfileViewModelBase
{


    private static Stack<ViewModel?> _vmCaches = new();

    [ObservableProperty]
    private ViewModel? _VM;


    public StackCachePage()
    {
        VM = ServiceProvider.GetService<ViewModel>();
        if (VM != null)
        {
            VM.Initialize();
            Loaded += VM.Loaded;
        }
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.Back)
        {
            if (_vmCaches.TryPop(out var cache))
            {
                VM = cache;
            }
        }
        else if (e.Parameter != null)
        {
            VM = ServiceProvider.GetService<ViewModel>();
            if (VM != null)
            {
                VM.Initialize(e.Parameter);
            }
        }
        if (VM != null)
        {
            Loaded += VM.Loaded;
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





    #region 点击卡片



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


    #endregion











}

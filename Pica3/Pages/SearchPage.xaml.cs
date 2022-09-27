using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi.Comic;
using Pica3.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class SearchPage : Page
{

    private static Stack<SearchPageModel?> _vmCaches = new();


    [ObservableProperty]
    private SearchPageModel? _VM;


    public SearchPage()
    {
        this.InitializeComponent();
        VM = ServiceProvider.GetService<SearchPageModel>();
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
        else if (e.Parameter is string str)
        {
            VM = ServiceProvider.GetService<SearchPageModel>();
            if (VM != null)
            {
                VM.Initialize(str);
            }
            c_AutoSuggestBox_Search.Text = str;
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



    private void c_AutoSuggestBox_Search_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is AutoSuggestBox suggestBox)
            {
                if (string.IsNullOrWhiteSpace(suggestBox.Text))
                {
                    suggestBox.ItemsSource = VM?.SuggestionKeywords;
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
                    sender.ItemsSource = VM?.SuggestionKeywords;
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
                sender.Text = str;
                if (VM != null)
                {
                    VM.Keyword = str;
                    VM.SearchAsync();
                }
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
                var text = args.QueryText;
                sender.Text = text;
                if (VM != null)
                {
                    VM.Keyword = text;
                    VM.SearchAsync();
                }
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



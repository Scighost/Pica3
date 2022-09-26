using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pica3.CoreApi;
using Pica3.CoreApi.App;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class CategoryPage : Page
{


    private readonly PicaClient picaClient;



    public CategoryPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += CategoryPage_Loaded;
    }




    private async void CategoryPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin)
            {
                if (Categories is null)
                {
                    var c = await picaClient.GetHomeCategoriesAsync();
                    Categories = c.Where(x => !x.IsWeb).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }




    [ObservableProperty]
    private List<HomeCategory> categories;




    private void c_GridView_Categories_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is HomeCategory category && !category.IsWeb)
            {
                MainPage.Current.Navigate(typeof(CategoryDetailPage), category.Title, new DrillInNavigationTransitionInfo());
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


}

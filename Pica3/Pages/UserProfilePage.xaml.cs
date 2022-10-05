using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pica3.CoreApi.Account;
using Pica3.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class UserProfilePage : Page
{



    private readonly PicaService picaService;



    public UserProfilePage()
    {
        this.InitializeComponent();
        picaService = ServiceProvider.GetService<PicaService>()!;
        Loaded += UserProfilePage_Loaded;
    }




    private async void UserProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaService.IsLogin)
            {
                MyProfile = await picaService.GetUserProfileAsync();
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }





    [ObservableProperty]
    private UserProfile myProfile;












}

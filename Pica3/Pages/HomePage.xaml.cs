using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class HomePage : Page
{


    private readonly PicaClient picaClient;


    public HomePage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += HomePage_Loaded;
    }







    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin)
            {
                if (RecommendComics is null)
                {
                    RecommendComics = await picaClient.GetRecommendComicsAsync();
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
    private List<RecommendComic> recommendComics;










}

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi;
using Pica3.CoreApi.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class MainPage : Page
{



    public static MainPage Current { get; private set; }



    private readonly PicaClient picaClient;


    public bool IsLogin => picaClient.IsLogin;


    public MainPage()
    {
        Current = this;
        this.InitializeComponent();
        MainWindow.Current.SetTitleBar(AppTitleBar);
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Navigate(typeof(HomePage), null, new SuppressNavigationTransitionInfo());
        c_NavigationView.SelectedItem = c_NavigationViewItem_Home;
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppSetting.TryGetValue<bool>(SettingKeys.NavigationViewPaneClose, out var isClosed))
        {
            c_NavigationView.IsPaneOpen = !isClosed;
        }
        try
        {
            if (picaClient.IsLogin)
            {
                UserProfile = await picaClient.GetUserProfileAsync();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }




    [ObservableProperty]
    private UserProfile userProfile;



    [RelayCommand]
    private async Task RefreshUserProfileAsync()
    {
        try
        {
            if (picaClient.IsLogin)
            {
                UserProfile = await picaClient.GetUserProfileAsync();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



    private void Logout()
    {
        picaClient.Logout();
        MainWindow.Current.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
    }





    #region Navigation




    private void c_NavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        UpdateAppTitleMargin(sender);
        c_Border_AccountImage.Width = 40;
        c_Border_AccountImage.Height = 40;
        c_Border_AccountImage.Margin = new Thickness(4, 4, 4, 0);

    }

    private void c_NavigationView_PaneOpening(NavigationView sender, object args)
    {
        UpdateAppTitleMargin(sender);
        c_Border_AccountImage.Width = 44;
        c_Border_AccountImage.Height = 44;
        c_Border_AccountImage.Margin = new Thickness(16, 0, 0, 0);
    }

    private void c_Border_AccountImage_Tapped(object sender, TappedRoutedEventArgs e)
    {
        c_NavigationView.IsPaneOpen = !c_NavigationView.IsPaneOpen;
        AppSetting.TrySetValue(SettingKeys.NavigationViewPaneClose, !c_NavigationView.IsPaneOpen);
    }


    private void c_NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            AppTitleBar.Margin = new Thickness(sender.CompactPaneLength * 2, 0, 0, 0);
            c_NavigationView.IsPaneToggleButtonVisible = true;
        }
        else
        {
            AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, 0, 0, 0);
            c_NavigationView.IsPaneToggleButtonVisible = false;
        }
        UpdateAppTitleMargin(sender);
    }


    private void UpdateAppTitleMargin(NavigationView sender)
    {
        if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen)
            || sender.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            AppTitle.Translation = new System.Numerics.Vector3(4, 0, 0);
        }
        else
        {
            AppTitle.Translation = new System.Numerics.Vector3(24, 0, 0);
        }
    }



    private void c_NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (c_Frame.CanGoBack)
        {
            c_Frame.GoBack();
            c_NavigationView.SelectedItem = null;
        }
    }


    private void c_Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        e.Handled = true;
    }


    private readonly Dictionary<string, Type> pageTypeDic = new();

    private void c_NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        try
        {
            if (args.InvokedItemContainer.IsSelected)
            {
                return;
            }
            if (args.IsSettingsInvoked)
            {
                Navigate(typeof(SettingPage), null, new DrillInNavigationTransitionInfo());
            }
            else
            {
                var tag = args.InvokedItemContainer.Tag as string;
                if (string.IsNullOrWhiteSpace(tag))
                {
                    return;
                }
                else
                {
                    if (pageTypeDic.TryGetValue(tag, out var type))
                    {
                        Navigate(type, null, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        var asm = typeof(MainPage).Assembly;
                        type = asm.GetType($"Pica3.Pages.{tag}");
                        if (type is not null)
                        {
                            pageTypeDic.TryAdd(tag, type);
                            Navigate(type, null, new DrillInNavigationTransitionInfo());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, $"导肮错误 {args.InvokedItemContainer.Tag}");
            Logger.Error(ex, $"导肮错误 {args.InvokedItemContainer.Tag}");
        }
    }



    public void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (param is null && infoOverride is null)
        {
            c_Frame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            c_Frame.Navigate(sourcePageType, param);
        }
        else
        {
            c_Frame.Navigate(sourcePageType, param, infoOverride);
        }
    }


    #endregion














}

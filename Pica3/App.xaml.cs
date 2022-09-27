using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        InitializeApplicationTheme();
        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Logger.Error(e.Exception, e.Message);
        Logger.CloseAndFlush();
    }

    private void InitializeApplicationTheme()
    {
        if (AppSetting.TryGetValue<int>(SettingKeys.ApplicationTheme, out var themeIndex))
        {
            if (themeIndex == 1)
            {
                RequestedTheme = ApplicationTheme.Light;
            }
            if (themeIndex == 2)
            {
                RequestedTheme = ApplicationTheme.Dark;
            }
        }
    }



    private MainWindow m_window;

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs _)
    {
        var instance = AppInstance.FindOrRegisterForKey("Main");
        if (!instance.IsCurrent)
        {
            await instance.RedirectActivationToAsync(instance.GetActivatedEventArgs());
            Environment.Exit(0);
        }
        else
        {
            instance.Activated += Instance_Activated;
            m_window = new MainWindow();
            m_window.Activate();
        }
    }


    private void Instance_Activated(object? sender, AppActivationArguments e)
    {
        try
        {
            if (m_window is null)
            {
                Environment.Exit(0);
            }
            else
            {
                m_window.DispatcherQueue.TryEnqueue(m_window.MoveToTop);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

}

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pica3.Pages;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{

    public static new MainWindow Current { get; private set; }


    public IntPtr HWND { get; private set; }


    public XamlRoot XamlRoot => Content.XamlRoot;


    public float UIScale => (float)User32.GetDpiForWindow(HWND) / 96;


    public DisplayArea DisplayArea => DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);



    private WindowId windowId;

    private AppWindow appWindow;

    private SystemBackdropHelper backdrop;


    public MainWindow()
    {
        Current = this;
        this.InitializeComponent();
        InitliazeWindow();
        RootFrame.Content = new LoginPage();
        this.Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        SaveWindowState();
    }

    public void InitliazeWindow()
    {
        backdrop = new SystemBackdropHelper(this);
        if (backdrop.TrySetBackdrop())
        {
            BackgroundRectangle.Visibility = Visibility.Collapsed;
        }

        HWND = WindowNative.GetWindowHandle(this);
        windowId = Win32Interop.GetWindowIdFromWindow(HWND);
        appWindow = AppWindow.GetFromWindowId(windowId);

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(WindowTitleBar);

        NotificationProvider.Initialize(InfoBarContainer);

        if (AppSetting.TryGetValue(SettingKeys.IsMainWindowMaximum, out bool isMax) && isMax)
        {
            User32.ShowWindow(HWND, ShowWindowCommand.SW_MAXIMIZE);
            return;
        }
        if (AppSetting.TryGetValue<ulong>(SettingKeys.MainWindowRect, out var value))
        {
            var display = DisplayArea;
            var workAreaWidth = display.WorkArea.Width;
            var workAreaHeight = display.WorkArea.Height;
            var rect = new WindowRect(value);
            if (rect.Left > 0 && rect.Top > 0 && rect.Right < workAreaWidth && rect.Bottom < workAreaHeight)
            {
                appWindow.MoveAndResize(rect.ToRectInt32());
            }
        }
    }



    private void SaveWindowState()
    {
        var wpl = new User32.WINDOWPLACEMENT();
        if (User32.GetWindowPlacement(HWND, ref wpl))
        {
            AppSetting.TrySetValue(SettingKeys.IsMainWindowMaximum, wpl.showCmd == ShowWindowCommand.SW_MAXIMIZE);
            var p = appWindow.Position;
            var s = appWindow.Size;
            var rect = new WindowRect(p.X, p.Y, s.Width, s.Height);
            AppSetting.TrySetValue(SettingKeys.MainWindowRect, rect.Value);
        }
    }


    public void ChangeApplicationTheme(int theme)
    {
        var elementTheme = theme switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
        RootGrid.RequestedTheme = elementTheme;
    }



    /// <summary>
    /// 窗口 Root Frame 导航
    /// </summary>
    /// <param name="sourcePageType"></param>
    /// <param name="param"></param>
    /// <param name="infoOverride"></param>
    public void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (param is null && infoOverride is null)
        {
            RootFrame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            RootFrame.Navigate(sourcePageType, param);
        }
        else
        {
            RootFrame.Navigate(sourcePageType, param, infoOverride);
        }
    }



    /// <summary>
    /// 设置覆盖于窗口上的内容
    /// </summary>
    /// <param name="content"></param>
    /// <param name="collapsed">隐藏底部内容</param>
    public void SetFullWindowContent(Control content, bool collapsed = false)
    {
        FullWindowContent.Visibility = Visibility.Visible;
        FullWindowContent.Content = content;
        if (collapsed)
        {
            RootFrame.Visibility = Visibility.Collapsed;
        }
    }



    /// <summary>
    /// 清除覆盖于窗口上的内容
    /// </summary>
    public void CloseFullWindowContent()
    {
        FullWindowContent.Content = null;
        FullWindowContent.Visibility = Visibility.Collapsed;
        RootFrame.Visibility = Visibility.Visible;
        RootFrame.Focus(FocusState.Programmatic);
    }



    public void MoveToTop()
    {
        User32.ShowWindow(HWND, ShowWindowCommand.SW_SHOWDEFAULT);
        User32.SetForegroundWindow(HWND);
    }



    public bool TryChangeBackdrop(uint value)
    {
        var result = backdrop.TryChangeBackdrop(value, out uint backdropType);
        if (result && backdropType > 0)
        {
            BackgroundRectangle.Visibility = Visibility.Collapsed;
        }
        else
        {
            BackgroundRectangle.Visibility = Visibility.Visible;
        }
        return result;
    }










}



[StructLayout(LayoutKind.Explicit)]
file struct WindowRect
{
    [FieldOffset(0)]
    public short X;

    [FieldOffset(2)]
    public short Y;

    [FieldOffset(4)]
    public short Width;

    [FieldOffset(6)]
    public short Height;

    [FieldOffset(0)]
    public ulong Value;

    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;

    public WindowRect(int x, int y, int width, int height)
    {
        Value = 0;
        X = (short)x;
        Y = (short)y;
        Width = (short)width;
        Height = (short)height;
    }

    public WindowRect(ulong value)
    {
        X = 0;
        Y = 0;
        Width = 0;
        Height = 0;
        Value = value;
    }

    public RectInt32 ToRectInt32()
    {
        return new RectInt32(X, Y, Width, Height);
    }
}
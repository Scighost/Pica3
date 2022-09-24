using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Pickers;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class SettingPage : Page
{
    public SettingPage()
    {
        this.InitializeComponent();
    }





    #region 外观




    /// <summary>
    /// 切换应用模式
    /// </summary>
    [ObservableProperty]
    private int _SelectedThemeIndex = AppSetting.GetValue<int>(SettingKeys.ApplicationTheme);
    partial void OnSelectedThemeIndexChanged(int value)
    {
        AppSetting.TrySetValue(SettingKeys.ApplicationTheme, value);
        MainWindow.Current.ChangeApplicationTheme(value);
    }



    /// <summary>
    /// 窗口背景材质
    /// </summary>
    [ObservableProperty]
    private int _WindowBackdropIndex = (int)AppSetting.GetValue<uint>(SettingKeys.WindowBackdrop) & 0xF;
    partial void OnWindowBackdropIndexChanged(int value)
    {
        ChangeWindowBackdrop((AlwaysActiveBackdrop ? 0x80000000 : 0) | (uint)value);
    }


    /// <summary>
    /// 始终激活背景
    /// </summary>
    [ObservableProperty]
    private bool _AlwaysActiveBackdrop = (AppSetting.GetValue<uint>(SettingKeys.WindowBackdrop) & 0x80000000) > 0;
    partial void OnAlwaysActiveBackdropChanged(bool value)
    {
        ChangeWindowBackdrop((value ? 0x80000000 : 0) | (uint)WindowBackdropIndex);
    }


    /// <summary>
    /// 修改背景材质
    /// </summary>
    /// <param name="value"></param>
    private void ChangeWindowBackdrop(uint value)
    {
        if (MainWindow.Current.TryChangeBackdrop(value))
        {
            AppSetting.TrySetValue(SettingKeys.WindowBackdrop, value);
        }
        else
        {
            AppSetting.TrySetValue(SettingKeys.WindowBackdrop, 0);
            NotificationProvider.Warning("不支持此操作");
        }
    }


    #endregion








    #region 数据




    /// <summary>
    /// 数据文件夹
    /// </summary>
    [ObservableProperty]
    private string _DataFolder = AppSetting.GetValue<string>(SettingKeys.DataFolder) ?? Path.Combine(AppContext.BaseDirectory, "Data");
    partial void OnDataFolderChanged(string value)
    {
        AppSetting.TrySetValue(SettingKeys.DataFolder, value);
    }




    /// <summary>
    /// 缓存文件夹
    /// </summary>
    [ObservableProperty]
    private string _CacheFolder = AppSetting.GetValue<string>(SettingKeys.CacheFolder) ?? Path.Combine(AppContext.BaseDirectory, "Cache");
    partial void OnCacheFolderChanged(string value)
    {
        AppSetting.TrySetValue(SettingKeys.CacheFolder, value);
    }



    /// <summary>
    /// 下载文件夹
    /// </summary>
    [ObservableProperty]
    private string _DownloadFolder = AppSetting.GetValue<string>(SettingKeys.DownloadFolder) ?? Path.Combine(AppContext.BaseDirectory, "Download");
    partial void OnDownloadFolderChanged(string value)
    {
        AppSetting.TrySetValue(SettingKeys.DownloadFolder, value);
    }


    private async Task ChangeDataFolderAsync()
    {
        try
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Current.HWND);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                DataFolder = folder.Path;
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置数据文件夹");
            Logger.Error(ex, "设置数据文件夹");
        }
    }


    private async Task ChangeCacheFolderAsync()
    {
        try
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Current.HWND);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                CacheFolder = folder.Path;
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置缓存文件夹");
            Logger.Error(ex, "设置缓存文件夹");
        }
    }


    private async Task ChangeDownloadFolderAsync()
    {
        try
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Current.HWND);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                DownloadFolder = folder.Path;
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置下载文件夹");
            Logger.Error(ex, "设置下载文件夹");
        }
    }



    /// <summary>
    /// 日志文件夹
    /// </summary>
    public string LogFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pica3\\Log");


    private async Task OpenLogFolderAsync()
    {
        if (!Directory.Exists(LogFolder))
        {
            Directory.CreateDirectory(LogFolder);
        }
        await Launcher.LaunchFolderPathAsync(LogFolder);
    }






    #endregion





}

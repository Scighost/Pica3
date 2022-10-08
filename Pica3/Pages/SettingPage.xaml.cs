using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Pica3.Controls;
using Scighost.WinUILib.Helpers;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Windows.Storage;
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
        Loaded += SettingPage_Loaded;
    }



    private async void SettingPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await GetCacheSizeAsync();
    }




    #region 版本


#if Dev
    public string AppVersion => (typeof(App).Assembly.GetName().Version?.ToString() ?? "-") + " (dev)";
#else
    public string AppVersion => typeof(App).Assembly.GetName().Version?.ToString() ?? "-";
#endif




    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        try
        {
            var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Pica3"));
            var releases = await github.Repository.Release.GetAll("Scighost", "Pica3");
            if (releases.FirstOrDefault() is Octokit.Release release)
            {
                var thisVersion = typeof(MainPage).Assembly.GetName().Version;
                if (Version.TryParse(release.TagName, out var latestVersion))
                {
                    if (latestVersion > thisVersion)
                    {
                        var dialog = new ContentDialog
                        {
                            Content = new UpdateDialog(release)
                            {
                                Width = 500,
                                Height = 624,
                            },
                            DefaultButton = ContentDialogButton.Primary,
                            IsPrimaryButtonEnabled = true,
                            IsSecondaryButtonEnabled = true,
                            PrimaryButtonText = "下载新版本",
                            SecondaryButtonText = "暂不更新",
                            CloseButtonText = "忽略此版本",
                            XamlRoot = MainWindow.Current.XamlRoot,
                        };
                        var result = await dialog.ShowWithZeroMarginAsync();
                        if (result is ContentDialogResult.Primary)
                        {
                            await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl));
                        }
                        if (result is ContentDialogResult.None)
                        {
                            AppSetting.TrySetValue(SettingKeys.IgnoreVersion, release.TagName);
                        }
                        return;
                    }
                }
            }
            NotificationProvider.Success("已是最新版本");
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }










    #endregion






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


    /// <summary>
    /// 缓存大小
    /// </summary>
    [ObservableProperty]
    private string cacheSize;




    /// <summary>
    /// 日志文件夹
    /// </summary>
    [ObservableProperty]
    private string _LogFolder = AppSetting.GetValue<string>(SettingKeys.LogFolder) ?? Path.Combine(AppContext.BaseDirectory, "Log");
    partial void OnLogFolderChanged(string value)
    {
        AppSetting.TrySetValue(SettingKeys.LogFolder, value);
    }


    /// <summary>
    /// 打开文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OpenFolderAsync(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        await Launcher.LaunchFolderPathAsync(folder);
    }


    // todo 修改文件夹位置后，迁移数据库

    /// <summary>
    /// 打开文件夹选择器
    /// </summary>
    /// <returns></returns>
    private async Task<StorageFolder?> OpenFolderPickerAsync()
    {
        var folderPicker = new FolderPicker();
        folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
        folderPicker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Current.HWND);
        return await folderPicker.PickSingleFolderAsync();
    }


    /// <summary>
    /// 修改数据文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeDataFolderAsync()
    {
        try
        {
            var folder = await OpenFolderPickerAsync();
            if (folder is not null)
            {
                var databaseFile = Path.Combine(folder.Path, "PicaData.db");
                if (File.Exists(databaseFile))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = """
                        目标文件夹存在数据文件，如何处理？

                        覆盖：删除目标文件夹的文件，将当前文件夹的文件移动至目标文件夹；
                        跳过：使用目标文件夹已存在的文件；
                        取消：取消修改数据文件夹。
                        """,
                        XamlRoot = MainWindow.Current.XamlRoot,
                        IsPrimaryButtonEnabled = true,
                        IsSecondaryButtonEnabled = true,
                        DefaultButton = ContentDialogButton.Close,
                        PrimaryButtonText = "覆盖",
                        SecondaryButtonText = "跳过",
                        CloseButtonText = "取消",
                    };
                    var result = await dialog.ShowWithZeroMarginAsync();
                    if (result is ContentDialogResult.None)
                    {
                        return;
                    }
                    if (result is ContentDialogResult.Primary)
                    {
                        File.Copy(DatabaseProvider.SqlitePath, databaseFile, true);
                    }
                }
                else
                {
                    File.Copy(DatabaseProvider.SqlitePath, databaseFile, true);
                }
                DataFolder = folder.Path;
                DatabaseProvider.Reset();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置数据文件夹");
            Logger.Error(ex, "设置数据文件夹");
        }
    }



    /// <summary>
    /// 修改缓存文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeCacheFolderAsync()
    {
        try
        {
            var folder = await OpenFolderPickerAsync();
            if (folder is not null)
            {
                PicaFileCache.Instance.Initialize(folder);
                var oldFolder = CacheFolder;
                CacheFolder = folder.Path;
                await Task.Run(() =>
                {
                    try
                    {
                        var files = Directory.GetFiles(oldFolder);
                        foreach (var file in files)
                        {
                            var dest = Path.Combine(CacheFolder, Path.GetFileName(file));
                            File.Move(file, dest, true);
                        }
                        Directory.Delete(oldFolder, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置缓存文件夹");
            Logger.Error(ex, "设置缓存文件夹");
        }
    }


    /// <summary>
    /// 获取缓存大小
    /// </summary>
    /// <returns></returns>
    private async Task GetCacheSizeAsync()
    {
        try
        {
            if (Directory.Exists(CacheFolder))
            {
                long totalSize = 0;
                await Task.Run(() =>
                {
                    var files = new DirectoryInfo(CacheFolder).GetFiles("*", SearchOption.AllDirectories);
                    totalSize = files.Sum(x => x.Length);
                });
                CacheSize = $"{((double)totalSize) / (1 << 20):F2} MB";
            }
            else
            {
                CacheSize = "0 MB";
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            CacheSize = "无法计算";
        }
    }


    /// <summary>
    /// 清理缓存
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(CacheFolder);
            if (folder is null)
            {
                Directory.CreateDirectory(CacheFolder);
            }
            else
            {
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                Directory.CreateDirectory(CacheFolder);
                CacheSize = "0 MB";
                NotificationProvider.Success("清理完成");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    /// <summary>
    /// 修改下载文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeDownloadFolderAsync()
    {
        try
        {
            var folder = await OpenFolderPickerAsync();
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
    /// 修改日志文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeLogFolderAsync()
    {
        try
        {
            var folder = await OpenFolderPickerAsync();
            if (folder is not null)
            {
                Logger.CloseAndFlush();
                var oldFolder = LogFolder;
                LogFolder = folder.Path;
                await Task.Run(() =>
                {
                    try
                    {
                        var files = Directory.GetFiles(oldFolder);
                        foreach (var file in files)
                        {
                            var dest = Path.Combine(LogFolder, Path.GetFileName(file));
                            File.Move(file, dest, true);
                        }
                        Directory.Delete(oldFolder, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置日志文件夹");
            Logger.Error(ex, "设置日志文件夹");
        }
    }











    #endregion




    /// <summary>
    /// 应用单例模式
    /// </summary>
    [ObservableProperty]
    private bool _ApplicationSingleton = AppSetting.GetValue<bool>(SettingKeys.EnableApplicationSingleton);
    partial void OnApplicationSingletonChanged(bool value)
    {
        AppSetting.TrySetValue(SettingKeys.EnableApplicationSingleton, value);
    }



    [RelayCommand]
    private async Task CheckDevVersionAsync()
    {
        try
        {
            const string URL = "https://os.scighost.com/pica3/build/bika3_latest_x64.zip";
            const string SCRIPT = """
                try {
                    Write-Host "哔咔 3 开发版下载脚本"
                    Write-Host "注意：开发版基于最新的代码，不是正式发布的版本，可能存在功能缺失，Bug 频出的问题。" -ForegroundColor Yellow
                    Write-Host "`n`n`n"
                    Write-Host "当前路径：$(Get-Location)"
                    if(![System.IO.File]::Exists('./7Zip4Powershell/7Zip4Powershell.psd1')) {
                        Write-Host "下载解压模块"
                        Invoke-WebRequest "https://os.scighost.com/common/tool/7Zip4Powershell.zip" -UseBasicParsing -OutFile "./7Zip4Powershell.zip" -ErrorAction Stop
                        Expand-Archive -Path "./7Zip4Powershell.zip" -DestinationPath "./" -Force -ErrorAction Stop
                        Remove-Item -Path "./7Zip4Powershell.zip" -Force -Recurse -ErrorAction Stop
                    }
                    Import-Module -Name "./7Zip4Powershell/7Zip4Powershell.psd1" -Force -ErrorAction Stop
                    $null = New-Item "./temp" -ItemType "Directory" -Force -ErrorAction Stop
                    $result = Invoke-WebRequest "https://os.scighost.com/pica3/build/bika3_latest_x64.7z" -UseBasicParsing -Method HEAD -ErrorAction Stop
                    $version = $result.Headers['x-oss-meta-version']
                    $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
                    Write-Host "下载安装包 `(v$version`, $size MB`)"
                    Invoke-WebRequest "https://os.scighost.com/pica3/build/bika3_latest_x64.7z" -UseBasicParsing -OutFile "./temp/bika3_latest.7z" -ErrorAction Stop
                    Write-Host "开始解压"
                    Expand-7Zip -ArchiveFileName "./temp/bika3_latest.7z" -TargetPath "./temp/" -ErrorAction Stop
                    try {
                        $null = Get-Process -Name bika3 -ErrorAction Stop
                        Write-Host "bika3.exe 正在运行，等待进程退出" -ForegroundColor Yellow
                        Wait-Process -Name bika3 -ErrorAction Stop
                        Start-Sleep -Seconds 1
                    } catch { }
                    Write-Host "替换文件"
                    Get-ChildItem -Path "./temp/bika3/*" -File | Move-Item -Destination "./" -Force -ErrorAction Stop
                    Copy-Item -Path "./temp/bika3/*" -Destination "./" -Force -Recurse -ErrorAction Stop
                    Write-Host "更新完成"
                    Invoke-Item -Path "./bika3.exe" -ErrorAction Stop
                    Write-Host "清理安装包"
                    Remove-Item -Path "./temp" -Force -Recurse -ErrorAction Stop
                    Start-Sleep -Seconds 1
                } catch {
                    Write-Host $_.Exception -ForegroundColor Red -BackgroundColor Black
                    Write-Host "`n更新失败，可以从以下链接手动下载最新开发版：" -ForegroundColor Yellow
                    Write-Host "https://os.scighost.com/pica3/build/bika3_latest_x64.7z`n" -ForegroundColor Yellow
                    Write-Host "任意键退出"
                    [Console]::ReadKey()
                }
                """;
            var replace = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => false,
                Architecture.Arm64 => true,
                _ => throw new PlatformNotSupportedException($"不支持 {RuntimeInformation.ProcessArchitecture}"),
            };
            var url = replace ? URL.Replace("x64", "arm64") : URL;
            var result = await new HttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (result.Headers.TryGetValues("x-oss-meta-version", out var values))
            {
                if (Version.TryParse(values.FirstOrDefault(), out var version))
                {
                    if (version > typeof(App).Assembly.GetName().Version)
                    {
                        var dialog = new ContentDialog
                        {
                            Title = $"新版本 {version}",
                            Content = "开发版不是正式发布的版本，可能存在功能缺失，Bug 频出的问题。\n下载过程中可继续使用，关闭应用后自动更新。",
                            IsPrimaryButtonEnabled = true,
                            PrimaryButtonText = "下载",
                            CloseButtonText = "取消",
                            DefaultButton = ContentDialogButton.Close,
                            XamlRoot = MainWindow.Current.XamlRoot,
                        };
                        if (await dialog.ShowWithZeroMarginAsync() is ContentDialogResult.Primary)
                        {
                            Process.Start("PowerShell", replace ? SCRIPT.Replace("x64", "arm64") : SCRIPT);
                        }
                        return;
                    }
                }
            }
            NotificationProvider.Success("已是最新版本");
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }



}

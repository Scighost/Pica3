using CommunityToolkit.WinUI.Notifications;
using Microsoft.UI.Xaml.Controls;
using NuGet.Versioning;
using Octokit;
using Pica3.Controls;
using Scighost.WinUILib.Helpers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using Windows.System;
using Windows.UI.Notifications;

namespace Pica3.Services;

internal static class UpdateService
{

    public static readonly SemanticVersion? AppVersion;

    private static readonly GitHubClient GitHubClient;

    private static readonly HttpClient HttpClient;

    private static bool addClosedHandle;



    static UpdateService()
    {
        try
        {
            var location = typeof(App).Assembly.Location;
            if (File.Exists(location))
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(location);
                AppVersion = SemanticVersion.Parse(fileVersion.ProductVersion);
            }
            else
            {
                var path = Path.Combine(AppContext.BaseDirectory, "bika3.exe");
                if (File.Exists(path))
                {
                    var fileVersion = FileVersionInfo.GetVersionInfo(location);
                    AppVersion = SemanticVersion.Parse(fileVersion.ProductVersion);
                }
            }
        }
        catch { }
        GitHubClient = new GitHubClient(new ProductHeaderValue("Pica3"));
        HttpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });
        HttpClient.DefaultRequestHeaders.Add("User-Agent", $"Pica3/{AppVersion}");
    }


    public static async Task CheckUpdateAsync(bool showResult = false)
    {
        var devChannel = AppSetting.GetValue<bool>(SettingKeys.EnableDevChannel);
        SemanticVersion.TryParse(AppSetting.GetValue<string>(SettingKeys.IgnoreVersion), out var ignoreVersion);
        SemanticVersion? devVersion = null, releaseVersoin = null;
        Release? release = null;

        if (devChannel)
        {
            devVersion = await GetDevVersionAsync();
            try
            {
                (releaseVersoin, release) = await GetReleaseVersionAsytnc();
            }
            catch
            {
                if (devVersion is null)
                {
                    throw;
                }
            }
        }
        else
        {
            (releaseVersoin, release) = await GetReleaseVersionAsytnc();
        }

        if (devChannel)
        {
            if (devVersion > AppVersion && devVersion > releaseVersoin)
            {
                if (showResult || (devVersion > ignoreVersion))
                {
                    await ShowDevUpdateAsync(devVersion!);
                    return;
                }
            }
            else if (releaseVersoin > AppVersion)
            {
                if (showResult || (releaseVersoin > ignoreVersion))
                {
                    await ShowReleaseUpdateAsync(release!);
                    return;
                }
            }
        }
        else
        {
            if (releaseVersoin > AppVersion)
            {
                if (showResult || (releaseVersoin > ignoreVersion))
                {
                    await ShowReleaseUpdateAsync(release!);
                    return;
                }
            }
        }

        if (showResult)
        {
            NotificationProvider.Success("已是最新版本");
        }
    }




    private static async Task<(SemanticVersion? Version, Release? Release)> GetReleaseVersionAsytnc()
    {
        var releases = await GitHubClient.Repository.Release.GetAll("Scighost", "Pica3");
        SemanticVersion? version = null;
        var release = releases.FirstOrDefault();
        if (release != null)
        {
            string versionStr = release.TagName.FirstOrDefault() switch
            {
                'V' or 'v' => release.TagName[1..],
                _ => release.TagName,
            };
            SemanticVersion.TryParse(versionStr, out version);
        }
        return (version, release);
    }




    private static async Task<SemanticVersion?> GetDevVersionAsync()
    {
        var url = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "https://os.scighost.com/pica3/build/bika3_latest_x64.7z",
            Architecture.Arm64 => "https://os.scighost.com/pica3/build/bika3_latest_arm64.7z",
            _ => throw new PlatformNotSupportedException($"不支持 {RuntimeInformation.ProcessArchitecture}"),
        };
        var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        SemanticVersion? version = null;
        if (response.Headers.TryGetValues("x-oss-meta-version", out var values))
        {
            string? versionStr = values.FirstOrDefault()?.FirstOrDefault() switch
            {
                'V' or 'v' => values.FirstOrDefault()?[1..],
                _ => values.FirstOrDefault(),
            };
            SemanticVersion.TryParse(versionStr, out version);
        }
        return version;
    }




    private static async Task ShowReleaseUpdateAsync(Release release)
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
            PrimaryButtonText = "下载安装包",
            SecondaryButtonText = "暂不更新",
            CloseButtonText = "忽略此版本",
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        var result = await dialog.ShowWithZeroMarginAsync();
        if (result is ContentDialogResult.Primary)
        {
            AppSetting.TrySetValue(SettingKeys.IgnoreVersion, "");
            await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl));
        }
        if (result is ContentDialogResult.None)
        {
            AppSetting.TrySetValue(SettingKeys.IgnoreVersion, release.TagName);
        }
    }




    private static async Task ShowDevUpdateAsync(SemanticVersion version)
    {
        var url = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "https://os.scighost.com/pica3/build/bika3_latest_x64.7z",
            Architecture.Arm64 => "https://os.scighost.com/pica3/build/bika3_latest_arm64.7z",
            _ => throw new PlatformNotSupportedException($"不支持 {RuntimeInformation.ProcessArchitecture}"),
        };
        var dialog = new ContentDialog
        {
            Title = $"开发版 v{version}",
            Content = "开发版不是正式发布的版本，可能存在功能缺失，Bug 频出的问题。\n\n下载过程中可继续使用，关闭应用后自动更新。",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "下载并安装",
            SecondaryButtonText = "暂不更新",
            CloseButtonText = "忽略此版本",
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        var result = await dialog.ShowWithZeroMarginAsync();
        if (result is ContentDialogResult.Primary)
        {
            AppSetting.TrySetValue(SettingKeys.IgnoreVersion, "");
            Directory.CreateDirectory("./temp");
            var manager = ToastNotificationManagerCompat.CreateToastNotifier();
#pragma warning disable SYSLIB0014 // 类型或成员已过时
            var client = new WebClient();
#pragma warning restore SYSLIB0014 // 类型或成员已过时
            var toast = SendToast(manager);
            client.DownloadProgressChanged += (_, e) => UpdateToast(manager, e.BytesReceived, e.TotalBytesToReceive);
            client.DownloadFileCompleted += (_, e) =>
            {
                manager.Hide(toast);
                if (e.Error != null)
                {
                    Logger.Error(e.Error, "下载安装包时遇到错误");
                    new ToastContentBuilder().AddText("下载安装包时遇到错误").AddText("查看日志了解详细原因").Show();
                }
                else
                {
                    if (!addClosedHandle)
                    {
                        new ToastContentBuilder().AddText("下载完成").AddText("关闭应用后开始更新").Show();
                        MainWindow.Current.Closed += Current_Closed;
                        addClosedHandle = true;
                    }
                }
            };
            client.DownloadFileAsync(new Uri(url), "./temp/bika3_latest.7z");
        }
        if (result is ContentDialogResult.None)
        {
            AppSetting.TrySetValue(SettingKeys.IgnoreVersion, version);
        }
    }



    private static void Current_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        Process.Start("PowerShell", SCRIPT);
        ToastNotificationManagerCompat.History.Clear();
    }

    private static ToastNotification SendToast(ToastNotifierCompat manager)
    {
        var content = new ToastContentBuilder().AddText("下载安装包").AddVisualChild(new AdaptiveProgressBar()
        {
            //Title = new BindableString("progressTitle"),
            Value = new BindableProgressBarValue("progressValue"),
            ValueStringOverride = new BindableString("progressValueString"),
            Status = new BindableString("progressStatus")
        }).AddToastActivationInfo("DownloadDevVersion", ToastActivationType.Background).GetToastContent();

        var toast = new ToastNotification(content.GetXml());
        toast.Tag = "DownloadDevVersion";
        toast.Group = "bika3Download";
        toast.Data = new NotificationData();
        //toast.Data.Values["progressTitle"] = "0 MB";
        toast.Data.Values["progressValue"] = "0";
        toast.Data.Values["progressValueString"] = "0%";
        toast.Data.Values["progressStatus"] = "0 MB";
        toast.Data.SequenceNumber = 0;
        manager.Show(toast);
        return toast;
    }


    private static void UpdateToast(ToastNotifierCompat manager, long downloadedSize, long totalSize)
    {
        var data = new NotificationData();
        //data.Values["progressTitle"] = $"{(double)totalSize / (1 << 20):F2} MB";
        data.Values["progressValue"] = $"{(double)downloadedSize / totalSize}";
        data.Values["progressValueString"] = $"{(double)downloadedSize / totalSize:P1}";
        data.Values["progressStatus"] = $"{(double)downloadedSize / (1 << 20):F2} / {(double)totalSize / (1 << 20):F2} MB";
        manager.Update(data, "DownloadDevVersion", "bika3Download");
    }





    const string SCRIPT = """
        try {
            $ErrorActionPreference = 'Stop'
            $ProgressPreference = 'SilentlyContinue'
            Write-Host "哔咔 3 开发版更新脚本"
            Write-Host "注意：开发版基于最新的代码，不是正式发布的版本，可能存在功能缺失，Bug 频出的问题。" -ForegroundColor Yellow
            Write-Host "当前路径：$(Get-Location)"
            if(![System.IO.File]::Exists('./bika3.exe')) {
                Write-Host "没有找到已安装的版本" -ForegroundColor Yellow
                $archi = (Get-WmiObject WIN32_PROCESSOR).Architecture
                if($archi -eq 5) {
                    $fn = 'bika3_latest_arm64.7z'
                } else {
                    $fn = 'bika3_latest_x64.7z'
                }
                $result = Invoke-WebRequest "https://os.scighost.com/pica3/build/$fn" -UseBasicParsing -Method HEAD
                $version = $result.Headers['x-oss-meta-version']
                $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
                Write-Host "下载安装包 `(v$version`, $size MB`)"
                Write-Host "是否显示下载进度条（显示进度条会减慢下载速度）"
                Write-Host '[Y] 是(Y) ' -NoNewline
                Write-Host ' [N] 否(N)' -NoNewline -ForegroundColor Yellow
                Write-Host ' (默认值为 N ):' -NoNewline
                $read = Read-Host
                if($read -eq 'Y' -or $read -eq 'y') {
                    $ProgressPreference = 'Continue'
                }
                Write-Host "下载中，请稍等。。。"
                Invoke-WebRequest "https://os.scighost.com/pica3/build/$fn" -UseBasicParsing -OutFile "./$fn"
                Write-Host "下载完成，文件名：$fn"
                Write-Host "任意键退出"
                [Console]::ReadKey()
                Exit
            }
            if(![System.IO.File]::Exists('./temp/bika3_latest.7z')) {
                Write-Host "安装包不存在" -ForegroundColor Yellow
                $null = New-Item "./temp" -ItemType "Directory" -Force
                $archi = (Get-WmiObject WIN32_PROCESSOR).Architecture
                if($archi -eq 5) {
                    $url = 'https://os.scighost.com/pica3/build/bika3_latest_arm64.7z'
                } else {
                    $url = 'https://os.scighost.com/pica3/build/bika3_latest_x64.7z'
                }
                $result = Invoke-WebRequest -Uri $url -UseBasicParsing -Method HEAD
                $version = $result.Headers['x-oss-meta-version']
                $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
                Write-Host "下载安装包 `(v$version`, $size MB`)"
                Write-Host "是否显示下载进度条（显示进度条会减慢下载速度）"
                Write-Host '[Y] 是(Y) ' -NoNewline
                Write-Host ' [N] 否(N)' -NoNewline -ForegroundColor Yellow
                Write-Host ' (默认值为 N ):' -NoNewline
                $read = Read-Host
                if($read -eq 'Y' -or $read -eq 'y') {
                    $ProgressPreference = 'Continue'
                }
                Write-Host "下载中，请稍等。。。"
                Invoke-WebRequest -Uri $url -UseBasicParsing -OutFile "./temp/bika3_latest.7z"
            }
            if(![System.IO.File]::Exists('./7Zip4Powershell/7Zip4Powershell.psd1')) {
                Write-Host "下载解压模块"
                Invoke-WebRequest "https://os.scighost.com/common/tool/7Zip4Powershell.zip" -UseBasicParsing -OutFile "./7Zip4Powershell.zip"
                Expand-Archive -Path "./7Zip4Powershell.zip" -DestinationPath "./" -Force
                Remove-Item -Path "./7Zip4Powershell.zip" -Force -Recurse
            }
            Import-Module -Name "./7Zip4Powershell/7Zip4Powershell.psd1" -Force
            Write-Host "开始解压"
            Expand-7Zip -ArchiveFileName "./temp/bika3_latest.7z" -TargetPath "./temp/"
            try {
                $null = Get-Process -Name bika3
                Write-Host "bika3.exe 正在运行，等待进程退出" -ForegroundColor Yellow
                Wait-Process -Name bika3
                Start-Sleep -Seconds 1
            } catch { }
            Write-Host "替换文件"
            Get-ChildItem -Path "./temp/bika3/*" -File | Move-Item -Destination "./" -Force
            Copy-Item -Path "./temp/bika3/*" -Destination "./" -Force -Recurse
            Write-Host "更新完成"
            Invoke-Item -Path "./bika3.exe"
            Write-Host "清理安装包"
            Remove-Item -Path "./temp" -Force -Recurse
            Start-Sleep -Seconds 1
        } catch {
            Write-Host $_.Exception -ForegroundColor Red -BackgroundColor Black
            $archi = (Get-WmiObject WIN32_PROCESSOR).Architecture
            if($archi -eq 5) {
                $url = 'https://os.scighost.com/pica3/build/bika3_latest_arm64.7z'
            } else {
                $url = 'https://os.scighost.com/pica3/build/bika3_latest_x64.7z'
            }
            Write-Host "`n更新失败，请解压并替换「安装路径/temp」文件夹中的安装包，或重新下载最新开发版：" -ForegroundColor Yellow
            Write-Host "$url`n" -ForegroundColor Yellow
            Write-Host "任意键退出"
            [Console]::ReadKey()
        }
        """;





}

﻿using CommunityToolkit.Mvvm.Input;
using Dapper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using Pica3.CoreApi;
using Pica3.CoreApi.Account;
using Scighost.WinUILib.Helpers;
using System.Net;
using System.Net.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class LoginPage : Page
{


    private readonly PicaClient picaClient;



    public LoginPage()
    {
        this.InitializeComponent();
        picaClient = ServiceProvider.GetService<PicaClient>()!;
        Loaded += LoginPage_Loaded;
    }


    /// <summary>
    /// 不要自动登录
    /// </summary>
    private bool doNotAutoLogin;


    /// <summary>
    /// 从主页返回值不要自动登录
    /// </summary>
    /// <param name="e"></param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        doNotAutoLogin = true;
    }



    private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Delay(1);
            using var dapper = DatabaseProvider.CreateConnection();
            var accounts = dapper.Query<PicaAccount>("SELECT * FROM PicaAccount;").ToList();
            accounts.ForEach(x => x.Decrypt());
            Accounts = accounts.Where(x => x.IsDecrypted).ToList();
            var selectedItem = Accounts.FirstOrDefault();
            if (Accounts.FirstOrDefault(x => x.Selected) is PicaAccount a)
            {
                selectedItem = a;
            }
            if (selectedItem != null)
            {
                c_Combox_Account.SelectedItem = selectedItem;
                if (!string.IsNullOrWhiteSpace(selectedItem.Password))
                {
                    c_PasswordBox_Password.Password = selectedItem.Password;
                    c_CheckBox_RememberPassword.IsChecked = true;
                }
                if (selectedItem.AutoLogin)
                {
                    c_CheckBox_AutoLogin.IsChecked = true;
                    if (!doNotAutoLogin)
                    {
                        LoginCommand.Execute(null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    /// <summary>
    /// 所有账号
    /// </summary>
    [ObservableProperty]
    private List<PicaAccount> _accounts;


    /// <summary>
    /// 登录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LoginAsync()
    {
        var account = c_Combox_Account.Text;
        var password = c_PasswordBox_Password.Password;
        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }
        try
        {
            c_Combox_Account.IsEnabled = false;
            c_PasswordBox_Password.IsEnabled = false;
            c_CheckBox_RememberPassword.IsEnabled = false;
            c_CheckBox_AutoLogin.IsEnabled = false;
            picaClient.Logout();
            if (await picaClient.LoginAsync(account, password))
            {
                using var dapper = DatabaseProvider.CreateConnection();
                var model = new PicaAccount { Account = account, Selected = true };
                if (c_CheckBox_RememberPassword.IsChecked ?? false)
                {
                    model.Password = password;
                }
                model.AutoLogin = c_CheckBox_AutoLogin.IsChecked ?? false;
                await Task.Run(() =>
                {
                    try
                    {
                        model.Encrypt();
                        dapper.Execute("UPDATE PicaAccount SET Selected=False WHERE True;");
                        dapper.Execute("INSERT OR REPLACE INTO PicaAccount VALUES (@Account, @Password, @Selected, @AutoLogin);", model);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        NotificationProvider.Error(ex);
                    }
                });
                MainWindow.Current.Navigate(typeof(MainPage));
            }
            else
            {
                NotificationProvider.Warning("未知错误", 1500);
            }
        }
        //catch (PicaApiException ex)
        //{
        //    Logger.Error(ex);
        //    NotificationProvider.Error(ex);
        //}
        //catch (HttpRequestException ex)
        //{
        //    Logger.Error(ex);
        //    NotificationProvider.Warning("网络错误", 1500);
        //}
        //catch (TaskCanceledException ex)
        //{
        //    Logger.Error(ex);
        //    NotificationProvider.Warning("连接超时", 1500);
        //}
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
        finally
        {
            c_Combox_Account.IsEnabled = true;
            c_PasswordBox_Password.IsEnabled = true;
            c_CheckBox_RememberPassword.IsEnabled = true;
            c_CheckBox_AutoLogin.IsEnabled = true;
        }
    }



    private void AutoLoginChecked()
    {
        if (c_CheckBox_AutoLogin.IsChecked ?? false)
        {
            c_CheckBox_RememberPassword.IsChecked = true;
        }
    }



    private void ChangeAccount()
    {
        if (c_Combox_Account.SelectedItem is PicaAccount account)
        {
            c_PasswordBox_Password.Password = account.Password;
            if (string.IsNullOrWhiteSpace(account.Password))
            {
                c_CheckBox_RememberPassword.IsChecked = false;
            }
            else
            {
                c_CheckBox_RememberPassword.IsChecked = true;
            }
            c_CheckBox_AutoLogin.IsChecked = account.AutoLogin;
        }
    }



    /// <summary>
    /// 注册信息
    /// </summary>
    private RegisterAccountRequest registerAccountRequest;


    /// <summary>
    /// 暂不登录
    /// </summary>
    private void SkipLogin()
    {
        MainWindow.Current.Navigate(typeof(MainPage));
    }


    /// <summary>
    /// 打开代理界面
    /// </summary>
    private void ShowProxySettingFlyout()
    {
        c_TextBlock_ProxyError.Text = "";
        c_TextBox_Proxy.Text = AppSetting.GetValue<string>(SettingKeys.WebProxy, "");
        FlyoutBase.ShowAttachedFlyout(c_Button_SetProxy);
    }


    /// <summary>
    /// 设置代理
    /// </summary>
    private void SetWebProxy()
    {
        try
        {
            var proxyText = c_TextBox_Proxy.Text;
            if (string.IsNullOrWhiteSpace(proxyText))
            {
                ServiceProvider.ChangeProxy(null);
                AppSetting.SetValue(SettingKeys.WebProxy, "");
                c_TextBlock_ProxyError.Text = "已清除代理设置";
            }
            else
            {
                var proxy = new WebProxy(proxyText);
                ServiceProvider.ChangeProxy(proxy);
                AppSetting.SetValue(SettingKeys.WebProxy, proxyText);
                c_TextBlock_ProxyError.Text = "已设置代理";
            }
        }
        catch (UriFormatException ex)
        {
            Logger.Error(ex);
            c_TextBlock_ProxyError.Text = "输入值无效";
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            c_TextBlock_ProxyError.Text = "未知错误";
        }
    }



    /// <summary>
    /// 打开注册界面
    /// </summary>
    private void OpenRegisterTeachingTip()
    {
        if (registerAccountRequest is null)
        {
            registerAccountRequest = new RegisterAccountRequest();
        }
        c_TeachingTip_Register.IsOpen = true;
    }



    /// <summary>
    /// 注册
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task RegisterAccountAsync()
    {
        try
        {
            var request = registerAccountRequest;
            await picaClient.RegisterAccountAsync(request);
            NotificationProvider.Success("注册成功");
            c_Combox_Account.Text = request.Account;
            c_PasswordBox_Password.Password = request.Password;
            c_TeachingTip_Register.IsOpen = false;
        }
        catch (PicaApiException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Warning("网络错误", 1500);
        }
        catch (TaskCanceledException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Warning("连接超时", 1500);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }




    private int forgotQuestionIndex = 0;


    /// <summary>
    /// 打开忘记密码界面
    /// </summary>
    private void OpenForgotPasswordTeachingTip()
    {
        c_TeachingTip_ForgotPassword.IsOpen = true;
    }


    /// <summary>
    /// 重置密码
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ResetAccountPasswordAsync()
    {
        var account = c_TextBox_ForgotPassword_Account.Text;
        if (string.IsNullOrWhiteSpace(account))
        {
            return;
        }
        try
        {
            if (forgotQuestionIndex == 0)
            {
                var question = await picaClient.GetAccountSecurityQuestionAsync(account);
                forgotQuestionIndex = Random.Shared.Next(2) + 1;
                c_TextBlock_ForgotPassword_Question.Text = forgotQuestionIndex switch
                {
                    1 => question.Question1,
                    2 => question.Question2,
                    3 => question.Question3,
                    _ => question.Question1,
                };
                c_TextBlock_ForgotPassword_Question.Visibility = Visibility.Visible;
                c_TextBox_ForgotPassword_Answer.Visibility = Visibility.Visible;
            }
            else
            {
                var question = c_TextBlock_ForgotPassword_Question.Text;
                var answer = c_TextBox_ForgotPassword_Answer.Text;
                var newPassword = await picaClient.ResetPasswordAsync(account, forgotQuestionIndex, answer);
                ClipboardHelper.SetText(newPassword);
                NotificationProvider.Success("已重置密码", "新密码已复制到剪贴板，请登录后再次修改", 5000);
                c_Combox_Account.Text = account;
                c_PasswordBox_Password.Password = newPassword;
                c_TeachingTip_ForgotPassword.IsOpen = false;
            }
        }
        catch (PicaApiException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Warning("网络错误", 1500);
        }
        catch (TaskCanceledException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Warning("网络错误", 1500);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }







}

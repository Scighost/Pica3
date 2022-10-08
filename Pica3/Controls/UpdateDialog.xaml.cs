using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Octokit;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Controls;

public sealed partial class UpdateDialog : UserControl
{


    private readonly Release release;



    public UpdateDialog(Release release)
    {
        this.InitializeComponent();
        this.release = release;
        Loaded += UpdateDialog_Loaded;
    }

    private async void UpdateDialog_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {release.Name}");
            sb.AppendLine();
            sb.AppendLine($"> {release.TagName}{(release.Prerelease ? " | 预览版" : "")} | {release.PublishedAt?.LocalDateTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine(release.Body);
            sb.AppendLine();

            var html = Markdig.Markdown.ToHtml(sb.ToString());
            var css = (int)ActualTheme switch
            {
                1 => "https://os.scighost.com/pica3/app/github-markdown-light_5.1.0.css",
                2 => "https://os.scighost.com/pica3/app/github-markdown-dark_5.1.0.css",
                _ => "https://os.scighost.com/pica3/app/github-markdown_5.1.0.css",
            };
            html = $$"""
                <!DOCTYPE html>
                <html>
                <head>
                <base target="_blank">
                <link rel="stylesheet" href="{{css}}">
                <meta name="color-scheme" content="light dark">
                <style>
                body::-webkit-scrollbar {display: none;}
                </style>
                </head>
                <body style="background-color: transparent;">
                <article class="markdown-body" style="background-color: transparent;">
                {{html}}
                </article>
                </body>
                </html>
                """;
            await webview.EnsureCoreWebView2Async();
            webview.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webview.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            webview.NavigateToString(html);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }
}

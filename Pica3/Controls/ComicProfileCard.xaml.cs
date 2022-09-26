using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pica3.CoreApi.Comic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Controls;

public sealed partial class ComicProfileCard : UserControl
{




    public ComicProfile ComicProfile
    {
        get { return (ComicProfile)GetValue(ComicProfileProperty); }
        set { SetValue(ComicProfileProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ComicProfile.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ComicProfileProperty =
        DependencyProperty.Register("ComicProfile", typeof(ComicProfile), typeof(ComicProfileCard), new PropertyMetadata(null));



    public ComicProfileCard()
    {
        this.InitializeComponent();
    }



}

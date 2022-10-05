using Microsoft.UI.Xaml;

namespace Pica3.ViewModels;

internal interface IViewModel
{
    void Initialize(object? param = null);

    void Loaded(object sender, RoutedEventArgs e);
}

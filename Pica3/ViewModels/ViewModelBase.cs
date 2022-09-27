using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pica3.ViewModels;

internal interface IViewModel
{
    void Initialize(object? param = null);

    void Loaded(object sender, RoutedEventArgs e);
}

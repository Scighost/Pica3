using Microsoft.UI.Xaml;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;

namespace Pica3.ViewModels;

public abstract partial class ComicProfileViewModelBase : ObservableObject
{

    private PicaClient picaClient;


    public ComicProfileViewModelBase(PicaClient picaClient)
    {
        this.picaClient = picaClient;
    }


    [ObservableProperty]
    private int sortTypeIndex;


    [ObservableProperty]
    private int totalPage = 1;


    [ObservableProperty]
    private int currentPage = 1;


    [ObservableProperty]
    private List<ComicProfile>? comicList;


    public ComicProfile? LastClickedComic { get; set; }



    public abstract void Initialize(object? param = null);



    public abstract void Loaded(object sender, RoutedEventArgs e);


}

using Microsoft.UI.Xaml;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;

namespace Pica3.ViewModels;

public sealed partial class FavoritePageModel : ObservableObject, IViewModel
{


    private readonly PicaClient picaClient;


    public FavoritePageModel(PicaClient picaClient)
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



    public void Initialize(object? param = null)
    {

    }

    public void Loaded(object sender, RoutedEventArgs e)
    {
        if (ComicList is null)
        {
            ChangePageAsync();
        }
    }


    partial void OnSortTypeIndexChanged(int value)
    {
        ChangePageAsync();
    }


    partial void OnCurrentPageChanged(int value)
    {
        if (value > 0)
        {
            ChangePageAsync();
        }
    }



    private int randomId;

    private async void ChangePageAsync()
    {
        try
        {
            if (picaClient.IsLogin)
            {
                var id = Random.Shared.Next();
                randomId = id;
                var pageResult = await picaClient.GetFavouriteAsync((SortType)SortTypeIndex + 1, CurrentPage);
                if (randomId == id)
                {
                    TotalPage = pageResult.Pages;
                    CurrentPage = pageResult.Page;
                    ComicList = pageResult.TList;
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }
}

using Microsoft.UI.Xaml;
using Pica3.CoreApi.Comic;
using Pica3.Services;

namespace Pica3.ViewModels;

public sealed partial class FavoritePageModel : ObservableObject, IViewModel
{



    private readonly PicaService picaService;


    public FavoritePageModel(PicaService picaService)
    {
        this.picaService = picaService;
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
            if (picaService.IsLogin)
            {
                var id = Random.Shared.Next();
                randomId = id;
                var pageResult = await picaService.GetFavouriteComicAsync((SortType)SortTypeIndex + 1, CurrentPage);
                if (randomId == id)
                {
                    TotalPage = pageResult.Pages;
                    CurrentPage = pageResult.Page;
                    ComicList = pageResult.List;
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }
}

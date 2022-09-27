using Microsoft.UI.Xaml;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;
using Pica3.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pica3.ViewModels;

public sealed partial class CategoryDetailPageModel : ObservableObject, IViewModel
{

    private readonly PicaClient picaClient;


    public CategoryDetailPageModel(PicaClient picaClient)
    {
        this.picaClient = picaClient;
    }


    [ObservableProperty]
    private string categoryName;

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
        if (param is string str)
        {
            CategoryName = str;
        }
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
                var pageResult = await picaClient.CategorySearchAsync(CategoryName, CurrentPage, (SortType)SortTypeIndex);
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

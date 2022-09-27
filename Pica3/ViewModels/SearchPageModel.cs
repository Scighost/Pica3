using Microsoft.UI.Xaml;
using Pica3.CoreApi.Comic;
using Pica3.CoreApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pica3.ViewModels;

public sealed partial class SearchPageModel : ObservableObject, IViewModel
{

    private readonly PicaClient picaClient;


    public SearchPageModel(PicaClient picaClient)
    {
        this.picaClient = picaClient;
    }


    public List<string> SuggestionKeywords { get; set; }


    [ObservableProperty]
    private List<SearchCategory> categories;


    [ObservableProperty]
    private int sortTypeIndex;


    [ObservableProperty]
    private int totalPage = 1;


    [ObservableProperty]
    private int currentPage = 1;


    [ObservableProperty]
    private List<ComicProfile>? comicList;


    public ComicProfile? LastClickedComic { get; set; }


    public string Keyword { get; set; }



    public void Initialize(object? param = null)
    {
        if (param is string str)
        {
            Keyword = str;
        }
    }

    public async void Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin && ComicList is null)
            {
                SearchAsync();
            }
            if (picaClient.IsLogin && SuggestionKeywords is null)
            {
                SuggestionKeywords = await picaClient.GetKeywordsAsync();
            }
            if (picaClient.IsLogin && Categories is null)
            {
                var info = await picaClient.GetAppInfoAsync();
                Categories = info.Categories.Select(x => new SearchCategory(x.Title)).ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    partial void OnSortTypeIndexChanged(int value)
    {
        SearchAsync();
    }


    partial void OnCurrentPageChanged(int value)
    {
        if (value > 0)
        {
            SearchAsync();
        }
    }



    private int randomId;

    public async void SearchAsync()
    {
        try
        {
            if (picaClient.IsLogin)
            {
                var id = Random.Shared.Next();
                randomId = id;
                var cats = Categories?.Where(x => x.IsChecked).Select(x => x.Category).ToList();
                if (string.IsNullOrWhiteSpace(Keyword) && !(cats?.Any() ?? false))
                {
                    var list = await picaClient.GetRandomComicsAsync();
                    if (randomId == id)
                    {
                        TotalPage = 1;
                        CurrentPage = 1;
                        ComicList = list;
                    }
                }
                else
                {
                    var pageResult = await picaClient.AdvanceSearchAsync(Keyword?.Trim() ?? "", CurrentPage, (SortType)SortTypeIndex, cats);
                    if (randomId == id)
                    {
                        TotalPage = pageResult.Pages;
                        CurrentPage = pageResult.Page;
                        ComicList = pageResult.TList;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }


}


public sealed partial class SearchCategory : ObservableObject
{

    [ObservableProperty]
    private bool isChecked;

    public string Category { get; set; }

    public SearchCategory(string category)
    {
        Category = category;
    }
}
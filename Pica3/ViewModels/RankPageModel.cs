using Microsoft.UI.Xaml;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;

namespace Pica3.ViewModels;

public sealed partial class RankPageModel : ObservableObject, IViewModel
{

    private PicaClient picaClient;


    public RankPageModel(PicaClient picaClient)
    {
        this.picaClient = picaClient;
    }


    [ObservableProperty]
    private int rankTypeIndex;


    [ObservableProperty]
    private List<RankComic> dayRanks;


    [ObservableProperty]
    public List<RankComic> weekRanks;


    [ObservableProperty]
    private List<RankComic> monthRanks;


    [ObservableProperty]
    private List<RankComic>? comicList;


    public ComicProfile? LastClickedComic { get; set; }




    public void Initialize(object? param = null)
    {

    }


    public async void Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (picaClient.IsLogin)
            {
                DayRanks ??= await picaClient.GetRankComicsAsync(RankType.H24);
                WeekRanks ??= await picaClient.GetRankComicsAsync(RankType.D7);
                MonthRanks ??= await picaClient.GetRankComicsAsync(RankType.D30);
                ComicList ??= DayRanks;
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }



    partial void OnRankTypeIndexChanged(int value)
    {
        ComicList = value switch
        {
            0 => DayRanks,
            1 => WeekRanks,
            2 => MonthRanks,
            _ => ComicList,
        };
    }


}

using Microsoft.UI.Xaml;
using Pica3.CoreApi.Comic;
using Pica3.Services;

namespace Pica3.ViewModels;

public sealed partial class RankPageModel : ObservableObject, IViewModel
{


    private readonly PicaService picaService;


    public RankPageModel(PicaService picaService)
    {
        this.picaService = picaService;
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
            if (picaService.IsLogin)
            {
                DayRanks ??= await picaService.GetRankComicsAsync(RankType.H24);
                WeekRanks ??= await picaService.GetRankComicsAsync(RankType.D7);
                MonthRanks ??= await picaService.GetRankComicsAsync(RankType.D30);
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

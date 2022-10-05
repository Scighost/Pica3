using Dapper;
using Microsoft.Data.Sqlite;
using Pica3.CoreApi;
using Pica3.CoreApi.Account;
using System.IO;

namespace Pica3.Helpers;

internal static class DatabaseProvider
{

    private static string _sqlitePath;

    private static string _sqliteConnectionString;

    private static bool _initialized;

    public static string SqlitePath => _sqlitePath;


    private static readonly List<string> UpdateSqls = new() { TableStructure_v1 };



    static DatabaseProvider()
    {
        SqlMapper.AddTypeHandler(new DapperSqlMapper.DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new DapperSqlMapper.JsonSerializerHandler<PicaFile>());
        SqlMapper.AddTypeHandler(new DapperSqlMapper.JsonSerializerHandler<UserProfile>());
        SqlMapper.AddTypeHandler(new DapperSqlMapper.JsonSerializerHandler<List<string>>());
    }




    private static void Initialize()
    {
        if (!_initialized)
        {
            var dataFolder = AppSetting.GetValue<string>(SettingKeys.DataFolder) ?? Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            _sqlitePath = Path.Combine(dataFolder, "PicaData.db");
            _sqliteConnectionString = $"Data Source={_sqlitePath};";
            using var dapper = new SqliteConnection(_sqliteConnectionString);
            dapper.Open();
            dapper.Execute("PRAGMA JOURNAL_MODE = WAL;");
            var version = dapper.QueryFirstOrDefault<int>("PRAGMA USER_VERSION;");
            if (version < UpdateSqls.Count)
            {
                foreach (var sql in UpdateSqls.Skip(version))
                {
                    dapper.Execute(sql);
                }
            }
            _initialized = true;
        }
    }


    public static void Reset()
    {
        _initialized = false;
    }



    public static SqliteConnection CreateConnection()
    {
        if (!_initialized)
        {
            Initialize();
        }
        var dapper = new SqliteConnection(_sqliteConnectionString);
        dapper.Open();
        return dapper;
    }







    private const string TableStructure_v1 = """
        BEGIN TRANSACTION;
        CREATE TABLE IF NOT EXISTS PicaAccount
        (
            Account   TEXT NOT NULL PRIMARY KEY,
            Password  TEXT,
            Selected  INTEGER,
            AutoLogin INTEGER
        );
        CREATE TABLE IF NOT EXISTS ComicDetail
        (
            Id             TEXT NOT NULL PRIMARY KEY,
            Title          TEXT,
            Author         TEXT,
            ChineseTeam    TEXT,
            Description    TEXT,
            EpisodeCount   INTEGER,
            PagesCount     INTEGER,
            CreatedAt      TEXT,
            UpdatedAt      TEXT,
            Finished       INTEGER,
            Cover          TEXT,
            Creator        TEXT,
            Categories     TEXT,
            Tags           TEXT,
            TotalViews     INTEGER,
            TotalLikes     INTEGER,
            LikesCount     INTEGER,
            ViewsCount     INTEGER,
            CommentsCount  INTEGER,
            AllowDownload  INTEGER,
            AllowComment   INTEGER,
            IsLiked        INTEGER,
            IsFavourite    INTEGER,
            LastUpdateTime TEXT
        );
        CREATE INDEX IF NOT EXISTS ComicDetail_Title_index ON ComicDetail (Title);
        CREATE INDEX IF NOT EXISTS ComicDetail_Author_index ON ComicDetail (Author);
        CREATE INDEX IF NOT EXISTS ComicDetail_ChineseTeam_index ON ComicDetail (ChineseTeam);
        CREATE INDEX IF NOT EXISTS ComicDetail_EpisodeCount_index ON ComicDetail (EpisodeCount);
        CREATE INDEX IF NOT EXISTS ComicDetail_PagesCount_index ON ComicDetail (PagesCount);
        CREATE INDEX IF NOT EXISTS ComicDetail_CreatedAt_index ON ComicDetail (CreatedAt);
        CREATE INDEX IF NOT EXISTS ComicDetail_UpdatedAt_index ON ComicDetail (UpdatedAt);
        CREATE INDEX IF NOT EXISTS ComicDetail_Finished_index ON ComicDetail (Finished);
        CREATE INDEX IF NOT EXISTS ComicDetail_ViewsCountId_index ON ComicDetail (ViewsCount);
        CREATE INDEX IF NOT EXISTS ComicDetail_LikesCount_index ON ComicDetail (LikesCount);
        CREATE INDEX IF NOT EXISTS ComicDetail_IsLiked_index ON ComicDetail (IsLiked);
        CREATE INDEX IF NOT EXISTS ComicDetail_IsFavourite_index ON ComicDetail (IsFavourite);
        CREATE INDEX IF NOT EXISTS ComicDetail_LastUpdateTime_index ON ComicDetail (LastUpdateTime);
        CREATE TABLE IF NOT EXISTS ComicCategory
        (
            ComicId  TEXT,
            Category TEXT,
            PRIMARY KEY (ComicId, Category)
        );
        CREATE INDEX IF NOT EXISTS ComicCategory_Category_index ON ComicCategory (Category);
        CREATE TABLE IF NOT EXISTS ComicTag
        (
            ComicId TEXT,
            Tag     TEXT,
            PRIMARY KEY (ComicId, Tag)
        );
        CREATE INDEX IF NOT EXISTS ComicTag_Tag_index ON ComicTag (Tag);
        CREATE TABLE IF NOT EXISTS ComicEpisode
        (
            Id        TEXT NOT NULL PRIMARY KEY,
            ComicId   TEXT,
            Title     TEXT,
            "Order"   INTEGER,
            PageCount INTEGER,
            UpdatedAt TEXT
        );
        CREATE INDEX IF NOT EXISTS ComicEpisode_ComicId_index ON ComicEpisode (ComicId);
        CREATE INDEX IF NOT EXISTS ComicEpisode_Order_index ON ComicEpisode ("Order");
        CREATE TABLE IF NOT EXISTS ComicEpisodeImage
        (
            Id         TEXT NOT NULL PRIMARY KEY,
            EpisodeId  TEXT,
            ImageIndex INTEGER,
            Image      TEXT
        );
        CREATE INDEX IF NOT EXISTS ComicEpisodeImage_EpisodeId_index ON ComicEpisodeImage (EpisodeId);
        CREATE INDEX IF NOT EXISTS ComicEpisodeImage_ImageIndex_index ON ComicEpisodeImage (ImageIndex);
        CREATE TABLE IF NOT EXISTS ReadHistory
        (
            Id           INTEGER PRIMARY KEY AUTOINCREMENT,
            ComicId      TEXT,
            EpisodeId    TEXT,
            EpisodeIndex INTEGER,
            ImageIndex   INTEGER,
            DateTime     TEXT,
            HistoryType  INTEGER
        );
        CREATE INDEX IF NOT EXISTS ReadHistory_ComicId_index ON ReadHistory (ComicId);
        CREATE INDEX IF NOT EXISTS ReadHistory_EpisodeId_index ON ReadHistory (EpisodeId);
        CREATE INDEX IF NOT EXISTS ReadHistory_DateTime_index ON ReadHistory (DateTime);
        CREATE INDEX IF NOT EXISTS ReadHistory_HistoryType_index ON ReadHistory (HistoryType);
        CREATE TABLE IF NOT EXISTS SearchHistory
        (
            Id       INTEGER PRIMARY KEY AUTOINCREMENT,
            Keyword  TEXT,
            DateTime TEXT
        );
        CREATE INDEX IF NOT EXISTS SearchHistory_Keyword_index ON SearchHistory (Keyword);
        CREATE INDEX IF NOT EXISTS SearchHistory_DateTime_index ON SearchHistory (DateTime);
        CREATE TABLE IF NOT EXISTS BlockKeyword
        (
            Keyword TEXT NOT NULL PRIMARY KEY,
            Enabled INTEGER,
            Allowed INTEGER
        );
        PRAGMA USER_VERSION = 1;
        COMMIT TRANSACTION;
        """;



}

using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;

namespace Pica3.Helpers;

internal static class DatabaseProvider
{

    private static string _sqlitePath;

    private static string _sqliteConnectionString;

    private static bool _initialized;


    private const int DATABASE_VERSION = 0;

    public static string SqlitePath => _sqlitePath;




    static DatabaseProvider()
    {
        SqlMapper.AddTypeHandler(new DapperSqlMapper.DateTimeOffsetHandler());
    }




    private static void Initialize()
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
        dapper.Execute(TableStructure_Init);
        var version = dapper.QueryFirstOrDefault<int>("SELECT Value FROM DatabaseVersion WHERE Key='DatabaseVersion' LIMIT 1;");
        if (version < DATABASE_VERSION)
        {
            var updatingSqls = GetUpdatingSqls(version);
            foreach (var sql in updatingSqls)
            {
                dapper.Execute(sql);
            }
        }
        _initialized = true;
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



    private static List<string> GetUpdatingSqls(int version)
    {
        return version switch
        {
            0 => new List<string> { TableStructure_v1, TableStructure_v2 },
            1 => new List<string> { TableStructure_v2 },
            _ => new List<string> { },
        };
    }




    private const string TableStructure_Init = "";
    private const string TableStructure_v1 = "";
    private const string TableStructure_v2 = "";



}

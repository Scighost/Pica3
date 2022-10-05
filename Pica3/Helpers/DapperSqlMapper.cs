using Dapper;
using System.Data;
using System.Text.Json;

namespace Pica3.Helpers;

internal class DapperSqlMapper
{

    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };



    public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            if (value is string str)
            {
                return DateTimeOffset.Parse(str);
            }
            else
            {
                return new DateTimeOffset();
            }
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToString();
        }
    }



    public class JsonSerializerHandler<T> : SqlMapper.TypeHandler<T>
    {
        public override T Parse(object value)
        {
            if (value is string { Length: > 0 } str)
            {
                return JsonSerializer.Deserialize<T>(str, JsonSerializerOptions)!;
            }
            else
            {
                return default!;
            }
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = JsonSerializer.Serialize(value, JsonSerializerOptions);
        }
    }


}

using System.Text.Json.Serialization;
using LimDB.lib.Objects.Base;

namespace LimDB.lib.Json
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<BaseObject>))]
    public partial class LimDbJsonContext : JsonSerializerContext
    {
    }
}

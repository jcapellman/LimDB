using System.Text.Json.Serialization;
using LimDB.Tests.Objects;

namespace LimDB.Tests.Json
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<Posts>))]
    [JsonSerializable(typeof(IReadOnlyList<Posts>))]
    public partial class TestJsonContext : JsonSerializerContext
    {
    }
}

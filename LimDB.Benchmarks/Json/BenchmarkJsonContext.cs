using System.Text.Json.Serialization;
using LimDB.Benchmarks.Objects;

namespace LimDB.Benchmarks.Json
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<BenchmarkPost>))]
    public partial class BenchmarkJsonContext : JsonSerializerContext
    {
    }
}

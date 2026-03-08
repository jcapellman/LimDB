using System.Text.Json.Serialization;
using LimDB.Benchmarks.Objects;

namespace LimDB.Benchmarks.Json
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<BenchmarkPost>))]
    [JsonSerializable(typeof(BenchmarkPost[]))]
    [JsonSerializable(typeof(IReadOnlyList<BenchmarkPost>))]
    public partial class BenchmarkJsonContext : JsonSerializerContext
    {
    }
}

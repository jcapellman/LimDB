using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Benchmarks comparing DateTime.Now vs DateTime.UtcNow
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class DateTimeBenchmarks
    {
        [Benchmark(Baseline = true, Description = "DateTime.Now (with timezone)")]
        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }

        [Benchmark(Description = "DateTime.UtcNow (no timezone)")]
        public DateTime GetDateTimeUtcNow()
        {
            return DateTime.UtcNow;
        }

        [Benchmark(Description = "DateTime.Now - 1000 calls")]
        public void DateTimeNow_Multiple()
        {
            for (int i = 0; i < 1000; i++)
            {
                _ = DateTime.Now;
            }
        }

        [Benchmark(Description = "DateTime.UtcNow - 1000 calls")]
        public void DateTimeUtcNow_Multiple()
        {
            for (int i = 0; i < 1000; i++)
            {
                _ = DateTime.UtcNow;
            }
        }
    }
}

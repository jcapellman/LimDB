using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;
using Microsoft.VSDiagnostics;

namespace LimDB.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [CPUUsageDiagnoser]
    public class InitializationOptimizationBenchmarks
    {
        private List<BenchmarkPost> _sourceData = null!;
        [Params(100, 1000, 10000, 100000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _sourceData = new List<BenchmarkPost>(DatasetSize);
            for (int i = 1; i <= DatasetSize; i++)
            {
                _sourceData.Add(new BenchmarkPost { Id = i, Title = $"Post {i}", Body = $"Body {i}", Category = $"Cat {i % 10}", URL = $"https://example.com/{i}", PostDate = DateTime.UtcNow, Active = true, Created = DateTime.UtcNow, Modified = DateTime.UtcNow });
            }
        }

        [Benchmark(Baseline = true, Description = "BEFORE: Init two-pass O(2n) - ToDictionary + Max")]
        public int Before_Init_TwoPass()
        {
            // Pass 1: Build dictionary - O(n)
            var idIndex = _sourceData.ToDictionary(p => p.Id);
            // Pass 2: Find max - O(n)
            var maxId = _sourceData.Count == 0 ? 0 : _sourceData.Max(p => p.Id);
            return idIndex.Count + maxId; // Return value to prevent dead code elimination
        }

        [Benchmark(Description = "AFTER: Init single-pass O(n)")]
        public int After_Init_SinglePass()
        {
            // Single pass: Build all indexes and find max - O(n)
            var idIndex = new Dictionary<int, BenchmarkPost>(_sourceData.Count);
            var positionIndex = new Dictionary<int, int>(_sourceData.Count);
            var maxId = 0;
            for (var i = 0; i < _sourceData.Count; i++)
            {
                var obj = _sourceData[i];
                idIndex[obj.Id] = obj;
                positionIndex[obj.Id] = i;
                if (obj.Id > maxId)
                {
                    maxId = obj.Id;
                }
            }

            return idIndex.Count + positionIndex.Count + maxId; // Return value to prevent dead code elimination
        }
    }
}
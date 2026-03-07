using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Quick benchmarks focusing on the key optimizations
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class QuickOptimizationsBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private Dictionary<int, BenchmarkPost> _postIndex = null!;
        private int _maxId;

        [Params(1000, 10000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _posts = new List<BenchmarkPost>(DatasetSize);
            for (int i = 1; i <= DatasetSize; i++)
            {
                _posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post {i}",
                    Body = $"Body {i}",
                    Category = $"Cat{i % 10}",
                    URL = $"url{i}",
                    PostDate = DateTime.UtcNow,
                    Active = true,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                });
            }

            _postIndex = _posts.ToDictionary(p => p.Id);
            _maxId = DatasetSize;
        }

        // Optimization 1: MaxId for Insert
        [Benchmark(Baseline = true, Description = "Before: Max on objects")]
        public int Opt1_Before_MaxOnObjects()
        {
            return _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
        }

        [Benchmark(Description = "Middle: Max on dict keys")]
        public int Opt1_Middle_MaxOnDictKeys()
        {
            return _postIndex.Count == 0 ? 1 : _postIndex.Keys.Max() + 1;
        }

        [Benchmark(Description = "After: Cached maxId field")]
        public int Opt1_After_CachedMaxId()
        {
            return _maxId + 1;
        }

        // Optimization 2: ID Lookup
        [Benchmark(Description = "Before: FirstOrDefault")]
        public BenchmarkPost? Opt2_Before_FirstOrDefault()
        {
            return _posts.FirstOrDefault(p => p.Id == DatasetSize / 2);
        }

        [Benchmark(Description = "After: Dictionary TryGetValue")]
        public BenchmarkPost? Opt2_After_DictionaryLookup()
        {
            return _postIndex.TryGetValue(DatasetSize / 2, out var p) ? p : null;
        }

        // Optimization 3: Existence Check
        [Benchmark(Description = "Before: FindIndex != -1")]
        public bool Opt3_Before_FindIndex()
        {
            return _posts.FindIndex(p => p.Id == DatasetSize / 2) != -1;
        }

        [Benchmark(Description = "After: ContainsKey")]
        public bool Opt3_After_ContainsKey()
        {
            return _postIndex.ContainsKey(DatasetSize / 2);
        }
    }
}

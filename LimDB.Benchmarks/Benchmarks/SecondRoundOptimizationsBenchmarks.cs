using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Benchmarks the additional optimizations:
    /// 1. Cached MaxId field vs calculating Max() on every insert
    /// 2. Dictionary ContainsKey check vs FindIndex for existence checks
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class SecondRoundOptimizationsBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private Dictionary<int, BenchmarkPost> _postIndex = null!;
        private int _maxId;

        [Params(100, 1000, 10000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _posts = new List<BenchmarkPost>();
            for (int i = 1; i <= DatasetSize; i++)
            {
                _posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post Title {i}",
                    Body = $"Body content {i}",
                    Category = $"Category {i % 10}",
                    URL = $"https://example.com/post/{i}",
                    PostDate = DateTime.UtcNow.AddDays(-i),
                    Active = true,
                    Created = DateTime.UtcNow.AddDays(-i),
                    Modified = DateTime.UtcNow.AddDays(-i)
                });
            }

            _postIndex = _posts.ToDictionary(p => p.Id);
            _maxId = DatasetSize;
        }

        // ===== Max ID Generation (for Insert) =====

        [Benchmark(Baseline = true, Description = "BEFORE: Max() on dictionary keys")]
        public int Before_GetMaxId_DictionaryKeys()
        {
            return _postIndex.Count == 0 ? 1 : _postIndex.Keys.Max() + 1;
        }

        [Benchmark(Description = "AFTER: Cached _maxId field")]
        public int After_GetMaxId_CachedField()
        {
            return _maxId + 1;
        }

        [Benchmark(Description = "OLD: Max() on full objects")]
        public int Old_GetMaxId_FullObjects()
        {
            return _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
        }

        // ===== Existence Check (for Update) =====

        [Benchmark(Description = "ContainsKey check (O(1))")]
        public bool After_ContainsKeyCheck()
        {
            return _postIndex.ContainsKey(DatasetSize / 2);
        }

        [Benchmark(Description = "FindIndex check (O(n))")]
        public bool Before_FindIndexCheck()
        {
            return _posts.FindIndex(p => p.Id == DatasetSize / 2) != -1;
        }

        // ===== DateTime Comparison =====

        [Benchmark(Description = "DateTime.Now")]
        public DateTime GetDateTime_Now()
        {
            return DateTime.Now;
        }

        [Benchmark(Description = "DateTime.UtcNow")]
        public DateTime GetDateTime_UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}

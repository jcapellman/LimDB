using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Simulates the old vs new implementation to show performance improvements
    /// This benchmark uses in-memory collections to isolate the lookup performance
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class BeforeAfterComparisonBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private Dictionary<int, BenchmarkPost> _postIndex = null!;
        private int[] _searchIds = null!;

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
            
            // Test with random IDs
            var random = new Random(42);
            _searchIds = Enumerable.Range(0, 100)
                .Select(_ => random.Next(1, DatasetSize + 1))
                .ToArray();
        }

        // ===== BEFORE (Linear Search) =====

        [Benchmark(Baseline = true, Description = "BEFORE: GetOneById (linear search)")]
        public BenchmarkPost? Before_GetOneById()
        {
            return _posts.FirstOrDefault(p => p.Id == _searchIds[0]);
        }

        [Benchmark(Description = "BEFORE: 100 random ID lookups (linear)")]
        public int Before_MultipleIdLookups()
        {
            int count = 0;
            foreach (var id in _searchIds)
            {
                if (_posts.FirstOrDefault(p => p.Id == id) != null)
                {
                    count++;
                }
            }
            return count;
        }

        [Benchmark(Description = "BEFORE: Get max ID for insert (linear)")]
        public int Before_GetMaxIdForInsert()
        {
            return _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
        }

        [Benchmark(Description = "BEFORE: Delete by ID (linear search)")]
        public bool Before_DeleteById()
        {
            var id = _searchIds[0];
            var obj = _posts.FirstOrDefault(a => a.Id == id);
            if (obj == null) return false;
            
            _posts.Remove(obj);
            _posts.Add(obj); // Re-add to keep dataset consistent
            return true;
        }

        // ===== AFTER (Dictionary Lookup) =====

        [Benchmark(Description = "AFTER: GetOneById (dictionary lookup)")]
        public BenchmarkPost? After_GetOneById()
        {
            return _postIndex.TryGetValue(_searchIds[0], out var post) ? post : null;
        }

        [Benchmark(Description = "AFTER: 100 random ID lookups (dictionary)")]
        public int After_MultipleIdLookups()
        {
            int count = 0;
            foreach (var id in _searchIds)
            {
                if (_postIndex.TryGetValue(id, out var post))
                {
                    count++;
                }
            }
            return count;
        }

        [Benchmark(Description = "AFTER: Get max ID for insert (dictionary keys)")]
        public int After_GetMaxIdForInsert()
        {
            return _postIndex.Count == 0 ? 1 : _postIndex.Keys.Max() + 1;
        }

        [Benchmark(Description = "AFTER: Delete by ID (dictionary lookup)")]
        public bool After_DeleteById()
        {
            var id = _searchIds[0];
            if (!_postIndex.TryGetValue(id, out var obj)) return false;
            
            _posts.Remove(obj);
            _posts.Add(obj); // Re-add to keep dataset consistent
            return true;
        }
    }
}

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;
using System.Text.Json;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Benchmarks comparing dictionary lookup (O(1)) vs linear search (O(n))
    /// This demonstrates the performance improvement of using dictionary-based ID lookups
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class DictionaryVsLinearSearchBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private Dictionary<int, BenchmarkPost> _postIndex = null!;
        private int _searchId;

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
            _searchId = DatasetSize / 2;
        }

        [Benchmark(Baseline = true, Description = "Linear Search (FirstOrDefault)")]
        public BenchmarkPost? LinearSearch()
        {
            return _posts.FirstOrDefault(p => p.Id == _searchId);
        }

        [Benchmark(Description = "Dictionary Lookup (TryGetValue)")]
        public BenchmarkPost? DictionaryLookup()
        {
            return _postIndex.TryGetValue(_searchId, out var post) ? post : null;
        }

        [Benchmark(Description = "Max ID - Linear (on objects)")]
        public int GetMaxId_Linear()
        {
            return _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
        }

        [Benchmark(Description = "Max ID - Dictionary Keys")]
        public int GetMaxId_Dictionary()
        {
            return _postIndex.Count == 0 ? 1 : _postIndex.Keys.Max() + 1;
        }
    }
}

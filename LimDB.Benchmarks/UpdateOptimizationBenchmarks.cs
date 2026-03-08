using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;
using Microsoft.VSDiagnostics;

namespace LimDB.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [CPUUsageDiagnoser]
    public class UpdateOptimizationBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private Dictionary<int, BenchmarkPost> _idIndex = null!;
        private Dictionary<int, int> _positionIndex = null!;
        private int[] _updateIds = null!;
        [Params(100, 1000, 10000, 100000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _updateIds = new int[100];
            var random = new Random(42);
            for (int i = 0; i < 100; i++)
            {
                _updateIds[i] = random.Next(1, DatasetSize + 1);
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _posts = new List<BenchmarkPost>(DatasetSize);
            _idIndex = new Dictionary<int, BenchmarkPost>(DatasetSize);
            _positionIndex = new Dictionary<int, int>(DatasetSize);
            for (int i = 1; i <= DatasetSize; i++)
            {
                var post = new BenchmarkPost
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
                };
                _posts.Add(post);
                _idIndex[i] = post;
                _positionIndex[i] = i - 1;
            }
        }

        [Benchmark(Baseline = true, Description = "BEFORE: Update with IndexOf() O(n)")]
        public int Before_Update_IndexOf()
        {
            int updated = 0;
            foreach (var id in _updateIds)
            {
                if (_idIndex.TryGetValue(id, out var obj))
                {
                    // O(n) - must search for index
                    var index = _posts.IndexOf(obj);
                    if (index >= 0)
                    {
                        obj.Modified = DateTime.UtcNow;
                        _posts[index] = obj;
                        updated++;
                    }
                }
            }

            return updated;
        }

        [Benchmark(Description = "AFTER: Update with position index O(1)")]
        public int After_Update_PositionIndex()
        {
            int updated = 0;
            foreach (var id in _updateIds)
            {
                if (_idIndex.TryGetValue(id, out var obj) && _positionIndex.TryGetValue(id, out var index))
                {
                    // O(1) - direct lookup
                    obj.Modified = DateTime.UtcNow;
                    _posts[index] = obj;
                    updated++;
                }
            }

            return updated;
        }
    }
}
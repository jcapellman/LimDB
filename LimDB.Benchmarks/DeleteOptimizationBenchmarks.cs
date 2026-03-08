using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;
using Microsoft.VSDiagnostics;

namespace LimDB.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [CPUUsageDiagnoser]
    public class DeleteOptimizationBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private Dictionary<int, BenchmarkPost> _idIndex = null!;
        private Dictionary<int, int> _positionIndex = null!;
        private int[] _deleteIds = null!;
        [Params(100, 1000, 10000, 100000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _deleteIds = new int[100];
            var random = new Random(42);
            for (int i = 0; i < 100; i++)
            {
                _deleteIds[i] = random.Next(1, DatasetSize + 1);
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

        [Benchmark(Baseline = true, Description = "BEFORE: Delete with List.Remove() O(n)")]
        public int Before_Delete_ListRemove()
        {
            int deleted = 0;
            foreach (var id in _deleteIds)
            {
                if (_idIndex.TryGetValue(id, out var obj))
                {
                    // O(n) - must search and shift elements
                    _posts.Remove(obj);
                    _idIndex.Remove(id);
                    _posts.Add(obj); // Re-add to keep dataset consistent
                    _idIndex[id] = obj;
                    deleted++;
                }
            }

            return deleted;
        }

        [Benchmark(Description = "AFTER: Delete with swap-remove O(1)")]
        public int After_Delete_SwapRemove()
        {
            int deleted = 0;
            foreach (var id in _deleteIds)
            {
                if (_idIndex.TryGetValue(id, out var obj) && _positionIndex.TryGetValue(id, out var index))
                {
                    // O(1) - swap with last element, then remove from end
                    var lastIndex = _posts.Count - 1;
                    if (index != lastIndex)
                    {
                        var lastObj = _posts[lastIndex];
                        _posts[index] = lastObj;
                        _positionIndex[lastObj.Id] = index;
                    }

                    _posts.RemoveAt(lastIndex);
                    _idIndex.Remove(id);
                    _positionIndex.Remove(id);
                    // Re-add to keep dataset consistent for benchmark
                    _posts.Add(obj);
                    _idIndex[id] = obj;
                    _positionIndex[id] = _posts.Count - 1;
                    deleted++;
                }
            }

            return deleted;
        }
    }
}
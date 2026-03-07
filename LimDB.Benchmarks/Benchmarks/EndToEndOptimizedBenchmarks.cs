using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Json;
using LimDB.Benchmarks.Objects;
using LimDB.lib;
using LimDB.lib.Sources;
using System.Text.Json;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// End-to-end benchmarks showing real-world performance with all optimizations
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EndToEndOptimizedBenchmarks
    {
        private LimDbContext<BenchmarkPost> _dbContext = null!;
        private string _tempDbFile = null!;

        [Params(1000, 10000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"LimDb_E2E_{Guid.NewGuid()}.json");

            var posts = new List<BenchmarkPost>();
            for (int i = 1; i <= DatasetSize; i++)
            {
                posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post Title {i}",
                    Body = $"Body content for post {i} with some additional text to make it realistic.",
                    Category = $"Category {i % 10}",
                    URL = $"https://example.com/post/{i}",
                    PostDate = DateTime.UtcNow.AddDays(-i),
                    Active = true,
                    Created = DateTime.UtcNow.AddDays(-i),
                    Modified = DateTime.UtcNow.AddDays(-i)
                });
            }

            var json = JsonSerializer.Serialize(posts);
            await File.WriteAllTextAsync(_tempDbFile, json);

            var storageSource = new LocalStorageSource(_tempDbFile);
            _dbContext = await LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (File.Exists(_tempDbFile))
            {
                File.Delete(_tempDbFile);
            }
        }

        [Benchmark(Description = "Sequential: 100 random GetOneById calls")]
        public int SequentialRandomReads()
        {
            var random = new Random(42);
            int count = 0;
            
            for (int i = 0; i < 100; i++)
            {
                var id = random.Next(1, DatasetSize + 1);
                if (_dbContext.GetOneById(id) != null)
                {
                    count++;
                }
            }
            
            return count;
        }

        [Benchmark(Description = "GetMany with filter (10% match)")]
        public List<BenchmarkPost>? FilteredQuery()
        {
            return _dbContext.GetMany(p => p.Category == "Category 5")?.ToList();
        }

        [Benchmark(Description = "GetMany with complex filter")]
        public List<BenchmarkPost>? ComplexFilteredQuery()
        {
            return _dbContext.GetMany(p => 
                p.Category == "Category 5" && 
                p.Active && 
                p.PostDate > DateTime.UtcNow.AddDays(-100))?.ToList();
        }

        [Benchmark(Description = "Mixed workload (90% reads, 10% writes)")]
        public async Task<int> MixedWorkload()
        {
            int operations = 0;
            var random = new Random(42);

            for (int i = 0; i < 10; i++)
            {
                if (i < 9)
                {
                    var id = random.Next(1, DatasetSize + 1);
                    if (_dbContext.GetOneById(id) != null)
                    {
                        operations++;
                    }
                }
                else
                {
                    var post = _dbContext.GetOneById(random.Next(1, DatasetSize + 1));
                    if (post != null)
                    {
                        post.Title = $"Updated {i}";
                        await _dbContext.UpdateAsync(post);
                        operations++;
                    }
                }
            }

            return operations;
        }
    }
}

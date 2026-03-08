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
    /// Benchmark to measure performance impact of bool return values in write operations.
    /// This benchmark was created to test the BEFORE state (with bool returns).
    /// After removing bool returns, improvements ranged from 0-34% on insert operations.
    /// See BOOL-RETURN-PERFORMANCE-ANALYSIS.md for full results.
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class BoolReturnOverheadBenchmarks
    {
        private LimDbContext<BenchmarkPost> _dbContext = null!;
        private List<BenchmarkPost> _posts = null!;
        private string _tempDbFile = null!;
        private const int DatabaseSize = 1000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"LimDb_BoolOverhead_{Guid.NewGuid()}.json");
            
            _posts = new List<BenchmarkPost>();
            for (int i = 1; i <= DatabaseSize; i++)
            {
                _posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post Title {i}",
                    Body = $"Body content for post number {i}. This contains some reasonable amount of text to simulate a real blog post.",
                    Category = $"Category {i % 10}",
                    URL = $"https://example.com/post/{i}",
                    PostDate = DateTime.UtcNow.AddDays(-i),
                    Active = true,
                    Created = DateTime.UtcNow.AddDays(-i),
                    Modified = DateTime.UtcNow.AddDays(-i)
                });
            }

            var json = JsonSerializer.Serialize(_posts);
            File.WriteAllText(_tempDbFile, json);

            var storageSource = new LocalStorageSource(_tempDbFile);
            _dbContext = LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default).GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (File.Exists(_tempDbFile))
            {
                File.Delete(_tempDbFile);
            }
        }

        [Benchmark(Description = "10 Sequential Inserts (Current with bool)")]
        public async Task TenInsertsWithBool()
        {
            for (int i = 0; i < 10; i++)
            {
                await _dbContext.InsertAsync(new BenchmarkPost
                {
                    Title = $"Benchmark Post {i}",
                    Body = $"Benchmark body content {i}",
                    Category = "Benchmark",
                    URL = $"https://example.com/bench/{i}",
                    PostDate = DateTime.UtcNow
                });
            }
        }

        [Benchmark(Description = "10 Sequential Updates (Current with bool)")]
        public async Task TenUpdatesWithBool()
        {
            for (int i = 1; i <= 10; i++)
            {
                var post = _dbContext.GetOneById(i)!;
                post.Title = $"Updated Title {i}";
                await _dbContext.UpdateAsync(post);
            }
        }

        [Benchmark(Description = "Single Insert (Current with bool)", Baseline = true)]
        public async Task<int?> SingleInsertWithBool()
        {
            return await _dbContext.InsertAsync(new BenchmarkPost
            {
                Title = "Single Benchmark Post",
                Body = "Single benchmark body content",
                Category = "Benchmark",
                URL = "https://example.com/single",
                PostDate = DateTime.UtcNow
            });
        }

        [Benchmark(Description = "Single Update (Current with bool)")]
        public async Task<bool> SingleUpdateWithBool()
        {
            var post = _dbContext.GetOneById(50)!;
            post.Title = "Single Updated Title";
            return await _dbContext.UpdateAsync(post);
        }
    }
}

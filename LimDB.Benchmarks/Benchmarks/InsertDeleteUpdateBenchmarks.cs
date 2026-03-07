using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Json;
using LimDB.Benchmarks.Objects;
using LimDB.lib;
using LimDB.lib.Sources;
using System.Text.Json;

namespace LimDB.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class InsertDeleteUpdateBenchmarks
    {
        private LimDbContext<BenchmarkPost> _dbContext = null!;
        private const int InitialDatabaseSize = 1000;
        private List<string> _tempFiles = new();

        [GlobalSetup]
        public void GlobalSetup()
        {
            _tempFiles = new List<string>();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var tempDbFile = Path.Combine(Path.GetTempPath(), $"LimDb_Benchmark_{Guid.NewGuid()}.json");
            _tempFiles.Add(tempDbFile);

            var posts = new List<BenchmarkPost>();
            for (int i = 1; i <= InitialDatabaseSize; i++)
            {
                posts.Add(new BenchmarkPost
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

            var json = JsonSerializer.Serialize(posts);
            File.WriteAllText(tempDbFile, json);

            var storageSource = new LocalStorageSource(tempDbFile);
            _dbContext = LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default).GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            foreach (var file in _tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        [Benchmark(Description = "Insert single item")]
        public async Task<int?> InsertAsync()
        {
            return await _dbContext.InsertAsync(new BenchmarkPost
            {
                Title = "New Post",
                Body = "New Body",
                Category = "New Category",
                URL = "https://example.com/new",
                PostDate = DateTime.UtcNow
            });
        }

        [Benchmark(Description = "Update existing item")]
        public async Task<bool> UpdateAsync()
        {
            var post = _dbContext.GetOneById(500)!;
            post.Title = "Updated Title";
            return await _dbContext.UpdateAsync(post);
        }

        [Benchmark(Description = "Delete existing item")]
        public async Task<bool> DeleteAsync()
        {
            return await _dbContext.DeleteByIdAsync(500);
        }
    }
}

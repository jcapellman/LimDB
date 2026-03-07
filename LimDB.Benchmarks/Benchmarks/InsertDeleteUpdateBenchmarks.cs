using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Objects;
using LimDB.lib;
using System.Text.Json;

namespace LimDB.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class InsertDeleteUpdateBenchmarks
    {
        private string _tempDbFile = null!;
        private LimDbContext<BenchmarkPost> _dbContext = null!;
        private const int InitialDatabaseSize = 1000;

        [IterationSetup]
        public async Task IterationSetup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"LimDb_Benchmark_{Guid.NewGuid()}.json");

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
            await File.WriteAllTextAsync(_tempDbFile, json);

            _dbContext = await LimDbContext<BenchmarkPost>.CreateFromLocalStorageSourceAsync(_tempDbFile);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            if (File.Exists(_tempDbFile))
            {
                File.Delete(_tempDbFile);
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

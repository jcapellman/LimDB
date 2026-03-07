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
    public class LimDbContextBenchmarks
    {
        private string _tempDbFile = null!;
        private LimDbContext<BenchmarkPost> _dbContext = null!;
        private int _existingId;
        private const int DatabaseSize = 10000;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"LimDb_Benchmark_{Guid.NewGuid()}.json");

            var posts = new List<BenchmarkPost>();
            for (int i = 1; i <= DatabaseSize; i++)
            {
                posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post Title {i}",
                    Body = $"This is the body of post {i}. It contains some sample text to make the data more realistic.",
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
            _existingId = DatabaseSize / 2;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (File.Exists(_tempDbFile))
            {
                File.Delete(_tempDbFile);
            }
        }

        [Benchmark(Description = "GetOneById - Middle of dataset")]
        public BenchmarkPost? GetOneById_Middle()
        {
            return _dbContext.GetOneById(_existingId);
        }

        [Benchmark(Description = "GetOneById - First item")]
        public BenchmarkPost? GetOneById_First()
        {
            return _dbContext.GetOneById(1);
        }

        [Benchmark(Description = "GetOneById - Last item")]
        public BenchmarkPost? GetOneById_Last()
        {
            return _dbContext.GetOneById(DatabaseSize);
        }

        [Benchmark(Description = "GetMany - No filter")]
        public IEnumerable<BenchmarkPost>? GetMany_NoFilter()
        {
            return _dbContext.GetMany();
        }

        [Benchmark(Description = "GetMany - With filter")]
        public IEnumerable<BenchmarkPost>? GetMany_WithFilter()
        {
            return _dbContext.GetMany(p => p.Category == "Category 5");
        }

        [Benchmark(Description = "GetOne - No filter")]
        public BenchmarkPost? GetOne_NoFilter()
        {
            return _dbContext.GetOne();
        }

        [Benchmark(Description = "GetOne - With filter")]
        public BenchmarkPost? GetOne_WithFilter()
        {
            return _dbContext.GetOne(p => p.Title.Contains("5000"));
        }
    }
}

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Json;
using LimDB.Benchmarks.Objects;
using LimDB.lib;
using LimDB.lib.Sources;
using System.Text.Json;

namespace LimDB.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class GetManyReturnTypeBenchmarks
    {
        private string _tempDbFile = null!;
        private LimDbContext<BenchmarkPost> _dbContext = null!;
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
                    Body = $"This is the body of post {i}.",
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

        // Baseline: Current IEnumerable<T> approach
        [Benchmark(Baseline = true)]
        public int GetMany_IEnumerable_GetAll()
        {
            var results = _dbContext.GetMany();
            return results?.Count() ?? 0;
        }

        [Benchmark]
        public int GetMany_IEnumerable_WithFilter()
        {
            var results = _dbContext.GetMany(p => p.Category == "Category 5");
            return results?.Count() ?? 0;
        }

        [Benchmark]
        public int GetMany_IEnumerable_MultipleEnumerations()
        {
            var results = _dbContext.GetMany();
            if (results == null) return 0;
            
            // Simulate real-world usage: multiple enumerations
            var count = results.Count();
            var first = results.FirstOrDefault();
            var any = results.Any();
            
            return count;
        }

        [Benchmark]
        public int GetMany_IEnumerable_IndexAccess_ViaElementAt()
        {
            var results = _dbContext.GetMany();
            if (results == null) return 0;
            
            // Accessing by index using ElementAt (inefficient with IEnumerable)
            var item = results.ElementAt(100);
            return item?.Id ?? 0;
        }

        [Benchmark]
        public int GetMany_IReadOnlyList_GetAll()
        {
            var results = _dbContext.GetMany();
            return results.Count;
        }

        [Benchmark]
        public int GetMany_IReadOnlyList_WithFilter()
        {
            var results = _dbContext.GetMany(p => p.Category == "Category 5");
            return results.Count;
        }

        [Benchmark]
        public int GetMany_IReadOnlyList_MultipleEnumerations()
        {
            var results = _dbContext.GetMany();

            // With IReadOnlyList, multiple accesses are free
            var count = results.Count;
            var first = results.FirstOrDefault();
            var any = results.Any();

            return count;
        }

        [Benchmark]
        public int GetMany_IReadOnlyList_IndexAccess()
        {
            var results = _dbContext.GetMany();

            // Direct index access (O(1) with IReadOnlyList)
            var item = results[100];
            return item?.Id ?? 0;
        }
    }
}

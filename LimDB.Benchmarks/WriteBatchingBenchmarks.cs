using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Json;
using LimDB.Benchmarks.Objects;
using LimDB.lib;
using LimDB.lib.Common;
using LimDB.lib.Sources;
using System.Text.Json;
using Microsoft.VSDiagnostics;

namespace LimDB.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [CPUUsageDiagnoser]
    public class WriteBatchingBenchmarks
    {
        private const int InitialDatabaseSize = 100;
        private const int OperationsPerBatch = 10;
        private List<string> _tempFiles = new();
        [GlobalSetup]
        public void GlobalSetup()
        {
            _tempFiles = new List<string>();
        }

        private string CreateTempDbFile()
        {
            var tempDbFile = Path.Combine(Path.GetTempPath(), $"LimDb_Batch_{Guid.NewGuid()}.json");
            _tempFiles.Add(tempDbFile);
            var posts = new List<BenchmarkPost>();
            for (int i = 1; i <= InitialDatabaseSize; i++)
            {
                posts.Add(new BenchmarkPost { Id = i, Title = $"Post Title {i}", Body = $"Body content {i}", Category = $"Category {i % 10}", URL = $"https://example.com/post/{i}", PostDate = DateTime.UtcNow.AddDays(-i), Active = true, Created = DateTime.UtcNow.AddDays(-i), Modified = DateTime.UtcNow.AddDays(-i) });
            }

            var json = JsonSerializer.Serialize(posts);
            File.WriteAllText(tempDbFile, json);
            return tempDbFile;
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

        [Benchmark(Description = "Before: 10 inserts (immediate flush each)")]
        public async Task Before_ImmediateFlush()
        {
            var tempDbFile = CreateTempDbFile();
            var storageSource = new LocalStorageSource(tempDbFile);
            var dbContext = await LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default, batchSize: LibConstants.DefaultBatchSize);
            for (int i = 0; i < OperationsPerBatch; i++)
            {
                await dbContext.InsertAsync(new BenchmarkPost { Title = $"New Post {i}", Body = $"New Body {i}", Category = "New Category", URL = $"https://example.com/new/{i}", PostDate = DateTime.UtcNow });
            }

            dbContext.Dispose();
        }

        [Benchmark(Description = "After: 10 inserts (batched, single flush)")]
        public async Task After_BatchedFlush()
        {
            var tempDbFile = CreateTempDbFile();
            var storageSource = new LocalStorageSource(tempDbFile);
            var dbContext = await LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default, batchSize: OperationsPerBatch);
            for (int i = 0; i < OperationsPerBatch; i++)
            {
                await dbContext.InsertAsync(new BenchmarkPost { Title = $"New Post {i}", Body = $"New Body {i}", Category = "New Category", URL = $"https://example.com/new/{i}", PostDate = DateTime.UtcNow });
            }

            dbContext.Dispose();
        }

        [Benchmark(Description = "After: 10 inserts (manual flush at end)")]
        public async Task After_ManualFlush()
        {
            var tempDbFile = CreateTempDbFile();
            var storageSource = new LocalStorageSource(tempDbFile);
            var dbContext = await LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default, batchSize: int.MaxValue);
            for (int i = 0; i < OperationsPerBatch; i++)
            {
                await dbContext.InsertAsync(new BenchmarkPost { Title = $"New Post {i}", Body = $"New Body {i}", Category = "New Category", URL = $"https://example.com/new/{i}", PostDate = DateTime.UtcNow });
            }

            await dbContext.FlushAsync();
            dbContext.Dispose();
        }
    }
}
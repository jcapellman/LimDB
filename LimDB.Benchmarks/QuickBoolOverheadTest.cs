using System.Diagnostics;
using LimDB.Benchmarks.Json;
using LimDB.Benchmarks.Objects;
using LimDB.lib;
using LimDB.lib.Sources;
using System.Text.Json;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Quick test to measure bool return overhead in WriteDbAsync
    /// </summary>
    public class QuickBoolOverheadTest
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Bool Return Overhead Test ===\n");
            Console.WriteLine("Testing impact of bool return value in WriteDbAsync...\n");

            await RunInsertBenchmark();
            await RunUpdateBenchmark();

            Console.WriteLine("\n=== Test Complete ===");
        }

        private static async Task RunInsertBenchmark()
        {
            Console.WriteLine("1. Insert Operations (without bool return):");
            
            var iterations = new[] { 10, 50, 100 };
            
            foreach (var count in iterations)
            {
                var tempFile = Path.Combine(Path.GetTempPath(), $"LimDb_QuickTest_{Guid.NewGuid()}.json");
                
                try
                {
                    // Create initial database
                    var initialPosts = CreateInitialData(100);
                    File.WriteAllText(tempFile, JsonSerializer.Serialize(initialPosts));

                    var storageSource = new LocalStorageSource(tempFile);
                    var dbContext = await LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default);

                    // Measure inserts
                    var sw = Stopwatch.StartNew();
                    for (int i = 0; i < count; i++)
                    {
                        await dbContext.InsertAsync(new BenchmarkPost
                        {
                            Title = $"Test Post {i}",
                            Body = $"Test body content for post {i}",
                            Category = "Test",
                            URL = $"https://example.com/test/{i}",
                            PostDate = DateTime.UtcNow
                        });
                    }
                    sw.Stop();

                    var avgMs = sw.Elapsed.TotalMilliseconds / count;
                    Console.WriteLine($"   {count,3} inserts: {sw.Elapsed.TotalMilliseconds:F2} ms total, {avgMs:F3} ms avg per insert");
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            Console.WriteLine();
        }

        private static async Task RunUpdateBenchmark()
        {
            Console.WriteLine("2. Update Operations (without bool return):");
            
            var iterations = new[] { 10, 50, 100 };
            
            foreach (var count in iterations)
            {
                var tempFile = Path.Combine(Path.GetTempPath(), $"LimDb_QuickTest_{Guid.NewGuid()}.json");
                
                try
                {
                    // Create initial database
                    var initialPosts = CreateInitialData(500);
                    File.WriteAllText(tempFile, JsonSerializer.Serialize(initialPosts));

                    var storageSource = new LocalStorageSource(tempFile);
                    var dbContext = await LimDbContext<BenchmarkPost>.CreateAsync(storageSource, BenchmarkJsonContext.Default);

                    // Measure updates
                    var sw = Stopwatch.StartNew();
                    for (int i = 1; i <= count; i++)
                    {
                        var post = dbContext.GetOneById(i)!;
                        post.Title = $"Updated Title {i}";
                        await dbContext.UpdateAsync(post);
                    }
                    sw.Stop();

                    var avgMs = sw.Elapsed.TotalMilliseconds / count;
                    Console.WriteLine($"   {count,3} updates: {sw.Elapsed.TotalMilliseconds:F2} ms total, {avgMs:F3} ms avg per update");
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            Console.WriteLine();
        }

        private static List<BenchmarkPost> CreateInitialData(int count)
        {
            var posts = new List<BenchmarkPost>();
            for (int i = 1; i <= count; i++)
            {
                posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post {i}",
                    Body = $"Body content for post {i}",
                    Category = $"Category {i % 10}",
                    URL = $"https://example.com/post/{i}",
                    PostDate = DateTime.UtcNow.AddDays(-i),
                    Active = true,
                    Created = DateTime.UtcNow.AddDays(-i),
                    Modified = DateTime.UtcNow.AddDays(-i)
                });
            }
            return posts;
        }
    }
}

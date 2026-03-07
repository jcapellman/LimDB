using System.Diagnostics;
using LimDB.Benchmarks.Objects;

namespace LimDB.Benchmarks
{
    /// <summary>
    /// Simple micro-benchmark runner that shows immediate results
    /// Use this for quick validation without waiting for BenchmarkDotNet
    /// </summary>
    public class QuickMicroBenchmark
    {
        public static void Run()
        {
            Console.WriteLine("=== Quick Micro-Benchmark Results ===\n");

            RunMaxIdBenchmark();
            RunLookupBenchmark();
            RunDateTimeBenchmark();

            Console.WriteLine("\n=== For detailed benchmarks, run: dotnet run -c Release ===");
        }

        private static void RunMaxIdBenchmark()
        {
            Console.WriteLine("1. Max ID Generation (Insert Optimization):");
            
            var sizes = new[] { 1000, 10000 };
            
            foreach (var size in sizes)
            {
                var posts = CreateTestData(size);
                var postIndex = posts.ToDictionary(p => p.Id);
                int maxId = size;

                // Before: Max on objects
                var sw1 = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    _ = posts.Count == 0 ? 1 : posts.Max(p => p.Id) + 1;
                }
                sw1.Stop();
                var beforeNs = sw1.Elapsed.TotalNanoseconds / 10000;

                // Middle: Max on dictionary keys  
                var sw2 = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    _ = postIndex.Count == 0 ? 1 : postIndex.Keys.Max() + 1;
                }
                sw2.Stop();
                var middleNs = sw2.Elapsed.TotalNanoseconds / 10000;

                // After: Cached field
                var sw3 = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    _ = maxId + 1;
                }
                sw3.Stop();
                var afterNs = sw3.Elapsed.TotalNanoseconds / 10000;

                Console.WriteLine($"   Dataset: {size:N0} items");
                Console.WriteLine($"   - Before (Max on objects):    {beforeNs:F2} ns");
                Console.WriteLine($"   - Middle (Max on dict keys):  {middleNs:F2} ns  ({beforeNs/middleNs:F1}x faster)");
                Console.WriteLine($"   - After  (Cached maxId):      {afterNs:F2} ns  ({beforeNs/afterNs:F0}x faster) ⚡");
                Console.WriteLine();
            }
        }

        private static void RunLookupBenchmark()
        {
            Console.WriteLine("2. ID Lookup (GetOneById Optimization):");
            
            var sizes = new[] { 1000, 10000 };
            
            foreach (var size in sizes)
            {
                var posts = CreateTestData(size);
                var postIndex = posts.ToDictionary(p => p.Id);
                var searchId = size / 2;

                // Before: Linear search
                var sw1 = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    _ = posts.FirstOrDefault(p => p.Id == searchId);
                }
                sw1.Stop();
                var beforeNs = sw1.Elapsed.TotalNanoseconds / 10000;

                // After: Dictionary lookup
                var sw2 = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    _ = postIndex.TryGetValue(searchId, out var p) ? p : null;
                }
                sw2.Stop();
                var afterNs = sw2.Elapsed.TotalNanoseconds / 10000;

                Console.WriteLine($"   Dataset: {size:N0} items");
                Console.WriteLine($"   - Before (FirstOrDefault):    {beforeNs:F2} ns");
                Console.WriteLine($"   - After  (Dictionary lookup): {afterNs:F2} ns  ({beforeNs/afterNs:F0}x faster) ⚡");
                Console.WriteLine();
            }
        }

        private static void RunDateTimeBenchmark()
        {
            Console.WriteLine("3. DateTime Generation:");

            // DateTime.Now
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                _ = DateTime.Now;
            }
            sw1.Stop();
            var nowNs = sw1.Elapsed.TotalNanoseconds / 100000;

            // DateTime.UtcNow
            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                _ = DateTime.UtcNow;
            }
            sw2.Stop();
            var utcNs = sw2.Elapsed.TotalNanoseconds / 100000;

            Console.WriteLine($"   - DateTime.Now:    {nowNs:F2} ns");
            Console.WriteLine($"   - DateTime.UtcNow: {utcNs:F2} ns  ({nowNs/utcNs:F1}x faster) ⚡");
            Console.WriteLine();
        }

        private static List<BenchmarkPost> CreateTestData(int count)
        {
            var posts = new List<BenchmarkPost>(count);
            for (int i = 1; i <= count; i++)
            {
                posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post {i}",
                    Body = $"Body {i}",
                    Category = $"Cat{i % 10}",
                    URL = $"url{i}",
                    PostDate = DateTime.UtcNow,
                    Active = true,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                });
            }
            return posts;
        }
    }
}

using BenchmarkDotNet.Running;

namespace LimDB.Benchmarks
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--quick")
            {
                QuickMicroBenchmark.Run();
                return;
            }

            if (args.Length > 0 && args[0] == "--bool")
            {
                await QuickBoolOverheadTest.Run();
                return;
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
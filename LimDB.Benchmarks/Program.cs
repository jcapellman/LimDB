using BenchmarkDotNet.Running;

namespace LimDB.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--quick")
            {
                QuickMicroBenchmark.Run();
                return;
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
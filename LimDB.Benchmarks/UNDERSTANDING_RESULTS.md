# Understanding Benchmark Results

## Example Output

When you run the benchmarks, you'll see output like this:

```
| Method                    | DatasetSize | Mean      | Error    | StdDev   | Rank | Allocated |
|-------------------------- |------------ |----------:|---------:|---------:|-----:|----------:|
| Dictionary Lookup         | 10000       |  50.00 ns | 0.500 ns | 0.450 ns |    1 |         - |
| Linear Search             | 10000       | 50000 ns  | 500 ns   | 450 ns   |    2 |         - |
```

## Key Metrics Explained

### Mean
- **Average execution time** across all runs
- Lower is better
- Units: ns (nanoseconds), μs (microseconds), ms (milliseconds)

### Error & StdDev
- **Error**: 99.9% confidence interval
- **StdDev**: Standard deviation showing consistency
- Lower values mean more consistent performance

### Rank
- **Performance ranking** (1 = fastest)
- Makes it easy to compare methods

### Allocated
- **Memory allocated** per operation
- Shows garbage collection pressure
- Lower is better

## Performance Improvement Calculations

### Example: GetOneById with 10,000 items

**Before** (Linear Search): 50,000 ns = 50 μs
**After** (Dictionary Lookup): 50 ns

**Speedup**: 50,000 / 50 = **1,000x faster!**

### Why Does Dataset Size Matter?

- **Linear Search (O(n))**: Time increases with dataset size
  - 100 items: ~500 ns
  - 1,000 items: ~5,000 ns (5 μs)
  - 10,000 items: ~50,000 ns (50 μs)

- **Dictionary Lookup (O(1))**: Time stays constant
  - 100 items: ~50 ns
  - 1,000 items: ~50 ns
  - 10,000 items: ~50 ns

## Real-World Impact

For a database with **10,000 records**:
- **100,000 ID lookups per second**
  - Before: Would take ~5 seconds
  - After: Takes ~0.005 seconds
  - **Improvement**: 1,000x faster

## Tips for Running Benchmarks

1. **Close other applications** - Reduces noise in measurements
2. **Run in Release mode** - Always use `-c Release`
3. **Let it warm up** - BenchmarkDotNet runs warmup iterations automatically
4. **Multiple runs** - Run benchmarks multiple times to ensure consistency
5. **Check CPU usage** - Make sure your CPU isn't throttled

## Common Issues

### High Error/StdDev
- Other processes using CPU
- Thermal throttling
- Background tasks running

### Unexpected Results
- Running in Debug mode (always use Release)
- Antivirus scanning files
- Insufficient warmup (BenchmarkDotNet handles this)

# Quick Start Guide

## Run the Benchmarks

To run all benchmarks:

```bash
cd LimDB.Benchmarks
dotnet run -c Release
```

## Run Individual Benchmark Suites

### Dictionary vs Linear Search Comparison
This shows the O(1) vs O(n) performance difference:
```bash
dotnet run -c Release --filter *DictionaryVsLinearSearchBenchmarks*
```

### Core Operations (Read-heavy)
Tests GetOneById, GetMany, GetOne:
```bash
dotnet run -c Release --filter *LimDbContextBenchmarks*
```

### Write Operations
Tests Insert, Update, Delete:
```bash
dotnet run -c Release --filter *InsertDeleteUpdateBenchmarks*
```

### DateTime Comparison
Shows DateTime.UtcNow vs DateTime.Now:
```bash
dotnet run -c Release --filter *DateTimeBenchmarks*
```

## What to Look For

### GetOneById Performance
- Should be **~O(1)** regardless of dataset size
- **Before**: ~5-50μs for 10,000 items (depending on position)
- **After**: ~50-100ns consistently

### Insert/Update/Delete
- Dictionary maintenance adds minimal overhead
- ID generation is faster with dictionary keys

### DateTime
- UtcNow should be **~2-3x faster** than Now

## Expected Performance Gains

| Operation | Before (O(n)) | After (O(1)) | Speedup |
|-----------|--------------|--------------|---------|
| GetOneById (100 items) | ~500 ns | ~50 ns | ~10x |
| GetOneById (1,000 items) | ~5 μs | ~50 ns | ~100x |
| GetOneById (10,000 items) | ~50 μs | ~50 ns | ~1000x |

The larger the database, the more dramatic the improvement!

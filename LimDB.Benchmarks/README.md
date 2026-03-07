# LimDB Performance Benchmarks

This project contains benchmarks to measure and validate performance improvements in LimDB.

## Running the Benchmarks

### Run All Benchmarks
```bash
dotnet run -c Release --project LimDB.Benchmarks
```

### Run Specific Benchmark
```bash
# Run only the dictionary vs linear search comparison
dotnet run -c Release --project LimDB.Benchmarks --filter *DictionaryVsLinearSearchBenchmarks*

# Run only the LimDbContext operations
dotnet run -c Release --project LimDB.Benchmarks --filter *LimDbContextBenchmarks*

# Run only the DateTime comparison
dotnet run -c Release --project LimDB.Benchmarks --filter *DateTimeBenchmarks*

# Run only Insert/Delete/Update operations
dotnet run -c Release --project LimDB.Benchmarks --filter *InsertDeleteUpdateBenchmarks*
```

## Benchmark Categories

### 1. DictionaryVsLinearSearchBenchmarks
Compares the performance difference between:
- **Linear Search**: `FirstOrDefault(p => p.Id == id)` - O(n) complexity
- **Dictionary Lookup**: `TryGetValue(id, out var post)` - O(1) complexity

Tests with dataset sizes: 100, 1,000, and 10,000 items.

**Expected Results**: Dictionary lookup should be significantly faster, especially with larger datasets.

### 2. LimDbContextBenchmarks
Tests the performance of core LimDB operations on a 10,000-item database:
- GetOneById (first, middle, and last items)
- GetMany (with and without filters)
- GetOne (with and without filters)

**Expected Results**: GetOneById should show consistent O(1) performance regardless of item position.

### 3. InsertDeleteUpdateBenchmarks
Measures the performance of write operations:
- InsertAsync
- UpdateAsync
- DeleteByIdAsync

**Expected Results**: These operations should be fast with the dictionary index maintained properly.

### 4. DateTimeBenchmarks
Compares DateTime.Now vs DateTime.UtcNow:
- Single call performance
- 1,000 consecutive calls

**Expected Results**: DateTime.UtcNow should be faster as it avoids timezone conversion.

## Performance Improvements Implemented

1. **Dictionary-based ID indexing** - O(1) lookups instead of O(n) linear searches
2. **Static HttpClient reuse** - Prevents socket exhaustion
3. **DateTime.UtcNow** - Faster timestamp generation
4. **Thread-safe file writes** - SemaphoreSlim prevents concurrent write conflicts

## Interpreting Results

- **Mean**: Average execution time
- **Error**: 99.9% confidence interval
- **StdDev**: Standard deviation of measurements
- **Rank**: Performance ranking (1 = fastest)
- **Allocated**: Memory allocated during operation

Lower values are better for all metrics.

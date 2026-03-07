```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7840/25H2/2025Update/HudsonValley2)
Snapdragon X - X126100 - Qualcomm Oryon CPU 2.96GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.3, 10.0.326.7603), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.3 (10.0.3, 10.0.326.7603), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                            | DatasetSize | Mean           | Error          | StdDev      | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
|---------------------------------- |------------ |---------------:|---------------:|------------:|------:|--------:|-----:|----------:|------------:|
| &#39;Dictionary Lookup (TryGetValue)&#39; | 100         |      0.6925 ns |      0.0054 ns |   0.0003 ns |  0.04 |    0.00 |    1 |         - |          NA |
| &#39;Linear Search (FirstOrDefault)&#39;  | 100         |     18.6371 ns |     16.1064 ns |   0.8828 ns |  1.00 |    0.06 |    2 |         - |          NA |
| &#39;Max ID - Dictionary Keys&#39;        | 100         |    259.9354 ns |      2.2085 ns |   0.1211 ns | 13.97 |    0.56 |    3 |         - |          NA |
| &#39;Max ID - Linear (on objects)&#39;    | 100         |    281.7712 ns |    156.9608 ns |   8.6036 ns | 15.14 |    0.73 |    3 |         - |          NA |
|                                   |             |                |                |             |       |         |      |           |             |
| &#39;Dictionary Lookup (TryGetValue)&#39; | 1000        |     22.3171 ns |     12.7261 ns |   0.6976 ns |  0.11 |    0.00 |    1 |         - |          NA |
| &#39;Linear Search (FirstOrDefault)&#39;  | 1000        |    200.5387 ns |      5.6736 ns |   0.3110 ns |  1.00 |    0.00 |    2 |         - |          NA |
| &#39;Max ID - Dictionary Keys&#39;        | 1000        |  2,664.0185 ns |     19.6789 ns |   1.0787 ns | 13.28 |    0.02 |    3 |         - |          NA |
| &#39;Max ID - Linear (on objects)&#39;    | 1000        |  2,951.3297 ns |     12.5288 ns |   0.6867 ns | 14.72 |    0.02 |    3 |         - |          NA |
|                                   |             |                |                |             |       |         |      |           |             |
| &#39;Dictionary Lookup (TryGetValue)&#39; | 10000       |      0.7989 ns |      0.1904 ns |   0.0104 ns | 0.000 |    0.00 |    1 |         - |          NA |
| &#39;Linear Search (FirstOrDefault)&#39;  | 10000       |  4,744.3860 ns |     18.9220 ns |   1.0372 ns | 1.000 |    0.00 |    2 |         - |          NA |
| &#39;Max ID - Dictionary Keys&#39;        | 10000       | 26,496.3847 ns |    720.2110 ns |  39.4772 ns | 5.585 |    0.01 |    3 |         - |          NA |
| &#39;Max ID - Linear (on objects)&#39;    | 10000       | 32,312.5224 ns | 15,337.8186 ns | 840.7176 ns | 6.811 |    0.15 |    4 |      40 B |          NA |

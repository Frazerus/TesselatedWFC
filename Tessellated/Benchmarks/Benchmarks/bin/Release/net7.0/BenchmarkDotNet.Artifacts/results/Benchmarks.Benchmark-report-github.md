```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
12th Gen Intel Core i5-12600K, 1 CPU, 16 logical and 10 physical cores
.NET SDK 8.0.303
  [Host] : .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
| Method                                         | Mean       | Error     | StdDev    |
|----------------------------------------------- |-----------:|----------:|----------:|
| OriginalModel_MRV                              |   158.4 μs |   0.68 μs |   0.60 μs |
| OriginalModel_Entropy                          |   158.5 μs |   1.53 μs |   1.28 μs |
| OctagonTessellationModel_NormalSquares_MRV     | 1,635.5 μs |  66.67 μs |  62.37 μs |
| OctagonTessellationModel_NormalSquares_Entropy | 1,619.0 μs | 119.07 μs | 111.38 μs |
| OctagonTessellationModel_GrassSquares_MRV      |   330.4 μs |   8.54 μs |   7.99 μs |
| OctagonTessellationModel_GrassSquares_Entropy  |   333.1 μs |   6.32 μs |   5.61 μs |

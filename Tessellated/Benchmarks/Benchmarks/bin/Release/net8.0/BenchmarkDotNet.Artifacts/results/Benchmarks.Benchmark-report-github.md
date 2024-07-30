```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
12th Gen Intel Core i5-12600K, 1 CPU, 16 logical and 10 physical cores
.NET SDK 8.0.303
  [Host] : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
| Method                                         | Mean       | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated   |
|----------------------------------------------- |-----------:|----------:|----------:|---------:|---------:|---------:|------------:|
| OriginalModel_MRV                              |   152.7 μs |   2.32 μs |   2.05 μs |  11.2305 |   2.4414 |        - |   119.55 KB |
| OriginalModel_Entropy                          |   159.4 μs |   5.63 μs |   5.27 μs |  11.4746 |   2.6855 |        - |   119.55 KB |
| OctagonTessellationModel_NormalSquares_MRV     | 7,555.0 μs | 271.82 μs | 254.26 μs | 750.0000 | 734.3750 | 539.0625 | 13799.63 KB |
| OctagonTessellationModel_NormalSquares_Entropy | 1,585.0 μs |  51.68 μs |  48.35 μs | 406.2500 | 378.9063 | 332.0313 |  3661.09 KB |
| OctagonTessellationModel_GrassSquares_MRV      |   313.5 μs |   2.23 μs |   2.08 μs |  31.7383 |  15.6250 |        - |   327.88 KB |
| OctagonTessellationModel_GrassSquares_Entropy  |   319.3 μs |   3.40 μs |   3.18 μs |  31.7383 |  15.6250 |        - |   327.88 KB |

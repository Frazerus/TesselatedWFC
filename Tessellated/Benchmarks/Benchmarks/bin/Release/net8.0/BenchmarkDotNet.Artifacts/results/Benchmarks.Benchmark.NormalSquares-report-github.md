```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
12th Gen Intel Core i5-12600K, 1 CPU, 16 logical and 10 physical cores
.NET SDK 8.0.303
  [Host] : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
| Method                                 | Size | Heuristic | Mean      | Error     | StdDev    |
|--------------------------------------- |----- |---------- |----------:|----------:|----------:|
| **OctagonTessellationModel_NormalSquares** | **10**   | **Entropy**   |  **1.557 ms** | **0.0495 ms** | **0.0463 ms** |
| **OctagonTessellationModel_NormalSquares** | **10**   | **MRV**       |  **1.558 ms** | **0.0533 ms** | **0.0472 ms** |
| **OctagonTessellationModel_NormalSquares** | **20**   | **Entropy**   |  **5.522 ms** | **0.0928 ms** | **0.0823 ms** |
| **OctagonTessellationModel_NormalSquares** | **20**   | **MRV**       |  **7.364 ms** | **0.3163 ms** | **0.2959 ms** |
| **OctagonTessellationModel_NormalSquares** | **50**   | **Entropy**   | **55.194 ms** | **1.4670 ms** | **1.3723 ms** |
| **OctagonTessellationModel_NormalSquares** | **50**   | **MRV**       | **47.749 ms** | **0.6457 ms** | **0.5392 ms** |

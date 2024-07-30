```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
12th Gen Intel Core i5-12600K, 1 CPU, 16 logical and 10 physical cores
.NET SDK 8.0.303
  [Host] : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2

Job=MediumRun  OutlierMode=DontRemove  Toolchain=InProcessNoEmitToolchain  
IterationCount=15  LaunchCount=1  WarmupCount=10  

```
| Method                                 | Heuristic | Size | Mean        | Error     | StdDev    | Gen0      | Gen1      | Gen2     | Allocated   |
|--------------------------------------- |---------- |----- |------------:|----------:|----------:|----------:|----------:|---------:|------------:|
| **OriginalModel_MRV**                      | **Entropy**   | **10**   |    **146.7 μs** |   **2.64 μs** |   **2.47 μs** |   **11.2305** |    **2.4414** |        **-** |   **119.55 KB** |
| OctagonTessellationModel_NormalSquares | Entropy   | 10   |  1,444.1 μs |  24.51 μs |  22.93 μs |  406.2500 |  376.9531 | 332.0313 |  3661.13 KB |
| OctagonTessellationModel_GrassSquares  | Entropy   | 10   |    290.6 μs |   3.08 μs |   2.88 μs |   31.7383 |   15.6250 |        - |   327.88 KB |
| **OriginalModel_MRV**                      | **Entropy**   | **20**   |    **825.3 μs** |  **12.40 μs** |  **11.60 μs** |   **34.1797** |   **16.6016** |        **-** |   **349.28 KB** |
| OctagonTessellationModel_NormalSquares | Entropy   | 20   |  7,433.6 μs | 222.09 μs | 207.75 μs |  757.8125 |  726.5625 | 546.8750 | 13799.79 KB |
| OctagonTessellationModel_GrassSquares  | Entropy   | 20   |  1,892.3 μs |  12.51 μs |  11.70 μs |   89.8438 |   89.8438 |  89.8438 |  1085.06 KB |
| **OriginalModel_MRV**                      | **Entropy**   | **30**   |  **3,000.2 μs** |  **83.18 μs** |  **77.80 μs** |   **78.1250** |   **74.2188** |  **23.4375** |   **732.17 KB** |
| OctagonTessellationModel_NormalSquares | Entropy   | 30   | 20,122.6 μs | 234.04 μs | 218.92 μs | 1125.0000 | 1093.7500 | 687.5000 | 30698.91 KB |
| OctagonTessellationModel_GrassSquares  | Entropy   | 30   |  6,807.0 μs |  60.56 μs |  56.65 μs |  195.3125 |  195.3125 | 195.3125 |     2347 KB |
| **OriginalModel_MRV**                      | **MRV**       | **10**   |    **146.4 μs** |   **1.70 μs** |   **1.59 μs** |   **11.4746** |    **2.6855** |        **-** |   **119.55 KB** |
| OctagonTessellationModel_NormalSquares | MRV       | 10   |  1,471.6 μs |  39.43 μs |  36.88 μs |  408.2031 |  380.8594 | 333.9844 |   3661.2 KB |
| OctagonTessellationModel_GrassSquares  | MRV       | 10   |    286.2 μs |   3.03 μs |   2.83 μs |   31.7383 |   15.6250 |        - |   327.88 KB |
| **OriginalModel_MRV**                      | **MRV**       | **20**   |    **821.3 μs** |   **8.14 μs** |   **7.61 μs** |   **34.1797** |   **16.6016** |        **-** |   **349.28 KB** |
| OctagonTessellationModel_NormalSquares | MRV       | 20   |  7,116.0 μs | 144.91 μs | 135.55 μs |  750.0000 |  726.5625 | 539.0625 | 13799.86 KB |
| OctagonTessellationModel_GrassSquares  | MRV       | 20   |  1,771.7 μs |   8.83 μs |   8.26 μs |   89.8438 |   89.8438 |  89.8438 |  1085.06 KB |
| **OriginalModel_MRV**                      | **MRV**       | **30**   |  **3,745.1 μs** |  **46.32 μs** |  **43.33 μs** |   **78.1250** |   **74.2188** |  **23.4375** |   **732.17 KB** |
| OctagonTessellationModel_NormalSquares | MRV       | 30   | 18,625.8 μs | 407.47 μs | 381.15 μs | 1125.0000 | 1093.7500 | 687.5000 |  30698.9 KB |
| OctagonTessellationModel_GrassSquares  | MRV       | 30   |  6,633.3 μs |  31.04 μs |  29.03 μs |  195.3125 |  195.3125 | 195.3125 |     2347 KB |

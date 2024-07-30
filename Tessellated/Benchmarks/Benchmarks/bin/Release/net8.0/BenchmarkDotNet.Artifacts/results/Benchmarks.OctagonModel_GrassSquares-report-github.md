```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
12th Gen Intel Core i5-12600K, 1 CPU, 16 logical and 10 physical cores
.NET SDK 8.0.303
  [Host] : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
| Method  | Mean     | Error   | StdDev  |
|-------- |---------:|--------:|--------:|
| MRV     | 304.2 μs | 3.59 μs | 3.36 μs |
| Entropy | 310.7 μs | 5.78 μs | 5.13 μs |

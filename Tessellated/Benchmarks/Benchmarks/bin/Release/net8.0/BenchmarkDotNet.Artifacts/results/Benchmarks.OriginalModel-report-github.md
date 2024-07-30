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
| MRV     | 154.9 μs | 1.89 μs | 1.76 μs |
| Entropy | 155.3 μs | 4.01 μs | 3.75 μs |

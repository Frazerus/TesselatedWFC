using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;

namespace Benchmarks;

public enum Type
{
    Grass,
    Normal,
    Basic
}

[RPlotExporter]
[Outliers(OutlierMode.DontRemove)]
[MemoryDiagnoser]
public class Benchmark
{
    private const string CPathToXmlFile = @"..\..\..\..\..\Assets";

    [Params(Heuristic.Entropy, Heuristic.MRV)]
    public Heuristic Heuristic { get; set; }

    [Params(10, 20, 30)] public int Size { get; set; }

    public int Seed(Type info)
    {
        return (Size, Heuristic, info) switch
        {
            (10, Heuristic.Entropy, Type.Grass) => 205,
            (20, Heuristic.Entropy, Type.Grass) => 104,
            (30, Heuristic.Entropy, Type.Grass) => 1093,
            (10, Heuristic.MRV, Type.Grass) => 322,
            (20, Heuristic.MRV, Type.Grass) => 118,
            (30, Heuristic.MRV, Type.Grass) => 329,
            (10, Heuristic.Entropy, Type.Basic) => 51,
            (20, Heuristic.Entropy, Type.Basic) => 664,
            (30, Heuristic.Entropy, Type.Basic) => 1323,
            (10, Heuristic.MRV, Type.Basic) => 952,
            (20, Heuristic.MRV, Type.Basic) => 1199,
            (30, Heuristic.MRV, Type.Basic) => 1239,
            (10, Heuristic.Entropy, Type.Normal) => 740,
            (20, Heuristic.Entropy, Type.Normal) => 766,
            (30, Heuristic.Entropy, Type.Normal) => 1265,
            (10, Heuristic.MRV, Type.Normal) => 1125,
            (20, Heuristic.MRV, Type.Normal) => 742,
            (30, Heuristic.MRV, Type.Normal) => 926,
            _ => -1
        };
    }

    [Benchmark]
    public void OriginalModel_MRV()
    {
        var model = new SimpleTiledModel("standard", null, Size, Size, false, Heuristic, "standard",
            CPathToXmlFile);
        model.Run(Seed(Type.Basic), 10_000);
    }

    [Benchmark]
    public void OctagonTessellationModel_NormalSquares()
    {
        var model = new SimpleOctagonTessellationModel("octagon_standardImitation_NormalSquares", Size, Size, false,
            Heuristic, 2, CPathToXmlFile);
        model.Run(Seed(Type.Normal), 10_000);
    }

    [Benchmark]
    public void OctagonTessellationModel_GrassSquares()
    {
        var model = new SimpleOctagonTessellationModel("octagon_standardImitation_GrassOnlySquares", Size, Size,
            false,
            Heuristic, 2, CPathToXmlFile);
        model.Run(Seed(Type.Grass), 10_000);
    }
}


public class Program
{
    public static void Main()
    {

        var config =
            DefaultConfig.Instance.AddJob(Job.MediumRun.WithLaunchCount(1)
                .WithToolchain(InProcessNoEmitToolchain.Instance));
        //BenchmarkRunner.Run<Benchmark.NormalSquares>(config);
        BenchmarkRunner.Run(typeof(Benchmark).Assembly, config);
    }
}
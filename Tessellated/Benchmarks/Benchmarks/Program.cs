using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;

namespace Benchmarks;


public class Benchmark
{
    private const string CPathToXmlFile = @"..\..\..\..\..\Assets";


    [Outliers(OutlierMode.DontRemove)]
    [MemoryDiagnoser]
    private class Basic
    {
        [Params(Model.Heuristic.Entropy, Model.Heuristic.MRV)]
        public Model.Heuristic Heuristic { get; set; }

        [Params(10, 20, 50)]
        public int Size { get; set; }

        [Benchmark]
        public void OriginalModel_MRV()
        {
            var model = new SimpleTiledModel("standard", null, Size, Size, false, Heuristic, "standard",
                CPathToXmlFile);
            model.Run(27, 10_000);
        }
    }

    [Outliers(OutlierMode.DontRemove)]
    [MemoryDiagnoser]
    public class NormalSquares
    {
        [Params(10, 20, 50)]
        public int Size { get;set; }

        [Params(Heuristic.Entropy, Heuristic.MRV)]
        public Heuristic Heuristic { get; set; }

        [Benchmark]
        public void OctagonTessellationModel_NormalSquares()
        {
            var model = new SimpleOctagonTessellationModel("octagon_standardImitation_NormalSquares", Size, Size, false,
                Heuristic, 2, CPathToXmlFile);
            model.Run(27, 10_000);
        }
    }

    [Outliers(OutlierMode.DontRemove)]
    [MemoryDiagnoser]
    public class GrassSquares
    {
        [Params(Heuristic.Entropy, Heuristic.MRV)]
        public Heuristic Heuristic { get; set; }

        [Params(10, 20, 50)]
        public int Size { get; set; }

        [Benchmark]
        public void OctagonTessellationModel_GrassSquares()
        {
            var model = new SimpleOctagonTessellationModel("octagon_standardImitation_GrassOnlySquares", Size, Size, false,
                Heuristic, 2, CPathToXmlFile);
            model.Run(27, 10_000);
        }
    }
}

public class Program
{
    public static void Main()
    {
        var config =
            DefaultConfig.Instance.AddJob(Job.MediumRun.WithLaunchCount(1)
                .WithToolchain(InProcessNoEmitToolchain.Instance));

        BenchmarkRunner.Run<Benchmark.NormalSquares>(config);
    }
}
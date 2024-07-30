using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Example.Benchmarking
{
    public class BenchmarkConfiguration : ManualConfig
    {
        public BenchmarkConfiguration()
        {
            AddJob(Job.MediumRun.WithLaunchCount(1).WithToolchain(InProcessNoEmitToolchain.Instance));
            AddLogger(ConsoleLogger.Default);
            AddColumn(TargetMethodColumn.Method);
            AddColumn(new BenchmarkCustomColumn("Successful", UnitType.Size)); //  Your custom column
        }
    }
}
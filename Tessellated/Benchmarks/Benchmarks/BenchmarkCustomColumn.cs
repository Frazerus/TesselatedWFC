using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System.Linq;

namespace Example.Benchmarking
{
    public class BenchmarkCustomColumn : IColumn
    {
        public string Id => nameof(BenchmarkCustomColumn);
        public string ColumnName { get; private set; }
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 1;
        public bool IsNumeric => true;
        public UnitType UnitType { get; private set; }
        public string Legend => "Successful";
        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => true;

        public BenchmarkCustomColumn(string columnName, UnitType unitType)
        {
            ColumnName = columnName;
            UnitType = unitType;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            string benchmarkName = benchmarkCase.Descriptor.WorkloadMethod.Name;

            BenchmarkMetadata.Instance.Load($"{benchmarkName}");
            var metadata = BenchmarkMetadata.Instance.GetMetdata(ColumnName);
            if (metadata.Count > 0)
            {
                object value = metadata.Values.LastOrDefault();
                if (value != null)
                    return value.ToString();
                else
                    return "N/A";
            }
            else
                return "N/A";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            return GetValue(summary, benchmarkCase);
        }
    }
}
using System;

namespace BenchmarkWeb.Models
{
    public class Run
    {
        public int Id { get; set; }

        // Dimensions
        public string TestClassFullName { get; set; }
        public string TestClass { get; set; }
        public string TestMethod { get; set; }
        public string Variation { get; set; }
        public string MachineName { get; set; }
        public string ProductReportingVersion { get; set; }
        public string Framework { get; set; }
        public string CustomData { get; set; }
        public DateTime RunStarted { get; set; }
        public int WarmupIterations { get; set; }
        public int Iterations { get; set; }

        // Metrics
        public long TimeElapsedAverage { get; private set; }
        public long TimeElapsedPercentile99 { get; private set; }
        public long TimeElapsedPercentile95 { get; private set; }
        public long TimeElapsedPercentile90 { get; private set; }
        public double TimeElapsedStandardDeviation { get; private set; }

        public long MemoryDeltaAverage { get; private set; }
        public long MemoryDeltaPercentile99 { get; private set; }
        public long MemoryDeltaPercentile95 { get; private set; }
        public long MemoryDeltaPercentile90 { get; private set; }
        public double MemoryDeltaStandardDeviation { get; private set; }

        public string ToolTip
        {
            get
            {
                return $"Collected: {RunStarted}{Environment.NewLine}{CustomData}";
            }
        }
    }
}

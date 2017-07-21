// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class BenchmarkSummaryProcessor
    {
        private static readonly string _machineName = GetMachineName();
        private static readonly string _framework = GetFramework();
        private static readonly SqlServerBenchmarkResultProcessor _resultProcessor = new SqlServerBenchmarkResultProcessor();
        private static readonly BenchmarkConfig _benchmarkConfig = BenchmarkConfig.Instance;

        private static string GetMachineName()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            return config["computerName"];
        }

        private static string GetFramework()
        {
#if NET461
            return ".NET Framework";
#elif NETCOREAPP1_1
            return ".NET Core 1.1";
#elif NETCOREAPP2_0
            return ".NET Core 2.0";
#endif
        }

        public virtual void Process(Summary summary)
        {
            if (_benchmarkConfig.RunIterations)
            {
                foreach (var benchmarkReport in summary.Reports)
                {
                    var benchmarkResult = new BenchmarkResult
                    {
                        MachineName = _machineName,
                        Framework = _framework,
                        Architecture = summary.HostEnvironmentInfo.Architecture,
                        EfVersion = _benchmarkConfig.ProductVersion,
                        CustomData = _benchmarkConfig.CustomData,
                        ReportingTime = DateTime.UtcNow,
                        Variation = string.Join(
                            ", ",
                            benchmarkReport.Benchmark.Parameters.Items
                                .OrderBy(pi => pi.Name)
                                .Select(pi => $"{pi.Name}={pi.Value}")),
                        // ReSharper disable once PossibleNullReferenceException
                        TimeElapsedMean = benchmarkReport.ResultStatistics.Mean / 1E6,
                        TimeElapsedPercentile90 = benchmarkReport.ResultStatistics.Percentiles.P90 / 1E6,
                        TimeElapsedPercentile95 = benchmarkReport.ResultStatistics.Percentiles.P95 / 1E6,
                        TimeElapsedStandardDeviation = benchmarkReport.ResultStatistics.StandardDeviation / 1E6,
                        TimeElapsedStandardError = benchmarkReport.ResultStatistics.StandardError / 1E6,
                        MemoryAllocated = benchmarkReport.GcStats.BytesAllocatedPerOperation * 1.0 / 1024,
                        // ReSharper disable once PossibleNullReferenceException
                        TestClassFullName = benchmarkReport.Benchmark.Target.Method.DeclaringType.FullName,
                        TestClass = benchmarkReport.Benchmark.Target.Method.DeclaringType.Name,
                        TestMethodName = benchmarkReport.Benchmark.Target.Method.Name,
                        WarmupIterations = benchmarkReport.AllMeasurements.Count(m => m.IterationMode == IterationMode.MainWarmup),
                        MainIterations = benchmarkReport.AllMeasurements.Count(m => m.IterationMode == IterationMode.MainTarget)
                    };

                    foreach (var database in _benchmarkConfig.ResultDatabases)
                    {
                        _resultProcessor.SaveSummary(database, benchmarkResult);
                    }
                }
            }
        }
    }
}

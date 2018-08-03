// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class BenchmarkSummaryProcessor
    {
        private static readonly string _machineName = GetMachineName();
        private static readonly SqlServerBenchmarkResultProcessor _resultProcessor = new SqlServerBenchmarkResultProcessor();
        private static readonly BenchmarkConfig _benchmarkConfig = BenchmarkConfig.Instance;

        private static string GetMachineName()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            return config["computerName"];
        }

        private static string NormalizeFrameworkName(string runtimeInformation)
        {
            var endIndex = runtimeInformation.IndexOf('(') - 1;
            var previewIndex = runtimeInformation.IndexOf("preview");

            if (previewIndex != -1)
            {
                endIndex = previewIndex - 1;
            }

            return runtimeInformation.Substring(0, endIndex);
        }


        public virtual void Process(Summary summary)
        {
            if (_benchmarkConfig.RunIterations)
            {
                foreach (var benchmarkReport in summary.Reports)
                {
                    if (benchmarkReport.ResultStatistics != null)
                    {
                        var testClass = benchmarkReport.BenchmarkCase.Descriptor.Type;
                        var displayName = testClass.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                        var description = testClass.GetCustomAttribute<DescriptionAttribute>()?.Description;
                        var benchmarkResult = new BenchmarkResult
                        {
                            MachineName = _machineName,
                            Framework = NormalizeFrameworkName(benchmarkReport.GetRuntimeInfo()),
                            Architecture = summary.HostEnvironmentInfo.Architecture,
                            EfVersion = _benchmarkConfig.ProductVersion,
                            CustomData = _benchmarkConfig.CustomData,
                            ReportingTime = DateTime.UtcNow,
                            Variation = description
                                ?? string.Join(
                                    ", ",
                                    benchmarkReport.BenchmarkCase.Parameters.Items
                                        .OrderBy(pi => pi.Name)
                                        .Select(pi => $"{pi.Name}={pi.Value}")),
                            TimeElapsedMean = benchmarkReport.ResultStatistics.Mean / 1E6,
                            TimeElapsedPercentile90 = benchmarkReport.ResultStatistics.Percentiles.P90 / 1E6,
                            TimeElapsedPercentile95 = benchmarkReport.ResultStatistics.Percentiles.P95 / 1E6,
                            TimeElapsedStandardDeviation = benchmarkReport.ResultStatistics.StandardDeviation / 1E6,
                            TimeElapsedStandardError = benchmarkReport.ResultStatistics.StandardError / 1E6,
                            MemoryAllocated = benchmarkReport.GcStats.BytesAllocatedPerOperation * 1.0 / 1024,
                            TestClassFullName = testClass.FullName,
                            TestClass = displayName ?? testClass.Name,
                            TestMethodName = benchmarkReport.BenchmarkCase.Descriptor.WorkloadMethod.Name,
                            WarmupIterations = benchmarkReport.AllMeasurements.Count(m => m.IterationMode == IterationMode.Overhead),
                            MainIterations = benchmarkReport.AllMeasurements.Count(m => m.IterationMode == IterationMode.Workload)
                        };

                        foreach (var database in _benchmarkConfig.ResultDatabases)
                        {
                            _resultProcessor.SaveSummary(database, benchmarkResult);
                        }
                    }
                    else
                    {
                        var testName = $"{benchmarkReport.BenchmarkCase.Descriptor.Type.FullName}.{benchmarkReport.BenchmarkCase.Descriptor.WorkloadMethod.Name}";
                        var variation = string.Join(
                            ", ",
                            benchmarkReport.BenchmarkCase.Parameters.Items
                                .OrderBy(pi => pi.Name)
                                .Select(pi => $"{pi.Name}={pi.Value}"));

                        Console.WriteLine($"##teamcity[testIgnored name='{testName}|[{variation}|]' message='Benchmark did not run correctly. No results will be saved.']");
                    }
                }
            }
        }
    }
}

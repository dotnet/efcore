// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace EntityFramework.Microbenchmarks.Core
{
    public class PerfTestRunner
    {
        private readonly TestDefinition _test;

        public PerfTestRunner(TestDefinition test)
        {
            _test = test;
        }

        public void Run(string resultDirectory)
        {
            var performanceCaseResult = new PerformanceCaseResult();
            performanceCaseResult.StartTimer();

            var result = Run(_test);

            performanceCaseResult.StopTimer();
            performanceCaseResult.Metrics = ConvertResultToMetrics(result).ToArray();

            PrintSummary(result);

            if (!result.Successful)
            {
                // log the failure using an Assert that will always fail
                Assert.True(false, result.ReportedException.Message);
            }

            WriteResultFile(resultDirectory, _test.TestName, performanceCaseResult);
        }

        private void PrintSummary(RunResult runResult)
        {
            var sb = new StringBuilder();
            sb.Append(runResult.TestName);
            if (runResult.Successful)
            {
                sb.AppendLine(" (Success) ");
                sb.Append(runResult.ElapsedMillis);
                sb.Append("ms total (");
                var iterationCount = runResult.IterationCounters.Count;
                Debug.Assert(iterationCount > 0);
                sb.Append(iterationCount.ToString(CultureInfo.InvariantCulture));
                if (iterationCount > 1)
                {
                    sb.AppendLine(" iterations)");
                    foreach (var i in new[] { 0.95, 0.99, 0.999 })
                    {
                        var percentile = (i * 100).ToString(CultureInfo.InvariantCulture);
                        var resultName = $"{runResult.TestName} - {percentile}th percentile";
                        var resultPercentile =
                            GetPercentile(runResult, i, c => c.ElapsedMillis, true);

                        sb.Append(resultPercentile);
                        sb.Append("ms ");
                        sb.Append(percentile);
                        sb.AppendLine("th percentile");
                    }
                }
                else
                {
                    sb.AppendLine(" iteration)");
                }
            }
            else
            {
                sb.Append(" (Fail) ");
                sb.Append(runResult.ReportedException.Message);
            }
            sb.AppendLine();
            Console.WriteLine(sb.ToString());
        }

        private long GetPercentile(RunResult results, double percentile, Func<IterationCounter, long> propertyAccessor, bool lowerIsBetter)
        {
            Debug.Assert(percentile > 0 && percentile < 1);
            var sortedDataPoints = lowerIsBetter ?
                results.IterationCounters.OrderBy(propertyAccessor) :
                results.IterationCounters.OrderByDescending(propertyAccessor);
            var total = sortedDataPoints.Count();
            var percentileIndex = (int)(total * percentile);
            return propertyAccessor(sortedDataPoints.ElementAt(percentileIndex));
        }

        protected RunResult Run(TestDefinition test)
        {
            //localize test settings
            var warmupCount = test.WarmupCount;
            var iterationCount = test.IterationCount;
            var testName = test.TestName ?? test.GetType() + "#" + test.GetHashCode();
            var setup = test.Setup;
            var run = test.Run;
            var cleanup = test.Cleanup;

            //validate
            if (run == null)
            {
                throw new ArgumentNullException($"Verify that test {testName} has a run action.");
            }

            //setup
            try
            {
                setup?.Invoke();
            }
            catch (Exception e)
            {
                return new RunResult(testName, e);
            }

            //warmup
            try
            {
                for (var w = 0; w < warmupCount; ++w)
                {
                    run(TestHarness.NullHarness);
                }
            }
            catch (Exception e)
            {
                return new RunResult(testName, e);
            }

            var runStopwatch = new Stopwatch();
            var iterationStopwatch = new Stopwatch();
            var iterationCounters = new List<IterationCounter>();

            //run
            try
            {
                for (var i = 0; i < iterationCount; ++i)
                {
                    iterationStopwatch.Reset();

                    var harness = new TestHarness(iterationStopwatch, runStopwatch);
                    run(harness);

                    iterationCounters.Add(
                        new IterationCounter
                            {
                                ElapsedMillis = iterationStopwatch.ElapsedMilliseconds,
                                WorkingSet = GC.GetTotalMemory(false)
                            });
                }
            }
            catch (Exception e)
            {
                return new RunResult(testName, e);
            }

            var result = new RunResult(testName, runStopwatch.ElapsedMilliseconds, GC.GetTotalMemory(false), iterationCounters);

            //cleanup
            try
            {
                if (cleanup != null)
                {
                    cleanup();
                }
            }
            catch (Exception e)
            {
                result.ReportedException = e;
            }

            //report
            return result;
        }

        private IEnumerable<PerformanceMetric> ConvertResultToMetrics(RunResult runResult)
        {
            var metrics = new List<PerformanceMetric>();
            if (runResult.Successful)
            {
                var metric = $"{"total"} {TestConfig.Instance.RuntimeFlavor}";
                metrics.Add(
                    new PerformanceMetric
                        {
                            Scenario = runResult.TestName,
                            Metric = metric,
                            Unit = "Milliseconds",
                            Value = runResult.ElapsedMillis
                        });

                if (runResult.IterationCounters.Count > 1)
                {
                    foreach (var i in new[] { 0.95, 0.99, 0.999 })
                    {
                        var percentile = (i * 100).ToString(CultureInfo.InvariantCulture);
                        var resultPercentile = GetPercentile(runResult, i, c => c.ElapsedMillis, true);
                        long resultMemoryPercentile = 0;

                        resultMemoryPercentile = GetPercentile(runResult, i,
                            c => c.WorkingSet, true);

                        metric = $"{percentile}th percentile {TestConfig.Instance.RuntimeFlavor}";

                        metrics.Add(
                            new PerformanceMetric
                                {
                                    Scenario = runResult.TestName,
                                    Metric = metric,
                                    Unit = "Milliseconds",
                                    Value = resultPercentile
                                });
                    }
                }
            }
            return metrics;
        }

        private static void WriteResultFile(string resultDirectory, string scenario, PerformanceCaseResult performanceCaseResult)
        {
            if (!Directory.Exists(resultDirectory))
            {
                Directory.CreateDirectory(resultDirectory);
            }

            var jsonData = JsonConvert.SerializeObject(performanceCaseResult, Formatting.Indented);
            var jsonFilename = $"result_{scenario}_{TestConfig.Instance.RuntimeFlavor}.json";
            File.WriteAllText(Path.Combine(resultDirectory, jsonFilename), jsonData);

            var csvFilename = string.Format(Path.Combine(resultDirectory, "results.csv"));
            if (!File.Exists(csvFilename))
            {
                File.WriteAllText(csvFilename, "StartTime,EndTime,Scenario,Metric,Unit,Value");
            }

            foreach (var item in performanceCaseResult.Metrics)
            {
                File.AppendAllText(csvFilename, $"\r\n{performanceCaseResult.StartTime},{performanceCaseResult.EndTime},{item.Scenario},{item.Metric},{item.Unit},{item.Value}");
            }
        }
    }
}

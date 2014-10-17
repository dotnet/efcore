// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using KPerf;
using Newtonsoft.Json;

namespace Microbenchmarks.Core
{
    public class PerfTestRunner
    {
        public string PathToResultsFile { get; set; }
        private ICollection<TestDefinition> Tests { get; set; }

        public PerfTestRunner()
        {
            Tests = new List<TestDefinition>();
        }

        public void Register(TestDefinition test)
        {
            Tests.Add(test);
        }

        public void RunTests(string resultDirectory)
        {
            var results = new List<PerformanceMetric>();
            foreach (var testDefinition in Tests)
            {
                var result = Run(testDefinition);
                PrintSummary(result);
                results.AddRange(ConvertResultToMetrics(result));
            }

            var parsedData = JsonConvert.SerializeObject(results);
            File.WriteAllText(Path.Combine(resultDirectory, "result.json"), parsedData);
        }

        private void PrintSummary(List<RunResult> results)
        {
            foreach (var runResult in results)
            {
                PrintSummary(runResult);
            }
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
                        var result = GetPercentile(runResult, i);

                        sb.Append(result);
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
            var output = sb.ToString();
            Console.WriteLine(output);
        }

        private long GetPercentile(RunResult results, double percentile)
        {
            Debug.Assert(percentile > 0 && percentile < 1);
            var sortedDataPoints = results.IterationCounters.OrderBy(e => e.ElapsedMillis);
            var total = sortedDataPoints.Count();
            var percentileIndex = (int)(total * percentile);
            return sortedDataPoints.ElementAt(percentileIndex).ElapsedMillis;
        }

        protected RunResult RunAndPrintSummary(TestDefinition test)
        {
            var result = Run(test);
            PrintSummary(result);
            return result;
        }

        protected RunResult Run(TestDefinition test)
        {
            //localize test settings
            var warmupCount = 0;
            if (test.WarmupCount.HasValue)
            {
                warmupCount = test.WarmupCount.Value;
            }
            var iterationCount = 100;
            if (test.IterationCount.HasValue)
            {
                iterationCount = test.IterationCount.Value;
            }
            var testName = test.TestName ?? test.GetType() + "#" + test.GetHashCode();
            var setup = test.Setup;
            var run = test.Run;
            var cleanup = test.Cleanup;

            //validate
            if (run == null)
            {
                throw new ArgumentNullException(string.Format("Verify that test {0} has a run action.", testName));
            }

            //setup
            try
            {
                if (setup != null)
                {
                    setup();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    return new RunResult(testName, e.InnerException);
                }
                return new RunResult(testName, e);
            }

            //warmup
            try
            {
                for (var w = 0; w < warmupCount; ++w)
                {
                    run();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    return new RunResult(testName, e.InnerException);
                }
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
                    iterationStopwatch.Restart();
                    runStopwatch.Start();

                    run();

                    runStopwatch.Stop();
                    iterationStopwatch.Stop();
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
                if (e.InnerException != null)
                {
                    return new RunResult(testName, e.InnerException);
                }
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
                if (e.InnerException != null)
                {
                    return new RunResult(testName, e.InnerException);
                }
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
                metrics.Add(
                    new PerformanceMetric
                        {
                            Scenario = runResult.TestName,
                            Metric = "total",
                            Unit = "Milliseconds",
                            Value = runResult.ElapsedMillis
                        });

                if (runResult.IterationCounters.Count > 1)
                {
                    foreach (var i in new[] { 0.95, 0.99, 0.999 })
                    {
                        var percentile = (i * 100).ToString(CultureInfo.InvariantCulture);
                        var resultPercentile = GetPercentile(runResult, i);
                        var metric = string.Format("{0}th percentile", percentile);

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
    }
}

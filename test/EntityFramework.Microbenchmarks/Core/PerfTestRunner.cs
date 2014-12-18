// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Xunit;

namespace EntityFramework.Microbenchmarks.Core
{
    public class PerfTestRunner
    {
        public string PathToResultsFile { get; set; }
        private ICollection<TestDefinitionBase> Tests { get; set; }

        public PerfTestRunner()
        {
            Tests = new List<TestDefinitionBase>();
        }

        public void Register(TestDefinitionBase test)
        {
            Tests.Add(test);
        }

        public void Register(string testName, Action testAction)
        {
            Register(
                new TestDefinition
                    {
                        TestName = testName,
                        Run = testAction
                    });
        }

        public void Register(Action testAction)
        {
            Register(
                new TestDefinition
                    {
                        Run = testAction
                    });
        }

        public void RunTests(string resultDirectory)
        {
            var results = new List<PerformanceMetric>();
            var failedRunResult = new List<Exception>();
            var performanceCaseResult = new PerformanceCaseResult();
            performanceCaseResult.StartTimer();
            
            foreach (var testDefinition in Tests)
            {
                var result = Run(testDefinition);
                PrintSummary(result);
                results.AddRange(ConvertResultToMetrics(result));
                if (!result.Successful)
                {
                    failedRunResult.Add(result.ReportedException);
                }
            }

            performanceCaseResult.StopTimer();
            performanceCaseResult.Metrics = results.ToArray();

            Assert.False(failedRunResult.Any(), failedRunResult.Any() ? failedRunResult.First().Message : string.Empty);
            Assert.False(results.Count == 0, "tests returned no results");

            var parsedData = JsonConvert.SerializeObject(performanceCaseResult, Formatting.Indented);

            if (!Directory.Exists(resultDirectory))
            {
                Directory.CreateDirectory(resultDirectory);
            }

            var filename = string.Format("result_{0}_{1}.json", results.First().Scenario.Replace(' ', '_'), TestConfig.Instance.RuntimeFlavor);

            File.WriteAllText(Path.Combine(resultDirectory, filename), parsedData);
        }

        private void PrintSummary(List<RunResult> results)
        {
            foreach (var runResult in results)
            {
                PrintSummary(runResult);
            }
        }

        private void PrintSummary(RunResultBase runResult)
        {
            if (runResult is RunResult)
            {
                PrintRunSummary(runResult as RunResult);
            }
            else if (runResult is ThreadedRunResult)
            {
                PrintRunSummary(runResult as ThreadedRunResult);
            }
        }

        private void PrintRunSummary(ThreadedRunResult runResult)
        {
            var sb = new StringBuilder();
            sb.Append(runResult.TestName);
            if (runResult.Successful)
            {
                sb.AppendLine(" (Success) ");
                sb.Append(runResult.RequestsPerSecond);
                sb.Append(" RPS total (");
                var samplePoints = runResult.IterationCounters.Count;
                Debug.Assert(samplePoints > 0);
                sb.Append(samplePoints.ToString(CultureInfo.InvariantCulture));
                if (samplePoints > 1)
                {
                    sb.AppendLine(" samples)");
                    foreach (var i in new[] { 0.95, 0.99, 0.999 })
                    {
                        var percentile = (i * 100).ToString(CultureInfo.InvariantCulture);
                        var resultPercentile = GetPercentile(runResult, i, c => ((ThreadedIterationCounter)c).RequestsPerSecond, false);
                        var resultName = string.Format("{0} - {1}th percentile", runResult.TestName, percentile);

                        sb.Append(resultPercentile);
                        sb.Append(" RPS ");
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

        private void PrintRunSummary(RunResult runResult)
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
                        var resultName = string.Format("{0} - {1}th percentile", runResult.TestName, percentile);
                        var resultPercentile =
                            GetPercentile(runResult, i, c => ((IterationCounter)c).ElapsedMillis, true);

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

        private long GetPercentile(RunResultBase results, double percentile, Func<IterationCounterBase, long> propertyAccessor, bool lowerIsBetter)
        {
            Debug.Assert(percentile > 0 && percentile < 1);
            var sortedDataPoints = lowerIsBetter ?
                results.IterationCounters.OrderBy(propertyAccessor) :
                results.IterationCounters.OrderByDescending(propertyAccessor);
            var total = sortedDataPoints.Count();
            var percentileIndex = (int)(total * percentile);
            return propertyAccessor(sortedDataPoints.ElementAt(percentileIndex));
        }

        protected RunResultBase Run(TestDefinitionBase test)
        {
            var result = default(RunResultBase);
            if (test is TestDefinition)
            {
                result = Run(test as TestDefinition);
            }
            else if (test is ThreadedTestDefinition)
            {
                result = Run(test as ThreadedTestDefinition);
            }
            else
            {
                throw new ArgumentException("Unexpected test definition type");
            }
            return result;
        }

        protected ThreadedRunResult Run(ThreadedTestDefinition test)
        {
            //localize test settings
            var threadCount = 8;
            if (test.ThreadCount.HasValue)
            {
                threadCount = test.ThreadCount.Value;
            }
            var warmupDuration = 10000;
            if (test.WarmupDuration.HasValue)
            {
                warmupDuration = test.WarmupDuration.Value;
            }
            var testDuration = 60000;
            if (test.TestDuration.HasValue)
            {
                testDuration = test.TestDuration.Value;
            }
            var testName = test.TestName ?? test.GetType() + "#" + test.GetHashCode();

            var setup = test.Setup;
            var run = test.Run;
            var cleanup = test.Cleanup;
            var threadStateFactory = test.ThreadStateFactory;

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
                return new ThreadedRunResult(testName, e);
            }

            var iterationSnaps = new List<Tuple<long, long>>();

            var iterationCounter = 0L;
            var testStopSignal = false;

            Action decoratedRunAction = () =>
                {
                    var state = threadStateFactory();
                    do
                    {
                        run(state);
                        Interlocked.Increment(ref iterationCounter);
                    }
                    while (!testStopSignal);
                };

            Action rpsCounterAction = () =>
                {
                    do
                    {
                        var current = Interlocked.Read(ref iterationCounter);
                        iterationSnaps.Add(new Tuple<long, long>(current, GC.GetTotalMemory(false)));
                        Thread.Sleep(1000);
                    }
                    while (!testStopSignal);
                };

            var workers = new List<Thread>();
            //add worker tasks
            for (var i = 0; i < threadCount; ++i)
            {
                workers.Add(new Thread(new ThreadStart(decoratedRunAction)));
            }

            long totalExecutionTime;
            //run
            try
            {
                var runStopWatch = new Stopwatch();
                runStopWatch.Start();
                foreach (var worker in workers)
                {
                    worker.Start();
                }
                // warmup
                runStopWatch.Restart();
                do
                {
                    Thread.Sleep(1000);
                }
                while (runStopWatch.ElapsedMilliseconds < warmupDuration);
                //add rps counter thread
                Thread counter = null;
                workers.Add(counter = new Thread(new ThreadStart(rpsCounterAction)));
                counter.Start();
                //actual run
                runStopWatch.Restart();
                do
                {
                    Thread.Sleep(1000);
                }
                while (runStopWatch.ElapsedMilliseconds < testDuration);
                testStopSignal = true;
                runStopWatch.Stop();
                totalExecutionTime = runStopWatch.ElapsedMilliseconds / 1000;
            }
            catch (Exception e)
            {
                return new ThreadedRunResult(testName, e);
            }

            //summarize iteration counters
            var prevCummulativeValue = 0L;
            var iterationCounters = new List<ThreadedIterationCounter>();
            foreach (var snap in iterationSnaps)
            {
                if (snap.Item1 == 0)
                {
                    continue;
                }

                var iterationRps = snap.Item1 - prevCummulativeValue;
                iterationCounters.Add(new ThreadedIterationCounter
                    {
                        RequestsPerSecond = iterationRps,
                        WorkingSet = snap.Item2
                    });
                prevCummulativeValue += iterationRps;
            }

            var result = new ThreadedRunResult(testName, iterationCounter / totalExecutionTime, GC.GetTotalMemory(false), iterationCounters);

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
            var run = test.RunWithCollector;
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
                return new RunResult(testName, e);
            }

            //warmup
            try
            {
                for (var w = 0; w < warmupCount; ++w)
                {
                    run(new MetricCollector(new Stopwatch(), new Stopwatch()));
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

                    var collector = new MetricCollector(iterationStopwatch, runStopwatch);
                    run(collector);

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

        private IEnumerable<PerformanceMetric> ConvertResultToMetrics(RunResultBase runResult)
        {
            var metrics = new List<PerformanceMetric>();
            if (runResult.Successful)
            {
                var metric = string.Format("{0} {1}", "total", TestConfig.Instance.RuntimeFlavor);
                metrics.Add(
                    new PerformanceMetric
                        {
                            Scenario = runResult.TestName,
                            Metric = metric,
                            Unit = "Milliseconds",
                            Value = runResult.ElapsedMillis
                        });

                
                Func<IterationCounterBase, long> propertyAccessor;
                string unit;

                if (runResult.IterationCounters.First() is ThreadedIterationCounter)
                {
                    propertyAccessor = (c => ((ThreadedIterationCounter)c).RequestsPerSecond);
                    unit = "RPS";
                }
                else if (runResult.IterationCounters.First() is IterationCounter)
                {
                    propertyAccessor = (c => ((IterationCounter)c).ElapsedMillis);
                    unit = "Milliseconds";
                }
                else
                {
                    throw new ArgumentException("Unexpected iteration counter type: " + runResult.IterationCounters.First().GetType());
                }

                if (runResult.IterationCounters.Count > 1)
                {
                    foreach (var i in new[] { 0.95, 0.99, 0.999 })
                    {
                        var percentile = (i * 100).ToString(CultureInfo.InvariantCulture);
                        long resultPercentile = GetPercentile(runResult, i, propertyAccessor, true);
                        long resultMemoryPercentile = 0;
                       
                        resultMemoryPercentile = GetPercentile(runResult, i,
                            c => c.WorkingSet, true);

                        metric = string.Format("{0}th percentile {1}", percentile, TestConfig.Instance.RuntimeFlavor);

                        metrics.Add(
                            new PerformanceMetric
                                {
                                    Scenario = runResult.TestName,
                                    Metric = metric,
                                    Unit = unit,
                                    Value = resultPercentile
                                });
                    }
                }
            }
            return metrics;
        }
    }
}

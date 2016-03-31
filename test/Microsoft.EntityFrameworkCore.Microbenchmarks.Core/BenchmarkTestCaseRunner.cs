// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
    public class BenchmarkTestCaseRunner : XunitTestCaseRunner
    {
        private static readonly string _machineName = GetMachineName();
        private readonly IMessageSink _diagnosticMessageSink;

        public BenchmarkTestCaseRunner(
            BenchmarkTestCase testCase,
            string displayName,
            string skipReason,
            object[] constructorArguments,
            object[] testMethodArguments,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            IMessageSink diagnosticMessageSink)
            : base(
                testCase,
                displayName,
                skipReason,
                constructorArguments,
                testMethodArguments,
                messageBus,
                aggregator,
                cancellationTokenSource)
        {
            TestCase = testCase;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public new BenchmarkTestCase TestCase { get; }

        protected override async Task<RunSummary> RunTestAsync()
        {
            var runSummary = new BenchmarkRunSummary
            {
                TestClassFullName = TestCase.TestMethod.TestClass.Class.Name,
                TestClass = TestCase.TestMethod.TestClass.Class.Name.Split('.').Last(),
                TestMethod = TestCase.TestMethodName,
                Variation = TestCase.Variation,
                ProductReportingVersion = BenchmarkConfig.Instance.ProductReportingVersion,
                RunStarted = DateTime.UtcNow,
                MachineName = _machineName,
                Framework = PlatformServices.Default.Runtime.RuntimeType,
                Architecture = IntPtr.Size > 4 ? "x64" : "x86",
                WarmupIterations = TestCase.WarmupIterations,
                Iterations = TestCase.Iterations,
                CustomData = BenchmarkConfig.Instance.CustomData
            };

            for (var i = 0; i < TestCase.WarmupIterations; i++)
            {
                var runner = CreateRunner(i + 1, TestCase.WarmupIterations, TestCase.Variation, warmup: true);
                runSummary.Aggregate(await runner.RunAsync());
            }

            for (var i = 0; i < TestCase.Iterations; i++)
            {
                TestCase.MetricCollector.Reset();
                var runner = CreateRunner(i + 1, TestCase.Iterations, TestCase.Variation, warmup: false);
                var iterationSummary = new BenchmarkIterationSummary();
                iterationSummary.Aggregate(await runner.RunAsync(), TestCase.MetricCollector);
                runSummary.Aggregate(iterationSummary);
            }

            if (runSummary.Failed != 0)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"No valid results for {TestCase.DisplayName}. {runSummary.Failed} of {TestCase.Iterations + TestCase.WarmupIterations} iterations failed."));
            }
            else
            {
                runSummary.PopulateMetrics();
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage(runSummary.ToString()));

                foreach (var database in BenchmarkConfig.Instance.ResultDatabases)
                {
                    try
                    {
                        new SqlServerBenchmarkResultProcessor(database).SaveSummary(runSummary);
                    }
                    catch (Exception ex)
                    {
                        _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Failed to save results to {database}{Environment.NewLine} {ex}"));
                        throw;
                    }
                }
            }

            return runSummary;
        }

        private XunitTestRunner CreateRunner(int iteration, int totalIterations, string variation, bool warmup)
        {
            var name = $"{DisplayName} [Stage: {(warmup ? "Warmup" : "Collection")}] [Iteration: {iteration}/{totalIterations}]";

            return new XunitTestRunner(
                new XunitTest(TestCase, name),
                MessageBus,
                TestClass,
                ConstructorArguments,
                TestMethod,
                TestMethodArguments,
                SkipReason,
                BeforeAfterAttributes,
                Aggregator,
                CancellationTokenSource);
        }

        private static string GetMachineName()
        {
#if NETSTANDARDAPP1_5
            var config = new ConfigurationBuilder()
                .SetBasePath(".")
                .AddEnvironmentVariables()
                .Build();

            return config["computerName"];
#else
            return Environment.MachineName;
#endif
        }
    }
}

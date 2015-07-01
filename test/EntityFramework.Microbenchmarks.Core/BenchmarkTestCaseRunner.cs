// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

#if DNXCORE50
using Microsoft.Framework.Configuration;
#endif

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkTestCaseRunner : XunitTestCaseRunner
    {
#if DNXCORE50
        private static Lazy<string> _machineName = new Lazy<string>(() =>
        {
            var config = new ConfigurationBuilder(".")
                .AddEnvironmentVariables()
                .Build();

            return config.Get("computerName");
        });
#else
        private static Lazy<string> _machineName = new Lazy<string>(() => Environment.MachineName);
#endif

#if DNX451 || DNXCORE50
        private static Lazy<string> _framwork = new Lazy<string>(() => 
        {
            var services = Microsoft.Framework.Runtime.Infrastructure.CallContextServiceLocator.Locator.ServiceProvider; 
            var env = (Microsoft.Framework.Runtime.IRuntimeEnvironment)services.GetService(typeof(Microsoft.Framework.Runtime.IRuntimeEnvironment)); 
            return "DNX." + env.RuntimeType;
        });
#else
        private static Lazy<string> _framwork = new Lazy<string>(() => ".NETFramework");
#endif

        public BenchmarkTestCaseRunner(
                BenchmarkTestCase testCase,
                string displayName,
                string skipReason,
                object[] constructorArguments,
                object[] testMethodArguments,
                IMessageBus messageBus,
                ExceptionAggregator aggregator,
                CancellationTokenSource cancellationTokenSource)
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
        }

        public new BenchmarkTestCase TestCase { get; private set; }

        protected override async Task<RunSummary> RunTestAsync()
        {
            var runSummary = new BenchmarkRunSummary
            {
                TestClassFullName = TestCase.TestMethod.TestClass.Class.Name,
                TestClass = TestCase.TestMethod.TestClass.Class.Name.Split('.').Last(),
                TestMethod = TestCase.DisplayName,
                Variation = TestCase.Variation,
                ProductReportingVersion = BenchmarkConfig.Instance.ProductReportingVersion,
                RunStarted = DateTime.UtcNow,
                MachineName = _machineName.Value,
                Framework = _framwork.Value,
                WarmupIterations = TestCase.WarmupIterations,
                Iterations = TestCase.Iterations
            };

            for (int i = 0; i < TestCase.WarmupIterations; i++)
            {
                var runner = CreateRunner(i + 1, TestCase.WarmupIterations, TestCase.Variation, warmup: true);
                runSummary.Aggregate(await runner.RunAsync());
            }

            for (int i = 0; i < TestCase.Iterations; i++)
            {
                TestCase.MetricCollector.Reset();
                var runner = CreateRunner(i + 1, TestCase.Iterations, TestCase.Variation, warmup: false);
                var iterationSummary = new BenchmarkIterationSummary();
                iterationSummary.Aggregate(await runner.RunAsync(), TestCase.MetricCollector);
                runSummary.Aggregate(iterationSummary);
            }

            runSummary.PopulateMetrics();
            if (BenchmarkConfig.Instance.ResultsDatabase != null)
            {
                new SqlServerBenchmarkResultProcessor(BenchmarkConfig.Instance.ResultsDatabase).SaveSummary(runSummary);
            }

            return runSummary;
        }

        private XunitTestRunner CreateRunner(int iteration, int totalIterations, string variation, bool warmup)
        {
            var name = $"{DisplayName} [Stage: {(warmup ? "Warmup" : "Collection")}] [Iteration: {iteration}/{totalIterations}] [Variation: {variation}]";

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
    }
}

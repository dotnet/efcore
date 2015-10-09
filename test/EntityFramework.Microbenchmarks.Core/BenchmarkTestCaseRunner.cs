// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

#if DNXCORE50 || DNX451
using Microsoft.Extensions.Configuration;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using XunitDiagnosticMessage = Xunit.DiagnosticMessage;
#else
using XunitDiagnosticMessage = Xunit.Sdk.DiagnosticMessage;
#endif

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkTestCaseRunner : XunitTestCaseRunner
    {
        private static string _machineName = GetMachineName();
        private static string _framework = GetFramework();
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

        public new BenchmarkTestCase TestCase { get; private set; }

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
                Framework = _framework,
                Architecture = IntPtr.Size > 4 ? "x64" : "x86",
                WarmupIterations = TestCase.WarmupIterations,
                Iterations = TestCase.Iterations,
                CustomData = BenchmarkConfig.Instance.CustomData
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

            if (runSummary.Failed != 0)
            {
                _diagnosticMessageSink.OnMessage(new XunitDiagnosticMessage($"No valid results for {TestCase.DisplayName}. {runSummary.Failed} of {TestCase.Iterations + TestCase.WarmupIterations} iterations failed."));
            }
            else
            {
                runSummary.PopulateMetrics();
                _diagnosticMessageSink.OnMessage(new XunitDiagnosticMessage(runSummary.ToString()));

                foreach (var database in BenchmarkConfig.Instance.ResultDatabases)
                {
                    try
                    {
                        new SqlServerBenchmarkResultProcessor(database).SaveSummary(runSummary);
                    }
                    catch (Exception ex)
                    {
                        _diagnosticMessageSink.OnMessage(new XunitDiagnosticMessage($"Failed to save results to {database}{Environment.NewLine} {ex.ToString()}"));
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

        private static string GetFramework()
        {
#if DNX451 || DNXCORE50
            var services = CallContextServiceLocator.Locator.ServiceProvider; 
            var env = (IRuntimeEnvironment)services.GetService(typeof(IRuntimeEnvironment)); 
            return "DNX." + env.RuntimeType;
#else
            return ".NETFramework";
#endif
        }

        private static string GetMachineName()
        {
#if DNXCORE50
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

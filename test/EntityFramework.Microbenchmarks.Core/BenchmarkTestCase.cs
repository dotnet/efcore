// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkTestCase : XunitTestCase
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public BenchmarkTestCase(
                int iterations,
                int warmupIterations,
                string variation,
                IMessageSink diagnosticMessageSink,
                ITestMethod testMethod,
                object[] testMethodArguments)
            : base(diagnosticMessageSink, Xunit.Sdk.TestMethodDisplay.Method, testMethod, null)
        {
            // Override display name to avoid getting info about TestMethodArguments in the
            // name (this is covered by the concept of Variation for benchmarks)
            var suppliedDisplayName = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute))
                .First()
                .GetNamedArgument<string>("DisplayName");

            _diagnosticMessageSink = diagnosticMessageSink;
            DisplayName = suppliedDisplayName ?? BaseDisplayName;
            Variation = variation;
            Iterations = iterations;
            WarmupIterations = warmupIterations;

            var methodArguments = new List<object> { MetricCollector };
            if (testMethodArguments != null)
            {
                methodArguments.AddRange(testMethodArguments);
            }

            TestMethodArguments = methodArguments.ToArray();
        }

        public string Variation { get; private set; }
        public int Iterations { get; private set; }
        public int WarmupIterations { get; private set; }
        public MetricCollector MetricCollector { get; private set; } = new MetricCollector();

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new BenchmarkTestCaseRunner(
                this, 
                DisplayName, 
                SkipReason, 
                constructorArguments, 
                TestMethodArguments, 
                messageBus, 
                aggregator,
                cancellationTokenSource,
                _diagnosticMessageSink).RunAsync();
        }
    }

}

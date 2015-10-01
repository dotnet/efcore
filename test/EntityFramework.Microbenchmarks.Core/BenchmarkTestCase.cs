// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkTestCase : BenchmarkTestCaseBase
    {
        private readonly IMetricCollector _metricCollector = new MetricCollector();

        public BenchmarkTestCase(
                int iterations,
                int warmupIterations,
                string variation,
                IMessageSink diagnosticMessageSink,
                ITestMethod testMethod,
                object[] testMethodArguments)
            : base(variation, diagnosticMessageSink, testMethod, testMethodArguments)
        {
            Iterations = iterations;
            WarmupIterations = warmupIterations;
        }

        public override IMetricCollector MetricCollector => _metricCollector;
        public int Iterations { get; protected set; }
        public int WarmupIterations { get; protected set; }

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
                DiagnosticMessageSink).RunAsync();
        }
    }

}

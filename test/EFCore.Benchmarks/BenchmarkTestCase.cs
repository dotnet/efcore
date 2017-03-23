// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class BenchmarkTestCase : BenchmarkTestCaseBase
    {
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public BenchmarkTestCase() : base()
        { }

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

        public int Iterations { get; protected set; }
        public int WarmupIterations { get; protected set; }

        protected override IMetricCollector CreateMetricCollector()
        {
            return new MetricCollector();
        }

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            => new BenchmarkTestCaseRunner(
                this,
                DisplayName,
                SkipReason,
                constructorArguments,
                TestMethodArguments,
                messageBus,
                aggregator,
                cancellationTokenSource,
                diagnosticMessageSink).RunAsync();

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            Iterations = data.GetValue<int>(nameof(Iterations));
            WarmupIterations = data.GetValue<int>(nameof(WarmupIterations));
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue(nameof(Iterations), Iterations);
            data.AddValue(nameof(WarmupIterations), WarmupIterations);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
    public class NonCollectingBenchmarkTestCase : BenchmarkTestCaseBase
    {
        public NonCollectingBenchmarkTestCase(
            string variation,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(variation, diagnosticMessageSink, testMethod, testMethodArguments)
        {
        }

        public override IMetricCollector MetricCollector { get; } = new NullMetricCollector();
    }
}

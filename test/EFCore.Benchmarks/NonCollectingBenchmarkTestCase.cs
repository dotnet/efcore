// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class NonCollectingBenchmarkTestCase : BenchmarkTestCaseBase
    {
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public NonCollectingBenchmarkTestCase() : base()
        { }

        public NonCollectingBenchmarkTestCase(
            string variation,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(variation, diagnosticMessageSink, testMethod, testMethodArguments)
        {
        }

        protected override IMetricCollector CreateMetricCollector()
        {
            return new NullMetricCollector();
        }
    }
}

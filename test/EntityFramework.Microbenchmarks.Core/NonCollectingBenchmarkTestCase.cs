// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace EntityFramework.Microbenchmarks.Core
{
    public class NonCollectingBenchmarkTestCase : BenchmarkTestCaseBase
    {
        private readonly IMetricCollector _metricCollector = new NullMetricCollector();

        public NonCollectingBenchmarkTestCase(
               string variation,
               IMessageSink diagnosticMessageSink,
               ITestMethod testMethod,
               object[] testMethodArguments)
            : base(variation, diagnosticMessageSink, testMethod, testMethodArguments)
        {
        }

        public override IMetricCollector MetricCollector => _metricCollector;
    }
}

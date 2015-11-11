// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;

namespace EntityFramework.Microbenchmarks.Core
{
    public abstract class BenchmarkTestCaseBase : XunitTestCase
    {
        public BenchmarkTestCaseBase(
            string variation,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(diagnosticMessageSink, TestMethodDisplay.Method, testMethod, null)
        {
            // Override display name to avoid getting info about TestMethodArguments in the
            // name (this is covered by the concept of Variation for benchmarks)
            var name = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute))
                .First()
                .GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            TestMethodName = name;
            DisplayName = $"{name} [Variation: {variation}]";

            DiagnosticMessageSink = diagnosticMessageSink;
            Variation = variation;

            var methodArguments = new List<object> { MetricCollector };
            if (testMethodArguments != null)
            {
                methodArguments.AddRange(testMethodArguments);
            }

            TestMethodArguments = methodArguments.ToArray();
        }

        protected IMessageSink DiagnosticMessageSink { get; private set; }
        public abstract IMetricCollector MetricCollector { get; }
        public string TestMethodName { get; protected set; }
        public string Variation { get; protected set; }

        protected override string GetUniqueID()
        {
            return $"{TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name}{TestMethod.TestClass.Class.Name}{TestMethod.Method.Name}{Variation}";
        }
    }
}

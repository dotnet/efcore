// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    public class ConditionalTheoryDiscoverer : TheoryDiscoverer
    {
        public ConditionalTheoryDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            var skipReason = testMethod.EvaluateSkipConditions();
            return skipReason != null
                ? new[] { new SkippedTestCase(skipReason, DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod) }
                : base.CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
        {
            var skipReason = testMethod.EvaluateSkipConditions();
            return skipReason != null
                ? CreateTestCasesForSkippedDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, skipReason)
                : base.CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit
{
    public class ConditionalAttributeDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public ConditionalAttributeDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            var skipReason = EvaluateSkipConditions(testMethod);

            IXunitTestCaseDiscoverer innerDiscoverer;
            if (testMethod.Method.GetCustomAttributes(typeof(TheoryAttribute)).Any())
            {
                innerDiscoverer = new TheoryDiscoverer(_diagnosticMessageSink);
            }
            else
            {
                innerDiscoverer = new FactDiscoverer(_diagnosticMessageSink);
            }

            var res = innerDiscoverer.Discover(discoveryOptions, testMethod, factAttribute)
                .Select(testCase => new SkipReasonTestCase(skipReason, testCase));
            return res;
        }

        private string EvaluateSkipConditions(ITestMethod testMethod)
        {
            var conditionAttributes = testMethod.Method
                .GetCustomAttributes(typeof(ITestCondition))
                .OfType<ReflectionAttributeInfo>()
                .Select(attributeInfo => attributeInfo.Attribute)
                .ToList();

            conditionAttributes.AddRange(testMethod.TestClass.Class
                .GetCustomAttributes(typeof(ITestCondition))
                .OfType<ReflectionAttributeInfo>()
                .Select(attributeInfo => attributeInfo.Attribute));

            foreach (ITestCondition condition in conditionAttributes)
            {
                if (!condition.IsMet)
                {
                    return condition.SkipReason;
                }
            }

            return null;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit
{
    internal class SkipXunitTheoryTestCase : XunitTheoryTestCase
    {
        public SkipXunitTheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod)
        {
        }

        protected override string GetSkipReason(IAttributeInfo factAttribute) => EvaluateSkipConditions(TestMethod) ?? base.GetSkipReason(factAttribute);

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

            var reasons = conditionAttributes.Cast<ITestCondition>()
                .Where(condition => !condition.IsMet)
                .Select(condition => condition.SkipReason)
                .ToList();

            return reasons.Count > 0 ? string.Join(Environment.NewLine, reasons) : null;
        }
    }
}

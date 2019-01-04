// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    public class ConditionalTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        public ConditionalTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            var skipReason = EvaluateSkipConditions(AssemblyInfo);
            if (string.IsNullOrEmpty(skipReason))
            {
                base.RunTestCases(testCases, executionMessageSink, executionOptions);
            }
            else
            {
                skipReason = "Unmet assembly test condition(s): " + skipReason;
                var testCaseCount = testCases.Count();
                using (var messageBus = CreateMessageBus(executionMessageSink, executionOptions))
                {
                    foreach (var test in testCases.Select(testCase => new XunitTest(testCase, testCase.DisplayName)))
                    {
                        messageBus.QueueMessage(new TestStarting(test));
                        messageBus.QueueMessage(new TestSkipped(test, skipReason));
                        messageBus.QueueMessage(new TestFinished(test, 0, null));
                    }

                    messageBus.QueueMessage(new TestAssemblyFinished(testCases, TestAssembly, 0, testCaseCount, 0, testCaseCount));
                }
            }
        }

        private static IMessageBus CreateMessageBus(IMessageSink messageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            return executionOptions.SynchronousMessageReportingOrDefault() ? new SynchronousMessageBus(messageSink) : (IMessageBus)new MessageBus(messageSink);
        }

        private static string EvaluateSkipConditions(IAssemblyInfo assembly)
        {
            var reasons = assembly
                .GetCustomAttributes(typeof(ITestCondition))
                .OfType<ReflectionAttributeInfo>()
                .Select(attributeInfo => (ITestCondition)attributeInfo.Attribute)
                .Where(condition => !condition.IsMet)
                .Select(condition => condition.SkipReason)
                .ToList();

            return reasons.Count > 0 ? string.Join(Environment.NewLine, reasons) : null;
        }
    }
}

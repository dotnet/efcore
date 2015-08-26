// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit
{
    internal class SkipReasonTestCase : IXunitTestCase
    {
        private readonly IXunitTestCase _wrappedTestCase;

        public SkipReasonTestCase(string skipReason, IXunitTestCase wrappedTestCase)
        {
            SkipReason = wrappedTestCase.SkipReason ?? skipReason;
            _wrappedTestCase = wrappedTestCase;
        }

        public string DisplayName => _wrappedTestCase.DisplayName;

        public IMethodInfo Method => _wrappedTestCase.Method;

        public string SkipReason { get; }

        public ISourceInformation SourceInformation
        {
            get { return _wrappedTestCase.SourceInformation; }
            set { _wrappedTestCase.SourceInformation = value; }
        }

        public ITestMethod TestMethod => _wrappedTestCase.TestMethod;

        public object[] TestMethodArguments => _wrappedTestCase.TestMethodArguments;

        public Dictionary<string, List<string>> Traits => _wrappedTestCase.Traits;

        public string UniqueID => _wrappedTestCase.UniqueID;

        public void Deserialize(IXunitSerializationInfo info) => _wrappedTestCase.Deserialize(info);

        public Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            XunitTestCaseRunner runner;
            if (_wrappedTestCase is XunitTheoryTestCase)
            {
                runner = new XunitTheoryTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource);
            }
            else
            {
                runner = new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource);
            }
            return runner.RunAsync();
        }

        public void Serialize(IXunitSerializationInfo info) => _wrappedTestCase.Serialize(info);
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests;
using Microsoft.Data.Entity.Redis.FunctionalTests;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class CrossStoreXunitTestFramework : XunitTestFramework
    {
        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new CrossStoreTestExecutor(assemblyName, SourceInformationProvider);
        }
    }

    public class CrossStoreTestExecutor : RedisXunitTestExecutor
    {
        public CrossStoreTestExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider)
            : base(assemblyName, sourceInformationProvider)
        {
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions)
        {
            var cases = testCases.Where(t =>
                !t.Class.GetCustomAttributes(typeof(RunIfConfiguredAttribute)).Any()
                || TestConfig.Instance.IsConfigured);
            base.RunTestCases(cases, messageSink, executionOptions);
        }
    }
}

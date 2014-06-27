// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class AtsTestFramework : XunitTestFramework
    {
        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new AtsTestExecutor(assemblyName, SourceInformationProvider);
        }
    }

    public class AtsTestExecutor : XunitTestFrameworkExecutor
    {
        public AtsTestExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider)
            : base(assemblyName, sourceInformationProvider)
        {
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions)
        {
            var cases = testCases.Where(t =>
                t.Class.GetCustomAttributes(typeof(RunIfConfiguredAttribute)) == null
                || TestConfig.Instance.IsConfigured);
            base.RunTestCases(cases, messageSink, executionOptions);
        }
    }

    public class RunIfConfiguredAttribute : Attribute
    {
    }
}

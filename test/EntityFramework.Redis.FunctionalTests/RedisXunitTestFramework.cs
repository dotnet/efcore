// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisXunitTestFramework : XunitTestFramework
    {
        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new RedisXunitFrameworkExecutor(assemblyName, SourceInformationProvider);
        }
    }

    public class RedisXunitFrameworkExecutor : XunitTestFrameworkExecutor
    {
        public RedisXunitFrameworkExecutor(
            AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider)
            : base(assemblyName, sourceInformationProvider)
        {
        }

        protected override void RunTestCases(
            IEnumerable<IXunitTestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions)
        {
            var serverStarted = false;
            try
            {
                serverStarted = RedisTestConfig.StartServer();
            }
            catch (Exception)
            {
                // test just hangs if we allow exceptions to propagate
            }

            var cases = testCases.Where(t =>
                t.Class.GetCustomAttributes(typeof(RequiresRedisServerAttribute)) == null
                || serverStarted);
            base.RunTestCases(cases, messageSink, executionOptions);

            // wait long enough for all tests to finish on other threads
            Thread.Sleep(RedisTestConfig.ServerTimeoutInSecs * 1000);

            try
            {
                RedisTestConfig.StopServer();
            }
            catch (Exception)
            {
                // test just hangs if we allow exceptions to propagate
            }
        }
    }

    public class RequiresRedisServerAttribute : Attribute
    {
    }
}

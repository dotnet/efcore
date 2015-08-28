// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers
{
    public class AdventureWorksDatabaseTestDiscoverer : BenchmarkTestCaseDiscoverer
    {
        private static Lazy<bool> _databaseExists = new Lazy<bool>(() =>
        {
            using (var connection = new SqlConnection(AdventureWorksFixtureBase.ConnectionString))
            {
                try
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        });

        private readonly IMessageSink _diagnosticMessageSink;

        public AdventureWorksDatabaseTestDiscoverer(IMessageSink diagnosticMessageSink)
            :base(diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public override IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            if (_databaseExists.Value)
            {
                return base.Discover(discoveryOptions, testMethod, factAttribute);
            }
            else
            {
                return new IXunitTestCase[] { new SkippedTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod) };
            }
        }

        private class SkippedTestCase : XunitTestCase
        {
            public SkippedTestCase(IMessageSink diagnosticMessageSink, Xunit.Sdk.TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod)
                : base(diagnosticMessageSink, defaultMethodDisplay, testMethod)
            {
                SkipReason = $"AdventureWorks2014 database does not exist on {BenchmarkConfig.Instance.BenchmarkDatabaseInstance}. Download the AdventureWorks backup from https://msftdbprodsamples.codeplex.com/downloads/get/880661 and restore it to {BenchmarkConfig.Instance.BenchmarkDatabaseInstance} to enable these tests.";
            }
        }
    }
}

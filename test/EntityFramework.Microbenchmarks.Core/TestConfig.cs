// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Framework.Configuration;
using Xunit.Abstractions;
using Xunit.Sdk;

#if DNX451 || DNXCORE50
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace EntityFramework.Microbenchmarks.Core
{
    public class PerfTestFramework : XunitTestFramework
    {
        public PerfTestFramework(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new PerfTestDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
        }
    }

    public class PerfTestDiscoverer : XunitTestFrameworkDiscoverer
    {
        private readonly TestConfig _testConfig;

        public PerfTestDiscoverer(
            IAssemblyInfo assemblyInfo,
            ISourceInformationProvider sourceProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
        {
            _testConfig = new TestConfig();
        }

        protected override bool IsValidTestClass(ITypeInfo type)
        {
            if (!_testConfig.RunPerfTests)
            {
                return false;
            }

            return base.IsValidTestClass(type);
        }
    }

    public class TestConfig
    {
        public TestConfig()
        {
            _dataSource = Environment.GetEnvironmentVariable("PERFRUN_DataSource");
            RuntimeFlavor = getRuntimeFlavor();

            var resultsDirectory = Environment.GetEnvironmentVariable("PERFRUN_ResultsDirectory");
            ResultsDirectory = string.IsNullOrEmpty(resultsDirectory) ? ResultsDirectory : resultsDirectory;

            const string cliConfigPath = "LocalConfig.json";
            const string vsConfigPath = "..\\..\\LocalConfig.json";

            if (_dataSource != null)
            {
                RunPerfTests = true;
            }
            else
            {
                var builder = new ConfigurationBuilder(GetApplicationBathPath());
                if (File.Exists(cliConfigPath))
                {
                    builder.AddJsonFile(cliConfigPath);
                }
                else if (File.Exists(vsConfigPath))
                {
                    builder.AddJsonFile(vsConfigPath);
                }

                var configuration = builder.Build();
                if (configuration.TryGet("Data:DefaultDataSource:DataSource", out _dataSource))
                {
                    _dataSource = _dataSource.Trim();
                    string runPerfTests;
                    configuration.TryGet("Data:RunPerfTests", out runPerfTests);
                    RunPerfTests = !string.IsNullOrEmpty(runPerfTests) && !runPerfTests.ToLower().Equals("false");
                }
            }
        }

        private static TestConfig _instance;
        private readonly string _dataSource = @"(localdb)\MSSQLLocalDB";

        public static TestConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TestConfig();
                }
                return _instance;
            }
        }

        public bool RunPerfTests { get; }

        public string DataSource
        {
            get { return _dataSource; }
        }

        public string ResultsDirectory { get; } = @".\PerfResults";

        public string RuntimeFlavor { get; }

        private string getRuntimeFlavor()
        {
            var runtimeFlavor = "Net45";
#if DNX451 || DNXCORE50
            var services = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnv = (IApplicationEnvironment)services.GetService(typeof(IApplicationEnvironment));
            var isCoreCLR = appEnv.RuntimeFramework.Identifier == "Asp.NetCore";
            runtimeFlavor = isCoreCLR ? "CoreCLR" : "Desktop";
#endif
            return runtimeFlavor;
        }

        private string GetApplicationBathPath()
        {
            var applicatioBasePath = ".";

#if DNX451 || DNXCORE50
            var services = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnv = (IApplicationEnvironment)services.GetService(typeof(IApplicationEnvironment));
            applicatioBasePath = appEnv.ApplicationBasePath;
#endif

            return applicatioBasePath;
        }
    }
}

using System;
using System.IO;
using Microsoft.Framework.ConfigurationModel;
using Xunit.Abstractions;
using Xunit.Sdk;

#if ASPNET50 || ASPNETCORE50
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace EntityFramework.Microbenchmarks
{
    public class PerfTestFramework : XunitTestFramework
    {
        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new PerfTestDiscoverer(assemblyInfo, SourceInformationProvider);
        }
    }


    public class PerfTestDiscoverer : XunitTestFrameworkDiscoverer
    {
        private readonly TestConfig _testConfig;

        public PerfTestDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider)
            : base(assemblyInfo, sourceProvider)
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
            _runtimeFlavor = getRuntimeFlavor();

            var resultsDirectory = Environment.GetEnvironmentVariable("PERFRUN_ResultsDirectory");
            _resultsDirectory = string.IsNullOrEmpty(resultsDirectory) ? _resultsDirectory : resultsDirectory;

            const string cliConfigPath = "LocalConfig.json";
            const string vsConfigPath = "..\\..\\LocalConfig.json";
            
            if (_dataSource != null)
            {
                _runPerfTests = true;
            }
            else
            {
                var configuration = new Configuration();
                if (File.Exists(cliConfigPath))
                {
                    configuration.AddJsonFile(cliConfigPath);
                }
                else if (File.Exists(vsConfigPath))
                {
                    configuration.AddJsonFile(vsConfigPath);
                }

                if (configuration.TryGet("Data:DefaultDataSource:DataSource", out _dataSource))
                {
                    _dataSource = _dataSource.Trim();
                    string runPerfTests;
                    configuration.TryGet("Data:RunPerfTests", out runPerfTests);
                    _runPerfTests = !string.IsNullOrEmpty(runPerfTests) && !runPerfTests.ToLower().Equals("false");
                }
            }
        }

        private static TestConfig _instance;
        private readonly string _dataSource = @".\SQLEXPRESS";
        private readonly string _resultsDirectory = @".\PerfResults";
        private readonly bool _runPerfTests = false;
        private readonly string _runtimeFlavor;
        
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

        public bool RunPerfTests
        {
            get { return _runPerfTests; }
        }

        public string DataSource
        {
            get { return _dataSource; }
        }

        public string ResultsDirectory
        {
            get { return _resultsDirectory; }
        }

        public string RuntimeFlavor
        {
            get { return _runtimeFlavor; }
        }

        private string getRuntimeFlavor()
        {
            var runtimeFlavor = "Net45";
#if ASPNET50 || ASPNETCORE50
            var services = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnv = (IApplicationEnvironment)services.GetService(typeof(IApplicationEnvironment));
            var isCoreCLR = appEnv.RuntimeFramework.Identifier == "ASP.NETCore";
            runtimeFlavor = isCoreCLR ? "CoreCLR" : "Desktop";
#endif
            return runtimeFlavor;
        }
    }
}

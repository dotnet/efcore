using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using System;

#if WINSQLITE3
using System.Runtime.InteropServices;
#endif

[assembly: TestFramework(
    "Microsoft.Data.Sqlite.Tests.TestUtilities.SqliteTestFramework",
#if E_SQLITE3
    "Microsoft.Data.Sqlite.Tests")]
#elif E_SQLCIPHER
    "Microsoft.Data.Sqlite.e_sqlcipher.Tests")]
#elif WINSQLITE3
    "Microsoft.Data.Sqlite.winsqlite3.Tests")]
#else
#error Unexpected native library
#endif

namespace Microsoft.Data.Sqlite.Tests.TestUtilities
{
    class SqliteTestFramework : XunitTestFramework
    {
        protected SqliteTestFramework(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
            => new SqliteTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }

    class SqliteTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        public SqliteTestFrameworkExecutor(
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
        }

        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            using var assemblyRunner = new SqliteTestAssemblyRunner(
                TestAssembly,
                testCases,
                DiagnosticMessageSink,
                executionMessageSink,
                executionOptions);
            await assemblyRunner.RunAsync();
        }
    }

    class SqliteTestAssemblyRunner : XunitTestAssemblyRunner
    {
        public SqliteTestAssemblyRunner(
            ITestAssembly testAssembly,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        {
        }


        protected override Task<RunSummary> RunTestCollectionAsync(
            IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
#if WINSQLITE3
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SkipAll("winsqlite3 isn't supported on " + RuntimeInformation.OSDescription);
            }

#endif
            var version = new SqliteConnection().ServerVersion;
            if (new Version(version) < new Version(3, 16, 0))
            {
                return SkipAll("SQLite " + version + " isn't supported. Upgrade to 3.16.0 or higher");
            }

            return new XunitTestCollectionRunner(
                    testCollection,
                    testCases,
                    DiagnosticMessageSink,
                    messageBus,
                    TestCaseOrderer,
                    new ExceptionAggregator(Aggregator),
                    cancellationTokenSource)
                .RunAsync();

            Task<RunSummary> SkipAll(string reason)
            {
                var count = 0;
                foreach (var testCase in testCases)
                {
                    messageBus.QueueMessage(new TestSkipped(new XunitTest(testCase, testCase.DisplayName), reason));
                    count++;
                }

                return Task.FromResult(new RunSummary { Skipped = count, Total = count });
            }
        }
    }
}

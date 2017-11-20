// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class OperationExecutorTest
    {
        [Fact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(null, null));
            Assert.Equal("reportHandler", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(new OperationReportHandler(), null));
            Assert.Equal("args", ex.ParamName);
        }

        public class OperationBaseTests
        {
            [Fact]
            public void Execute_catches_exceptions()
            {
                var handler = new OperationResultHandler();
                var operation = new MockOperation(handler);
                var error = new ArgumentOutOfRangeException("Needs to be about 20% more cool.");

                operation.Execute(() => { throw error; });

                Assert.Equal(error.GetType().FullName, handler.ErrorType);
                Assert.Equal(error.Message, handler.ErrorMessage);
                Assert.NotEmpty(handler.ErrorStackTrace);
            }

            [Fact]
            public void Execute_sets_results()
            {
                var handler = new OperationResultHandler();
                var operation = new MockOperation(handler);
                var result = "Twilight Sparkle";

                operation.Execute(() => result);

                Assert.Equal(result, handler.Result);
            }

            [Fact]
            public void Execute_enumerates_results()
            {
                var handler = new OperationResultHandler();
                var operation = new MockOperation(handler);

                operation.Execute(() => YieldResults());

                Assert.IsType<string[]>(handler.Result);
                Assert.Equal(new[] { "Twilight Sparkle", "Princess Celestia" }, handler.Result);
            }

            [Theory(Skip = "DatabaseOperations.ScaffoldContext throws exception")]
            [InlineData("FakeOutputDir", null)]
            [InlineData("FakeOutputDir", "FakeOutputDir")]
            [InlineData("FakeOutputDir", "FakeContextOutputDir")]
            [InlineData("FakeOutputDir", "../AnotherFakeProject")]
            [InlineData("FakeOutputDir", "../AnotherFakeProject/FakeContextOutputDir")]
            [InlineData("FakeOutputDir", "rooted/AnotherFakeProject")]
            [InlineData("FakeOutputDir", "rooted/AnotherFakeProject/FakeContextOutputDir")]
            public void OperationExecutor_ScaffoldContext_generates_separate_context_output_path(string outputDir, string outputDbContextDir)
            {
                IOperationReportHandler reportHandler = new OperationReportHandler();
                var resultHandler = new OperationResultHandler();
                var projectPath = Path.Combine(new TempDirectory().Path, "FakeProjectDir");
                var executorArgs = new Dictionary<string, object>
                {
                    { "targetName", "FakeTarget"},
                    { "startupTargetName", "Microsoft.EntityFrameworkCore.Design.Tests"},
                    { "projectDir", projectPath},
                    { "rootNamespace", "FakeRootNamespace"},
                    { "language", "C#"},
                };
                var executor = new OperationExecutor(reportHandler, executorArgs);
                var connectionString = new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\MSSQLLocalDB",
                    InitialCatalog = "CommandConfiguration",
                    IntegratedSecurity = true,
                }.ConnectionString;

                if (outputDbContextDir != null && outputDbContextDir.StartsWith("rooted"))
                {
                    var altDirName = outputDbContextDir.Substring(7);
                    outputDbContextDir = Path.Combine(new TempDirectory().Path, altDirName);
                }

                var scaffolderArgs = new Dictionary<string, object>
                {
                    { "connectionString", connectionString},
                    { "provider", "Microsoft.EntityFrameworkCore.SqlServer"},
                    { "outputDir", outputDir},
                    { "outputDbContextDir", outputDbContextDir},
                    { "dbContextClassName", "FakeDbContextClassName"},
                    { "schemaFilters", new[]{"FakeSchemaFilter"}},
                    { "tableFilters", new[] {"FakeTableFilter"}},
                    { "useDataAnnotations", false},
                    { "overwriteFiles", true},
                    { "useDatabaseNames", false},
                };

                new OperationExecutor.ScaffoldContext(executor, resultHandler, scaffolderArgs);

                var files = (Hashtable)resultHandler.Result;
                var fullContextPath = Path.GetDirectoryName((string)files["ContextFile"]);
                var contextPath = new DirectoryInfo(fullContextPath).Name;
                var expectedOutputPath = outputDir;
                if (outputDbContextDir != null && outputDbContextDir != outputDir)
                {
                    expectedOutputPath = new DirectoryInfo(outputDbContextDir).Name;
                }

                Assert.Equal(expectedOutputPath, contextPath);
            }

            private IEnumerable<string> YieldResults()
            {
                yield return "Twilight Sparkle";
                yield return "Princess Celestia";
            }

            private class MockOperation : OperationExecutor.OperationBase
            {
                public MockOperation(object resultHandler)
                    : base(resultHandler)
                {
                }
            }
        }
    }
}

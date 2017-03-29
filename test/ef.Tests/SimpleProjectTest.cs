// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET46

using System;
using System.Collections;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Tools.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools
{
    [Collection("OperationExecutorTests")]
    public class SimpleProjectTest : IClassFixture<SimpleProjectTest.SimpleProject>
    {
        private readonly SimpleProject _project;

        public SimpleProjectTest(SimpleProject project)
        {
            _project = project;
        }

        private void AssertDefaultMigrationName(IDictionary artifacts)
            => Assert.Contains("namespace SimpleProject.Migrations", File.ReadAllText(artifacts["MigrationFile"] as string));

        [Fact]
        public void AddMigration()
        {
            var artifacts = _project.Executor.AddMigration("EmptyMigration", "CustomFolder", "SimpleContext");
            Assert.NotNull(artifacts);
            Assert.NotNull(artifacts["MigrationFile"]);
            Assert.NotNull(artifacts["MetadataFile"]);
            Assert.NotNull(artifacts["SnapshotFile"]);
            Assert.True(Directory.Exists(Path.Combine(_project.TargetDir, "CustomFolder")));
            Assert.Contains("namespace SimpleProject.CustomFolder", File.ReadAllText(artifacts["MigrationFile"] as string));
        }

        [Fact]
        public void AddMigration_output_dir_relative_to_projectdir()
        {
            var artifacts = _project.Executor.AddMigration("EmptyMigration1", "./CustomFolder", "SimpleContext");
            Assert.NotNull(artifacts);
            Assert.StartsWith(Path.Combine(_project.TargetDir, "CustomFolder"), artifacts["MigrationFile"] as string);
            Assert.Contains("namespace SimpleProject.CustomFolder", File.ReadAllText(artifacts["MigrationFile"] as string));
        }

        [Fact]
        public void AddMigration_output_dir_relative_out_of_to_projectdir()
        {
            var artifacts = _project.Executor.AddMigration("EmptyMigration1", "../CustomFolder", "SimpleContext");
            Assert.NotNull(artifacts);
            Assert.StartsWith(Path.GetFullPath(Path.Combine(_project.TargetDir, "../CustomFolder")), artifacts["MigrationFile"] as string);
            AssertDefaultMigrationName(artifacts);
        }

        [Fact]
        public void AddMigration_output_dir_absolute_path_in_project()
        {
            var outputDir = Path.Combine(_project.TargetDir, "A/B/C");
            var artifacts = _project.Executor.AddMigration("EmptyMigration1", outputDir, "SimpleContext");
            Assert.NotNull(artifacts);
            Assert.Equal(Path.Combine(outputDir, Path.GetFileName(artifacts["MigrationFile"] as string)), artifacts["MigrationFile"]);
            Assert.Contains("namespace SimpleProject.A.B.C", File.ReadAllText(artifacts["MigrationFile"] as string));
        }

        [Fact]
        public void AddMigration_output_dir_absolute_path_outside_project()
        {
            var outputDir = Path.GetTempPath();
            var artifacts = _project.Executor.AddMigration("EmptyMigration1", outputDir, "SimpleContext");
            Assert.NotNull(artifacts);
            Assert.StartsWith(outputDir, artifacts["MigrationFile"] as string);
            AssertDefaultMigrationName(artifacts);
        }

        [Theory]
        [InlineData("")]
        [InlineData("     ")]
        [InlineData(null)]
        public void AddMigration_handles_empty_output_dir(string outputDir)
        {
            var artifacts = _project.Executor.AddMigration("EmptyMigration2", outputDir, "SimpleContext");
            Assert.NotNull(artifacts);
            Assert.StartsWith(Path.Combine(_project.TargetDir, "Migrations"), artifacts["MigrationFile"] as string);
            AssertDefaultMigrationName(artifacts);
        }

        [Fact]
        public void ScriptMigration()
        {
            var sql = _project.Executor.ScriptMigration(null, "InitialCreate", false, "SimpleContext");
            Assert.NotEmpty(sql);
        }

        [Fact]
        public void GetContextType()
        {
            var contextTypeName = _project.Executor.GetContextType("SimpleContext");
            Assert.StartsWith("SimpleProject.SimpleContext, ", contextTypeName);
        }

        [Fact]
        public void GetContextTypes()
        {
            var contextTypes = _project.Executor.GetContextTypes();
            Assert.Equal(1, contextTypes.Count());
        }

        [Fact]
        public void GetMigrations()
        {
            var migrations = _project.Executor.GetMigrations("SimpleContext");
            Assert.Equal(1, migrations.Count());
        }

        [Fact]
        public void GetContextInfo_returns_connection_string()
        {
            var info = _project.Executor.GetContextInfo("SimpleContext");
            Assert.Equal(@"(localdb)\MSSQLLocalDB", info["DataSource"]);
            Assert.Equal("SimpleProject.SimpleContext", info["DatabaseName"]);
        }

        public class SimpleProject : IDisposable
        {
            private readonly TempDirectory _directory = new TempDirectory();

            public SimpleProject()
            {
                var source = new BuildSource
                {
                    TargetDir = TargetDir,
                    References =
                    {
                        BuildReference.ByName("System.Diagnostics.DiagnosticSource", true),
                        BuildReference.ByName("System.Interactive.Async", true),
                        BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Design", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer", true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Memory", true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Options", true),
                        BuildReference.ByName("Remotion.Linq", true)
                    },
                    Sources = { @"
                            using Microsoft.EntityFrameworkCore;
                            using Microsoft.EntityFrameworkCore.Infrastructure;
                            using Microsoft.EntityFrameworkCore.Migrations;

                            namespace SimpleProject
                            {
                                internal class SimpleContext : DbContext
                                {
                                    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                                    {
                                        optionsBuilder.UseSqlServer(""Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SimpleProject.SimpleContext;Integrated Security=True"");
                                    }
                                }

                                namespace Migrations
                                {
                                    [DbContext(typeof(SimpleContext))]
                                    [Migration(""20141010222726_InitialCreate"")]
                                    public class InitialCreate : Migration
                                    {
                                        protected override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }" }
                };
                var build = source.Build();
                Executor = new AppDomainOperationExecutor(build.TargetPath,
                    build.TargetPath,
                    build.TargetDir,
                    build.TargetDir,
                    build.TargetDir,
                    "SimpleProject",
                    null);
            }

            public string TargetDir => _directory.Path;

            internal IOperationExecutor Executor { get; }

            public void Dispose()
            {
                Executor.Dispose();
                _directory.Dispose();
            }
        }
    }
}
#elif NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif
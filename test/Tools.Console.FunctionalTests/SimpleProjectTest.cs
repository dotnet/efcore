// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests
{
    [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Code compilation fails")]
    public class SimpleProjectTest : IClassFixture<SimpleProjectTest.SimpleProject>
    {
        private readonly SimpleProject _project;

        public SimpleProjectTest(SimpleProject project)
        {
            _project = project;
        }

        private void AssertDefaultMigrationName(IDictionary artiConditionalFacts)
        {
            Assert.Contains("namespace SimpleProject.Migrations", File.ReadAllText(artiConditionalFacts["MigrationFile"] as string));
        }

        [ConditionalFact]
        public void AddMigration()
        {
            var artiConditionalFacts = _project.Executor.AddMigration("EmptyMigration", "CustomFolder", "SimpleContext");
            Assert.NotNull(artiConditionalFacts);
            Assert.NotNull(artiConditionalFacts["MigrationFile"]);
            Assert.NotNull(artiConditionalFacts["MetadataFile"]);
            Assert.NotNull(artiConditionalFacts["SnapshotFile"]);
            Assert.True(Directory.Exists(Path.Combine(_project.TargetDir, "CustomFolder")));
            Assert.Contains("namespace SimpleProject.CustomFolder", File.ReadAllText(artiConditionalFacts["MigrationFile"] as string));
        }

        [ConditionalFact]
        public void AddMigration_output_dir_relative_to_projectdir()
        {
            var artiConditionalFacts = _project.Executor.AddMigration("EmptyMigration1", "./CustomFolder", "SimpleContext");
            Assert.NotNull(artiConditionalFacts);
            Assert.StartsWith(Path.Combine(_project.TargetDir, "CustomFolder"), artiConditionalFacts["MigrationFile"] as string);
            Assert.Contains("namespace SimpleProject.CustomFolder", File.ReadAllText(artiConditionalFacts["MigrationFile"] as string));
        }

        [ConditionalFact]
        public void AddMigration_output_dir_relative_out_of_to_projectdir()
        {
            var artiConditionalFacts = _project.Executor.AddMigration("EmptyMigration1", "../CustomFolder", "SimpleContext");
            Assert.NotNull(artiConditionalFacts);
            Assert.StartsWith(Path.GetFullPath(Path.Combine(_project.TargetDir, "../CustomFolder")), artiConditionalFacts["MigrationFile"] as string);
            AssertDefaultMigrationName(artiConditionalFacts);
        }

        [ConditionalFact]
        public void AddMigration_output_dir_absolute_path_in_project()
        {
            var outputDir = Path.Combine(_project.TargetDir, "A/B/C");
            var artiConditionalFacts = _project.Executor.AddMigration("EmptyMigration1", outputDir, "SimpleContext");
            Assert.NotNull(artiConditionalFacts);
            Assert.Equal(Path.Combine(outputDir, Path.GetFileName(artiConditionalFacts["MigrationFile"] as string)), artiConditionalFacts["MigrationFile"]);
            Assert.Contains("namespace SimpleProject.A.B.C", File.ReadAllText(artiConditionalFacts["MigrationFile"] as string));
        }

        [ConditionalFact]
        public void AddMigration_output_dir_absolute_path_outside_project()
        {
            var outputDir = Path.GetTempPath();
            var artiConditionalFacts = _project.Executor.AddMigration("EmptyMigration1", outputDir, "SimpleContext");
            Assert.NotNull(artiConditionalFacts);
            Assert.StartsWith(outputDir, artiConditionalFacts["MigrationFile"] as string);
            AssertDefaultMigrationName(artiConditionalFacts);
        }

        [ConditionalTheory]
        [InlineData("")]
        [InlineData("     ")]
        [InlineData(null)]
        public void AddMigration_handles_empty_output_dir(string outputDir)
        {
            var artiConditionalFacts = _project.Executor.AddMigration("EmptyMigration2", outputDir, "SimpleContext");
            Assert.NotNull(artiConditionalFacts);
            Assert.StartsWith(Path.Combine(_project.TargetDir, "Migrations"), artiConditionalFacts["MigrationFile"] as string);
            AssertDefaultMigrationName(artiConditionalFacts);
        }

        [ConditionalFact]
        public void ScriptMigration()
        {
            var sql = _project.Executor.ScriptMigration(null, "InitialCreate", false, "SimpleContext");
            Assert.NotEmpty(sql);
        }

        [ConditionalFact]
        public void GetContextType()
        {
            var contextTypeName = _project.Executor.GetContextType("SimpleContext");
            Assert.StartsWith("SimpleProject.SimpleContext, ", contextTypeName);
        }

        [ConditionalFact]
        public void GetContextTypes()
        {
            var contextTypes = _project.Executor.GetContextTypes();
            Assert.Equal(1, contextTypes.Count());
        }

        [ConditionalFact]
        public void GetMigrations()
        {
            var migrations = _project.Executor.GetMigrations("SimpleContext");
            Assert.Equal(1, migrations.Count());
        }

        [ConditionalFact]
        public void FindDatabase_returns_connection_string()
        {
            var connectionString = _project.Executor.GetDatabase("SimpleContext");
            Assert.Equal(@"(localdb)\MSSQLLocalDB", connectionString["DataSource"]);
            Assert.Equal("SimpleProject.SimpleContext", connectionString["DatabaseName"]);
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
#if NET451
                        BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
#endif
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
#if NET451
                Executor = new AppDomainOperationExecutor(build.TargetPath,
                    build.TargetPath,
                    build.TargetDir,
                    build.TargetDir,
                    build.TargetDir,
                    "SimpleProject",
                    null,
                    AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
#else
                Executor = new ReflectionOperationExecutor(build.TargetPath,
                    build.TargetPath,
                    build.TargetDir,
                    build.TargetDir,
                    build.TargetDir,
                    "SimpleProject",
                    null);
#endif
            }

            public string TargetDir => _directory.Path;

            public IOperationExecutor Executor { get; }

            public void Dispose()
            {
                Executor.Dispose();
                _directory.Dispose();
            }
        }
    }
}

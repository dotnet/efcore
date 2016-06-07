// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Tools.Core.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Tools.FunctionalTests.TestUtilities;
using Xunit;
using System.Collections;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests.Design
{
    [FrameworkSkipCondition(RuntimeFrameworks.Mono, SkipReason = "Mono's assembly loading mechanisms are buggy")]
    public class OperationExecutorTest
    {
        [FrameworkSkipCondition(RuntimeFrameworks.Mono, SkipReason = "Mono's assembly loading mechanisms are buggy")]
        public class SimpleProjectTest : IClassFixture<SimpleProjectTest.SimpleProject>
        {
            private readonly SimpleProject _project;

            public SimpleProjectTest(SimpleProject project)
            {
                _project = project;
            }

            [ConditionalFact]
            public void GetContextType_works_cross_domain()
            {
                var contextTypeName = _project.Executor.GetContextType("SimpleContext");
                Assert.StartsWith("SimpleProject.SimpleContext, ", contextTypeName);
            }

            private void AssertDefaultMigrationName(IDictionary artifacts)
            {
                Assert.Contains("namespace SimpleProject.Migrations", File.ReadAllText(artifacts["MigrationFile"] as string));
            }

            [ConditionalFact]
            public void AddMigration_works_cross_domain()
            {
                var artifacts = _project.Executor.AddMigration("EmptyMigration", "CustomFolder", "SimpleContext");
                Assert.NotNull(artifacts);
                Assert.NotNull(artifacts["MigrationFile"]);
                Assert.NotNull(artifacts["MetadataFile"]);
                Assert.NotNull(artifacts["SnapshotFile"]);
                Assert.True(Directory.Exists(Path.Combine(_project.TargetDir, "CustomFolder")));
                Assert.Contains("namespace SimpleProject.CustomFolder", File.ReadAllText(artifacts["MigrationFile"] as string));
            }

            [ConditionalFact]
            public void AddMigration_output_dir_relative_to_projectdir()
            {
                var artifacts = _project.Executor.AddMigration("EmptyMigration1", "./CustomFolder", "SimpleContext");
                Assert.NotNull(artifacts);
                Assert.StartsWith(Path.Combine(_project.TargetDir, "CustomFolder"), artifacts["MigrationFile"] as string);
                Assert.Contains("namespace SimpleProject.CustomFolder", File.ReadAllText(artifacts["MigrationFile"] as string));
            }

            [ConditionalFact]
            public void AddMigration_output_dir_relative_out_of_to_projectdir()
            {
                var artifacts = _project.Executor.AddMigration("EmptyMigration1", "../CustomFolder", "SimpleContext");
                Assert.NotNull(artifacts);
                Assert.StartsWith(Path.GetFullPath(Path.Combine(_project.TargetDir, "../CustomFolder")), artifacts["MigrationFile"] as string);
                AssertDefaultMigrationName(artifacts);
            }


            [ConditionalFact]
            public void AddMigration_output_dir_absolute_path_in_project()
            {
                var outputDir = Path.Combine(_project.TargetDir, "A/B/C");
                var artifacts = _project.Executor.AddMigration("EmptyMigration1", outputDir, "SimpleContext");
                Assert.NotNull(artifacts);
                Assert.Equal(Path.Combine(outputDir, Path.GetFileName(artifacts["MigrationFile"] as string)), artifacts["MigrationFile"]);
                Assert.Contains("namespace SimpleProject.A.B.C", File.ReadAllText(artifacts["MigrationFile"] as string));
            }

            [ConditionalFact]
            public void AddMigration_output_dir_absolute_path_outside_project()
            {
                var outputDir = Path.GetTempPath();
                var artifacts = _project.Executor.AddMigration("EmptyMigration1", outputDir, "SimpleContext");
                Assert.NotNull(artifacts);
                Assert.StartsWith(outputDir, artifacts["MigrationFile"] as string);
                AssertDefaultMigrationName(artifacts);
            }

            [ConditionalTheory]
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

            [ConditionalFact]
            public void ScriptMigration_works_cross_domain()
            {
                var sql = _project.Executor.ScriptMigration(null, "InitialCreate", false, "SimpleContext");
                Assert.NotEmpty(sql);
            }

            [ConditionalFact]
            public void GetContextTypes_works_cross_domain()
            {
                var contextTypes = _project.Executor.GetContextTypes();
                Assert.Equal(1, contextTypes.Count());
            }

            [ConditionalFact]
            public void GetMigrations_works_cross_domain()
            {
                var migrations = _project.Executor.GetMigrations("SimpleContext");
                Assert.Equal(1, migrations.Count());
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
                            BuildReference.ByName("System.Diagnostics.DiagnosticSource", copyLocal: true),
                            BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                            BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                            BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", copyLocal: true),
                            BuildReference.ByName("Microsoft.EntityFrameworkCore", copyLocal: true),
                            BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools", copyLocal: true),
                            BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools.Core", copyLocal: true),
                            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", copyLocal: true),
                            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", copyLocal: true),
                            BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.Caching.Memory", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.DependencyInjection", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.Logging", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true),
                            BuildReference.ByName("Microsoft.Extensions.Options", copyLocal: true),
                            BuildReference.ByName("Remotion.Linq", copyLocal: true)
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
                    Executor = new AppDomainOperationExecutor(TargetDir, build.TargetName, TargetDir, TargetDir, "SimpleProject");
                }

                public string TargetDir => _directory.Path;

                public AppDomainOperationExecutor Executor { get; }

                public void Dispose()
                {
                    Executor.Dispose();
                    _directory.Dispose();
                }
            }
        }

        [ConditionalFact]
        public void GetMigrations_filters_by_context_name()
        {
            using (var directory = new TempDirectory())
            {
                var targetDir = directory.Path;
                var source = new BuildSource
                {
                    TargetDir = targetDir,
                    References =
                    {
                        BuildReference.ByName("System.Diagnostics.DiagnosticSource", copyLocal: true),
                        BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                        BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools.Core", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Memory", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Options", copyLocal: true),
                        BuildReference.ByName("Remotion.Linq", copyLocal: true)
                    },
                    Sources = { @"
                        using Microsoft.EntityFrameworkCore;
                        using Microsoft.EntityFrameworkCore.Infrastructure;
                        using Microsoft.EntityFrameworkCore.Migrations;

                        namespace MyProject
                        {
                            internal class Context1 : DbContext
                            {
                                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                                {
                                    optionsBuilder.UseSqlServer(""Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SimpleProject.SimpleContext;Integrated Security=True"");
                                }
                            }

                            internal class Context2 : DbContext
                            {
                            }

                            namespace Migrations
                            {
                                namespace Context1Migrations
                                {
                                    [DbContext(typeof(Context1))]
                                    [Migration(""000000000000000_Context1Migration"")]
                                    public class Context1Migration : Migration
                                    {
                                        protected override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }

                                namespace Context2Migrations
                                {
                                    [DbContext(typeof(Context2))]
                                    [Migration(""000000000000000_Context2Migration"")]
                                    public class Context2Migration : Migration
                                    {
                                        protected override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }
                        }" }
                };
                var build = source.Build();
                using (var executor = new AppDomainOperationExecutor(targetDir, build.TargetName, targetDir, targetDir, "MyProject"))
                {
                    var migrations = executor.GetMigrations("Context1");

                    Assert.Equal(1, migrations.Count());
                }
            }
        }

        [ConditionalFact]
        public void GetContextType_works_with_multiple_assemblies()
        {
            using (var directory = new TempDirectory())
            {
                var targetDir = directory.Path;
                var contextsSource = new BuildSource
                {
                    TargetDir = targetDir,
                    References =
                    {
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools.Core", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true)
                    },
                    Sources = { @"
                        using Microsoft.EntityFrameworkCore;

                        namespace MyProject
                        {
                            public class Context1 : DbContext
                            {
                            }

                            public class Context2 : DbContext
                            {
                            }
                        }" }
                };
                var contextsBuild = contextsSource.Build();
                var migrationsSource = new BuildSource
                {
                    TargetDir = targetDir,
                    References =
                    {
                        BuildReference.ByName("System.Reflection.Metadata", copyLocal: true),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Options", copyLocal: true),
                        BuildReference.ByPath(contextsBuild.TargetPath)
                    },
                    Sources = { @"
                        using Microsoft.EntityFrameworkCore;
                        using Microsoft.EntityFrameworkCore.Infrastructure;
                        using Microsoft.EntityFrameworkCore.Migrations;

                        namespace MyProject
                        {
                            internal class Context3 : DbContext
                            {
                            }

                            namespace Migrations
                            {
                                namespace Context1Migrations
                                {
                                    [DbContext(typeof(Context1))]
                                    [Migration(""000000000000000_Context1Migration"")]
                                    public class Context1Migration : Migration
                                    {
                                        protected override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }

                                namespace Context2Migrations
                                {
                                    [DbContext(typeof(Context2))]
                                    [Migration(""000000000000000_Context2Migration"")]
                                    public class Context2Migration : Migration
                                    {
                                        protected override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }
                        }" }
                };
                var migrationsBuild = migrationsSource.Build();
                using (var executor = new AppDomainOperationExecutor(targetDir, migrationsBuild.TargetName, targetDir, targetDir, "MyProject"))
                {
                    var contextTypes = executor.GetContextTypes();

                    Assert.Equal(3, contextTypes.Count());
                }
            }
        }

        [ConditionalFact]
        public void AddMigration_begins_new_namespace_when_foreign_migrations()
        {
            using (var directory = new TempDirectory())
            {
                var targetDir = directory.Path;
                var source = new BuildSource
                {
                    TargetDir = targetDir,
                    References =
                    {
                        BuildReference.ByName("System.Diagnostics.DiagnosticSource", copyLocal: true),
                        BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                        BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools.Core", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Memory", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Options", copyLocal: true),
                        BuildReference.ByName("Remotion.Linq", copyLocal: true)
                    },
                    Sources = { @"
                            using Microsoft.EntityFrameworkCore;
                            using Microsoft.EntityFrameworkCore.Infrastructure;
                            using Microsoft.EntityFrameworkCore.Migrations;

                            namespace MyProject
                            {
                                internal class MyFirstContext : DbContext
                                {
                                    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                                    {
                                        optionsBuilder.UseSqlServer(""Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MyProject.MyFirstContext"");
                                    }
                                }

                                internal class MySecondContext : DbContext
                                {
                                    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                                    {
                                        optionsBuilder.UseSqlServer(""Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MyProject.MySecondContext"");
                                    }
                                }

                                namespace Migrations
                                {
                                    [DbContext(typeof(MyFirstContext))]
                                    [Migration(""20151006140723_InitialCreate"")]
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
                using (var executor = new AppDomainOperationExecutor(targetDir, build.TargetName, targetDir, targetDir, "MyProject"))
                {
                    var artifacts = executor.AddMigration("MyMigration", /*outputDir:*/ null, "MySecondContext");
                    Assert.Equal(3, artifacts.Keys.Count);
                    Assert.True(Directory.Exists(Path.Combine(targetDir, @"Migrations\MySecond")));
                }
            }
        }

        [ConditionalFact]
        public void Throws_for_no_parameterless_constructor()
        {
            using (var directory = new TempDirectory())
            {
                var targetDir = directory.Path;
                var source = new BuildSource
                {
                    TargetDir = targetDir,
                    References =
                    {
                        BuildReference.ByName("System.Diagnostics.DiagnosticSource", copyLocal: true),
                        BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                        BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools.Core", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Memory", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Options", copyLocal: true),
                        BuildReference.ByName("Remotion.Linq", copyLocal: true)
                    },
                    Sources = { @"
                            using Microsoft.EntityFrameworkCore;
                            using Microsoft.EntityFrameworkCore.Infrastructure;
                            using Microsoft.EntityFrameworkCore.Migrations;

                            namespace MyProject
                            {
                                internal class MyContext : DbContext
                                {
                                    public MyContext(DbContextOptions<MyContext> options) :base(options)  {}
                                }
                            }" }
                };
                var build = source.Build();
                using (var executor = new AppDomainOperationExecutor(targetDir, build.TargetName, targetDir, targetDir, "MyProject"))
                {
                    var ex = Assert.Throws<OperationException>(
                        () => executor.GetMigrations("MyContext"));

                    Assert.Equal(
                        ToolsCoreStrings.NoParameterlessConstructor("MyContext"),
                        ex.Message);
                }
            }
        }

        [ConditionalFact]
        public void GetMigrations_throws_when_target_and_migrations_assemblies_mismatch()
        {
            using (var directory = new TempDirectory())
            {
                var targetDir = directory.Path;
                var source = new BuildSource
                {
                    TargetDir = targetDir,
                    References =
                    {
                        BuildReference.ByName("System.Diagnostics.DiagnosticSource", copyLocal: true),
                        BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                        BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Tools.Core", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", copyLocal: true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Memory", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", copyLocal: true),
                        BuildReference.ByName("Microsoft.Extensions.Options", copyLocal: true),
                        BuildReference.ByName("Remotion.Linq", copyLocal: true)
                    },
                    Sources = { @"
                            using Microsoft.EntityFrameworkCore;
                            using Microsoft.EntityFrameworkCore.Infrastructure;
                            using Microsoft.EntityFrameworkCore.Migrations;

                            namespace MyProject
                            {
                                internal class MyContext : DbContext
                                {
                                    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                                    {
                                        optionsBuilder
                                            .UseSqlServer(
                                                ""Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MyProject.MyContext;Integrated Security=True"",
                                                b => b.MigrationsAssembly(""UnknownAssembly""));
                                    }
                                }

                                namespace Migrations
                                {
                                    [DbContext(typeof(MyContext))]
                                    [Migration(""20151215152142_MyMigration"")]
                                    public class MyMigration : Migration
                                    {
                                        protected override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }" }
                };
                var build = source.Build();
                using (var executor = new AppDomainOperationExecutor(targetDir, build.TargetName, targetDir, targetDir, "MyProject"))
                {
                    var ex = Assert.Throws<OperationException>(
                        () => executor.GetMigrations("MyContext"));

                    Assert.Equal(
                        ToolsCoreStrings.MigrationsAssemblyMismatch(build.TargetName, "UnknownAssembly"),
                        ex.Message);
                }
            }
        }

        [ConditionalFact]
        public void Assembly_load_errors_are_wrapped()
        {
            var targetDir = AppDomain.CurrentDomain.BaseDirectory;
            using (var executor = new AppDomainOperationExecutor(targetDir, "Unknown", targetDir, targetDir, "Unknown"))
            {
                Assert.Throws<OperationException>(() => executor.GetContextTypes());
            }
        }
    }
}

#endif

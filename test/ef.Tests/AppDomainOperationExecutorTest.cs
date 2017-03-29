// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET46

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Tools.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools
{
    [Collection("OperationExecutorTests")]
    public class AppDomainOperationExecutorTest
    {
        private IOperationExecutor CreateExecutorFromBuildResult(BuildFileResult build, string rootNamespace = null)
            => new AppDomainOperationExecutor(build.TargetPath,
                build.TargetPath,
                build.TargetDir,
                build.TargetDir,
                build.TargetDir,
                rootNamespace,
                environment: null);

        [Fact]
        public void Assembly_load_errors_are_wrapped()
        {
            var targetDir = AppDomain.CurrentDomain.BaseDirectory;
            using (var executor = new AppDomainOperationExecutor(Assembly.GetExecutingAssembly().Location, Path.Combine(targetDir, "Unknown.dll"), targetDir, null, null, null, null))
            {
                Assert.Throws<WrappedException>(() => executor.GetContextTypes());
            }
        }

        [Fact]
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
                using (var executor = CreateExecutorFromBuildResult(build, "MyProject"))
                {
                    var migrations = executor.GetMigrations("Context1");

                    Assert.Equal(1, migrations.Count());
                }
            }
        }

        [Fact]
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
                        BuildReference.ByName("Microsoft.EntityFrameworkCore", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Design", true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", true)
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
                        BuildReference.ByName("System.Reflection.Metadata", true),
                        BuildReference.ByName("Microsoft.AspNetCore.Hosting.Abstractions", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational", true),
                        BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design", true),
                        BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Configuration.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection", true),
                        BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.FileProviders.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Logging", true),
                        BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions", true),
                        BuildReference.ByName("Microsoft.Extensions.Options", true),
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
                var build = migrationsSource.Build();
                using (var executor = CreateExecutorFromBuildResult(build, "MyProject"))
                {
                    var contextTypes = executor.GetContextTypes();

                    Assert.Equal(3, contextTypes.Count());
                }
            }
        }

        [Fact]
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
                using (var executor = CreateExecutorFromBuildResult(build, "MyProject"))
                {
                    var artifacts = executor.AddMigration("MyMigration", /*outputDir:*/ null, "MySecondContext");
                    Assert.Equal(3, artifacts.Keys.Count);
                    Assert.True(Directory.Exists(Path.Combine(targetDir, @"Migrations\MySecond")));
                }
            }
        }

        [Fact]
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

                            namespace MyProject
                            {
                                internal class MyContext : DbContext
                                {
                                    public MyContext(DbContextOptions<MyContext> options) :base(options)  {}
                                }
                            }" }
                };
                var build = source.Build();
                using (var executor = CreateExecutorFromBuildResult(build, "MyProject"))
                {
                    var ex = Assert.Throws<WrappedException>(
                        () => executor.GetMigrations("MyContext"));

                    Assert.Equal(
                        DesignStrings.NoParameterlessConstructor("MyContext"),
                        ex.Message);
                }
            }
        }
    }
}
#elif NETSTANDARD1_3 || NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif

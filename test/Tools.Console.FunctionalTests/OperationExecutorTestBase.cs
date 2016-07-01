// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests
{
    public abstract class OperationExecutorTestBase
    {
        protected abstract IOperationExecutor CreateExecutorFromBuildResult(BuildFileResult build, string rootNamespace = null);

        [ConditionalFact]
        public virtual void GetMigrations_filters_by_context_name()
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

        [ConditionalFact]
        public virtual void GetContextType_works_with_multiple_assemblies()
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

        [ConditionalFact]
        public virtual void AddMigration_begins_new_namespace_when_foreign_migrations()
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
                    var artiConditionalFacts = executor.AddMigration("MyMigration", /*outputDir:*/ null, "MySecondContext");
                    Assert.Equal(3, artiConditionalFacts.Keys.Count);
                    Assert.True(Directory.Exists(Path.Combine(targetDir, @"Migrations\MySecond")));
                }
            }
        }

        [ConditionalFact]
        public virtual void Throws_for_no_parameterless_constructor()
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
                    var ex = Assert.Throws<OperationErrorException>(
                        () => executor.GetMigrations("MyContext"));

                    Assert.Equal(
                        DesignStrings.NoParameterlessConstructor("MyContext"),
                        ex.Message);
                }
            }
        }

        [ConditionalFact]
        public virtual void GetMigrations_throws_when_target_and_migrations_assemblies_mismatch()
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
                using (var executor = CreateExecutorFromBuildResult(build, "MyProject"))
                {
                    var ex = Assert.Throws<OperationErrorException>(
                        () => executor.GetMigrations("MyContext"));

                    Assert.Equal(
                        DesignStrings.MigrationsAssemblyMismatch(build.TargetName, "UnknownAssembly"),
                        ex.Message);
                }
            }
        }
    }
}

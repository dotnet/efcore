// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET46

using System;
using System.Linq;
using Microsoft.Data.Entity.Commands.TestUtilities;
using Xunit;

namespace Microsoft.Data.Entity.Commands
{
    public class ExecutorTest
    {
        public class SimpleProjectTest : IClassFixture<SimpleProjectTest.SimpleProject>
        {
            private readonly SimpleProject _project;

            public SimpleProjectTest(SimpleProject project)
            {
                _project = project;
            }

            [Fact]
            public void GetContextType_works_cross_domain()
            {
                var contextTypeName = _project.Executor.GetContextType("SimpleContext");
                Assert.StartsWith("SimpleProject.SimpleContext, ", contextTypeName);
            }

            [Fact]
            public void GetContextTypes_works_cross_domain()
            {
                var contextTypes = _project.Executor.GetContextTypes();
                Assert.Equal(1, contextTypes.Count());
            }

            [Fact]
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
                                    BuildReference.ByName("System.Collections.Immutable", copyLocal: true),
                                    BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                                    BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                                    BuildReference.ByName("EntityFramework.Core", copyLocal: true),
                                    BuildReference.ByName("EntityFramework.Commands", copyLocal: true),
                                    BuildReference.ByName("EntityFramework.Relational", copyLocal: true),
                                    BuildReference.ByName("EntityFramework.Relational.Design", copyLocal: true),
                                    BuildReference.ByName("EntityFramework.SqlServer", copyLocal: true),
                                    BuildReference.ByName("Microsoft.CodeAnalysis", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.Caching.Abstractions", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.Caching.Memory", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.DependencyInjection", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.DependencyInjection.Abstractions", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.Logging", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.Logging.Abstractions", copyLocal: true),
                                    BuildReference.ByName("Microsoft.Framework.OptionsModel", copyLocal: true),
                                    BuildReference.ByName("Remotion.Linq", copyLocal: true)
                                },
                        Sources = { @"
                            using System;
                            using Microsoft.Data.Entity;
                            using Microsoft.Data.Entity.Metadata;
                            using Microsoft.Data.Entity.Migrations;
                            using Microsoft.Data.Entity.Migrations.Builders;
                            using Microsoft.Data.Entity.Migrations.Infrastructure;

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
                                    [ContextType(typeof(SimpleContext))]
                                    public class InitialCreate : Migration
                                    {
                                        public override string Id => ""201410102227260_InitialCreate"";

                                        public override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }

                                        public override void Down(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }" }
                    };
                    var build = source.Build();
                    Executor = new ExecutorWrapper(TargetDir, build.TargetName + ".dll", TargetDir, "SimpleProject");
                }

                public string TargetDir
                {
                    get { return _directory.Path; }
                }

                public ExecutorWrapper Executor { get; }

                public void Dispose()
                {
                    Executor.Dispose();
                    _directory.Dispose();
                }
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
                                BuildReference.ByName("System.Collections.Immutable", copyLocal: true),
                                BuildReference.ByName("System.Interactive.Async", copyLocal: true),
                                BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                                BuildReference.ByName("EntityFramework.Core", copyLocal: true),
                                BuildReference.ByName("EntityFramework.Commands", copyLocal: true),
                                BuildReference.ByName("EntityFramework.Relational", copyLocal: true),
                                BuildReference.ByName("EntityFramework.Relational.Design", copyLocal: true),
                                BuildReference.ByName("EntityFramework.SqlServer", copyLocal: true),
                                BuildReference.ByName("Microsoft.CodeAnalysis", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.Caching.Abstractions", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.Caching.Memory", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.DependencyInjection", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.DependencyInjection.Abstractions", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.Logging", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.Logging.Abstractions", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.OptionsModel", copyLocal: true),
                                BuildReference.ByName("Remotion.Linq", copyLocal: true)
                            },
                    Sources = { @"
                        using System;
                        using Microsoft.Data.Entity;
                        using Microsoft.Data.Entity.Metadata;
                        using Microsoft.Data.Entity.Migrations;
                        using Microsoft.Data.Entity.Migrations.Builders;
                        using Microsoft.Data.Entity.Migrations.Infrastructure;

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
                                    [ContextType(typeof(Context1))]
                                    public class Context1Migration : Migration
                                    {
                                        public override string Id => ""000000000000000_Context1Migration"";

                                        public override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }

                                        public override void Down(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }

                                namespace Context2Migrations
                                {
                                    [ContextType(typeof(Context2))]
                                    public class Context2Migration : Migration
                                    {
                                        public override string Id => ""000000000000000_Context2Migration"";

                                        public override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }

                                        public override void Down(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }
                        }" }
                };
                var build = source.Build();
                using (var executor = new ExecutorWrapper(targetDir, build.TargetName + ".dll", targetDir, "MyProject"))
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
                                BuildReference.ByName("EntityFramework.Core", copyLocal: true),
                                BuildReference.ByName("EntityFramework.Commands", copyLocal: true)
                            },
                    Sources = { @"
                        using Microsoft.Data.Entity;

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
                                BuildReference.ByName("System.Collections.Immutable", copyLocal: true),
                                BuildReference.ByName("System.Reflection.Metadata", copyLocal: true),
                                BuildReference.ByName("EntityFramework.Core"),
                                BuildReference.ByName("EntityFramework.Relational", copyLocal: true),
                                BuildReference.ByName("EntityFramework.Relational.Design", copyLocal: true),
                                BuildReference.ByName("Microsoft.CodeAnalysis", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.Logging", copyLocal: true),
                                BuildReference.ByName("Microsoft.Framework.Logging.Abstractions", copyLocal: true),
                                BuildReference.ByPath(contextsBuild.TargetPath)
                            },
                    Sources = { @"
                        using System;
                        using Microsoft.Data.Entity;
                        using Microsoft.Data.Entity.Metadata;
                        using Microsoft.Data.Entity.Migrations;
                        using Microsoft.Data.Entity.Migrations.Builders;
                        using Microsoft.Data.Entity.Migrations.Infrastructure;

                        namespace MyProject
                        {
                            internal class Context3 : DbContext
                            {
                            }

                            namespace Migrations
                            {
                                namespace Context1Migrations
                                {
                                    [ContextType(typeof(Context1))]
                                    public class Context1Migration : Migration
                                    {
                                        public override string Id => ""000000000000000_Context1Migration"";

                                        public override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }

                                        public override void Down(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }

                                namespace Context2Migrations
                                {
                                    [ContextType(typeof(Context2))]
                                    public class Context2Migration : Migration
                                    {
                                        public override string Id => ""000000000000000_Context2Migration"";

                                        public override void Up(MigrationBuilder migrationBuilder)
                                        {
                                        }

                                        public override void Down(MigrationBuilder migrationBuilder)
                                        {
                                        }
                                    }
                                }
                            }
                        }" }
                };
                var migrationsBuild = migrationsSource.Build();
                using (var executor = new ExecutorWrapper(targetDir, migrationsBuild.TargetName + ".dll", targetDir, "MyProject"))
                {
                    var contextTypes = executor.GetContextTypes();

                    Assert.Equal(3, contextTypes.Count());
                }
            }
        }
    }
}

#endif

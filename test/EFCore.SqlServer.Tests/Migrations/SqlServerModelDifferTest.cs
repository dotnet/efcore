// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqlServerModelDifferTest : MigrationsModelDifferTestBase
    {
        [Fact]
        public void Alter_table_to_MemoryOptimized()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").ForSqlServerIsClustered(false);
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").ForSqlServerIsClustered(false);
                            x.ForSqlServerIsMemoryOptimized();
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(operations[0]);
                        Assert.True(IsMemoryOptimized(alterDatabaseOperation));
                        Assert.Null(IsMemoryOptimized(alterDatabaseOperation.OldDatabase));

                        var alterTableOperation = Assert.IsType<AlterTableOperation>(operations[1]);
                        Assert.Equal("Person", alterTableOperation.Name);
                        Assert.True(IsMemoryOptimized(alterTableOperation));
                        Assert.Null(IsMemoryOptimized(alterTableOperation.OldTable));
                    });
        }

        [Fact]
        public void Alter_table_from_MemoryOptimized()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").ForSqlServerIsClustered(false);
                            x.ForSqlServerIsMemoryOptimized();
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").ForSqlServerIsClustered(false);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(operations[0]);
                        Assert.Null(IsMemoryOptimized(alterDatabaseOperation));
                        Assert.True(IsMemoryOptimized(alterDatabaseOperation.OldDatabase));

                        var alterTableOperation = Assert.IsType<AlterTableOperation>(operations[1]);
                        Assert.Equal("Person", alterTableOperation.Name);
                        Assert.Null(IsMemoryOptimized(alterTableOperation));
                        Assert.True(IsMemoryOptimized(alterTableOperation.OldTable));
                    });
        }

        [Fact]
        public void Add_column_with_dependencies()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_People");
                            x.ToTable("People", "dbo");
                        }),
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_People");
                            x.ToTable("People", "dbo");
                            x.Property<string>("FirstName");
                            x.Property<string>("FullName").HasComputedColumnSql("[FirstName] + [LastName]");
                            x.Property<string>("LastName");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var columnOperation = Assert.IsType<AddColumnOperation>(operations[2]);
                        Assert.Equal("[FirstName] + [LastName]", columnOperation.ComputedColumnSql);
                    });
        }

        [Fact]
        public void Alter_column_identity()
        {
            Execute(
                source => source.Entity("Lamb").ToTable("Lamb", "bah").Property<int>("Id").ValueGeneratedNever(),
                target => target.Entity("Lamb").ToTable("Lamb", "bah").Property<int>("Id").ValueGeneratedOnAdd(),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Lamb", operation.Table);
                        Assert.Equal("Id", operation.Name);
                        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, operation["SqlServer:ValueGenerationStrategy"]);
                    });
        }

        [Fact]
        public void Alter_column_non_key_identity()
        {
            Execute(
                source => source.Entity(
                    "Lamb",
                    x =>
                        {
                            x.ToTable("Lamb", "bah");
                            x.Property<int>("Num").ValueGeneratedNever();
                            x.Property<int>("Id");
                        }),
                target => target.Entity(
                    "Lamb",
                    x =>
                        {
                            x.ToTable("Lamb", "bah");
                            x.Property<int>("Num").ValueGeneratedOnAdd();
                            x.Property<int>("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Lamb", operation.Table);
                        Assert.Equal("Num", operation.Name);
                        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, operation["SqlServer:ValueGenerationStrategy"]);
                    });
        }

        [Fact]
        public void Alter_column_computation()
        {
            Execute(
                source => source.Entity(
                    "Sheep",
                    x =>
                        {
                            x.ToTable("Sheep", "bah");
                            x.Property<int>("Id");
                            x.Property<int>("Now");
                        }),
                target => target.Entity(
                    "Sheep",
                    x =>
                        {
                            x.ToTable("Sheep", "bah");
                            x.Property<int>("Id");
                            x.Property<int>("Now").HasComputedColumnSql("CAST(CURRENT_TIMESTAMP AS int)");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Sheep", operation.Table);
                        Assert.Equal("Now", operation.Name);
                        Assert.Equal("CAST(CURRENT_TIMESTAMP AS int)", operation.ComputedColumnSql);
                    });
        }

        [Fact]
        public void Alter_primary_key_clustering()
        {
            Execute(
                source => source.Entity(
                    "Ram",
                    x =>
                        {
                            x.ToTable("Ram", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id").ForSqlServerIsClustered(false);
                        }),
                target => target.Entity(
                    "Ram",
                    x =>
                        {
                            x.ToTable("Ram", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id").ForSqlServerIsClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Ram", dropOperation.Table);
                        Assert.Equal("PK_Ram", dropOperation.Name);

                        var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                        Assert.Equal("bah", addOperation.Schema);
                        Assert.Equal("Ram", addOperation.Table);
                        Assert.Equal("PK_Ram", addOperation.Name);
                        Assert.True((bool)addOperation[SqlServerAnnotationNames.Clustered]);
                    });
        }

        [Fact]
        public void Alter_unique_constraint_clustering()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").ForSqlServerIsClustered(false);
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").ForSqlServerIsClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Ewe", dropOperation.Table);
                        Assert.Equal("AK_Ewe_AlternateId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                        Assert.Equal("bah", addOperation.Schema);
                        Assert.Equal("Ewe", addOperation.Table);
                        Assert.Equal("AK_Ewe_AlternateId", addOperation.Name);
                        Assert.True((bool)addOperation[SqlServerAnnotationNames.Clustered]);
                    });
        }

        [Fact]
        public void Create_shared_table_with_two_entity_types()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity("Order", eb =>
                            {
                                eb.Property<int>("Id");
                                eb.ToTable("Orders");
                            });
                        modelBuilder.Entity("Details", eb =>
                            {
                                eb.Property<int>("Id");
                                eb.Property<DateTime>("Time");
                                eb.HasOne("Order").WithOne().HasForeignKey("Details", "Id");
                                eb.ToTable("Orders");
                            });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal(2, createTableOperation.Columns.Count);
                        var idColumn = createTableOperation.Columns[0];
                        Assert.Equal("Id", idColumn.Name);
                        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, idColumn["SqlServer:ValueGenerationStrategy"]);
                        var timeColumn = createTableOperation.Columns[1];
                        Assert.Equal("Time", timeColumn.Name);
                        Assert.False(timeColumn.IsNullable);
                    });
        }

        [Fact]
        public void Alter_index_clustering()
        {
            Execute(
                source => source.Entity(
                    "Mutton",
                    x =>
                        {
                            x.ToTable("Mutton", "bah");
                            x.Property<int>("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForSqlServerIsClustered(false);
                        }),
                target => target.Entity(
                    "Mutton",
                    x =>
                        {
                            x.ToTable("Mutton", "bah");
                            x.Property<int>("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForSqlServerIsClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Mutton", dropOperation.Table);
                        Assert.Equal("IX_Mutton_Value", dropOperation.Name);

                        var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal("bah", createOperation.Schema);
                        Assert.Equal("Mutton", createOperation.Table);
                        Assert.Equal("IX_Mutton_Value", createOperation.Name);
                        Assert.True((bool)createOperation[SqlServerAnnotationNames.Clustered]);
                    });
        }

        public static int Function()
        {
            return default(int);
        }

        [Fact]
        public void Add_dbfunction_ignore()
        {
            var mi = typeof(SqlServerModelDifferTest).GetRuntimeMethod(nameof(Function), new Type[] { });

            Execute(
                _ => { },
                modelBuilder => modelBuilder.HasDbFunction(mi),
                operations => { Assert.Equal(0, operations.Count); });
        }

        [Fact]
        public void Alter_column_rowversion()
        {
            Execute(
                source => source.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<byte[]>("Version");
                        }),
                target => target.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<byte[]>("Version")
                                .ValueGeneratedOnAddOrUpdate()
                                .IsConcurrencyToken();
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("Toad", operation.Table);
                        Assert.Equal("Version", operation.Name);
                        Assert.True(operation.IsRowVersion);
                        Assert.True(operation.IsDestructiveChange);
                    });
        }

        protected override ModelBuilder CreateModelBuilder() => SqlServerTestHelpers.Instance.CreateConventionBuilder();

        protected override MigrationsModelDiffer CreateModelDiffer(DbContext ctx)
            => new MigrationsModelDiffer(
                new SqlServerTypeMapper(new RelationalTypeMapperDependencies()),
                new SqlServerMigrationsAnnotationProvider(new MigrationsAnnotationProviderDependencies()),
                ctx.GetService<IChangeDetector>(),
                ctx.GetService<StateManagerDependencies>(),
                ctx.GetService<CommandBatchPreparerDependencies>());

        private bool? IsMemoryOptimized(Annotatable annotatable)
            => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool?;
    }
}

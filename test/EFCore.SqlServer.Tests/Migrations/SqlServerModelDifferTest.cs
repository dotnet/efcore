// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
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
                        x.HasKey("Id").ForSqlServerIsClustered();
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
        public void Add_non_clustered_primary_key_with_owned()
        {
            Execute(
                _ => { },
                target => target.Entity(
                    "Ram",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").ForSqlServerIsClustered(false);
                        x.OwnsOne("Address", "Address");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                    var addKey = createTableOperation.PrimaryKey;
                    Assert.Equal("PK_Ram", addKey.Name);
                    Assert.False((bool)addKey[SqlServerAnnotationNames.Clustered]);
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
                        x.HasAlternateKey("AlternateId").ForSqlServerIsClustered();
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
                    modelBuilder.Entity(
                        "Order", eb =>
                        {
                            eb.Property<int>("Id");
                            eb.ToTable("Orders");
                        });
                    modelBuilder.Entity(
                        "Details", eb =>
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
        public void Add_SequenceHiLo_with_seed_data()
        {
            Execute(
                common => common.Entity(
                    "Firefly",
                    x =>
                    {
                        x.ToTable("Firefly", "dbo");
                        x.Property<int>("Id");
                        x.Property<int>("SequenceId");
                        x.HasData(
                            new
                            {
                                Id = 42
                            });
                    }),
                _ => { },
                target => target.Entity(
                    "Firefly",
                    x =>
                    {
                        x.ToTable("Firefly", "dbo");
                        x.Property<int>("SequenceId").ForSqlServerUseSequenceHiLo(schema: "dbo");
                        x.HasData(
                            new
                            {
                                Id = 43
                            });
                    }),
                upOps => Assert.Collection(
                    upOps,
                    o =>
                    {
                        var operation = Assert.IsType<CreateSequenceOperation>(o);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("EntityFrameworkHiLoSequence", operation.Name);
                    },
                    o =>
                    {
                        var m = Assert.IsType<InsertDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.Values,
                            v => Assert.Equal(43, v));
                    }),
                downOps => Assert.Collection(
                    downOps,
                    o =>
                    {
                        var operation = Assert.IsType<DropSequenceOperation>(o);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("EntityFrameworkHiLoSequence", operation.Name);
                    },
                    o =>
                    {
                        var m = Assert.IsType<DeleteDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(43, v));
                    }));
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
                        x.HasIndex("Value").ForSqlServerIsClustered();
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
            return default;
        }

        [Fact]
        public void Add_dbfunction_ignore()
        {
            var mi = typeof(SqlServerModelDifferTest).GetRuntimeMethod(nameof(Function), Array.Empty<Type>());

            Execute(
                _ => { },
                modelBuilder => modelBuilder.HasDbFunction(mi),
                operations => Assert.Equal(0, operations.Count));
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

        [Fact]
        public void SeedData_all_operations()
        {
            Execute(
                _ => { },
                source => source.Entity(
                    "EntityWithTwoProperties",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("Value1");
                        x.Property<string>("Value2");
                        x.HasData(
                            new
                            {
                                Id = 99999,
                                Value1 = 0,
                                Value2 = ""
                            }, // deleted
                            new
                            {
                                Id = 42,
                                Value1 = 32,
                                Value2 = "equal",
                                InvalidProperty = "is ignored"
                            }, // modified
                            new
                            {
                                Id = 8,
                                Value1 = 100,
                                Value2 = "equal"
                            }, // unchanged
                            new
                            {
                                Id = 24,
                                Value1 = 72,
                                Value2 = "not equal1"
                            }); // modified
                    }),
                target => target.Entity(
                    "EntityWithTwoProperties",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("Value1");
                        x.Property<string>("Value2");
                        x.HasData(
                            new
                            {
                                Id = 11111,
                                Value1 = 0,
                                Value2 = ""
                            }, // added
                            new
                            {
                                Id = 11112,
                                Value1 = 1,
                                Value2 = "new"
                            }, // added
                            new
                            {
                                Id = 42,
                                Value1 = 27,
                                Value2 = "equal",
                                InvalidProperty = "is ignored here too"
                            }, // modified
                            new
                            {
                                Id = 8,
                                Value1 = 100,
                                Value2 = "equal"
                            }, // unchanged
                            new
                            {
                                Id = 24,
                                Value1 = 99,
                                Value2 = "not equal2"
                            }); // modified
                    }),
                upOps => Assert.Collection(
                    upOps,
                    o =>
                    {
                        var m = Assert.IsType<DeleteDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(99999, v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<UpdateDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(24, v));
                        AssertMultidimensionalArray(
                            m.Values,
                            v => Assert.Equal(99, v),
                            v => Assert.Equal("not equal2", v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<UpdateDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(42, v));
                        AssertMultidimensionalArray(
                            m.Values,
                            v => Assert.Equal(27, v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<InsertDataOperation>(o);
                        Assert.Collection(
                            ToJaggedArray(m.Values),
                            r => Assert.Collection(
                                r,
                                v => Assert.Equal(11111, v),
                                v => Assert.Equal(0, v),
                                v => Assert.Equal("", v)),
                            r => Assert.Collection(
                                r,
                                v => Assert.Equal(11112, v),
                                v => Assert.Equal(1, v),
                                v => Assert.Equal("new", v))
                        );
                    }),
                downOps => Assert.Collection(
                    downOps,
                    o =>
                    {
                        var m = Assert.IsType<DeleteDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(11111, v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<DeleteDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(11112, v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<UpdateDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(24, v));
                        AssertMultidimensionalArray(
                            m.Values,
                            v => Assert.Equal(72, v),
                            v => Assert.Equal("not equal1", v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<UpdateDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.KeyValues,
                            v => Assert.Equal(42, v));
                        AssertMultidimensionalArray(
                            m.Values,
                            v => Assert.Equal(32, v));
                    },
                    o =>
                    {
                        var m = Assert.IsType<InsertDataOperation>(o);
                        AssertMultidimensionalArray(
                            m.Values,
                            v => Assert.Equal(99999, v),
                            v => Assert.Equal(0, v),
                            v => Assert.Equal("", v));
                    }));
        }

        [Fact]
        public void Dont_rebuild_index_with_equal_include()
        {
            Execute(
                source => source
                    .Entity(
                        "Address",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Zip");
                            x.Property<string>("City");
                            x.HasIndex("Zip")
                                .ForSqlServerInclude("City");
                        }),
                target => target
                    .Entity(
                        "Address",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Zip");
                            x.Property<string>("City");
                            x.HasIndex("Zip")
                                .ForSqlServerInclude("City");
                        }),
                operations => Assert.Equal(0, operations.Count));
        }

        [Fact]
        public void Rebuild_index_with_different_include()
        {
            Execute(
                source => source
                    .Entity(
                        "Address",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Zip");
                            x.Property<string>("City");
                            x.Property<string>("Street");
                            x.HasIndex("Zip")
                                .ForSqlServerInclude("City");
                        }),
                target => target
                    .Entity(
                        "Address",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Zip");
                            x.Property<string>("City");
                            x.Property<string>("Street");
                            x.HasIndex("Zip")
                                .ForSqlServerInclude("Street");
                        }),
                operations =>
                {
                    Assert.Equal(2, operations.Count);

                    var operation1 = Assert.IsType<DropIndexOperation>(operations[0]);
                    Assert.Equal("Address", operation1.Table);
                    Assert.Equal("IX_Address_Zip", operation1.Name);

                    var operation2 = Assert.IsType<CreateIndexOperation>(operations[1]);
                    Assert.Equal("Address", operation1.Table);
                    Assert.Equal("IX_Address_Zip", operation1.Name);

                    var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.Include);
                    Assert.NotNull(annotation);

                    var annotationValue = Assert.IsType<string[]>(annotation.Value);
                    Assert.Equal(1, annotationValue.Length);
                    Assert.Equal("Street", annotationValue[0]);
                });
        }

        protected override TestHelpers TestHelpers => SqlServerTestHelpers.Instance;

        protected override MigrationsModelDiffer CreateModelDiffer(IModel model)
        {
            var ctx = TestHelpers.CreateContext(
                TestHelpers.AddProviderOptions(new DbContextOptionsBuilder())
                    .UseModel(model).EnableSensitiveDataLogging().Options);
            return new MigrationsModelDiffer(
                new SqlServerTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                new SqlServerMigrationsAnnotationProvider(
                    new MigrationsAnnotationProviderDependencies()),
                ctx.GetService<IChangeDetector>(),
                ctx.GetService<StateManagerDependencies>(),
                ctx.GetService<CommandBatchPreparerDependencies>());
        }

        private bool? IsMemoryOptimized(Annotatable annotatable)
            => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool?;
    }
}

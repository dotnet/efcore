// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqlServerModelDifferTest : MigrationsModelDifferTestBase
{
    [ConditionalFact]
    public void Alter_database_edition_options()
        => Execute(
            _ => { },
            source => source.HasDatabaseMaxSize("100 MB")
                .HasPerformanceLevel("S0"),
            target => target
                .HasServiceTier("basic"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(upOps[0]);
                Assert.Equal(
                    "EDITION = 'basic'",
                    alterDatabaseOperation[SqlServerAnnotationNames.EditionOptions]);
                Assert.Equal(
                    "MAXSIZE = 100 MB, SERVICE_OBJECTIVE = 'S0'",
                    alterDatabaseOperation.OldDatabase[SqlServerAnnotationNames.EditionOptions]);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);

                var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(downOps[0]);
                Assert.Equal(
                    "MAXSIZE = 100 MB, SERVICE_OBJECTIVE = 'S0'",
                    alterDatabaseOperation[SqlServerAnnotationNames.EditionOptions]);
                Assert.Equal(
                    "EDITION = 'basic'",
                    alterDatabaseOperation.OldDatabase[SqlServerAnnotationNames.EditionOptions]);
            });

    [ConditionalFact]
    public void Alter_table_MemoryOptimized()
        => Execute(
            common => common.Entity(
                "Person",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered(false);
                }),
            _ => { },
            target => target.Entity(
                "Person",
                x =>
                {
                    x.ToTable(tb => tb.IsMemoryOptimized());
                }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(upOps[0]);
                Assert.True(IsMemoryOptimized(alterDatabaseOperation));
                Assert.Null(IsMemoryOptimized(alterDatabaseOperation.OldDatabase));

                Assert.Single(alterDatabaseOperation.GetAnnotations());
                Assert.Empty(alterDatabaseOperation.OldDatabase.GetAnnotations());

                var alterTableOperation = Assert.IsType<AlterTableOperation>(upOps[1]);
                Assert.Equal("Person", alterTableOperation.Name);
                Assert.True(IsMemoryOptimized(alterTableOperation));
                Assert.Null(IsMemoryOptimized(alterTableOperation.OldTable));

                Assert.Single(alterTableOperation.GetAnnotations());
                Assert.Empty(alterTableOperation.OldTable.GetAnnotations());
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(downOps[0]);
                Assert.Null(IsMemoryOptimized(alterDatabaseOperation));
                Assert.True(IsMemoryOptimized(alterDatabaseOperation.OldDatabase));

                Assert.Empty(alterDatabaseOperation.GetAnnotations());
                Assert.Single(alterDatabaseOperation.OldDatabase.GetAnnotations());

                var alterTableOperation = Assert.IsType<AlterTableOperation>(downOps[1]);
                Assert.Equal("Person", alterTableOperation.Name);
                Assert.Null(IsMemoryOptimized(alterTableOperation));
                Assert.True(IsMemoryOptimized(alterTableOperation.OldTable));

                Assert.Empty(alterTableOperation.GetAnnotations());
                Assert.Single(alterTableOperation.OldTable.GetAnnotations());
            });

    [ConditionalFact]
    public void Add_table_MemoryOptimized()
        => Execute(
            _ => { },
            _ => { },
            target => target.Entity(
                "Person",
                x =>
                {
                    x.ToTable(tb => tb.IsMemoryOptimized());
                }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(upOps[0]);
                Assert.True(IsMemoryOptimized(alterDatabaseOperation));
                Assert.Null(IsMemoryOptimized(alterDatabaseOperation.OldDatabase));

                Assert.Single(alterDatabaseOperation.GetAnnotations());
                Assert.Empty(alterDatabaseOperation.OldDatabase.GetAnnotations());

                var createTableOperation = Assert.IsType<CreateTableOperation>(upOps[1]);
                Assert.Equal("Person", createTableOperation.Name);
                Assert.True(IsMemoryOptimized(createTableOperation));

                Assert.Single(createTableOperation.GetAnnotations());
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var dropTableOperation = Assert.IsType<DropTableOperation>(downOps[0]);
                Assert.Equal("Person", dropTableOperation.Name);
                Assert.True(IsMemoryOptimized(dropTableOperation));

                Assert.Single(dropTableOperation.GetAnnotations());

                var alterDatabaseOperation = Assert.IsType<AlterDatabaseOperation>(downOps[1]);
                Assert.Null(IsMemoryOptimized(alterDatabaseOperation));
                Assert.True(IsMemoryOptimized(alterDatabaseOperation.OldDatabase));

                Assert.Empty(alterDatabaseOperation.GetAnnotations());
                Assert.Single(alterDatabaseOperation.OldDatabase.GetAnnotations());
            });

    [ConditionalFact]
    public void Add_column_with_dependencies()
        => Execute(
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

    [ConditionalFact]
    public void Alter_column_identity()
        => Execute(
            source => source.Entity("Lamb").ToTable("Lamb", "bah").Property<int>("Id").ValueGeneratedNever(),
            target => target.Entity("Lamb").ToTable("Lamb", "bah").Property<int>("Id").ValueGeneratedOnAdd(),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("bah", operation.Schema);
                Assert.Equal("Lamb", operation.Table);
                Assert.Equal("Id", operation.Name);
                Assert.Equal("1, 1", operation["SqlServer:Identity"]);
            });

    [ConditionalFact]
    public void Alter_column_non_key_identity()
        => Execute(
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
                Assert.Equal("1, 1", operation["SqlServer:Identity"]);
            });

    [ConditionalFact]
    public void Alter_column_computation()
        => Execute(
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

    [ConditionalFact] // Issue #30321
    public void Rename_column_TPC()
        => Execute(
            source =>
            {
                source.Entity(
                    "Campaign",
                    x =>
                    {
                        x.ToTable((string)null);
                        x.UseTpcMappingStrategy();
                        x.Property<int>("Id");
                        x.Property<int>("Status");
                    });

                source.Entity(
                    "SearchCampaign",
                    x =>
                    {
                        x.HasBaseType("Campaign");
                    });
            },
            source =>
            {
            },
            target =>
            {
                target.Entity(
                    "Campaign",
                    x =>
                    {
                        x.Property<int>("Status").HasColumnName("status_new");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Null(operation.Schema);
                Assert.Equal("SearchCampaign", operation.Table);
                Assert.Equal("Status", operation.Name);
                Assert.Equal("status_new", operation.NewName);
            });

    [ConditionalFact]
    public void Rename_column_TPT()
        => Execute(
            source =>
            {
                source.Entity(
                    "Campaign",
                    x =>
                    {
                        x.UseTptMappingStrategy();
                        x.Property<int>("Id");
                        x.Property<int>("Status");
                    });

                source.Entity(
                    "SearchCampaign",
                    x =>
                    {
                        x.HasBaseType("Campaign");
                    });
            },
            source =>
            {
            },
            target =>
            {
                target.Entity(
                    "Campaign",
                    x =>
                    {
                        x.Property<int>("Status").HasColumnName("status_new");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Null(operation.Schema);
                Assert.Equal("Campaign", operation.Table);
                Assert.Equal("Status", operation.Name);
                Assert.Equal("status_new", operation.NewName);
            });

    [ConditionalFact]
    public void Rename_column_TPC_non_abstract()
        => Execute(
            source =>
            {
                source.Entity(
                    "Campaign",
                    x =>
                    {
                        x.UseTpcMappingStrategy();
                        x.Property<int>("Id");
                        x.Property<int>("Status");
                    });

                source.Entity(
                    "SearchCampaign",
                    x =>
                    {
                        x.HasBaseType("Campaign");
                    });
            },
            source =>
            {
            },
            target =>
            {
                target.Entity(
                    "Campaign",
                    x =>
                    {
                        x.Property<int>("Status").HasColumnName("status_new");
                    });
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Null(operation.Schema);
                Assert.Equal("SearchCampaign", operation.Table);
                Assert.Equal("Status", operation.Name);
                Assert.Equal("status_new", operation.NewName);

                operation = Assert.IsType<RenameColumnOperation>(operations[1]);
                Assert.Null(operation.Schema);
                Assert.Equal("Campaign", operation.Table);
                Assert.Equal("Status", operation.Name);
                Assert.Equal("status_new", operation.NewName);
            });

    [ConditionalFact]
    public void Alter_primary_key_clustering()
        => Execute(
            source => source.Entity(
                "Ram",
                x =>
                {
                    x.ToTable("Ram", "bah");
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered(false);
                }),
            target => target.Entity(
                "Ram",
                x =>
                {
                    x.ToTable("Ram", "bah");
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered();
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

    [ConditionalFact]
    public void Add_non_clustered_primary_key_with_owned()
        => Execute(
            _ => { },
            target => target.Entity(
                "Ram",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered(false);
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

    [ConditionalFact]
    public void Alter_unique_constraint_clustering()
        => Execute(
            source => source.Entity(
                "Ewe",
                x =>
                {
                    x.ToTable("Ewe", "bah");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId").IsClustered(false);
                }),
            target => target.Entity(
                "Ewe",
                x =>
                {
                    x.ToTable("Ewe", "bah");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId").IsClustered();
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

    [ConditionalFact]
    public void Create_shared_table_with_two_entity_types()
        => Execute(
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
                Assert.Equal("1, 1", idColumn["SqlServer:Identity"]);
                var timeColumn = createTableOperation.Columns[1];
                Assert.Equal("Time", timeColumn.Name);
                Assert.True(timeColumn.IsNullable);
            });

    [ConditionalFact]
    public void Add_SequenceHiLo_with_seed_data()
        => Execute(
            common => common.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("SequenceId");
                    x.HasData(
                        new { Id = 42 });
                }),
            _ => { },
            target => target.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("SequenceId").UseHiLo(schema: "dbo");
                    x.HasData(
                        new { Id = 43 });
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
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropSequenceOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("EntityFrameworkHiLoSequence", operation.Name);
                }));

    [ConditionalFact]
    public void Add_KeySequence_with_seed_data()
        => Execute(
            common => common.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("SequenceId");
                    x.HasData(
                        new { Id = 42 });
                }),
            _ => { },
            target => target.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("SequenceId").UseSequence(schema: "dbo");
                    x.HasData(
                        new { Id = 43 });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<CreateSequenceOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("FireflySequence", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("NEXT VALUE FOR [dbo].[FireflySequence]", operation.DefaultValueSql);
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
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropSequenceOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("FireflySequence", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Null(operation.DefaultValueSql);
                }));

    [ConditionalFact]
    public void Alter_index_clustering()
        => Execute(
            source => source.Entity(
                "Mutton",
                x =>
                {
                    x.ToTable("Mutton", "bah");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value").IsClustered(false);
                }),
            target => target.Entity(
                "Mutton",
                x =>
                {
                    x.ToTable("Mutton", "bah");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value").IsClustered();
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("bah", dropOperation.Schema);
                Assert.Equal("Mutton", dropOperation.Table);
                Assert.Equal("IX_Mutton_Value", dropOperation.Name);

                Assert.Empty(dropOperation.GetAnnotations());

                var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("bah", createOperation.Schema);
                Assert.Equal("Mutton", createOperation.Table);
                Assert.Equal("IX_Mutton_Value", createOperation.Name);
                Assert.True((bool)createOperation[SqlServerAnnotationNames.Clustered]);
            });

    public static int Function()
        => default;

    [ConditionalFact]
    public void Add_dbfunction_ignore()
    {
        var mi = typeof(SqlServerModelDifferTest).GetRuntimeMethod(nameof(Function), []);

        Execute(
            _ => { },
            modelBuilder => modelBuilder.HasDbFunction(mi),
            operations => Assert.Equal(0, operations.Count));
    }

    [ConditionalFact]
    public void Alter_column_rowversion()
        => Execute(
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

    [ConditionalFact]
    public void SeedData_all_operations()
        => Execute(
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

    [ConditionalFact]
    public void Dont_reseed_value_with_value_generated_on_add_property()
        => Execute(
            common =>
            {
                common.Entity(
                    "EntityWithValueGeneratedOnAddProperty",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("ValueGeneratedOnAddProperty")
                            .ValueGeneratedOnAdd();
                        x.HasData(
                            new { Id = 1, ValueGeneratedOnAddProperty = "Value" });
                    });
            },
            source => { },
            target => { },
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Dont_rebuild_index_with_equal_include()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasIndex("Zip")
                            .IncludeProperties("City");
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
                            .IncludeProperties("City");
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Rebuild_index_with_different_include()
        => Execute(
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
                            .IncludeProperties("City");
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
                            .IncludeProperties("Street");
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.Include);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<string[]>(annotation.Value);
                Assert.Single(annotationValue);
                Assert.Equal("Street", annotationValue[0]);
            });

    [ConditionalFact]
    public void Dont_rebuild_index_with_unchanged_online_option()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasIndex("Zip")
                            .IsCreatedOnline();
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
                            .IsCreatedOnline();
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Rebuild_index_when_changing_online_option()
        => Execute(
            _ => { },
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
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
                            .IsCreatedOnline();
                    }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(upOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(upOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.CreatedOnline);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<bool>(annotation.Value);
                Assert.True(annotationValue);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(downOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(downOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation2.GetAnnotations());
            });

    [ConditionalFact]
    public void Noop_TPT_with_FKs_and_seed_data()
        => Execute(
            modelBuilder =>
            {
            },
            source =>
            {
                source.Entity(
                    "Animal", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .UseIdentityColumn();

                        b.Property<int?>("MouseId")
                            .HasColumnType("int");

                        b.HasKey("Id");

                        b.HasIndex("MouseId");

                        b.ToTable("Animal");
                    });

                source.Entity(
                    "Cat", b =>
                    {
                        b.HasBaseType("Animal");

                        b.Property<int?>("PreyId")
                            .HasColumnType("int")
                            .HasColumnName("PreyId");

                        b.HasIndex("PreyId");

                        b.ToTable("Cats");

                        b.HasData(
                            new { Id = 11, MouseId = 31 });
                    });

                source.Entity(
                    "Dog", b =>
                    {
                        b.HasBaseType("Animal");

                        b.Property<int?>("PreyId")
                            .HasColumnType("int")
                            .HasColumnName("PreyId");

                        b.HasIndex("PreyId");

                        b.ToTable("Dogs");

                        b.HasData(
                            new { Id = 21, PreyId = 31 });
                    });

                source.Entity(
                    "Mouse", b =>
                    {
                        b.HasBaseType("Animal");

                        b.ToTable("Mice");

                        b.HasData(
                            new { Id = 31 });
                    });

                source.Entity(
                    "Animal", b =>
                    {
                        b.HasOne("Mouse", null)
                            .WithMany()
                            .HasForeignKey("MouseId");
                    });

                source.Entity(
                    "Cat", b =>
                    {
                        b.HasOne("Animal", null)
                            .WithOne()
                            .HasForeignKey("Cat", "Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();

                        b.HasOne("Animal", null)
                            .WithMany()
                            .HasForeignKey("PreyId");
                    });

                source.Entity(
                    "Dog", b =>
                    {
                        b.HasOne("Animal", null)
                            .WithOne()
                            .HasForeignKey("Dog", "Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();

                        b.HasOne("Animal", null)
                            .WithMany()
                            .HasForeignKey("PreyId");
                    });

                source.Entity(
                    "Mouse", b =>
                    {
                        b.HasOne("Animal", null)
                            .WithOne()
                            .HasForeignKey("Mouse", "Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal", x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int?>("MouseId");

                        x.HasOne("Mouse").WithMany().HasForeignKey("MouseId");
                    });
                modelBuilder.Entity(
                    "Cat", x =>
                    {
                        x.HasBaseType("Animal");
                        x.ToTable("Cats");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 11, MouseId = 31 });
                    });
                modelBuilder.Entity(
                    "Dog", x =>
                    {
                        x.HasBaseType("Animal");
                        x.ToTable("Dogs");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 21, PreyId = 31 });
                    });
                modelBuilder.Entity(
                    "Mouse", x =>
                    {
                        x.HasBaseType("Animal");
                        x.ToTable("Mice");

                        x.HasData(
                            new { Id = 31 });
                    });
            },
            upOps => Assert.Empty(upOps),
            downOps => Assert.Empty(downOps),
            skipSourceConventions: true);

    protected override TestHelpers TestHelpers
        => SqlServerTestHelpers.Instance;

    protected override MigrationsModelDiffer CreateModelDiffer(DbContextOptions options)
        => (MigrationsModelDiffer)TestHelpers.CreateContext(options).GetService<IMigrationsModelDiffer>();

    private bool? IsMemoryOptimized(Annotatable annotatable)
        => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool?;

    [ConditionalFact]
    public void Dont_rebuild_key_index_with_unchanged_fillfactor_option()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").HasFillFactor(90);
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                    }),
            target => target
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").HasFillFactor(90);
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Dont_rebuild_composite_key_index_with_unchanged_fillfactor_option()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasAlternateKey("Zip", "City").HasFillFactor(90);
                    }),
            target => target
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasAlternateKey("Zip", "City").HasFillFactor(90);
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Dont_rebuild_index_with_unchanged_fillfactor_option()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasIndex("Zip")
                            .HasFillFactor(90);
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
                            .HasFillFactor(90);
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Rebuild_key_index_when_adding_fillfactor_option()
        => Execute(
            _ => { },
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
                    }),
            target => target
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").HasFillFactor(90);
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
                    }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var operation1 = Assert.IsType<DropPrimaryKeyOperation>(upOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("PK_Address", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<AddPrimaryKeyOperation>(upOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("PK_Address", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.FillFactor);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<int>(annotation.Value);
                Assert.Equal(90, annotationValue);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var operation1 = Assert.IsType<DropPrimaryKeyOperation>(downOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("PK_Address", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<AddPrimaryKeyOperation>(downOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("PK_Address", operation1.Name);

                Assert.Empty(operation2.GetAnnotations());
            });

    [ConditionalFact]
    public void Rebuild_key_index_with_different_fillfactor_value()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").HasFillFactor(50);
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                    }),
            target => target
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").HasFillFactor(90);
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("PK_Address", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("PK_Address", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.FillFactor);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<int>(annotation.Value);

                Assert.Equal(90, annotationValue);
            });

    [ConditionalFact]
    public void Rebuild_composite_key_index_when_adding_fillfactor_option()
        => Execute(
            _ => { },
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasAlternateKey("Zip", "City");
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
                        x.HasAlternateKey("Zip", "City").HasFillFactor(90);
                    }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var operation1 = Assert.IsType<DropUniqueConstraintOperation>(upOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("AK_Address_Zip_City", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<AddUniqueConstraintOperation>(upOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("AK_Address_Zip_City", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.FillFactor);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<int>(annotation.Value);
                Assert.Equal(90, annotationValue);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var operation1 = Assert.IsType<DropUniqueConstraintOperation>(downOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("AK_Address_Zip_City", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<AddUniqueConstraintOperation>(downOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("AK_Address_Zip_City", operation1.Name);

                Assert.Empty(operation2.GetAnnotations());
            });

    [ConditionalFact]
    public void Rebuild_composite_key_index_with_different_fillfactor_value()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
                        x.HasAlternateKey("Zip", "City").HasFillFactor(50);
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
                        x.HasIndex("Zip");
                        x.HasAlternateKey("Zip", "City").HasFillFactor(90);
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("AK_Address_Zip_City", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("AK_Address_Zip_City", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.FillFactor);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<int>(annotation.Value);

                Assert.Equal(90, annotationValue);
            });

    [ConditionalFact]
    public void Rebuild_index_when_adding_fillfactor_option()
        => Execute(
            _ => { },
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
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
                            .HasFillFactor(90);
                    }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(upOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(upOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.FillFactor);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<int>(annotation.Value);
                Assert.Equal(90, annotationValue);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(downOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(downOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation2.GetAnnotations());
            });

    [ConditionalFact]
    public void Rebuild_index_with_different_fillfactor_value()
        => Execute(
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
                            .HasFillFactor(50);
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
                            .HasFillFactor(90);
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.FillFactor);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<int>(annotation.Value);

                Assert.Equal(90, annotationValue);
            });

    [ConditionalFact]
    public void Dont_rebuild_index_with_unchanged_sortintempdb_option()
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasIndex("Zip")
                            .SortInTempDb();
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
                            .SortInTempDb();
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Rebuild_index_when_changing_sortintempdb_option()
        => Execute(
            _ => { },
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
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
                            .SortInTempDb();
                    }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(upOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(upOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.SortInTempDb);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<bool>(annotation.Value);
                Assert.True(annotationValue);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(downOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(downOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation2.GetAnnotations());
            });

    [ConditionalTheory]
    [InlineData(DataCompressionType.None)]
    [InlineData(DataCompressionType.Row)]
    [InlineData(DataCompressionType.Page)]
    public void Dont_rebuild_index_with_unchanged_datacompression_option(DataCompressionType dataCompression)
        => Execute(
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.HasIndex("Zip")
                            .UseDataCompression(dataCompression);
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
                            .UseDataCompression(dataCompression);
                    }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalTheory]
    [InlineData(DataCompressionType.None)]
    [InlineData(DataCompressionType.Row)]
    [InlineData(DataCompressionType.Page)]
    public void Rebuild_index_when_adding_datacompression_option(DataCompressionType dataCompression)
        => Execute(
            _ => { },
            source => source
                .Entity(
                    "Address",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Zip");
                        x.Property<string>("City");
                        x.Property<string>("Street");
                        x.HasIndex("Zip");
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
                            .UseDataCompression(dataCompression);
                    }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(upOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(upOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.DataCompression);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<DataCompressionType>(annotation.Value);
                Assert.Equal(dataCompression, annotationValue);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(downOps[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(downOps[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation2.GetAnnotations());
            });

    [ConditionalFact]
    public void Rebuild_index_with_different_datacompression_value()
        => Execute(
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
                            .UseDataCompression(DataCompressionType.Row);
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
                            .UseDataCompression(DataCompressionType.Page);
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                Assert.Empty(operation1.GetAnnotations());

                var operation2 = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("Address", operation1.Table);
                Assert.Equal("IX_Address_Zip", operation1.Name);

                var annotation = operation2.GetAnnotation(SqlServerAnnotationNames.DataCompression);
                Assert.NotNull(annotation);

                var annotationValue = Assert.IsType<DataCompressionType>(annotation.Value);

                Assert.Equal(DataCompressionType.Page, annotationValue);
            });
}

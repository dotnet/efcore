// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

public partial class MigrationsSqlServerTest : MigrationsTestBase<MigrationsSqlServerTest.MigrationsSqlServerFixture>
{
    [ConditionalFact]
    public virtual async Task Create_temporal_table_default_column_mappings_and_default_history_table()
    {
        await Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("SystemTimeStart");
                                ttb.HasPeriodEnd("SystemTimeEnd");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomerHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + N'.[CustomerHistory]))');
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_custom_column_mappings_and_default_history_table()
    {
        await Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("SystemTimeStart").HasColumnName("Start");
                                ttb.HasPeriodEnd("SystemTimeEnd").HasColumnName("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomerHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [End] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [Start] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([Start], [End])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + N'.[CustomerHistory]))');
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_default_column_mappings_and_custom_history_table()
    {
        await Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("SystemTimeStart");
                                ttb.HasPeriodEnd("SystemTimeEnd");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + N'.[HistoryTable]))');
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_explicitly_defined_schema()
    {
        await Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("SystemTimeStart");
                                ttb.HasPeriodEnd("SystemTimeEnd");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema') IS NULL EXEC(N'CREATE SCHEMA [mySchema];');
""",
            //
            """
CREATE TABLE [mySchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema].[CustomersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_schema_for_model_changed_and_no_explicit_table_schema_provided()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("myDefaultSchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myDefaultSchema].[CustomersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_schema_for_model_changed_and_explicit_table_schema_provided()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema') IS NULL EXEC(N'CREATE SCHEMA [mySchema];');
""",
            //
            """
CREATE TABLE [mySchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema].[CustomersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_model_schema()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("myDefaultSchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myDefaultSchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myDefaultSchema].[CustomersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_model_schema_specified_after_entity_definition()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });

                builder.Entity("Customer", e => e.ToTable("Customers", "mySchema1"));
                builder.Entity("Customer", e => e.ToTable("Customers"));
                builder.HasDefaultSchema("myDefaultSchema");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("myDefaultSchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myDefaultSchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myDefaultSchema].[CustomersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task
        Create_temporal_table_with_default_model_schema_specified_after_entity_definition_and_history_table_schema_specified_explicitly()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("History", "myHistorySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });

                builder.Entity("Customer", e => e.ToTable("Customers", "mySchema1"));
                builder.Entity("Customer", e => e.ToTable("Customers"));
                builder.HasDefaultSchema("myDefaultSchema");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("myDefaultSchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("History", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myHistorySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
IF SCHEMA_ID(N'myHistorySchema') IS NULL EXEC(N'CREATE SCHEMA [myHistorySchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myHistorySchema].[History]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_model_schema_changed_after_entity_definition()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.HasDefaultSchema("myFakeSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });

                builder.HasDefaultSchema("myDefaultSchema");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("myDefaultSchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myDefaultSchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myDefaultSchema].[CustomersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task
        Create_temporal_table_with_default_schema_for_model_changed_and_explicit_history_table_schema_not_provided()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myDefaultSchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myDefaultSchema].[HistoryTable]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_schema_for_model_changed_and_explicit_history_table_schema_provided()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("historySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myDefaultSchema];');
""",
            //
            """
IF SCHEMA_ID(N'historySchema') IS NULL EXEC(N'CREATE SCHEMA [historySchema];');
""",
            //
            """
CREATE TABLE [myDefaultSchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]));
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_default_schema_for_table_and_explicit_history_table_schema_provided()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("historySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'historySchema') IS NULL EXEC(N'CREATE SCHEMA [historySchema];');
""",
            //
            """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]));
""");
    }

    [ConditionalFact]
    public virtual async Task Drop_temporal_table_default_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("Start").HasColumnName("PeriodStart");
                                ttb.HasPeriodEnd("End").HasColumnName("PeriodEnd");
                            }));
                }),
            builder => { },
            model =>
            {
                Assert.Empty(model.Tables);
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DROP TABLE [Customer];
""",
            //
            """
DROP TABLE [CustomerHistory];
""");
    }

    [ConditionalFact]
    public virtual async Task Drop_temporal_table_custom_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start").HasColumnName("PeriodStart");
                                ttb.HasPeriodEnd("End").HasColumnName("PeriodEnd");
                            }));
                }),
            builder => { },
            model =>
            {
                Assert.Empty(model.Tables);
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DROP TABLE [Customer];
""",
            //
            """
DROP TABLE [HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Drop_temporal_table_custom_history_table_and_history_table_schema()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "historySchema");
                                ttb.HasPeriodStart("Start").HasColumnName("PeriodStart");
                                ttb.HasPeriodEnd("End").HasColumnName("PeriodEnd");
                            }));
                }),
            builder => { },
            model =>
            {
                Assert.Empty(model.Tables);
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DROP TABLE [Customer];
""",
            //
            """
DROP TABLE [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("RenamedCustomers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("RenamedCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
""",
            //
            """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_temporal_table_rename_and_modify_column_in_same_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<decimal>("Discount");
                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<DateTime>("DoB");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<decimal>("Discount").HasComment("for VIP only");
                    e.Property<DateTime>("DateOfBirth");
                    e.ToTable("RenamedCustomers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("RenamedCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Discount", c.Name),
                    c => Assert.Equal("DateOfBirth", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
""",
            //
            """
EXEC sp_rename N'[RenamedCustomers].[DoB]', N'DateOfBirth', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable].[DoB]', N'DateOfBirth', 'COLUMN';
""",
            //
            """
DECLARE @defaultSchema2 AS sysname;
SET @defaultSchema2 = SCHEMA_NAME();
DECLARE @description2 AS sql_variant;
SET @description2 = N'for VIP only';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'RenamedCustomers', 'COLUMN', N'Discount';
""",
            //
            """
DECLARE @defaultSchema3 AS sysname;
SET @defaultSchema3 = SCHEMA_NAME();
DECLARE @description3 AS sql_variant;
SET @description3 = N'for VIP only';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'HistoryTable', 'COLUMN', N'Discount';
""",
            //
            """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_temporal_table_with_custom_history_table_schema()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "historySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("RenamedCustomers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("RenamedCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
""",
            //
            """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]))
""");
    }

    public virtual async Task Rename_temporal_table_schema_when_history_table_doesnt_have_its_schema_specified()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customers", "mySchema2");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema2", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("mySchema2", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[Customers];
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[HistoryTable];
""",
            //
            """
ALTER TABLE [mySchema2].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_temporal_table_schema_when_history_table_has_its_schema_specified()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "myHistorySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customers", "mySchema2");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema2", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myHistorySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[Customers];
""",
            //
            """
ALTER TABLE [mySchema2].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myHistorySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_temporal_table_schema_and_history_table_name_when_history_table_doesnt_have_its_schema_specified()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "mySchema2", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable2");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema2", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable2", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("mySchema2", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[Customers];
""",
            //
            """
EXEC sp_rename N'[mySchema].[HistoryTable]', N'HistoryTable2', 'OBJECT';
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[HistoryTable2];
""",
            //
            """
ALTER TABLE [mySchema2].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[HistoryTable2]))
""");
    }

    [ConditionalFact]
    public virtual async Task
        Rename_temporal_table_schema_and_history_table_name_when_history_table_doesnt_have_its_schema_specified_convention_with_default_global_schema22()
    {
        await Test(
            builder =>
            {
                builder.HasDefaultSchema("defaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.HasKey("Id");
                    });
            },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "mySchema2", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable2");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema2", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable2", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("mySchema2", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
ALTER TABLE [defaultSchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [defaultSchema].[Customers];
""",
            //
            """
EXEC sp_rename N'[defaultSchema].[HistoryTable]', N'HistoryTable2', 'OBJECT';
ALTER SCHEMA [mySchema2] TRANSFER [defaultSchema].[HistoryTable2];
""",
            //
            """
ALTER TABLE [mySchema2].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[HistoryTable2]))
""");
    }

    [ConditionalFact]
    public virtual async Task
        Rename_temporal_table_schema_and_history_table_name_when_history_table_doesnt_have_its_schema_specified_convention_with_default_global_schema_and_table_schema_corrected()
    {
        await Test(
            builder =>
            {
                builder.HasDefaultSchema("defaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.HasKey("Id");
                    });
            },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));

                    e.ToTable("Customers", "modifiedSchema");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "mySchema2", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable2");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema2", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable2", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("mySchema2", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
ALTER TABLE [modifiedSchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [modifiedSchema].[Customers];
""",
            //
            """
EXEC sp_rename N'[modifiedSchema].[HistoryTable]', N'HistoryTable2', 'OBJECT';
ALTER SCHEMA [mySchema2] TRANSFER [modifiedSchema].[HistoryTable2];
""",
            //
            """
ALTER TABLE [mySchema2].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[HistoryTable2]))
""");
    }

    [ConditionalFact]
    public virtual async Task
        Rename_temporal_table_schema_when_history_table_doesnt_have_its_schema_specified_convention_with_default_global_schema_and_table_name_corrected()
    {
        await Test(
            builder =>
            {
                builder.HasDefaultSchema("defaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.HasKey("Id");
                    });
            },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "MockCustomers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));

                    e.ToTable("Customers", "mySchema");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "mySchema2", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema2", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("mySchema2", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[Customers];
""",
            //
            """
ALTER SCHEMA [mySchema2] TRANSFER [mySchema].[CustomersHistory];
""",
            //
            """
ALTER TABLE [mySchema2].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[CustomersHistory]))
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("RenamedHistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("RenamedHistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[HistoryTable]', N'RenamedHistoryTable', 'OBJECT';
""");
    }

    [ConditionalFact]
    public virtual async Task Change_history_table_schema()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "historySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "modifiedHistorySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("modifiedHistorySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'modifiedHistorySchema') IS NULL EXEC(N'CREATE SCHEMA [modifiedHistorySchema];');
""",
            //
            """
ALTER SCHEMA [modifiedHistorySchema] TRANSFER [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_temporal_table_history_table_and_their_schemas()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "Customers", "schema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "historySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));

                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        "RenamedCustomers", "newSchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("RenamedHistoryTable", "newHistorySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("RenamedCustomers", table.Name);
                Assert.Equal("newSchema", table.Schema);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("RenamedHistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("newHistorySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
IF SCHEMA_ID(N'newSchema') IS NULL EXEC(N'CREATE SCHEMA [newSchema];');
""",
            //
            """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
ALTER SCHEMA [newSchema] TRANSFER [RenamedCustomers];
""",
            //
            """
IF SCHEMA_ID(N'newHistorySchema') IS NULL EXEC(N'CREATE SCHEMA [newHistorySchema];');
""",
            //
            """
EXEC sp_rename N'[historySchema].[HistoryTable]', N'RenamedHistoryTable', 'OBJECT';
ALTER SCHEMA [newHistorySchema] TRANSFER [historySchema].[RenamedHistoryTable];
""",
            //
            """
ALTER TABLE [newSchema].[RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
ALTER TABLE [newSchema].[RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [newHistorySchema].[RenamedHistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_columns_from_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            builder =>
            {
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Name');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_columns_from_temporal_table_with_history_table_schema()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "myHistorySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            builder =>
            {
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [myHistorySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [myHistorySchema].[HistoryTable] DROP COLUMN [Number];
""",
            //
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myHistorySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_columns_from_temporal_table_with_table_schema()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            builder =>
            {
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [mySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [mySchema].[HistoryTable] DROP COLUMN [Number];
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_columns_from_temporal_table_with_default_schema()
    {
        await Test(
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable");
                                    ttb.HasPeriodStart("Start");
                                    ttb.HasPeriodEnd("End");
                                }));
                    });
            },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            builder =>
            {
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [mySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [mySchema].[HistoryTable] DROP COLUMN [Number];
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_columns_from_temporal_table_with_different_schemas_on_each_level()
    {
        await Test(
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "myHistorySchema");
                                    ttb.HasPeriodStart("Start");
                                    ttb.HasPeriodEnd("End");
                                }));
                    });
            },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            builder =>
            {
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [myHistorySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [myHistorySchema].[HistoryTable] DROP COLUMN [Number];
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myHistorySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Add_columns_to_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] ADD [Name] nvarchar(max) NULL;
""",
            //
            """
ALTER TABLE [Customers] ADD [Number] int NOT NULL DEFAULT 0;
""");
    }

    [ConditionalFact]
    public virtual async Task
        Convert_temporal_table_with_default_column_mappings_and_custom_history_table_to_normal_table_keep_period_columns()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart");
                    e.Property<DateTime>("PeriodEnd");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("PeriodEnd", c.Name),
                    c => Assert.Equal("PeriodStart", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DROP TABLE [HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_temporal_table_with_default_column_mappings_and_default_history_table_to_normal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodEnd');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customer] DROP COLUMN [PeriodEnd];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodStart');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customer] DROP COLUMN [PeriodStart];
""",
            //
            """
DROP TABLE [CustomerHistory];
""");
    }

    [ConditionalFact]
    public virtual async Task
        Convert_temporal_table_with_default_column_mappings_and_custom_history_table_to_normal_table_remove_period_columns()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodEnd');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customer] DROP COLUMN [PeriodEnd];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodStart');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customer] DROP COLUMN [PeriodStart];
""",
            //
            """
DROP TABLE [HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_temporal_table_with_explicit_history_table_schema_to_normal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "historySchema");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart");
                    e.Property<DateTime>("PeriodEnd");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("PeriodEnd", c.Name),
                    c => Assert.Equal("PeriodStart", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DROP TABLE [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_temporal_table_with_explicit_schemas_same_schema_for_table_and_history_to_normal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customer", "mySchema", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "mySchema");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customer", "mySchema");
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart");
                    e.Property<DateTime>("PeriodEnd");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("PeriodEnd", c.Name),
                    c => Assert.Equal("PeriodStart", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [mySchema].[Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [mySchema].[Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DROP TABLE [mySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_temporal_table_using_custom_default_schema_to_normal_table()
    {
        await Test(
            builder => builder.HasDefaultSchema("myDefaultSchema"),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customer", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customer");
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart");
                    e.Property<DateTime>("PeriodEnd");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("PeriodEnd", c.Name),
                    c => Assert.Equal("PeriodStart", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [myDefaultSchema].[Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [myDefaultSchema].[Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DROP TABLE [myDefaultSchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_temporal_table_using_custom_default_schema_and_explicit_history_schema_to_normal_table()
    {
        await Test(
            builder => builder.HasDefaultSchema("myDefaultSchema"),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customer", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "mySchema");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customer");
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart");
                    e.Property<DateTime>("PeriodEnd");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Null(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Null(table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("PeriodEnd", c.Name),
                    c => Assert.Equal("PeriodStart", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [myDefaultSchema].[Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [myDefaultSchema].[Customer] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DROP TABLE [mySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_normal_table_to_temporal_table_with_minimal_configuration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(tb => tb.IsTemporal());

                    e.Metadata[SqlServerAnnotationNames.TemporalPeriodStartPropertyName] = "PeriodStart";
                    e.Metadata[SqlServerAnnotationNames.TemporalPeriodEndPropertyName] = "PeriodEnd";
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomerHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] ADD [PeriodEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customer] ADD [PeriodStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [PeriodStart] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [PeriodEnd] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[CustomerHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_normal_table_to_temporal_generates_exec_when_idempotent()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(tb => tb.IsTemporal());

                    e.Metadata[SqlServerAnnotationNames.TemporalPeriodStartPropertyName] = "PeriodStart";
                    e.Metadata[SqlServerAnnotationNames.TemporalPeriodEndPropertyName] = "PeriodEnd";
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomerHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
ALTER TABLE [Customer] ADD [PeriodEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customer] ADD [PeriodStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
EXEC(N'ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])')
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [PeriodStart] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [PeriodEnd] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[CustomerHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task
        Convert_normal_table_with_period_columns_to_temporal_table_default_column_mappings_and_default_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start");
                    e.Property<DateTime>("End");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("CustomerHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[CustomerHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task
        Convert_normal_table_with_period_columns_to_temporal_table_default_column_mappings_and_specified_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start");
                    e.Property<DateTime>("End");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_normal_table_to_temporal_table_default_column_mappings_and_default_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.NotNull(table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customer] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[CustomerHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task
        Convert_normal_table_without_period_columns_to_temporal_table_default_column_mappings_and_specified_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customer] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_period_properties_of_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("ModifiedStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("ModifiedEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("ModifiedStart");
                                ttb.HasPeriodEnd("ModifiedEnd");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("ModifiedStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("ModifiedEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customer].[Start]', N'ModifiedStart', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[Customer].[End]', N'ModifiedEnd', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_period_columns_of_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start").HasColumnName("ModifiedStart");
                                ttb.HasPeriodEnd("End").HasColumnName("ModifiedEnd");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("ModifiedStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("ModifiedEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customer].[Start]', N'ModifiedStart', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[Customer].[End]', N'ModifiedEnd', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_period_column_of_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity("Customer").Property<DateTime>("End").HasComment("My comment").ValueGeneratedOnAddOrUpdate(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @defaultSchema1 AS sysname;
SET @defaultSchema1 = SCHEMA_NAME();
DECLARE @description1 AS sql_variant;
SET @description1 = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'Customers', 'COLUMN', N'End';
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_regular_columns_of_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("FullName");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("FullName", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customer].[Name]', N'FullName', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_regular_column_of_temporal_table_from_nullable_to_non_nullable()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));

                    // adding data to make sure default for null value can be applied correctly
                    e.HasData(
                        new { Id = 1, IsVip = (bool?)true },
                        new { Id = 2, IsVip = (bool?)false },
                        new { Id = 3, IsVip = (bool?)null });
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<bool?>("IsVip");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<bool>("IsVip");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("IsVip", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'IsVip');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var2 + ';');
UPDATE [Customer] SET [IsVip] = CAST(0 AS bit) WHERE [IsVip] IS NULL;
ALTER TABLE [Customer] ALTER COLUMN [IsVip] bit NOT NULL;
ALTER TABLE [Customer] ADD DEFAULT CAST(0 AS bit) FOR [IsVip];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'IsVip');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
UPDATE [HistoryTable] SET [IsVip] = CAST(0 AS bit) WHERE [IsVip] IS NULL;
ALTER TABLE [HistoryTable] ALTER COLUMN [IsVip] bit NOT NULL;
ALTER TABLE [HistoryTable] ADD DEFAULT CAST(0 AS bit) FOR [IsVip];
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_regular_table_to_temporal_and_regular_column_from_nullable_to_non_nullable()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<bool?>("IsVip");
                    e.HasKey("Id");

                    e.ToTable("Customers");

                    e.HasData(
                        new { Id = 1, IsVip = (bool?)true },
                        new { Id = 2, IsVip = (bool?)false },
                        new { Id = 3, IsVip = (bool?)null });
                }),

            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.Property<bool>("IsVip");
                    e.HasKey("Id");

                    e.ToTable("Customers",
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));

                    e.HasData(
                        new { Id = 1, IsVip = (bool?)true },
                        new { Id = 2, IsVip = (bool?)false },
                        new { Id = 3, IsVip = (bool?)null });
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("IsVip", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'IsVip');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
UPDATE [Customers] SET [IsVip] = CAST(0 AS bit) WHERE [IsVip] IS NULL;
ALTER TABLE [Customers] ALTER COLUMN [IsVip] bit NOT NULL;
ALTER TABLE [Customers] ADD DEFAULT CAST(0 AS bit) FOR [IsVip];
""",
                //
                """
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
                //
                """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_regular_table_to_temporal_and_regular_column_to_sparse()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<int?>("MyColumn");
                    e.HasKey("Id");

                    e.ToTable("Customers");
                }),

            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.Property<int?>("MyColumn").IsSparse();
                    e.HasKey("Id");

                    e.ToTable("Customers",
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("MyColumn", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
IF EXISTS (SELECT 1 FROM [sys].[tables] [t] INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] WHERE [t].[name] = 'HistoryTable' AND data_compression <> 0)
EXEC(N'ALTER TABLE [HistoryTable] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE);');
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'MyColumn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] ALTER COLUMN [MyColumn] int SPARSE NULL;
""",
                //
                """
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
                //
                """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_computed_column()
    {
        await Test(
            builder => { },
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.Property<int>("Number");
                    e.Property<int?>("NumberPlusFive").HasComputedColumnSql("Number + 5 PERSISTED");
                    e.HasKey("Id");
                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Number", c.Name),
                    c => Assert.Equal("NumberPlusFive", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [End] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [Number] int NOT NULL,
    [NumberPlusFive] AS Number + 5 PERSISTED,
    [Start] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([Start], [End])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + N'.[HistoryTable]))');
""");
    }

    [ConditionalFact]
    public virtual async Task Add_nullable_computed_column_to_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 5 PERSISTED");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("IdPlusFive", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customer] ADD [IdPlusFive] AS Id + 5 PERSISTED;
""",
            //
            """
ALTER TABLE [HistoryTable] ADD [IdPlusFive] int NULL;
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Add_non_nullable_computed_column_to_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Five").HasComputedColumnSql("5 PERSISTED");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Five", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customer] ADD [Five] AS 5 PERSISTED;
""",
            //
            """
ALTER TABLE [HistoryTable] ADD [Five] int NOT NULL DEFAULT 0;
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_computed_column_from_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 5 PERSISTED");
                }),
            builder => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'IdPlusFive');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customer] DROP COLUMN [IdPlusFive];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'IdPlusFive');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [IdPlusFive];
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_computed_column_sql_on_temporal_table()
    {
        var message = (await Assert.ThrowsAsync<NotSupportedException>(
            () => Test(
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable");
                                    ttb.HasPeriodStart("Start");
                                    ttb.HasPeriodEnd("End");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 5 PERSISTED");
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 10 PERSISTED");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("Customer", table.Name);
                    Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                    Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                    Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                    Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                    Assert.Collection(
                        table.Columns,
                        c => Assert.Equal("Id", c.Name),
                        c => Assert.Equal("IdPlusFive", c.Name));
                    Assert.Same(
                        table.Columns.Single(c => c.Name == "Id"),
                        Assert.Single(table.PrimaryKey!.Columns));
                }))).Message;

        Assert.Equal(
            SqlServerStrings.TemporalMigrationModifyingComputedColumnNotSupported("IdPlusFive", "Customer"),
            message);
    }

    [ConditionalFact]
    public virtual async Task Add_column_on_temporal_table_with_computed_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 5 PERSISTED");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Number");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("IdPlusFive", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] ADD [Number] int NOT NULL DEFAULT 0;
""");
    }

    [ConditionalFact]
    public virtual async Task Remove_column_on_temporal_table_with_computed_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 5 PERSISTED");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Number");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("IdPlusFive", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customer] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Rename_column_on_temporal_table_with_computed_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<int?>("IdPlusFive").HasComputedColumnSql("Id + 5 PERSISTED");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Number");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("RenamedNumber");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("IdPlusFive", c.Name),
                    c => Assert.Equal("RenamedNumber", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customer].[Number]', N'RenamedNumber', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Add_sparse_column_to_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("MyColumn").IsSparse();
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("MyColumn", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
IF EXISTS (SELECT 1 FROM [sys].[tables] [t] INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] WHERE [t].[name] = 'HistoryTable' AND data_compression <> 0)
EXEC(N'ALTER TABLE [HistoryTable] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE);');
""",
            //
            """
ALTER TABLE [Customer] ADD [MyColumn] int SPARSE NULL;
""",
            //
            """
ALTER TABLE [HistoryTable] ADD [MyColumn] int SPARSE NULL;
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Add_sparse_column_to_temporal_table_with_custom_schemas()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        "Customers", "mySchema",
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable", "myHistorySchema");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("MyColumn").IsSparse();
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal("mySchema", table.Schema);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("myHistorySchema", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("MyColumn", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
IF EXISTS (SELECT 1 FROM [sys].[tables] [t] INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] WHERE [t].[name] = 'HistoryTable' AND [t].[schema_id] = schema_id('myHistorySchema') AND data_compression <> 0)
EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE);');
""",
            //
            """
ALTER TABLE [mySchema].[Customers] ADD [MyColumn] int SPARSE NULL;
""",
            //
            """
ALTER TABLE [myHistorySchema].[HistoryTable] ADD [MyColumn] int SPARSE NULL;
""",
            //
            """
ALTER TABLE [mySchema].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myHistorySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_regular_column_of_temporal_table_to_sparse()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                    e.HasData(
                        new { MyColumn = 1 },
                        new { MyColumn = 2 },
                        new { MyColumn = (int?)null },
                        new { MyColumn = (int?)null });
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("MyColumn");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("MyColumn").IsSparse();
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("MyColumn", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
IF EXISTS (SELECT 1 FROM [sys].[tables] [t] INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] WHERE [t].[name] = 'HistoryTable' AND data_compression <> 0)
EXEC(N'ALTER TABLE [HistoryTable] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE);');
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'MyColumn');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customer] ALTER COLUMN [MyColumn] int SPARSE NULL;
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'MyColumn');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] ALTER COLUMN [MyColumn] int SPARSE NULL;
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_sparse_column_of_temporal_table_to_regular()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                    e.HasData(
                        new { MyColumn = 1 },
                        new { MyColumn = 2 },
                        new { MyColumn = (int?)null },
                        new { MyColumn = (int?)null });
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("MyColumn").IsSparse();
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int?>("MyColumn");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("MyColumn", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'MyColumn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customer] ALTER COLUMN [MyColumn] int NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_regular_table_with_sparse_column_to_temporal()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<int?>("MyColumn").IsSparse();
                    e.HasData(
                        new { MyColumn = 1 },
                        new { MyColumn = 2 },
                        new { MyColumn = (int?)null },
                        new { MyColumn = (int?)null });
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.ToTable(
                        "Customers",
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.NotNull(table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("MyColumn", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Create_temporal_table_with_comments()
    {
        await Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name").HasComment("Column comment");
                    e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                })
                            .HasComment("Table comment"));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.NotNull(table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + N'.[CustomerHistory]))');
DECLARE @defaultSchema1 AS sysname;
SET @defaultSchema1 = SCHEMA_NAME();
DECLARE @description1 AS sql_variant;
SET @description1 = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'Customer';
SET @description1 = N'Column comment';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'Customer', 'COLUMN', N'Name';
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_normal_table_to_temporal_while_also_adding_comments_and_index()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name").HasComment("Column comment");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.HasIndex("Name");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customer] ALTER COLUMN [Name] nvarchar(450) NULL;
DECLARE @defaultSchema2 AS sysname;
SET @defaultSchema2 = SCHEMA_NAME();
DECLARE @description2 AS sql_variant;
SET @description2 = N'Column comment';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'Customer', 'COLUMN', N'Name';
""",
            //
            """
ALTER TABLE [Customer] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customer] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
CREATE INDEX [IX_Customer_Name] ON [Customer] ([Name]);
""",
            //
            """
ALTER TABLE [Customer] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customer] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public async Task Alter_comments_for_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("SystemTimeStart");
                                ttb.HasPeriodEnd("SystemTimeEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name").HasComment("Column comment");
                    e.ToTable(tb => tb.HasComment("Table comment"));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name").HasComment("Modified column comment");
                    e.ToTable(tb => tb.HasComment("Modified table comment"));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customer", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.NotNull(table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal("SystemTimeStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("SystemTimeEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @defaultSchema2 AS sysname;
SET @defaultSchema2 = SCHEMA_NAME();
DECLARE @description2 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema2, 'TABLE', N'Customer';
SET @description2 = N'Modified table comment';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'Customer';
""",
            //
            """
DECLARE @defaultSchema3 AS sysname;
SET @defaultSchema3 = SCHEMA_NAME();
DECLARE @description3 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema3, 'TABLE', N'Customer', 'COLUMN', N'Name';
SET @description3 = N'Modified column comment';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'Customer', 'COLUMN', N'Name';
""");
    }

    [ConditionalFact]
    public virtual async Task Add_index_to_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.HasIndex("Name");
                    e.HasIndex("Number").IsUnique();
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);
                Assert.Equal(2, table.Indexes.Count);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
CREATE INDEX [IX_Customers_Name] ON [Customers] ([Name]);
""",
            //
            """
CREATE UNIQUE INDEX [IX_Customers_Number] ON [Customers] ([Number]);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_index_on_period_column_to_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.HasIndex("Start");
                    e.HasIndex("End", "Name");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                // TODO: issue #26008 - we don't reverse engineer indexes on period columns since the columns are not added to the database model
                //Assert.Equal(2, table.Indexes.Count);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
CREATE INDEX [IX_Customers_End_Name] ON [Customers] ([End], [Name]);
""",
            //
            """
CREATE INDEX [IX_Customers_Start] ON [Customers] ([Start]);
""");
    }

    [ConditionalFact]
    public virtual async Task History_table_schema_created_when_necessary()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("MyHistoryTable", "mySchema2");
                                }));
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("mySchema", table.Schema);
                Assert.Equal("mySchema2", table[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema') IS NULL EXEC(N'CREATE SCHEMA [mySchema];');
""",
            //
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
CREATE TABLE [mySchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[MyHistoryTable]));
""");
    }

    [ConditionalFact]
    public virtual async Task History_table_schema_not_created_if_we_know_it_already_exists1()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });

                builder.Entity(
                    "Order", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Orders", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    });
            },
            model =>
            {
                Assert.Equal(2, model.Tables.Count);
                Assert.True(model.Tables.All(x => x.Schema == "mySchema"));
                Assert.True(model.Tables.All(x => x[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string == "mySchema"));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema') IS NULL EXEC(N'CREATE SCHEMA [mySchema];');
""",
            //
            """
CREATE TABLE [mySchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema].[CustomersHistory]));
""",
            //
            """
CREATE TABLE [mySchema].[Orders] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema].[OrdersHistory]));
""");
    }

    [ConditionalFact]
    public virtual async Task History_table_schema_not_created_if_we_know_it_already_exists2()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("CustomersHistoryTable", "mySchema2");
                                }));
                    });

                builder.Entity(
                    "Order", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Orders", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("OrdersHistoryTable", "mySchema2");
                                }));
                    });
            },
            model =>
            {
                Assert.Equal(2, model.Tables.Count);
                Assert.True(model.Tables.All(x => x.Schema == "mySchema"));
                Assert.True(model.Tables.All(x => x[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string == "mySchema2"));
            });

        AssertSql(
            """
IF SCHEMA_ID(N'mySchema') IS NULL EXEC(N'CREATE SCHEMA [mySchema];');
""",
            //
            """
IF SCHEMA_ID(N'mySchema2') IS NULL EXEC(N'CREATE SCHEMA [mySchema2];');
""",
            //
            """
CREATE TABLE [mySchema].[Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[CustomersHistoryTable]));
""",
            //
            """
CREATE TABLE [mySchema].[Orders] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [mySchema2].[OrdersHistoryTable]));
""");
    }

    [ConditionalFact]
    public virtual async Task History_table_schema_renamed_to_one_exisiting_in_the_model()
    {
        await Test(
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("CustomersHistoryTable", "mySchema2");
                                }));
                    });

                builder.Entity(
                    "Order", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Orders", "mySchema2", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("OrdersHistoryTable", "mySchema2");
                                }));
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", "mySchema", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("CustomersHistoryTable", "mySchema2");
                                }));
                    });

                builder.Entity(
                    "Order", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Orders", "mySchema2", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                    ttb.UseHistoryTable("OrdersHistoryTable", "mySchema");
                                }));
                    });
            },
            model =>
            {
                Assert.Equal(2, model.Tables.Count);
                var customers = model.Tables.First(t => t.Name == "Customers");
                Assert.Equal("mySchema", customers.Schema);
                Assert.Equal("mySchema2", customers[SqlServerAnnotationNames.TemporalHistoryTableSchema]);

                var orders = model.Tables.First(t => t.Name == "Orders");
                Assert.Equal("mySchema2", orders.Schema);
                Assert.Equal("mySchema", orders[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
            });

        // TODO: we could avoid creating the schema if we peek into the model
        AssertSql(
            """
IF SCHEMA_ID(N'mySchema') IS NULL EXEC(N'CREATE SCHEMA [mySchema];');
""",
            //
            """
ALTER SCHEMA [mySchema] TRANSFER [mySchema2].[OrdersHistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_with_default_global_schema_noop_migtation_doesnt_generate_unnecessary_steps()
    {
        await Test(
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal());
                    });
            },
            builder =>
            {
            },
            builder =>
            {
            },
            model =>
            {
                Assert.Equal(1, model.Tables.Count);
                var customers = model.Tables.First(t => t.Name == "Customers");
                Assert.Equal("myDefaultSchema", customers.Schema);
                Assert.Equal("myDefaultSchema", customers[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
            });

        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_with_default_global_schema_changing_global_schema()
    {
        await Test(
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal());
                    });
            },
            builder =>
            {
                builder.HasDefaultSchema("myDefaultSchema");
            },
            builder =>
            {
                builder.HasDefaultSchema("myModifiedDefaultSchema");
            },
            model =>
            {
                Assert.Equal(1, model.Tables.Count);
                var customers = model.Tables.First(t => t.Name == "Customers");
                Assert.Equal("myModifiedDefaultSchema", customers.Schema);
                Assert.Equal("myModifiedDefaultSchema", customers[SqlServerAnnotationNames.TemporalHistoryTableSchema]);
            });

        AssertSql(
            """
IF SCHEMA_ID(N'myModifiedDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [myModifiedDefaultSchema];');
""",
            //
            """
ALTER TABLE [myDefaultSchema].[Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER SCHEMA [myModifiedDefaultSchema] TRANSFER [myDefaultSchema].[Customers];
""",
            //
            """
ALTER SCHEMA [myModifiedDefaultSchema] TRANSFER [myDefaultSchema].[CustomersHistory];
""",
            //
            """
ALTER TABLE [myModifiedDefaultSchema].[Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [myModifiedDefaultSchema].[CustomersHistory]))
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_rename_and_delete_columns_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.Property<DateTime>("Dob");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("FullName");
                    e.Property<DateTime>("DateOfBirth");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("DateOfBirth", c.Name),
                    c => Assert.Equal("FullName", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
EXEC sp_rename N'[Customers].[Name]', N'FullName', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable].[Name]', N'FullName', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[Customers].[Dob]', N'DateOfBirth', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable].[Dob]', N'DateOfBirth', 'COLUMN';
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_rename_and_delete_columns_and_also_rename_table_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");

                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("FullName");

                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "ModifiedCustomers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("ModifiedCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("FullName", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'ModifiedCustomers', 'OBJECT';
""",
            //
            """
EXEC sp_rename N'[ModifiedCustomers].[Name]', N'FullName', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable].[Name]', N'FullName', 'COLUMN';
""",
            //
            """
ALTER TABLE [ModifiedCustomers] ADD CONSTRAINT [PK_ModifiedCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [ModifiedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_rename_and_delete_columns_and_also_rename_history_table_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");

                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("FullName");

                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("ModifiedHistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("ModifiedHistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("FullName", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
EXEC sp_rename N'[Customers].[Name]', N'FullName', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable].[Name]', N'FullName', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable]', N'ModifiedHistoryTable', 'OBJECT';
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[ModifiedHistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_delete_column_and_add_another_column_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<DateTime>("DateOfBirth");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("DateOfBirth", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
ALTER TABLE [Customers] ADD [DateOfBirth] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [HistoryTable] ADD [DateOfBirth] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_delete_column_and_alter_another_column_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");

                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.Property<DateTime>("DateOfBirth");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<string>("Name").HasComment("My comment");
                    e.Property<DateTime>("DateOfBirth");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("DateOfBirth", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
DECLARE @defaultSchema4 AS sysname;
SET @defaultSchema4 = SCHEMA_NAME();
DECLARE @description4 AS sql_variant;
SET @description4 = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'Customers', 'COLUMN', N'Name';
""",
            //
            """
DECLARE @defaultSchema5 AS sysname;
SET @defaultSchema5 = SCHEMA_NAME();
DECLARE @description5 AS sql_variant;
SET @description5 = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'HistoryTable', 'COLUMN', N'Name';
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_rename_and_alter_period_column_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").HasComment("My comment").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start").HasColumnName("ModifiedStart");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("ModifiedStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customers].[Start]', N'ModifiedStart', 'COLUMN';
""",
            //
            """
DECLARE @defaultSchema1 AS sysname;
SET @defaultSchema1 = SCHEMA_NAME();
DECLARE @description1 AS sql_variant;
SET @description1 = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'Customers', 'COLUMN', N'End';
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_table_delete_column_rename_and_alter_period_column_in_one_migration()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<DateTime>("DateOfBirth");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").HasComment("My comment").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start").HasColumnName("ModifiedStart");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("ModifiedStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'DateOfBirth');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [DateOfBirth];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'DateOfBirth');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [DateOfBirth];
""",
            //
            """
EXEC sp_rename N'[Customers].[Start]', N'ModifiedStart', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[HistoryTable].[Start]', N'ModifiedStart', 'COLUMN';
""",
            //
            """
DECLARE @defaultSchema4 AS sysname;
SET @defaultSchema4 = SCHEMA_NAME();
DECLARE @description4 AS sql_variant;
SET @description4 = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'Customers', 'COLUMN', N'End';
""",
            //
            """
DECLARE @defaultSchema5 AS sysname;
SET @defaultSchema5 = SCHEMA_NAME();
DECLARE @description5 AS sql_variant;
SET @description5 = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'HistoryTable', 'COLUMN', N'End';
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Convert_from_temporal_table_with_minimal_configuration_to_explicit_one_noop()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable("Customers", tb => tb.IsTemporal());
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("PeriodStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("PeriodEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task Convert_from_temporal_table_with_explicit_configuration_to_minimal_one_noop()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable("Customers", tb => tb.IsTemporal());
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("PeriodStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("PeriodEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task Convert_from_temporal_table_with_minimal_configuration_to_explicit_one()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable("Customers", tb => tb.IsTemporal());
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customers].[PeriodStart]', N'Start', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[Customers].[PeriodEnd]', N'End', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[CustomersHistory]', N'HistoryTable', 'OBJECT';
""");
    }

    [ConditionalFact]
    public virtual async Task Change_names_of_period_columns_in_temporal_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<DateTime>("ValidFrom").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("ValidTo").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("ValidFrom");
                                ttb.HasPeriodEnd("ValidTo");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("ValidFrom", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("ValidTo", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customers].[PeriodStart]', N'ValidFrom', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[Customers].[PeriodEnd]', N'ValidTo', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_to_temporal_and_add_new_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customers] ADD [Number] int NOT NULL DEFAULT 0;
""",
            //
            """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_to_temporal_and_remove_existing_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_to_temporal_and_rename_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("NewNumber");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("NewNumber", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
EXEC sp_rename N'[Customers].[Number]', N'NewNumber', 'COLUMN';
""",
            //
            """
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_from_temporal_and_add_new_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Start];
""",
            //
            """
DROP TABLE [HistoryTable];
""",
            //
            """
ALTER TABLE [Customers] ADD [Number] int NOT NULL DEFAULT 0;
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_from_temporal_and_remove_existing_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Customers] DROP COLUMN [Start];
""",
            //
            """
DROP TABLE [HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_from_temporal_and_rename_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("NewNumber");
                    e.ToTable("Customers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("NewNumber", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Start];
""",
            //
            """
EXEC sp_rename N'[Customers].[Number]', N'NewNumber', 'COLUMN';
""",
            //
            """
DROP TABLE [HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_to_temporal_rename_table_and_add_new_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable(
                        "NewCustomers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("NewCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [Number] int NOT NULL DEFAULT 0;
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
ALTER TABLE [NewCustomers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [NewCustomers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [NewCustomers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_to_temporal_rename_table_and_remove_existing_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "NewCustomers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("NewCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
ALTER TABLE [NewCustomers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [NewCustomers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [NewCustomers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_to_temporal_rename_table_and_rename_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("NewNumber");
                    e.ToTable(
                        "NewCustomers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("NewCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("NewNumber", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
            //
            """
EXEC sp_rename N'[NewCustomers].[Number]', N'NewNumber', 'COLUMN';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
ALTER TABLE [NewCustomers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
            //
            """
ALTER TABLE [NewCustomers] ALTER COLUMN [Start] ADD HIDDEN
""",
            //
            """
ALTER TABLE [NewCustomers] ALTER COLUMN [End] ADD HIDDEN
""",
            //
            """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_from_temporal_rename_table_and_add_new_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("NewCustomers");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("NewCustomers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Start];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
            //
            """
DROP TABLE [HistoryTable];
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [Number] int NOT NULL DEFAULT 0;
""",
            //
            """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_rename_table_rename_history_table_and_add_new_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable(
                        "NewCustomers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("NewHistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("NewCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("NewHistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
            //
            """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
            //
            """
EXEC sp_rename N'[HistoryTable]', N'NewHistoryTable', 'OBJECT';
""",
            //
            """
ALTER TABLE [NewCustomers] ADD [Number] int NOT NULL DEFAULT 0;
""",
            //
            """
ALTER TABLE [NewHistoryTable] ADD [Number] int NOT NULL DEFAULT 0;
""",
            //
            """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[NewHistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_from_temporal_create_another_table_with_same_name_as_history_table()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            builder =>
            {
                builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.Property<int>("Number");
                        e.ToTable("Customers");
                    });

                builder.Entity(
                    "History", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.Property<int>("Number");
                        e.ToTable("HistoryTable");
                    });
            },
            model =>
            {
                var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                var historyTable = Assert.Single(model.Tables, t => t.Name == "HistoryTable");

                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));

                Assert.Collection(
                    historyTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    historyTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(historyTable.PrimaryKey!.Columns));
            });

        AssertSql(
            """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
            //
            """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
            //
            """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
            //
            """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [Start];
""",
            //
            """
DROP TABLE [HistoryTable];
""",
            //
            """
CREATE TABLE [HistoryTable] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Number] int NOT NULL,
    CONSTRAINT [PK_HistoryTable] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_convert_regular_table_to_temporal_and_add_rowversion_column()
    {
        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("Start").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("End").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.Property<byte[]>("MyRowVersion").IsRowVersion();
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb.HasPeriodStart("Start");
                                ttb.HasPeriodEnd("End");
                            }));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("Start", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("End", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("HistoryTable", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name),
                    c => Assert.Equal("MyRowVersion", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
ALTER TABLE [Customers] ADD [End] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [MyRowVersion] rowversion NULL;
""",
                //
                """
ALTER TABLE [Customers] ADD [Start] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([Start], [End])
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [Start] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [End] ADD HIDDEN
""",
                //
                """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_create_temporal_table_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
                    .Annotation("SqlServer:IsTemporal", true)
                    .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
                    .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                    .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                    .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                    .Annotation("SqlServer:IsTemporal", true)
                    .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
                    .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                    .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                    .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:IsTemporal", true)
                    .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
                    .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                    .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                    .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:IsTemporal", true)
                    .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
                    .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                    .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                    .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => { },
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("PeriodStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("PeriodEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + N'.[CustomersHistory]))');
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_convert_regular_table_to_temporal_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.AlterTable(
            name: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Customers",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .Annotation("SqlServer:Identity", "1, 1")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:Identity", "1, 1");

        migrationBuilder.AddColumn<DateTime>(
            name: "PeriodEnd",
            table: "Customers",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AddColumn<DateTime>(
            name: "PeriodStart",
            table: "Customers",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Number");
                    e.ToTable("Customers");
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("PeriodStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("PeriodEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
ALTER TABLE [Customers] ADD [PeriodEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [PeriodStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [PeriodStart] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [PeriodEnd] ADD HIDDEN
""",
                //
                """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[CustomersHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_convert_regular_table_with_rowversion_to_temporal_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.AlterTable(
            name: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<byte[]>(
            name: "MyRowVersion",
            table: "Customers",
            type: "rowversion",
            rowVersion: true,
            nullable: false,
            oldClrType: typeof(byte[]),
            oldType: "rowversion",
            oldRowVersion: true)
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Customers",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .Annotation("SqlServer:Identity", "1, 1")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:Identity", "1, 1");

        migrationBuilder.AddColumn<DateTime>(
            name: "PeriodEnd",
            table: "Customers",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AddColumn<DateTime>(
            name: "PeriodStart",
            table: "Customers",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<byte[]>("MyRowVersion").IsRowVersion();
                    e.ToTable("Customers");
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("PeriodStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("PeriodEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("CustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("MyRowVersion", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
ALTER TABLE [Customers] ADD [PeriodEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [PeriodStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [PeriodStart] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [PeriodEnd] ADD HIDDEN
""",
                //
                """
DECLARE @historyTableSchema nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema + '.[CustomersHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_rename_temporal_table_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Customers",
            table: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.RenameTable(
            name: "Customers",
            newName: "RenamedCustomers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null);

        migrationBuilder.AlterTable(
            name: "RenamedCustomers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "RenamedCustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<DateTime>(
            name: "PeriodStart",
            table: "RenamedCustomers",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "RenamedCustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<DateTime>(
            name: "PeriodEnd",
            table: "RenamedCustomers",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "RenamedCustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "RenamedCustomers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "RenamedCustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "RenamedCustomers",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .Annotation("SqlServer:Identity", "1, 1")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "RenamedCustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
            .OldAnnotation("SqlServer:Identity", "1, 1")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AddPrimaryKey(
            name: "PK_RenamedCustomers",
            table: "RenamedCustomers",
            column: "Id");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "RenamedCustomers");
                Assert.Equal("RenamedCustomers", table.Name);
                Assert.Equal(true, table[SqlServerAnnotationNames.IsTemporal]);
                Assert.Equal("PeriodStart", table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName]);
                Assert.Equal("PeriodEnd", table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName]);
                Assert.Equal("RenamedCustomersHistory", table[SqlServerAnnotationNames.TemporalHistoryTableName]);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
                //
                """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
""",
                //
                """
EXEC sp_rename N'[CustomersHistory]', N'RenamedCustomersHistory', 'OBJECT';
""",
                //
                """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
                //
                """
DECLARE @historyTableSchema1 nvarchar(max) = QUOTENAME(SCHEMA_NAME())
EXEC(N'ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @historyTableSchema1 + '.[RenamedCustomersHistory]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_convert_temporal_table_to_regular_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.DropColumn(
            name: "PeriodEnd",
            table: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropColumn(
            name: "PeriodStart",
            table: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterTable(
            name: "Customers")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Customers",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .Annotation("SqlServer:Identity", "1, 1")
            .OldAnnotation("SqlServer:Identity", "1, 1")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'PeriodEnd');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [PeriodEnd];
""",
                //
                """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'PeriodStart');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [PeriodStart];
""",
                //
                """
DROP TABLE [CustomersHistory];
""",
                //
                """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Customers] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
""",
                //
                """
DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Id');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Customers] ALTER COLUMN [Id] int NOT NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_add_column_to_temporal_table_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.AddColumn<byte[]>(
            name: "MyRowVersion",
            table: "Customers",
            type: "rowversion",
            rowVersion: true,
            nullable: false,
            defaultValue: new byte[0])
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("MyRowVersion", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
ALTER TABLE [Customers] ADD [MyRowVersion] rowversion NOT NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_remove_temporal_table_column_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.DropColumn(
            name: "IsVip",
            table: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<bool>("IsVip");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'IsVip');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [IsVip];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_rename_temporal_table_column_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.RenameColumn(
            name: "Name",
            table: "Customers",
            newName: "FullName")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("FullName", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
EXEC sp_rename N'[Customers].[Name]', N'FullName', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_rename_temporal_table_period_columns_using_EF8_migration_code()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        migrationBuilder.RenameColumn(
            name: "PeriodStart",
            table: "Customers",
            newName: "NewPeriodStart")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.RenameColumn(
            name: "PeriodEnd",
            table: "Customers",
            newName: "NewPeriodEnd")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterTable(
            name: "Customers")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "NewPeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "NewPeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "NewPeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "NewPeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Customers",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .Annotation("SqlServer:Identity", "1, 1")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "NewPeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "NewPeriodStart")
            .OldAnnotation("SqlServer:Identity", "1, 1")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<DateTime>(
            name: "NewPeriodStart",
            table: "Customers",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "NewPeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "NewPeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.AlterColumn<DateTime>(
            name: "NewPeriodEnd",
            table: "Customers",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "NewPeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "NewPeriodStart")
            .OldAnnotation("SqlServer:IsTemporal", true)
            .OldAnnotation("SqlServer:TemporalHistoryTableName", "CustomersHistory")
            .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
            .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        await Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<DateTime>("PeriodStart").ValueGeneratedOnAddOrUpdate();
                    e.Property<DateTime>("PeriodEnd").ValueGeneratedOnAddOrUpdate();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(
                        "Customers", tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.UseHistoryTable("CustomersHistory");
                                ttb.HasPeriodStart("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd");
                            }));
                }),
            migrationBuilder.Operations,
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Equal("Customers", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

        AssertSql(
"""
EXEC sp_rename N'[Customers].[PeriodStart]', N'NewPeriodStart', 'COLUMN';
""",
                //
                """
EXEC sp_rename N'[Customers].[PeriodEnd]', N'NewPeriodEnd', 'COLUMN';
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_drop_temporal_table_and_add_the_same_table_in_one_migration()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    })
                ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
IF SCHEMA_ID(N'historySchema') IS NULL EXEC(N'CREATE SCHEMA [historySchema];');
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]));
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_rename_period_column_twice()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("NewName");
                        e.Property<DateTime>("NewSystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("NewSystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("NewName");
                        e.Property<DateTime>("FinalSystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("FinalSystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                ]);

        AssertSql(
"""
EXEC sp_rename N'[Customers].[Name]', N'NewName', 'COLUMN';
""",
                //
                """
EXEC sp_rename N'[Customers].[SystemTimeStart]', N'NewSystemTimeStart', 'COLUMN';
""",
                //
                """
EXEC sp_rename N'[Customers].[NewSystemTimeStart]', N'FinalSystemTimeStart', 'COLUMN';
""");
    }

    [ConditionalFact(Skip = "Issue #36161")]
    public virtual async Task Temporal_multiop_create_regular_convert_to_temporal_rename_table_drop_column()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<long>("Number");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),

                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<long>("Number");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),

                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<long>("Number");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "RenamedCustomers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),


                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "RenamedCustomers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                ]);

        AssertSql(
"""
ALTER TABLE [Customers] ADD [SystemTimeEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [SystemTimeStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
                //
                """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
""",
                //
                """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RenamedCustomers]') AND [c].[name] = N'Number');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [RenamedCustomers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [RenamedCustomers] DROP COLUMN [Number];
""",
                //
                """
ALTER TABLE [RenamedCustomers] ADD PERIOD FOR SYSTEM_TIME ([SystemTimeStart], [SystemTimeEnd])
""",
                //
                """
ALTER TABLE [RenamedCustomers] ALTER COLUMN [SystemTimeStart] ADD HIDDEN
""",
                //
                """
ALTER TABLE [RenamedCustomers] ALTER COLUMN [SystemTimeEnd] ADD HIDDEN
""",
                //
                """
IF SCHEMA_ID(N'historySchema') IS NULL EXEC(N'CREATE SCHEMA [historySchema];');
""",
                //
                """
ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_drop_temporal_table_and_add_slightly_different_table_with_the_same_name_in_one_migration()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("NewSystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("NewSystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("NewHistoryTable", "newHistorySchema");
                                    ttb.HasPeriodStart("NewSystemTimeStart");
                                    ttb.HasPeriodEnd("NewSystemTimeEnd");
                                }));
                    }),
                builder => { },
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("FinalSystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("FinalSystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("FinalHistoryTable", "finalHistorySchema");
                                    ttb.HasPeriodStart("FinalSystemTimeStart");
                                    ttb.HasPeriodEnd("FinalSystemTimeEnd");
                                }));
                    }),
                ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
IF SCHEMA_ID(N'newHistorySchema') IS NULL EXEC(N'CREATE SCHEMA [newHistorySchema];');
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [NewSystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [NewSystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([NewSystemTimeStart], [NewSystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [newHistorySchema].[NewHistoryTable]));
""",
                //
                """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [newHistorySchema].[NewHistoryTable];
""",
                //
                """
IF SCHEMA_ID(N'finalHistorySchema') IS NULL EXEC(N'CREATE SCHEMA [finalHistorySchema];');
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [FinalSystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [FinalSystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([FinalSystemTimeStart], [FinalSystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [finalHistorySchema].[FinalHistoryTable]));
""");
    }

    [ConditionalFact(Skip = "Issue #36161")]
    public virtual async Task Temporal_multiop_drop_temporal_create_normal_add_column_rename_convert_to_temporal_drop_create_again_as_temporal_convert_to_normal_edit_drop()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                // drop temporal
                builder => { },

                // create normal
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),

                // add column
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<string>("Extra");
                        e.HasKey("Id");
                        e.ToTable("Customers");
                    }),
    
                // rename
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<string>("Extra");
                        e.HasKey("Id");
                        e.ToTable("RenamedCustomers");
                    }),

                // convert to temporal
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<string>("Extra");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "RenamedCustomers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),

                // drop again
                builder => { },

                // create again as temporal
                builder => builder.Entity(
                    "BrandNewCustomer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<string>("Extra");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "BrandNewCustomers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),

                // convert to normal
                builder => builder.Entity(
                    "BrandNewCustomer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<string>("Extra");
                        e.HasKey("Id");
                        e.ToTable("BrandNewCustomers");
                    }),

                // remove column
                builder => builder.Entity(
                    "BrandNewCustomer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");
                        e.ToTable("BrandNewCustomers");
                    }),

                // drop
                builder => { }
            ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
""",
                //
                """
ALTER TABLE [Customers] ADD [Extra] nvarchar(max) NULL;
""",
                //
                """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
                //
                """
EXEC sp_rename N'[Customers]', N'RenamedCustomers', 'OBJECT';
""",
                //
                """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
                //
                """
ALTER TABLE [RenamedCustomers] ADD [SystemTimeEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [RenamedCustomers] ADD [SystemTimeStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
DROP TABLE [RenamedCustomers];
""",
                //
                """
IF SCHEMA_ID(N'historySchema') IS NULL EXEC(N'CREATE SCHEMA [historySchema];');
""",
                //
                """
CREATE TABLE [BrandNewCustomers] (
    [Id] int NOT NULL IDENTITY,
    [Extra] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_BrandNewCustomers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]));
""",
                //
                """
ALTER TABLE [BrandNewCustomers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
ALTER TABLE [BrandNewCustomers] DROP PERIOD FOR SYSTEM_TIME
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BrandNewCustomers]') AND [c].[name] = N'SystemTimeEnd');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [BrandNewCustomers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [BrandNewCustomers] DROP COLUMN [SystemTimeEnd];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BrandNewCustomers]') AND [c].[name] = N'SystemTimeStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [BrandNewCustomers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [BrandNewCustomers] DROP COLUMN [SystemTimeStart];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BrandNewCustomers]') AND [c].[name] = N'Extra');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [BrandNewCustomers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [BrandNewCustomers] DROP COLUMN [Extra];
""",
                //
                """
DROP TABLE [BrandNewCustomers];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_many_renames()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "CustomersOne", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "CustomersTwo", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "CustomersThree", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("CustomersFour");
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("CustomersFive");
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("CustomersSix");
                    }),
            ]);

        AssertSql(
"""
ALTER TABLE [CustomersOne] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
ALTER TABLE [CustomersOne] DROP CONSTRAINT [PK_CustomersOne];
""",
                //
                """
EXEC sp_rename N'[CustomersOne]', N'CustomersTwo', 'OBJECT';
""",
                //
                """
ALTER TABLE [CustomersTwo] ADD CONSTRAINT [PK_CustomersTwo] PRIMARY KEY ([Id]);
""",
                //
                """
ALTER TABLE [CustomersTwo] DROP CONSTRAINT [PK_CustomersTwo];
""",
                //
                """
EXEC sp_rename N'[CustomersTwo]', N'CustomersThree', 'OBJECT';
""",
                //
                """
ALTER TABLE [CustomersThree] ADD CONSTRAINT [PK_CustomersThree] PRIMARY KEY ([Id]);
""",
                //
                """
ALTER TABLE [CustomersThree] DROP CONSTRAINT [PK_CustomersThree];
""",
                //
                """
ALTER TABLE [CustomersThree] DROP PERIOD FOR SYSTEM_TIME
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomersThree]') AND [c].[name] = N'SystemTimeEnd');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [CustomersThree] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [CustomersThree] DROP COLUMN [SystemTimeEnd];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomersThree]') AND [c].[name] = N'SystemTimeStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CustomersThree] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [CustomersThree] DROP COLUMN [SystemTimeStart];
""",
                //
                """
EXEC sp_rename N'[CustomersThree]', N'CustomersFour', 'OBJECT';
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
ALTER TABLE [CustomersFour] ADD CONSTRAINT [PK_CustomersFour] PRIMARY KEY ([Id]);
""",
                //
                """
ALTER TABLE [CustomersFour] DROP CONSTRAINT [PK_CustomersFour];
""",
                //
                """
EXEC sp_rename N'[CustomersFour]', N'CustomersFive', 'OBJECT';
""",
                //
                """
ALTER TABLE [CustomersFive] ADD CONSTRAINT [PK_CustomersFive] PRIMARY KEY ([Id]);
""",
                //
                """
ALTER TABLE [CustomersFive] DROP CONSTRAINT [PK_CustomersFive];
""",
                //
                """
EXEC sp_rename N'[CustomersFive]', N'CustomersSix', 'OBJECT';
""",
                //
                """
ALTER TABLE [CustomersSix] ADD CONSTRAINT [PK_CustomersSix] PRIMARY KEY ([Id]);
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_convert_temporal_to_regular_and_back()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
            ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'SystemTimeEnd');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Customers] DROP COLUMN [SystemTimeEnd];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'SystemTimeStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [SystemTimeStart];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
ALTER TABLE [Customers] ADD [SystemTimeEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [SystemTimeStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
ALTER TABLE [Customers] ADD PERIOD FOR SYSTEM_TIME ([SystemTimeStart], [SystemTimeEnd])
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [SystemTimeStart] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] ALTER COLUMN [SystemTimeEnd] ADD HIDDEN
""",
                //
                """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]))
""");
    }

    [ConditionalFact(Skip = "Issue #36161")]
    public virtual async Task Temporal_multiop_convert_regular_to_temporal_and_back()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
            ]);

        AssertSql(
"""
ALTER TABLE [Customers] ADD [SystemTimeEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [Customers] ADD [SystemTimeStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'SystemTimeEnd');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Customers] DROP COLUMN [SystemTimeEnd];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'SystemTimeStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Customers] DROP COLUMN [SystemTimeStart];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_add_sparse_column_to_temporal_table_then_remove_it()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<int?>("MyColumn").IsSparse();
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
            ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
IF EXISTS (SELECT 1 FROM [sys].[tables] [t] INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] WHERE [t].[name] = 'HistoryTable' AND [t].[schema_id] = schema_id('historySchema') AND data_compression <> 0)
EXEC(N'ALTER TABLE [historySchema].[HistoryTable] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE);');
""",
                //
                """
ALTER TABLE [Customers] ADD [MyColumn] int SPARSE NULL;
""",
                //
                """
ALTER TABLE [historySchema].[HistoryTable] ADD [MyColumn] int SPARSE NULL;
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'MyColumn');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Customers] DROP COLUMN [MyColumn];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[historySchema].[HistoryTable]') AND [c].[name] = N'MyColumn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [historySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [historySchema].[HistoryTable] DROP COLUMN [MyColumn];
""",
                //
                """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]))
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_change_column_nullability_rename_table_drop_table()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<int?>("MyColumn");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<int>("MyColumn");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<int>("MyColumn");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "NewCustomers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
            ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'MyColumn');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var + ';');
UPDATE [Customers] SET [MyColumn] = 0 WHERE [MyColumn] IS NULL;
ALTER TABLE [Customers] ALTER COLUMN [MyColumn] int NOT NULL;
ALTER TABLE [Customers] ADD DEFAULT 0 FOR [MyColumn];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[historySchema].[HistoryTable]') AND [c].[name] = N'MyColumn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [historySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
UPDATE [historySchema].[HistoryTable] SET [MyColumn] = 0 WHERE [MyColumn] IS NULL;
ALTER TABLE [historySchema].[HistoryTable] ALTER COLUMN [MyColumn] int NOT NULL;
ALTER TABLE [historySchema].[HistoryTable] ADD DEFAULT 0 FOR [MyColumn];
""",
                //
                """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
                //
                """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
                //
                """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""",
                //
                """
DROP TABLE [NewCustomers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_add_sparse_column_to_temporal_then_convert_to_regular()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<int?>("MyColumn").IsSparse();
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
            ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
IF EXISTS (SELECT 1 FROM [sys].[tables] [t] INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] WHERE [t].[name] = 'HistoryTable' AND [t].[schema_id] = schema_id('historySchema') AND data_compression <> 0)
EXEC(N'ALTER TABLE [historySchema].[HistoryTable] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE);');
""",
                //
                """
ALTER TABLE [Customers] ADD [MyColumn] int SPARSE NULL;
""",
                //
                """
ALTER TABLE [historySchema].[HistoryTable] ADD [MyColumn] int SPARSE NULL;
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'MyColumn');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Customers] DROP COLUMN [MyColumn];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[historySchema].[HistoryTable]') AND [c].[name] = N'MyColumn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [historySchema].[HistoryTable] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [historySchema].[HistoryTable] DROP COLUMN [MyColumn];
""",
                //
                """
ALTER TABLE [Customers] DROP PERIOD FOR SYSTEM_TIME
""",
                //
                """
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'SystemTimeEnd');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [SystemTimeEnd];
""",
                //
                """
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'SystemTimeStart');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Customers] DROP COLUMN [SystemTimeStart];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact(Skip = "Issue #36161")]
    public virtual async Task Temporal_multiop_add_column_to_temporal_table_with_default_schemas_change_default_schema_add_another_column()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder =>
                {
                    builder.HasDefaultSchema("myCustomSchema");
                    builder.Entity(
                        "Customer", e =>
                        {
                            e.Property<int>("Id").ValueGeneratedOnAdd();
                            e.Property<string>("Name");
                            e.Property<int>("MyColumn");
                            e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                            e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                            e.HasKey("Id");

                            e.ToTable(
                                "Customers", tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.UseHistoryTable("HistoryTable");
                                        ttb.HasPeriodStart("SystemTimeStart");
                                        ttb.HasPeriodEnd("SystemTimeEnd");
                                    }));
                        });
                },
                builder =>
                {
                    builder.HasDefaultSchema("myCustomSchema");
                    builder.Entity(
                        "Customer", e =>
                        {
                            e.Property<int>("Id").ValueGeneratedOnAdd();
                            e.Property<string>("Name");
                            e.Property<int>("MyColumn");
                            e.Property<int>("AnotherColumn");
                            e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                            e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                            e.HasKey("Id");

                            e.ToTable(
                                "Customers", tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.UseHistoryTable("HistoryTable");
                                        ttb.HasPeriodStart("SystemTimeStart");
                                        ttb.HasPeriodEnd("SystemTimeEnd");
                                    }));
                        });
                },
            ]);

        AssertSql("");
    }

    [ConditionalFact(Skip = "Issue #36161")]
    public virtual async Task Temporal_multiop_convert_regular_table_to_temporal_change_default_schema_convert_back_to_regular()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
                builder =>
                {
                    builder.HasDefaultSchema("modifiedDefaultSchema");
                    builder.Entity(
                        "Customer", e =>
                        {
                            e.Property<int>("Id").ValueGeneratedOnAdd();
                            e.Property<string>("Name");
                            e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                            e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                            e.HasKey("Id");

                            e.ToTable(
                                "Customers", tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.UseHistoryTable("HistoryTable");
                                        ttb.HasPeriodStart("SystemTimeStart");
                                        ttb.HasPeriodEnd("SystemTimeEnd");
                                    }));
                        });
                },
                builder =>
                {
                    builder.HasDefaultSchema("modifiedDefaultSchema");
                    builder.Entity(
                        "Customer", e =>
                        {
                            e.Property<int>("Id").ValueGeneratedOnAdd();
                            e.Property<string>("Name");
                            e.HasKey("Id");

                            e.ToTable("Customers");
                        });
                },
            ]);

        AssertSql(
"""
IF SCHEMA_ID(N'modifiedDefaultSchema') IS NULL EXEC(N'CREATE SCHEMA [modifiedDefaultSchema];');
""",
                //
                """
ALTER SCHEMA [modifiedDefaultSchema] TRANSFER [Customers];
""",
                //
                """
ALTER TABLE [modifiedDefaultSchema].[Customers] ADD [SystemTimeEnd] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
""",
                //
                """
ALTER TABLE [modifiedDefaultSchema].[Customers] ADD [SystemTimeStart] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
""",
                //
                """
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[modifiedDefaultSchema].[Customers]') AND [c].[name] = N'SystemTimeEnd');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [modifiedDefaultSchema].[Customers] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [modifiedDefaultSchema].[Customers] DROP COLUMN [SystemTimeEnd];
""",
                //
                """
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[modifiedDefaultSchema].[Customers]') AND [c].[name] = N'SystemTimeStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [modifiedDefaultSchema].[Customers] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [modifiedDefaultSchema].[Customers] DROP COLUMN [SystemTimeStart];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_create_temporal_and_drop()
    {
        await TestComposite(
            [
                builder => { },
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
                ]);

        AssertSql(
"""
IF SCHEMA_ID(N'historySchema') IS NULL EXEC(N'CREATE SCHEMA [historySchema];');
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [historySchema].[HistoryTable]));
""",
                //
                """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_rename_temporal_and_drop()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "NewCustomers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
                ]);

        AssertSql(
"""
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers];
""",
                //
                """
EXEC sp_rename N'[Customers]', N'NewCustomers', 'OBJECT';
""",
                //
                """
ALTER TABLE [NewCustomers] ADD CONSTRAINT [PK_NewCustomers] PRIMARY KEY ([Id]);
""",
                //
                """
DROP TABLE [NewCustomers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_rename_period_drop_table_create_as_regular()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("NewSystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("NewSystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
                ]);

        AssertSql(
"""
EXEC sp_rename N'[Customers].[SystemTimeStart]', N'NewSystemTimeStart', 'COLUMN';
""",
                //
                """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public virtual async Task Temporal_multiop_rename_column_drop_table_create_as_regular()
    {
        await TestComposite(
            [
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("NewName");
                        e.Property<DateTime>("SystemTimeStart").ValueGeneratedOnAddOrUpdate();
                        e.Property<DateTime>("SystemTimeEnd").ValueGeneratedOnAddOrUpdate();
                        e.HasKey("Id");

                        e.ToTable(
                            "Customers", tb => tb.IsTemporal(
                                ttb =>
                                {
                                    ttb.UseHistoryTable("HistoryTable", "historySchema");
                                    ttb.HasPeriodStart("SystemTimeStart");
                                    ttb.HasPeriodEnd("SystemTimeEnd");
                                }));
                    }),
                builder => { },
                builder => builder.Entity(
                    "Customer", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");

                        e.ToTable("Customers");
                    }),
                ]);

        AssertSql(
"""
EXEC sp_rename N'[Customers].[Name]', N'NewName', 'COLUMN';
""",
                //
                """
ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = OFF)
""",
                //
                """
DROP TABLE [Customers];
""",
                //
                """
DROP TABLE [historySchema].[HistoryTable];
""",
                //
                """
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
""");
    }
}

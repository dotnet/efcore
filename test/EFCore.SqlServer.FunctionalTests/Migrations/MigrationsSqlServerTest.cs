// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;

// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedParameter.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Microsoft.EntityFrameworkCore.Migrations;

public class MigrationsSqlServerTest : MigrationsTestBase<MigrationsSqlServerTest.MigrationsSqlServerFixture>
{
    public MigrationsSqlServerTest(MigrationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Create_table()
    {
        await base.Create_table();

        AssertSql(
            """
CREATE TABLE [People] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);
""");
    }

    public override async Task Create_table_all_settings()
    {
        await base.Create_table_all_settings();

        AssertSql(
            """
IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');
""",
            //
            """
CREATE TABLE [dbo2].[People] (
    [CustomId] int NOT NULL IDENTITY,
    [EmployerId] int NOT NULL,
    [SSN] nvarchar(11) COLLATE German_PhoneBook_CI_AS NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([CustomId]),
    CONSTRAINT [AK_People_SSN] UNIQUE ([SSN]),
    CONSTRAINT [CK_People_EmployerId] CHECK ([EmployerId] > 0),
    CONSTRAINT [FK_People_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([Id]) ON DELETE CASCADE
);
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'dbo2', 'TABLE', N'People';
SET @description = N'Employer ID comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'dbo2', 'TABLE', N'People', 'COLUMN', N'EmployerId';
""",
            //
            """
CREATE INDEX [IX_People_EmployerId] ON [dbo2].[People] ([EmployerId]);
""");
    }

    public override async Task Create_table_no_key()
    {
        await base.Create_table_no_key();

        AssertSql(
            """
CREATE TABLE [Anonymous] (
    [SomeColumn] int NOT NULL
);
""");
    }

    public override async Task Create_table_with_comments()
    {
        await base.Create_table_with_comments();

        AssertSql(
            """
CREATE TABLE [People] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';
SET @description = N'Column comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';
""");
    }

    public override async Task Create_table_with_multiline_comments()
    {
        await base.Create_table_with_multiline_comments();

        AssertSql(
            """
CREATE TABLE [People] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = CONCAT(N'This is a multi-line', NCHAR(13), NCHAR(10), N'table comment.', NCHAR(13), NCHAR(10), N'More information can', NCHAR(13), NCHAR(10), N'be found in the docs.');
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';
SET @description = CONCAT(N'This is a multi-line', NCHAR(10), N'column comment.', NCHAR(10), N'More information can', NCHAR(10), N'be found in the docs.');
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';
""");
    }

    public override async Task Create_table_with_computed_column(bool? stored)
    {
        await base.Create_table_with_computed_column(stored);

        var storedSql = stored == true ? " PERSISTED" : "";

        AssertSql(
            $"""
CREATE TABLE [People] (
    [Id] int NOT NULL IDENTITY,
    [Sum] AS [X] + [Y]{storedSql},
    [X] int NOT NULL,
    [Y] int NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);
""");
    }

    public override async Task Create_table_with_json_column()
    {
        await base.Create_table_with_json_column();

        AssertSql(
            """
CREATE TABLE [Entity] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [OwnedCollection] nvarchar(max) NULL,
    [OwnedReference] nvarchar(max) NULL,
    [OwnedRequiredReference] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Entity] PRIMARY KEY ([Id])
);
""");
    }

    public override async Task Create_table_with_json_column_explicit_json_column_names()
    {
        await base.Create_table_with_json_column_explicit_json_column_names();

        AssertSql(
            """
CREATE TABLE [Entity] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [json_collection] nvarchar(max) NULL,
    [json_reference] nvarchar(max) NULL,
    CONSTRAINT [PK_Entity] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_table_with_sparse_column()
    {
        await Test(
            _ => { },
            builder => builder.Entity("People", e => e.Property<string>("SomeProperty").IsSparse()),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "SomeProperty");
                Assert.True((bool?)column[SqlServerAnnotationNames.Sparse]);
            });

        AssertSql(
            """
CREATE TABLE [People] (
    [SomeProperty] nvarchar(max) SPARSE NULL
);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_table_with_identity_column_value_converter()
    {
        await Test(
            _ => { },
            builder => builder.UseIdentityColumns()
                .Entity("People").Property<int>("IdentityColumn").HasConversion<short>().ValueGeneratedOnAdd(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "IdentityColumn");
                Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
            });

        AssertSql(
            """
CREATE TABLE [People] (
    [IdentityColumn] smallint NOT NULL IDENTITY
);
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public virtual async Task Create_memory_optimized_table()
    {
        await Test(
            _ => { },
            builder => builder.UseIdentityColumns().Entity(
                "People", b =>
                {
                    b.ToTable(tb => tb.IsMemoryOptimized());
                    b.Property<int>("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.True((bool)table[SqlServerAnnotationNames.MemoryOptimized]!);
            });

        AssertSql(
            """
IF SERVERPROPERTY('IsXTPSupported') = 1 AND SERVERPROPERTY('EngineEdition') <> 5
    BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM [sys].[filegroups] [FG] JOIN [sys].[database_files] [F] ON [FG].[data_space_id] = [F].[data_space_id] WHERE [FG].[type] = N'FX' AND [F].[type] = 2)
        BEGIN
        ALTER DATABASE CURRENT SET AUTO_CLOSE OFF;
        DECLARE @db_name nvarchar(max) = DB_NAME();
        DECLARE @fg_name nvarchar(max);
        SELECT TOP(1) @fg_name = [name] FROM [sys].[filegroups] WHERE [type] = N'FX';

        IF @fg_name IS NULL
            BEGIN
            SET @fg_name = @db_name + N'_MODFG';
            EXEC(N'ALTER DATABASE CURRENT ADD FILEGROUP [' + @fg_name + '] CONTAINS MEMORY_OPTIMIZED_DATA;');
            END

        DECLARE @path nvarchar(max);
        SELECT TOP(1) @path = [physical_name] FROM [sys].[database_files] WHERE charindex('\', [physical_name]) > 0 ORDER BY [file_id];
        IF (@path IS NULL)
            SET @path = '\' + @db_name;

        DECLARE @filename nvarchar(max) = right(@path, charindex('\', reverse(@path)) - 1);
        SET @filename = REPLACE(left(@filename, len(@filename) - charindex('.', reverse(@filename))), '''', '''''') + N'_MOD';
        DECLARE @new_path nvarchar(max) = REPLACE(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(max)), '''', '''''') + @filename;

        EXEC(N'
            ALTER DATABASE CURRENT
            ADD FILE (NAME=''' + @filename + ''', filename=''' + @new_path + ''')
            TO FILEGROUP [' + @fg_name + '];')
        END
    END

IF SERVERPROPERTY('IsXTPSupported') = 1
EXEC(N'
    ALTER DATABASE CURRENT
    SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON;')
""",
            //
            """
CREATE TABLE [People] (
    [Id] int NOT NULL IDENTITY,
    CONSTRAINT [PK_People] PRIMARY KEY NONCLUSTERED ([Id])
) WITH (MEMORY_OPTIMIZED = ON);
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public virtual async Task Create_memory_optimized_temporal_table()
    {
        await Test(
            _ => { },
            builder => builder.UseIdentityColumns().Entity(
                "People", b =>
                {
                    b.ToTable("Customers", tb => tb.IsMemoryOptimized().IsTemporal());
                    b.Property<int>("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.True((bool)table[SqlServerAnnotationNames.MemoryOptimized]!);
            });

        AssertSql(
            """
IF SERVERPROPERTY('IsXTPSupported') = 1 AND SERVERPROPERTY('EngineEdition') <> 5
    BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM [sys].[filegroups] [FG] JOIN [sys].[database_files] [F] ON [FG].[data_space_id] = [F].[data_space_id] WHERE [FG].[type] = N'FX' AND [F].[type] = 2)
        BEGIN
        ALTER DATABASE CURRENT SET AUTO_CLOSE OFF;
        DECLARE @db_name nvarchar(max) = DB_NAME();
        DECLARE @fg_name nvarchar(max);
        SELECT TOP(1) @fg_name = [name] FROM [sys].[filegroups] WHERE [type] = N'FX';

        IF @fg_name IS NULL
            BEGIN
            SET @fg_name = @db_name + N'_MODFG';
            EXEC(N'ALTER DATABASE CURRENT ADD FILEGROUP [' + @fg_name + '] CONTAINS MEMORY_OPTIMIZED_DATA;');
            END

        DECLARE @path nvarchar(max);
        SELECT TOP(1) @path = [physical_name] FROM [sys].[database_files] WHERE charindex('\', [physical_name]) > 0 ORDER BY [file_id];
        IF (@path IS NULL)
            SET @path = '\' + @db_name;

        DECLARE @filename nvarchar(max) = right(@path, charindex('\', reverse(@path)) - 1);
        SET @filename = REPLACE(left(@filename, len(@filename) - charindex('.', reverse(@filename))), '''', '''''') + N'_MOD';
        DECLARE @new_path nvarchar(max) = REPLACE(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(max)), '''', '''''') + @filename;

        EXEC(N'
            ALTER DATABASE CURRENT
            ADD FILE (NAME=''' + @filename + ''', filename=''' + @new_path + ''')
            TO FILEGROUP [' + @fg_name + '];')
        END
    END

IF SERVERPROPERTY('IsXTPSupported') = 1
EXEC(N'
    ALTER DATABASE CURRENT
    SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON;')
""",
            //
            """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY NONCLUSTERED ([Id]),
    PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
) WITH (
    SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CustomersHistory]),
    MEMORY_OPTIMIZED = ON
)');
""");
    }

    public override async Task Drop_table()
    {
        await base.Drop_table();

        AssertSql(
            """
DROP TABLE [People];
""");
    }

    public override async Task Alter_table_add_comment()
    {
        await base.Alter_table_add_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';
""");
    }

    public override async Task Alter_table_add_comment_non_default_schema()
    {
        await base.Alter_table_add_comment_non_default_schema();

        AssertSql(
            """
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'SomeOtherSchema', 'TABLE', N'People';
""");
    }

    public override async Task Alter_table_change_comment()
    {
        await base.Alter_table_change_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People';
SET @description = N'Table comment2';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';
""");
    }

    public override async Task Alter_table_remove_comment()
    {
        await base.Alter_table_remove_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People';
""");
    }

    public override async Task Rename_table()
    {
        await base.Rename_table();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [PK_People];
""",
            //
            """
EXEC sp_rename N'[People]', N'Persons', 'OBJECT';
""",
            //
            """
ALTER TABLE [Persons] ADD CONSTRAINT [PK_Persons] PRIMARY KEY ([Id]);
""");
    }

    public override async Task Rename_table_with_primary_key()
    {
        await base.Rename_table_with_primary_key();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [PK_People];
""",
            //
            """
EXEC sp_rename N'[People]', N'Persons', 'OBJECT';
""",
            //
            """
ALTER TABLE [Persons] ADD CONSTRAINT [PK_Persons] PRIMARY KEY ([Id]);
""");
    }

    public override async Task Rename_table_with_json_column()
    {
        await base.Rename_table_with_json_column();

        AssertSql(
            """
ALTER TABLE [Entities] DROP CONSTRAINT [PK_Entities];
""",
            //
            """
EXEC sp_rename N'[Entities]', N'NewEntities', 'OBJECT';
""",
            //
            """
ALTER TABLE [NewEntities] ADD CONSTRAINT [PK_NewEntities] PRIMARY KEY ([Id]);
""");
    }

    public override async Task Move_table()
    {
        await base.Move_table();

        AssertSql(
            """
IF SCHEMA_ID(N'TestTableSchema') IS NULL EXEC(N'CREATE SCHEMA [TestTableSchema];');
""",
            //
            """
ALTER SCHEMA [TestTableSchema] TRANSFER [TestTable];
""");
    }

    [ConditionalFact]
    public virtual async Task Move_table_into_default_schema()
    {
        await Test(
            builder => builder.Entity("TestTable")
                .ToTable("TestTable", "TestTableSchema")
                .Property<int>("Id"),
            builder => builder.Entity("TestTable")
                .Property<int>("Id"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("dbo", table.Schema);
                Assert.Equal("TestTable", table.Name);
            });

        AssertSql(
            """
DECLARE @defaultSchema sysname = SCHEMA_NAME();
EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [TestTableSchema].[TestTable];');
""");
    }

    public override async Task Create_schema()
    {
        await base.Create_schema();

        AssertSql(
            """
IF SCHEMA_ID(N'SomeOtherSchema') IS NULL EXEC(N'CREATE SCHEMA [SomeOtherSchema];');
""",
            //
            """
CREATE TABLE [SomeOtherSchema].[People] (
    [Id] int NOT NULL IDENTITY,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_schema_dbo_is_ignored()
    {
        await Test(
            builder => { },
            builder => builder.Entity("People")
                .ToTable("People", "dbo")
                .Property<int>("Id"),
            model => Assert.Equal("dbo", Assert.Single(model.Tables).Schema));

        AssertSql(
            """
CREATE TABLE [dbo].[People] (
    [Id] int NOT NULL IDENTITY,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);
""");
    }

    public override async Task Add_column_with_defaultValue_string()
    {
        await base.Add_column_with_defaultValue_string();

        AssertSql(
            """
ALTER TABLE [People] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'John Doe';
""");
    }

    public override async Task Add_column_with_defaultValue_datetime()
    {
        await base.Add_column_with_defaultValue_datetime();

        AssertSql(
            """
ALTER TABLE [People] ADD [Birthday] datetime2 NOT NULL DEFAULT '2015-04-12T17:05:00.0000000';
""");
    }

    [ConditionalTheory]
    [InlineData(0, "", 1234567)]
    [InlineData(1, ".1", 1234567)]
    [InlineData(2, ".12", 1234567)]
    [InlineData(3, ".123", 1234567)]
    [InlineData(4, ".1234", 1234567)]
    [InlineData(5, ".12345", 1234567)]
    [InlineData(6, ".123456", 1234567)]
    [InlineData(7, ".1234567", 1234567)]
    [InlineData(7, ".1200000", 1200000)] //should this really output trailing zeros?
    public async Task Add_column_with_defaultValue_datetime_with_explicit_precision(int precision, string fractionalSeconds, int ticksToAdd)
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<DateTime>("Birthday").HasPrecision(precision)
                .HasDefaultValue(new DateTime(2015, 4, 12, 17, 5, 0).AddTicks(ticksToAdd)),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var birthdayColumn = Assert.Single(table.Columns, c => c.Name == "Birthday");
                Assert.False(birthdayColumn.IsNullable);
            });

        AssertSql(
            $"""
ALTER TABLE [People] ADD [Birthday] datetime2({precision}) NOT NULL DEFAULT '2015-04-12T17:05:00{fractionalSeconds}';
""");
    }

    [ConditionalTheory]
    [InlineData(0, "", 1234567)]
    [InlineData(1, ".1", 1234567)]
    [InlineData(2, ".12", 1234567)]
    [InlineData(3, ".123", 1234567)]
    [InlineData(4, ".1234", 1234567)]
    [InlineData(5, ".12345", 1234567)]
    [InlineData(6, ".123456", 1234567)]
    [InlineData(7, ".1234567", 1234567)]
    [InlineData(7, ".1200000", 1200000)] //should this really output trailing zeros?
    public async Task Add_column_with_defaultValue_datetimeoffset_with_explicit_precision(
        int precision,
        string fractionalSeconds,
        int ticksToAdd)
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<DateTimeOffset>("Birthday").HasPrecision(precision)
                .HasDefaultValue(new DateTimeOffset(new DateTime(2015, 4, 12, 17, 5, 0).AddTicks(ticksToAdd), TimeSpan.FromHours(10))),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var birthdayColumn = Assert.Single(table.Columns, c => c.Name == "Birthday");
                Assert.False(birthdayColumn.IsNullable);
            });

        AssertSql(
            $"""
ALTER TABLE [People] ADD [Birthday] datetimeoffset({precision}) NOT NULL DEFAULT '2015-04-12T17:05:00{fractionalSeconds}+10:00';
""");
    }

    [ConditionalTheory]
    [InlineData(0, "", 1234567)]
    [InlineData(1, ".1", 1234567)]
    [InlineData(2, ".12", 1234567)]
    [InlineData(3, ".123", 1234567)]
    [InlineData(4, ".1234", 1234567)]
    [InlineData(5, ".12345", 1234567)]
    [InlineData(6, ".123456", 1234567)]
    [InlineData(7, ".1234567", 1234567)]
    [InlineData(7, ".12", 1200000)]
    public async Task Add_column_with_defaultValue_time_with_explicit_precision(int precision, string fractionalSeconds, int ticksToAdd)
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<TimeSpan>("Age").HasPrecision(precision)
                .HasDefaultValue(
                    TimeSpan.Parse("12:34:56", CultureInfo.InvariantCulture).Add(TimeSpan.FromTicks(ticksToAdd))),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var birthdayColumn = Assert.Single(table.Columns, c => c.Name == "Age");
                Assert.False(birthdayColumn.IsNullable);
            });

        AssertSql(
            $"""
ALTER TABLE [People] ADD [Age] time({precision}) NOT NULL DEFAULT '12:34:56{fractionalSeconds}';
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_with_defaultValue_datetime_store_type()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<DateTime>("Birthday")
                .HasColumnType("datetime")
                .HasDefaultValue(new DateTime(2019, 1, 1)),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Birthday");
                Assert.Contains("2019", column.DefaultValueSql);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD [Birthday] datetime NOT NULL DEFAULT '2019-01-01T00:00:00.000';
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_with_defaultValue_smalldatetime_store_type()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<DateTime>("Birthday")
                .HasColumnType("smalldatetime")
                .HasDefaultValue(new DateTime(2019, 1, 1)),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Birthday");
                Assert.Contains("2019", column.DefaultValueSql);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD [Birthday] smalldatetime NOT NULL DEFAULT '2019-01-01T00:00:00';
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_with_rowversion()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<byte[]>("RowVersion").IsRowVersion(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "RowVersion");
                Assert.Equal("rowversion", column.StoreType);
                Assert.True(column.IsRowVersion());
            });

        AssertSql(
            """
ALTER TABLE [People] ADD [RowVersion] rowversion NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_with_rowversion_and_value_conversion()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<ulong>("RowVersion")
                .IsRowVersion()
                .HasConversion<byte[]>(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "RowVersion");
                Assert.Equal("rowversion", column.StoreType);
                Assert.True(column.IsRowVersion());
            });

        AssertSql(
            """
ALTER TABLE [People] ADD [RowVersion] rowversion NOT NULL;
""");
    }

    public override async Task Add_column_with_defaultValueSql()
    {
        await base.Add_column_with_defaultValueSql();

        AssertSql(
            """
ALTER TABLE [People] ADD [Sum] int NOT NULL DEFAULT (1 + 2);
""");
    }

    public override async Task Add_json_columns_to_existing_table()
    {
        await base.Add_json_columns_to_existing_table();

        AssertSql(
            """
ALTER TABLE [Entity] ADD [OwnedCollection] nvarchar(max) NULL;
""",
            //
            """
ALTER TABLE [Entity] ADD [OwnedReference] nvarchar(max) NULL;
""",
            //
            """
ALTER TABLE [Entity] ADD [OwnedRequiredReference] nvarchar(max) NOT NULL DEFAULT N'{}';
""");
    }

    public override async Task Add_column_with_computedSql(bool? stored)
    {
        await base.Add_column_with_computedSql(stored);

        var computedColumnTypeSql = stored == true ? " PERSISTED" : "";

        AssertSql(
            $"""
ALTER TABLE [People] ADD [Sum] AS [X] + [Y]{computedColumnTypeSql};
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_generates_exec_when_computed_and_idempotent()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int>("IdPlusOne").HasComputedColumnSql("[Id] + 1"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var column = Assert.Single(table.Columns, c => c.Name == "IdPlusOne");
                Assert.Equal("([Id]+(1))", column.ComputedColumnSql);
            },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
EXEC(N'ALTER TABLE [People] ADD [IdPlusOne] AS [Id] + 1');
""");
    }

    public override async Task Add_column_with_required()
    {
        await base.Add_column_with_required();

        AssertSql(
            """
ALTER TABLE [People] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';
""");
    }

    public override async Task Add_column_with_ansi()
    {
        await base.Add_column_with_ansi();

        AssertSql(
            """
ALTER TABLE [People] ADD [Name] varchar(max) NULL;
""");
    }

    public override async Task Add_column_with_max_length()
    {
        await base.Add_column_with_max_length();

        AssertSql(
            """
ALTER TABLE [People] ADD [Name] nvarchar(30) NULL;
""");
    }

    public override async Task Add_column_with_max_length_on_derived()
    {
        await base.Add_column_with_max_length_on_derived();

        Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
    }

    public override async Task Add_column_with_fixed_length()
    {
        await base.Add_column_with_fixed_length();

        AssertSql(
            """
ALTER TABLE [People] ADD [Name] nchar(100) NULL;
""");
    }

    public override async Task Add_column_with_comment()
    {
        await base.Add_column_with_comment();

        AssertSql(
            """
ALTER TABLE [People] ADD [FullName] nvarchar(max) NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'FullName';
""");
    }

    public override async Task Add_column_with_collation()
    {
        await base.Add_column_with_collation();

        AssertSql(
            """
ALTER TABLE [People] ADD [Name] nvarchar(max) COLLATE German_PhoneBook_CI_AS NULL;
""");
    }

    public override async Task Add_column_computed_with_collation(bool stored)
    {
        await base.Add_column_computed_with_collation(stored);

        AssertSql(
            stored
                ? """ALTER TABLE [People] ADD [Name] AS 'hello' COLLATE German_PhoneBook_CI_AS PERSISTED;"""
                : """ALTER TABLE [People] ADD [Name] AS 'hello' COLLATE German_PhoneBook_CI_AS;""");
    }

    public override async Task Add_column_shared()
    {
        await base.Add_column_shared();

        AssertSql();
    }

    public override async Task Add_column_with_check_constraint()
    {
        await base.Add_column_with_check_constraint();

        AssertSql(
            """
ALTER TABLE [People] ADD [DriverLicense] int NOT NULL DEFAULT 0;
""",
            //
            """
ALTER TABLE [People] ADD CONSTRAINT [CK_People_Foo] CHECK ([DriverLicense] > 0);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_identity()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int>("IdentityColumn").UseIdentityColumn(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "IdentityColumn");
                Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD [IdentityColumn] int NOT NULL IDENTITY;
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_identity_seed_increment()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int>("IdentityColumn").UseIdentityColumn(100, 5),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "IdentityColumn");
                Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
                // TODO: Do we not reverse-engineer identity facets?
                // Assert.Equal(100, column[SqlServerAnnotationNames.IdentitySeed]);
                // Assert.Equal(5, column[SqlServerAnnotationNames.IdentityIncrement]);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD [IdentityColumn] int NOT NULL IDENTITY(100, 5);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_identity_seed_increment_for_TPC()
    {
        await Test(
            builder =>
            {
                builder.Entity("Animal").UseTpcMappingStrategy().Property<string>("Id");
                builder.Entity("Cat").HasBaseType("Animal").ToTable("Cats");
                builder.Entity("Dog").HasBaseType("Animal").ToTable("Dogs");
            },
            builder => { },
            builder =>
            {
                builder.Entity("Animal")
                    .Property<int>("IdentityColumn");
                builder.Entity("Cat").ToTable("Cats", tb => tb.Property("IdentityColumn").UseIdentityColumn(1, 2));
                builder.Entity("Dog").ToTable("Dogs", tb => tb.Property("IdentityColumn").UseIdentityColumn(2, 2));
            },
            model =>
            {
                Assert.Collection(
                    model.Tables,
                    t =>
                    {
                        Assert.Equal("Animal", t.Name);
                        var column = Assert.Single(t.Columns, c => c.Name == "IdentityColumn");
                        Assert.Null(column.ValueGenerated);
                    },
                    t =>
                    {
                        Assert.Equal("Cats", t.Name);
                        var column = Assert.Single(t.Columns, c => c.Name == "IdentityColumn");
                        Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
                        // TODO: Do we not reverse-engineer identity facets?
                        // Assert.Equal(100, column[SqlServerAnnotationNames.IdentitySeed]);
                        // Assert.Equal(5, column[SqlServerAnnotationNames.IdentityIncrement]);
                    },
                    t =>
                    {
                        Assert.Equal("Dogs", t.Name);
                        var column = Assert.Single(t.Columns, c => c.Name == "IdentityColumn");
                        Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
                        // TODO: Do we not reverse-engineer identity facets?
                        // Assert.Equal(100, column[SqlServerAnnotationNames.IdentitySeed]);
                        // Assert.Equal(5, column[SqlServerAnnotationNames.IdentityIncrement]);
                    });
            });

        AssertSql(
            """
ALTER TABLE [Dogs] ADD [IdentityColumn] int NOT NULL IDENTITY(2, 2);
""",
            //
            """
ALTER TABLE [Cats] ADD [IdentityColumn] int NOT NULL IDENTITY(1, 2);
""",
            //
            """
ALTER TABLE [Animal] ADD [IdentityColumn] int NOT NULL DEFAULT 0;
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_sequence()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int>("SequenceColumn").UseSequence(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "SequenceColumn");

                // Note: #29863 tracks recognizing sequence columns as such
                Assert.Equal("(NEXT VALUE FOR [PeopleSequence])", column.DefaultValueSql);
            });

        AssertSql(
            """
CREATE SEQUENCE [PeopleSequence] START WITH 1 INCREMENT BY 1 NO CYCLE;
""",
            //
            """
ALTER TABLE [People] ADD [SequenceColumn] int NOT NULL DEFAULT (NEXT VALUE FOR [PeopleSequence]);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_column_hilo()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int>("SequenceColumn").UseHiLo(),
            _ =>
            {
                // Reverse-engineering of hilo columns isn't supported
            });

        AssertSql(
            """
CREATE SEQUENCE [EntityFrameworkHiLoSequence] START WITH 1 INCREMENT BY 10 NO CYCLE;
""",
            //
            """
ALTER TABLE [People] ADD [SequenceColumn] int NOT NULL DEFAULT 0;
""");
    }

    public override async Task Alter_column_change_type()
    {
        await base.Alter_column_change_type();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] bigint NOT NULL;
""");
    }

    public override async Task Alter_column_make_required()
    {
        await base.Alter_column_make_required();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [People] SET [SomeColumn] = N'' WHERE [SomeColumn] IS NULL;
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(max) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];
""");
    }

    public override async Task Alter_column_make_required_with_null_data()
    {
        await base.Alter_column_make_required_with_null_data();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [People] SET [SomeColumn] = N'' WHERE [SomeColumn] IS NULL;
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(max) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_make_required_with_index()
    {
        await base.Alter_column_make_required_with_index();

        AssertSql(
            """
DROP INDEX [IX_People_SomeColumn] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [People] SET [SomeColumn] = N'' WHERE [SomeColumn] IS NULL;
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];
CREATE INDEX [IX_People_SomeColumn] ON [People] ([SomeColumn]);
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_make_required_with_composite_index()
    {
        await base.Alter_column_make_required_with_composite_index();

        AssertSql(
            """
DROP INDEX [IX_People_FirstName_LastName] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [People] SET [FirstName] = N'' WHERE [FirstName] IS NULL;
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [FirstName];
CREATE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]);
""");
    }

    public override async Task Alter_column_make_computed(bool? stored)
    {
        await base.Alter_column_make_computed(stored);

        var computedColumnTypeSql = stored == true ? " PERSISTED" : "";

        AssertSql(
            $"""
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] + [Y]{computedColumnTypeSql};
""");
    }

    public override async Task Alter_column_change_computed()
    {
        await base.Alter_column_change_computed();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] - [Y];
""");
    }

    public override async Task Alter_column_change_computed_recreates_indexes()
    {
        await base.Alter_column_change_computed_recreates_indexes();

        AssertSql(
            """
DROP INDEX [IX_People_Sum] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] - [Y];
""",
            //
            """
CREATE INDEX [IX_People_Sum] ON [People] ([Sum]);
""");
    }

    public override async Task Alter_column_change_computed_type()
    {
        await base.Alter_column_change_computed_type();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] + [Y] PERSISTED;
""");
    }

    public override async Task Alter_column_make_non_computed()
    {
        await base.Alter_column_make_non_computed();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] int NOT NULL;
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_add_comment()
    {
        await base.Alter_column_add_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Some comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
""");
    }

    [ConditionalFact]
    public override async Task Alter_computed_column_add_comment()
    {
        await base.Alter_computed_column_add_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Some comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'SomeColumn';
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_change_comment()
    {
        await base.Alter_column_change_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
SET @description = N'Some comment2';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_remove_comment()
    {
        await base.Alter_column_remove_comment();

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_set_collation()
    {
        await base.Alter_column_set_collation();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(max) COLLATE German_PhoneBook_CI_AS NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_set_collation_with_index()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Name");
                    e.HasIndex("Name");
                }),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .UseCollation(NonDefaultCollation),
            model =>
            {
                var nameColumn = Assert.Single(Assert.Single(model.Tables).Columns);
                Assert.Equal(NonDefaultCollation, nameColumn.Collation);
            });

        AssertSql(
            """
DROP INDEX [IX_People_Name] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) COLLATE German_PhoneBook_CI_AS NULL;
CREATE INDEX [IX_People_Name] ON [People] ([Name]);
""");
    }

    [ConditionalFact]
    public override async Task Alter_column_reset_collation()
    {
        await base.Alter_column_reset_collation();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(max) NULL;
""");
    }

    public override async Task Convert_json_entities_to_regular_owned()
    {
        await base.Convert_json_entities_to_regular_owned();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'OwnedCollection');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Entity] DROP COLUMN [OwnedCollection];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'OwnedReference');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Entity] DROP COLUMN [OwnedReference];
""",
            //
            """
ALTER TABLE [Entity] ADD [OwnedReference_Date] datetime2 NULL;
""",
            //
            """
ALTER TABLE [Entity] ADD [OwnedReference_NestedReference_Number] int NULL;
""",
            //
            """
CREATE TABLE [Entity_NestedCollection] (
    [OwnedEntityId] int NOT NULL,
    [Id] int NOT NULL IDENTITY,
    [Number2] int NOT NULL,
    CONSTRAINT [PK_Entity_NestedCollection] PRIMARY KEY ([OwnedEntityId], [Id]),
    CONSTRAINT [FK_Entity_NestedCollection_Entity_OwnedEntityId] FOREIGN KEY ([OwnedEntityId]) REFERENCES [Entity] ([Id]) ON DELETE CASCADE
);
""",
//
            """
CREATE TABLE [Entity_OwnedCollection] (
    [EntityId] int NOT NULL,
    [Id] int NOT NULL IDENTITY,
    [Date2] datetime2 NOT NULL,
    [NestedReference2_Number3] int NULL,
    CONSTRAINT [PK_Entity_OwnedCollection] PRIMARY KEY ([EntityId], [Id]),
    CONSTRAINT [FK_Entity_OwnedCollection_Entity_EntityId] FOREIGN KEY ([EntityId]) REFERENCES [Entity] ([Id]) ON DELETE CASCADE
);
""",
            //
            """
CREATE TABLE [Entity_OwnedCollection_NestedCollection2] (
    [Owned2EntityId] int NOT NULL,
    [Owned2Id] int NOT NULL,
    [Id] int NOT NULL IDENTITY,
    [Number4] int NOT NULL,
    CONSTRAINT [PK_Entity_OwnedCollection_NestedCollection2] PRIMARY KEY ([Owned2EntityId], [Owned2Id], [Id]),
    CONSTRAINT [FK_Entity_OwnedCollection_NestedCollection2_Entity_OwnedCollection_Owned2EntityId_Owned2Id] FOREIGN KEY ([Owned2EntityId], [Owned2Id]) REFERENCES [Entity_OwnedCollection] ([EntityId], [Id]) ON DELETE CASCADE
);
""");
    }

    public override async Task Convert_regular_owned_entities_to_json()
    {
        await base.Convert_regular_owned_entities_to_json();

        AssertSql(
            """
DROP TABLE [Entity_NestedCollection];
""",
            //
            """
DROP TABLE [Entity_OwnedCollection_NestedCollection2];
""",
            //
            """
DROP TABLE [Entity_OwnedCollection];
""",
            //
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'OwnedReference_Date');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Entity] DROP COLUMN [OwnedReference_Date];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'OwnedReference_NestedReference_Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Entity] DROP COLUMN [OwnedReference_NestedReference_Number];
""",
            //
            """
ALTER TABLE [Entity] ADD [OwnedCollection] nvarchar(max) NULL;
""",
            //
            """
ALTER TABLE [Entity] ADD [OwnedReference] nvarchar(max) NULL;
""");
    }

    public override async Task Convert_string_column_to_a_json_column_containing_reference()
    {
        await base.Convert_string_column_to_a_json_column_containing_reference();

        AssertSql();
    }

    public override async Task Convert_string_column_to_a_json_column_containing_required_reference()
    {
        await base.Convert_string_column_to_a_json_column_containing_required_reference();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Entity] SET [Name] = N'{}' WHERE [Name] IS NULL;
ALTER TABLE [Entity] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
ALTER TABLE [Entity] ADD DEFAULT N'{}' FOR [Name];
""");
    }

    public override async Task Convert_string_column_to_a_json_column_containing_collection()
    {
        await base.Convert_string_column_to_a_json_column_containing_collection();

        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task Alter_column_make_required_with_index_with_included_properties()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("SomeColumn");
                    e.Property<string>("SomeOtherColumn");
                    e.HasIndex("SomeColumn").IncludeProperties("SomeOtherColumn");
                }),
            builder => { },
            builder => builder.Entity("People").Property<string>("SomeColumn").IsRequired(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "SomeColumn");
                Assert.False(column.IsNullable);
                var index = Assert.Single(table.Indexes);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "SomeColumn"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
            });

        AssertSql(
            """
DROP INDEX [IX_People_SomeColumn] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [People] SET [SomeColumn] = N'' WHERE [SomeColumn] IS NULL;
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];
CREATE INDEX [IX_People_SomeColumn] ON [People] ([SomeColumn]) INCLUDE ([SomeOtherColumn]);
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public virtual async Task Alter_column_memoryOptimized_with_index()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.ToTable(tb => tb.IsMemoryOptimized());
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.HasKey("Id").IsClustered(false);
                    e.HasIndex("Name").IsClustered(false);
                }),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name").HasMaxLength(30),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                Assert.Equal("nvarchar(30)", column.StoreType);
            });

        AssertSql(
            """
ALTER TABLE [People] DROP INDEX [IX_People_Name];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(30) NULL;
ALTER TABLE [People] ADD INDEX [IX_People_Name] NONCLUSTERED ([Name]);
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_with_index_no_narrowing()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.HasIndex("Name");
                }),
            builder => builder.Entity("People").Property<string>("Name").IsRequired(),
            builder => builder.Entity("People").Property<string>("Name").IsRequired(false),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                Assert.True(column.IsNullable);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_with_index_included_column()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.HasIndex("FirstName", "LastName").IncludeProperties("Name");
                }),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name").HasMaxLength(30),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Equal(2, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "FirstName"), index.Columns);
                Assert.Contains(table.Columns.Single(c => c.Name == "LastName"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
            });

        AssertSql(
            """
DROP INDEX [IX_People_FirstName_LastName] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(30) NULL;
CREATE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]) INCLUDE ([Name]);
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_add_identity()
    {
        var ex = await TestThrows<InvalidOperationException>(
            builder => builder.Entity("People").Property<int>("SomeColumn"),
            builder => builder.Entity("People").Property<int>("SomeColumn").UseIdentityColumn());

        Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
    }

    [ConditionalFact]
    public virtual async Task Alter_column_remove_identity()
    {
        var ex = await TestThrows<InvalidOperationException>(
            builder => builder.Entity("People").Property<int>("SomeColumn").UseIdentityColumn(),
            builder => builder.Entity("People").Property<int>("SomeColumn"));

        Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
    }

    [ConditionalFact]
    public virtual async Task Alter_column_change_type_with_identity()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Id");
                    e.Property<int>("IdentityColumn").UseIdentityColumn();
                }),
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Id");
                    e.Property<long>("IdentityColumn").UseIdentityColumn();
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "IdentityColumn");
                Assert.Equal("bigint", column.StoreType);
                Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'IdentityColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [IdentityColumn] bigint NOT NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_change_identity_seed()
    {
        await Test(
            builder => builder.Entity("People", e => e.Property<int>("Id").UseIdentityColumn(seed: 10)),
            builder => builder.Entity("People", e => e.Property<int>("Id").UseIdentityColumn(seed: 100)),
            model =>
            {
                // DBCC CHECKIDENT RESEED doesn't actually change the table definition, it only resets the current identity value.
                // For example, if the table is truncated, the identity is reset back to its original value (with the RESEED lost).
                // Therefore we cannot check the value via scaffolding.
            });

        AssertSql(
            """
DBCC CHECKIDENT(N'[People]', RESEED, 100);
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_change_default()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Name"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .HasDefaultValue("Doe"),
            model =>
            {
                var nameColumn = Assert.Single(Assert.Single(model.Tables).Columns);
                Assert.Equal("(N'Doe')", nameColumn.DefaultValueSql);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ADD DEFAULT N'Doe' FOR [Name];
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_change_comment_with_default()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("Name").HasDefaultValue("Doe"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .HasComment("Some comment"),
            model =>
            {
                var nameColumn = Assert.Single(Assert.Single(model.Tables).Columns);
                Assert.Equal("(N'Doe')", nameColumn.DefaultValueSql);
                Assert.Equal("Some comment", nameColumn.Comment);
            });

        AssertSql(
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Some comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_column_make_sparse()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("SomeProperty"),
            builder => { },
            builder => builder.Entity("People").Property<string>("SomeProperty")
                .IsSparse(),
            model =>
            {
                var column = Assert.Single(Assert.Single(model.Tables).Columns);
                Assert.True((bool?)column[SqlServerAnnotationNames.Sparse]);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeProperty');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeProperty] nvarchar(max) SPARSE NULL;
""");
    }

    public override async Task Drop_column()
    {
        await base.Drop_column();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [SomeColumn];
""");
    }

    public override async Task Drop_column_primary_key()
    {
        await base.Drop_column_primary_key();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [PK_People];
""",
            //
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Id];
""");
    }

    public override async Task Drop_column_computed_and_non_computed_with_dependency()
    {
        await base.Drop_column_computed_and_non_computed_with_dependency();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Y');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Y];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'X');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [People] DROP COLUMN [X];
""");
    }

    public override async Task Drop_json_columns_from_existing_table()
    {
        await base.Drop_json_columns_from_existing_table();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'OwnedCollection');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Entity] DROP COLUMN [OwnedCollection];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'OwnedReference');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Entity] DROP COLUMN [OwnedReference];
""");
    }

    public override async Task Rename_column()
    {
        await base.Rename_column();

        AssertSql(
            """
EXEC sp_rename N'[People].[SomeColumn]', N'SomeOtherColumn', 'COLUMN';
""");
    }

    public override async Task Rename_json_column()
    {
        await base.Rename_json_column();

        AssertSql(
            """
EXEC sp_rename N'[Entity].[json_reference]', N'new_json_reference', 'COLUMN';
""",
            //
            """
EXEC sp_rename N'[Entity].[json_collection]', N'new_json_collection', 'COLUMN';
""");
    }

    public override async Task Create_index()
    {
        await base.Create_index();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;
""",
            //
            """
CREATE INDEX [IX_People_FirstName] ON [People] ([FirstName]);
""");
    }

    public override async Task Create_index_unique()
    {
        await base.Create_index_unique();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'LastName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [LastName] nvarchar(450) NULL;
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL;
""");
    }

    public override async Task Create_index_descending()
    {
        await base.Create_index_descending();

        AssertSql(
            """
CREATE INDEX [IX_People_X] ON [People] ([X] DESC);
""");
    }

    public override async Task Create_index_descending_mixed()
    {
        await base.Create_index_descending_mixed();

        AssertSql(
            """
CREATE INDEX [IX_People_X_Y_Z] ON [People] ([X], [Y] DESC, [Z]);
""");
    }

    public override async Task Alter_index_make_unique()
    {
        await base.Alter_index_make_unique();

        AssertSql(
            """
DROP INDEX [IX_People_X] ON [People];
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_X] ON [People] ([X]);
""");
    }

    public override async Task Alter_index_change_sort_order()
    {
        await base.Alter_index_change_sort_order();

        AssertSql(
            """
DROP INDEX [IX_People_X_Y_Z] ON [People];
""",
            //
            """
CREATE INDEX [IX_People_X_Y_Z] ON [People] ([X], [Y] DESC, [Z]);
""");
    }

    public override async Task Create_index_with_filter()
    {
        await base.Create_index_with_filter();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
CREATE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task CreateIndex_generates_exec_when_filter_and_idempotent()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name").HasFilter("[Name] IS NOT NULL"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Same(table.Columns.Single(c => c.Name == "Name"), Assert.Single(index.Columns));
                Assert.Contains("Name", index.Filter);
            },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
EXEC(N'CREATE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL');
""");
    }

    public override async Task Create_unique_index_with_filter()
    {
        await base.Create_unique_index_with_filter();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL AND [Name] <> '';
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_clustered()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("FirstName"),
            builder => { },
            builder => builder.Entity("People").HasIndex("FirstName").IsClustered(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True((bool?)index[SqlServerAnnotationNames.Clustered]);
                Assert.False(index.IsUnique);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;
""",
            //
            """
CREATE CLUSTERED INDEX [IX_People_FirstName] ON [People] ([FirstName]);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_unique_clustered()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("FirstName"),
            builder => { },
            builder => builder.Entity("People").HasIndex("FirstName")
                .IsUnique()
                .IsClustered(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True((bool?)index[SqlServerAnnotationNames.Clustered]);
                Assert.True(index.IsUnique);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;
""",
            //
            """
CREATE UNIQUE CLUSTERED INDEX [IX_People_FirstName] ON [People] ([FirstName]);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_with_include()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name");
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IncludeProperties("FirstName", "LastName"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_with_include_and_filter()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name");
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IncludeProperties("FirstName", "LastName")
                .HasFilter("[Name] IS NOT NULL"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Equal("([Name] IS NOT NULL)", index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL;
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_unique_with_include()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_unique_with_include_and_filter()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName")
                .HasFilter("[Name] IS NOT NULL"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Equal("([Name] IS NOT NULL)", index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL;
""");
    }

    [ConditionalFact(Skip = "#19668, Online index operations can only be performed in Enterprise edition of SQL Server")]
    [SqlServerCondition(SqlServerCondition.SupportsOnlineIndexes)]
    public virtual async Task Create_index_unique_with_include_and_filter_online()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName")
                .HasFilter("[Name] IS NOT NULL")
                .IsCreatedOnline(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Equal("([Name] IS NOT NULL)", index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
                // TODO: Online index not scaffolded?
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL WITH (ONLINE = ON);
""");
    }

    [ConditionalFact(Skip = "#19668, Online index operations can only be performed in Enterprise edition of SQL Server")]
    [SqlServerCondition(SqlServerCondition.SupportsOnlineIndexes)]
    public virtual async Task Create_index_unique_with_include_filter_online_and_fillfactor()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName")
                .HasFilter("[Name] IS NOT NULL")
                .IsCreatedOnline()
                .HasFillFactor(90),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Equal("([Name] IS NOT NULL)", index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
                // TODO: Online index not scaffolded?
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL WITH (FILLFACTOR = 90, ONLINE = ON);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_unique_with_include_filter_and_fillfactor()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName")
                .HasFilter("[Name] IS NOT NULL")
                .HasFillFactor(90),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Equal("([Name] IS NOT NULL)", index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
                // TODO: Online index not scaffolded?
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
            //
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL WITH (FILLFACTOR = 90);
""");
    }

    [ConditionalFact]
    public virtual async Task Create_index_unique_with_include_fillfactor_and_sortintempdb()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName")
                .HasFillFactor(75)
                .SortInTempDb(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Null(index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
                Assert.Equal(75, index[SqlServerAnnotationNames.FillFactor]);
                Assert.Null(index[SqlServerAnnotationNames.SortInTempDb]);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
//
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WITH (FILLFACTOR = 75, SORT_IN_TEMPDB = ON);
""");
    }

    [ConditionalTheory]
    [InlineData(DataCompressionType.None, "NONE")]
    [InlineData(DataCompressionType.Row, "ROW")]
    [InlineData(DataCompressionType.Page, "PAGE")]
    public virtual async Task Create_index_unique_with_include_sortintempdb_and_datacompression(
        DataCompressionType dataCompression,
        string dataCompressionSql)
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("FirstName");
                    e.Property<string>("LastName");
                    e.Property<string>("Name").IsRequired();
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name")
                .IsUnique()
                .IncludeProperties("FirstName", "LastName")
                .SortInTempDb()
                .UseDataCompression(dataCompression),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
                Assert.Null(index.Filter);
                Assert.Equal(1, index.Columns.Count);
                Assert.Contains(table.Columns.Single(c => c.Name == "Name"), index.Columns);
                var includedColumns = (IReadOnlyList<string>?)index[SqlServerAnnotationNames.Include];
                Assert.Null(includedColumns);
                Assert.Null(index[SqlServerAnnotationNames.SortInTempDb]);
                Assert.Null(index[SqlServerAnnotationNames.DataCompression]);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
""",
//
            $"""
CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WITH (SORT_IN_TEMPDB = ON, DATA_COMPRESSION = {dataCompressionSql});
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public virtual async Task Create_index_memoryOptimized_unique_nullable()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.ToTable(tb => tb.IsMemoryOptimized());
                    e.HasKey("Id").IsClustered(false);
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name").IsUnique(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Same(table.Columns.Single(c => c.Name == "Name"), Assert.Single(index.Columns));
                Assert.False(index.IsUnique);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
ALTER TABLE [People] ADD INDEX [IX_People_Name] NONCLUSTERED ([Name]);
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public virtual async Task Create_index_memoryOptimized_unique_nullable_with_filter()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.ToTable(tb => tb.IsMemoryOptimized());
                    e.HasKey("Id").IsClustered(false);
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name").IsUnique().HasFilter("[Name] IS NOT NULL AND <> ''"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Same(table.Columns.Single(c => c.Name == "Name"), Assert.Single(index.Columns));
                Assert.False(index.IsUnique);
                Assert.Null(index.Filter);
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;
""",
            //
            """
ALTER TABLE [People] ADD INDEX [IX_People_Name] NONCLUSTERED ([Name]);
""");
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public virtual async Task Create_index_memoryOptimized_unique_nonclustered_not_nullable()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Name").IsRequired();
                    e.ToTable(tb => tb.IsMemoryOptimized());
                    e.ToTable(tb => tb.IsMemoryOptimized());
                    e.HasKey("Name").IsClustered(false);
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("Name").IsUnique().IsClustered(false),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Same(table.Columns.Single(c => c.Name == "Name"), Assert.Single(index.Columns));
                Assert.True(index.IsUnique);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD INDEX [IX_People_Name] UNIQUE NONCLUSTERED ([Name]);
""");
    }

    public override async Task Drop_index()
    {
        await base.Drop_index();

        AssertSql(
            """
DROP INDEX [IX_People_SomeField] ON [People];
""");
    }

    public override async Task Rename_index()
    {
        await base.Rename_index();

        AssertSql(
            """
EXEC sp_rename N'[People].[Foo]', N'foo', 'INDEX';
""");
    }

    public override async Task Add_primary_key_int()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Add_primary_key_int());

        Assert.Equal(SqlServerStrings.AlterIdentityColumn, exception.Message);
    }

    public override async Task Add_primary_key_string()
    {
        await base.Add_primary_key_string();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeField');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeField] nvarchar(450) NOT NULL;
""",
            //
            """
ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY ([SomeField]);
""");
    }

    public override async Task Add_primary_key_with_name()
    {
        await base.Add_primary_key_with_name();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeField');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [People] SET [SomeField] = N'' WHERE [SomeField] IS NULL;
ALTER TABLE [People] ALTER COLUMN [SomeField] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeField];
""",
            //
            """
ALTER TABLE [People] ADD CONSTRAINT [PK_Foo] PRIMARY KEY ([SomeField]);
""");
    }

    public override async Task Add_primary_key_composite_with_name()
    {
        await base.Add_primary_key_composite_with_name();

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [PK_Foo] PRIMARY KEY ([SomeField1], [SomeField2]);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_primary_key_nonclustered()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("SomeField").IsRequired().HasMaxLength(450),
            builder => { },
            builder => builder.Entity("People").HasKey("SomeField").IsClustered(false),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.False((bool?)primaryKey![SqlServerAnnotationNames.Clustered]);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY NONCLUSTERED ([SomeField]);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_primary_key_with_fill_factor()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("SomeField").IsRequired().HasMaxLength(450),
            builder => { },
            builder => builder.Entity("People").HasKey("SomeField").HasFillFactor(80),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Equal(80, primaryKey[SqlServerAnnotationNames.FillFactor]);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY ([SomeField]) WITH (FILLFACTOR = 80);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_alternate_key_with_fill_factor()
    {
        await Test(
            builder => {
                builder.Entity("People").Property<string>("SomeField").IsRequired().HasMaxLength(450);
                builder.Entity("People").Property<string>("SomeOtherField").IsRequired().HasMaxLength(450);
                },
            builder => { },
            builder => builder.Entity("People").HasAlternateKey(["SomeField", "SomeOtherField"]).HasFillFactor(80),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var uniqueConstraint = table.UniqueConstraints.FirstOrDefault();
                Assert.NotNull(uniqueConstraint);
                Assert.Equal(80, uniqueConstraint[SqlServerAnnotationNames.FillFactor]);
            });

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [AK_People_SomeField_SomeOtherField] UNIQUE ([SomeField], [SomeOtherField]) WITH (FILLFACTOR = 80);
""");
    }

    public override async Task Drop_primary_key_int()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Drop_primary_key_int());

        Assert.Equal(SqlServerStrings.AlterIdentityColumn, exception.Message);
    }

    public override async Task Drop_primary_key_string()
    {
        await base.Drop_primary_key_string();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [PK_People];
""",
            //
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeField');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeField] nvarchar(max) NOT NULL;
""");
    }

    public override async Task Add_foreign_key()
    {
        await base.Add_foreign_key();

        AssertSql(
            """
CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
""",
            //
            """
ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE;
""");
    }

    public override async Task Add_foreign_key_with_name()
    {
        await base.Add_foreign_key_with_name();

        // AssertSql(
        //     @"ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Customers_CustomerId];",
        //     //
        //     @"DROP INDEX [IX_Orders_CustomerId] ON [Orders];");

        AssertSql(
            """
CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
""",
            //
            """
ALTER TABLE [Orders] ADD CONSTRAINT [FK_Foo] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE;
""");
    }

    public override async Task Drop_foreign_key()
    {
        await base.Drop_foreign_key();

        AssertSql(
            """
ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Customers_CustomerId];
""",
            //
            """
DROP INDEX [IX_Orders_CustomerId] ON [Orders];
""");
    }

    public override async Task Add_unique_constraint()
    {
        await base.Add_unique_constraint();

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [AK_People_AlternateKeyColumn] UNIQUE ([AlternateKeyColumn]);
""");
    }

    public override async Task Add_unique_constraint_composite_with_name()
    {
        await base.Add_unique_constraint_composite_with_name();

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [AK_Foo] UNIQUE ([AlternateKeyColumn1], [AlternateKeyColumn2]);
""");
    }

    public override async Task Drop_unique_constraint()
    {
        await base.Drop_unique_constraint();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [AK_People_AlternateKeyColumn];
""");
    }

    public override async Task Add_check_constraint_with_name()
    {
        await base.Add_check_constraint_with_name();

        AssertSql(
            """
ALTER TABLE [People] ADD CONSTRAINT [CK_People_Foo] CHECK ([DriverLicense] > 0);
""");
    }

    [ConditionalFact]
    public virtual async Task Add_check_constraint_generates_exec_when_idempotent()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("DriverLicense");
                }),
            builder => { },
            builder => builder.Entity("People").ToTable(tb => tb.HasCheckConstraint("CK_People_Foo", "[DriverLicense] > 0")),
            model =>
            {
                // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
            },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
EXEC(N'ALTER TABLE [People] ADD CONSTRAINT [CK_People_Foo] CHECK ([DriverLicense] > 0)');
""");
    }

    public override async Task Alter_check_constraint()
    {
        await base.Alter_check_constraint();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [CK_People_Foo];
""",
            //
            """
ALTER TABLE [People] ADD CONSTRAINT [CK_People_Foo] CHECK ([DriverLicense] > 1);
""");
    }

    public override async Task Drop_check_constraint()
    {
        await base.Drop_check_constraint();

        AssertSql(
            """
ALTER TABLE [People] DROP CONSTRAINT [CK_People_Foo];
""");
    }

    public override async Task Create_sequence()
    {
        await base.Create_sequence();

        AssertSql(
            """
CREATE SEQUENCE [TestSequence] AS int START WITH 1 INCREMENT BY 1 NO CYCLE;
""");
    }

    [ConditionalFact]
    public async Task Create_sequence_byte()
    {
        await Test(
            builder => { },
            builder => builder.HasSequence<byte>("TestSequence"),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
            });
        AssertSql(
            """
CREATE SEQUENCE [TestSequence] AS tinyint START WITH 1 INCREMENT BY 1 NO CYCLE;
""");
    }

    [ConditionalFact]
    public async Task Create_sequence_decimal()
    {
        await Test(
            builder => { },
            builder => builder.HasSequence<decimal>("TestSequence"),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
            });

        AssertSql(
            """
CREATE SEQUENCE [TestSequence] AS decimal START WITH 1 INCREMENT BY 1 NO CYCLE;
""");
    }

    public override async Task Create_sequence_long()
    {
        await base.Create_sequence_long();

        AssertSql(
            """
CREATE SEQUENCE [TestSequence] START WITH 1 INCREMENT BY 1 NO CYCLE;
""");
    }

    public override async Task Create_sequence_short()
    {
        await base.Create_sequence_short();

        AssertSql(
            """
CREATE SEQUENCE [TestSequence] AS smallint START WITH 1 INCREMENT BY 1 NO CYCLE;
""");
    }

    public override async Task Create_sequence_all_settings()
    {
        await base.Create_sequence_all_settings();

        AssertSql(
            """
IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');
""",
//
            """
CREATE SEQUENCE [dbo2].[TestSequence] START WITH 3 INCREMENT BY 2 MINVALUE 2 MAXVALUE 916 CYCLE CACHE 20;
""");
    }

    public override async Task Create_sequence_nocache()
    {
        await base.Create_sequence_nocache();

        AssertSql(
            """
CREATE SEQUENCE [Alpha] START WITH 1 INCREMENT BY 1 NO CYCLE NO CACHE;
""");
    }

    public override async Task Create_sequence_cache()
    {
        await base.Create_sequence_cache();

        AssertSql(
            """
CREATE SEQUENCE [Beta] START WITH 1 INCREMENT BY 1 NO CYCLE CACHE 20;
""");
    }

    public override async Task Create_sequence_default_cache()
    {
        await base.Create_sequence_default_cache();

        AssertSql(
            """
CREATE SEQUENCE [Gamma] START WITH 1 INCREMENT BY 1 NO CYCLE;
""");
    }

    public override async Task Alter_sequence_all_settings()
    {
        await base.Alter_sequence_all_settings();

        AssertSql(
            """
ALTER SEQUENCE [foo] INCREMENT BY 2 MINVALUE -5 MAXVALUE 10 CYCLE CACHE 20;
""",
            //
            """
ALTER SEQUENCE [foo] RESTART WITH -3;
""");
    }

    public override async Task Alter_sequence_increment_by()
    {
        await base.Alter_sequence_increment_by();

        AssertSql(
            """
ALTER SEQUENCE [foo] INCREMENT BY 2 NO MINVALUE NO MAXVALUE NO CYCLE CACHE;
""");
    }

    public override async Task Alter_sequence_default_cache_to_cache()
    {
        await base.Alter_sequence_default_cache_to_cache();

        AssertSql(
            """
ALTER SEQUENCE [Delta] INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE CACHE 20;
""");
    }

    public override async Task Alter_sequence_default_cache_to_nocache()
    {
        await base.Alter_sequence_default_cache_to_nocache();

        AssertSql(
            """
ALTER SEQUENCE [Epsilon] INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE NO CACHE;
""");
    }

    public override async Task Alter_sequence_cache_to_nocache()
    {
        await base.Alter_sequence_cache_to_nocache();

        AssertSql(
            """
ALTER SEQUENCE [Zeta] INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE NO CACHE;
""");
    }

    public override async Task Alter_sequence_cache_to_default_cache()
    {
        await base.Alter_sequence_cache_to_default_cache();

        AssertSql(
            """
ALTER SEQUENCE [Eta] INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE CACHE;
""");
    }

    public override async Task Alter_sequence_nocache_to_cache()
    {
        await base.Alter_sequence_nocache_to_cache();

        AssertSql(
            """
ALTER SEQUENCE [Theta] INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE CACHE 20;
""");
    }

    public override async Task Alter_sequence_nocache_to_default_cache()
    {
        await base.Alter_sequence_nocache_to_default_cache();

        AssertSql(
"""
ALTER SEQUENCE [Iota] INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE CACHE;
""");
    }

    public override async Task Alter_sequence_restart_with()
    {
        await base.Alter_sequence_restart_with();

        AssertSql(
            @"ALTER SEQUENCE [foo] RESTART WITH 3;");
    }

    public override async Task Drop_sequence()
    {
        await base.Drop_sequence();

        AssertSql(
            """
DROP SEQUENCE [TestSequence];
""");
    }

    public override async Task Rename_sequence()
    {
        await base.Rename_sequence();

        AssertSql(
            """
EXEC sp_rename N'[TestSequence]', N'testsequence', 'OBJECT';
""");
    }

    public override async Task Move_sequence()
    {
        await base.Move_sequence();

        AssertSql(
            """
IF SCHEMA_ID(N'TestSequenceSchema') IS NULL EXEC(N'CREATE SCHEMA [TestSequenceSchema];');
""",
            //
            """
ALTER SCHEMA [TestSequenceSchema] TRANSFER [TestSequence];
""");
    }

    [ConditionalFact]
    public virtual async Task Move_sequence_into_default_schema()
    {
        await Test(
            builder => builder.HasSequence<int>("TestSequence", "TestSequenceSchema"),
            builder => builder.HasSequence<int>("TestSequence"),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("dbo", sequence.Schema);
                Assert.Equal("TestSequence", sequence.Name);
            });

        AssertSql(
            """
DECLARE @defaultSchema sysname = SCHEMA_NAME();
EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [TestSequenceSchema].[TestSequence];');
""");
    }

    [ConditionalFact]
    public async Task Create_sequence_and_dependent_column()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder =>
            {
                builder.HasSequence<int>("TestSequence");
                builder.Entity("People").Property<int>("SeqProp").HasDefaultValueSql("NEXT VALUE FOR TestSequence");
            },
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
            });

        AssertSql(
            """
CREATE SEQUENCE [TestSequence] AS int START WITH 1 INCREMENT BY 1 NO CYCLE;
""",
            //
            """
ALTER TABLE [People] ADD [SeqProp] int NOT NULL DEFAULT (NEXT VALUE FOR TestSequence);
""");
    }

    [ConditionalFact]
    public async Task Drop_sequence_and_dependent_column()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder =>
            {
                builder.HasSequence<int>("TestSequence");
                builder.Entity("People").Property<int>("SeqProp").HasDefaultValueSql("NEXT VALUE FOR TestSequence");
            },
            builder => { },
            model => Assert.Empty(model.Sequences));

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SeqProp');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [SeqProp];
""",
            //
            """
DROP SEQUENCE [TestSequence];
""");
    }

    public override async Task InsertDataOperation()
    {
        await base.InsertDataOperation();

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Person]'))
    SET IDENTITY_INSERT [Person] ON;
INSERT INTO [Person] ([Id], [Name])
VALUES (1, N'Daenerys Targaryen'),
(2, N'John Snow'),
(3, N'Arya Stark'),
(4, N'Harry Strickland'),
(5, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Person]'))
    SET IDENTITY_INSERT [Person] OFF;
""");
    }

    public override async Task DeleteDataOperation_simple_key()
    {
        await base.DeleteDataOperation_simple_key();

        // TODO remove rowcount
        AssertSql(
            """
DELETE FROM [Person]
WHERE [Id] = 2;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task DeleteDataOperation_composite_key()
    {
        await base.DeleteDataOperation_composite_key();

        // TODO remove rowcount
        AssertSql(
            """
DELETE FROM [Person]
WHERE [AnotherId] = 12 AND [Id] = 2;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task UpdateDataOperation_simple_key()
    {
        await base.UpdateDataOperation_simple_key();

        // TODO remove rowcount
        AssertSql(
            """
UPDATE [Person] SET [Name] = N'Another John Snow'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task UpdateDataOperation_composite_key()
    {
        await base.UpdateDataOperation_composite_key();

        // TODO remove rowcount
        AssertSql(
            """
UPDATE [Person] SET [Name] = N'Another John Snow'
WHERE [AnotherId] = 11 AND [Id] = 2;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task UpdateDataOperation_multiple_columns()
    {
        await base.UpdateDataOperation_multiple_columns();

        // TODO remove rowcount
        AssertSql(
            """
UPDATE [Person] SET [Age] = 21, [Name] = N'Another John Snow'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;
""");
    }

    [ConditionalFact]
    public virtual async Task InsertDataOperation_generates_exec_when_idempotent()
    {
        await Test(
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.HasKey("Id");
                }),
            builder => { },
            builder => builder.Entity("Person")
                .HasData(
                    new Person { Id = 1, Name = "Daenerys Targaryen" },
                    new Person { Id = 2, Name = "John Snow" },
                    new Person { Id = 3, Name = "Arya Stark" },
                    new Person { Id = 4, Name = "Harry Strickland" },
                    new Person { Id = 5, Name = null }),
            model => { },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Person]'))
    SET IDENTITY_INSERT [Person] ON;
EXEC(N'INSERT INTO [Person] ([Id], [Name])
VALUES (1, N''Daenerys Targaryen''),
(2, N''John Snow''),
(3, N''Arya Stark''),
(4, N''Harry Strickland''),
(5, NULL)');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Person]'))
    SET IDENTITY_INSERT [Person] OFF;
""");
    }

    [ConditionalFact]
    public virtual async Task DeleteDataOperation_generates_exec_when_idempotent()
    {
        await Test(
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.HasKey("Id");
                    e.HasData(new Person { Id = 1, Name = "Daenerys Targaryen" });
                }),
            builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "John Snow" }),
            builder => { },
            model => { },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
EXEC(N'DELETE FROM [Person]
WHERE [Id] = 2;
SELECT @@ROWCOUNT');
""");
    }

    [ConditionalFact]
    public virtual async Task UpdateDataOperation_generates_exec_when_idempotent()
    {
        await Test(
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.HasKey("Id");
                    e.HasData(new Person { Id = 1, Name = "Daenerys Targaryen" });
                }),
            builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "John Snow" }),
            builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "Another John Snow" }),
            model => { },
            migrationsSqlGenerationOptions: MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
EXEC(N'UPDATE [Person] SET [Name] = N''Another John Snow''
WHERE [Id] = 2;
SELECT @@ROWCOUNT');
""");
    }

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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CustomerHistory]))');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [End] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [Start] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([Start], [End])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CustomerHistory]))');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[HistoryTable]))');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'for VIP only';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'RenamedCustomers', 'COLUMN', N'Discount';
""",
            //
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'for VIP only';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'HistoryTable', 'COLUMN', N'Discount';
""",
            //
            """
ALTER TABLE [RenamedCustomers] ADD CONSTRAINT [PK_RenamedCustomers] PRIMARY KEY ([Id]);
""",
            //
            """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [RenamedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [myHistorySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT [' + @var3 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [mySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT [' + @var3 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [mySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[HistoryTable] DROP CONSTRAINT [' + @var3 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [myHistorySchema].[HistoryTable] DROP COLUMN [Name];
""",
            //
            """
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[mySchema].[Customers]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mySchema].[Customers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [mySchema].[Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[myHistorySchema].[HistoryTable]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [myHistorySchema].[HistoryTable] DROP CONSTRAINT [' + @var3 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodEnd');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customer] DROP COLUMN [PeriodEnd];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodEnd');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customer] DROP COLUMN [PeriodEnd];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'PeriodStart');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[CustomerHistory]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[CustomerHistory]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[CustomerHistory]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[CustomerHistory]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customers', 'COLUMN', N'End';
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'IsVip');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Customer] SET [IsVip] = CAST(0 AS bit) WHERE [IsVip] IS NULL;
ALTER TABLE [Customer] ALTER COLUMN [IsVip] bit NOT NULL;
ALTER TABLE [Customer] ADD DEFAULT CAST(0 AS bit) FOR [IsVip];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'IsVip');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
UPDATE [HistoryTable] SET [IsVip] = CAST(0 AS bit) WHERE [IsVip] IS NULL;
ALTER TABLE [HistoryTable] ALTER COLUMN [IsVip] bit NOT NULL;
ALTER TABLE [HistoryTable] ADD DEFAULT CAST(0 AS bit) FOR [IsVip];
""",
                //
                """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [End] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [Number] int NOT NULL,
    [NumberPlusFive] AS Number + 5 PERSISTED,
    [Start] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([Start], [End])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[HistoryTable]))');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'IdPlusFive');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customer] DROP COLUMN [IdPlusFive];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'IdPlusFive');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [HistoryTable] DROP COLUMN [IdPlusFive];
""",
                //
                """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
""");
    }

    [ConditionalFact]
    public virtual async Task Alter_computed_column_sql_on_temporal_table()
    {
        var message = (await Assert.ThrowsAsync<NotSupportedException>(() => Test(
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customer] DROP COLUMN [Number];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
                //
                """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
                    e.ToTable("Customers", "mySchema",
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'MyColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customer] ALTER COLUMN [MyColumn] int SPARSE NULL;
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'MyColumn');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [HistoryTable] ALTER COLUMN [MyColumn] int SPARSE NULL;
""",
                //
                """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'MyColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'CREATE TABLE [Customer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [SystemTimeEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [SystemTimeStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([SystemTimeStart], [SystemTimeEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CustomerHistory]))');
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customer';
SET @description = N'Column comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customer', 'COLUMN', N'Name';
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customer] ALTER COLUMN [Name] nvarchar(450) NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Column comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customer', 'COLUMN', N'Name';
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customer] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'Customer';
SET @description = N'Modified table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customer';
""",
            //
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'Customer', 'COLUMN', N'Name';
SET @description = N'Modified column comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customer', 'COLUMN', N'Name';
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [ModifiedCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[ModifiedHistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
            //
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customers', 'COLUMN', N'Name';
""",
            //
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'HistoryTable', 'COLUMN', N'Name';
""",
            //
            """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customers', 'COLUMN', N'End';
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'DateOfBirth');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [DateOfBirth];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'DateOfBirth');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Customers', 'COLUMN', N'End';
""",
            //
            """
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'HistoryTable', 'COLUMN', N'End';
""",
            //
            """
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Customers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Customers] DROP COLUMN [Number];
""",
                //
                """
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[HistoryTable]') AND [c].[name] = N'Number');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [HistoryTable] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [HistoryTable] DROP COLUMN [Number];
""",
                //
                """
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var3 + '];');
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Number');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[HistoryTable]))')
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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
            //
            """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
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
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [NewCustomers] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[NewHistoryTable]))')
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
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                var historyTable = Assert.Single(model.Tables.Where(t => t.Name == "HistoryTable"));

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
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'End');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Customers] DROP COLUMN [End];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Start');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
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
    public override async Task Add_required_primitive_collection_to_existing_table()
    {
        await base.Add_required_primitive_collection_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'[]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_with_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'[1,2,3]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table_core("N'[3, 2, 1]'");

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT (N'[3, 2, 1]');
""");
    }

    [ConditionalFact(Skip = "issue #33038")]
    public override async Task Add_required_primitive_collection_with_custom_converter_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_converter_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'nothing';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'some numbers';
""");
    }

    [ConditionalFact]
    public override async Task Add_optional_primitive_collection_to_existing_table()
    {
        await base.Add_optional_primitive_collection_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NULL;
""");
    }

    [ConditionalFact]
    public override async Task Create_table_with_required_primitive_collection()
    {
        await base.Create_table_with_required_primitive_collection();

        AssertSql(
"""
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Numbers] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public override async Task Create_table_with_optional_primitive_collection()
    {
        await base.Create_table_with_optional_primitive_collection();

        AssertSql(
"""
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Numbers] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public override async Task Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH()
    {
        await base.Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH();

        AssertSql(
"""
CREATE TABLE [Contacts] (
    [Id] int NOT NULL IDENTITY,
    [Discriminator] nvarchar(8) NOT NULL,
    [Name] nvarchar(max) NULL,
    [Number] int NULL,
    [MyComplex_Prop] nvarchar(max) NULL,
    [MyComplex_MyNestedComplex_Bar] datetime2 NULL,
    [MyComplex_MyNestedComplex_Foo] int NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_to_existing_table()
    {
        await base.Add_required_primitve_collection_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'[]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'[1,2,3]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table_core("N'[3, 2, 1]'");

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT (N'[3, 2, 1]');
""");
    }

    [ConditionalFact(Skip = "issue #33038")]
    public override async Task Add_required_primitve_collection_with_custom_converter_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_converter_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'nothing';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'some numbers';
""");
    }

    protected override string NonDefaultCollation
        => _nonDefaultCollation ??= GetDatabaseCollation() == "German_PhoneBook_CI_AS"
            ? "French_CI_AS"
            : "German_PhoneBook_CI_AS";

    private string? _nonDefaultCollation;

    private string? GetDatabaseCollation()
    {
        using var ctx = CreateContext();
        var connection = ctx.Database.GetDbConnection();
        using var command = connection.CreateCommand();

        command.CommandText = $@"
SELECT collation_name
FROM sys.databases
WHERE name = '{connection.Database}';";

        return command.ExecuteScalar() is string collation
            ? collation
            : null;
    }

    public class MigrationsSqlServerFixture : MigrationsFixtureBase
    {
        protected override string StoreName
            => nameof(MigrationsSqlServerTest);

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override RelationalTestHelpers TestHelpers
            => SqlServerTestHelpers.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddScoped<IDatabaseModelFactory, SqlServerDatabaseModelFactory>();
    }
}

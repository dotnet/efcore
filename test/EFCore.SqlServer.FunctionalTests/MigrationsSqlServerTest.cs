// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsSqlServerTest : MigrationsTestBase<MigrationsSqlServerTest.MigrationsSqlServerFixture>
    {
        protected static string EOL
            => Environment.NewLine;

        public MigrationsSqlServerTest(MigrationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Create_table()
        {
            await base.Create_table();

            AssertSql(
                @"CREATE TABLE [People] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id])
);");
        }

        public override async Task Create_table_all_settings()
        {
            await base.Create_table_all_settings();

            AssertSql(
                @"IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');",
                //
                @"CREATE TABLE [dbo2].[People] (
    [CustomId] int NOT NULL,
    [EmployerId] int NOT NULL,
    [SSN] nvarchar(11) COLLATE German_PhoneBook_CI_AS NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([CustomId]),
    CONSTRAINT [AK_People_SSN] UNIQUE ([SSN]),
    CONSTRAINT [CK_EmployerId] CHECK ([EmployerId] > 0),
    CONSTRAINT [FK_People_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([Id]) ON DELETE NO ACTION
);
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'dbo2', 'TABLE', N'People';
SET @description = N'Employer ID comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'dbo2', 'TABLE', N'People', 'COLUMN', N'EmployerId';");
        }

        public override async Task Create_table_no_key()
        {
            await base.Create_table_no_key();

            AssertSql(
                @"CREATE TABLE [Anonymous] (
    [SomeColumn] int NOT NULL
);");
        }

        public override async Task Create_table_with_comments()
        {
            await base.Create_table_with_comments();

            AssertSql(
                @"CREATE TABLE [People] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NULL
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';
SET @description = N'Column comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';");
        }

        public override async Task Create_table_with_multiline_comments()
        {
            await base.Create_table_with_multiline_comments();

            AssertSql(
                @"CREATE TABLE [People] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NULL
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = CONCAT(N'This is a multi-line', NCHAR(13), NCHAR(10), N'table comment.', NCHAR(13), NCHAR(10), N'More information can', NCHAR(13), NCHAR(10), N'be found in the docs.');
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';
SET @description = CONCAT(N'This is a multi-line', NCHAR(10), N'column comment.', NCHAR(10), N'More information can', NCHAR(10), N'be found in the docs.');
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';");
        }

        public override async Task Create_table_with_computed_column(bool? stored)
        {
            await base.Create_table_with_computed_column(stored);

            var storedSql = stored == true ? " PERSISTED" : "";

            AssertSql(
                $@"CREATE TABLE [People] (
    [Id] int NOT NULL,
    [Sum] AS [X] + [Y]{storedSql},
    [X] int NOT NULL,
    [Y] int NOT NULL
);");
        }

        public override async Task Drop_table()
        {
            await base.Drop_table();

            AssertSql(
                @"DROP TABLE [People];");
        }

        public override async Task Alter_table_add_comment()
        {
            await base.Alter_table_add_comment();

            AssertSql(
                @"DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';");
        }

        public override async Task Alter_table_add_comment_non_default_schema()
        {
            await base.Alter_table_add_comment_non_default_schema();

            AssertSql(
                @"DECLARE @description AS sql_variant;
SET @description = N'Table comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'SomeOtherSchema', 'TABLE', N'People';");
        }

        public override async Task Alter_table_change_comment()
        {
            await base.Alter_table_change_comment();

            AssertSql(
                @"DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People';
SET @description = N'Table comment2';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People';");
        }

        public override async Task Alter_table_remove_comment()
        {
            await base.Alter_table_remove_comment();

            AssertSql(
                @"DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People';");
        }

        public override async Task Rename_table()
        {
            await base.Rename_table();

            AssertSql(
                @"EXEC sp_rename N'[People]', N'Persons';");
        }

        public override async Task Rename_table_with_primary_key()
        {
            await base.Rename_table_with_primary_key();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [PK_People];",
                //
                @"EXEC sp_rename N'[People]', N'Persons';",
                //
                @"ALTER TABLE [Persons] ADD CONSTRAINT [PK_Persons] PRIMARY KEY ([Id]);");
        }

        public override async Task Move_table()
        {
            await base.Move_table();

            AssertSql(
                @"IF SCHEMA_ID(N'TestTableSchema') IS NULL EXEC(N'CREATE SCHEMA [TestTableSchema];');",
                //
                @"ALTER SCHEMA [TestTableSchema] TRANSFER [TestTable];");
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
                @"DECLARE @defaultSchema sysname = SCHEMA_NAME();
EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [TestTableSchema].[TestTable];');");
        }

        public override async Task Create_schema()
        {
            await base.Create_schema();

            AssertSql(
                @"IF SCHEMA_ID(N'SomeOtherSchema') IS NULL EXEC(N'CREATE SCHEMA [SomeOtherSchema];');",
                //
                @"CREATE TABLE [SomeOtherSchema].[People] (
    [Id] int NOT NULL
);");
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
                @"CREATE TABLE [dbo].[People] (
    [Id] int NOT NULL
);");
        }

        public override async Task Add_column_with_defaultValue_string()
        {
            await base.Add_column_with_defaultValue_string();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'John Doe';");
        }

        public override async Task Add_column_with_defaultValue_datetime()
        {
            await base.Add_column_with_defaultValue_datetime();

            AssertSql(
                @"ALTER TABLE [People] ADD [Birthday] datetime2 NOT NULL DEFAULT '2015-04-12T17:05:00.0000000';");
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
                @"ALTER TABLE [People] ADD [Birthday] datetime NOT NULL DEFAULT '2019-01-01T00:00:00.000';");
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
                @"ALTER TABLE [People] ADD [Birthday] smalldatetime NOT NULL DEFAULT '2019-01-01T00:00:00';");
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
                @"ALTER TABLE [People] ADD [RowVersion] rowversion NULL;");
        }

        public override async Task Add_column_with_defaultValueSql()
        {
            await base.Add_column_with_defaultValueSql();

            AssertSql(
                @"ALTER TABLE [People] ADD [Sum] int NOT NULL DEFAULT (1 + 2);");
        }

        public override async Task Add_column_with_computedSql(bool? stored)
        {
            await base.Add_column_with_computedSql(stored);

            var computedColumnTypeSql = stored == true ? " PERSISTED" : "";

            AssertSql(
                @$"ALTER TABLE [People] ADD [Sum] AS [X] + [Y]{computedColumnTypeSql};");
        }

        public override async Task Add_column_with_required()
        {
            await base.Add_column_with_required();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';");
        }

        public override async Task Add_column_with_ansi()
        {
            await base.Add_column_with_ansi();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] varchar(max) NULL;");
        }

        public override async Task Add_column_with_max_length()
        {
            await base.Add_column_with_max_length();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(30) NULL;");
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
                @"ALTER TABLE [People] ADD [Name] nchar(100) NULL;");
        }

        public override async Task Add_column_with_comment()
        {
            await base.Add_column_with_comment();

            AssertSql(
                @"ALTER TABLE [People] ADD [FullName] nvarchar(max) NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'My comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'FullName';");
        }

        public override async Task Add_column_with_collation()
        {
            await base.Add_column_with_collation();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(max) COLLATE German_PhoneBook_CI_AS NULL;");
        }

        public override async Task Add_column_computed_with_collation()
        {
            await base.Add_column_computed_with_collation();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] AS 'hello' COLLATE German_PhoneBook_CI_AS;");
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
                @"ALTER TABLE [People] ADD [DriverLicense] int NOT NULL DEFAULT 0;",
                //
                @"ALTER TABLE [People] ADD CONSTRAINT [CK_Foo] CHECK ([DriverLicense] > 0);");
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
                @"ALTER TABLE [People] ADD [IdentityColumn] int NOT NULL IDENTITY;");
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
                @"ALTER TABLE [People] ADD [IdentityColumn] int NOT NULL IDENTITY(100, 5);");
        }

        public override async Task Alter_column_change_type()
        {
            await base.Alter_column_change_type();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] bigint NOT NULL;");
        }

        public override async Task Alter_column_make_required()
        {
            await base.Alter_column_make_required();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(max) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];");
        }

        [ConditionalFact]
        public override async Task Alter_column_make_required_with_index()
        {
            await base.Alter_column_make_required_with_index();

            AssertSql(
                @"DROP INDEX [IX_People_SomeColumn] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];
CREATE INDEX [IX_People_SomeColumn] ON [People] ([SomeColumn]);");
        }

        [ConditionalFact]
        public override async Task Alter_column_make_required_with_composite_index()
        {
            await base.Alter_column_make_required_with_composite_index();

            AssertSql(
                @"DROP INDEX [IX_People_FirstName_LastName] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [FirstName];
CREATE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]);");
        }

        public override async Task Alter_column_make_computed(bool? stored)
        {
            await base.Alter_column_make_computed(stored);

            var computedColumnTypeSql = stored == true ? " PERSISTED" : "";

            AssertSql(
                $@"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] + [Y]{computedColumnTypeSql};");
        }

        public override async Task Alter_column_change_computed()
        {
            await base.Alter_column_change_computed();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] - [Y];");
        }

        public override async Task Alter_column_change_computed_type()
        {
            await base.Alter_column_change_computed_type();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Sum');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Sum];
ALTER TABLE [People] ADD [Sum] AS [X] + [Y] PERSISTED;");
        }

        [ConditionalFact]
        public override async Task Alter_column_add_comment()
        {
            await base.Alter_column_add_comment();

            AssertSql(
                @"DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Some comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';");
        }

        [ConditionalFact]
        public virtual async Task Alter_computed_column_add_comment()
        {
            await Test(
                builder => builder.Entity("People", x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("SomeColumn").HasComputedColumnSql("42");
                }),
                builder => { },
                builder => builder.Entity("People").Property<int>("SomeColumn").HasComment("Some comment"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns.Where(c => c.Name == "SomeColumn"));
                    Assert.Equal("Some comment", column.Comment);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [SomeColumn];
ALTER TABLE [People] ADD [SomeColumn] AS 42;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Some comment';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'SomeColumn';");
        }

        [ConditionalFact]
        public override async Task Alter_column_change_comment()
        {
            await base.Alter_column_change_comment();

            AssertSql(
                @"DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
SET @description = N'Some comment2';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';");
        }

        [ConditionalFact]
        public override async Task Alter_column_remove_comment()
        {
            await base.Alter_column_remove_comment();

            AssertSql(
                @"DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';");
        }

        [ConditionalFact]
        public override async Task Alter_column_set_collation()
        {
            await base.Alter_column_set_collation();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(max) COLLATE German_PhoneBook_CI_AS NULL;");
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
                @"DROP INDEX [IX_People_Name] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) COLLATE German_PhoneBook_CI_AS NULL;
CREATE INDEX [IX_People_Name] ON [People] ([Name]);");
        }

        [ConditionalFact]
        public override async Task Alter_column_reset_collation()
        {
            await base.Alter_column_reset_collation();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(max) NULL;");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                });

            AssertSql(
                @"DROP INDEX [IX_People_SomeColumn] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(450) NOT NULL;
ALTER TABLE [People] ADD DEFAULT N'' FOR [SomeColumn];
CREATE INDEX [IX_People_SomeColumn] ON [People] ([SomeColumn]) INCLUDE ([SomeOtherColumn]);");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
        public virtual async Task Alter_column_memoryOptimized_with_index()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.IsMemoryOptimized();
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
                @"ALTER TABLE [People] DROP INDEX [IX_People_Name];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(30) NULL;
ALTER TABLE [People] ADD INDEX [IX_People_Name] NONCLUSTERED ([Name]);");
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
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                });

            AssertSql(
                @"DROP INDEX [IX_People_FirstName_LastName] ON [People];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(30) NULL;
CREATE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]) INCLUDE ([Name]);");
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
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'IdentityColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [IdentityColumn] bigint NOT NULL;");
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
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ADD DEFAULT N'Doe' FOR [Name];");
        }

        public override async Task Drop_column()
        {
            await base.Drop_column();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [SomeColumn];");
        }

        public override async Task Drop_column_primary_key()
        {
            await base.Drop_column_primary_key();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [PK_People];",
                //
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [Id];");
        }

        public override async Task Rename_column()
        {
            await base.Rename_column();

            AssertSql(
                @"EXEC sp_rename N'[People].[SomeColumn]', N'SomeOtherColumn', N'COLUMN';");
        }

        public override async Task Create_index()
        {
            await base.Create_index();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;",
                //
                @"CREATE INDEX [IX_People_FirstName] ON [People] ([FirstName]);");
        }

        public override async Task Create_index_unique()
        {
            await base.Create_index_unique();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'LastName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [LastName] nvarchar(450) NULL;",
                //
                @"DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]);");
        }

        public override async Task Create_index_with_filter()
        {
            await base.Create_index_with_filter();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;",
                //
                @"CREATE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;");
        }

        public override async Task Create_unique_index_with_filter()
        {
            await base.Create_unique_index_with_filter();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL AND [Name] <> '';");
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
                    Assert.True((bool)index[SqlServerAnnotationNames.Clustered]);
                    Assert.False(index.IsUnique);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;",
                //
                @"CREATE CLUSTERED INDEX [IX_People_FirstName] ON [People] ([FirstName]);");
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
                    Assert.True((bool)index[SqlServerAnnotationNames.Clustered]);
                    Assert.True(index.IsUnique);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [FirstName] nvarchar(450) NULL;",
                //
                @"CREATE UNIQUE CLUSTERED INDEX [IX_People_FirstName] ON [People] ([FirstName]);");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;",
                //
                @"CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;",
                //
                @"CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL;");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL;");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                    // TODO: Online index not scaffolded?
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL WITH (ONLINE = ON);");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                    // TODO: Online index not scaffolded?
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL WITH (FILLFACTOR = 90, ONLINE = ON);");
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
                    var includedColumns = (IReadOnlyList<string>)index[SqlServerAnnotationNames.Include];
                    Assert.Null(includedColumns);
                    // TODO: Online index not scaffolded?
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NOT NULL;",
                //
                @"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL WITH (FILLFACTOR = 90);");
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
                        e.IsMemoryOptimized().HasKey("Id").IsClustered(false);
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
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;",
                //
                @"ALTER TABLE [People] ADD INDEX [IX_People_Name] ([Name]);");
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
                        e.IsMemoryOptimized().HasKey("Id").IsClustered(false);
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
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Name] nvarchar(450) NULL;",
                //
                @"ALTER TABLE [People] ADD INDEX [IX_People_Name] ([Name]);");
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
                        e.IsMemoryOptimized().HasKey("Name").IsClustered(false);
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
                @"ALTER TABLE [People] ADD INDEX [IX_People_Name] UNIQUE NONCLUSTERED ([Name]);");
        }

        public override async Task Drop_index()
        {
            await base.Drop_index();

            AssertSql(
                @"DROP INDEX [IX_People_SomeField] ON [People];");
        }

        public override async Task Rename_index()
        {
            await base.Rename_index();

            AssertSql(
                @"EXEC sp_rename N'[People].[Foo]', N'foo', N'INDEX';");
        }

        public override async Task Add_primary_key()
        {
            await base.Add_primary_key();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY ([SomeField]);");
        }

        public override async Task Add_primary_key_with_name()
        {
            await base.Add_primary_key_with_name();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [PK_Foo] PRIMARY KEY ([SomeField]);");
        }

        public override async Task Add_primary_key_composite_with_name()
        {
            await base.Add_primary_key_composite_with_name();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [PK_Foo] PRIMARY KEY ([SomeField1], [SomeField2]);");
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
                    Assert.False((bool)primaryKey![SqlServerAnnotationNames.Clustered]);
                });

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY NONCLUSTERED ([SomeField]);");
        }

        public override async Task Drop_primary_key()
        {
            await base.Drop_primary_key();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [PK_People];");
        }

        public override async Task Add_foreign_key()
        {
            await base.Add_foreign_key();

            AssertSql(
                @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION;");
        }

        public override async Task Add_foreign_key_with_name()
        {
            await base.Add_foreign_key_with_name();

            AssertSql(
                @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Foo] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION;");
        }

        public override async Task Drop_foreign_key()
        {
            await base.Drop_foreign_key();

            AssertSql(
                @"ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Customers_CustomerId];");
        }

        public override async Task Add_unique_constraint()
        {
            await base.Add_unique_constraint();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [AK_People_AlternateKeyColumn] UNIQUE ([AlternateKeyColumn]);");
        }

        public override async Task Add_unique_constraint_composite_with_name()
        {
            await base.Add_unique_constraint_composite_with_name();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [AK_Foo] UNIQUE ([AlternateKeyColumn1], [AlternateKeyColumn2]);");
        }

        public override async Task Drop_unique_constraint()
        {
            await base.Drop_unique_constraint();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [AK_People_AlternateKeyColumn];");
        }

        public override async Task Add_check_constraint_with_name()
        {
            await base.Add_check_constraint_with_name();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [CK_Foo] CHECK ([DriverLicense] > 0);");
        }

        public override async Task Alter_check_constraint()
        {
            await base.Alter_check_constraint();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [CK_Foo];",
                //
                @"ALTER TABLE [People] ADD CONSTRAINT [CK_Foo] CHECK ([DriverLicense] > 1);");
        }

        public override async Task Drop_check_constraint()
        {
            await base.Drop_check_constraint();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [CK_Foo];");
        }

        public override async Task Create_sequence()
        {
            await base.Create_sequence();

            AssertSql(
                @"CREATE SEQUENCE [TestSequence] AS int START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;");
        }

        public override async Task Create_sequence_all_settings()
        {
            await base.Create_sequence_all_settings();

            AssertSql(
                @"IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');",
                //
                @"CREATE SEQUENCE [dbo2].[TestSequence] START WITH 3 INCREMENT BY 2 MINVALUE 2 MAXVALUE 916 CYCLE;");
        }

        public override async Task Alter_sequence_all_settings()
        {
            await base.Alter_sequence_all_settings();

            AssertSql(
                @"ALTER SEQUENCE [foo] INCREMENT BY 2 MINVALUE -5 MAXVALUE 10 CYCLE;",
                //
                @"ALTER SEQUENCE [foo] RESTART WITH -3;");
        }

        public override async Task Alter_sequence_increment_by()
        {
            await base.Alter_sequence_increment_by();

            AssertSql(
                @"ALTER SEQUENCE [foo] INCREMENT BY 2 NO MINVALUE NO MAXVALUE NO CYCLE;");
        }

        public override async Task Drop_sequence()
        {
            await base.Drop_sequence();

            AssertSql(
                @"DROP SEQUENCE [TestSequence];");
        }

        public override async Task Rename_sequence()
        {
            await base.Rename_sequence();

            AssertSql(
                @"EXEC sp_rename N'[TestSequence]', N'testsequence';");
        }

        public override async Task Move_sequence()
        {
            await base.Move_sequence();

            AssertSql(
                @"IF SCHEMA_ID(N'TestSequenceSchema') IS NULL EXEC(N'CREATE SCHEMA [TestSequenceSchema];');",
                //
                @"ALTER SCHEMA [TestSequenceSchema] TRANSFER [TestSequence];");
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
                @"DECLARE @defaultSchema sysname = SCHEMA_NAME();
EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [TestSequenceSchema].[TestSequence];');");
        }

        public override async Task InsertDataOperation()
        {
            await base.InsertDataOperation();

            AssertSql(
                @"IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Person]'))
    SET IDENTITY_INSERT [Person] ON;
INSERT INTO [Person] ([Id], [Name])
VALUES (1, N'Daenerys Targaryen'),
(2, N'John Snow'),
(3, N'Arya Stark'),
(4, N'Harry Strickland'),
(5, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Person]'))
    SET IDENTITY_INSERT [Person] OFF;");
        }

        public override async Task DeleteDataOperation_simple_key()
        {
            await base.DeleteDataOperation_simple_key();

            // TODO remove rowcount
            AssertSql(
                @"DELETE FROM [Person]
WHERE [Id] = 2;
SELECT @@ROWCOUNT;");
        }

        public override async Task DeleteDataOperation_composite_key()
        {
            await base.DeleteDataOperation_composite_key();

            // TODO remove rowcount
            AssertSql(
                @"DELETE FROM [Person]
WHERE [AnotherId] = 12 AND [Id] = 2;
SELECT @@ROWCOUNT;");
        }

        public override async Task UpdateDataOperation_simple_key()
        {
            await base.UpdateDataOperation_simple_key();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE [Person] SET [Name] = N'Another John Snow'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;");
        }

        public override async Task UpdateDataOperation_composite_key()
        {
            await base.UpdateDataOperation_composite_key();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE [Person] SET [Name] = N'Another John Snow'
WHERE [AnotherId] = 11 AND [Id] = 2;
SELECT @@ROWCOUNT;");
        }

        public override async Task UpdateDataOperation_multiple_columns()
        {
            await base.UpdateDataOperation_multiple_columns();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE [Person] SET [Age] = 21, [Name] = N'Another John Snow'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;");
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

        protected override ReferentialAction Normalize(ReferentialAction value)
            => value == ReferentialAction.Restrict
                ? ReferentialAction.NoAction
                : value;

        public class MigrationsSqlServerFixture : MigrationsFixtureBase
        {
            protected override string StoreName { get; } = nameof(MigrationsSqlServerTest);

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            public override TestHelpers TestHelpers
                => SqlServerTestHelpers.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, SqlServerDatabaseModelFactory>();
        }
    }
}

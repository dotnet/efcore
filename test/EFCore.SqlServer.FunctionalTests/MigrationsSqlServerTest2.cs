// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsSqlServerTest2 : MigrationsTestBase2<MigrationsSqlServerTest2.MigrationsSqlServerFixture2>
    {
        public MigrationsSqlServerTest2(MigrationsSqlServerFixture2 fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task CreateIndexOperation_with_filter_where_clause()
        {
            await base.CreateIndexOperation_with_filter_where_clause();

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

        public override async Task CreateIndexOperation_with_filter_where_clause_and_is_unique()
        {
            await base.CreateIndexOperation_with_filter_where_clause_and_is_unique();

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

        public override async Task AddColumnOperation_with_defaultValue()
        {
            await base.AddColumnOperation_with_defaultValue();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'John Doe';");
        }

        public override async Task AddColumnOperation_with_defaultValueSql()
        {
            await base.AddColumnOperation_with_defaultValueSql();

            AssertSql(
                @"ALTER TABLE [People] ADD [Birthday] date NULL DEFAULT (CURRENT_TIMESTAMP);");
        }

        public override async Task AddColumnOperation_without_column_type()
        {
            await base.AddColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';");
        }

        public override async Task AddColumnOperation_with_ansi()
        {
            await base.AddColumnOperation_with_ansi();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] varchar(max) NULL;");
        }

        // TODO: AddColumnOperation_with_unicode_overridden. In which scenarios do we need to do this?

        // TODO: AddColumnOperation_with_unicode_no_model. In which scenarios do we need to do this?

        public override async Task AddColumnOperation_with_fixed_length()
        {
            await base.AddColumnOperation_with_fixed_length();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(max) NULL;");
        }

        // TODO: AddColumnOperation_with_fixed_length_no_model

        public override async Task AddColumnOperation_with_maxLength()
        {
            await base.AddColumnOperation_with_maxLength();

            AssertSql(
                @"ALTER TABLE [People] ADD [Name] nvarchar(30) NULL;");
        }

        public override async Task AddColumnOperation_with_maxLength_on_derived()
        {
            await base.AddColumnOperation_with_maxLength_on_derived();

            Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
        }

        public override async Task AddColumnOperation_with_shared_column()
        {
            await base.AddColumnOperation_with_shared_column();

            AssertSql(
                @"ALTER TABLE [Base] ADD [Foo] nvarchar(max) NULL;");
        }

        public override async Task AddForeignKeyOperation()
        {
            await base.AddForeignKeyOperation();

            AssertSql(
                @"CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);",
                //
                @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE;");
        }

        public override async Task AddForeignKeyOperation_with_name()
        {
            await base.AddForeignKeyOperation_with_name();

            AssertSql(
                @"CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);",
                //
                @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Foo] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE;");
        }

        // TODO: AddForeignKeyOperation_without_principal_columns, how to generate the scenario via model diffing

        public override async Task AddPrimaryKeyOperation()
        {
            await base.AddPrimaryKeyOperation();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeField');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeField] nvarchar(450) NOT NULL;",
                //
                @"ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY ([SomeField]);");
        }

        public override async Task AddPrimaryKeyOperation_composite_with_name()
        {
            await base.AddPrimaryKeyOperation_composite_with_name();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeField2');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeField2] nvarchar(450) NOT NULL;",
                //
                @"DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeField1');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeField1] nvarchar(450) NOT NULL;",
                //
                @"ALTER TABLE [People] ADD CONSTRAINT [PK_Foo] PRIMARY KEY ([SomeField1], [SomeField2]);");
        }

        public override async Task AddUniqueConstraintOperation()
        {
            await base.AddUniqueConstraintOperation();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [AK_People_AlternateKeyColumn] UNIQUE ([AlternateKeyColumn]);");
        }

        public override async Task AddUniqueConstraintOperation_composite_with_name()
        {
            await base.AddUniqueConstraintOperation_composite_with_name();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [AK_Foo] UNIQUE ([AlternateKeyColumn1], [AlternateKeyColumn2]);");
        }

        public override async Task CreateCheckConstraintOperation_with_name()
        {
            await base.CreateCheckConstraintOperation_with_name();

            AssertSql(
                @"ALTER TABLE [People] ADD CONSTRAINT [CK_Foo] CHECK ([DriverLicense] > 0);");
        }

        public override async Task AlterColumnOperation_name()
        {
            await base.AlterColumnOperation_name();

            AssertSql(
                @"EXEC sp_rename N'[People].[SomeColumn]', N'somecolumn', N'COLUMN';");
        }

        public override async Task AlterColumnOperation_type()
        {
            await base.AlterColumnOperation_type();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] bigint NOT NULL;");
        }

        public override async Task AlterColumnOperation_required()
        {
            await base.AlterColumnOperation_required();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [SomeColumn] nvarchar(max) NOT NULL;");
        }

        public override async Task AlterSequenceOperation_all_settings()
        {
            await base.AlterSequenceOperation_all_settings();

            AssertSql(
                @"ALTER SEQUENCE [foo] INCREMENT BY 2 MINVALUE -5 MAXVALUE 10 CYCLE;",
                //
                @"ALTER SEQUENCE [foo] RESTART WITH -3;");
        }

        public override async Task AlterSequenceOperation_increment_by()
        {
            await base.AlterSequenceOperation_increment_by();

            AssertSql(
                @"ALTER SEQUENCE [foo] INCREMENT BY 2 NO MINVALUE NO MAXVALUE NO CYCLE;");
        }

        public override async Task RenameTableOperation()
        {
            await base.RenameTableOperation();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [PK_People];",
                //
                @"EXEC sp_rename N'[People]', N'people';",
                //
                @"ALTER TABLE [people] ADD CONSTRAINT [PK_people] PRIMARY KEY ([Id]);");
        }

        public override async Task RenameTableOperation_schema()
        {
            await base.RenameTableOperation_schema();

            AssertSql(
                @"IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');",
                //
                @"ALTER SCHEMA [dbo2] TRANSFER [dbo].[People];");
        }

        // TODO: RenameTableOperation_legacy

        public override async Task CreateIndexOperation()
        {
            await base.CreateIndexOperation();

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

        public override async Task CreateIndexOperation_unique()
        {
            await base.CreateIndexOperation_unique();

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
                @"CREATE UNIQUE INDEX [IX_People_FirstName_LastName] ON [People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL;");
        }

        public override async Task CreateIndexOperation_with_where_clauses()
        {
            await base.CreateIndexOperation_with_where_clauses();

            AssertSql(
                @"CREATE INDEX [IX_People_Age] ON [People] ([Age]) WHERE [Age] > 18;");
        }

        public override async Task CreateSequenceOperation_all_settings()
        {
            await base.CreateSequenceOperation_all_settings();

            AssertSql(
                @"IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');",
                //
                @"CREATE SEQUENCE [dbo2].[TestSequence] START WITH 3 INCREMENT BY 2 MINVALUE 2 MAXVALUE 916 CYCLE;");
        }

        public override async Task CreateTableOperation_all_settings()
        {
            await base.CreateTableOperation_all_settings();

            AssertSql(
                @"IF SCHEMA_ID(N'dbo2') IS NULL EXEC(N'CREATE SCHEMA [dbo2];');",
                //
                @"CREATE TABLE [dbo2].[People] (
    [CustomId] int NOT NULL IDENTITY,
    [EmployerId] int NOT NULL,
    [SSN] char(11) NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([CustomId]),
    CONSTRAINT [AK_People_SSN] UNIQUE ([SSN]),
    CONSTRAINT [CK_SSN] CHECK ([SSN] > 0),
    CONSTRAINT [FK_People_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([Id]) ON DELETE CASCADE
);
EXEC sp_addextendedproperty 'MS_Description', N'Employer ID comment', 'SCHEMA', N'dbo2', 'TABLE', N'People', 'COLUMN', N'EmployerId';",
                //
                @"CREATE INDEX [IX_People_EmployerId] ON [dbo2].[People] ([EmployerId]);");
        }

        public override async Task CreateTableOperation_no_key()
        {
            await base.CreateTableOperation_no_key();

            AssertSql(
                @"CREATE TABLE [Anonymous] (
    [SomeColumn] int NOT NULL
);");
        }

        public override async Task DropColumnOperation()
        {
            await base.DropColumnOperation();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'SomeColumn');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [SomeColumn];");
        }

        public override async Task DropForeignKeyOperation()
        {
            await base.DropForeignKeyOperation();

            AssertSql(
                @"ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Customers_CustomerId];",
                //
                @"DROP INDEX [IX_Orders_CustomerId] ON [Orders];");
        }

        public override async Task DropIndexOperation()
        {
            await base.DropIndexOperation();

            AssertSql(
                @"DROP INDEX [IX_People_SomeField] ON [People];");
        }

        public override async Task DropPrimaryKeyOperation()
        {
            await base.DropPrimaryKeyOperation();

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

        public override async Task DropSequenceOperation()
        {
            await base.DropSequenceOperation();

            AssertSql(
                @"DROP SEQUENCE [TestSequence];");
        }

        public override async Task DropTableOperation()
        {
            await base.DropTableOperation();

            AssertSql(
                @"DROP TABLE [People];");
        }

        public override async Task DropUniqueConstraintOperation()
        {
            await base.DropUniqueConstraintOperation();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [AK_People_AlternateKeyColumn];");
        }

        public override async Task DropCheckConstraintOperation()
        {
            await base.DropCheckConstraintOperation();

            AssertSql(
                @"ALTER TABLE [People] DROP CONSTRAINT [CK_Foo];");
        }

        // TODO: SqlOperation

        // TODO: Data tests

        public class MigrationsSqlServerFixture2 : MigrationsFixtureBase2
        {
            protected override string StoreName { get; } = nameof(MigrationsSqlServerTest2);
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public override TestHelpers TestHelpers => SqlServerTestHelpers.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, SqlServerDatabaseModelFactory>();
        }
    }
}

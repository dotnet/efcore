// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            AssertSql(
                @"CREATE TABLE [dbo].[People] (
    [Id] int NOT NULL,
    [EmployerId] int NULL,
    [SSN] char(11) NULL,
    PRIMARY KEY ([Id]),
    UNIQUE ([SSN]),
    CHECK (SSN > 0),
    FOREIGN KEY ([EmployerId]) REFERENCES [Companies] ([Id])
);
EXEC sp_addextendedproperty 'MS_Description', N'Table comment', 'SCHEMA', N'dbo', 'TABLE', N'People';
EXEC sp_addextendedproperty 'MS_Description', N'Employer ID comment', 'SCHEMA', N'dbo', 'TABLE', N'People', 'COLUMN', N'EmployerId';
");
        }

        [ConditionalFact]
        public void CreateTableOperation_default_schema_with_comments()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "People",
                            ClrType = typeof(int),
                            IsNullable = false,
                            Comment = "ID comment"
                        },
                        new AddColumnOperation
                        {
                            Name = "Name",
                            Table = "People",
                            ClrType = typeof(string),
                            IsNullable = false,
                            Comment = "Name comment"
                        },
                    },
                    Comment = "Table comment"
                });

            AssertSql(
                @"CREATE TABLE [People] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NOT NULL
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
EXEC sp_addextendedproperty 'MS_Description', N'Table comment', 'SCHEMA', @defaultSchema, 'TABLE', N'People';
EXEC sp_addextendedproperty 'MS_Description', N'ID comment', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'Name comment', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';
");
        }

        [ConditionalFact]
        public void CreateTableOperation_default_schema_with_column_comments()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "People",
                            ClrType = typeof(int),
                            IsNullable = false,
                            Comment = "ID comment"
                        },
                        new AddColumnOperation
                        {
                            Name = "Name",
                            Table = "People",
                            ClrType = typeof(string),
                            IsNullable = false,
                            Comment = "Name comment"
                        },
                    }
                });

            AssertSql(
                @"CREATE TABLE [People] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NOT NULL
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
EXEC sp_addextendedproperty 'MS_Description', N'ID comment', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'Name comment', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'Name';
");
        }

        public override void CreateTableOperation_no_key()
        {
            base.CreateTableOperation_no_key();

            AssertSql(
                @"CREATE TABLE [Anonymous] (
    [Value] int NOT NULL
);
");
        }

        public override void CreateIndexOperation_with_filter_where_clause()
        {
            base.CreateIndexOperation_with_filter_where_clause();

            AssertSql(@"CREATE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;
");
        }

        public override void CreateIndexOperation_with_filter_where_clause_and_is_unique()
        {
            base.CreateIndexOperation_with_filter_where_clause_and_is_unique();

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL AND <> '';
");
        }

        [ConditionalFact]
        public void AlterTableOperation_with_new_comment()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    Comment = "My Comment"
                });

            AssertSql(
                @"EXEC sp_addextendedproperty 'MS_Description', N'My Comment', 'SCHEMA', N'dbo', 'TABLE', N'People';
");
        }

        [ConditionalFact]
        public void AlterTableOperation_with_different_comment_to_existing()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.HasComment("My Comment");
                        }),
                new AlterTableOperation
                {
                    Schema = "dbo",
                    Name = "People",
                    Comment = "My Comment 2",
                    OldTable = new TableOperation
                    {
                        Comment = "My Comment"
                    }
                });

            AssertSql(
                @"EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'People';
EXEC sp_addextendedproperty 'MS_Description', N'My Comment 2', 'SCHEMA', N'dbo', 'TABLE', N'People';
");
        }

        [ConditionalFact]
        public void AlterTableOperation_removing_comment()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.HasComment("My Comment");
                        }),
                new AlterTableOperation
                {
                    Schema = "dbo",
                    Name = "People",
                    OldTable = new TableOperation
                    {
                        Comment = "My Comment"
                    }
                });

            AssertSql(
                @"EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'People';
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_computedSql()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    ComputedColumnSql = "FirstName + ' ' + LastName"
                });

            AssertSql(@"ALTER TABLE [People] ADD [FullName] AS FirstName + ' ' + LastName;
");
        }

        [ConditionalFact]
        public void AddColumnOperation_with_computed_column_SQL()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "date",
                    IsNullable = true,
                    ComputedColumnSql = "CURRENT_TIMESTAMP"
                });

            AssertSql(@"ALTER TABLE [People] ADD [Birthday] AS CURRENT_TIMESTAMP;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_identity()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    DefaultValue = 0,
                    IsNullable = false,
                    [SqlServerAnnotationNames.Identity] = "1, 1"
                });

            AssertSql(@"ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_identity_legacy()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    DefaultValue = 0,
                    IsNullable = false,
                    [SqlServerAnnotationNames.ValueGenerationStrategy] =
                        SqlServerValueGenerationStrategy.IdentityColumn
                });

            AssertSql(@"ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;
");
        }
        [ConditionalFact]
        public virtual void AddColumnOperation_identity_seed_increment()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    DefaultValue = 0,
                    IsNullable = false,
                    [SqlServerAnnotationNames.Identity] = "100,5"
                });

            AssertSql(@"ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY(100,5);
");
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(@"ALTER TABLE [People] ADD [Alias] nvarchar(max) NOT NULL;
");
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            base.AddColumnOperation_with_unicode_no_model();

            AssertSql(@"ALTER TABLE [Person] ADD [Name] varchar(max) NULL;
");
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            AssertSql(@"ALTER TABLE [Person] ADD [Name] nvarchar(30) NULL;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_datetime_with_defaultValue()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "datetime",
                    IsNullable = false,
                    DefaultValue = new DateTime(2019, 1, 1)
                });

            AssertSql(@"ALTER TABLE [dbo].[People] ADD [Birthday] datetime NOT NULL DEFAULT '2019-01-01T00:00:00.000';
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_smalldatetime_with_defaultValue()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "smalldatetime",
                    IsNullable = false,
                    DefaultValue = new DateTime(2019, 1, 1)
                });

            AssertSql(@"ALTER TABLE [dbo].[People] ADD [Birthday] smalldatetime NOT NULL DEFAULT '2019-01-01T00:00:00';
");
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            AssertSql(@"ALTER TABLE [Person] ADD [Name] nvarchar(32) NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            AssertSql(@"ALTER TABLE [Person] ADD [Name] nvarchar(30) NULL;
");
        }

        public override void AddColumnOperation_with_ansi()
        {
            base.AddColumnOperation_with_ansi();

            AssertSql(@"ALTER TABLE [Person] ADD [Name] varchar(max) NULL;
");
        }

        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            AssertSql(@"ALTER TABLE [Person] ADD [Name] nvarchar(max) NULL;
");
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            AssertSql(@"ALTER TABLE [Base] ADD [Foo] nvarchar(max) NULL;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_rowversion_overridden()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<byte[]>("RowVersion"),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "RowVersion",
                    ClrType = typeof(byte[]),
                    IsRowVersion = true,
                    IsNullable = true
                });

            AssertSql(@"ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_rowversion_no_model()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "RowVersion",
                    ClrType = typeof(byte[]),
                    IsRowVersion = true,
                    IsNullable = true
                });

            AssertSql(@"ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_comment()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    Comment = "My comment"
                });

            AssertSql(
                @"ALTER TABLE [People] ADD [FullName] nvarchar(max) NOT NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
EXEC sp_addextendedproperty 'MS_Description', N'My comment', 'SCHEMA', @defaultSchema, 'TABLE', N'People', 'COLUMN', N'FullName';
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_comment_non_default_schema()
        {
            Generate(
                new AddColumnOperation
                {
                    Schema = "my",
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    Comment = "My comment"
                });

            AssertSql(
                @"ALTER TABLE [my].[People] ADD [FullName] nvarchar(max) NOT NULL;
EXEC sp_addextendedproperty 'MS_Description', N'My comment', 'SCHEMA', N'my', 'TABLE', N'People', 'COLUMN', N'FullName';
");
        }

        [ConditionalFact]
        public virtual void AddPrimaryKeyOperation_nonclustered()
        {
            Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "Id" },
                    [SqlServerAnnotationNames.Clustered] = false
                });

            AssertSql(@"ALTER TABLE [People] ADD PRIMARY KEY NONCLUSTERED ([Id]);
");
        }

        public override void AlterColumnOperation()
        {
            base.AlterColumnOperation();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'LuckyNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [dbo].[People] ALTER COLUMN [LuckyNumber] int NOT NULL;
ALTER TABLE [dbo].[People] ADD DEFAULT 7 FOR [LuckyNumber];
");
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'LuckyNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [LuckyNumber] int NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_identity()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [SqlServerAnnotationNames.Identity] = "1, 1"
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Id] int NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_identity_legacy()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [SqlServerAnnotationNames.ValueGenerationStrategy] =
                        SqlServerValueGenerationStrategy.IdentityColumn
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Id] int NOT NULL;
");
        }

        [ConditionalFact]
        public void AlterColumnOperation_computed()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    ComputedColumnSql = "[FirstName] + ' ' + [LastName]"
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FullName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] DROP COLUMN [FullName];
ALTER TABLE [People] ADD [FullName] AS [FirstName] + ' ' + [LastName];
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_computed_with_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("FullName").HasComputedColumnSql("[FirstName] + ' ' + [LastName]");
                            x.HasKey("FullName");
                            x.HasIndex("FullName");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "FullName",
                    ClrType = typeof(string),
                    ComputedColumnSql = "[FirstName] + ' ' + [LastName]",
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        ComputedColumnSql = "[LastName] + ', ' + [FirstName]"
                    }
                });

            AssertSql(
                @"DROP INDEX [IX_Person_FullName] ON [Person];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'FullName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] DROP COLUMN [FullName];
ALTER TABLE [Person] ADD [FullName] AS [FirstName] + ' ' + [LastName];
CREATE INDEX [IX_Person_FullName] ON [Person] ([FullName]);
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void AlterColumnOperation_memoryOptimized_with_index(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            if (obsolete)
                            {
#pragma warning disable 618
                                x.ForSqlServerIsMemoryOptimized();
#pragma warning restore 618
                            }
                            else
                            {
                                x.IsMemoryOptimized();
                            }
                            x.Property<string>("Name");
                            x.HasKey("Name");
                            x.HasIndex("Name");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string)
                    }
                });

            AssertSql(
                @"ALTER TABLE [Person] DROP INDEX [IX_Person_Name];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NOT NULL;
ALTER TABLE [Person] ADD INDEX [IX_Person_Name] NONCLUSTERED ([Name]);
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_index_no_narrowing()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name");
                            x.HasKey("Name");
                            x.HasIndex("Name");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsNullable = true,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = false
                    }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(450) NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);
                            x.HasKey("Name");
                            x.HasIndex("Name");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true
                    }
                });

            AssertSql(
                @"DROP INDEX [IX_Person_Name] ON [Person];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void AlterColumnOperation_with_index_included_column(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);
                            x.Property<string>("FirstName");
                            x.Property<string>("LastName");
                            x.HasKey("Name");

                            if (obsolete)
                            {
#pragma warning disable 618
                                x.HasIndex("FirstName", "LastName").ForSqlServerInclude("Name");
#pragma warning restore 618
                            }
                            else
                            {
                                x.HasIndex("FirstName", "LastName").IncludeProperties("Name");
                            }
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true
                    }
                });

            AssertSql(
                @"DROP INDEX [IX_Person_FirstName_LastName] ON [Person];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
CREATE INDEX [IX_Person_FirstName_LastName] ON [Person] ([FirstName], [LastName]) INCLUDE ([Name]);
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_index_no_included()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);
                            x.Property<string>("FirstName");
                            x.Property<string>("LastName");
                            x.HasIndex("FirstName", "LastName");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true
                    }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_index_no_oldColumn()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.0.0-rtm")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);
                            x.HasIndex("Name");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true,
                    OldColumn = new ColumnOperation()
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_composite_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("FirstName").IsRequired();
                            x.Property<string>("LastName");
                            x.HasKey("LastName");
                            x.HasIndex("FirstName", "LastName");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "FirstName",
                    ClrType = typeof(string),
                    IsNullable = false,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true
                    }
                });

            AssertSql(
                @"DROP INDEX [IX_Person_FirstName_LastName] ON [Person];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'FirstName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [FirstName] nvarchar(450) NOT NULL;
CREATE INDEX [IX_Person_FirstName_LastName] ON [Person] ([FirstName], [LastName]);
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_with_added_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);
                            x.HasIndex("Name");
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true
                    }
                },
                new CreateIndexOperation
                {
                    Name = "IX_Person_Name",
                    Table = "Person",
                    Columns = new[] { "Name" }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
GO

CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void AlterColumnOperation_with_added_online_index(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);

                            if (obsolete)
                            {
#pragma warning disable 618
                                x.HasIndex("Name").ForSqlServerIsCreatedOnline();
#pragma warning restore 618
                            }
                            else
                            {
                                x.HasIndex("Name").IsCreatedOnline();
                            }
                        }),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true
                    }
                },
                new CreateIndexOperation
                {
                    Name = "IX_Person_Name",
                    Table = "Person",
                    Columns = new[] { "Name" },
                    [SqlServerAnnotationNames.CreatedOnline] = true
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
GO

CREATE INDEX [IX_Person_Name] ON [Person] ([Name]) WITH (ONLINE = ON);
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_identity()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0"),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Id",
                    ClrType = typeof(long),
                    [SqlServerAnnotationNames.Identity] = "1, 1",
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [SqlServerAnnotationNames.Identity] = "1, 1"
                    }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Id] bigint NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_identity_legacy()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0"),
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Id",
                    ClrType = typeof(long),
                    [SqlServerAnnotationNames.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [SqlServerAnnotationNames.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn
                    }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Id] bigint NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_add_identity()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(
                    modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0"),
                    new AlterColumnOperation
                    {
                        Table = "Person",
                        Name = "Id",
                        ClrType = typeof(int),
                        [SqlServerAnnotationNames.Identity] = "1, 1",
                        OldColumn = new ColumnOperation
                        {
                            ClrType = typeof(int)
                        }
                    }));

            Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_add_identity_legacy()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(
                    modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0"),
                    new AlterColumnOperation
                    {
                        Table = "Person",
                        Name = "Id",
                        ClrType = typeof(int),
                        [SqlServerAnnotationNames.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn,
                        OldColumn = new ColumnOperation
                        {
                            ClrType = typeof(int)
                        }
                    }));

            Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_remove_identity()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(
                    modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0"),
                    new AlterColumnOperation
                    {
                        Table = "Person",
                        Name = "Id",
                        ClrType = typeof(int),
                        OldColumn = new ColumnOperation
                        {
                            ClrType = typeof(int),
                            [SqlServerAnnotationNames.Identity] = "1, 1"
                        }
                    }));

            Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
        }

        [ConditionalFact]
        public virtual void AlterColumnOperation_remove_identity_legacy()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(
                    modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0"),
                    new AlterColumnOperation
                    {
                        Table = "Person",
                        Name = "Id",
                        ClrType = typeof(int),
                        OldColumn = new ColumnOperation
                        {
                            ClrType = typeof(int),
                            [SqlServerAnnotationNames.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn
                        }
                    }));

            Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
        }

        [ConditionalFact]
        public void AlterColumnOperation_with_new_comment()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "LuckyNumber",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    Comment = "My Comment"
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'LuckyNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [dbo].[People] ALTER COLUMN [LuckyNumber] int NOT NULL;
EXEC sp_addextendedproperty 'MS_Description', N'My Comment', 'SCHEMA', N'dbo', 'TABLE', N'People', 'COLUMN', N'LuckyNumber';
");
        }

        [ConditionalFact]
        public void AlterColumnOperation_with_different_comment_to_existing()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasComment("My Comment");
                        }),
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsNullable = false,
                    Comment = "My Comment 2",
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true,
                        Comment = "My Comment"
                    }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [dbo].[People] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'People', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'My Comment 2', 'SCHEMA', N'dbo', 'TABLE', N'People', 'COLUMN', N'Name';
");
        }

        [ConditionalFact]
        public void AlterColumnOperation_removing_comment()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasComment("My Comment");
                        }),
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsNullable = false,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(string),
                        IsNullable = true,
                        Comment = "My Comment"
                    }
                });

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [dbo].[People] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', N'dbo', 'TABLE', N'People', 'COLUMN', N'Name';
");
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(
                new SqlServerCreateDatabaseOperation
                {
                    Name = "Northwind"
                });

            AssertSql(
                @"CREATE DATABASE [Northwind];
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
");
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation_with_filename()
        {
            Generate(
                new SqlServerCreateDatabaseOperation
                {
                    Name = "Northwind",
                    FileName = "Narf.mdf"
                });

            var expectedFile = Path.GetFullPath("Narf.mdf");
            var expectedLog = Path.GetFullPath("Narf_log.ldf");

            AssertSql(
                $@"CREATE DATABASE [Northwind]
ON (NAME = N'Narf', FILENAME = N'{expectedFile}')
LOG ON (NAME = N'Narf_log', FILENAME = N'{expectedLog}');
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
");
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation_with_filename_and_datadirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Generate(
                new SqlServerCreateDatabaseOperation
                {
                    Name = "Northwind",
                    FileName = "|DataDirectory|Narf.mdf"
                });

            var expectedFile = Path.Combine(baseDirectory, "Narf.mdf");
            var expectedLog = Path.Combine(baseDirectory, "Narf_log.ldf");

            AssertSql(
                $@"CREATE DATABASE [Northwind]
ON (NAME = N'Narf', FILENAME = N'{expectedFile}')
LOG ON (NAME = N'Narf_log', FILENAME = N'{expectedLog}');
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
");
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation_with_filename_and_custom_datadirectory()
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);

            Generate(
                new SqlServerCreateDatabaseOperation
                {
                    Name = "Northwind",
                    FileName = "|DataDirectory|Narf.mdf"
                });

            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            var expectedFile = Path.Combine(dataDirectory, "Narf.mdf");
            var expectedLog = Path.Combine(dataDirectory, "Narf_log.ldf");

            AssertSql(
                $@"CREATE DATABASE [Northwind]
ON (NAME = N'Narf', FILENAME = N'{expectedFile}')
LOG ON (NAME = N'Narf_log', FILENAME = N'{expectedLog}');
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
");
        }

        [ConditionalFact]
        public virtual void AlterDatabaseOperationOperation()
        {
            Generate(
                new AlterDatabaseOperation
                {
                    [SqlServerAnnotationNames.MemoryOptimized] = true
                });

            Assert.Contains(
                "CONTAINS MEMORY_OPTIMIZED_DATA;",
                Sql);
        }

        public override void CreateIndexOperation_nonunique()
        {
            base.CreateIndexOperation_nonunique();

            AssertSql(@"CREATE INDEX [IX_People_Name] ON [People] ([Name]);
");
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_non_legacy()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.0.0"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "dbo",
                    Columns = new[] { "FirstName", "LastName" },
                    IsUnique = true
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]);
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_clustered()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    [SqlServerAnnotationNames.Clustered] = true
                });

            AssertSql(@"CREATE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_clustered()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Clustered] = true
                });

            AssertSql(@"CREATE UNIQUE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_include()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" }
                });

            AssertSql(@"CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_include_and_filter()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    Filter = "[Name] IS NOT NULL AND <> ''",
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" }
                });

            AssertSql(@"CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL AND <> '';
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_with_include()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" }
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_with_include_and_filter()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    Filter = "[Name] IS NOT NULL AND <> ''",
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" }
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL AND <> '';
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_with_include_and_filter_online()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    Filter = "[Name] IS NOT NULL AND <> ''",
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" },
                    [SqlServerAnnotationNames.CreatedOnline] = true
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL AND <> '' WITH (ONLINE = ON);
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_with_include_non_legacy()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.0.0"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" }
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_bound_null()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique_bound_not_null()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People", x =>
                {
                    x.Property<string>("Name").IsRequired();
                    x.HasKey("Name");
                }),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true
                });

            AssertSql(@"CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]);
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nullable(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People", x =>
                {
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.ToTable("People", "dbo").ForSqlServerIsMemoryOptimized().Property<string>("Name");
#pragma warning restore 618
                    }
                    else
                    {
                        x.ToTable("People", "dbo").IsMemoryOptimized().Property<string>("Name");
                    }
                    x.Property<int>("Id");
                    x.HasKey("Id");
                }),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Schema = "dbo",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true
                });

            AssertSql(@"ALTER TABLE [dbo].[People] ADD INDEX [IX_People_Name] ([Name]);
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nullable_with_filter(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People", x =>
                {
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.ForSqlServerIsMemoryOptimized().Property<string>("Name");
#pragma warning restore 618
                    }
                    else
                    {
                        x.IsMemoryOptimized().Property<string>("Name");
                    }
                    x.Property<int>("Id");
                    x.HasKey("Id");
                }),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    Filter = "[Name] IS NOT NULL AND <> ''"
                });

            AssertSql(@"ALTER TABLE [People] ADD INDEX [IX_People_Name] ([Name]);
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nonclustered_not_nullable(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People", x =>
                {
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.ForSqlServerIsMemoryOptimized().Property<string>("Name").IsRequired();
#pragma warning restore 618
                    }
                    else
                    {
                        x.IsMemoryOptimized().Property<string>("Name").IsRequired();
                    }
                    x.HasKey("Name");
                }),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Clustered] = false
                });

            AssertSql(@"ALTER TABLE [People] ADD INDEX [IX_People_Name] UNIQUE NONCLUSTERED ([Name]);
");
        }

        [ConditionalFact]
        public virtual void CreateSchemaOperation()
        {
            Generate(
                new EnsureSchemaOperation
                {
                    Name = "my"
                });

            AssertSql(@"IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');
");
        }

        [ConditionalFact]
        public virtual void CreateSchemaOperation_dbo()
        {
            Generate(
                new EnsureSchemaOperation
                {
                    Name = "dbo"
                });

            AssertSql("");
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            AssertSql(
                @"DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'LuckyNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [dbo].[People] DROP COLUMN [LuckyNumber];
");
        }

        [ConditionalFact]
        public virtual void DropDatabaseOperation()
        {
            Generate(
                new SqlServerDropDatabaseOperation
                {
                    Name = "Northwind"
                });

            AssertSql(
                @"IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END;
GO

DROP DATABASE [Northwind];
");
        }

        public override void DropIndexOperation()
        {
            base.DropIndexOperation();

            AssertSql(@"DROP INDEX [IX_People_Name] ON [dbo].[People];
");
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void DropIndexOperation_memoryOptimized(bool obsolete)
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People", x =>
                {
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.ForSqlServerIsMemoryOptimized();
#pragma warning restore 618
                    }
                    else
                    {
                        x.IsMemoryOptimized();
                    }
                    x.Property<int>("Id");
                    x.HasKey("Id");
                }),
                new DropIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People"
                });

            AssertSql(@"ALTER TABLE [People] DROP INDEX [IX_People_Name];
");
        }

        [ConditionalFact]
        public virtual void MoveSequenceOperation_legacy()
        {
            Generate(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewSchema = "my"
                });

            AssertSql(@"ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];
");
        }

        [ConditionalFact]
        public virtual void MoveSequenceOperation()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "EntityFrameworkHiLoSequence",
                    NewSchema = "my"
                });

            AssertSql(@"ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];
");
        }

        [ConditionalFact]
        public virtual void MoveSequenceOperation_into_default()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "EntityFrameworkHiLoSequence"
                });

            AssertSql(
                @"DECLARE @defaultSchema sysname = SCHEMA_NAME();
EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [dbo].[EntityFrameworkHiLoSequence];');
");
        }

        [ConditionalFact]
        public virtual void MoveTableOperation_legacy()
        {
            Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewSchema = "hr"
                });

            AssertSql(@"ALTER SCHEMA [hr] TRANSFER [dbo].[People];
");
        }

        [ConditionalFact]
        public virtual void MoveTableOperation()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "People",
                    NewSchema = "hr"
                });

            AssertSql(@"ALTER SCHEMA [hr] TRANSFER [dbo].[People];
");
        }

        [ConditionalFact]
        public virtual void MoveTableOperation_into_default()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "People"
                });

            AssertSql(
                @"DECLARE @defaultSchema sysname = SCHEMA_NAME();
EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [dbo].[People];');
");
        }

        [ConditionalFact]
        public virtual void RenameColumnOperation()
        {
            Generate(
                new RenameColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "Name",
                    NewName = "FullName"
                });

            AssertSql(@"EXEC sp_rename N'[dbo].[People].[Name]', N'FullName', N'COLUMN';
");
        }

        [ConditionalFact]
        public virtual void RenameIndexOperation()
        {
            Generate(
                new RenameIndexOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "IX_People_Name",
                    NewName = "IX_People_FullName"
                });

            AssertSql(@"EXEC sp_rename N'[dbo].[People].[IX_People_Name]', N'IX_People_FullName', N'INDEX';
");
        }

        [ConditionalFact]
        public virtual void RenameIndexOperations_throws_when_no_table()
        {
            var migrationBuilder = new MigrationBuilder("SqlServer");

            migrationBuilder.RenameIndex(
                name: "IX_OldIndex",
                newName: "IX_NewIndex");

            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(migrationBuilder.Operations.ToArray()));

            Assert.Equal(SqlServerStrings.IndexTableRequired, ex.Message);
        }

        [ConditionalFact]
        public virtual void RenameSequenceOperation_legacy()
        {
            Generate(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "MySequence"
                });

            AssertSql(@"EXEC sp_rename N'[dbo].[EntityFrameworkHiLoSequence]', N'MySequence';
");
        }

        [ConditionalFact]
        public virtual void RenameSequenceOperation()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "MySequence",
                    NewSchema = "dbo"
                });

            AssertSql(@"EXEC sp_rename N'[dbo].[EntityFrameworkHiLoSequence]', N'MySequence';
");
        }

        [ConditionalFact]
        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();

            AssertSql(@"EXEC sp_rename N'[dbo].[People]', N'Person';
");
        }

        [ConditionalFact]
        public override void RenameTableOperation()
        {
            base.RenameTableOperation();

            AssertSql(@"EXEC sp_rename N'[dbo].[People]', N'Person';
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_handles_backslash()
        {
            Generate(
                new SqlOperation
                {
                    Sql = @"-- Multiline \" + EOL +
                          "comment"
                });

            AssertSql(@"-- Multiline comment
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_ignores_sequential_gos()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- Ready set" + EOL +
                          "GO" + EOL +
                          "GO"
                });

            AssertSql(@"-- Ready set
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_handles_go()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I" + EOL +
                          "go" + EOL +
                          "-- Too"
                });

            AssertSql(
                @"-- I
GO

-- Too
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_handles_go_with_count()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I" + EOL +
                          "GO 2"
                });

            AssertSql(
                @"-- I
GO

-- I
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_ignores_non_go()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I GO 2"
                });

            AssertSql(@"-- I GO 2
");
        }

        public override void InsertDataOperation()
        {
            base.InsertDataOperation();

            AssertSql(
                @"IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Full Name') AND [object_id] = OBJECT_ID(N'[People]'))
    SET IDENTITY_INSERT [People] ON;
INSERT INTO [People] ([Id], [Full Name])
VALUES (0, NULL),
(1, N'Daenerys Targaryen'),
(2, N'John Snow'),
(3, N'Arya Stark'),
(4, N'Harry Strickland');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Full Name') AND [object_id] = OBJECT_ID(N'[People]'))
    SET IDENTITY_INSERT [People] OFF;
");
        }

        public override void DeleteDataOperation_simple_key()
        {
            base.DeleteDataOperation_simple_key();

            // TODO remove rowcount
            AssertSql(
                @"DELETE FROM [People]
WHERE [Id] = 2;
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [Id] = 4;
SELECT @@ROWCOUNT;

");
        }

        public override void DeleteDataOperation_composite_key()
        {
            base.DeleteDataOperation_composite_key();

            // TODO remove rowcount
            AssertSql(
                @"DELETE FROM [People]
WHERE [First Name] = N'Hodor' AND [Last Name] IS NULL;
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';
SELECT @@ROWCOUNT;

");
        }

        public override void UpdateDataOperation_simple_key()
        {
            base.UpdateDataOperation_simple_key();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE [People] SET [Full Name] = N'Daenerys Stormborn'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

UPDATE [People] SET [Full Name] = N'Homeless Harry Strickland'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;

");
        }

        public override void UpdateDataOperation_composite_key()
        {
            base.UpdateDataOperation_composite_key();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE [People] SET [First Name] = N'Hodor'
WHERE [Id] = 0 AND [Last Name] IS NULL;
SELECT @@ROWCOUNT;

UPDATE [People] SET [First Name] = N'Harry'
WHERE [Id] = 4 AND [Last Name] = N'Strickland';
SELECT @@ROWCOUNT;

");
        }

        public override void UpdateDataOperation_multiple_columns()
        {
            base.UpdateDataOperation_multiple_columns();

            // TODO remove rowcount
            AssertSql(
                @"UPDATE [People] SET [First Name] = N'Daenerys', [Nickname] = N'Dany'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

UPDATE [People] SET [First Name] = N'Harry', [Nickname] = N'Homeless'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;

");
        }

        public SqlServerMigrationSqlGeneratorTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}

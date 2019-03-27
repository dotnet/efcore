// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        public override void CreateIndexOperation_with_filter_where_clause()
        {
            base.CreateIndexOperation_with_filter_where_clause();

            Assert.Equal(
                "CREATE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_with_filter_where_clause_and_is_unique()
        {
            base.CreateIndexOperation_with_filter_where_clause_and_is_unique();

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL AND <> '';" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "ALTER TABLE [People] ADD [FullName] AS FirstName + ' ' + LastName;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_computed_column_SQL()
        {
            base.AddColumnOperation_with_computed_column_SQL();

            Assert.Equal(
                "ALTER TABLE [People] ADD [Birthday] AS CURRENT_TIMESTAMP;" + EOL,
                Sql);
        }

        [Fact]
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
                    [SqlServerAnnotationNames.ValueGenerationStrategy] =
                        SqlServerValueGenerationStrategy.IdentityColumn
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            Assert.Equal(
                "ALTER TABLE [People] ADD [Alias] nvarchar(max) NOT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            base.AddColumnOperation_with_unicode_no_model();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] varchar(max) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(30) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(32) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(30) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_ansi()
        {
            base.AddColumnOperation_with_ansi();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] varchar(max) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(max) NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            Assert.Equal(
                "ALTER TABLE [Base] ADD [Foo] nvarchar(max) NULL;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_nonclustered()
        {
            Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "Id" },
                    [SqlServerAnnotationNames.Clustered] = false
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD PRIMARY KEY NONCLUSTERED ([Id]);" + EOL,
                Sql);
        }

        public override void AlterColumnOperation()
        {
            base.AlterColumnOperation();

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [dbo].[People] ALTER COLUMN [LuckyNumber] int NOT NULL;" + EOL +
                "ALTER TABLE [dbo].[People] ADD DEFAULT 7 FOR [LuckyNumber];" + EOL,
                Sql);
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [People] ALTER COLUMN [LuckyNumber] int NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_identity()
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

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Id');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [People] ALTER COLUMN [Id] int NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_computed()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    ComputedColumnSql = "[FirstName] + ' ' + [LastName]"
                });

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'FullName');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [People] DROP COLUMN [FullName];" + EOL +
                "ALTER TABLE [People] ADD [FullName] AS [FirstName] + ' ' + [LastName];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_computed_with_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("FullName").HasComputedColumnSql("[FirstName] + ' ' + [LastName]");
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

            Assert.Equal(
                "DROP INDEX [IX_Person_FullName] ON [Person];" + EOL +
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'FullName');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] DROP COLUMN [FullName];" + EOL +
                "ALTER TABLE [Person] ADD [FullName] AS [FirstName] + ' ' + [LastName];" + EOL +
                "CREATE INDEX [IX_Person_FullName] ON [Person] ([FullName]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_memoryOptimized_with_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.ForSqlServerIsMemoryOptimized();
                            x.Property<string>("Name");
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

            Assert.Equal(
                "ALTER TABLE [Person] DROP INDEX [IX_Person_Name];" + EOL +
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NOT NULL;" + EOL +
                "ALTER TABLE [Person] ADD INDEX [IX_Person_Name] NONCLUSTERED ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_index_no_narrowing()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name");
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

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(450) NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
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
                });

            Assert.Equal(
                "DROP INDEX [IX_Person_Name] ON [Person];" + EOL +
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;" + EOL +
                "CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);" + EOL,
                Sql);
        }

#if !Test21
        [Fact]
        public virtual void AlterColumnOperation_with_index_included_column()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("Name").HasMaxLength(30);
                            x.Property<string>("FirstName");
                            x.Property<string>("LastName");
                            x.HasIndex("FirstName", "LastName")
                                .ForSqlServerInclude("Name");
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

            Assert.Equal(
                "DROP INDEX [IX_Person_FirstName_LastName] ON [Person];" + EOL +
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;" + EOL +
                "CREATE INDEX [IX_Person_FirstName_LastName] ON [Person] ([FirstName], [LastName]) INCLUDE ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_index_no_included()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
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

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;" + EOL,
                Sql);
        }
#endif

        [Fact]
        public virtual void AlterColumnOperation_with_index_no_oldColumn()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.0.0-rtm")
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

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_composite_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity(
                        "Person", x =>
                        {
                            x.Property<string>("FirstName").IsRequired();
                            x.Property<string>("LastName");
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

            Assert.Equal(
                "DROP INDEX [IX_Person_FirstName_LastName] ON [Person];" + EOL +
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'FirstName');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [FirstName] nvarchar(450) NOT NULL;" + EOL +
                "CREATE INDEX [IX_Person_FirstName_LastName] ON [Person] ([FirstName], [LastName]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_added_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
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

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;" + EOL +
                "GO" + EOL +
                EOL +
                "CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_identity()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0"),
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

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Id');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Id] bigint NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_add_identity()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(
                    modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0"),
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

        [Fact]
        public virtual void AlterColumnOperation_remove_identity()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(
                    modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0"),
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

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(
                new SqlServerCreateDatabaseOperation
                {
                    Name = "Northwind"
                });

            Assert.Equal(
                "CREATE DATABASE [Northwind];" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5" + EOL +
                "BEGIN" + EOL +
                "    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;" + EOL +
                "END;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "ON (NAME = N'Narf', FILENAME = N'" + expectedFile + "')" + EOL +
                "LOG ON (NAME = N'Narf_log', FILENAME = N'" + expectedLog + "');" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5" + EOL +
                "BEGIN" + EOL +
                "    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;" + EOL +
                "END;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "ON (NAME = N'Narf', FILENAME = N'" + expectedFile + "')" + EOL +
                "LOG ON (NAME = N'Narf_log', FILENAME = N'" + expectedLog + "');" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5" + EOL +
                "BEGIN" + EOL +
                "    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;" + EOL +
                "END;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "ON (NAME = N'Narf', FILENAME = N'" + expectedFile + "')" + EOL +
                "LOG ON (NAME = N'Narf_log', FILENAME = N'" + expectedLog + "');" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5" + EOL +
                "BEGIN" + EOL +
                "    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;" + EOL +
                "END;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_unique_non_legacy()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.0.0"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "dbo",
                    Columns = new[] { "FirstName", "LastName" },
                    IsUnique = true
                });

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]);" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE UNIQUE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

#if !Test21
        [Fact]
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

            Assert.Equal(
                "CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL AND <> '';" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]) WHERE [Name] IS NOT NULL AND <> '';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_unique_with_include_non_legacy()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.0.0"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Include] = new[] { "FirstName", "LastName" }
                });

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) INCLUDE ([FirstName], [LastName]);" + EOL,
                Sql);
        }
#endif

        [Fact]
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

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_unique_bound_not_null()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name").IsRequired(),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true
                });

            Assert.Equal(
                "CREATE UNIQUE INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nullable()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").ToTable("People", "dbo").ForSqlServerIsMemoryOptimized().Property<string>("Name"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Schema = "dbo",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true
                });

            Assert.Equal(
                "ALTER TABLE [dbo].[People] ADD INDEX [IX_People_Name] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nullable_with_filter()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").ForSqlServerIsMemoryOptimized().Property<string>("Name"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    Filter = "[Name] IS NOT NULL AND <> ''"
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD INDEX [IX_People_Name] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nonclustered_not_nullable()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").ForSqlServerIsMemoryOptimized().Property<string>("Name").IsRequired(),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Clustered] = false
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD INDEX [IX_People_Name] UNIQUE NONCLUSTERED ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation()
        {
            Generate(
                new EnsureSchemaOperation
                {
                    Name = "my"
                });

            Assert.Equal(
                "IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation_dbo()
        {
            Generate(
                new EnsureSchemaOperation
                {
                    Name = "dbo"
                });

            Assert.Equal(
                "",
                Sql);
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[People]') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [dbo].[People] DROP COLUMN [LuckyNumber];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropDatabaseOperation()
        {
            Generate(
                new SqlServerDropDatabaseOperation
                {
                    Name = "Northwind"
                });

            Assert.Equal(
                "IF SERVERPROPERTY('EngineEdition') <> 5" + EOL +
                "BEGIN" + EOL +
                "    ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" + EOL +
                "END;" + EOL +
                "GO" + EOL +
                EOL +
                "DROP DATABASE [Northwind];" + EOL,
                Sql);
        }

        public override void DropIndexOperation()
        {
            base.DropIndexOperation();

            Assert.Equal(
                "DROP INDEX [IX_People_Name] ON [dbo].[People];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropIndexOperation_memoryOptimized()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").ForSqlServerIsMemoryOptimized(),
                new DropIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People"
                });

            Assert.Equal(
                "ALTER TABLE [People] DROP INDEX [IX_People_Name];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void MoveSequenceOperation_legacy()
        {
            Generate(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewSchema = "my"
                });

            Assert.Equal(
                "ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void MoveSequenceOperation()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.1.0"),
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "EntityFrameworkHiLoSequence",
                    NewSchema = "my"
                });

            Assert.Equal(
                "ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];" + EOL,
                Sql);
        }

#if  !Test21
        [Fact]
        public virtual void MoveSequenceOperation_into_default()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.1.0"),
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "EntityFrameworkHiLoSequence"
                });

            Assert.Equal(
                "DECLARE @defaultSchema sysname = SCHEMA_NAME();" + EOL +
                "EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [dbo].[EntityFrameworkHiLoSequence];');" + EOL,
                Sql);
        }
#endif

        [Fact]
        public virtual void MoveTableOperation_legacy()
        {
            Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewSchema = "hr"
                });

            Assert.Equal(
                "ALTER SCHEMA [hr] TRANSFER [dbo].[People];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void MoveTableOperation()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "People",
                    NewSchema = "hr"
                });

            Assert.Equal(
                "ALTER SCHEMA [hr] TRANSFER [dbo].[People];" + EOL,
                Sql);
        }

#if !Test21
        [Fact]
        public virtual void MoveTableOperation_into_default()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "People"
                });

            Assert.Equal(
                "DECLARE @defaultSchema sysname = SCHEMA_NAME();" + EOL +
                "EXEC(N'ALTER SCHEMA [' + @defaultSchema + N'] TRANSFER [dbo].[People];');" + EOL,
                Sql);
        }
#endif

        [Fact]
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

            Assert.Equal(
                "EXEC sp_rename N'[dbo].[People].[Name]', N'FullName', N'COLUMN';" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "EXEC sp_rename N'[dbo].[People].[IX_People_Name]', N'IX_People_FullName', N'INDEX';" + EOL,
                Sql);
        }

        [Fact]
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

        [Fact]
        public virtual void RenameSequenceOperation_legacy()
        {
            Generate(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "MySequence"
                });

            Assert.Equal(
                "EXEC sp_rename N'[dbo].[EntityFrameworkHiLoSequence]', N'MySequence';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameSequenceOperation()
        {
            Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "2.1.0"),
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "MySequence",
                    NewSchema = "dbo"
                });

            Assert.Equal(
                "EXEC sp_rename N'[dbo].[EntityFrameworkHiLoSequence]', N'MySequence';" + EOL,
                Sql);
        }

        [Fact]
        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();

            Assert.Equal(
                "EXEC sp_rename N'[dbo].[People]', N'Person';" + EOL,
                Sql);
        }

        [Fact]
        public override void RenameTableOperation()
        {
            base.RenameTableOperation();

            Assert.Equal(
                "EXEC sp_rename N'[dbo].[People]', N'Person';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void SqlOperation_handles_backslash()
        {
            Generate(
                new SqlOperation
                {
                    Sql = @"-- Multiline \" + EOL +
                          "comment"
                });

            Assert.Equal(
                "-- Multiline comment" + EOL,
                Sql);
        }

        [Fact]
        public virtual void SqlOperation_ignores_sequential_gos()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- Ready set" + EOL +
                          "GO" + EOL +
                          "GO"
                });

            Assert.Equal(
                "-- Ready set" + EOL,
                Sql);
        }

        [Fact]
        public virtual void SqlOperation_handles_go()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I" + EOL +
                          "go" + EOL +
                          "-- Too"
                });

            Assert.Equal(
                "-- I" + EOL +
                "GO" + EOL +
                EOL +
                "-- Too" + EOL,
                Sql);
        }

        [Fact]
        public virtual void SqlOperation_handles_go_with_count()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I" + EOL +
                          "GO 2"
                });

            Assert.Equal(
                "-- I" + EOL +
                "GO" + EOL +
                EOL +
                "-- I" + EOL,
                Sql);
        }

        [Fact]
        public virtual void SqlOperation_ignores_non_go()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I GO 2"
                });

            Assert.Equal(
                "-- I GO 2" + EOL,
                Sql);
        }

        public override void InsertDataOperation()
        {
            base.InsertDataOperation();

            Assert.Equal(
                "IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Full Name') AND [object_id] = OBJECT_ID(N'[People]'))" + EOL +
                "    SET IDENTITY_INSERT [People] ON;" + EOL +
                "INSERT INTO [People] ([Id], [Full Name])" + EOL +
                "VALUES (0, NULL)," + EOL +
                "(1, N'Daenerys Targaryen')," + EOL +
                "(2, N'John Snow')," + EOL +
                "(3, N'Arya Stark')," + EOL +
                "(4, N'Harry Strickland');" + EOL +
                "IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Full Name') AND [object_id] = OBJECT_ID(N'[People]'))" + EOL +
                "    SET IDENTITY_INSERT [People] OFF;" + EOL,
                Sql);
        }

        public override void DeleteDataOperation_simple_key()
        {
            base.DeleteDataOperation_simple_key();

            // TODO remove rowcount
            Assert.Equal(
                "DELETE FROM [People]" + EOL +
                "WHERE [Id] = 2;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL +
                "DELETE FROM [People]" + EOL +
                "WHERE [Id] = 4;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL,
                Sql);
        }

        public override void DeleteDataOperation_composite_key()
        {
            base.DeleteDataOperation_composite_key();

            // TODO remove rowcount
            Assert.Equal(
                "DELETE FROM [People]" + EOL +
                "WHERE [First Name] = N'Hodor' AND [Last Name] IS NULL;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL +
                "DELETE FROM [People]" + EOL +
                "WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL,
                Sql);
        }

        public override void UpdateDataOperation_simple_key()
        {
            base.UpdateDataOperation_simple_key();

            // TODO remove rowcount
            Assert.Equal(
                "UPDATE [People] SET [Full Name] = N'Daenerys Stormborn'" + EOL +
                "WHERE [Id] = 1;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL +
                "UPDATE [People] SET [Full Name] = N'Homeless Harry Strickland'" + EOL +
                "WHERE [Id] = 4;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL,
                Sql);
        }

        public override void UpdateDataOperation_composite_key()
        {
            base.UpdateDataOperation_composite_key();

            // TODO remove rowcount
            Assert.Equal(
                "UPDATE [People] SET [First Name] = N'Hodor'" + EOL +
                "WHERE [Id] = 0 AND [Last Name] IS NULL;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL +
                "UPDATE [People] SET [First Name] = N'Harry'" + EOL +
                "WHERE [Id] = 4 AND [Last Name] = N'Strickland';" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL,
                Sql);
        }

        public override void UpdateDataOperation_multiple_columns()
        {
            base.UpdateDataOperation_multiple_columns();

            // TODO remove rowcount
            Assert.Equal(
                "UPDATE [People] SET [First Name] = N'Daenerys', [Nickname] = N'Dany'" + EOL +
                "WHERE [Id] = 1;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL +
                "UPDATE [People] SET [First Name] = N'Harry', [Nickname] = N'Homeless'" + EOL +
                "WHERE [Id] = 4;" + EOL +
                "SELECT @@ROWCOUNT;" + EOL + EOL,
                Sql);
        }

        public SqlServerMigrationSqlGeneratorTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class SqlServerMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        [Fact]
        public override void CreateIndexOperation_with_filter_where_clause()
        {
            base.CreateIndexOperation_with_filter_where_clause();

            Assert.Equal(
                "CREATE INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
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

        [Fact]
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
                    [SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] =
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
                "ALTER TABLE [Person] ADD [Name] varchar(max);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(30);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(32);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(30);" + EOL,
                Sql);
        }

        [Fact]
        public override void AddColumnOperation_with_ansi()
        {
            base.AddColumnOperation_with_ansi();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] varchar(max);" + EOL,
                Sql);
        }

        [Fact]
        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            Assert.Equal(
                "ALTER TABLE [Person] ADD [Name] nvarchar(max);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            Assert.Equal(
                "ALTER TABLE [Base] ADD [Foo] nvarchar(max);" + EOL,
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
                "ALTER TABLE [Person] ADD [RowVersion] rowversion;" + EOL,
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
                "ALTER TABLE [Person] ADD [RowVersion] rowversion;" + EOL,
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
                    [SqlServerFullAnnotationNames.Instance.Clustered] = false
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'dbo.People') AND [c].[name] = N'LuckyNumber');" + EOL +
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'People') AND [c].[name] = N'LuckyNumber');" + EOL +
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
                    [SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] =
                        SqlServerValueGenerationStrategy.IdentityColumn
                });

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'People') AND [c].[name] = N'Id');" + EOL +
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'People') AND [c].[name] = N'FullName');" + EOL +
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
                    .Entity("Person", x =>
                        {
                            x.Property<string>("FullName").ForSqlServerHasComputedColumnSql("[FirstName] + ' ' + [LastName]");
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'FullName');" + EOL +
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
                    .Entity("Person", x =>
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
                    },
                    [SqlServerFullAnnotationNames.Instance.MemoryOptimized] = true
                });

            Assert.Equal(
                "ALTER TABLE [Person] DROP INDEX [IX_Person_Name];" + EOL +
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'Name');" + EOL +
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
                    .Entity("Person", x =>
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(450);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity("Person", x =>
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30);" + EOL +
                "CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_index_no_oldColumn()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.0.0-rtm")
                    .Entity("Person", x =>
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_composite_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersionAnnotation, "1.1.0")
                    .Entity("Person", x =>
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'FirstName');" + EOL +
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
                    .Entity("Person", x =>
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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'Name');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30);" + EOL +
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
                    [SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn
                    }
                });

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'Person') AND [c].[name] = N'Id');" + EOL +
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
                        [SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn,
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
                            [SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn
                        }
                    }));

            Assert.Equal(SqlServerStrings.AlterIdentityColumn, ex.Message);
        }

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new SqlServerCreateDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                "CREATE DATABASE [Northwind];" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;');" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation_with_filename()
        {
            Generate(new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "Narf.mdf" });

            var expectedFile = Path.GetFullPath("Narf.mdf");
            var expectedLog = Path.GetFullPath("Narf_log.ldf");

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "ON (NAME = 'Narf', FILENAME = '" + expectedFile + "')" + EOL +
                "LOG ON (NAME = 'Narf_log', FILENAME = '" + expectedLog + "');" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;');" + EOL,
                Sql);
        }

#if NET46

        [Fact]
        public virtual void CreateDatabaseOperation_with_filename_and_datadirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Generate(new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "|DataDirectory|Narf.mdf" });

            var expectedFile = Path.Combine(baseDirectory, "Narf.mdf");
            var expectedLog = Path.Combine(baseDirectory, "Narf_log.ldf");

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "ON (NAME = 'Narf', FILENAME = '" + expectedFile + "')" + EOL +
                "LOG ON (NAME = 'Narf_log', FILENAME = '" + expectedLog + "');" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;');" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation_with_filename_and_custom_datadirectory()
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);

            Generate(new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "|DataDirectory|Narf.mdf" });

            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            var expectedFile = Path.Combine(dataDirectory, "Narf.mdf");
            var expectedLog = Path.Combine(dataDirectory, "Narf_log.ldf");

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "ON (NAME = 'Narf', FILENAME = '" + expectedFile + "')" + EOL +
                "LOG ON (NAME = 'Narf_log', FILENAME = '" + expectedLog + "');" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;');" + EOL,
                Sql);
        }
#elif NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif

        [Fact]
        public virtual void AlterDatabaseOperationOperation()
        {
            Generate(new AlterDatabaseOperation { [SqlServerFullAnnotationNames.Instance.MemoryOptimized] = true });

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
                    [SqlServerFullAnnotationNames.Instance.Clustered] = true
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
                    [SqlServerFullAnnotationNames.Instance.Clustered] = true
                });

            Assert.Equal(
                "CREATE UNIQUE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
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
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerFullAnnotationNames.Instance.MemoryOptimized] = true
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD INDEX [IX_People_Name] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nullable_with_filter()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    Filter = "[Name] IS NOT NULL AND <> ''",
                    [SqlServerFullAnnotationNames.Instance.MemoryOptimized] = true
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD INDEX [IX_People_Name] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_memoryOptimized_unique_nonclustered_not_nullable()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name").IsRequired(),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerFullAnnotationNames.Instance.MemoryOptimized] = true,
                    [SqlServerFullAnnotationNames.Instance.Clustered] = false
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD INDEX [IX_People_Name] UNIQUE NONCLUSTERED ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation()
        {
            Generate(new EnsureSchemaOperation { Name = "my" });

            Assert.Equal(
                "IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation_dbo()
        {
            Generate(new EnsureSchemaOperation { Name = "dbo" });

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
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'dbo.People') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + '];');" + EOL +
                "ALTER TABLE [dbo].[People] DROP COLUMN [LuckyNumber];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropDatabaseOperation()
        {
            Generate(new SqlServerDropDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;');" + EOL +
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
                new DropIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    [SqlServerFullAnnotationNames.Instance.MemoryOptimized] = true
                });

            Assert.Equal(
                "ALTER TABLE [People] DROP INDEX [IX_People_Name];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void MoveSequenceOperation()
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
        public virtual void MoveTableOperation()
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
                "EXEC sp_rename N'dbo.People.Name', N'FullName', N'COLUMN';" + EOL,
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
                "EXEC sp_rename N'dbo.People.IX_People_Name', N'IX_People_FullName', N'INDEX';" + EOL,
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
        public virtual void RenameSequenceOperation()
        {
            Generate(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "MySequence"
                });

            Assert.Equal(
                "EXEC sp_rename N'dbo.EntityFrameworkHiLoSequence', N'MySequence';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameTableOperation()
        {
            Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person"
                });

            Assert.Equal(
                "EXEC sp_rename N'dbo.People', N'Person';" + EOL,
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

        public SqlServerMigrationSqlGeneratorTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}

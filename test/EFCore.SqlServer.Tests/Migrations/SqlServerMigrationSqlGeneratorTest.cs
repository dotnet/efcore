// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqlServerMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
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

            AssertSql(
                @"ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;
");
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE [People] ADD [Alias] nvarchar(max) NOT NULL;
");
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            base.AddColumnOperation_with_unicode_no_model();

            AssertSql(
                @"ALTER TABLE [Person] ADD [Name] varchar(max) NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            AssertSql(
                @"ALTER TABLE [Person] ADD [Name] nvarchar(32) NULL;
");
        }

        public override void AddColumnOperation_with_precision_and_scale_overridden()
        {
            base.AddColumnOperation_with_precision_and_scale_overridden();

            AssertSql(
                @"ALTER TABLE [Person] ADD [Pi] decimal(15,10) NOT NULL;
");
        }

        public override void AddColumnOperation_with_precision_and_scale_no_model()
        {
            base.AddColumnOperation_with_precision_and_scale_no_model();

            AssertSql(
                @"ALTER TABLE [Person] ADD [Pi] decimal(20,7) NOT NULL;
");
        }

        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            AssertSql(
                @"ALTER TABLE [Person] ADD [Name] nvarchar(max) NULL;
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_rowversion_overridden()
        {
            Generate(
                modelBuilder => modelBuilder.Entity<Person>().Property<byte[]>("RowVersion"),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "RowVersion",
                    ClrType = typeof(byte[]),
                    IsRowVersion = true,
                    IsNullable = true
                });

            AssertSql(
                @"ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;
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

            AssertSql(
                @"ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;
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
        public virtual void AlterColumnOperation_with_index_no_oldColumn()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.0.0-rtm")
                    .Entity<Person>(
                        x =>
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
        public virtual void AlterColumnOperation_with_added_index()
        {
            Generate(
                modelBuilder => modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity<Person>(
                        x =>
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
                    OldColumn = new ColumnOperation { ClrType = typeof(string), IsNullable = true }
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
                        OldColumn = new ColumnOperation { ClrType = typeof(int) }
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
        public virtual void CreateDatabaseOperation()
        {
            Generate(
                new SqlServerCreateDatabaseOperation { Name = "Northwind" });

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
                new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "Narf.mdf" });

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
                new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "|DataDirectory|Narf.mdf" });

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
                new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "|DataDirectory|Narf.mdf" });

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
        public virtual void AlterDatabaseOperation_memory_optimized()
        {
            Generate(
                new AlterDatabaseOperation { [SqlServerAnnotationNames.MemoryOptimized] = true });

            Assert.Contains(
                "CONTAINS MEMORY_OPTIMIZED_DATA;",
                Sql);
        }


        [ConditionalFact]
        public virtual void DropDatabaseOperation()
        {
            Generate(
                new SqlServerDropDatabaseOperation { Name = "Northwind" });

            AssertSql(
                @"IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END;
GO

DROP DATABASE [Northwind];
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

            AssertSql(
                @"ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];
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

            AssertSql(
                @"ALTER SCHEMA [hr] TRANSFER [dbo].[People];
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

            AssertSql(
                @"EXEC sp_rename N'[dbo].[EntityFrameworkHiLoSequence]', N'MySequence';
");
        }

        [ConditionalFact]
        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();

            AssertSql(
                @"EXEC sp_rename N'[dbo].[People]', N'Person';
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_handles_backslash()
        {
            Generate(
                new SqlOperation { Sql = @"-- Multiline \" + EOL + "comment" });

            AssertSql(
                @"-- Multiline comment
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_ignores_sequential_gos()
        {
            Generate(
                new SqlOperation { Sql = "-- Ready set" + EOL + "GO" + EOL + "GO" });

            AssertSql(
                @"-- Ready set
");
        }

        [ConditionalFact]
        public virtual void SqlOperation_handles_go()
        {
            Generate(
                new SqlOperation { Sql = "-- I" + EOL + "go" + EOL + "-- Too" });

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
                new SqlOperation { Sql = "-- I" + EOL + "GO 2" });

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
                new SqlOperation { Sql = "-- I GO 2" });

            AssertSql(
                @"-- I GO 2
");
        }

        public override void DefaultValue_with_line_breaks(bool isUnicode)
        {
            base.DefaultValue_with_line_breaks(isUnicode);

            var storeType = isUnicode ? "nvarchar(max)" : "varchar(max)";
            var unicodePrefix = isUnicode ? "N" : string.Empty;
            var expectedSql = @$"CREATE TABLE [dbo].[TestLineBreaks] (
    [TestDefaultValue] {storeType} NOT NULL DEFAULT CONCAT(CHAR(13), CHAR(10), {unicodePrefix}'Various Line', CHAR(13), {unicodePrefix}'Breaks', CHAR(10))
);
";
            AssertSql(expectedSql);
        }

        public SqlServerMigrationSqlGeneratorTest()
            : base(SqlServerTestHelpers.Instance)
        {
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

public class SqlServerMigrationsSqlGeneratorTest : MigrationsSqlGeneratorTestBase
{
    [ConditionalFact]
    public void CreateIndexOperation_unique_online()
    {
        Generate(
            new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = ["FirstName", "LastName"],
                IsUnique = true,
                [SqlServerAnnotationNames.CreatedOnline] = true
            });

        AssertSql(
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL WITH (ONLINE = ON);
""");
    }

    [ConditionalFact]
    public void CreateIndexOperation_unique_sortintempdb()
    {
        Generate(
            new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = ["FirstName", "LastName"],
                IsUnique = true,
                [SqlServerAnnotationNames.SortInTempDb] = true
            });

        AssertSql(
            """
CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL WITH (SORT_IN_TEMPDB = ON);
""");
    }

    [ConditionalTheory]
    [InlineData(DataCompressionType.None, "NONE")]
    [InlineData(DataCompressionType.Row, "ROW")]
    [InlineData(DataCompressionType.Page, "PAGE")]
    public void CreateIndexOperation_unique_datacompression(DataCompressionType dataCompression, string dataCompressionSql)
    {
        Generate(
            new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = ["FirstName", "LastName"],
                IsUnique = true,
                [SqlServerAnnotationNames.DataCompression] = dataCompression
            });

        AssertSql(
            $"""
CREATE UNIQUE INDEX [IX_People_Name] ON [dbo].[People] ([FirstName], [LastName]) WHERE [FirstName] IS NOT NULL AND [LastName] IS NOT NULL WITH (DATA_COMPRESSION = {dataCompressionSql});
""");
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

        AssertSql(
            """
ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;
""");
    }

    public override void AddColumnOperation_without_column_type()
    {
        base.AddColumnOperation_without_column_type();

        AssertSql(
            """
ALTER TABLE [People] ADD [Alias] nvarchar(max) NOT NULL;
""");
    }

    public override void AddColumnOperation_with_unicode_no_model()
    {
        base.AddColumnOperation_with_unicode_no_model();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Name] varchar(max) NULL;
""");
    }

    public override void AddColumnOperation_with_fixed_length_no_model()
    {
        base.AddColumnOperation_with_fixed_length_no_model();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Name] char(100) NULL;
""");
    }

    public override void AddColumnOperation_with_maxLength_no_model()
    {
        base.AddColumnOperation_with_maxLength_no_model();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Name] nvarchar(30) NULL;
""");
    }

    public override void AddColumnOperation_with_maxLength_overridden()
    {
        base.AddColumnOperation_with_maxLength_overridden();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Name] nvarchar(32) NULL;
""");
    }

    public override void AddColumnOperation_with_precision_and_scale_overridden()
    {
        base.AddColumnOperation_with_precision_and_scale_overridden();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Pi] decimal(15,10) NOT NULL;
""");
    }

    public override void AddColumnOperation_with_precision_and_scale_no_model()
    {
        base.AddColumnOperation_with_precision_and_scale_no_model();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Pi] decimal(20,7) NOT NULL;
""");
    }

    public override void AddColumnOperation_with_unicode_overridden()
    {
        base.AddColumnOperation_with_unicode_overridden();

        AssertSql(
            """
ALTER TABLE [Person] ADD [Name] nvarchar(max) NULL;
""");
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
            """
ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;
""");
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
            """
ALTER TABLE [Person] ADD [RowVersion] rowversion NULL;
""");
    }

    public override void AlterColumnOperation_without_column_type()
    {
        base.AlterColumnOperation_without_column_type();

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'LuckyNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [LuckyNumber] int NOT NULL;
""");
    }

    public override void AddForeignKeyOperation_without_principal_columns()
    {
        base.AddForeignKeyOperation_without_principal_columns();

        AssertSql(
            """
ALTER TABLE [People] ADD FOREIGN KEY ([SpouseId]) REFERENCES [People];
""");
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
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [People] ALTER COLUMN [Id] int NOT NULL;
""");
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
                OldColumn = new AddColumnOperation()
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
""");
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
                OldColumn = new AddColumnOperation { ClrType = typeof(string), IsNullable = true }
            },
            new CreateIndexOperation
            {
                Name = "IX_Person_Name",
                Table = "Person",
                Columns = ["Name"]
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(30) NULL;
GO

CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);
""");
    }

    [ConditionalFact]
    public virtual void AlterColumnOperation_with_added_index_no_oldType()
    {
        Generate(
            modelBuilder => modelBuilder
                .HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0")
                .Entity<Person>(
                    x =>
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
                OldColumn = new AddColumnOperation { ClrType = typeof(string), IsNullable = true }
            },
            new CreateIndexOperation
            {
                Name = "IX_Person_Name",
                Table = "Person",
                Columns = ["Name"]
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(450) NULL;
GO

CREATE INDEX [IX_Person_Name] ON [Person] ([Name]);
""");
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
                OldColumn = new AddColumnOperation
                {
                    ClrType = typeof(int),
                    [SqlServerAnnotationNames.ValueGenerationStrategy] = SqlServerValueGenerationStrategy.IdentityColumn
                }
            });

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Person] ALTER COLUMN [Id] bigint NOT NULL;
""");
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
                    OldColumn = new AddColumnOperation { ClrType = typeof(int) }
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
                    OldColumn = new AddColumnOperation
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
            """
CREATE DATABASE [Northwind];
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
""");
    }

    [ConditionalFact]
    public virtual void CreateDatabaseOperation_with_filename()
    {
        Generate(
            new SqlServerCreateDatabaseOperation { Name = "Northwind", FileName = "Narf.mdf" });

        var expectedFile = Path.GetFullPath("Narf.mdf");
        var expectedLog = Path.GetFullPath("Narf_log.ldf");

        AssertSql(
            $"""
CREATE DATABASE [Northwind]
ON (NAME = N'Narf', FILENAME = N'{expectedFile}')
LOG ON (NAME = N'Narf_log', FILENAME = N'{expectedLog}');
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
""");
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
            $"""
CREATE DATABASE [Northwind]
ON (NAME = N'Narf', FILENAME = N'{expectedFile}')
LOG ON (NAME = N'Narf_log', FILENAME = N'{expectedLog}');
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
""");
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
            $"""
CREATE DATABASE [Northwind]
ON (NAME = N'Narf', FILENAME = N'{expectedFile}')
LOG ON (NAME = N'Narf_log', FILENAME = N'{expectedLog}');
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
""");
    }

    [ConditionalFact]
    public virtual void CreateDatabaseOperation_with_collation()
    {
        Generate(
            new SqlServerCreateDatabaseOperation { Name = "Northwind", Collation = "German_PhoneBook_CI_AS" });

        AssertSql(
            """
CREATE DATABASE [Northwind]
COLLATE German_PhoneBook_CI_AS;
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON;
END;
""");
    }

    [ConditionalFact]
    public virtual void AlterDatabaseOperation_collation()
    {
        Generate(
            new AlterDatabaseOperation { Collation = "German_PhoneBook_CI_AS" });

        Assert.Contains(
            "COLLATE German_PhoneBook_CI_AS",
            Sql);
    }

    [ConditionalFact]
    public virtual void AlterDatabaseOperation_collation_to_default()
    {
        Generate(
            new AlterDatabaseOperation { Collation = null, OldDatabase = { Collation = "SQL_Latin1_General_CP1_CI_AS" } });

        AssertSql(
            """
BEGIN
DECLARE @db_name nvarchar(max) = DB_NAME();
DECLARE @defaultCollation nvarchar(max) = CAST(SERVERPROPERTY('Collation') AS nvarchar(max));
EXEC(N'ALTER DATABASE [' + @db_name + '] COLLATE ' + @defaultCollation + N';');
END

""");
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
            """
IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END;
GO

DROP DATABASE [Northwind];
""");
    }

    [ConditionalFact]
    public virtual void DropIndexOperations_throws_when_no_table()
    {
        var migrationBuilder = new MigrationBuilder("SqlServer");

        migrationBuilder.DropIndex(
            name: "IX_Name");

        var ex = Assert.Throws<InvalidOperationException>(
            () => Generate(migrationBuilder.Operations.ToArray()));

        Assert.Equal(SqlServerStrings.IndexTableRequired, ex.Message);
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
            """
ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];
""");
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
            """
ALTER SCHEMA [hr] TRANSFER [dbo].[People];
""");
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
            """
EXEC sp_rename N'[dbo].[EntityFrameworkHiLoSequence]', N'MySequence', 'OBJECT';
""");
    }

    [ConditionalFact]
    public override void RenameTableOperation_legacy()
    {
        base.RenameTableOperation_legacy();

        AssertSql(
            """
EXEC sp_rename N'[dbo].[People]', N'Person', 'OBJECT';
""");
    }

    public override void RenameTableOperation()
    {
        base.RenameTableOperation();

        AssertSql(
            """
EXEC sp_rename N'[dbo].[People]', N'Person', 'OBJECT';
""");
    }

    [ConditionalFact]
    public virtual void SqlOperation_handles_backslash()
    {
        Generate(
            new SqlOperation { Sql = @"-- Multiline \" + EOL + "comment" });

        AssertSql(
            """
-- Multiline comment
""");
    }

    [ConditionalFact]
    public virtual void SqlOperation_ignores_sequential_gos()
    {
        Generate(
            new SqlOperation { Sql = "-- Ready set" + EOL + "GO" + EOL + "GO" });

        AssertSql(
            """
-- Ready set
""");
    }

    [ConditionalFact]
    public virtual void SqlOperation_handles_go()
    {
        Generate(
            new SqlOperation { Sql = "-- I" + EOL + "go" + EOL + "-- Too" });

        AssertSql(
            """
-- I
GO

-- Too
""");
    }

    [ConditionalFact]
    public virtual void SqlOperation_handles_go_with_count()
    {
        Generate(
            new SqlOperation { Sql = "-- I" + EOL + "GO 2" });

        AssertSql(
            """
-- I
GO

-- I
""");
    }

    [ConditionalFact]
    public virtual void SqlOperation_ignores_non_go()
    {
        Generate(
            new SqlOperation { Sql = "-- I GO 2" });

        AssertSql(
            """
-- I GO 2
""");
    }

    public override void SqlOperation()
    {
        base.SqlOperation();

        AssertSql(
            """
-- I <3 DDL
""");
    }

    public override void InsertDataOperation_all_args_spatial()
    {
        base.InsertDataOperation_all_args_spatial();

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Full Name', N'Geometry') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] ON;
INSERT INTO [dbo].[People] ([Id], [Full Name], [Geometry])
VALUES (0, NULL, NULL),
(1, 'Daenerys Targaryen', NULL),
(2, 'John Snow', NULL),
(3, 'Arya Stark', NULL),
(4, 'Harry Strickland', NULL),
(5, 'The Imp', NULL),
(6, 'The Kingslayer', NULL),
(7, 'Aemon Targaryen', geography::Parse('GEOMETRYCOLLECTION (LINESTRING (1.1 2.2 NULL, 2.2 2.2 NULL, 2.2 1.1 NULL, 7.1 7.2 NULL), LINESTRING (7.1 7.2 NULL, 20.2 20.2 NULL, 20.2 1.1 NULL, 70.1 70.2 NULL), MULTIPOINT ((1.1 2.2 NULL), (2.2 2.2 NULL), (2.2 1.1 NULL)), POLYGON ((1.1 2.2 NULL, 2.2 2.2 NULL, 2.2 1.1 NULL, 1.1 2.2 NULL)), POLYGON ((10.1 20.2 NULL, 20.2 20.2 NULL, 20.2 10.1 NULL, 10.1 20.2 NULL)), POINT (1.1 2.2 3.3), MULTILINESTRING ((1.1 2.2 NULL, 2.2 2.2 NULL, 2.2 1.1 NULL, 7.1 7.2 NULL), (7.1 7.2 NULL, 20.2 20.2 NULL, 20.2 1.1 NULL, 70.1 70.2 NULL)), MULTIPOLYGON (((10.1 20.2 NULL, 20.2 20.2 NULL, 20.2 10.1 NULL, 10.1 20.2 NULL)), ((1.1 2.2 NULL, 2.2 2.2 NULL, 2.2 1.1 NULL, 1.1 2.2 NULL))))'));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Full Name', N'Geometry') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] OFF;
""");
    }

    // The test data we're using is geographic but is represented in NTS as a GeometryCollection
    protected override string GetGeometryCollectionStoreType()
        => "geography";

    public override void InsertDataOperation_required_args()
    {
        base.InsertDataOperation_required_args();

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] ON;
INSERT INTO [dbo].[People] ([First Name])
VALUES (N'John');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] OFF;
""");
    }

    public override void InsertDataOperation_required_args_composite()
    {
        base.InsertDataOperation_required_args_composite();

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name', N'Last Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] ON;
INSERT INTO [dbo].[People] ([First Name], [Last Name])
VALUES (N'John', N'Snow');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name', N'Last Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] OFF;
""");
    }

    public override void InsertDataOperation_required_args_multiple_rows()
    {
        base.InsertDataOperation_required_args_multiple_rows();

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] ON;
INSERT INTO [dbo].[People] ([First Name])
VALUES (N'John'),
(N'Daenerys');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] OFF;
""");
    }

    [ConditionalFact]
    public virtual void InsertDataOperation_max_batch_size_is_respected()
    {
        // The SQL Server max batch size is 42 by default
        var values = new object[50, 1];
        for (var i = 0; i < 50; i++)
        {
            values[i, 0] = "Foo" + i;
        }

        Generate(
            CreateGotModel,
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["First Name"],
                Values = values
            });

        AssertSql(
            """
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] ON;
INSERT INTO [dbo].[People] ([First Name])
VALUES (N'Foo0'),
(N'Foo1'),
(N'Foo2'),
(N'Foo3'),
(N'Foo4'),
(N'Foo5'),
(N'Foo6'),
(N'Foo7'),
(N'Foo8'),
(N'Foo9'),
(N'Foo10'),
(N'Foo11'),
(N'Foo12'),
(N'Foo13'),
(N'Foo14'),
(N'Foo15'),
(N'Foo16'),
(N'Foo17'),
(N'Foo18'),
(N'Foo19'),
(N'Foo20'),
(N'Foo21'),
(N'Foo22'),
(N'Foo23'),
(N'Foo24'),
(N'Foo25'),
(N'Foo26'),
(N'Foo27'),
(N'Foo28'),
(N'Foo29'),
(N'Foo30'),
(N'Foo31'),
(N'Foo32'),
(N'Foo33'),
(N'Foo34'),
(N'Foo35'),
(N'Foo36'),
(N'Foo37'),
(N'Foo38'),
(N'Foo39'),
(N'Foo40'),
(N'Foo41');
INSERT INTO [dbo].[People] ([First Name])
VALUES (N'Foo42'),
(N'Foo43'),
(N'Foo44'),
(N'Foo45'),
(N'Foo46'),
(N'Foo47'),
(N'Foo48'),
(N'Foo49');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'First Name') AND [object_id] = OBJECT_ID(N'[dbo].[People]'))
    SET IDENTITY_INSERT [dbo].[People] OFF;
""");
    }

    public override void InsertDataOperation_throws_for_unsupported_column_types()
        => base.InsertDataOperation_throws_for_unsupported_column_types();

    public override void DeleteDataOperation_all_args()
    {
        base.DeleteDataOperation_all_args();

        AssertSql(
            """
DELETE FROM [People]
WHERE [First Name] = N'Hodor';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Daenerys';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'John';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Arya';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Harry';
SELECT @@ROWCOUNT;

""");
    }

    public override void DeleteDataOperation_all_args_composite()
    {
        base.DeleteDataOperation_all_args_composite();

        AssertSql(
            """
DELETE FROM [People]
WHERE [First Name] = N'Hodor' AND [Last Name] IS NULL;
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'John' AND [Last Name] = N'Snow';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Arya' AND [Last Name] = N'Stark';
SELECT @@ROWCOUNT;

DELETE FROM [People]
WHERE [First Name] = N'Harry' AND [Last Name] = N'Strickland';
SELECT @@ROWCOUNT;

""");
    }

    public override void DeleteDataOperation_required_args()
    {
        base.DeleteDataOperation_required_args();

        AssertSql(
            """
DELETE FROM [People]
WHERE [Last Name] = N'Snow';
SELECT @@ROWCOUNT;

""");
    }

    public override void DeleteDataOperation_required_args_composite()
    {
        base.DeleteDataOperation_required_args_composite();

        AssertSql(
            """
DELETE FROM [People]
WHERE [First Name] = N'John' AND [Last Name] = N'Snow';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_all_args()
    {
        base.UpdateDataOperation_all_args();

        AssertSql(
            """
UPDATE [People] SET [Birthplace] = N'Winterfell', [House Allegiance] = N'Stark', [Culture] = N'Northmen'
WHERE [First Name] = N'Hodor';
SELECT @@ROWCOUNT;

UPDATE [People] SET [Birthplace] = N'Dragonstone', [House Allegiance] = N'Targaryen', [Culture] = N'Valyrian'
WHERE [First Name] = N'Daenerys';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_all_args_composite()
    {
        base.UpdateDataOperation_all_args_composite();

        AssertSql(
            """
UPDATE [People] SET [House Allegiance] = N'Stark'
WHERE [First Name] = N'Hodor' AND [Last Name] IS NULL;
SELECT @@ROWCOUNT;

UPDATE [People] SET [House Allegiance] = N'Targaryen'
WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_all_args_composite_multi()
    {
        base.UpdateDataOperation_all_args_composite_multi();

        AssertSql(
            """
UPDATE [People] SET [Birthplace] = N'Winterfell', [House Allegiance] = N'Stark', [Culture] = N'Northmen'
WHERE [First Name] = N'Hodor' AND [Last Name] IS NULL;
SELECT @@ROWCOUNT;

UPDATE [People] SET [Birthplace] = N'Dragonstone', [House Allegiance] = N'Targaryen', [Culture] = N'Valyrian'
WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_all_args_multi()
    {
        base.UpdateDataOperation_all_args_multi();

        AssertSql(
            """
UPDATE [People] SET [Birthplace] = N'Dragonstone', [House Allegiance] = N'Targaryen', [Culture] = N'Valyrian'
WHERE [First Name] = N'Daenerys';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_required_args()
    {
        base.UpdateDataOperation_required_args();

        AssertSql(
            """
UPDATE [People] SET [House Allegiance] = N'Targaryen'
WHERE [First Name] = N'Daenerys';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_required_args_composite()
    {
        base.UpdateDataOperation_required_args_composite();

        AssertSql(
            """
UPDATE [People] SET [House Allegiance] = N'Targaryen'
WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_required_args_composite_multi()
    {
        base.UpdateDataOperation_required_args_composite_multi();

        AssertSql(
            """
UPDATE [People] SET [Birthplace] = N'Dragonstone', [House Allegiance] = N'Targaryen', [Culture] = N'Valyrian'
WHERE [First Name] = N'Daenerys' AND [Last Name] = N'Targaryen';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_required_args_multi()
    {
        base.UpdateDataOperation_required_args_multi();

        AssertSql(
            """
UPDATE [People] SET [Birthplace] = N'Dragonstone', [House Allegiance] = N'Targaryen', [Culture] = N'Valyrian'
WHERE [First Name] = N'Daenerys';
SELECT @@ROWCOUNT;

""");
    }

    public override void UpdateDataOperation_required_args_multiple_rows()
    {
        base.UpdateDataOperation_required_args_multiple_rows();

        AssertSql(
            """
UPDATE [People] SET [House Allegiance] = N'Stark'
WHERE [First Name] = N'Hodor';
SELECT @@ROWCOUNT;

UPDATE [People] SET [House Allegiance] = N'Targaryen'
WHERE [First Name] = N'Daenerys';
SELECT @@ROWCOUNT;

""");
    }

    public override void DefaultValue_with_line_breaks(bool isUnicode)
    {
        base.DefaultValue_with_line_breaks(isUnicode);

        var storeType = isUnicode ? "nvarchar(max)" : "varchar(max)";
        var unicodePrefix = isUnicode ? "N" : string.Empty;
        var unicodePrefixForType = isUnicode ? "n" : string.Empty;
        var expectedSql = @$"CREATE TABLE [dbo].[TestLineBreaks] (
    [TestDefaultValue] {storeType} NOT NULL DEFAULT CONCAT(CAST({unicodePrefixForType}char(13) AS {storeType}), {unicodePrefixForType}char(10), {unicodePrefix}'Various Line', {unicodePrefixForType}char(13), {unicodePrefix}'Breaks', {unicodePrefixForType}char(10))
);
";
        AssertSql(expectedSql);
    }

    public override void DefaultValue_with_line_breaks_2(bool isUnicode)
    {
        base.DefaultValue_with_line_breaks_2(isUnicode);

        var storeType = isUnicode ? "nvarchar(max)" : "varchar(max)";
        var unicodePrefix = isUnicode ? "N" : string.Empty;
        var unicodePrefixForType = isUnicode ? "n" : string.Empty;
        var expectedSql = @$"CREATE TABLE [dbo].[TestLineBreaks] (
    [TestDefaultValue] {storeType} NOT NULL DEFAULT CONCAT(CAST({unicodePrefix}'0' AS {storeType}), {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'1', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'2', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'3', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'4', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'5', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'6', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'7', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'8', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'9', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'10', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'11', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'12', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'13', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'14', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'15', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'16', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'17', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'18', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'19', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'20', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'21', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'22', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'23', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'24', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'25', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'26', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'27', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'28', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'29', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'30', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'31', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'32', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'33', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'34', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'35', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'36', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'37', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'38', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'39', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'40', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'41', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'42', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'43', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'44', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'45', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'46', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'47', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'48', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'49', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'50', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'51', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'52', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'53', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'54', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'55', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'56', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'57', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'58', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'59', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'60', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'61', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'62', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'63', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'64', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'65', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'66', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'67', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'68', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'69', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'70', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'71', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'72', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'73', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'74', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'75', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'76', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'77', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'78', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'79', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'80', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'81', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'82', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'83', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'84', CONCAT(CAST({unicodePrefixForType}char(13) AS {storeType}), {unicodePrefixForType}char(10), {unicodePrefix}'85', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'86', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'87', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'88', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'89', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'90', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'91', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'92', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'93', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'94', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'95', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'96', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'97', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'98', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'99', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'100', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'101', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'102', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'103', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'104', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'105', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'106', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'107', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'108', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'109', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'110', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'111', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'112', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'113', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'114', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'115', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'116', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'117', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'118', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'119', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'120', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'121', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'122', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'123', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'124', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'125', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'126', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'127', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'128', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'129', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'130', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'131', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'132', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'133', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'134', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'135', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'136', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'137', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'138', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'139', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'140', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'141', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'142', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'143', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'144', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'145', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'146', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'147', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'148', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'149', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'150', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'151', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'152', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'153', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'154', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'155', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'156', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'157', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'158', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'159', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'160', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'161', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'162', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'163', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'164', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'165', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'166', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'167', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'168', {unicodePrefixForType}char(13), CONCAT(CAST({unicodePrefixForType}char(10) AS {storeType}), {unicodePrefix}'169', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'170', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'171', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'172', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'173', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'174', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'175', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'176', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'177', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'178', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'179', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'180', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'181', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'182', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'183', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'184', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'185', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'186', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'187', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'188', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'189', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'190', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'191', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'192', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'193', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'194', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'195', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'196', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'197', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'198', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'199', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'200', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'201', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'202', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'203', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'204', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'205', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'206', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'207', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'208', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'209', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'210', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'211', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'212', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'213', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'214', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'215', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'216', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'217', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'218', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'219', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'220', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'221', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'222', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'223', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'224', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'225', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'226', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'227', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'228', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'229', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'230', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'231', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'232', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'233', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'234', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'235', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'236', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'237', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'238', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'239', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'240', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'241', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'242', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'243', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'244', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'245', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'246', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'247', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'248', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'249', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'250', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'251', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'252', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), CONCAT(CAST({unicodePrefix}'253' AS {storeType}), {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'254', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'255', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'256', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'257', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'258', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'259', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'260', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'261', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'262', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'263', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'264', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'265', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'266', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'267', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'268', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'269', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'270', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'271', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'272', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'273', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'274', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'275', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'276', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'277', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'278', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'279', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'280', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'281', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'282', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'283', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'284', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'285', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'286', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'287', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'288', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'289', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'290', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'291', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'292', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'293', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'294', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'295', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'296', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'297', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'298', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10), {unicodePrefix}'299', {unicodePrefixForType}char(13), {unicodePrefixForType}char(10)))))
);
";
        AssertSql(expectedSql);
    }

    public override void Sequence_restart_operation(long? startsAt)
    {
        base.Sequence_restart_operation(startsAt);

        var expectedSql = startsAt.HasValue
            ? @$"ALTER SEQUENCE [dbo].[TestRestartSequenceOperation] RESTART WITH {startsAt};"
            : @"ALTER SEQUENCE [dbo].[TestRestartSequenceOperation] RESTART;";
        AssertSql(expectedSql);
    }

    [ConditionalFact]
    public virtual void CreateIndex_generates_exec_when_legacy_filter_and_idempotent()
    {
        Generate(
            modelBuilder =>
            {
                modelBuilder
                    .HasAnnotation(CoreAnnotationNames.ProductVersion, "1.1.0")
                    .Entity("Table1").Property<int?>("Column1");
            },
            migrationBuilder => migrationBuilder.CreateIndex(
                name: "IX_Table1_Column1",
                table: "Table1",
                column: "Column1",
                unique: true),
            MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
EXEC(N'CREATE UNIQUE INDEX [IX_Table1_Column1] ON [Table1] ([Column1]) WHERE [Column1] IS NOT NULL');
""");
    }

    [ConditionalFact]
    public virtual void AlterColumn_make_required_with_idempotent()
    {
        Generate(
            new AlterColumnOperation
            {
                Table = "Person",
                Name = "Name",
                ClrType = typeof(string),
                IsNullable = false,
                DefaultValue = "",
                OldColumn = new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsNullable = true
                }
            },
            MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Person]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Person] DROP CONSTRAINT [' + @var0 + '];');
EXEC(N'UPDATE [Person] SET [Name] = N'''' WHERE [Name] IS NULL');
ALTER TABLE [Person] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
ALTER TABLE [Person] ADD DEFAULT N'' FOR [Name];
""");
    }

    private static void CreateGotModel(ModelBuilder b)
        => b.HasDefaultSchema("dbo").Entity(
            "Person", pb =>
            {
                pb.ToTable("People");
                pb.Property<string>("FirstName").HasColumnName("First Name");
                pb.Property<string>("LastName").HasColumnName("Last Name");
                pb.Property<string>("Birthplace").HasColumnName("Birthplace");
                pb.Property<string>("Allegiance").HasColumnName("House Allegiance");
                pb.Property<string>("Culture").HasColumnName("Culture");
                pb.HasKey("FirstName", "LastName");
            });

    public SqlServerMigrationsSqlGeneratorTest()
        : base(
            SqlServerTestHelpers.Instance,
            new ServiceCollection().AddEntityFrameworkSqlServerNetTopologySuite(),
            SqlServerTestHelpers.Instance.AddProviderOptions(
                ((IRelationalDbContextOptionsBuilderInfrastructure)
                    new SqlServerDbContextOptionsBuilder(new DbContextOptionsBuilder()).UseNetTopologySuite())
                .OptionsBuilder).Options)
    {
    }
}

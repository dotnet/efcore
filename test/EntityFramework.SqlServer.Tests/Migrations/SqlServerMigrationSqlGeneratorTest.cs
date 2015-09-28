// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Migrations
{
    public class SqlServerMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationsSqlGenerator SqlGenerator
        {
            get
            {
                var typeMapper = new SqlServerTypeMapper();

                return new SqlServerMigrationsSqlGenerator(
                    new RelationalCommandBuilderFactory(typeMapper),
                    new SqlServerSqlGenerator(),
                    typeMapper,
                    new SqlServerAnnotationProvider());
            }
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
                    IsNullable = false,
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy] =
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

        [Fact]
        public virtual void AddPrimaryKeyOperation_nonclustered()
        {
            Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "Id" },
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] = false
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
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'dbo.People') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + ']');" + EOL +
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
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'People') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + ']');" + EOL +
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
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy] =
                        SqlServerValueGenerationStrategy.IdentityColumn
                });

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'People') AND [c].[name] = N'Id');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var0 + ']');" + EOL +
                "ALTER TABLE [People] ALTER COLUMN [Id] int NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new SqlServerCreateDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                "CREATE DATABASE [Northwind];" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON');" + EOL,
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
        public virtual void CreateIndexOperation_clustered()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] = true
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
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] = true
                });

            Assert.Equal(
                "CREATE UNIQUE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateIndexOperation_unique_nonclustered()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] = false
                });

            Assert.Equal(
                "CREATE UNIQUE NONCLUSTERED INDEX [IX_People_Name] ON [People] ([Name]) WHERE [Name] IS NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation()
        {
            Generate(new EnsureSchemaOperation { Name = "my" });

            Assert.Equal(
                "IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my]');" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation_dbo()
        {
            Generate(new EnsureSchemaOperation { Name = "dbo" });

            Assert.Equal(
                ";" + EOL,
                Sql);
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            Assert.Equal(
                "DECLARE @var0 sysname;" + EOL +
                "SELECT @var0 = [d].[name]" + EOL +
                "FROM [sys].[default_constraints] [d]" + EOL +
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]" + EOL +
                "WHERE ([d].[parent_object_id] = OBJECT_ID(N'dbo.People') AND [c].[name] = N'LuckyNumber');" + EOL +
                "IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[People] DROP CONSTRAINT [' + @var0 + ']');" + EOL +
                "ALTER TABLE [dbo].[People] DROP COLUMN [LuckyNumber];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropDatabaseOperation()
        {
            Generate(new SqlServerDropDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE');" + EOL +
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
                "EXEC sp_rename N'dbo.People.Name', N'FullName', 'COLUMN';" + EOL,
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
                "EXEC sp_rename N'dbo.People.IX_People_Name', N'IX_People_FullName', 'INDEX';" + EOL,
                Sql);
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
    }
}

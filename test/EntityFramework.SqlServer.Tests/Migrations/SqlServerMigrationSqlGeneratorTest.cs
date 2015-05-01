// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationSqlGenerator SqlGenerator =>
            new SqlServerMigrationSqlGenerator(new SqlServerSqlGenerator());

        [Fact]
        public virtual void AddColumnOperation_with_computedSql()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    Type = "nvarchar(30)",
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression] =
                        "FirstName + ' ' + LastName"
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD [FullName] AS FirstName + ' ' + LastName;" + EOL,
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
                    Type = "int",
                    IsNullable = false,
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration] =
                        SqlServerValueGenerationStrategy.Identity.ToString()
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;" + EOL,
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
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] = bool.FalseString
                });

            Assert.Equal(
                "ALTER TABLE [People] ADD PRIMARY KEY NONCLUSTERED ([Id]);" + EOL,
                Sql);
        }

        public override void AlterColumnOperation()
        {
            base.AlterColumnOperation();

            Assert.Equal(
                "ALTER TABLE [dbo].[People] ALTER COLUMN [LuckyNumber] int NOT NULL DEFAULT 7;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new CreateDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                "CREATE DATABASE [Northwind]" + EOL +
                "GO" + EOL +
                EOL +
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N'ALTER DATABASE [Northwind] SET READ_COMMITTED_SNAPSHOT ON';" + EOL,
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
                    [SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] = bool.TrueString
                });

            Assert.Equal(
                "CREATE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropDatabaseOperation()
        {
            Generate(new DropDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                "IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N'ALTER DATABASE [Northwind] SET SINGLE_USER WITH ROLLBACK IMMEDIATE'" + EOL +
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
                    Name = "DefaultSequence",
                    Schema = "dbo",
                    NewSchema = "my"
                });

            Assert.Equal(
                "ALTER SCHEMA [my] TRANSFER [dbo].[DefaultSequence];" + EOL,
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
                "EXECUTE sp_rename 'dbo.People.Name', 'FullName', 'COLUMN';" + EOL,
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
                "EXECUTE sp_rename 'dbo.People.IX_People_Name', 'IX_People_FullName', 'INDEX';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameSequenceOperation()
        {
            Generate(
                new RenameSequenceOperation
                {
                    Name = "DefaultSequence",
                    Schema = "dbo",
                    NewName = "MySequence"
                });

            Assert.Equal(
                "EXECUTE sp_rename 'dbo.DefaultSequence', 'MySequence';" + EOL,
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
                "EXECUTE sp_rename 'dbo.People', 'Person';" + EOL,
                Sql);
        }
    }
}

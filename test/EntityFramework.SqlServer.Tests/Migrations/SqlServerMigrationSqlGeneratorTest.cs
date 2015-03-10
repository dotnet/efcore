using System.Collections.Generic;
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
                new AddColumnOperation(
                    "People",
                    /*schema:*/ null,
                    new ColumnModel(
                        "FullName",
                        storeType: null,
                        nullable: false,
                        defaultValue: null,
                        defaultValueSql: null,
                        annotations: new Dictionary<string, string>
                        {
                            {
                                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression,
                                "FirstName + ' ' + LastName"
                            }
                        })));

            Assert.Equal(
                "ALTER TABLE [People] ADD [FullName] AS FirstName + ' ' + LastName;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AddColumnOperation_identity()
        {
            Generate(
                new AddColumnOperation(
                    "People",
                    /*schema:*/ null,
                    new ColumnModel(
                        "Id",
                        "int",
                        nullable: false,
                        defaultValue: null,
                        defaultValueSql: null,
                        annotations: new Dictionary<string, string>
                        {
                            {
                                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration,
                                SqlServerValueGenerationStrategy.Identity.ToString()
                            }
                        })));

            Assert.Equal(
                "ALTER TABLE [People] ADD [Id] int NOT NULL IDENTITY;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_nonclustered()
        {
            Generate(
                new AddPrimaryKeyOperation(
                    "People",
                    /*schema:*/ null,
                    /*name:*/ null,
                    new[] { "Id" },
                    new Dictionary<string, string>
                    {
                        {
                            SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered,
                            bool.FalseString
                        }
                    }));

            Assert.Equal(
                "ALTER TABLE [People] ADD PRIMARY KEY NONCLUSTERED ([Id]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new CreateDatabaseOperation("Northwind"));

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
                new CreateIndexOperation(
                    "IX_People_Name",
                    "People",
                    /*schema:*/ null,
                    new[] { "Name" },
                    unique: false,
                    annotations: new Dictionary<string, string>
                    {
                        {
                            SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered,
                            bool.TrueString
                        }
                    }));

            Assert.Equal(
                "CREATE CLUSTERED INDEX [IX_People_Name] ON [People] ([Name]);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropDatabaseOperation()
        {
            Generate(new DropDatabaseOperation("Northwind"));

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
                new MoveSequenceOperation(
                    "DefaultSequence",
                    "dbo",
                    "my"));

            Assert.Equal(
                "ALTER SCHEMA [my] TRANSFER [dbo].[DefaultSequence];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void MoveTableOperation()
        {
            Generate(
                new MoveTableOperation(
                    "People",
                    "dbo",
                    "hr"));

            Assert.Equal(
                "ALTER SCHEMA [hr] TRANSFER [dbo].[People];" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameColumnOperation()
        {
            Generate(
                new RenameColumnOperation(
                    "People",
                    "dbo",
                    "Name",
                    "FullName"));

            Assert.Equal(
                "EXECUTE sp_rename @objname = N'dbo.People.Name', @newname = N'FullName', @objtype = N'COLUMN';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameIndexOperation()
        {
            Generate(
                new RenameIndexOperation(
                    "People",
                    "dbo",
                    "IX_People_Name",
                    "IX_People_FullName"));

            Assert.Equal(
                "EXECUTE sp_rename @objname = N'dbo.People.IX_People_Name', @newname = N'IX_People_FullName', @objtype = N'INDEX';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameSequenceOperation()
        {
            Generate(
                new RenameSequenceOperation(
                    "DefaultSequence",
                    "dbo",
                    "MySequence"));

            Assert.Equal(
                "EXECUTE sp_rename @objname = N'dbo.DefaultSequence', @newname = N'MySequence', @objtype = N'OBJECT';" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameTableOperation()
        {
            Generate(
                new RenameTableOperation(
                    "People",
                    "dbo",
                    "Person"));

            Assert.Equal(
                "EXECUTE sp_rename @objname = N'dbo.People', @newname = N'Person', @objtype = N'OBJECT';" + EOL,
                Sql);
        }
    }
}

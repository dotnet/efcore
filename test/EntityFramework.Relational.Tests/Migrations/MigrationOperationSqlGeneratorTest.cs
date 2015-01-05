// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations
{
    public class MigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            Assert.Equal(
                @"CREATE DATABASE ""MyDatabase""",
                Generate(new CreateDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"DROP DATABASE ""MyDatabase""",
                Generate(new DropDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CREATE SEQUENCE ""dbo"".""MySequence"" AS bigint START WITH 0 INCREMENT BY 1",
                Generate(new CreateSequenceOperation("dbo.MySequence", 0, 1)));
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DROP SEQUENCE ""dbo"".""MySequence""",
                Generate(new DropSequenceOperation("dbo.MySequence")));
        }

        [Fact]
        public void Generate_when_alter_sequence_operation()
        {
            Assert.Equal(
                @"ALTER SEQUENCE ""dbo"".""MySequence"" INCREMENT BY 7",
                Generate(new AlterSequenceOperation("dbo.MySequence", 7)));
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                    {
                        b.Property<int>("Foo").ForRelational().DefaultValue(5);
                        b.Property<int?>("Bar");
                        b.ForRelational().Table("MyTable", "dbo");
                        b.Key("Foo", "Bar").ForRelational().Name("MyPK");
                    });

            var operation = OperationFactory().CreateTableOperation(model.GetEntityType("E"));

            Assert.Equal(
                @"CREATE TABLE ""dbo"".""MyTable"" (
    ""Foo"" integer NOT NULL DEFAULT 5,
    ""Bar"" integer,
    CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo"", ""Bar"")
)",
                Generate(operation, model));
        }

        [Fact]
        public void Generate_when_create_table_with_unique_constraints()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").ForRelational().DefaultValue(5);
                    var bar = b.Property<int?>("Bar").Metadata;
                    var c1 = b.Property<string>("C1").Metadata;
                    var c2 = b.Property<string>("C2").Metadata;
                    b.ForRelational().Table("MyTable", "dbo");
                    b.Key("Foo").ForRelational().Name("MyPK");
                    b.Metadata.AddKey(c1).Relational().Name = "MyUC0";
                    b.Metadata.AddKey(new[] { bar, c2 }).Relational().Name = "MyUC1";
                });

            var operation = OperationFactory().CreateTableOperation(model.GetEntityType("E"));

            Assert.Equal(
                @"CREATE TABLE ""dbo"".""MyTable"" (
    ""Foo"" integer NOT NULL DEFAULT 5,
    ""Bar"" integer,
    ""C1"" varchar(4000),
    ""C2"" varchar(4000),
    CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo""),
    CONSTRAINT ""MyUC0"" UNIQUE (""C1""),
    CONSTRAINT ""MyUC1"" UNIQUE (""Bar"", ""C2"")
)",
                Generate(operation, model));
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DROP TABLE ""dbo"".""MyTable""",
                Generate(new DropTableOperation("dbo.MyTable")));
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("Bar").ForRelational().DefaultValue(5);
                        b.Key("Id");
                        b.ForRelational().Table("MyTable", "dbo");
                    });

            var operation = new AddColumnOperation(
                "dbo.MyTable", OperationFactory().Column(model.GetEntityType("E").GetProperty("Bar")));

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD ""Bar"" integer NOT NULL DEFAULT 5",
                Generate(operation, model));
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP COLUMN ""Foo""",
                Generate(new DropColumnOperation("dbo.MyTable", "Foo")));
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_nullable()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<int?>("Foo");
                    b.Key("Id");
                    b.ForRelational().Table("MyTable", "dbo");
                });

            var operation = new AlterColumnOperation(
                "dbo.MyTable",
                OperationFactory().Column(model.GetEntityType("E").GetProperty("Foo")),
                isDestructiveChange: false);

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" integer NULL",
                Generate(operation, model));
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_not_nullable()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<int>("Foo");
                    b.Property<int>("Foo").Metadata.IsNullable = false;
                    b.Key("Id");
                    b.ForRelational().Table("MyTable", "dbo");
                });

            var operation = new AlterColumnOperation(
                "dbo.MyTable",
                OperationFactory().Column(model.GetEntityType("E").GetProperty("Foo")), 
                isDestructiveChange: false);

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" integer NOT NULL",
                Generate(operation, model));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_value()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT 'MyDefault'",
                Generate(new AddDefaultConstraintOperation("dbo.MyTable", "Foo", "MyDefault", null)));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT GETDATE()",
                Generate(new AddDefaultConstraintOperation("dbo.MyTable", "Foo", null, "GETDATE()")));
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" DROP DEFAULT",
                Generate(new DropDefaultConstraintOperation("dbo.MyTable", "Foo")));
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo"", ""Bar"")",
                Generate(new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false)));
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyPK""",
                Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")));
        }

        [Fact]
        public void Generate_when_add_unique_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyUC"" UNIQUE (""Foo"", ""Bar"")",
                Generate(new AddUniqueConstraintOperation("dbo.MyTable", "MyUC", new[] { "Foo", "Bar" })));
        }

        [Fact]
        public void Generate_when_drop_unique_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyUC""",
                Generate(new DropUniqueConstraintOperation("dbo.MyTable", "MyUC")));
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyFK"" FOREIGN KEY (""Foo"", ""Bar"") REFERENCES ""dbo"".""MyTable2"" (""Foo2"", ""Bar2"") ON DELETE CASCADE",
                Generate(new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                    "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true)));
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyFK""",
                Generate(new DropForeignKeyOperation("dbo.MyTable", "MyFK")));
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CREATE UNIQUE CLUSTERED INDEX ""MyIndex"" ON ""dbo"".""MyTable"" (""Foo"", ""Bar"")",
                Generate(new CreateIndexOperation("dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                    isUnique: true, isClustered: true)));
        }

        [Fact]
        public void Generate_when_drop_index_operation()
        {
            Assert.Equal(
                @"DROP INDEX ""MyIndex""",
                Generate(new DropIndexOperation("dbo.MyTable", "MyIndex")));
        }

        [Fact]
        public void Generate_when_copy_data_operation()
        {
            Assert.Equal(
                @"INSERT INTO ""dbo"".""T2"" ( ""C"", ""D"" )
    SELECT ""A"", ""B"" FROM ""dbo"".""T1""",
                Generate(new CopyDataOperation("dbo.T1", new[] { "A", "B" }, "dbo.T2", new[] { "C", "D" })));
        }

        [Fact]
        public void Generate_when_sql_operation()
        {
            const string sql =
                @"UPDATE T
    SET C1='V1'
    WHERE C2='V2'";

            var batches = SqlGenerator().Generate(new SqlOperation(sql)).ToList();

            Assert.Equal(1, batches.Count);
            Assert.Equal(sql, batches[0].Sql);
            Assert.False(batches[0].SuppressTransaction);
        }

        [Fact]
        public void Generate_when_sql_operation_with_suppress_transaction_true()
        {
            const string sql =
                @"UPDATE T
    SET C1='V1'
    WHERE C2='V2'";

            var batches = SqlGenerator().Generate(new SqlOperation(sql) { SuppressTransaction = true }).ToList();

            Assert.Equal(1, batches.Count);
            Assert.Equal(sql, batches[0].Sql);
            Assert.True(batches[0].SuppressTransaction);
        }

        [Fact]
        public void Generate_multiple_operations_batches_them_properly()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("Customer",
                b =>
                {
                    b.Property<string>("FirstName");
                    b.Property<string>("LastName");
                    b.ForRelational().Table("Customers", "dbo");
                    b.Key("FirstName", "LastName").ForRelational().Name("CustomerPK");
                });

            modelBuilder.Entity("Order",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<decimal?>("Quantity");
                    b.ForRelational().Table("Orders", "dbo");
                    b.Key("Id").ForRelational().Name("OrderPK");
                });

            var operations = new List<MigrationOperation>
            {
                new CreateDatabaseOperation("CustomeOrderDb"),
                OperationFactory().CreateTableOperation(model.GetEntityType("Customer")),
                OperationFactory().CreateTableOperation(model.GetEntityType("Order")),
                new DropDatabaseOperation("CustomeOrderDb"),
            };

            var batches = SqlGenerator(model).Generate(operations).ToList();

            Assert.Equal(1, batches.Count);
            Assert.Equal(
@"CREATE DATABASE ""CustomeOrderDb"";
CREATE TABLE ""dbo"".""Customers"" (
    ""FirstName"" varchar(4000),
    ""LastName"" varchar(4000),
    CONSTRAINT ""CustomerPK"" PRIMARY KEY (""FirstName"", ""LastName"")
);
CREATE TABLE ""dbo"".""Orders"" (
    ""Id"" integer NOT NULL,
    ""Quantity"" decimal(18, 2),
    CONSTRAINT ""OrderPK"" PRIMARY KEY (""Id"")
);
DROP DATABASE ""CustomeOrderDb""", batches[0].Sql);
        }

        [Fact]
        public void Delimit_identifier()
        {
            Assert.Equal("\"foo\"\"bar\"", SqlGenerator().DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_identifier_when_schema_qualified()
        {
            Assert.Equal("\"foo\".\"bar\"", SqlGenerator().DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void Escape_identifier()
        {
            Assert.Equal("foo\"\"bar", SqlGenerator().EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            Assert.Equal("'foo''bar'", SqlGenerator().GenerateLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            Assert.Equal("foo''bar", SqlGenerator().EscapeLiteral("foo'bar"));
        }

        private static MigrationOperationSqlGenerator SqlGenerator(IModel targetModel = null)
        {
            var sqlGenerator = MigrationsTestHelpers.MockSqlGenerator(callBase: true).Object;

            sqlGenerator.TargetModel = targetModel ?? new Entity.Metadata.Model();

            return sqlGenerator;
        }

        private static string Generate(MigrationOperation operation, IModel targetModel = null)
        {
            var batches = SqlGenerator(targetModel).Generate(operation);

            return string.Join(Environment.NewLine, batches.Select(b => b.Sql));
        }

        private static MigrationOperationFactory OperationFactory()
        {
            return new MigrationOperationFactory(RelationalTestHelpers.ExtensionProvider());
        }
    }
}

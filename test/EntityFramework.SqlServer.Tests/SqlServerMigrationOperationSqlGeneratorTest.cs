// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            var batches = SqlGenerator().Generate(new CreateDatabaseOperation("MyDatabase")).ToList();

            Assert.Equal(2, batches.Count);
            Assert.Equal(@"CREATE DATABASE [MyDatabase]", batches[0].Sql);
            Assert.Equal(@"IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N'ALTER DATABASE [MyDatabase] SET READ_COMMITTED_SNAPSHOT ON'", batches[1].Sql);
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N'ALTER DATABASE [MyDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE'
DROP DATABASE [MyDatabase]",
                Generate(new DropDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CREATE SEQUENCE [dbo].[MySequence] AS bigint START WITH 0 INCREMENT BY 1",
                Generate(new CreateSequenceOperation("dbo.MySequence", 0, 1)));
        }

        [Fact]
        public void Generate_when_create_sequence_operation_with_non_default_schema()
        {
            Assert.Equal(
@"IF schema_id('abc') IS NULL
    EXECUTE('CREATE SCHEMA [abc]')
CREATE SEQUENCE [abc].[MySequence] AS bigint START WITH 0 INCREMENT BY 1",
                Generate(new CreateSequenceOperation("abc.MySequence", 0, 1)));
        }

        [Fact]
        public void Generate_does_not_replicate_sql_for_creating_schema()
        {
            Assert.Equal(
@"IF schema_id('abc') IS NULL
    EXECUTE('CREATE SCHEMA [abc]')
CREATE SEQUENCE [abc].[S1] AS bigint START WITH 0 INCREMENT BY 1;
CREATE SEQUENCE [abc].[S2] AS bigint START WITH 2 INCREMENT BY 3",
                Generate(
                    new[]
                        {
                            new CreateSequenceOperation("abc.S1", 0, 1),
                            new CreateSequenceOperation("abc.S2", 2, 3)
                        }));
        }

        [Fact]
        public void Generate_when_move_sequence_operation()
        {
            Assert.Equal(
                @"ALTER SCHEMA [dbo2] TRANSFER [dbo].[MySequence]",
                Generate(new MoveSequenceOperation("dbo.MySequence", "dbo2")));
        }

        [Fact]
        public void Generate_when_rename_sequence_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MySequence', @newname = N'MySequence2', @objtype = N'OBJECT'",
                Generate(new RenameSequenceOperation("dbo.MySequence", "MySequence2")));
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DROP SEQUENCE [dbo].[MySequence]",
                Generate(new DropSequenceOperation("dbo.MySequence")));
        }

        [Fact]
        public void Generate_when_alter_sequence_operation()
        {
            Assert.Equal(
                @"ALTER SEQUENCE [dbo].[MySequence] INCREMENT BY 7",
                Generate(new AlterSequenceOperation("dbo.MySequence", 7)));
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").ForSqlServer().DefaultValue(5);
                    b.Property<int?>("Bar");
                    b.Property<int>("P1").StoreComputed().ForSqlServer().DefaultExpression("Foo + 1");
                    b.Property<int?>("P2").StoreComputed().ForSqlServer().DefaultExpression("Foo + 2");
                    b.ForSqlServer().Table("MyTable", "dbo");
                    b.Key("Foo", "Bar").ForSqlServer().Name("MyPK").Clustered(false);
                });

            var operation = OperationFactory().CreateTableOperation(targetModel.GetEntityType("E"));

            Assert.Equal(
                @"CREATE TABLE [dbo].[MyTable] (
    [Foo] int NOT NULL DEFAULT 5,
    [Bar] int,
    [P1] AS Foo + 1 PERSISTED NOT NULL,
    [P2] AS Foo + 2,
    CONSTRAINT [MyPK] PRIMARY KEY NONCLUSTERED ([Foo], [Bar])
)",
                Generate(operation, targetModel));
        }

        [Fact]
        public void Generate_when_create_table_operation_with_identity_key()
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").GenerateValueOnAdd();
                    b.Property<int?>("Bar");
                    b.ForSqlServer().Table("MyTable", "abc");
                    b.Key("Foo").ForSqlServer().Name("MyPK").Clustered(false);
                });

            var operation = OperationFactory().CreateTableOperation(targetModel.GetEntityType("E"));

            Assert.Equal(
@"IF schema_id('abc') IS NULL
    EXECUTE('CREATE SCHEMA [abc]')
CREATE TABLE [abc].[MyTable] (
    [Foo] int NOT NULL IDENTITY,
    [Bar] int,
    CONSTRAINT [MyPK] PRIMARY KEY NONCLUSTERED ([Foo])
)",
                Generate(operation, targetModel));
        }

        [Fact]
        public void Generate_when_create_table_operation_with_non_identity_key()
        {
            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("ExpenseCategory",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name").Metadata.IsNullable = false;
                    b.Key("Id").ForSqlServer().Name("PK_ExpenseCategory");
                });

            var migrationBuilder = new MigrationBuilder();
            migrationBuilder.CreateTable("ExpenseCategory",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(nullable: false)
                })
                .PrimaryKey("PK_ExpenseCategory", t => t.Id);

            Assert.Equal(
@"CREATE TABLE [ExpenseCategory] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ExpenseCategory] PRIMARY KEY ([Id])
)",
                Generate(migrationBuilder.Operations.Single(), targetModelBuilder.Model));
        }

        [Fact]
        public void Generate_when_create_table_operation_with_non_nullable_properties()
        {
            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("ExpenseLine",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Category").Metadata.IsNullable = false;
                    b.Property<string>("CostCenter").Metadata.IsNullable = false;
                    b.Property<DateTimeOffset>("Date");
                    b.Property<string>("Description").Metadata.IsNullable = false;
                    b.Property<string>("MerchantCity").Metadata.IsNullable = false;
                    b.Property<string>("MerchantName").Metadata.IsNullable = false;
                    b.Property<decimal>("TransactionAmount");
                    b.Property<int>("ExpenseReportId");
                    b.Key("Id").ForSqlServer().Name("PK_ExpenseLine");
                });

            var migrationBuilder = new MigrationBuilder();
            migrationBuilder.CreateTable("ExpenseLine",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Category = c.String(nullable: false),
                    CostCenter = c.String(nullable: false),
                    Date = c.DateTimeOffset(nullable: false),
                    Description = c.String(nullable: false),
                    MerchantCity = c.String(nullable: false),
                    MerchantName = c.String(nullable: false),
                    TransactionAmount = c.Decimal(nullable: false),
                    ExpenseReportId = c.Int(nullable: false)
                })
                .PrimaryKey("PK_ExpenseLine", t => t.Id);

            Assert.Equal(
@"CREATE TABLE [ExpenseLine] (
    [Id] int NOT NULL IDENTITY,
    [Category] nvarchar(max) NOT NULL,
    [CostCenter] nvarchar(max) NOT NULL,
    [Date] datetimeoffset NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [MerchantCity] nvarchar(max) NOT NULL,
    [MerchantName] nvarchar(max) NOT NULL,
    [TransactionAmount] decimal(18, 2) NOT NULL,
    [ExpenseReportId] int NOT NULL,
    CONSTRAINT [PK_ExpenseLine] PRIMARY KEY ([Id])
)",
                Generate(migrationBuilder.Operations.Single(), targetModelBuilder.Model));
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DROP TABLE [dbo].[MyTable]",
                Generate(new DropTableOperation("dbo.MyTable")));
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable', @newname = N'MyTable2', @objtype = N'OBJECT'",
                Generate(new RenameTableOperation("dbo.MyTable", "MyTable2")));
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            Assert.Equal(
                @"ALTER SCHEMA [dbo2] TRANSFER [dbo].[MyTable]",
                Generate(new MoveTableOperation("dbo.MyTable", "dbo2")));
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                    {
                        b.Property<int>("Bar").ForSqlServer().DefaultValue(5);
                        b.ForSqlServer().Table("MyTable", "dbo");
                    });

            var operation = new AddColumnOperation("dbo.MyTable",
                OperationFactory().Column(targetModel.GetEntityType("E").GetProperty("Bar")));

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD [Bar] int NOT NULL DEFAULT 5",
                Generate(operation, targetModel));
        }

        [Fact]
        public void Generate_when_add_column_operation_with_computed_column()
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                {
                    b.Property<int?>("Bar").ForSqlServer().DefaultValue(5);
                    b.Property<int?>("P").StoreComputed().ForSqlServer().DefaultExpression("Bar + 2");
                    b.ForSqlServer().Table("MyTable", "dbo");
                });

            var operation = new AddColumnOperation("dbo.MyTable",
                OperationFactory().Column(targetModel.GetEntityType("E").GetProperty("P")));

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD [P] AS Bar + 2",
                Generate(operation, targetModel));
        }

        [Fact]
        public void Generate_when_add_column_operation_with_persisted_computed_column()
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Bar").ForSqlServer().DefaultValue(5);
                    b.Property<int>("P").StoreComputed().ForSqlServer().DefaultExpression("Bar + 2");
                    b.ForSqlServer().Table("MyTable", "dbo");
                });

            var operation = new AddColumnOperation("dbo.MyTable",
                OperationFactory().Column(targetModel.GetEntityType("E").GetProperty("P")));

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD [P] AS Bar + 2 PERSISTED NOT NULL",
                Generate(operation, targetModel));
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] DROP COLUMN [Foo]",
                Generate(new DropColumnOperation("dbo.MyTable", "Foo"), new Model()));
        }

        [Fact]
        public void Generate_when_alter_column_operation()
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").Metadata.IsNullable = false;
                    b.ForSqlServer().Table("MyTable", "dbo");
                });

            var operation = new AlterColumnOperation("dbo.MyTable",
                OperationFactory().Column(targetModel.GetEntityType("E").GetProperty("Foo")),
                isDestructiveChange: false);

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ALTER COLUMN [Foo] int NOT NULL",
                Generate(operation, targetModel));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD CONSTRAINT [DF_dbo.MyTable_Foo] DEFAULT 5 FOR [Foo]",
                Generate(new AddDefaultConstraintOperation("dbo.MyTable", "Foo", 5, null)));
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"DECLARE @var0 nvarchar(128)
SELECT @var0 = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.MyTable') AND COL_NAME(parent_object_id, parent_column_id) = N'Foo'
EXECUTE('ALTER TABLE [dbo].[MyTable] DROP CONSTRAINT ""' + @var0 + '""')",
                Generate(new DropDefaultConstraintOperation("dbo.MyTable", "Foo")));
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable.Foo', @newname = N'Foo2', @objtype = N'COLUMN'",
                Generate(new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2")));
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD CONSTRAINT [MyPK] PRIMARY KEY NONCLUSTERED ([Foo], [Bar])",
                Generate(
                    new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false)));
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] DROP CONSTRAINT [MyPK]",
                Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")));
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD CONSTRAINT [MyFK] FOREIGN KEY ([Foo], [Bar]) REFERENCES [dbo].[MyTable2] ([Foo2], [Bar2]) ON DELETE CASCADE",
                Generate(new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                    "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true)));
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable2] DROP CONSTRAINT [MyFK]",
                Generate(new DropForeignKeyOperation("dbo.MyTable2", "MyFK")));
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CREATE UNIQUE CLUSTERED INDEX [MyIndex] ON [dbo].[MyTable] ([Foo], [Bar])",
                Generate(new CreateIndexOperation("dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                    isUnique: true, isClustered: true)));
        }

        [Fact]
        public void Generate_when_drop_index_operation()
        {
            Assert.Equal(
                @"DROP INDEX [MyIndex] ON [dbo].[MyTable]",
                Generate(new DropIndexOperation("dbo.MyTable", "MyIndex")));
        }

        [Fact]
        public void Generate_when_rename_index_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable.MyIndex', @newname = N'MyIndex2', @objtype = N'INDEX'",
                Generate(new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2")));
        }

        [Fact]
        public void GenerateDataType_for_string_thats_not_a_key()
        {
            Assert.Equal("nvarchar(max)", GenerateDataType<string>());
        }

        [Fact]
        public void GenerateDataType_for_string_key()
        {
            Assert.Equal("nvarchar(450)", GenerateDataType<string>(isKey: true));
        }

        [Fact]
        public void GenerateDataType_for_DateTime()
        {
            Assert.Equal("datetime2", GenerateDataType<DateTime>());
        }

        [Fact]
        public void GenerateDataType_for_decimal()
        {
            Assert.Equal("decimal(18, 2)", GenerateDataType<decimal>());
        }

        [Fact]
        public void GenerateDataType_for_Guid()
        {
            Assert.Equal("uniqueidentifier", GenerateDataType<Guid>());
        }

        [Fact]
        public void GenerateDataType_for_bool()
        {
            Assert.Equal("bit", GenerateDataType<bool>());
        }

        [Fact]
        public void GenerateDataType_for_byte()
        {
            Assert.Equal("tinyint", GenerateDataType<byte>());
        }

        [Fact]
        public void GenerateDataType_for_char()
        {
            Assert.Equal("int", GenerateDataType<int>());
        }

        [Fact]
        public void GenerateDataType_for_double()
        {
            Assert.Equal("float", GenerateDataType<double>());
        }

        [Fact]
        public void GenerateDataType_for_short()
        {
            Assert.Equal("smallint", GenerateDataType<short>());
        }

        [Fact]
        public void GenerateDataType_for_long()
        {
            Assert.Equal("bigint", GenerateDataType<long>());
        }

        [Fact]
        public void GenerateDataType_for_sbyte()
        {
            Assert.Equal("smallint", GenerateDataType<sbyte>());
        }

        [Fact]
        public void GenerateDataType_for_float()
        {
            Assert.Equal("real", GenerateDataType<float>());
        }

        [Fact]
        public void GenerateDataType_for_ushort()
        {
            Assert.Equal("int", GenerateDataType<ushort>());
        }

        [Fact]
        public void GenerateDataType_for_uint()
        {
            Assert.Equal("bigint", GenerateDataType<uint>());
        }

        [Fact]
        public void GenerateDataType_for_ulong()
        {
            Assert.Equal("numeric(20, 0)", GenerateDataType<ulong>());
        }

        [Fact]
        public void GenerateDataType_for_DateTimeOffset()
        {
            Assert.Equal("datetimeoffset", GenerateDataType<DateTimeOffset>());
        }

        [Fact]
        public void GenerateDataType_for_byte_array_that_is_not_a_concurrency_token_or_a_primary_key()
        {
            Assert.Equal("varbinary(max)", GenerateDataType<byte[]>());
        }

        [Fact]
        public void GenerateDataType_for_byte_array_key()
        {
            Assert.Equal("varbinary(900)", GenerateDataType<byte[]>(isKey: true));
        }

        [Fact]
        public void GenerateDataType_for_byte_array_concurrency_token()
        {
            Assert.Equal("rowversion", GenerateDataType<byte[]>(isKey: false, isConcurrencyToken: true));
        }

        private static string GenerateDataType<T>(bool isKey = false, bool isConcurrencyToken = false)
        {
            var targetModel = new Model();
            var targetModelBuilder = new BasicModelBuilder(targetModel);
            targetModelBuilder.Entity("E",
                b =>
                {
                    b.Property<T>("P");
                    if (isKey)
                    {
                        b.Key("P");
                    }
                    if (isConcurrencyToken)
                    {
                        b.Property<T>("P").Metadata.IsConcurrencyToken = true;
                    }
                });

            var property = targetModel.GetEntityType("E").GetProperty("P");
            var batchBuilder = new SqlBatchBuilder();

            SqlGenerator(targetModel).GenerateColumnType(
                new SchemaQualifiedName(
                    property.EntityType.SqlServer().Table,
                    property.EntityType.SqlServer().Schema),
                OperationFactory().Column(property),
                batchBuilder);

            batchBuilder.EndBatch();

            return string.Join(Environment.NewLine, batchBuilder.SqlBatches.Select(b => b.Sql));
        }

        [Fact]
        public void Delimit_identifier()
        {
            Assert.Equal("[foo[]]bar]", SqlGenerator().DelimitIdentifier("foo[]bar"));
        }

        [Fact]
        public void Escape_identifier()
        {
            Assert.Equal("foo[]]]]bar", SqlGenerator().EscapeIdentifier("foo[]]bar"));
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

        private static string Generate(MigrationOperation migrationOperation, IModel targetModel = null)
        {
            var batches = SqlGenerator(targetModel).Generate(migrationOperation);

            return string.Join(Environment.NewLine, batches.Select(b => b.Sql));        
        }

        private static string Generate(IEnumerable<MigrationOperation> migrationOperations, IModel targetModel = null)
        {
            var batches = SqlGenerator(targetModel).Generate(migrationOperations);

            return string.Join(Environment.NewLine, batches.Select(b => b.Sql));
        }

        private static SqlServerMigrationOperationSqlGenerator SqlGenerator(IModel targetModel = null)
        {
            return
                new SqlServerMigrationOperationSqlGenerator(
                    new SqlServerMetadataExtensionProvider(), 
                    new SqlServerTypeMapper())
                    {
                        TargetModel = targetModel ?? new Model()
                    };
        }

        private static SqlServerMigrationOperationFactory OperationFactory()
        {
            return new SqlServerMigrationOperationFactory(new SqlServerMetadataExtensionProvider());
        }
    }
}

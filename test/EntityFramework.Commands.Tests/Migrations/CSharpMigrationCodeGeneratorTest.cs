// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Tests;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Tests.Migrations
{
    public class CSharpMigrationCodeGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            var operation = new CreateDatabaseOperation("MyDatabase");

            Assert.Equal(
                @"CreateDatabase(""MyDatabase"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            var operation = new DropDatabaseOperation("MyDatabase");

            Assert.Equal(
                @"DropDatabase(""MyDatabase"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            var operation = new CreateSequenceOperation("dbo.MySequence", 10, 5, 1, 100, typeof(int));

            Assert.Equal(
                @"CreateSequence(""dbo.MySequence"", 10, 5, 1, 100, typeof(int))",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_sequence_operation_with_some_defaults()
        {
            var operation = new CreateSequenceOperation("dbo.MySequence", Sequence.DefaultStartValue, 7);

            Assert.Equal(
                @"CreateSequence(""dbo.MySequence"", 1, 7)",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_sequence_operation_with_all_defaults()
        {
            var operation = new CreateSequenceOperation("dbo.MySequence");

            Assert.Equal(
                @"CreateSequence(""dbo.MySequence"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            var operation = new DropSequenceOperation("dbo.MySequence");

            Assert.Equal(
                @"DropSequence(""dbo.MySequence"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_move_sequence_operation()
        {
            var operation = new MoveSequenceOperation("dbo.MySequence", "RenamedSchema");

            Assert.Equal(
                @"MoveSequence(""dbo.MySequence"", ""RenamedSchema"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_rename_sequence_operation()
        {
            var operation = new RenameSequenceOperation("dbo.MySequence", "RenamedSequence");

            Assert.Equal(
                @"RenameSequence(""dbo.MySequence"", ""RenamedSequence"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_alter_sequence_operation()
        {
            var operation = new AlterSequenceOperation("dbo.MySequence", newIncrementBy: 13);

            Assert.Equal(
                @"AlterSequence(""dbo.MySequence"", 13)",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<byte>("P1").ForRelational().DefaultValue((byte)BikeType.Mountain);
                    b.Property<int>("P2").ForRelational().DefaultValue(5);
                    b.Property<int?>("P3");
                    b.Property<int>("P4").StoreComputed().ForRelational().DefaultExpression("P2 + P3");
                    b.ForRelational().Table("MyTable", "dbo");
                });

            var operation = OperationFactory().CreateTableOperation(model.GetEntityType("E"));

            Assert.Equal(
@"CreateTable(""dbo.MyTable"",
    c => new
        {
            P1 = c.Byte(nullable: false, defaultValue: 1),
            P2 = c.Int(nullable: false, defaultValue: 5),
            P3 = c.Int(),
            P4 = c.Int(nullable: false, defaultSql: ""P2 + P3"", computed: true)
        })",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_table_operation_with_one_primary_key_columns()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").ForRelational().DefaultValue(5);
                    b.Property<int?>("Bar");
                    b.Key("Foo").ForRelational().Name("MyPK");
                    b.ForRelational().Table("MyTable", "dbo");
                });

            var operation = OperationFactory().CreateTableOperation(model.GetEntityType("E"));

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int()
        })
    .PrimaryKey(""MyPK"", t => t.Foo)",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_table_operation_with_multiple_primary_key_columns()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").ForRelational().DefaultValue(5);
                    b.Property<int?>("Bar");
                    b.Key("Foo", "Bar").ForRelational().Name("MyPK");
                    b.ForRelational().Table("MyTable", "dbo");
                });

            var operation = OperationFactory().CreateTableOperation(model.GetEntityType("E"));

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int()
        })
    .PrimaryKey(""MyPK"", t => new { t.Foo, t.Bar })",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_table_with_unique_constraints()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);
            modelBuilder.Entity("E",
                b =>
                {
                    b.Property<int>("Foo").ForRelational().DefaultValue(5);
                    var bar = b.Property<int?>("Bar").Metadata;
                    var c1 = b.Property<string>("C1").Metadata;
                    var c2 = b.Property<string>("C2").Metadata;
                    b.Key("Foo").ForRelational().Name("MyPK");
                    b.Metadata.AddKey(c1).Relational().Name = "MyUC0";
                    b.Metadata.AddKey(new[] { bar, c2 }).Relational().Name = "MyUC1";
                    b.ForRelational().Table("MyTable", "dbo");
                });

            var operation = OperationFactory().CreateTableOperation(model.GetEntityType("E"));

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int(),
            C1 = c.String(),
            C2 = c.String()
        })
    .PrimaryKey(""MyPK"", t => t.Foo)
    .UniqueConstraint(""MyUC0"", t => t.C1)
    .UniqueConstraint(""MyUC1"", t => new { t.Bar, t.C2 })",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            var operation = new DropTableOperation("dbo.MyTable");

            Assert.Equal(
                @"DropTable(""dbo.MyTable"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {
            var operation = new RenameTableOperation("dbo.MyTable", "MyTable2");

            Assert.Equal(
                @"RenameTable(""dbo.MyTable"", ""MyTable2"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            var operation = new MoveTableOperation("dbo.MyTable", "dbo2");

            Assert.Equal(
                @"MoveTable(""dbo.MyTable"", ""dbo2"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var column = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 };
            var operation = new AddColumnOperation("dbo.MyTable", column);

            Assert.Equal(
                @"AddColumn(""dbo.MyTable"", ""Foo"", c => c.Int(nullable: false, defaultValue: 5))",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_column_operation_with_computed_column()
        {
            var column = new Column("C3", typeof(int)) { DefaultSql = "C1 + C2", IsComputed = true };
            var operation = new AddColumnOperation("dbo.MyTable", column);

            Assert.Equal(
                @"AddColumn(""dbo.MyTable"", ""C3"", c => c.Int(defaultSql: ""C1 + C2"", computed: true))",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_column_operation_with_enum_clr_type()
        {
            var column = new Column("Foo", typeof(BikeType)) { IsNullable = false, DefaultValue = BikeType.Mountain };
            var operation = new AddColumnOperation("dbo.MyTable", column);

            Assert.Equal(
                @"AddColumn(""dbo.MyTable"", ""Foo"", c => c.Byte(nullable: false, defaultValue: 1))",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            var operation = new DropColumnOperation("dbo.MyTable", "Foo");

            Assert.Equal(
                @"DropColumn(""dbo.MyTable"", ""Foo"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            var operation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");

            Assert.Equal(
                @"RenameColumn(""dbo.MyTable"", ""Foo"", ""Foo2"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_alter_column_operation()
        {
            var newColumn = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 };
            var operation = new AlterColumnOperation("dbo.MyTable", newColumn, isDestructiveChange: true);

            Assert.Equal(
                @"AlterColumn(""dbo.MyTable"", ""Foo"", c => c.Int(nullable: false, defaultValue: 5))",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_value()
        {
            var operation = new AddDefaultConstraintOperation("dbo.MyTable", "Foo", 5, null);

            Assert.Equal(
                @"AddDefaultValue(""dbo.MyTable"", ""Foo"", 5)",
                CSharpMigrationCodeGenerator.Generate(operation));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            var operation = new AddDefaultConstraintOperation("dbo.MyTable", "Foo", null, "GETDATE()");

            Assert.Equal(
                @"AddDefaultExpression(""dbo.MyTable"", ""Foo"", ""GETDATE()"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            var operation = new DropDefaultConstraintOperation("dbo.MyTable", "Foo");

            Assert.Equal(
                @"DropDefaultConstraint(""dbo.MyTable"", ""Foo"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            var operation = new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false);

            Assert.Equal(
                @"AddPrimaryKey(""dbo.MyTable"", ""MyPK"", new[] { ""Foo"", ""Bar"" }, isClustered: false)",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            var operation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");

            Assert.Equal(
                @"DropPrimaryKey(""dbo.MyTable"", ""MyPK"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_unique_constraint_operation()
        {
            var operation = new AddUniqueConstraintOperation("dbo.MyTable", "MyUC", new[] { "Foo", "Bar" });

            Assert.Equal(
                @"AddUniqueConstraint(""dbo.MyTable"", ""MyUC"", new[] { ""Foo"", ""Bar"" })",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_unique_constraint_operation()
        {
            var operation = new DropUniqueConstraintOperation("dbo.MyTable", "MyUC");

            Assert.Equal(
                @"DropUniqueConstraint(""dbo.MyTable"", ""MyUC"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            var operation = new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: false);

            Assert.Equal(
@"AddForeignKey(
    ""dbo.MyTable"",
    ""MyFK"",
    new[] { ""Foo"", ""Bar"" },
    ""dbo.MyTable2"",
    new[] { ""Foo2"", ""Bar2"" },
    cascadeDelete: false)",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            var operation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");

            Assert.Equal(
                @"DropForeignKey(""dbo.MyTable"", ""MyFK"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            var operation = new CreateIndexOperation(
                "dbo.MyTable", "MyIdx", new[] { "Foo", "Bar" }, isUnique: false, isClustered: false);

            Assert.Equal(
                @"CreateIndex(""dbo.MyTable"", ""MyIdx"", new[] { ""Foo"", ""Bar"" }, isUnique: false, isClustered: false)",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_drop_index_key_operation()
        {
            var operation = new DropIndexOperation("dbo.MyTable", "MyIdx");

            Assert.Equal(
                @"DropIndex(""dbo.MyTable"", ""MyIdx"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_rename_index_key_operation()
        {
            var operation = new RenameIndexOperation("dbo.MyTable", "MyIdx", "MyNewIdx");

            Assert.Equal(
                @"RenameIndex(""dbo.MyTable"", ""MyIdx"", ""MyNewIdx"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_copy_data_operation()
        {
            var operation = new CopyDataOperation("dbo.T1", new[] { "A", "B" }, "dbo.T2", new[] { "C", "D" });

            Assert.Equal(
                @"CopyData(""dbo.T1"", new[] { ""A"", ""B"" }, ""dbo.T2"", new[] { ""C"", ""D"" })",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_sql_operation()
        {
            var operation = new SqlOperation(
                @"UPDATE T
    SET C1='V""1'
    WHERE C2='V""2'");

            Assert.Equal(
                @"Sql(@""UPDATE T
    SET C1='V""""1'
    WHERE C2='V""""2'"")",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        // TODO: Add missing GenerateLiteral unit tests.

        [Fact]
        public void Generate_string_literal()
        {
            var csharpGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());

            Assert.Equal("\"foo\\\"bar\"", csharpGenerator.GenerateLiteral("foo\"bar"));
        }

        [Fact]
        public void Generate_verbatim_string_literal()
        {
            var csharpGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());

            Assert.Equal(
                @"@""foo""""bar
    bar""""foo""",
                csharpGenerator.GenerateVerbatimStringLiteral(
                    @"foo""bar
    bar""foo"));
        }

        [Fact]
        public void Escape_string()
        {
            var csharpGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());

            Assert.Equal("foo\\\"bar", csharpGenerator.EscapeString("foo\"bar"));
        }

        [Fact]
        public void Escape_verbatim_string()
        {
            var csharpGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());

            Assert.Equal(
                @"foo""""bar
    bar""""foo",
                csharpGenerator.EscapeVerbatimString(
                    @"foo""bar
    bar""foo"));
        }

        [Fact]
        public void Generate_migration_class()
        {
            var upgradeOperations
                = new[]
                    {
                        new AddColumnOperation("dbo.MyTable", new Column("Foo", typeof(int))),
                        new AddColumnOperation("dbo.MyTable", new Column("Bar", typeof(int)))
                    };

            var downgradeOperations
                = new[]
                    {
                        new DropColumnOperation("dbo.MyTable", "Foo"),
                        new DropColumnOperation("dbo.MyTable", "Bar")
                    };

            var migration
                = new MigrationInfo("000000000000001_Name")
                    {
                        UpgradeOperations = upgradeOperations,
                        DowngradeOperations = downgradeOperations
                    };

            var codeGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            var stringBuilder = new IndentedStringBuilder();

            codeGenerator.GenerateMigrationClass("MyNamespace", "MyClass", migration, stringBuilder);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using System;

namespace MyNamespace
{
    public partial class MyClass : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn(""dbo.MyTable"", ""Foo"", c => c.Int());
            
            migrationBuilder.AddColumn(""dbo.MyTable"", ""Bar"", c => c.Int());
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(""dbo.MyTable"", ""Foo"");
            
            migrationBuilder.DropColumn(""dbo.MyTable"", ""Bar"");
        }
    }
}",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_migration_metadata_class()
        {
            var model = new Model();
            var entityType = model.AddEntityType("Entity");

            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var migration
                = new MigrationInfo("000000000000001_Name", "1.2.3.4")
                    {
                        TargetModel = model
                    };

            var codeGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            var stringBuilder = new IndentedStringBuilder();

            codeGenerator.GenerateMigrationMetadataClass("MyNamespace", "MyClass", migration, typeof(MyContext), stringBuilder);

            Assert.Equal(
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpMigrationCodeGeneratorTest.MyContext))]
    public partial class MyClass : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_Name"";
            }
        }
        
        string IMigrationMetadata.ProductVersion
        {
            get
            {
                return ""1.2.3.4"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                stringBuilder.ToString());
        }

        [Fact]
        public void Language_returns_cs()
        {
            var generator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());

            Assert.Equal(".cs", generator.Language);
        }

        private enum BikeType : byte
        {
            Road,
            Mountain
        }

        public class MyContext : DbContext
        {
        }

        #region Helper methods

        private void GenerateAndValidateCode(MigrationOperation operation)
        {
            GenerateAndValidateCode(
                new MigrationInfo("000000000000000_Migration")
                    {
                        UpgradeOperations = new[] { operation },
                        DowngradeOperations = new[] { operation },
                        TargetModel = new Model()
                    });
        }

        private void GenerateAndValidateCode(MigrationInfo migration)
        {
            var @namespace = GetType().Namespace + ".DynamicallyCompiled";
            var className = "Migration" + Guid.NewGuid().ToString("N");

            var generator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            var migrationBuilder = new IndentedStringBuilder();
            var migrationMetadataBuilder = new IndentedStringBuilder();

            generator.GenerateMigrationClass(@namespace, className, migration, migrationBuilder);
            generator.GenerateMigrationMetadataClass(@namespace, className, migration, typeof(DbContext), migrationMetadataBuilder);

            var migrationSource = migrationBuilder.ToString();
            var migrationMetadataSource = migrationMetadataBuilder.ToString();

            Assembly.Load("System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

            var compiledAssembly = CodeGeneratorTestHelper.Compile(
                @namespace + ".dll",
                new[] { migrationSource, migrationMetadataSource },
                new[]
                    {
                        "mscorlib",
                        "System.Core",
                        "System.Linq.Expressions",
                        "System.Runtime",
                        "EntityFramework.Core",
                        "EntityFramework.Relational"
                    });

            var compiledMigration = new MigrationInfo(
                (Migration)compiledAssembly.CreateInstance(@namespace + "." + className));

            generator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            migrationBuilder = new IndentedStringBuilder();
            migrationMetadataBuilder = new IndentedStringBuilder();

            generator.GenerateMigrationClass(@namespace, className, compiledMigration, migrationBuilder);
            generator.GenerateMigrationMetadataClass(@namespace, className, compiledMigration, typeof(DbContext), migrationMetadataBuilder);

            Assert.Equal(migrationSource, migrationBuilder.ToString());
            Assert.Equal(migrationMetadataSource, migrationMetadataBuilder.ToString());
        }

        private static MigrationOperationFactory OperationFactory()
        {
            return new MigrationOperationFactory(RelationalTestHelpers.ExtensionProvider());
        }
        #endregion
    }
}

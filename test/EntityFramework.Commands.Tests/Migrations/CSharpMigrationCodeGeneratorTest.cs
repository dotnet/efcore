// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
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
            var operation = new CreateSequenceOperation(new Sequence("dbo.MySequence", "BIGINT", 10, 5));

            Assert.Equal(
                @"CreateSequence(""dbo.MySequence"", ""BIGINT"", 10, 5)",
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
        public void Generate_when_create_table_operation_without_primary_key()
        {
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 },
                        new Column("Bar", typeof(int))
                    });
            var operation = new CreateTableOperation(table);

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int()
        })",
                CSharpMigrationCodeGenerator.Generate(operation));

            GenerateAndValidateCode(operation);
        }

        [Fact]
        public void Generate_when_create_table_operation_with_one_primary_key_columns()
        {
            Column foo;
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 },
                        new Column("Bar", typeof(int))
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo })
                };
            var operation = new CreateTableOperation(table);

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
            Column foo, bar;
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 },
                        bar = new Column("Bar", typeof(int))
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo, bar })
                };
            var operation = new CreateTableOperation(table);

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
                @"AddDefaultConstraint(""dbo.MyTable"", ""Foo"", DefaultConstraint.Value(5))",
                CSharpMigrationCodeGenerator.Generate(operation));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            var operation = new AddDefaultConstraintOperation("dbo.MyTable", "Foo", null, "GETDATE()");

            Assert.Equal(
                @"AddDefaultConstraint(""dbo.MyTable"", ""Foo"", DefaultConstraint.Sql(""GETDATE()""))",
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
        public void Generate_when_add_foreign_key_operation()
        {
            var operation = new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: false);

            Assert.Equal(
                @"AddForeignKey(""dbo.MyTable"", ""MyFK"", new[] { ""Foo"", ""Bar"" }, ""dbo.MyTable2"", new[] { ""Foo2"", ""Bar2"" }, cascadeDelete: false)",
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
                = new MigrationMetadata("000000000000001_Name")
                    {
                        UpgradeOperations = upgradeOperations,
                        DowngradeOperations = downgradeOperations
                    };

            var codeGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            var stringBuilder = new IndentedStringBuilder();

            codeGenerator.GenerateMigrationClass("MyNamespace", "MyClass", migration, stringBuilder);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Model;
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
            var entityType = new EntityType("Entity");

            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            model.AddEntityType(entityType);

            var migration
                = new MigrationMetadata("000000000000001_Name")
                    {
                        TargetModel = model
                    };

            var codeGenerator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            var stringBuilder = new IndentedStringBuilder();

            codeGenerator.GenerateMigrationMetadataClass("MyNamespace", "MyClass", migration, typeof(MyContext), stringBuilder);

            Assert.Equal(
@"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(CSharpMigrationCodeGeneratorTest.MyContext))]
    public partial class MyClass : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_Name"";
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

        public class MyContext : DbContext
        {
        }

        #region Helper methods

        private void GenerateAndValidateCode(MigrationOperation operation)
        {
            GenerateAndValidateCode(
                new MigrationMetadata("000000000000000_Migration")
                    {
                        UpgradeOperations = new[] { operation },
                        DowngradeOperations = new[] { operation },
                        TargetModel = new Model()
                    });
        }

        private void GenerateAndValidateCode(IMigrationMetadata migration)
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
                        "EntityFramework",
                        "EntityFramework.Relational",
                        "EntityFramework.Migrations"
                    });

            var compiledMigration = (IMigrationMetadata)
                compiledAssembly.CreateInstance(@namespace + "." + className);

            Assert.NotNull(compiledMigration);

            generator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            migrationBuilder = new IndentedStringBuilder();
            migrationMetadataBuilder = new IndentedStringBuilder();

            generator.GenerateMigrationClass(@namespace, className, compiledMigration, migrationBuilder);
            generator.GenerateMigrationMetadataClass(@namespace, className, compiledMigration, typeof(DbContext), migrationMetadataBuilder);

            Assert.Equal(migrationSource, migrationBuilder.ToString());
            Assert.Equal(migrationMetadataSource, migrationMetadataBuilder.ToString());
        }

        #endregion
    }
}

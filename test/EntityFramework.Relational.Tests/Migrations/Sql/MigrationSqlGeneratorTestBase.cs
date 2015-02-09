using System;
using System.Linq;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Migrations.Sql
{
    public abstract class MigrationSqlGeneratorTestBase
    {
        protected static string EOL => Environment.NewLine;

        protected abstract MigrationSqlGenerator SqlGenerator { get; }

        protected virtual string Sql { get; set; }

        [Fact]
        public virtual void AddColumnOperation_with_defaultValue()
        {
            Generate(
                new AddColumnOperation(
                    "People",
                    "dbo",
                    new ColumnModel(
                        "Name",
                        "varchar(30)",
                        /*nullable:*/ false,
                        "John Doe",
                        defaultValueSql: null)));
        }

        [Fact]
        public virtual void AddColumnOperation_with_defaultValueSql()
        {
            Generate(
                new AddColumnOperation(
                    "People",
                        /*schema:*/ null,
                        new ColumnModel(
                            "Birthday",
                            "date",
                            /*nullable:*/ true,
                        /*defaultValueSql:*/ null,
                        "CURRENT_TIMESTAMP")));
        }

        [Fact]
        public virtual void AddForeignKeyOperation_with_name()
        {
            Generate(
                new AddForeignKeyOperation(
                    "People",
                    "dbo",
                    "FK_People_Companies",
                    new[] { "EmployerId1", "EmployerId2" },
                    "Companies",
                    "hr",
                    new[] { "Id1", "Id2" },
                    cascadeDelete: true));
        }

        [Fact]
        public virtual void AddForeignKeyOperation_without_name()
        {
            Generate(
                new AddForeignKeyOperation(
                    "People",
                    /*dependentSchema:*/ null,
                    /*name:*/ null,
                    new[] { "SpouseId" },
                    "People",
                    /*principalSchema:*/ null,
                    new[] { "Id" },
                    cascadeDelete: false));
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_with_name()
        {
            Generate(
                new AddPrimaryKeyOperation(
                    "People",
                    "dbo",
                    "PK_People",
                    new[] { "Id1", "Id2" }));
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_without_name()
        {
            Generate(
                new AddPrimaryKeyOperation(
                    "People",
                    /*schema:*/ null,
                    /*name:*/ null,
                    new[] { "Id" }));
        }

        [Fact]
        public virtual void AddUniqueConstraintOperation_with_name()
        {
            Generate(
                new AddUniqueConstraintOperation(
                    "People",
                    "dbo",
                    "AK_People_DriverLicense",
                    new[] { "DriverLicense_State", "DriverLicense_Number" }));
        }

        [Fact]
        public virtual void AddUniqueConstraintOperation_without_name()
        {
            Generate(
                new AddUniqueConstraintOperation(
                    "People",
                    /*schema:*/ null,
                    /*name:*/ null,
                    new[] { "SSN" }));
        }

        [Fact]
        public virtual void AlterColumnOperation()
        {
            Generate(
                new AlterColumnOperation(
                    "People",
                    "dbo",
                    new ColumnModel(
                        "LuckyNumber",
                        "int",
                        nullable: false,
                        defaultValue: 7,
                        defaultValueSql: null)));
        }

        [Fact]
        public virtual void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            Generate(
                new AlterSequenceOperation(
                    "DefaultSequence",
                    "dbo",
                    incrementBy: 1,
                    minValue: 2,
                    maxValue: 816));
        }

        [Fact]
        public virtual void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            Generate(
                new AlterSequenceOperation(
                    "DefaultSequence",
                    schema: null,
                    incrementBy: 1,
                    minValue: null,
                    maxValue: null));
        }

        [Fact]
        public virtual void AlterTableOperation()
        {
            Generate(
                new AlterTableOperation(
                    "People",
                    "dbo"));
        }

        [Fact]
        public virtual void CreateIndexOperation_unique()
        {
            Generate(
                new CreateIndexOperation(
                    "IX_People_Name",
                    "People",
                    "dbo",
                    new[] { "FirstName", "LastName" },
                    unique: true));
        }

        [Fact]
        public virtual void CreateIndexOperation_nonunique()
        {
            Generate(
                new CreateIndexOperation(
                    "IX_People_Name",
                    "People",
                    /*schema:*/ null,
                    new[] { "Name" },
                    unique: false));
        }

        [Fact]
        public virtual void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            Generate(
                new CreateSequenceOperation(
                    "DefaultSequence",
                    "dbo",
                    /*startValue:*/ 3,
                    /*incrementBy:*/ 1,
                    /*minValue:*/ 2,
                    /*maxValue:*/ 816,
                    "bigint"));
        }

        [Fact]
        public virtual void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            Generate(
                new CreateSequenceOperation(
                    "DefaultSequence",
                    /*schema:*/ null,
                    /*startValue:*/ 3,
                    /*incrementBy:*/ 1,
                    /*minValue:*/ null,
                    /*maxValue:*/ null,
                    "bigint"));
        }

        [Fact]
        public virtual void CreateTableOperation()
        {
            var operation = new CreateTableOperation("People", "dbo");
            operation.Columns.Add(
                new ColumnModel(
                    "Id",
                    "int",
                    nullable: false,
                    defaultValue: null,
                    defaultValueSql: null));
            operation.Columns.Add(
                new ColumnModel(
                    "EmployerId",
                    "int",
                    nullable: true,
                    defaultValue: null,
                    defaultValueSql: null));
            operation.Columns.Add(
                new ColumnModel(
                    "SSN",
                    "char(11)",
                    nullable: true,
                    defaultValue: null,
                    defaultValueSql: null));
            operation.PrimaryKey = new AddPrimaryKeyOperation(
                "People",
                "dbo",
                /*name:*/ null,
                new[] { "Id" });
            operation.UniqueConstraints.Add(
                new AddUniqueConstraintOperation(
                    "People",
                    "dbo",
                    /*name:*/ null,
                    new[] { "SSN" }));
            operation.ForeignKeys.Add(
                new AddForeignKeyOperation(
                    "People",
                    "dbo",
                    /*name:*/ null,
                    new[] { "EmployerId" },
                    "Companies",
                    principalSchema: null,
                    principalColumns: null,
                    cascadeDelete: false));

            Generate(operation);
        }

        [Fact]
        public virtual void DropColumnOperation()
        {
            Generate(
                new DropColumnOperation(
                    "People",
                    "dbo",
                    "LuckyNumber"));
        }

        [Fact]
        public virtual void DropForeignKeyOperation()
        {
            Generate(
                new DropForeignKeyOperation(
                    "People",
                    "dbo",
                    "FK_People_Companies"));
        }

        [Fact]
        public virtual void DropIndexOperation()
        {
            Generate(
                new DropIndexOperation(
                    "IX_People_Name",
                    "People",
                    "dbo"));
        }

        [Fact]
        public virtual void DropPrimaryKeyOperation()
        {
            Generate(
                new DropPrimaryKeyOperation(
                    "People",
                    "dbo",
                    "PK_People"));
        }

        [Fact]
        public virtual void DropSequenceOperation()
        {
            Generate(
                new DropSequenceOperation(
                    "DefaultSequence",
                    "dbo"));
        }

        [Fact]
        public virtual void DropTableOperation()
        {
            Generate(
                new DropTableOperation(
                    "People",
                    "dbo"));
        }

        [Fact]
        public virtual void DropUniqueConstraintOperation()
        {
            Generate(
                new DropUniqueConstraintOperation(
                    "People",
                    "dbo",
                    "AK_People_SSN"));
        }

        [Fact]
        public virtual void SqlOperation()
        {
            Generate(
                new SqlOperation(
                    "-- I <3 DDL",
                    suppressTransaction: false));
        }

        protected virtual void Generate(MigrationOperation operation)
        {
            var batch = SqlGenerator.Generate(new[] { operation });

            Sql = string.Join(
                EOL + "GO" + EOL + EOL,
                batch.Select(b => b.Sql));
        }
    }
}

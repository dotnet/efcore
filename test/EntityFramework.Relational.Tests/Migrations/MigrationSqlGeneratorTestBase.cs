// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class MigrationSqlGeneratorTestBase
    {
        protected static string EOL => Environment.NewLine;

        protected abstract IMigrationsSqlGenerator SqlGenerator { get; }

        protected virtual string Sql { get; set; }

        [Fact]
        public virtual void AddColumnOperation_with_defaultValue()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "Name",
                    ClrType = typeof(string),
                    ColumnType = "varchar(30)",
                    IsNullable = false,
                    DefaultValue = "John Doe"
                });
        }

        [Fact]
        public virtual void AddColumnOperation_with_defaultValueSql()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "date",
                    IsNullable = true,
                    DefaultValueSql = "CURRENT_TIMESTAMP"
                });
        }

        [Fact]
        public virtual void AddColumnOperation_with_computed_column_SQL()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "date",
                    IsNullable = true,
                    ComputedColumnSql = "CURRENT_TIMESTAMP"
                });
        }

        [Fact]
        public virtual void AddColumnOperation_without_column_type()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Alias",
                    ClrType = typeof(string)
                });
        }

        [Fact]
        public virtual void AddForeignKeyOperation_with_name()
        {
            Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "FK_People_Companies",
                    Columns = new[] { "EmployerId1", "EmployerId2" },
                    PrincipalTable = "Companies",
                    PrincipalSchema = "hr",
                    PrincipalColumns = new[] { "Id1", "Id2" },
                    OnDelete = ReferentialAction.Cascade
                });
        }

        [Fact]
        public virtual void AddForeignKeyOperation_without_name()
        {
            Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "SpouseId" },
                    PrincipalTable = "People",
                    PrincipalColumns = new[] { "Id" }
                });
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_with_name()
        {
            Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "PK_People",
                    Columns = new[] { "Id1", "Id2" }
                });
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_without_name()
        {
            Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "Id" }
                });
        }

        [Fact]
        public virtual void AddUniqueConstraintOperation_with_name()
        {
            Generate(
                new AddUniqueConstraintOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "AK_People_DriverLicense",
                    Columns = new[] { "DriverLicense_State", "DriverLicense_Number" }
                });
        }

        [Fact]
        public virtual void AddUniqueConstraintOperation_without_name()
        {
            Generate(
                new AddUniqueConstraintOperation
                {
                    Table = "People",
                    Columns = new[] { "SSN" }
                });
        }

        [Fact]
        public virtual void AlterColumnOperation()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "LuckyNumber",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    DefaultValue = 7
                });
        }

        [Fact]
        public virtual void AlterColumnOperation_without_column_type()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "LuckyNumber",
                    ClrType = typeof(int)
                });
        }

        [Fact]
        public virtual void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            Generate(
                new AlterSequenceOperation
                {
                    Name = "DefaultSequence",
                    Schema = "dbo",
                    IncrementBy = 1,
                    MinValue = 2,
                    MaxValue = 816,
                    IsCyclic = true
                });
        }

        [Fact]
        public virtual void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            Generate(
                new AlterSequenceOperation
                {
                    Name = "DefaultSequence",
                    IncrementBy = 1
                });
        }

        [Fact]
        public virtual void RenameTableOperation_within_schema()
        {
            Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Personas",
                    NewSchema = "dbo"
                });
        }

        [Fact]
        public virtual void CreateIndexOperation_unique()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "dbo",
                    Columns = new[] { "FirstName", "LastName" },
                    IsUnique = true
                });
        }

        [Fact]
        public virtual void CreateIndexOperation_nonunique()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = false
                });
        }

        [Fact]
        public virtual void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            Generate(
                new CreateSequenceOperation
                {
                    Name = "DefaultSequence",
                    Schema = "dbo",
                    StartValue = 3,
                    IncrementBy = 1,
                    MinValue = 2,
                    MaxValue = 816,
                    ClrType = typeof(long),
                    IsCyclic = true
                });
        }

        [Fact]
        public virtual void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
        {
            Generate(
                new CreateSequenceOperation
                {
                    Name = "DefaultSequence",
                    Schema = "dbo",
                    StartValue = 3,
                    IncrementBy = 1,
                    MinValue = 2,
                    MaxValue = 816,
                    ClrType = typeof(int),
                    IsCyclic = true
                });
        }

        [Fact]
        public virtual void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            Generate(
                new CreateSequenceOperation
                {
                    Name = "DefaultSequence",
                    ClrType = typeof(long),
                    StartValue = 3,
                    IncrementBy = 1
                });
        }

        [Fact]
        public virtual void CreateTableOperation()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            ClrType = typeof(int),
                            IsNullable = false
                        },
                        new AddColumnOperation
                        {
                            Name = "EmployerId",
                            ClrType = typeof(int),
                            IsNullable = true
                        },
                         new AddColumnOperation
                        {
                            Name = "SSN",
                            ClrType = typeof(string),
                            ColumnType = "char(11)",
                            IsNullable = true
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    },
                    UniqueConstraints =
                    {
                        new AddUniqueConstraintOperation
                        {
                            Columns = new[] { "SSN" }
                        }
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation
                        {
                            Columns = new[] { "EmployerId" },
                            PrincipalTable = "Companies",
                            PrincipalColumns = new[] { "Id" }
                        }
                    }
                });
        }

        [Fact]
        public virtual void DropColumnOperation()
        {
            Generate(
                new DropColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "LuckyNumber"
                });
        }

        [Fact]
        public virtual void DropForeignKeyOperation()
        {
            Generate(
                new DropForeignKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "FK_People_Companies"
                });
        }

        [Fact]
        public virtual void DropIndexOperation()
        {
            Generate(
                new DropIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "dbo"
                });
        }

        [Fact]
        public virtual void DropPrimaryKeyOperation()
        {
            Generate(
                new DropPrimaryKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "PK_People"
                });
        }

        [Fact]
        public virtual void DropSequenceOperation()
        {
            Generate(
                new DropSequenceOperation
                {
                    Name = "DefaultSequence",
                    Schema = "dbo"
                });
        }

        [Fact]
        public virtual void DropTableOperation()
        {
            Generate(
                new DropTableOperation
                {
                    Name = "People",
                    Schema = "dbo"
                });
        }

        [Fact]
        public virtual void DropUniqueConstraintOperation()
        {
            Generate(
                new DropUniqueConstraintOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "AK_People_SSN"
                });
        }

        [Fact]
        public virtual void SqlOperation()
        {
            Generate(
                new SqlOperation
                {
                    Sql = "-- I <3 DDL"
                });
        }

        protected virtual void Generate(params MigrationOperation[] operation)
        {
            var batch = SqlGenerator.Generate(operation);

            Sql = string.Join(
                "GO" + EOL + EOL,
                batch.Select(b => b.CommandText));
        }
    }
}

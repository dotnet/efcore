// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class MigrationSqlGeneratorTestBase
    {
        protected static string EOL => Environment.NewLine;

        protected virtual string Sql { get; set; }

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_filter_where_clause()
            => Generate(
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name").IsRequired(),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    Filter = "[Name] IS NOT NULL"
                });

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_filter_where_clause_and_is_unique()
            => Generate(
                modelBuilder => modelBuilder.Entity("People").Property<string>("Name"),
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = true,
                    Filter = "[Name] IS NOT NULL AND <> ''"
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_defaultValue()
            => Generate(
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

        [ConditionalFact]
        public virtual void AddColumnOperation_with_defaultValueSql()
            => Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "date",
                    IsNullable = true,
                    DefaultValueSql = "CURRENT_TIMESTAMP"
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_without_column_type()
            => Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Alias",
                    ClrType = typeof(string)
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_ansi()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").IsUnicode(false),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_unicode_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").IsUnicode(false),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = true,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_unicode_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_fixed_length()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").IsFixedLength(),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = true,
                    IsNullable = true,
                    IsFixedLength = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_fixed_length_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true,
                    IsFixedLength = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 32,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_on_derived()
            => Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity("Person");
                    modelBuilder.Entity(
                        "SpecialPerson", b =>
                        {
                            b.HasBaseType("Person");
                            b.Property<string>("Name").HasMaxLength(30);
                        });

                    modelBuilder.Entity("MoreSpecialPerson").HasBaseType("SpecialPerson");
                },
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_shared_column()
            => Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity<Base>();
                    modelBuilder.Entity<Derived1>();
                    modelBuilder.Entity<Derived2>();
                },
                new AddColumnOperation
                {
                    Table = "Base",
                    Name = "Foo",
                    ClrType = typeof(string),
                    IsNullable = true
                });

        private class Base
        {
            // ReSharper disable once UnusedMember.Local
            public int Id { get; set; }
        }

        private class Derived1 : Base
        {
            // ReSharper disable once UnusedMember.Local
            public string Foo { get; set; }
        }

        private class Derived2 : Base
        {
            // ReSharper disable once UnusedMember.Local
            public string Foo { get; set; }
        }

        [ConditionalFact]
        public virtual void AddForeignKeyOperation_with_name()
            => Generate(
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

        [ConditionalFact]
        public virtual void AddForeignKeyOperation_without_name()
            => Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "SpouseId" },
                    PrincipalTable = "People",
                    PrincipalColumns = new[] { "Id" }
                });

        [ConditionalFact]
        public virtual void AddForeignKeyOperation_without_principal_columns()
            => Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "SpouseId" },
                    PrincipalTable = "People"
                });

        [ConditionalFact]
        public virtual void AddPrimaryKeyOperation_with_name()
            => Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "PK_People",
                    Columns = new[] { "Id1", "Id2" }
                });

        [ConditionalFact]
        public virtual void AddPrimaryKeyOperation_without_name()
            => Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "Id" }
                });

        [ConditionalFact]
        public virtual void AddUniqueConstraintOperation_with_name()
            => Generate(
                new AddUniqueConstraintOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "AK_People_DriverLicense",
                    Columns = new[] { "DriverLicense_State", "DriverLicense_Number" }
                });

        [ConditionalFact]
        public virtual void AddUniqueConstraintOperation_without_name()
            => Generate(
                new AddUniqueConstraintOperation
                {
                    Table = "People",
                    Columns = new[] { "SSN" }
                });

        [ConditionalFact]
        public virtual void CreateCheckConstraintOperation_with_name()
            => Generate(
                new CreateCheckConstraintOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "CK_People_DriverLicense",
                    Sql = "DriverLicense_Number > 0"
                });

        [ConditionalFact]
        public virtual void AlterColumnOperation()
            => Generate(
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

        [ConditionalFact]
        public virtual void AlterColumnOperation_without_column_type()
            => Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "LuckyNumber",
                    ClrType = typeof(int)
                });

        [ConditionalFact]
        public virtual void AlterSequenceOperation_with_minValue_and_maxValue()
            => Generate(
                new AlterSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    IncrementBy = 1,
                    MinValue = 2,
                    MaxValue = 816,
                    IsCyclic = true
                });

        [ConditionalFact]
        public virtual void AlterSequenceOperation_without_minValue_and_maxValue()
            => Generate(
                new AlterSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    IncrementBy = 1
                });

        [ConditionalFact]
        public virtual void RenameTableOperation_legacy()
            => Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person"
                });

        [ConditionalFact]
        public virtual void RenameTableOperation()
            => Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person",
                    NewSchema = "dbo"
                });

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique()
            => Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "dbo",
                    Columns = new[] { "FirstName", "LastName" },
                    IsUnique = true
                });

        [ConditionalFact]
        public virtual void CreateIndexOperation_nonunique()
            => Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = false
                });

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_where_clauses()
            => Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = false,
                    Filter = "[Id] > 2"
                });

        [ConditionalFact]
        public virtual void CreateSequenceOperation_with_minValue_and_maxValue()
            => Generate(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    StartValue = 3,
                    IncrementBy = 1,
                    MinValue = 2,
                    MaxValue = 816,
                    ClrType = typeof(long),
                    IsCyclic = true
                });

        [ConditionalFact]
        public virtual void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
            => Generate(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    StartValue = 3,
                    IncrementBy = 1,
                    MinValue = 2,
                    MaxValue = 816,
                    ClrType = typeof(int),
                    IsCyclic = true
                });

        [ConditionalFact]
        public virtual void CreateSequenceOperation_without_minValue_and_maxValue()
            => Generate(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    ClrType = typeof(long),
                    StartValue = 3,
                    IncrementBy = 1
                });

        [ConditionalFact]
        public virtual void CreateTableOperation()
            => Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "People",
                            ClrType = typeof(int),
                            IsNullable = false
                        },
                        new AddColumnOperation
                        {
                            Name = "EmployerId",
                            Table = "People",
                            ClrType = typeof(int),
                            IsNullable = true,
                            Comment = "Employer ID comment"
                        },
                        new AddColumnOperation
                        {
                            Name = "SSN",
                            Table = "People",
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
                    CheckConstraints =
                    {
                        new CreateCheckConstraintOperation
                        {
                            Sql = "SSN > 0"
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
                    },
                    Comment = "Table comment"
                });

        [ConditionalFact]
        public virtual void CreateTableOperation_no_key()
            => Generate(
                new CreateTableOperation
                {
                    Name = "Anonymous",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Value",
                            Table = "Anonymous",
                            ClrType = typeof(int),
                            IsNullable = false
                        }
                    }
                });

        [ConditionalFact]
        public virtual void DropColumnOperation()
            => Generate(
                new DropColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "LuckyNumber"
                });

        [ConditionalFact]
        public virtual void DropForeignKeyOperation()
            => Generate(
                new DropForeignKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "FK_People_Companies"
                });

        [ConditionalFact]
        public virtual void DropIndexOperation()
            => Generate(
                new DropIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "dbo"
                });

        [ConditionalFact]
        public virtual void DropPrimaryKeyOperation()
            => Generate(
                new DropPrimaryKeyOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "PK_People"
                });

        [ConditionalFact]
        public virtual void DropSequenceOperation()
            => Generate(
                new DropSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo"
                });

        [ConditionalFact]
        public virtual void DropTableOperation()
            => Generate(
                new DropTableOperation
                {
                    Name = "People",
                    Schema = "dbo"
                });

        [ConditionalFact]
        public virtual void DropUniqueConstraintOperation()
            => Generate(
                new DropUniqueConstraintOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "AK_People_SSN"
                });

        [ConditionalFact]
        public virtual void DropCheckConstraintOperation()
            => Generate(
                new DropCheckConstraintOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "CK_People_SSN"
                });

        [ConditionalFact]
        public virtual void SqlOperation()
            => Generate(
                new SqlOperation
                {
                    Sql = "-- I <3 DDL"
                });

        [ConditionalFact]
        public virtual void InsertDataOperation()
            => Generate(
                new InsertDataOperation
                {
                    Table = "People",
                    Columns = new[] { "Id", "Full Name" },
                    Values = new object[,]
                    {
                        { 0, null },
                        { 1, "Daenerys Targaryen" },
                        { 2, "John Snow" },
                        { 3, "Arya Stark" },
                        { 4, "Harry Strickland" }
                    }
                });

        [ConditionalFact]
        public virtual void DeleteDataOperation_simple_key()
            => Generate(
                new DeleteDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "Id" },
                    KeyValues = new object[,]
                    {
                        { 2 },
                        { 4 }
                    }
                });

        [ConditionalFact]
        public virtual void DeleteDataOperation_composite_key()
            => Generate(
                new DeleteDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor", null },
                        { "Daenerys", "Targaryen" }
                    }
                });

        [ConditionalFact]
        public virtual void UpdateDataOperation_simple_key()
            => Generate(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "Id" },
                    KeyValues = new object[,]
                    {
                        { 1 },
                        { 4 }
                    },
                    Columns = new[] { "Full Name" },
                    Values = new object[,]
                    {
                        { "Daenerys Stormborn" },
                        { "Homeless Harry Strickland" }
                    }
                });

        [ConditionalFact]
        public virtual void UpdateDataOperation_composite_key()
            => Generate(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "Id", "Last Name" },
                    KeyValues = new object[,]
                    {
                        { 0, null },
                        { 4, "Strickland" }
                    },
                    Columns = new[] { "First Name" },
                    Values = new object[,]
                    {
                        { "Hodor" },
                        { "Harry" }
                    }
                });

        [ConditionalFact]
        public virtual void UpdateDataOperation_multiple_columns()
            => Generate(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "Id" },
                    KeyValues = new object[,]
                    {
                        { 1 },
                        { 4 }
                    },
                    Columns = new[] { "First Name", "Nickname" },
                    Values = new object[,]
                    {
                        { "Daenerys", "Dany" },
                        { "Harry", "Homeless" }
                    }
                });

        protected TestHelpers TestHelpers { get; }

        protected MigrationSqlGeneratorTestBase(TestHelpers testHelpers)
        {
            TestHelpers = testHelpers;
        }

        protected virtual void Generate(params MigrationOperation[] operation)
            => Generate(_ => { }, operation);

        protected virtual void Generate(Action<ModelBuilder> buildAction, params MigrationOperation[] operation)
        {
            var modelBuilder = TestHelpers.CreateConventionBuilder();
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
            buildAction(modelBuilder);

            var batch = TestHelpers.CreateContextServices().GetRequiredService<IMigrationsSqlGenerator>()
                .Generate(operation, modelBuilder.Model);

            Sql = string.Join(
                "GO" + EOL + EOL,
                batch.Select(b => b.CommandText));
        }

        protected void AssertSql(string expected)
            => Assert.Equal(expected, Sql, ignoreLineEndingDifferences: true);
    }
}

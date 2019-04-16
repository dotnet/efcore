// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class CSharpMigrationOperationGeneratorTest
    {
        private static readonly string _eol = Environment.NewLine;

        [Fact]
        public void Generate_separates_operations_by_a_blank_line()
        {
            var generator = new CSharpMigrationOperationGenerator(
                new CSharpMigrationOperationGeneratorDependencies(
                    new CSharpHelper(new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))));

            var builder = new IndentedStringBuilder();

            generator.Generate(
                "mb",
                new[]
                {
                    new SqlOperation
                    {
                        Sql = "-- Don't stand so"
                    },
                    new SqlOperation
                    {
                        Sql = "-- close to me"
                    }
                },
                builder);

            Assert.Equal(
                "mb.Sql(\"-- Don't stand so\");" + _eol +
                _eol +
                "mb.Sql(\"-- close to me\");",
                builder.ToString());
        }

        [Fact]
        public void AddColumnOperation_required_args()
        {
            Test(
                new AddColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int)
                },
                "mb.AddColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    nullable: false);",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(typeof(int), o.ClrType);
                });
        }

        [Fact]
        public void AddColumnOperation_all_args()
        {
            Test(
                new AddColumnOperation
                {
                    Name = "Id",
                    Schema = "dbo",
                    Table = "Post",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsUnicode = false,
                    MaxLength = 30,
                    IsRowVersion = true,
                    IsNullable = true,
                    DefaultValue = 1,
                    IsFixedLength = true
                },
                "mb.AddColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    type: \"int\"," + _eol +
                "    unicode: false," + _eol +
                "    fixedLength: true," + _eol +
                "    maxLength: 30," + _eol +
                "    rowVersion: true," + _eol +
                "    nullable: true," + _eol +
                "    defaultValue: 1);",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Equal("int", o.ColumnType);
                    Assert.True(o.IsNullable);
                    Assert.Equal(1, o.DefaultValue);
                    Assert.False(o.IsUnicode);
                    Assert.True(o.IsFixedLength);
                });
        }

        [Fact]
        public void AddColumnOperation_DefaultValueSql()
        {
            Test(
                new AddColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int),
                    DefaultValueSql = "1"
                },
                "mb.AddColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    nullable: false," + _eol +
                "    defaultValueSql: \"1\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Equal("1", o.DefaultValueSql);
                });
        }

        [Fact]
        public void AddColumnOperation_ComutedExpression()
        {
            Test(
                new AddColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int),
                    ComputedColumnSql = "1"
                },
                "mb.AddColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    nullable: false," + _eol +
                "    computedColumnSql: \"1\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Equal("1", o.ComputedColumnSql);
                });
        }

        [Fact]
        public void AddForeignKeyOperation_required_args()
        {
            Test(
                new AddForeignKeyOperation
                {
                    Table = "Post",
                    Name = "FK_Post_Blog_BlogId",
                    Columns = new[] { "BlogId" },
                    PrincipalTable = "Blog",
                    PrincipalColumns = new[] { "Id" }
                },
                "mb.AddForeignKey(" + _eol +
                "    name: \"FK_Post_Blog_BlogId\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"BlogId\"," + _eol +
                "    principalTable: \"Blog\"," + _eol +
                "    principalColumn: \"Id\");",
                o =>
                {
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("FK_Post_Blog_BlogId", o.Name);
                    Assert.Equal(new[] { "BlogId" }, o.Columns);
                    Assert.Equal("Blog", o.PrincipalTable);
                });
        }

        [Fact]
        public void AddForeignKeyOperation_all_args()
        {
            Test(
                new AddForeignKeyOperation
                {
                    Schema = "dbo",
                    Table = "Post",
                    Name = "FK_Post_Blog_BlogId",
                    Columns = new[] { "BlogId" },
                    PrincipalSchema = "my",
                    PrincipalTable = "Blog",
                    PrincipalColumns = new[] { "Id" },
                    OnUpdate = ReferentialAction.Restrict,
                    OnDelete = ReferentialAction.Cascade
                },
                "mb.AddForeignKey(" + _eol +
                "    name: \"FK_Post_Blog_BlogId\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"BlogId\"," + _eol +
                "    principalSchema: \"my\"," + _eol +
                "    principalTable: \"Blog\"," + _eol +
                "    principalColumn: \"Id\"," + _eol +
                "    onUpdate: ReferentialAction.Restrict," + _eol +
                "    onDelete: ReferentialAction.Cascade);",
                o =>
                {
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("FK_Post_Blog_BlogId", o.Name);
                    Assert.Equal(new[] { "BlogId" }, o.Columns);
                    Assert.Equal("Blog", o.PrincipalTable);
                    Assert.Equal("my", o.PrincipalSchema);
                    Assert.Equal(new[] { "Id" }, o.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, o.OnDelete);
                });
        }

        [Fact]
        public void AddForeignKeyOperation_composite()
        {
            Test(
                new AddForeignKeyOperation
                {
                    Name = "FK_Post_Blog_BlogId1_BlogId2",
                    Table = "Post",
                    Columns = new[] { "BlogId1", "BlogId2" },
                    PrincipalTable = "Blog",
                    PrincipalColumns = new[] { "Id1", "Id2" }
                },
                "mb.AddForeignKey(" + _eol +
                "    name: \"FK_Post_Blog_BlogId1_BlogId2\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    columns: new[] { \"BlogId1\", \"BlogId2\" }," + _eol +
                "    principalTable: \"Blog\"," + _eol +
                "    principalColumns: new[] { \"Id1\", \"Id2\" });",
                o =>
                {
                    Assert.Equal("FK_Post_Blog_BlogId1_BlogId2", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "BlogId1", "BlogId2" }, o.Columns);
                    Assert.Equal("Blog", o.PrincipalTable);
                    Assert.Equal(new[] { "Id1", "Id2" }, o.PrincipalColumns);
                });
        }

        [Fact]
        public void AddPrimaryKey_required_args()
        {
            Test(
                new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post",
                    Columns = new[] { "Id" }
                },
                "mb.AddPrimaryKey(" + _eol +
                "    name: \"PK_Post\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"Id\");",
                o =>
                {
                    Assert.Equal("PK_Post", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "Id" }, o.Columns);
                });
        }

        [Fact]
        public void AddPrimaryKey_all_args()
        {
            Test(
                new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Schema = "dbo",
                    Table = "Post",
                    Columns = new[] { "Id" }
                },
                "mb.AddPrimaryKey(" + _eol +
                "    name: \"PK_Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"Id\");",
                o =>
                {
                    Assert.Equal("PK_Post", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "Id" }, o.Columns);
                });
        }

        [Fact]
        public void AddPrimaryKey_composite()
        {
            Test(
                new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post",
                    Columns = new[] { "Id1", "Id2" }
                },
                "mb.AddPrimaryKey(" + _eol +
                "    name: \"PK_Post\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    columns: new[] { \"Id1\", \"Id2\" });",
                o =>
                {
                    Assert.Equal("PK_Post", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "Id1", "Id2" }, o.Columns);
                });
        }

        [Fact]
        public void AddUniqueConstraint_required_args()
        {
            Test(
                new AddUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Table = "Post",
                    Columns = new[] { "AltId" }
                },
                "mb.AddUniqueConstraint(" + _eol +
                "    name: \"AK_Post_AltId\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"AltId\");",
                o =>
                {
                    Assert.Equal("AK_Post_AltId", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "AltId" }, o.Columns);
                });
        }

        [Fact]
        public void AddUniqueConstraint_all_args()
        {
            Test(
                new AddUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Schema = "dbo",
                    Table = "Post",
                    Columns = new[] { "AltId" }
                },
                "mb.AddUniqueConstraint(" + _eol +
                "    name: \"AK_Post_AltId\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"AltId\");",
                o =>
                {
                    Assert.Equal("AK_Post_AltId", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "AltId" }, o.Columns);
                });
        }

        [Fact]
        public void AddUniqueConstraint_composite()
        {
            Test(
                new AddUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId1_AltId2",
                    Table = "Post",
                    Columns = new[] { "AltId1", "AltId2" }
                },
                "mb.AddUniqueConstraint(" + _eol +
                "    name: \"AK_Post_AltId1_AltId2\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    columns: new[] { \"AltId1\", \"AltId2\" });",
                o =>
                {
                    Assert.Equal("AK_Post_AltId1_AltId2", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "AltId1", "AltId2" }, o.Columns);
                });
        }

        [Fact]
        public void AlterColumnOperation_required_args()
        {
            Test(
                new AlterColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int)
                },
                "mb.AlterColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    nullable: false);",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Null(o.ColumnType);
                    Assert.Null(o.IsUnicode);
                    Assert.Null(o.IsFixedLength);
                    Assert.Null(o.MaxLength);
                    Assert.False(o.IsRowVersion);
                    Assert.False(o.IsNullable);
                    Assert.Null(o.DefaultValue);
                    Assert.Null(o.DefaultValueSql);
                    Assert.Null(o.ComputedColumnSql);
                    Assert.Equal(typeof(int), o.OldColumn.ClrType);
                    Assert.Null(o.OldColumn.ColumnType);
                    Assert.Null(o.OldColumn.IsUnicode);
                    Assert.Null(o.OldColumn.IsFixedLength);
                    Assert.Null(o.OldColumn.MaxLength);
                    Assert.False(o.OldColumn.IsRowVersion);
                    Assert.False(o.OldColumn.IsNullable);
                    Assert.Null(o.OldColumn.DefaultValue);
                    Assert.Null(o.OldColumn.DefaultValueSql);
                    Assert.Null(o.OldColumn.ComputedColumnSql);
                });
        }

        [Fact]
        public void AlterColumnOperation_all_args()
        {
            Test(
                new AlterColumnOperation
                {
                    Name = "Id",
                    Schema = "dbo",
                    Table = "Post",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsUnicode = false,
                    MaxLength = 30,
                    IsRowVersion = true,
                    IsNullable = true,
                    DefaultValue = 1,
                    IsFixedLength = true,
                    OldColumn =
                    {
                        ClrType = typeof(string),
                        ColumnType = "string",
                        IsUnicode = false,
                        MaxLength = 20,
                        IsRowVersion = true,
                        IsNullable = true,
                        DefaultValue = 0,
                        IsFixedLength = true
                    }
                },
                "mb.AlterColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    type: \"int\"," + _eol +
                "    unicode: false," + _eol +
                "    fixedLength: true," + _eol +
                "    maxLength: 30," + _eol +
                "    rowVersion: true," + _eol +
                "    nullable: true," + _eol +
                "    defaultValue: 1," + _eol +
                "    oldClrType: typeof(string)," + _eol +
                "    oldType: \"string\"," + _eol +
                "    oldUnicode: false," + _eol +
                "    oldFixedLength: true," + _eol +
                "    oldMaxLength: 20," + _eol +
                "    oldRowVersion: true," + _eol +
                "    oldNullable: true," + _eol +
                "    oldDefaultValue: 0);",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Equal("int", o.ColumnType);
                    Assert.False(o.IsUnicode);
                    Assert.True(o.IsFixedLength);
                    Assert.Equal(30, o.MaxLength);
                    Assert.True(o.IsRowVersion);
                    Assert.True(o.IsNullable);
                    Assert.Equal(1, o.DefaultValue);
                    Assert.Null(o.DefaultValueSql);
                    Assert.Null(o.ComputedColumnSql);
                    Assert.Equal(typeof(string), o.OldColumn.ClrType);
                    Assert.Equal("string", o.OldColumn.ColumnType);
                    Assert.False(o.OldColumn.IsUnicode);
                    Assert.True(o.OldColumn.IsFixedLength);
                    Assert.Equal(20, o.OldColumn.MaxLength);
                    Assert.True(o.OldColumn.IsRowVersion);
                    Assert.True(o.OldColumn.IsNullable);
                    Assert.Equal(0, o.OldColumn.DefaultValue);
                    Assert.Null(o.OldColumn.DefaultValueSql);
                    Assert.Null(o.OldColumn.ComputedColumnSql);
                });
        }

        [Fact]
        public void AlterColumnOperation_DefaultValueSql()
        {
            Test(
                new AlterColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int),
                    DefaultValueSql = "1"
                },
                "mb.AlterColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    nullable: false," + _eol +
                "    defaultValueSql: \"1\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("1", o.DefaultValueSql);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Null(o.ColumnType);
                    Assert.Null(o.IsUnicode);
                    Assert.Null(o.IsFixedLength);
                    Assert.Null(o.MaxLength);
                    Assert.False(o.IsRowVersion);
                    Assert.False(o.IsNullable);
                    Assert.Null(o.DefaultValue);
                    Assert.Null(o.ComputedColumnSql);
                    Assert.Equal(typeof(int), o.OldColumn.ClrType);
                    Assert.Null(o.OldColumn.ColumnType);
                    Assert.Null(o.OldColumn.IsUnicode);
                    Assert.Null(o.OldColumn.IsFixedLength);
                    Assert.Null(o.OldColumn.MaxLength);
                    Assert.False(o.OldColumn.IsRowVersion);
                    Assert.False(o.OldColumn.IsNullable);
                    Assert.Null(o.OldColumn.DefaultValue);
                    Assert.Null(o.OldColumn.DefaultValueSql);
                    Assert.Null(o.OldColumn.ComputedColumnSql);
                });
        }

        [Fact]
        public void AlterColumnOperation_computedColumnSql()
        {
            Test(
                new AlterColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int),
                    ComputedColumnSql = "1"
                },
                "mb.AlterColumn<int>(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    nullable: false," + _eol +
                "    computedColumnSql: \"1\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("1", o.ComputedColumnSql);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Null(o.ColumnType);
                    Assert.Null(o.IsUnicode);
                    Assert.Null(o.IsFixedLength);
                    Assert.Null(o.MaxLength);
                    Assert.False(o.IsRowVersion);
                    Assert.False(o.IsNullable);
                    Assert.Null(o.DefaultValue);
                    Assert.Null(o.DefaultValueSql);
                    Assert.Equal(typeof(int), o.OldColumn.ClrType);
                    Assert.Null(o.OldColumn.ColumnType);
                    Assert.Null(o.OldColumn.IsUnicode);
                    Assert.Null(o.OldColumn.IsFixedLength);
                    Assert.Null(o.OldColumn.MaxLength);
                    Assert.False(o.OldColumn.IsRowVersion);
                    Assert.False(o.OldColumn.IsNullable);
                    Assert.Null(o.OldColumn.DefaultValue);
                    Assert.Null(o.OldColumn.DefaultValueSql);
                    Assert.Null(o.OldColumn.ComputedColumnSql);
                });
        }

        [Fact]
        public void AlterDatabaseOperation()
        {
            Test(
                new AlterDatabaseOperation
                {
                    ["foo"] = "bar",
                    OldDatabase =
                    {
                        ["bar"] = "foo"
                    }
                },
                "mb.AlterDatabase()" + _eol +
                "    .Annotation(\"foo\", \"bar\")" + _eol +
                "    .OldAnnotation(\"bar\", \"foo\");",
                o =>
                {
                    Assert.Equal("bar", o["foo"]);
                    Assert.Equal("foo", o.OldDatabase["bar"]);
                });
        }

        [Fact]
        public void AlterSequenceOperation_required_args()
        {
            Test(
                new AlterSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence"
                },
                "mb.AlterSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Null(o.Schema);
                    Assert.Equal(1, o.IncrementBy);
                    Assert.Null(o.MinValue);
                    Assert.Null(o.MaxValue);
                    Assert.False(o.IsCyclic);
                    Assert.Equal(1, o.OldSequence.IncrementBy);
                    Assert.Null(o.OldSequence.MinValue);
                    Assert.Null(o.OldSequence.MaxValue);
                    Assert.False(o.OldSequence.IsCyclic);
                });
        }

        [Fact]
        public void AlterSequenceOperation_all_args()
        {
            Test(
                new AlterSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    IncrementBy = 3,
                    MinValue = 2,
                    MaxValue = 4,
                    IsCyclic = true,
                    OldSequence =
                    {
                        IncrementBy = 4,
                        MinValue = 3,
                        MaxValue = 5,
                        IsCyclic = true
                    }
                },
                "mb.AlterSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    incrementBy: 3," + _eol +
                "    minValue: 2L," + _eol +
                "    maxValue: 4L," + _eol +
                "    cyclic: true," + _eol +
                "    oldIncrementBy: 4," + _eol +
                "    oldMinValue: 3L," + _eol +
                "    oldMaxValue: 5L," + _eol +
                "    oldCyclic: true);",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal(3, o.IncrementBy);
                    Assert.Equal(2, o.MinValue);
                    Assert.Equal(4, o.MaxValue);
                    Assert.True(o.IsCyclic);
                    Assert.Equal(4, o.OldSequence.IncrementBy);
                    Assert.Equal(3, o.OldSequence.MinValue);
                    Assert.Equal(5, o.OldSequence.MaxValue);
                    Assert.True(o.OldSequence.IsCyclic);
                });
        }

        [Fact]
        public void AlterTableOperation()
        {
            Test(
                new AlterTableOperation
                {
                    Name = "Customer",
                    Schema = "dbo"
                },
                "mb.AlterTable(" + _eol +
                "    name: \"Customer\"," + _eol +
                "    schema: \"dbo\");",
                o =>
                {
                    Assert.Equal("Customer", o.Name);
                    Assert.Equal("dbo", o.Schema);
                });
        }

        [Fact]
        public void CreateIndexOperation_required_args()
        {
            Test(
                new CreateIndexOperation
                {
                    Name = "IX_Post_Title",
                    Table = "Post",
                    Columns = new[] { "Title" }
                },
                "mb.CreateIndex(" + _eol +
                "    name: \"IX_Post_Title\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"Title\");",
                o =>
                {
                    Assert.Equal("IX_Post_Title", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "Title" }, o.Columns);
                });
        }

        [Fact]
        public void CreateIndexOperation_all_args()
        {
            Test(
                new CreateIndexOperation
                {
                    Name = "IX_Post_Title",
                    Schema = "dbo",
                    Table = "Post",
                    Columns = new[] { "Title" },
                    IsUnique = true,
                    Filter = "[Title] IS NOT NULL"
                },
                "mb.CreateIndex(" + _eol +
                "    name: \"IX_Post_Title\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    column: \"Title\"," + _eol +
                "    unique: true," + _eol +
                "    filter: \"[Title] IS NOT NULL\");",
                o =>
                {
                    Assert.Equal("IX_Post_Title", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "Title" }, o.Columns);
                    Assert.True(o.IsUnique);
                });
        }

        [Fact]
        public void CreateIndexOperation_composite()
        {
            Test(
                new CreateIndexOperation
                {
                    Name = "IX_Post_Title_Subtitle",
                    Table = "Post",
                    Columns = new[] { "Title", "Subtitle" }
                },
                "mb.CreateIndex(" + _eol +
                "    name: \"IX_Post_Title_Subtitle\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    columns: new[] { \"Title\", \"Subtitle\" });",
                o =>
                {
                    Assert.Equal("IX_Post_Title_Subtitle", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal(new[] { "Title", "Subtitle" }, o.Columns);
                });
        }

        [Fact]
        public void CreateSchemaOperation_required_args()
        {
            Test(
                new EnsureSchemaOperation
                {
                    Name = "my"
                },
                "mb.EnsureSchema(" + _eol +
                "    name: \"my\");",
                o => Assert.Equal("my", o.Name));
        }

        [Fact]
        public void CreateSequenceOperation_required_args()
        {
            Test(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    ClrType = typeof(long)
                },
                "mb.CreateSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal(typeof(long), o.ClrType);
                });
        }

        [Fact]
        public void CreateSequenceOperation_required_args_not_long()
        {
            Test(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    ClrType = typeof(int)
                },
                "mb.CreateSequence<int>(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal(typeof(int), o.ClrType);
                });
        }

        [Fact]
        public void CreateSequenceOperation_all_args()
        {
            Test(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    ClrType = typeof(long),
                    StartValue = 3,
                    IncrementBy = 5,
                    MinValue = 2,
                    MaxValue = 4,
                    IsCyclic = true
                },
                "mb.CreateSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    startValue: 3L," + _eol +
                "    incrementBy: 5," + _eol +
                "    minValue: 2L," + _eol +
                "    maxValue: 4L," + _eol +
                "    cyclic: true);",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal(typeof(long), o.ClrType);
                    Assert.Equal(3, o.StartValue);
                    Assert.Equal(5, o.IncrementBy);
                    Assert.Equal(2, o.MinValue);
                    Assert.Equal(4, o.MaxValue);
                    Assert.True(o.IsCyclic);
                });
        }

        [Fact]
        public void CreateSequenceOperation_all_args_not_long()
        {
            Test(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    ClrType = typeof(int),
                    StartValue = 3,
                    IncrementBy = 5,
                    MinValue = 2,
                    MaxValue = 4,
                    IsCyclic = true
                },
                "mb.CreateSequence<int>(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    startValue: 3L," + _eol +
                "    incrementBy: 5," + _eol +
                "    minValue: 2L," + _eol +
                "    maxValue: 4L," + _eol +
                "    cyclic: true);",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal(typeof(int), o.ClrType);
                    Assert.Equal(3, o.StartValue);
                    Assert.Equal(5, o.IncrementBy);
                    Assert.Equal(2, o.MinValue);
                    Assert.Equal(4, o.MaxValue);
                    Assert.True(o.IsCyclic);
                });
        }

        [Fact]
        public void CreateTableOperation_Columns_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "Post",
                            ClrType = typeof(int)
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        Id = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("Post", o.Name);
                    Assert.Equal(1, o.Columns.Count);

                    Assert.Equal("Id", o.Columns[0].Name);
                    Assert.Equal("Post", o.Columns[0].Table);
                    Assert.Equal(typeof(int), o.Columns[0].ClrType);
                });
        }

        [Fact]
        public void CreateTableOperation_Columns_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Post Id",
                            Schema = "dbo",
                            Table = "Post",
                            ClrType = typeof(int),
                            ColumnType = "int",
                            IsUnicode = false,
                            IsFixedLength = true,
                            MaxLength = 30,
                            IsRowVersion = true,
                            IsNullable = true,
                            DefaultValue = 1
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        PostId = table.Column<int>(name: \"Post Id\", type: \"int\", unicode: false, fixedLength: true, maxLength: 30, rowVersion: true, nullable: true, defaultValue: 1)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("Post", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal(1, o.Columns.Count);

                    Assert.Equal("Post Id", o.Columns[0].Name);
                    Assert.Equal("dbo", o.Columns[0].Schema);
                    Assert.Equal("Post", o.Columns[0].Table);
                    Assert.Equal(typeof(int), o.Columns[0].ClrType);
                    Assert.Equal("int", o.Columns[0].ColumnType);
                    Assert.True(o.Columns[0].IsNullable);
                    Assert.False(o.Columns[0].IsUnicode);
                    Assert.True(o.Columns[0].IsFixedLength);
                    Assert.Equal(1, o.Columns[0].DefaultValue);
                });
        }

        [Fact]
        public void CreateTableOperation_Columns_DefaultValueSql()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "Post",
                            ClrType = typeof(int),
                            DefaultValueSql = "1"
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        Id = table.Column<int>(nullable: false, defaultValueSql: \"1\")" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.Columns.Count);

                    Assert.Equal("Id", o.Columns[0].Name);
                    Assert.Equal("Post", o.Columns[0].Table);
                    Assert.Equal(typeof(int), o.Columns[0].ClrType);
                    Assert.Equal("1", o.Columns[0].DefaultValueSql);
                });
        }

        [Fact]
        public void CreateTableOperation_Columns_computedColumnSql()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "Post",
                            ClrType = typeof(int),
                            ComputedColumnSql = "1"
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        Id = table.Column<int>(nullable: false, computedColumnSql: \"1\")" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.Columns.Count);

                    Assert.Equal("Id", o.Columns[0].Name);
                    Assert.Equal("Post", o.Columns[0].Table);
                    Assert.Equal(typeof(int), o.Columns[0].ClrType);
                    Assert.Equal("1", o.Columns[0].ComputedColumnSql);
                });
        }

        [Fact]
        public void CreateTableOperation_ForeignKeys_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "BlogId",
                            ClrType = typeof(int)
                        }
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation
                        {
                            Name = "FK_Post_Blog_BlogId",
                            Table = "Post",
                            Columns = new[] { "BlogId" },
                            PrincipalTable = "Blog",
                            PrincipalColumns = new[] { "Id" }
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        BlogId = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.ForeignKey(" + _eol +
                "            name: \"FK_Post_Blog_BlogId\"," + _eol +
                "            column: x => x.BlogId," + _eol +
                "            principalTable: \"Blog\"," + _eol +
                "            principalColumn: \"Id\");" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.ForeignKeys.Count);

                    var fk = o.ForeignKeys.First();
                    Assert.Equal("FK_Post_Blog_BlogId", fk.Name);
                    Assert.Equal("Post", fk.Table);
                    Assert.Equal(new[] { "BlogId" }, fk.Columns.ToArray());
                    Assert.Equal("Blog", fk.PrincipalTable);
                });
        }

        [Fact]
        public void CreateTableOperation_ForeignKeys_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Schema = "dbo",
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "BlogId",
                            ClrType = typeof(int)
                        }
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation
                        {
                            Schema = "dbo",
                            Table = "Post",
                            Name = "FK_Post_Blog_BlogId",
                            Columns = new[] { "BlogId" },
                            PrincipalTable = "Blog",
                            PrincipalSchema = "my",
                            PrincipalColumns = new[] { "Id" },
                            OnUpdate = ReferentialAction.SetNull,
                            OnDelete = ReferentialAction.SetDefault
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        BlogId = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.ForeignKey(" + _eol +
                "            name: \"FK_Post_Blog_BlogId\"," + _eol +
                "            column: x => x.BlogId," + _eol +
                "            principalSchema: \"my\"," + _eol +
                "            principalTable: \"Blog\"," + _eol +
                "            principalColumn: \"Id\"," + _eol +
                "            onUpdate: ReferentialAction.SetNull," + _eol +
                "            onDelete: ReferentialAction.SetDefault);" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.ForeignKeys.Count);

                    var fk = o.ForeignKeys.First();
                    Assert.Equal("Post", fk.Table);
                    Assert.Equal("dbo", fk.Schema);
                    Assert.Equal("FK_Post_Blog_BlogId", fk.Name);
                    Assert.Equal(new[] { "BlogId" }, fk.Columns.ToArray());
                    Assert.Equal("Blog", fk.PrincipalTable);
                    Assert.Equal("my", fk.PrincipalSchema);
                    Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                    Assert.Equal(ReferentialAction.SetNull, fk.OnUpdate);
                    Assert.Equal(ReferentialAction.SetDefault, fk.OnDelete);
                });
        }

        [Fact]
        public void CreateTableOperation_ForeignKeys_composite()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "BlogId1",
                            ClrType = typeof(int)
                        },
                        new AddColumnOperation
                        {
                            Name = "BlogId2",
                            ClrType = typeof(int)
                        }
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation
                        {
                            Name = "FK_Post_Blog_BlogId1_BlogId2",
                            Table = "Post",
                            Columns = new[] { "BlogId1", "BlogId2" },
                            PrincipalTable = "Blog",
                            PrincipalColumns = new[] { "Id1", "Id2" }
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        BlogId1 = table.Column<int>(nullable: false)," + _eol +
                "        BlogId2 = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.ForeignKey(" + _eol +
                "            name: \"FK_Post_Blog_BlogId1_BlogId2\"," + _eol +
                "            columns: x => new { x.BlogId1, x.BlogId2 }," + _eol +
                "            principalTable: \"Blog\"," + _eol +
                "            principalColumns: new[] { \"Id1\", \"Id2\" });" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.ForeignKeys.Count);

                    var fk = o.ForeignKeys.First();
                    Assert.Equal("Post", fk.Table);
                    Assert.Equal(new[] { "BlogId1", "BlogId2" }, fk.Columns.ToArray());
                    Assert.Equal("Blog", fk.PrincipalTable);
                    Assert.Equal(new[] { "Id1", "Id2" }, fk.PrincipalColumns);
                });
        }

        [Fact]
        public void CreateTableOperation_PrimaryKey_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            ClrType = typeof(int)
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = "PK_Post",
                        Table = "Post",
                        Columns = new[] { "Id" }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        Id = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.PrimaryKey(\"PK_Post\", x => x.Id);" + _eol +
                "    });",
                o =>
                {
                    Assert.NotNull(o.PrimaryKey);

                    Assert.Equal("PK_Post", o.PrimaryKey.Name);
                    Assert.Equal("Post", o.PrimaryKey.Table);
                    Assert.Equal(new[] { "Id" }, o.PrimaryKey.Columns);
                });
        }

        [Fact]
        public void CreateTableOperation_PrimaryKey_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            ClrType = typeof(int)
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = "PK_Post",
                        Schema = "dbo",
                        Table = "Post",
                        Columns = new[] { "Id" }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        Id = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.PrimaryKey(\"PK_Post\", x => x.Id);" + _eol +
                "    });",
                o =>
                {
                    Assert.NotNull(o.PrimaryKey);

                    Assert.Equal("PK_Post", o.PrimaryKey.Name);
                    Assert.Equal("dbo", o.PrimaryKey.Schema);
                    Assert.Equal("Post", o.PrimaryKey.Table);
                    Assert.Equal(new[] { "Id" }, o.PrimaryKey.Columns);
                });
        }

        [Fact]
        public void CreateTableOperation_PrimaryKey_composite()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id1",
                            ClrType = typeof(int)
                        },
                        new AddColumnOperation
                        {
                            Name = "Id2",
                            ClrType = typeof(int)
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = "PK_Post",
                        Table = "Post",
                        Columns = new[] { "Id1", "Id2" }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        Id1 = table.Column<int>(nullable: false)," + _eol +
                "        Id2 = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.PrimaryKey(\"PK_Post\", x => new { x.Id1, x.Id2 });" + _eol +
                "    });",
                o =>
                {
                    Assert.NotNull(o.PrimaryKey);

                    Assert.Equal("PK_Post", o.PrimaryKey.Name);
                    Assert.Equal("Post", o.PrimaryKey.Table);
                    Assert.Equal(new[] { "Id1", "Id2" }, o.PrimaryKey.Columns);
                });
        }

        [Fact]
        public void CreateTableOperation_UniqueConstraints_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "AltId",
                            ClrType = typeof(int)
                        }
                    },
                    UniqueConstraints =
                    {
                        new AddUniqueConstraintOperation
                        {
                            Name = "AK_Post_AltId",
                            Table = "Post",
                            Columns = new[] { "AltId" }
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        AltId = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.UniqueConstraint(\"AK_Post_AltId\", x => x.AltId);" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.UniqueConstraints.Count);

                    Assert.Equal("AK_Post_AltId", o.UniqueConstraints[0].Name);
                    Assert.Equal("Post", o.UniqueConstraints[0].Table);
                    Assert.Equal(new[] { "AltId" }, o.UniqueConstraints[0].Columns);
                });
        }

        [Fact]
        public void CreateTableOperation_UniqueConstraints_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "AltId",
                            ClrType = typeof(int)
                        }
                    },
                    UniqueConstraints =
                    {
                        new AddUniqueConstraintOperation
                        {
                            Name = "AK_Post_AltId",
                            Schema = "dbo",
                            Table = "Post",
                            Columns = new[] { "AltId" }
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        AltId = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.UniqueConstraint(\"AK_Post_AltId\", x => x.AltId);" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.UniqueConstraints.Count);

                    Assert.Equal("AK_Post_AltId", o.UniqueConstraints[0].Name);
                    Assert.Equal("dbo", o.UniqueConstraints[0].Schema);
                    Assert.Equal("Post", o.UniqueConstraints[0].Table);
                    Assert.Equal(new[] { "AltId" }, o.UniqueConstraints[0].Columns);
                });
        }

        [Fact]
        public void CreateTableOperation_UniqueConstraints_composite()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "AltId1",
                            ClrType = typeof(int)
                        },
                        new AddColumnOperation
                        {
                            Name = "AltId2",
                            ClrType = typeof(int)
                        }
                    },
                    UniqueConstraints =
                    {
                        new AddUniqueConstraintOperation
                        {
                            Name = "AK_Post_AltId1_AltId2",
                            Table = "Post",
                            Columns = new[] { "AltId1", "AltId2" }
                        }
                    }
                },
                "mb.CreateTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    columns: table => new" + _eol +
                "    {" + _eol +
                "        AltId1 = table.Column<int>(nullable: false)," + _eol +
                "        AltId2 = table.Column<int>(nullable: false)" + _eol +
                "    }," + _eol +
                "    constraints: table =>" + _eol +
                "    {" + _eol +
                "        table.UniqueConstraint(\"AK_Post_AltId1_AltId2\", x => new { x.AltId1, x.AltId2 });" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal(1, o.UniqueConstraints.Count);

                    Assert.Equal("AK_Post_AltId1_AltId2", o.UniqueConstraints[0].Name);
                    Assert.Equal("Post", o.UniqueConstraints[0].Table);
                    Assert.Equal(new[] { "AltId1", "AltId2" }, o.UniqueConstraints[0].Columns);
                });
        }

        [Fact]
        public void DropColumnOperation_required_args()
        {
            Test(
                new DropColumnOperation
                {
                    Name = "Id",
                    Table = "Post"
                },
                "mb.DropColumn(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropColumnOperation_all_args()
        {
            Test(
                new DropColumnOperation
                {
                    Name = "Id",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropColumn(" + _eol +
                "    name: \"Id\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropForeignKeyOperation_required_args()
        {
            Test(
                new DropForeignKeyOperation
                {
                    Name = "FK_Post_BlogId",
                    Table = "Post"
                },
                "mb.DropForeignKey(" + _eol +
                "    name: \"FK_Post_BlogId\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("FK_Post_BlogId", o.Name);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropForeignKeyOperation_all_args()
        {
            Test(
                new DropForeignKeyOperation
                {
                    Name = "FK_Post_BlogId",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropForeignKey(" + _eol +
                "    name: \"FK_Post_BlogId\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("FK_Post_BlogId", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropIndexOperation_required_args()
        {
            Test(
                new DropIndexOperation
                {
                    Name = "IX_Post_Title",
                    Table = "Post"
                },
                "mb.DropIndex(" + _eol +
                "    name: \"IX_Post_Title\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("IX_Post_Title", o.Name);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropIndexOperation_all_args()
        {
            Test(
                new DropIndexOperation
                {
                    Name = "IX_Post_Title",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropIndex(" + _eol +
                "    name: \"IX_Post_Title\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("IX_Post_Title", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropPrimaryKeyOperation_required_args()
        {
            Test(
                new DropPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post"
                },
                "mb.DropPrimaryKey(" + _eol +
                "    name: \"PK_Post\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("PK_Post", o.Name);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropPrimaryKeyOperation_all_args()
        {
            Test(
                new DropPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropPrimaryKey(" + _eol +
                "    name: \"PK_Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("PK_Post", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropSchemaOperation_required_args()
        {
            Test(
                new DropSchemaOperation
                {
                    Name = "my"
                },
                "mb.DropSchema(" + _eol +
                "    name: \"my\");",
                o => Assert.Equal("my", o.Name));
        }

        [Fact]
        public void DropSequenceOperation_required_args()
        {
            Test(
                new DropSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence"
                },
                "mb.DropSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));
        }

        [Fact]
        public void DropSequenceOperation_all_args()
        {
            Test(
                new DropSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo"
                },
                "mb.DropSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    schema: \"dbo\");",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal("dbo", o.Schema);
                });
        }

        [Fact]
        public void DropTableOperation_required_args()
        {
            Test(
                new DropTableOperation
                {
                    Name = "Post"
                },
                "mb.DropTable(" + _eol +
                "    name: \"Post\");",
                o => Assert.Equal("Post", o.Name));
        }

        [Fact]
        public void DropTableOperation_all_args()
        {
            Test(
                new DropTableOperation
                {
                    Name = "Post",
                    Schema = "dbo"
                },
                "mb.DropTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    schema: \"dbo\");",
                o =>
                {
                    Assert.Equal("Post", o.Name);
                    Assert.Equal("dbo", o.Schema);
                });
        }

        [Fact]
        public void DropUniqueConstraintOperation_required_args()
        {
            Test(
                new DropUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Table = "Post"
                },
                "mb.DropUniqueConstraint(" + _eol +
                "    name: \"AK_Post_AltId\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("AK_Post_AltId", o.Name);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void DropUniqueConstraintOperation_all_args()
        {
            Test(
                new DropUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropUniqueConstraint(" + _eol +
                "    name: \"AK_Post_AltId\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\");",
                o =>
                {
                    Assert.Equal("AK_Post_AltId", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                });
        }

        [Fact]
        public void RenameColumnOperation_required_args()
        {
            Test(
                new RenameColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    NewName = "PostId"
                },
                "mb.RenameColumn(" + _eol +
                "    name: \"Id\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    newName: \"PostId\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("PostId", o.NewName);
                });
        }

        [Fact]
        public void RenameColumnOperation_all_args()
        {
            Test(
                new RenameColumnOperation
                {
                    Name = "Id",
                    Schema = "dbo",
                    Table = "Post",
                    NewName = "PostId"
                },
                "mb.RenameColumn(" + _eol +
                "    name: \"Id\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    newName: \"PostId\");",
                o =>
                {
                    Assert.Equal("Id", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("PostId", o.NewName);
                });
        }

        [Fact]
        public void RenameIndexOperation_required_args()
        {
            Test(
                new RenameIndexOperation
                {
                    Name = "IX_Post_Title",
                    Table = "Post",
                    NewName = "IX_Post_PostTitle"
                },
                "mb.RenameIndex(" + _eol +
                "    name: \"IX_Post_Title\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    newName: \"IX_Post_PostTitle\");",
                o =>
                {
                    Assert.Equal("IX_Post_Title", o.Name);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("IX_Post_PostTitle", o.NewName);
                });
        }

        [Fact]
        public void RenameIndexOperation_all_args()
        {
            Test(
                new RenameIndexOperation
                {
                    Name = "IX_dbo.Post_Title",
                    Schema = "dbo",
                    Table = "Post",
                    NewName = "IX_dbo.Post_PostTitle"
                },
                "mb.RenameIndex(" + _eol +
                "    name: \"IX_dbo.Post_Title\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"Post\"," + _eol +
                "    newName: \"IX_dbo.Post_PostTitle\");",
                o =>
                {
                    Assert.Equal("IX_dbo.Post_Title", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Post", o.Table);
                    Assert.Equal("IX_dbo.Post_PostTitle", o.NewName);
                });
        }

        [Fact]
        public void RenameSequenceOperation_required_args()
        {
            Test(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence"
                },
                "mb.RenameSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));
        }

        [Fact]
        public void RenameSequenceOperation_all_args()
        {
            Test(
                new RenameSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    NewName = "MySequence",
                    NewSchema = "my"
                },
                "mb.RenameSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    newName: \"MySequence\"," + _eol +
                "    newSchema: \"my\");",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("MySequence", o.NewName);
                    Assert.Equal("my", o.NewSchema);
                });
        }

        [Fact]
        public void RenameTableOperation_required_args()
        {
            Test(
                new RenameTableOperation
                {
                    Name = "Post"
                },
                "mb.RenameTable(" + _eol +
                "    name: \"Post\");",
                o => Assert.Equal("Post", o.Name));
        }

        [Fact]
        public void RenameTableOperation_all_args()
        {
            Test(
                new RenameTableOperation
                {
                    Name = "Post",
                    Schema = "dbo",
                    NewName = "Posts",
                    NewSchema = "my"
                },
                "mb.RenameTable(" + _eol +
                "    name: \"Post\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    newName: \"Posts\"," + _eol +
                "    newSchema: \"my\");",
                o =>
                {
                    Assert.Equal("Post", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("Posts", o.NewName);
                    Assert.Equal("my", o.NewSchema);
                });
        }

        [Fact]
        public void RestartSequenceOperation_required_args()
        {
            Test(
                new RestartSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    StartValue = 1
                },
                "mb.RestartSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    startValue: 1L);",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal(1, o.StartValue);
                });
        }

        [Fact]
        public void RestartSequenceOperation_all_args()
        {
            Test(
                new RestartSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    StartValue = 1
                },
                "mb.RestartSequence(" + _eol +
                "    name: \"EntityFrameworkHiLoSequence\"," + _eol +
                "    schema: \"dbo\"," + _eol +
                "    startValue: 1L);",
                o =>
                {
                    Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal(1, o.StartValue);
                });
        }

        [Fact]
        public void SqlOperation_required_args()
        {
            Test(
                new SqlOperation
                {
                    Sql = "-- I <3 DDL"
                },
                "mb.Sql(\"-- I <3 DDL\");",
                o => Assert.Equal("-- I <3 DDL", o.Sql));
        }

        private static readonly LineString _lineString1 = new LineString(
            new[] { new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(7.1, 7.2) })
        {
            SRID = 4326
        };

        private static readonly LineString _lineString2 = new LineString(
            new[] { new Coordinate(7.1, 7.2), new Coordinate(20.2, 20.2), new Coordinate(20.20, 1.1), new Coordinate(70.1, 70.2) })
        {
            SRID = 4326
        };

        private static readonly MultiPoint _multiPoint = new MultiPoint(
            new IPoint[] { new Point(1.1, 2.2), new Point(2.2, 2.2), new Point(2.2, 1.1) })
        {
            SRID = 4326
        };

        private static readonly Polygon _polygon1 = new Polygon(
            new LinearRing(
                new[] { new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(1.1, 2.2) }))
        {
            SRID = 4326
        };

        private static readonly Polygon _polygon2 = new Polygon(
            new LinearRing(
                new[] { new Coordinate(10.1, 20.2), new Coordinate(20.2, 20.2), new Coordinate(20.2, 10.1), new Coordinate(10.1, 20.2) }))
        {
            SRID = 4326
        };

        private static readonly Point _point1 = new Point(1.1, 2.2, 3.3)
        {
            SRID = 4326
        };

        private static readonly MultiLineString _multiLineString = new MultiLineString(
            new ILineString[] { _lineString1, _lineString2, })
        {
            SRID = 4326
        };

        private static readonly MultiPolygon _multiPolygon = new MultiPolygon(
            new IPolygon[] { _polygon2, _polygon1, })
        {
            SRID = 4326
        };

        private static readonly GeometryCollection _geometryCollection = new GeometryCollection(
            new IGeometry[] { _lineString1, _lineString2, _multiPoint, _polygon1, _polygon2, _point1, _multiLineString, _multiPolygon })
        {
            SRID = 4326
        };

        [Fact]
        public void InsertDataOperation_all_args()
        {
            Test(
                new InsertDataOperation
                {
                    Schema = "dbo",
                    Table = "People",
                    Columns = new[] { "Id", "Full Name", "Geometry" },
                    Values = new object[,]
                    {
                        { 0, null, null },
                        { 1, "Daenerys Targaryen", _point1 },
                        { 2, "John Snow", _polygon1 },
                        { 3, "Arya Stark", _lineString1 },
                        { 4, "Harry Strickland", _multiPoint },
                        { 5, "The Imp", _multiPolygon },
                        { 6, "The Kingslayer", _multiLineString },
                        { 7, "Aemon Targaryen", _geometryCollection },
                    }
                },
                "mb.InsertData(" + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"People\"," + _eol +
                "    columns: new[] { \"Id\", \"Full Name\", \"Geometry\" }," + _eol +
                "    values: new object[,]" + _eol +
                "    {" + _eol +
                "        { 0, null, null }," + _eol +
                "        { 1, \"Daenerys Targaryen\", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;POINT (1.1 2.2 3.3)\") }," + _eol +
                "        { 2, \"John Snow\", (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))\") }," + _eol +
                "        { 3, \"Arya Stark\", (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)\") }," + _eol +
                "        { 4, \"Harry Strickland\", (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))\") }," + _eol +
                "        { 5, \"The Imp\", (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))\") }," + _eol +
                "        { 6, \"The Kingslayer\", (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))\") }," + _eol +
                "        { 7, \"Aemon Targaryen\", (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;GEOMETRYCOLLECTION (LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), LINESTRING (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2), MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1)), POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)), POLYGON ((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), POINT (1.1 2.2 3.3), MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2)), MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))))\") }" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("People", o.Table);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(8, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("John Snow", o.Values[2, 1]);
                    Assert.Equal(_point1, o.Values[1, 2]);
                    Assert.Equal(_polygon1, o.Values[2, 2]);
                    Assert.Equal(_lineString1, o.Values[3, 2]);
                    Assert.Equal(_multiPoint, o.Values[4, 2]);
                    Assert.Equal(_multiPolygon, o.Values[5, 2]);
                    Assert.Equal(_multiLineString, o.Values[6, 2]);
                    Assert.Equal(_geometryCollection, o.Values[7, 2]);
                });
        }

        [Fact]
        public void InsertDataOperation_required_args()
        {
            Test(
                new InsertDataOperation
                {
                    Table = "People",
                    Columns = new[] { "Geometry" },
                    Values = new object[,]
                        { { _point1 } }
                },
                "mb.InsertData(" + _eol +
                "    table: \"People\"," + _eol +
                "    column: \"Geometry\"," + _eol +
                "    value: (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;POINT (1.1 2.2 3.3)\"));",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(1, o.Values.GetLength(1));
                    Assert.Equal(_point1, o.Values[0, 0]);
                });
        }

        [Fact]
        public void InsertDataOperation_required_args_composite()
        {
            Test(
                new InsertDataOperation
                {
                    Table = "People",
                    Columns = new[] { "First Name", "Last Name", "Geometry" },
                    Values = new object[,]
                        { { "John", "Snow", _polygon1 } }
                },
                "mb.InsertData(" + _eol +
                "    table: \"People\"," + _eol +
                "    columns: new[] { \"First Name\", \"Last Name\", \"Geometry\" }," + _eol +
                "    values: new object[] { \"John\", \"Snow\", (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))\") });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("Snow", o.Values[0, 1]);
                    Assert.Equal(_polygon1, o.Values[0, 2]);
                });
        }

        [Fact]
        public void InsertDataOperation_required_args_multiple_rows()
        {
            Test(
                new InsertDataOperation
                {
                    Table = "People",
                    Columns = new[] { "Geometries" },
                    Values = new object[,]
                    {
                        { _lineString1 },
                        { _multiPoint }
                    }
                },
                "mb.InsertData(" + _eol +
                "    table: \"People\"," + _eol +
                "    column: \"Geometries\"," + _eol +
                "    values: new object[]" + _eol +
                "    {" + _eol +
                "        (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)\")," + _eol +
                "        (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read(\"SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))\")" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.Columns.Length);
                    Assert.Equal(2, o.Values.GetLength(0));
                    Assert.Equal(1, o.Values.GetLength(1));
                    Assert.Equal(_lineString1, o.Values[0, 0]);
                    Assert.Equal(_multiPoint, o.Values[1, 0]);
                });
        }

        [Fact]
        public void DeleteDataOperation_all_args()
        {
            Test(
                new DeleteDataOperation
                {
                    Schema = "dbo",
                    Table = "People",
                    KeyColumns = new[] { "First Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor" },
                        { "Daenerys" },
                        { "John" },
                        { "Arya" },
                        { "Harry" }
                    }
                },
                "mb.DeleteData(" + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"First Name\"," + _eol +
                "    keyValues: new object[]" + _eol +
                "    {" + _eol +
                "        \"Hodor\"," + _eol +
                "        \"Daenerys\"," + _eol +
                "        \"John\"," + _eol +
                "        \"Arya\"," + _eol +
                "        \"Harry\"" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(5, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("John", o.KeyValues[2, 0]);
                });
        }

        [Fact]
        public void DeleteDataOperation_all_args_composite()
        {
            Test(
                new DeleteDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor", null },
                        { "Daenerys", "Targaryen" },
                        { "John", "Snow" },
                        { "Arya", "Stark" },
                        { "Harry", "Strickland" }
                    }
                },
                "mb.DeleteData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumns: new[] { \"First Name\", \"Last Name\" }," + _eol +
                "    keyValues: new object[,]" + _eol +
                "    {" + _eol +
                "        { \"Hodor\", null }," + _eol +
                "        { \"Daenerys\", \"Targaryen\" }," + _eol +
                "        { \"John\", \"Snow\" }," + _eol +
                "        { \"Arya\", \"Stark\" }," + _eol +
                "        { \"Harry\", \"Strickland\" }" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(2, o.KeyColumns.Length);
                    Assert.Equal(5, o.KeyValues.GetLength(0));
                    Assert.Equal(2, o.KeyValues.GetLength(1));
                    Assert.Equal("Snow", o.KeyValues[2, 1]);
                });
        }

        [Fact]
        public void DeleteDataOperation_required_args()
        {
            Test(
                new DeleteDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "Last Name" },
                    KeyValues = new object[,]
                        { { "Snow" } }
                },
                "mb.DeleteData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"Last Name\"," + _eol +
                "    keyValue: \"Snow\");",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("Snow", o.KeyValues[0, 0]);
                });
        }

        [Fact]
        public void DeleteDataOperation_required_args_composite()
        {
            Test(
                new DeleteDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                        { { "John", "Snow" } }
                },
                "mb.DeleteData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumns: new[] { \"First Name\", \"Last Name\" }," + _eol +
                "    keyValues: new object[] { \"John\", \"Snow\" });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(2, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(2, o.KeyValues.GetLength(1));
                    Assert.Equal("Snow", o.KeyValues[0, 1]);
                });
        }

        [Fact]
        public void UpdateDataOperation_all_args()
        {
            Test(
                new UpdateDataOperation
                {
                    Schema = "dbo",
                    Table = "People",
                    KeyColumns = new[] { "First Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor" },
                        { "Daenerys" }
                    },
                    Columns = new[] { "Birthplace", "House Allegiance", "Culture" },
                    Values = new object[,]
                    {
                        { "Winterfell", "Stark", "Northmen" },
                        { "Dragonstone", "Targaryen", "Valyrian" }
                    }
                },
                "mb.UpdateData(" + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"First Name\"," + _eol +
                "    keyValues: new object[]" + _eol +
                "    {" + _eol +
                "        \"Hodor\"," + _eol +
                "        \"Daenerys\"" + _eol +
                "    }," + _eol +
                "    columns: new[] { \"Birthplace\", \"House Allegiance\", \"Culture\" }," + _eol +
                "    values: new object[,]" + _eol +
                "    {" + _eol +
                "        { \"Winterfell\", \"Stark\", \"Northmen\" }," + _eol +
                "        { \"Dragonstone\", \"Targaryen\", \"Valyrian\" }" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(2, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(2, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[1, 1]);
                });
        }

        [Fact]
        public void UpdateDataOperation_all_args_composite()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor", null },
                        { "Daenerys", "Targaryen" }
                    },
                    Columns = new[] { "House Allegiance" },
                    Values = new object[,]
                    {
                        { "Stark" },
                        { "Targaryen" }
                    }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumns: new[] { \"First Name\", \"Last Name\" }," + _eol +
                "    keyValues: new object[,]" + _eol +
                "    {" + _eol +
                "        { \"Hodor\", null }," + _eol +
                "        { \"Daenerys\", \"Targaryen\" }" + _eol +
                "    }," + _eol +
                "    column: \"House Allegiance\"," + _eol +
                "    values: new object[]" + _eol +
                "    {" + _eol +
                "        \"Stark\"," + _eol +
                "        \"Targaryen\"" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(2, o.KeyColumns.Length);
                    Assert.Equal(2, o.KeyValues.GetLength(0));
                    Assert.Equal(2, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                    Assert.Equal(1, o.Columns.Length);
                    Assert.Equal(2, o.Values.GetLength(0));
                    Assert.Equal(1, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[1, 0]);
                });
        }

        [Fact]
        public void UpdateDataOperation_all_args_composite_multi()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor", null },
                        { "Daenerys", "Targaryen" }
                    },
                    Columns = new[] { "Birthplace", "House Allegiance", "Culture" },
                    Values = new object[,]
                    {
                        { "Winterfell", "Stark", "Northmen" },
                        { "Dragonstone", "Targaryen", "Valyrian" }
                    }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumns: new[] { \"First Name\", \"Last Name\" }," + _eol +
                "    keyValues: new object[,]" + _eol +
                "    {" + _eol +
                "        { \"Hodor\", null }," + _eol +
                "        { \"Daenerys\", \"Targaryen\" }" + _eol +
                "    }," + _eol +
                "    columns: new[] { \"Birthplace\", \"House Allegiance\", \"Culture\" }," + _eol +
                "    values: new object[,]" + _eol +
                "    {" + _eol +
                "        { \"Winterfell\", \"Stark\", \"Northmen\" }," + _eol +
                "        { \"Dragonstone\", \"Targaryen\", \"Valyrian\" }" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(2, o.KeyColumns.Length);
                    Assert.Equal(2, o.KeyValues.GetLength(0));
                    Assert.Equal(2, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(2, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[1, 1]);
                });
        }

        [Fact]
        public void UpdateDataOperation_all_args_multi()
        {
            Test(
                new UpdateDataOperation
                {
                    Schema = "dbo",
                    Table = "People",
                    KeyColumns = new[] { "Full Name" },
                    KeyValues = new object[,]
                        { { "Daenerys Targaryen" } },
                    Columns = new[] { "Birthplace", "House Allegiance", "Culture" },
                    Values = new object[,]
                        { { "Dragonstone", "Targaryen", "Valyrian" } }
                },
                "mb.UpdateData(" + _eol +
                "    schema: \"dbo\"," + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"Full Name\"," + _eol +
                "    keyValue: \"Daenerys Targaryen\"," + _eol +
                "    columns: new[] { \"Birthplace\", \"House Allegiance\", \"Culture\" }," + _eol +
                "    values: new object[] { \"Dragonstone\", \"Targaryen\", \"Valyrian\" });",
                o =>
                {
                    Assert.Equal("dbo", o.Schema);
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys Targaryen", o.KeyValues[0, 0]);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[0, 1]);
                });
        }

        [Fact]
        public void UpdateDataOperation_required_args()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name" },
                    KeyValues = new object[,]
                        { { "Daenerys" } },
                    Columns = new[] { "House Allegiance" },
                    Values = new object[,]
                        { { "Targaryen" } }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"First Name\"," + _eol +
                "    keyValue: \"Daenerys\"," + _eol +
                "    column: \"House Allegiance\"," + _eol +
                "    value: \"Targaryen\");",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[0, 0]);
                    Assert.Equal(1, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(1, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[0, 0]);
                });
        }

        [Fact]
        public void UpdateDataOperation_required_args_multiple_rows()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name" },
                    KeyValues = new object[,]
                    {
                        { "Hodor" },
                        { "Daenerys" }
                    },
                    Columns = new[] { "House Allegiance" },
                    Values = new object[,]
                    {
                        { "Stark" },
                        { "Targaryen" }
                    }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"First Name\"," + _eol +
                "    keyValues: new object[]" + _eol +
                "    {" + _eol +
                "        \"Hodor\"," + _eol +
                "        \"Daenerys\"" + _eol +
                "    }," + _eol +
                "    column: \"House Allegiance\"," + _eol +
                "    values: new object[]" + _eol +
                "    {" + _eol +
                "        \"Stark\"," + _eol +
                "        \"Targaryen\"" + _eol +
                "    });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(2, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                    Assert.Equal(1, o.Columns.Length);
                    Assert.Equal(2, o.Values.GetLength(0));
                    Assert.Equal(1, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[1, 0]);
                });
        }

        [Fact]
        public void UpdateDataOperation_required_args_composite()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                        { { "Daenerys", "Targaryen" } },
                    Columns = new[] { "House Allegiance" },
                    Values = new object[,]
                        { { "Targaryen" } }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumns: new[] { \"First Name\", \"Last Name\" }," + _eol +
                "    keyValues: new object[] { \"Daenerys\", \"Targaryen\" }," + _eol +
                "    column: \"House Allegiance\"," + _eol +
                "    value: \"Targaryen\");",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(2, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(2, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[0, 0]);
                    Assert.Equal(1, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(1, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[0, 0]);
                });
        }

        [Fact]
        public void UpdateDataOperation_required_args_composite_multi()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "First Name", "Last Name" },
                    KeyValues = new object[,]
                        { { "Daenerys", "Targaryen" } },
                    Columns = new[] { "Birthplace", "House Allegiance", "Culture" },
                    Values = new object[,]
                        { { "Dragonstone", "Targaryen", "Valyrian" } }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumns: new[] { \"First Name\", \"Last Name\" }," + _eol +
                "    keyValues: new object[] { \"Daenerys\", \"Targaryen\" }," + _eol +
                "    columns: new[] { \"Birthplace\", \"House Allegiance\", \"Culture\" }," + _eol +
                "    values: new object[] { \"Dragonstone\", \"Targaryen\", \"Valyrian\" });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(2, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(2, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys", o.KeyValues[0, 0]);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[0, 1]);
                });
        }

        [Fact]
        public void UpdateDataOperation_required_args_multi()
        {
            Test(
                new UpdateDataOperation
                {
                    Table = "People",
                    KeyColumns = new[] { "Full Name" },
                    KeyValues = new object[,]
                        { { "Daenerys Targaryen" } },
                    Columns = new[] { "Birthplace", "House Allegiance", "Culture" },
                    Values = new object[,]
                        { { "Dragonstone", "Targaryen", "Valyrian" } }
                },
                "mb.UpdateData(" + _eol +
                "    table: \"People\"," + _eol +
                "    keyColumn: \"Full Name\"," + _eol +
                "    keyValue: \"Daenerys Targaryen\"," + _eol +
                "    columns: new[] { \"Birthplace\", \"House Allegiance\", \"Culture\" }," + _eol +
                "    values: new object[] { \"Dragonstone\", \"Targaryen\", \"Valyrian\" });",
                o =>
                {
                    Assert.Equal("People", o.Table);
                    Assert.Equal(1, o.KeyColumns.Length);
                    Assert.Equal(1, o.KeyValues.GetLength(0));
                    Assert.Equal(1, o.KeyValues.GetLength(1));
                    Assert.Equal("Daenerys Targaryen", o.KeyValues[0, 0]);
                    Assert.Equal(3, o.Columns.Length);
                    Assert.Equal(1, o.Values.GetLength(0));
                    Assert.Equal(3, o.Values.GetLength(1));
                    Assert.Equal("Targaryen", o.Values[0, 1]);
                });
        }

        private void Test<T>(T operation, string expectedCode, Action<T> assert)
            where T : MigrationOperation
        {
            var generator = new CSharpMigrationOperationGenerator(
                new CSharpMigrationOperationGeneratorDependencies(
                    new CSharpHelper(
                        new SqlServerTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            new RelationalTypeMappingSourceDependencies(
                                new IRelationalTypeMappingSourcePlugin[] { new SqlServerNetTopologySuiteTypeMappingSourcePlugin(NtsGeometryServices.Instance), })))));

            var builder = new IndentedStringBuilder();
            generator.Generate("mb", new[] { operation }, builder);
            var code = builder.ToString();

            Assert.Equal(expectedCode, code);

            var build = new BuildSource
            {
                References =
                {
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                    BuildReference.ByName("GeoAPI"),
                    BuildReference.ByName("NetTopologySuite"),
                    BuildReference.ByName("System.Runtime.Extensions"),
                },
                Sources =
                {
                    @"
                    using Microsoft.EntityFrameworkCore.Migrations;
                    using NetTopologySuite.Geometries;

                    public static class OperationsFactory
                    {
                        public static void Create(MigrationBuilder mb)
                        {
                            " + code + @"
                        }
                    }
                "
                }
            };

            var assembly = build.BuildInMemory();
            var factoryType = assembly.GetType("OperationsFactory");
            var createMethod = factoryType.GetTypeInfo().GetDeclaredMethod("Create");
            var mb = new MigrationBuilder(activeProvider: null);
            createMethod.Invoke(null, new[] { mb });
            var result = mb.Operations.Cast<T>().Single();

            assert(result);
        }
    }
}

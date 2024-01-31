// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using NetTopologySuite;
using NetTopologySuite.Geometries;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public class CSharpMigrationOperationGeneratorTest
{
    [ConditionalFact]
    public void Generate_separates_operations_by_a_blank_line()
    {
        var generator = new CSharpMigrationOperationGenerator(
            new CSharpMigrationOperationGeneratorDependencies(
                new CSharpHelper(
                    new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))));

        var builder = new IndentedStringBuilder();

        generator.Generate(
            "mb",
            new[] { new SqlOperation { Sql = "-- Don't stand so" }, new SqlOperation { Sql = "-- close to me" } },
            builder);

        Assert.Equal(
            """
mb.Sql("-- Don't stand so");

mb.Sql("-- close to me");
""",
            builder.ToString(), ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void AddColumnOperation_required_args()
        => Test(
            new AddColumnOperation
            {
                Name = "Id",
                Table = "Post",
                ClrType = typeof(int)
            },
            """
mb.AddColumn<int>(
    name: "Id",
    table: "Post",
    nullable: false);
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(typeof(int), o.ClrType);
            });

    [ConditionalFact]
    public void AddColumnOperation_all_args()
        => Test(
            new AddColumnOperation
            {
                Name = "Id",
                Schema = "dbo",
                Table = "Post",
                ClrType = typeof(int),
                ColumnType = "int",
                IsUnicode = false,
                MaxLength = 30,
                Precision = 10,
                Scale = 5,
                IsRowVersion = true,
                IsNullable = true,
                DefaultValue = 1,
                IsFixedLength = true,
                Comment = "My Comment",
                Collation = "Some Collation"
            },
            """
mb.AddColumn<int>(
    name: "Id",
    schema: "dbo",
    table: "Post",
    type: "int",
    unicode: false,
    fixedLength: true,
    maxLength: 30,
    precision: 10,
    scale: 5,
    rowVersion: true,
    nullable: true,
    defaultValue: 1,
    comment: "My Comment",
    collation: "Some Collation");
""",
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
                Assert.Equal("My Comment", o.Comment);
                Assert.Equal("Some Collation", o.Collation);
            });

    [ConditionalFact]
    public void AddColumnOperation_DefaultValueSql()
        => Test(
            new AddColumnOperation
            {
                Name = "Id",
                Table = "Post",
                ClrType = typeof(int),
                DefaultValueSql = "1"
            },
            """
mb.AddColumn<int>(
    name: "Id",
    table: "Post",
    nullable: false,
    defaultValueSql: "1");
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(typeof(int), o.ClrType);
                Assert.Equal("1", o.DefaultValueSql);
            });

    [ConditionalFact]
    public void AddColumnOperation_ComputedExpression()
        => Test(
            new AddColumnOperation
            {
                Name = "Id",
                Table = "Post",
                ClrType = typeof(int),
                ComputedColumnSql = "1",
                IsStored = true
            },
            """
mb.AddColumn<int>(
    name: "Id",
    table: "Post",
    nullable: false,
    computedColumnSql: "1",
    stored: true);
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(typeof(int), o.ClrType);
                Assert.Equal("1", o.ComputedColumnSql);
                Assert.True(o.IsStored);
            });

    [ConditionalFact]
    public void AddForeignKeyOperation_required_args()
        => Test(
            new AddForeignKeyOperation
            {
                Name = "FK_Post_Blog_BlogId",
                Table = "Post",
                Columns = ["BlogId"],
                PrincipalTable = "Blog"
            },
            """
mb.AddForeignKey(
    name: "FK_Post_Blog_BlogId",
    table: "Post",
    column: "BlogId",
    principalTable: "Blog");
""",
            o =>
            {
                Assert.Equal("FK_Post_Blog_BlogId", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "BlogId" }, o.Columns);
                Assert.Equal("Blog", o.PrincipalTable);
                Assert.Null(o.PrincipalColumns);
            });

    [ConditionalFact]
    public void AddForeignKeyOperation_required_args_composite()
        => Test(
            new AddForeignKeyOperation
            {
                Name = "FK_Post_Blog_BlogId1_BlogId2",
                Table = "Post",
                Columns = ["BlogId1", "BlogId2"],
                PrincipalTable = "Blog"
            },
            """
mb.AddForeignKey(
    name: "FK_Post_Blog_BlogId1_BlogId2",
    table: "Post",
    columns: new[] { "BlogId1", "BlogId2" },
    principalTable: "Blog");
""",
            o =>
            {
                Assert.Equal("FK_Post_Blog_BlogId1_BlogId2", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "BlogId1", "BlogId2" }, o.Columns);
                Assert.Equal("Blog", o.PrincipalTable);
                Assert.Null(o.PrincipalColumns);
            });

    [ConditionalFact]
    public void AddForeignKeyOperation_all_args()
        => Test(
            new AddForeignKeyOperation
            {
                Name = "FK_Post_Blog_BlogId",
                Schema = "dbo",
                Table = "Post",
                Columns = ["BlogId"],
                PrincipalSchema = "my",
                PrincipalTable = "Blog",
                PrincipalColumns = ["Id"],
                OnUpdate = ReferentialAction.Restrict,
                OnDelete = ReferentialAction.Cascade
            },
            """
mb.AddForeignKey(
    name: "FK_Post_Blog_BlogId",
    schema: "dbo",
    table: "Post",
    column: "BlogId",
    principalSchema: "my",
    principalTable: "Blog",
    principalColumn: "Id",
    onUpdate: ReferentialAction.Restrict,
    onDelete: ReferentialAction.Cascade);
""",
            o =>
            {
                Assert.Equal("FK_Post_Blog_BlogId", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "BlogId" }, o.Columns);
                Assert.Equal("my", o.PrincipalSchema);
                Assert.Equal("Blog", o.PrincipalTable);
                Assert.Equal(new[] { "Id" }, o.PrincipalColumns);
                Assert.Equal(ReferentialAction.Restrict, o.OnUpdate);
                Assert.Equal(ReferentialAction.Cascade, o.OnDelete);
            });

    [ConditionalFact]
    public void AddForeignKeyOperation_all_args_composite()
        => Test(
            new AddForeignKeyOperation
            {
                Name = "FK_Post_Blog_BlogId1_BlogId2",
                Schema = "dbo",
                Table = "Post",
                Columns = ["BlogId1", "BlogId2"],
                PrincipalSchema = "my",
                PrincipalTable = "Blog",
                PrincipalColumns = ["Id1", "Id2"],
                OnUpdate = ReferentialAction.Restrict,
                OnDelete = ReferentialAction.Cascade
            },
            """
mb.AddForeignKey(
    name: "FK_Post_Blog_BlogId1_BlogId2",
    schema: "dbo",
    table: "Post",
    columns: new[] { "BlogId1", "BlogId2" },
    principalSchema: "my",
    principalTable: "Blog",
    principalColumns: new[] { "Id1", "Id2" },
    onUpdate: ReferentialAction.Restrict,
    onDelete: ReferentialAction.Cascade);
""",
            o =>
            {
                Assert.Equal("FK_Post_Blog_BlogId1_BlogId2", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "BlogId1", "BlogId2" }, o.Columns);
                Assert.Equal("my", o.PrincipalSchema);
                Assert.Equal("Blog", o.PrincipalTable);
                Assert.Equal(new[] { "Id1", "Id2" }, o.PrincipalColumns);
                Assert.Equal(ReferentialAction.Restrict, o.OnUpdate);
                Assert.Equal(ReferentialAction.Cascade, o.OnDelete);
            });

    [ConditionalFact]
    public void AddPrimaryKey_required_args()
        => Test(
            new AddPrimaryKeyOperation
            {
                Name = "PK_Post",
                Table = "Post",
                Columns = ["Id"]
            },
            """
mb.AddPrimaryKey(
    name: "PK_Post",
    table: "Post",
    column: "Id");
""",
            o =>
            {
                Assert.Equal("PK_Post", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "Id" }, o.Columns);
            });

    [ConditionalFact]
    public void AddPrimaryKey_all_args()
        => Test(
            new AddPrimaryKeyOperation
            {
                Name = "PK_Post",
                Schema = "dbo",
                Table = "Post",
                Columns = ["Id"]
            },
            """
mb.AddPrimaryKey(
    name: "PK_Post",
    schema: "dbo",
    table: "Post",
    column: "Id");
""",
            o =>
            {
                Assert.Equal("PK_Post", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "Id" }, o.Columns);
            });

    [ConditionalFact]
    public void AddPrimaryKey_composite()
        => Test(
            new AddPrimaryKeyOperation
            {
                Name = "PK_Post",
                Table = "Post",
                Columns = ["Id1", "Id2"]
            },
            """
mb.AddPrimaryKey(
    name: "PK_Post",
    table: "Post",
    columns: new[] { "Id1", "Id2" });
""",
            o =>
            {
                Assert.Equal("PK_Post", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "Id1", "Id2" }, o.Columns);
            });

    [ConditionalFact]
    public void AddUniqueConstraint_required_args()
        => Test(
            new AddUniqueConstraintOperation
            {
                Name = "AK_Post_AltId",
                Table = "Post",
                Columns = ["AltId"]
            },
            """
mb.AddUniqueConstraint(
    name: "AK_Post_AltId",
    table: "Post",
    column: "AltId");
""",
            o =>
            {
                Assert.Equal("AK_Post_AltId", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "AltId" }, o.Columns);
            });

    [ConditionalFact]
    public void AddUniqueConstraint_all_args()
        => Test(
            new AddUniqueConstraintOperation
            {
                Name = "AK_Post_AltId",
                Schema = "dbo",
                Table = "Post",
                Columns = ["AltId"]
            },
            """
mb.AddUniqueConstraint(
    name: "AK_Post_AltId",
    schema: "dbo",
    table: "Post",
    column: "AltId");
""",
            o =>
            {
                Assert.Equal("AK_Post_AltId", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "AltId" }, o.Columns);
            });

    [ConditionalFact]
    public void AddUniqueConstraint_composite()
        => Test(
            new AddUniqueConstraintOperation
            {
                Name = "AK_Post_AltId1_AltId2",
                Table = "Post",
                Columns = ["AltId1", "AltId2"]
            },
            """
mb.AddUniqueConstraint(
    name: "AK_Post_AltId1_AltId2",
    table: "Post",
    columns: new[] { "AltId1", "AltId2" });
""",
            o =>
            {
                Assert.Equal("AK_Post_AltId1_AltId2", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "AltId1", "AltId2" }, o.Columns);
            });

    [ConditionalFact]
    public void AddCheckConstraint_required_args()
        => Test(
            new AddCheckConstraintOperation
            {
                Name = "CK_Post_AltId1_AltId2",
                Table = "Post",
                Sql = "AltId1 > AltId2"
            },
            """
mb.AddCheckConstraint(
    name: "CK_Post_AltId1_AltId2",
    table: "Post",
    sql: "AltId1 > AltId2");
""",
            o =>
            {
                Assert.Equal("CK_Post_AltId1_AltId2", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal("AltId1 > AltId2", o.Sql);
            });

    [ConditionalFact]
    public void AddCheckConstraint_all_args()
        => Test(
            new AddCheckConstraintOperation
            {
                Name = "CK_Post_AltId1_AltId2",
                Schema = "dbo",
                Table = "Post",
                Sql = "AltId1 > AltId2"
            },
            """
mb.AddCheckConstraint(
    name: "CK_Post_AltId1_AltId2",
    schema: "dbo",
    table: "Post",
    sql: "AltId1 > AltId2");
""",
            o =>
            {
                Assert.Equal("CK_Post_AltId1_AltId2", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal("AltId1 > AltId2", o.Sql);
            });

    [ConditionalFact]
    public void AlterColumnOperation_required_args()
        => Test(
            new AlterColumnOperation
            {
                Name = "Id",
                Table = "Post",
                ClrType = typeof(int)
            },
            """
mb.AlterColumn<int>(
    name: "Id",
    table: "Post",
    nullable: false);
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(typeof(int), o.ClrType);
                Assert.Null(o.ColumnType);
                Assert.Null(o.IsUnicode);
                Assert.Null(o.IsFixedLength);
                Assert.Null(o.MaxLength);
                Assert.Null(o.Precision);
                Assert.Null(o.Scale);
                Assert.False(o.IsRowVersion);
                Assert.False(o.IsNullable);
                Assert.Null(o.DefaultValue);
                Assert.Null(o.DefaultValueSql);
                Assert.Null(o.ComputedColumnSql);
                Assert.Null(o.Comment);
                Assert.Null(o.Collation);
                Assert.Equal(typeof(int), o.OldColumn.ClrType);
                Assert.Null(o.OldColumn.ColumnType);
                Assert.Null(o.OldColumn.IsUnicode);
                Assert.Null(o.OldColumn.IsFixedLength);
                Assert.Null(o.OldColumn.MaxLength);
                Assert.Null(o.OldColumn.Precision);
                Assert.Null(o.OldColumn.Scale);
                Assert.False(o.OldColumn.IsRowVersion);
                Assert.False(o.OldColumn.IsNullable);
                Assert.Null(o.OldColumn.DefaultValue);
                Assert.Null(o.OldColumn.DefaultValueSql);
                Assert.Null(o.OldColumn.ComputedColumnSql);
                Assert.Null(o.OldColumn.Comment);
                Assert.Null(o.OldColumn.Collation);
            });

    [ConditionalFact]
    public void AlterColumnOperation_all_args()
        => Test(
            new AlterColumnOperation
            {
                Name = "Id",
                Schema = "dbo",
                Table = "Post",
                ClrType = typeof(int),
                ColumnType = "int",
                IsUnicode = false,
                MaxLength = 30,
                Precision = 10,
                Scale = 5,
                IsRowVersion = true,
                IsNullable = true,
                DefaultValue = 1,
                IsFixedLength = true,
                Comment = "My Comment 2",
                Collation = "Some Collation 2",
                OldColumn =
                {
                    ClrType = typeof(string),
                    ColumnType = "string",
                    IsUnicode = false,
                    MaxLength = 20,
                    Precision = 5,
                    Scale = 1,
                    IsRowVersion = true,
                    IsNullable = true,
                    DefaultValue = 0,
                    IsFixedLength = true,
                    Comment = "My Comment",
                    Collation = "Some Collation"
                }
            },
            """
mb.AlterColumn<int>(
    name: "Id",
    schema: "dbo",
    table: "Post",
    type: "int",
    unicode: false,
    fixedLength: true,
    maxLength: 30,
    precision: 10,
    scale: 5,
    rowVersion: true,
    nullable: true,
    defaultValue: 1,
    comment: "My Comment 2",
    collation: "Some Collation 2",
    oldClrType: typeof(string),
    oldType: "string",
    oldUnicode: false,
    oldFixedLength: true,
    oldMaxLength: 20,
    oldPrecision: 5,
    oldScale: 1,
    oldRowVersion: true,
    oldNullable: true,
    oldDefaultValue: 0,
    oldComment: "My Comment",
    oldCollation: "Some Collation");
""",
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
                Assert.Equal(10, o.Precision);
                Assert.Equal(5, o.Scale);
                Assert.True(o.IsRowVersion);
                Assert.True(o.IsNullable);
                Assert.Equal(1, o.DefaultValue);
                Assert.Null(o.DefaultValueSql);
                Assert.Null(o.ComputedColumnSql);
                Assert.Equal("My Comment 2", o.Comment);
                Assert.Equal("Some Collation 2", o.Collation);
                Assert.Equal(typeof(string), o.OldColumn.ClrType);
                Assert.Equal("string", o.OldColumn.ColumnType);
                Assert.False(o.OldColumn.IsUnicode);
                Assert.True(o.OldColumn.IsFixedLength);
                Assert.Equal(20, o.OldColumn.MaxLength);
                Assert.Equal(5, o.OldColumn.Precision);
                Assert.Equal(1, o.OldColumn.Scale);
                Assert.True(o.OldColumn.IsRowVersion);
                Assert.True(o.OldColumn.IsNullable);
                Assert.Equal(0, o.OldColumn.DefaultValue);
                Assert.Null(o.OldColumn.DefaultValueSql);
                Assert.Null(o.OldColumn.ComputedColumnSql);
                Assert.Equal("My Comment", o.OldColumn.Comment);
                Assert.Equal("Some Collation", o.OldColumn.Collation);
            });

    [ConditionalFact]
    public void AlterColumnOperation_DefaultValueSql()
        => Test(
            new AlterColumnOperation
            {
                Name = "Id",
                Table = "Post",
                ClrType = typeof(int),
                DefaultValueSql = "1"
            },
            """
mb.AlterColumn<int>(
    name: "Id",
    table: "Post",
    nullable: false,
    defaultValueSql: "1");
""",
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

    [ConditionalFact]
    public void AlterColumnOperation_computedColumnSql()
        => Test(
            new AlterColumnOperation
            {
                Name = "Id",
                Table = "Post",
                ClrType = typeof(int),
                ComputedColumnSql = "1",
                IsStored = true
            },
            """
mb.AlterColumn<int>(
    name: "Id",
    table: "Post",
    nullable: false,
    computedColumnSql: "1",
    stored: true);
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal("1", o.ComputedColumnSql);
                Assert.True(o.IsStored);
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
                Assert.Null(o.OldColumn.Precision);
                Assert.Null(o.OldColumn.Scale);
                Assert.False(o.OldColumn.IsRowVersion);
                Assert.False(o.OldColumn.IsNullable);
                Assert.Null(o.OldColumn.DefaultValue);
                Assert.Null(o.OldColumn.DefaultValueSql);
                Assert.Null(o.OldColumn.ComputedColumnSql);
                Assert.Null(o.OldColumn.IsStored);
            });

    [ConditionalFact]
    public void AlterDatabaseOperation()
        => Test(
            new AlterDatabaseOperation
            {
                Collation = "Some collation",
                ["foo"] = "bar",
                OldDatabase = { Collation = "Some other collation", ["bar"] = "foo" }
            },
            """
mb.AlterDatabase(
    collation: "Some collation",
    oldCollation: "Some other collation")
    .Annotation("foo", "bar")
    .OldAnnotation("bar", "foo");
""",
            o =>
            {
                Assert.Equal("Some collation", o.Collation);
                Assert.Equal("Some other collation", o.OldDatabase.Collation);
                Assert.Equal("bar", o["foo"]);
                Assert.Equal("foo", o.OldDatabase["bar"]);
            });

    [ConditionalFact]
    public void AlterDatabaseOperation_with_default_old_collation()
        => Test(
            new AlterDatabaseOperation { Collation = "Some collation" },
            """
mb.AlterDatabase(
    collation: "Some collation");
""",
            o =>
            {
                Assert.Equal("Some collation", o.Collation);
                Assert.Null(o.OldDatabase.Collation);
            });

    [ConditionalFact]
    public void AlterDatabaseOperation_with_default_new_collation()
        => Test(
            new AlterDatabaseOperation { OldDatabase = { Collation = "Some collation" } },
            """
mb.AlterDatabase(
    oldCollation: "Some collation");
""",
            o =>
            {
                Assert.Null(o.Collation);
                Assert.Equal("Some collation", o.OldDatabase.Collation);
            });

    [ConditionalFact]
    public void AlterSequenceOperation_required_args()
        => Test(
            new AlterSequenceOperation { Name = "EntityFrameworkHiLoSequence" },
            """
mb.AlterSequence(
    name: "EntityFrameworkHiLoSequence");
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Null(o.Schema);
                Assert.Equal(1, o.IncrementBy);
                Assert.Null(o.MinValue);
                Assert.Null(o.MaxValue);
                Assert.False(o.IsCyclic);
                Assert.True(o.IsCached);
                Assert.Null(o.CacheSize);
                Assert.Equal(1, o.OldSequence.IncrementBy);
                Assert.Null(o.OldSequence.MinValue);
                Assert.Null(o.OldSequence.MaxValue);
                Assert.False(o.OldSequence.IsCyclic);
                Assert.True(o.OldSequence.IsCached);
                Assert.Null(o.OldSequence.CacheSize);
            });

    [ConditionalFact]
    public void AlterSequenceOperation_all_args()
        => Test(
            new AlterSequenceOperation
            {
                Name = "EntityFrameworkHiLoSequence",
                Schema = "dbo",
                IncrementBy = 3,
                MinValue = 2,
                MaxValue = 4,
                IsCyclic = true,
                IsCached = true,
                CacheSize = 20,
                OldSequence =
                {
                    IncrementBy = 4,
                    MinValue = 3,
                    MaxValue = 5,
                    IsCyclic = true,
                    IsCached = true,
                    CacheSize = 2
                }
            },
            """
mb.AlterSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    incrementBy: 3,
    minValue: 2L,
    maxValue: 4L,
    cyclic: true,
    cached: true,
    cacheSize: 20,
    oldIncrementBy: 4,
    oldMinValue: 3L,
    oldMaxValue: 5L,
    oldCyclic: true,
    oldCached: true,
    oldCacheSize: 2);
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal(3, o.IncrementBy);
                Assert.Equal(2, o.MinValue);
                Assert.Equal(4, o.MaxValue);
                Assert.True(o.IsCyclic);
                Assert.True(o.IsCached);
                Assert.Equal(20, o.CacheSize);
                Assert.Equal(4, o.OldSequence.IncrementBy);
                Assert.Equal(3, o.OldSequence.MinValue);
                Assert.Equal(5, o.OldSequence.MaxValue);
                Assert.True(o.OldSequence.IsCyclic);
                Assert.True(o.OldSequence.IsCached);
                Assert.Equal(2, o.OldSequence.CacheSize);
            });

    [ConditionalFact]
    public void AlterTableOperation_required_args()
        => Test(
            new AlterTableOperation { Name = "Customer" },
            """
mb.AlterTable(
    name: "Customer");
""",
            o =>
            {
                Assert.Equal("Customer", o.Name);
            });

    [ConditionalFact]
    public void AlterTableOperation_all_args()
        => Test(
            new AlterTableOperation
            {
                Name = "Customer",
                Schema = "dbo",
                Comment = "My Comment 2",
                OldTable = { Comment = "My Comment" }
            },
            """
mb.AlterTable(
    name: "Customer",
    schema: "dbo",
    comment: "My Comment 2",
    oldComment: "My Comment");
""",
            o =>
            {
                Assert.Equal("Customer", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("My Comment 2", o.Comment);
                Assert.Equal("My Comment", o.OldTable.Comment);
            });

    [ConditionalFact]
    public void CreateIndexOperation_required_args()
        => Test(
            new CreateIndexOperation
            {
                Name = "IX_Post_Title",
                Table = "Post",
                Columns = ["Title"]
            },
            """
mb.CreateIndex(
    name: "IX_Post_Title",
    table: "Post",
    column: "Title");
""",
            o =>
            {
                Assert.Equal("IX_Post_Title", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "Title" }, o.Columns);
                Assert.False(o.IsUnique);
                Assert.Null(o.IsDescending);
                Assert.Null(o.Filter);
            });

    [ConditionalFact]
    public void CreateIndexOperation_all_args()
        => Test(
            new CreateIndexOperation
            {
                Name = "IX_Post_Title",
                Schema = "dbo",
                Table = "Post",
                Columns = ["Title", "Name"],
                IsUnique = true,
                IsDescending = [true, false],
                Filter = "[Title] IS NOT NULL"
            },
            """
mb.CreateIndex(
    name: "IX_Post_Title",
    schema: "dbo",
    table: "Post",
    columns: new[] { "Title", "Name" },
    unique: true,
    descending: new[] { true, false },
    filter: "[Title] IS NOT NULL");
""",
            o =>
            {
                Assert.Equal("IX_Post_Title", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "Title", "Name" }, o.Columns);
                Assert.True(o.IsUnique);
                Assert.Equal(new[] { true, false }, o.IsDescending);
                Assert.Equal("[Title] IS NOT NULL", o.Filter);
            });

    [ConditionalFact]
    public void CreateIndexOperation_composite()
        => Test(
            new CreateIndexOperation
            {
                Name = "IX_Post_Title_Subtitle",
                Table = "Post",
                Columns = ["Title", "Subtitle"]
            },
            """
mb.CreateIndex(
    name: "IX_Post_Title_Subtitle",
    table: "Post",
    columns: new[] { "Title", "Subtitle" });
""",
            o =>
            {
                Assert.Equal("IX_Post_Title_Subtitle", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal(new[] { "Title", "Subtitle" }, o.Columns);
            });

    [ConditionalFact]
    public void CreateSchemaOperation_required_args()
        => Test(
            new EnsureSchemaOperation { Name = "my" },
            """
mb.EnsureSchema(
    name: "my");
""",
            o => Assert.Equal("my", o.Name));

    [ConditionalFact]
    public void CreateSequenceOperation_required_args()
        => Test(
            new CreateSequenceOperation { Name = "EntityFrameworkHiLoSequence", ClrType = typeof(long) },
            """
mb.CreateSequence(
    name: "EntityFrameworkHiLoSequence");
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal(typeof(long), o.ClrType);
            });

    [ConditionalFact]
    public void CreateSequenceOperation_required_args_not_long()
        => Test(
            new CreateSequenceOperation { Name = "EntityFrameworkHiLoSequence", ClrType = typeof(int) },
            """
mb.CreateSequence<int>(
    name: "EntityFrameworkHiLoSequence");
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal(typeof(int), o.ClrType);
            });

    [ConditionalFact]
    public void CreateSequenceOperation_all_args()
        => Test(
            new CreateSequenceOperation
            {
                Name = "EntityFrameworkHiLoSequence",
                Schema = "dbo",
                ClrType = typeof(long),
                StartValue = 3,
                IncrementBy = 5,
                MinValue = 2,
                MaxValue = 4,
                IsCyclic = true,
                IsCached = true,
                CacheSize = 20
            },
            """
mb.CreateSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    startValue: 3L,
    incrementBy: 5,
    minValue: 2L,
    maxValue: 4L,
    cyclic: true,
    cached: true,
    cacheSize: 20);
""",
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
                Assert.True(o.IsCached);
                Assert.Equal(20, o.CacheSize);
            });

    [ConditionalFact]
    public void CreateSequenceOperationCache()
    => Test(
        new CreateSequenceOperation
        {
            Name = "EntityFrameworkHiLoSequence",
            Schema = "dbo",
            ClrType = typeof(long),
            IsCached = true,
            CacheSize = 20
        },
        """
mb.CreateSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    cached: true,
    cacheSize: 20);
""",
        o =>
        {
            Assert.True(o.IsCached);
            Assert.Equal(20, o.CacheSize);
        });

    [ConditionalFact]
    public void CreateSequenceOperationNoCache()
    => Test(
        new CreateSequenceOperation
        {
            Name = "EntityFrameworkHiLoSequence",
            Schema = "dbo",
            ClrType = typeof(long),
            IsCached = false,
        },
        """
mb.CreateSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    cached: false);
""",
        o =>
        {
            Assert.False(o.IsCached);
            Assert.Null(o.CacheSize);
        });

    [ConditionalFact]
    public void CreateSequenceOperationDefaultCache()
    => Test(
        new CreateSequenceOperation
        {
            Name = "EntityFrameworkHiLoSequence",
            Schema = "dbo",
            ClrType = typeof(long),
            IsCached = true
        },
        """
mb.CreateSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo");
""",
        o =>
        {
            Assert.True(o.IsCached);
            Assert.Null(o.CacheSize);
        });


    [ConditionalFact]
    public void CreateSequenceOperation_all_args_not_long()
        => Test(
            new CreateSequenceOperation
            {
                Name = "EntityFrameworkHiLoSequence",
                Schema = "dbo",
                ClrType = typeof(int),
                StartValue = 3,
                IncrementBy = 5,
                MinValue = 2,
                MaxValue = 4,
                IsCyclic = true,
                IsCached = true,
                CacheSize = 20
            },
            """
mb.CreateSequence<int>(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    startValue: 3L,
    incrementBy: 5,
    minValue: 2L,
    maxValue: 4L,
    cyclic: true,
    cached: true,
    cacheSize: 20);
""",
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
                Assert.True(o.IsCached);
                Assert.Equal(20, o.CacheSize);
            });

    [ConditionalFact]
    public void CreateTableOperation_Columns_required_args()
        => Test(
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
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
    });
""",
            o =>
            {
                Assert.Equal("Post", o.Name);
                Assert.Single(o.Columns);

                Assert.Equal("Id", o.Columns[0].Name);
                Assert.Equal("Post", o.Columns[0].Table);
                Assert.Equal(typeof(int), o.Columns[0].ClrType);
            });

    [ConditionalFact]
    public void CreateTableOperation_Columns_all_args()
        => Test(
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
                        Precision = 20,
                        Scale = 10,
                        IsRowVersion = true,
                        IsNullable = true,
                        DefaultValue = 1,
                        Comment = "My Comment",
                        Collation = "Some Collation"
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        PostId = table.Column<int>(name: "Post Id", type: "int", unicode: false, fixedLength: true, maxLength: 30, precision: 20, scale: 10, rowVersion: true, nullable: true, defaultValue: 1, comment: "My Comment", collation: "Some Collation")
    },
    constraints: table =>
    {
    });
""",
            o =>
            {
                Assert.Equal("Post", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Single(o.Columns);

                Assert.Equal("Post Id", o.Columns[0].Name);
                Assert.Equal("dbo", o.Columns[0].Schema);
                Assert.Equal("Post", o.Columns[0].Table);
                Assert.Equal(typeof(int), o.Columns[0].ClrType);
                Assert.Equal("int", o.Columns[0].ColumnType);
                Assert.True(o.Columns[0].IsNullable);
                Assert.False(o.Columns[0].IsUnicode);
                Assert.True(o.Columns[0].IsFixedLength);
                Assert.Equal(1, o.Columns[0].DefaultValue);
                Assert.Equal("My Comment", o.Columns[0].Comment);
                Assert.Equal("Some Collation", o.Columns[0].Collation);
            });

    [ConditionalFact]
    public void CreateTableOperation_Columns_DefaultValueSql()
        => Test(
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
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false, defaultValueSql: "1")
    },
    constraints: table =>
    {
    });
""",
            o =>
            {
                Assert.Single(o.Columns);

                Assert.Equal("Id", o.Columns[0].Name);
                Assert.Equal("Post", o.Columns[0].Table);
                Assert.Equal(typeof(int), o.Columns[0].ClrType);
                Assert.Equal("1", o.Columns[0].DefaultValueSql);
            });

    [ConditionalFact]
    public void CreateTableOperation_Columns_computedColumnSql()
        => Test(
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
                        ComputedColumnSql = "1",
                        IsStored = true
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false, computedColumnSql: "1", stored: true)
    },
    constraints: table =>
    {
    });
""",
            o =>
            {
                Assert.Single(o.Columns);

                Assert.Equal("Id", o.Columns[0].Name);
                Assert.Equal("Post", o.Columns[0].Table);
                Assert.Equal(typeof(int), o.Columns[0].ClrType);
                Assert.Equal("1", o.Columns[0].ComputedColumnSql);
                Assert.True(o.Columns[0].IsStored);
            });

    [ConditionalFact]
    public void CreateTableOperation_ForeignKeys_required_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns = { new AddColumnOperation { Name = "BlogId", ClrType = typeof(int) } },
                ForeignKeys =
                {
                    new AddForeignKeyOperation
                    {
                        Name = "FK_Post_Blog_BlogId",
                        Table = "Post",
                        Columns = ["BlogId"],
                        PrincipalTable = "Blog"
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        BlogId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.ForeignKey(
            name: "FK_Post_Blog_BlogId",
            column: x => x.BlogId,
            principalTable: "Blog");
    });
""",
            o =>
            {
                Assert.Single(o.ForeignKeys);

                var fk = o.ForeignKeys.First();
                Assert.Equal("FK_Post_Blog_BlogId", fk.Name);
                Assert.Equal("Post", fk.Table);
                Assert.Equal(new[] { "BlogId" }, fk.Columns.ToArray());
                Assert.Equal("Blog", fk.PrincipalTable);
            });

    [ConditionalFact]
    public void CreateTableOperation_ForeignKeys_all_args()
        => Test(
            new CreateTableOperation
            {
                Schema = "dbo",
                Name = "Post",
                Columns = { new AddColumnOperation { Name = "BlogId", ClrType = typeof(int) } },
                ForeignKeys =
                {
                    new AddForeignKeyOperation
                    {
                        Schema = "dbo",
                        Table = "Post",
                        Name = "FK_Post_Blog_BlogId",
                        Columns = ["BlogId"],
                        PrincipalTable = "Blog",
                        PrincipalSchema = "my",
                        PrincipalColumns = ["Id"],
                        OnUpdate = ReferentialAction.SetNull,
                        OnDelete = ReferentialAction.SetDefault
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        BlogId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.ForeignKey(
            name: "FK_Post_Blog_BlogId",
            column: x => x.BlogId,
            principalSchema: "my",
            principalTable: "Blog",
            principalColumn: "Id",
            onUpdate: ReferentialAction.SetNull,
            onDelete: ReferentialAction.SetDefault);
    });
""",
            o =>
            {
                Assert.Single(o.ForeignKeys);

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

    [ConditionalFact]
    public void CreateTableOperation_ForeignKeys_composite()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns =
                {
                    new AddColumnOperation { Name = "BlogId1", ClrType = typeof(int) },
                    new AddColumnOperation { Name = "BlogId2", ClrType = typeof(int) }
                },
                ForeignKeys =
                {
                    new AddForeignKeyOperation
                    {
                        Name = "FK_Post_Blog_BlogId1_BlogId2",
                        Table = "Post",
                        Columns = ["BlogId1", "BlogId2"],
                        PrincipalTable = "Blog",
                        PrincipalColumns = ["Id1", "Id2"]
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        BlogId1 = table.Column<int>(nullable: false),
        BlogId2 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.ForeignKey(
            name: "FK_Post_Blog_BlogId1_BlogId2",
            columns: x => new { x.BlogId1, x.BlogId2 },
            principalTable: "Blog",
            principalColumns: new[] { "Id1", "Id2" });
    });
""",
            o =>
            {
                Assert.Single(o.ForeignKeys);

                var fk = o.ForeignKeys.First();
                Assert.Equal("Post", fk.Table);
                Assert.Equal(new[] { "BlogId1", "BlogId2" }, fk.Columns.ToArray());
                Assert.Equal("Blog", fk.PrincipalTable);
                Assert.Equal(new[] { "Id1", "Id2" }, fk.PrincipalColumns);
            });

    [ConditionalFact]
    public void CreateTableOperation_ForeignKeys_composite_no_principal_columns()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns =
                {
                    new AddColumnOperation { Name = "BlogId1", ClrType = typeof(int) },
                    new AddColumnOperation { Name = "BlogId2", ClrType = typeof(int) }
                },
                ForeignKeys =
                {
                    new AddForeignKeyOperation
                    {
                        Name = "FK_Post_Blog_BlogId1_BlogId2",
                        Table = "Post",
                        Columns = ["BlogId1", "BlogId2"],
                        PrincipalTable = "Blog"
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        BlogId1 = table.Column<int>(nullable: false),
        BlogId2 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.ForeignKey(
            name: "FK_Post_Blog_BlogId1_BlogId2",
            column: x => new { x.BlogId1, x.BlogId2 },
            principalTable: "Blog");
    });
""",
            o =>
            {
                Assert.Single(o.ForeignKeys);

                var fk = o.ForeignKeys.First();
                Assert.Equal("Post", fk.Table);
                Assert.Equal(new[] { "BlogId1", "BlogId2" }, fk.Columns.ToArray());
                Assert.Equal("Blog", fk.PrincipalTable);
            });

    [ConditionalFact]
    public void CreateTableOperation_PrimaryKey_required_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns = { new AddColumnOperation { Name = "Id", ClrType = typeof(int) } },
                PrimaryKey = new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post",
                    Columns = ["Id"]
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Post", x => x.Id);
    });
""",
            o =>
            {
                Assert.NotNull(o.PrimaryKey);

                Assert.Equal("PK_Post", o.PrimaryKey.Name);
                Assert.Equal("Post", o.PrimaryKey.Table);
                Assert.Equal(new[] { "Id" }, o.PrimaryKey.Columns);
            });

    [ConditionalFact]
    public void CreateTableOperation_PrimaryKey_all_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Schema = "dbo",
                Columns = { new AddColumnOperation { Name = "Id", ClrType = typeof(int) } },
                PrimaryKey = new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Schema = "dbo",
                    Table = "Post",
                    Columns = ["Id"]
                }
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Post", x => x.Id);
    });
""",
            o =>
            {
                Assert.NotNull(o.PrimaryKey);

                Assert.Equal("PK_Post", o.PrimaryKey.Name);
                Assert.Equal("dbo", o.PrimaryKey.Schema);
                Assert.Equal("Post", o.PrimaryKey.Table);
                Assert.Equal(new[] { "Id" }, o.PrimaryKey.Columns);
            });

    [ConditionalFact]
    public void CreateTableOperation_PrimaryKey_composite()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns =
                {
                    new AddColumnOperation { Name = "Id1", ClrType = typeof(int) },
                    new AddColumnOperation { Name = "Id2", ClrType = typeof(int) }
                },
                PrimaryKey = new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post",
                    Columns = ["Id1", "Id2"]
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        Id1 = table.Column<int>(nullable: false),
        Id2 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Post", x => new { x.Id1, x.Id2 });
    });
""",
            o =>
            {
                Assert.NotNull(o.PrimaryKey);

                Assert.Equal("PK_Post", o.PrimaryKey.Name);
                Assert.Equal("Post", o.PrimaryKey.Table);
                Assert.Equal(new[] { "Id1", "Id2" }, o.PrimaryKey.Columns);
            });

    [ConditionalFact]
    public void CreateTableOperation_UniqueConstraints_required_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns = { new AddColumnOperation { Name = "AltId", ClrType = typeof(int) } },
                UniqueConstraints =
                {
                    new AddUniqueConstraintOperation
                    {
                        Name = "AK_Post_AltId",
                        Table = "Post",
                        Columns = ["AltId"]
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        AltId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.UniqueConstraint("AK_Post_AltId", x => x.AltId);
    });
""",
            o =>
            {
                Assert.Single(o.UniqueConstraints);

                Assert.Equal("AK_Post_AltId", o.UniqueConstraints[0].Name);
                Assert.Equal("Post", o.UniqueConstraints[0].Table);
                Assert.Equal(new[] { "AltId" }, o.UniqueConstraints[0].Columns);
            });

    [ConditionalFact]
    public void CreateTableOperation_UniqueConstraints_all_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Schema = "dbo",
                Columns = { new AddColumnOperation { Name = "AltId", ClrType = typeof(int) } },
                UniqueConstraints =
                {
                    new AddUniqueConstraintOperation
                    {
                        Name = "AK_Post_AltId",
                        Schema = "dbo",
                        Table = "Post",
                        Columns = ["AltId"]
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        AltId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.UniqueConstraint("AK_Post_AltId", x => x.AltId);
    });
""",
            o =>
            {
                Assert.Single(o.UniqueConstraints);

                Assert.Equal("AK_Post_AltId", o.UniqueConstraints[0].Name);
                Assert.Equal("dbo", o.UniqueConstraints[0].Schema);
                Assert.Equal("Post", o.UniqueConstraints[0].Table);
                Assert.Equal(new[] { "AltId" }, o.UniqueConstraints[0].Columns);
            });

    [ConditionalFact]
    public void CreateTableOperation_UniqueConstraints_composite()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns =
                {
                    new AddColumnOperation { Name = "AltId1", ClrType = typeof(int) },
                    new AddColumnOperation { Name = "AltId2", ClrType = typeof(int) }
                },
                UniqueConstraints =
                {
                    new AddUniqueConstraintOperation
                    {
                        Name = "AK_Post_AltId1_AltId2",
                        Table = "Post",
                        Columns = ["AltId1", "AltId2"]
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        AltId1 = table.Column<int>(nullable: false),
        AltId2 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.UniqueConstraint("AK_Post_AltId1_AltId2", x => new { x.AltId1, x.AltId2 });
    });
""",
            o =>
            {
                Assert.Single(o.UniqueConstraints);

                Assert.Equal("AK_Post_AltId1_AltId2", o.UniqueConstraints[0].Name);
                Assert.Equal("Post", o.UniqueConstraints[0].Table);
                Assert.Equal(new[] { "AltId1", "AltId2" }, o.UniqueConstraints[0].Columns);
            });

    [ConditionalFact]
    public void CreateTableOperation_CheckConstraints_required_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Columns =
                {
                    new AddColumnOperation { Name = "AltId1", ClrType = typeof(int) },
                    new AddColumnOperation { Name = "AltId2", ClrType = typeof(int) }
                },
                CheckConstraints =
                {
                    new AddCheckConstraintOperation
                    {
                        Name = "CK_Post_AltId1_AltId2",
                        Table = "Post",
                        Sql = "AltId1 > AltId2"
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    columns: table => new
    {
        AltId1 = table.Column<int>(nullable: false),
        AltId2 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.CheckConstraint("CK_Post_AltId1_AltId2", "AltId1 > AltId2");
    });
""",
            o =>
            {
                Assert.Single(o.CheckConstraints);

                Assert.Equal("CK_Post_AltId1_AltId2", o.CheckConstraints[0].Name);
                Assert.Equal("Post", o.CheckConstraints[0].Table);
                Assert.Equal("AltId1 > AltId2", o.CheckConstraints[0].Sql);
            });

    [ConditionalFact]
    public void CreateTableOperation_ChecksConstraints_all_args()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Schema = "dbo",
                Columns =
                {
                    new AddColumnOperation { Name = "AltId1", ClrType = typeof(int) },
                    new AddColumnOperation { Name = "AltId2", ClrType = typeof(int) }
                },
                CheckConstraints =
                {
                    new AddCheckConstraintOperation
                    {
                        Name = "CK_Post_AltId1_AltId2",
                        Schema = "dbo",
                        Table = "Post",
                        Sql = "AltId1 > AltId2"
                    }
                }
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        AltId1 = table.Column<int>(nullable: false),
        AltId2 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.CheckConstraint("CK_Post_AltId1_AltId2", "AltId1 > AltId2");
    });
""",
            o =>
            {
                Assert.Single(o.CheckConstraints);

                Assert.Equal("CK_Post_AltId1_AltId2", o.CheckConstraints[0].Name);
                Assert.Equal("dbo", o.CheckConstraints[0].Schema);
                Assert.Equal("Post", o.CheckConstraints[0].Table);
                Assert.Equal("AltId1 > AltId2", o.CheckConstraints[0].Sql);
            });

    [ConditionalFact]
    public void CreateTableOperation_Comment()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Schema = "dbo",
                Columns = { new AddColumnOperation { Name = "AltId1", ClrType = typeof(int) } },
                Comment = "My Comment"
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        AltId1 = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
    },
    comment: "My Comment");
""",
            o =>
            {
                Assert.Equal("My Comment", o.Comment);
            });

    [ConditionalFact]
    public void CreateTableOperation_TableComment_ColumnComment()
        => Test(
            new CreateTableOperation
            {
                Name = "Post",
                Schema = "dbo",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "AltId1",
                        ClrType = typeof(int),
                        Comment = "My Column comment"
                    }
                },
                Comment = "My Operation Comment"
            },
            """
mb.CreateTable(
    name: "Post",
    schema: "dbo",
    columns: table => new
    {
        AltId1 = table.Column<int>(nullable: false, comment: "My Column comment")
    },
    constraints: table =>
    {
    },
    comment: "My Operation Comment");
""",
            o =>
            {
                Assert.Equal("My Operation Comment", o.Comment);
                Assert.Equal("My Column comment", o.Columns[0].Comment);
            });

    [ConditionalFact]
    public void DropColumnOperation_required_args()
        => Test(
            new DropColumnOperation { Name = "Id", Table = "Post" },
            """
mb.DropColumn(
    name: "Id",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropColumnOperation_all_args()
        => Test(
            new DropColumnOperation
            {
                Name = "Id",
                Schema = "dbo",
                Table = "Post"
            },
            """
mb.DropColumn(
    name: "Id",
    schema: "dbo",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropForeignKeyOperation_required_args()
        => Test(
            new DropForeignKeyOperation { Name = "FK_Post_BlogId", Table = "Post" },
            """
mb.DropForeignKey(
    name: "FK_Post_BlogId",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("FK_Post_BlogId", o.Name);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropForeignKeyOperation_all_args()
        => Test(
            new DropForeignKeyOperation
            {
                Name = "FK_Post_BlogId",
                Schema = "dbo",
                Table = "Post"
            },
            """
mb.DropForeignKey(
    name: "FK_Post_BlogId",
    schema: "dbo",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("FK_Post_BlogId", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropIndexOperation_required_args()
        => Test(
            new DropIndexOperation { Name = "IX_Post_Title" },
            """
mb.DropIndex(
    name: "IX_Post_Title");
""",
            o =>
            {
                Assert.Equal("IX_Post_Title", o.Name);
            });

    [ConditionalFact]
    public void DropIndexOperation_all_args()
        => Test(
            new DropIndexOperation
            {
                Name = "IX_Post_Title",
                Schema = "dbo",
                Table = "Post"
            },
            """
mb.DropIndex(
    name: "IX_Post_Title",
    schema: "dbo",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("IX_Post_Title", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropPrimaryKeyOperation_required_args()
        => Test(
            new DropPrimaryKeyOperation { Name = "PK_Post", Table = "Post" },
            """
mb.DropPrimaryKey(
    name: "PK_Post",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("PK_Post", o.Name);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropPrimaryKeyOperation_all_args()
        => Test(
            new DropPrimaryKeyOperation
            {
                Name = "PK_Post",
                Schema = "dbo",
                Table = "Post"
            },
            """
mb.DropPrimaryKey(
    name: "PK_Post",
    schema: "dbo",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("PK_Post", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropSchemaOperation_required_args()
        => Test(
            new DropSchemaOperation { Name = "my" },
            """
mb.DropSchema(
    name: "my");
""",
            o => Assert.Equal("my", o.Name));

    [ConditionalFact]
    public void DropSequenceOperation_required_args()
        => Test(
            new DropSequenceOperation { Name = "EntityFrameworkHiLoSequence" },
            """
mb.DropSequence(
    name: "EntityFrameworkHiLoSequence");
""",
            o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));

    [ConditionalFact]
    public void DropSequenceOperation_all_args()
        => Test(
            new DropSequenceOperation { Name = "EntityFrameworkHiLoSequence", Schema = "dbo" },
            """
mb.DropSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo");
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal("dbo", o.Schema);
            });

    [ConditionalFact]
    public void DropTableOperation_required_args()
        => Test(
            new DropTableOperation { Name = "Post" },
            """
mb.DropTable(
    name: "Post");
""",
            o => Assert.Equal("Post", o.Name));

    [ConditionalFact]
    public void DropTableOperation_all_args()
        => Test(
            new DropTableOperation { Name = "Post", Schema = "dbo" },
            """
mb.DropTable(
    name: "Post",
    schema: "dbo");
""",
            o =>
            {
                Assert.Equal("Post", o.Name);
                Assert.Equal("dbo", o.Schema);
            });

    [ConditionalFact]
    public void DropUniqueConstraintOperation_required_args()
        => Test(
            new DropUniqueConstraintOperation { Name = "AK_Post_AltId", Table = "Post" },
            """
mb.DropUniqueConstraint(
    name: "AK_Post_AltId",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("AK_Post_AltId", o.Name);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropUniqueConstraintOperation_all_args()
        => Test(
            new DropUniqueConstraintOperation
            {
                Name = "AK_Post_AltId",
                Schema = "dbo",
                Table = "Post"
            },
            """
mb.DropUniqueConstraint(
    name: "AK_Post_AltId",
    schema: "dbo",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("AK_Post_AltId", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropCheckConstraintOperation_required_args()
        => Test(
            new DropCheckConstraintOperation { Name = "CK_Post_AltId1_AltId2", Table = "Post" },
            """
mb.DropCheckConstraint(
    name: "CK_Post_AltId1_AltId2",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("CK_Post_AltId1_AltId2", o.Name);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void DropCheckConstraintOperation_all_args()
        => Test(
            new DropCheckConstraintOperation
            {
                Name = "CK_Post_AltId1_AltId2",
                Schema = "dbo",
                Table = "Post"
            },
            """
mb.DropCheckConstraint(
    name: "CK_Post_AltId1_AltId2",
    schema: "dbo",
    table: "Post");
""",
            o =>
            {
                Assert.Equal("CK_Post_AltId1_AltId2", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
            });

    [ConditionalFact]
    public void RenameColumnOperation_required_args()
        => Test(
            new RenameColumnOperation
            {
                Name = "Id",
                Table = "Post",
                NewName = "PostId"
            },
            """
mb.RenameColumn(
    name: "Id",
    table: "Post",
    newName: "PostId");
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("Post", o.Table);
                Assert.Equal("PostId", o.NewName);
            });

    [ConditionalFact]
    public void RenameColumnOperation_all_args()
        => Test(
            new RenameColumnOperation
            {
                Name = "Id",
                Schema = "dbo",
                Table = "Post",
                NewName = "PostId"
            },
            """
mb.RenameColumn(
    name: "Id",
    schema: "dbo",
    table: "Post",
    newName: "PostId");
""",
            o =>
            {
                Assert.Equal("Id", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal("PostId", o.NewName);
            });

    [ConditionalFact]
    public void RenameIndexOperation_required_args()
        => Test(
            new RenameIndexOperation { Name = "IX_Post_Title", NewName = "IX_Post_PostTitle" },
            """
mb.RenameIndex(
    name: "IX_Post_Title",
    newName: "IX_Post_PostTitle");
""",
            o =>
            {
                Assert.Equal("IX_Post_Title", o.Name);
                Assert.Equal("IX_Post_PostTitle", o.NewName);
            });

    [ConditionalFact]
    public void RenameIndexOperation_all_args()
        => Test(
            new RenameIndexOperation
            {
                Name = "IX_dbo.Post_Title",
                Schema = "dbo",
                Table = "Post",
                NewName = "IX_dbo.Post_PostTitle"
            },
            """
mb.RenameIndex(
    name: "IX_dbo.Post_Title",
    schema: "dbo",
    table: "Post",
    newName: "IX_dbo.Post_PostTitle");
""",
            o =>
            {
                Assert.Equal("IX_dbo.Post_Title", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Post", o.Table);
                Assert.Equal("IX_dbo.Post_PostTitle", o.NewName);
            });

    [ConditionalFact]
    public void RenameSequenceOperation_required_args()
        => Test(
            new RenameSequenceOperation { Name = "EntityFrameworkHiLoSequence" },
            """
mb.RenameSequence(
    name: "EntityFrameworkHiLoSequence");
""",
            o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));

    [ConditionalFact]
    public void RenameSequenceOperation_all_args()
        => Test(
            new RenameSequenceOperation
            {
                Name = "EntityFrameworkHiLoSequence",
                Schema = "dbo",
                NewName = "MySequence",
                NewSchema = "my"
            },
            """
mb.RenameSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    newName: "MySequence",
    newSchema: "my");
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("MySequence", o.NewName);
                Assert.Equal("my", o.NewSchema);
            });

    [ConditionalFact]
    public void RenameTableOperation_required_args()
        => Test(
            new RenameTableOperation { Name = "Post" },
            """
mb.RenameTable(
    name: "Post");
""",
            o => Assert.Equal("Post", o.Name));

    [ConditionalFact]
    public void RenameTableOperation_all_args()
        => Test(
            new RenameTableOperation
            {
                Name = "Post",
                Schema = "dbo",
                NewName = "Posts",
                NewSchema = "my"
            },
            """
mb.RenameTable(
    name: "Post",
    schema: "dbo",
    newName: "Posts",
    newSchema: "my");
""",
            o =>
            {
                Assert.Equal("Post", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("Posts", o.NewName);
                Assert.Equal("my", o.NewSchema);
            });

    [ConditionalFact]
    public void RestartSequenceOperation_required_args()
        => Test(
            new RestartSequenceOperation { Name = "EntityFrameworkHiLoSequence", StartValue = 1 },
            """
mb.RestartSequence(
    name: "EntityFrameworkHiLoSequence",
    startValue: 1L);
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal(1, o.StartValue);
            });

    [ConditionalFact]
    public void RestartSequenceOperation_all_args()
        => Test(
            new RestartSequenceOperation
            {
                Name = "EntityFrameworkHiLoSequence",
                Schema = "dbo",
                StartValue = 1
            },
            """
mb.RestartSequence(
    name: "EntityFrameworkHiLoSequence",
    schema: "dbo",
    startValue: 1L);
""",
            o =>
            {
                Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                Assert.Equal("dbo", o.Schema);
                Assert.Equal(1, o.StartValue);
            });

    [ConditionalFact]
    public void SqlOperation_required_args()
        => Test(
            new SqlOperation { Sql = "-- I <3 DDL" },
            "mb.Sql(\"-- I <3 DDL\");",
            o => Assert.Equal("-- I <3 DDL", o.Sql));

    private static readonly LineString _lineString1 = new(
        [new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(7.1, 7.2)]) { SRID = 4326 };

    private static readonly LineString _lineString2 = new(
        [new Coordinate(7.1, 7.2), new Coordinate(20.2, 20.2), new Coordinate(20.20, 1.1), new Coordinate(70.1, 70.2)])
    {
        SRID = 4326
    };

    private static readonly MultiPoint _multiPoint = new(
        [new Point(1.1, 2.2), new Point(2.2, 2.2), new Point(2.2, 1.1)]) { SRID = 4326 };

    private static readonly Polygon _polygon1 = new(
        new LinearRing(
            [new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(1.1, 2.2)]))
    {
        SRID = 4326
    };

    private static readonly Polygon _polygon2 = new(
        new LinearRing(
            [new Coordinate(10.1, 20.2), new Coordinate(20.2, 20.2), new Coordinate(20.2, 10.1), new Coordinate(10.1, 20.2)]))
    {
        SRID = 4326
    };

    private static readonly Point _point1 = new(1.1, 2.2, 3.3) { SRID = 4326 };

    private static readonly MultiLineString _multiLineString = new(
        [_lineString1, _lineString2]) { SRID = 4326 };

    private static readonly MultiPolygon _multiPolygon = new(
        [_polygon2, _polygon1]) { SRID = 4326 };

    private static readonly GeometryCollection _geometryCollection = new(
        [_lineString1, _lineString2, _multiPoint, _polygon1, _polygon2, _point1, _multiLineString, _multiPolygon])
    {
        SRID = 4326
    };

    [ConditionalFact]
    public void InsertDataOperation_all_args()
        => Test(
            new InsertDataOperation
            {
                Schema = "dbo",
                Table = "People",
                Columns = ["Id", "Full Name", "Geometry"],
                Values = new object[,]
                {
                    { 0, null, null },
                    { 1, "Daenerys Targaryen", _point1 },
                    { 2, "John Snow", _polygon1 },
                    { 3, "Arya Stark", _lineString1 },
                    { 4, "Harry Strickland", _multiPoint },
                    { 5, "The Imp", _multiPolygon },
                    { 6, "The Kingslayer", _multiLineString },
                    { 7, "Aemon Targaryen", _geometryCollection }
                }
            },
            """
mb.InsertData(
    schema: "dbo",
    table: "People",
    columns: new[] { "Id", "Full Name", "Geometry" },
    values: new object[,]
    {
        { 0, null, null },
        { 1, "Daenerys Targaryen", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT Z(1.1 2.2 3.3)") },
        { 2, "John Snow", (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))") },
        { 3, "Arya Stark", (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)") },
        { 4, "Harry Strickland", (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))") },
        { 5, "The Imp", (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))") },
        { 6, "The Kingslayer", (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))") },
        { 7, "Aemon Targaryen", (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;GEOMETRYCOLLECTION Z(LINESTRING Z(1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), LINESTRING Z(7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN), MULTIPOINT Z((1.1 2.2 NaN), (2.2 2.2 NaN), (2.2 1.1 NaN)), POLYGON Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN)), POLYGON Z((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), POINT Z(1.1 2.2 3.3), MULTILINESTRING Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), (7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN)), MULTIPOLYGON Z(((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), ((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN))))") }
    });
""",
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

    [ConditionalFact]
    public void InsertDataOperation_required_args()
        => Test(
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["Geometry"],
                Values = new object[,] { { _point1 } }
            },
            """
mb.InsertData(
    table: "People",
    column: "Geometry",
    value: (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT Z(1.1 2.2 3.3)"));
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.Columns);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal(_point1, o.Values[0, 0]);
            });

    [ConditionalFact]
    public void InsertDataOperation_required_empty_array()
        => Test(
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["Tags"],
                Values = new object[,] { { Array.Empty<string>() } }
            },
            """
mb.InsertData(
    table: "People",
    column: "Tags",
    value: new string[0]);
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.Columns);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal([], (string[])o.Values[0, 0]);
            });

    [ConditionalFact]
    public void InsertDataOperation_required_empty_array_composite()
        => Test(
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["First Name", "Last Name", "Geometry"],
                Values = new object[,] { { "John", null, Array.Empty<string>() } }
            },
            """
mb.InsertData(
    table: "People",
    columns: new[] { "First Name", "Last Name", "Geometry" },
    values: new object[] { "John", null, new string[0] });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Equal(3, o.Columns.Length);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(3, o.Values.GetLength(1));
                Assert.Null(o.Values[0, 1]);
                Assert.Equal([], (string[])o.Values[0, 2]);
            });

    [ConditionalFact]
    public void InsertDataOperation_required_args_composite()
        => Test(
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["First Name", "Last Name", "Geometry"],
                Values = new object[,] { { "John", "Snow", _polygon1 } }
            },
            """
mb.InsertData(
    table: "People",
    columns: new[] { "First Name", "Last Name", "Geometry" },
    values: new object[] { "John", "Snow", (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))") });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Equal(3, o.Columns.Length);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(3, o.Values.GetLength(1));
                Assert.Equal("Snow", o.Values[0, 1]);
                Assert.Equal(_polygon1, o.Values[0, 2]);
            });

    [ConditionalFact]
    public void InsertDataOperation_required_args_multiple_rows()
        => Test(
            new InsertDataOperation
            {
                Table = "People",
                Columns = ["Geometries"],
                Values = new object[,] { { _lineString1 }, { _multiPoint } }
            },
            """
mb.InsertData(
    table: "People",
    column: "Geometries",
    values: new object[]
    {
        (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)"),
        (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))")
    });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.Columns);
                Assert.Equal(2, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal(_lineString1, o.Values[0, 0]);
                Assert.Equal(_multiPoint, o.Values[1, 0]);
            });

    [ConditionalFact]
    public void InsertDataOperation_args_with_linebreaks()
        => Test(
            new InsertDataOperation
            {
                Schema = "dbo",
                Table = "TestLineBreaks",
                Columns = ["Id", "Description"],
                Values = new object[,]
                {
                    { 0, "Contains\r\na Windows linebreak" },
                    { 1, "Contains a\nLinux linebreak" },
                    { 2, "Contains a single Backslash r,\rjust in case" },
                }
            },
            $$"""
mb.InsertData(
    schema: "dbo",
    table: "TestLineBreaks",
    columns: new[] { "Id", "Description" },
    values: new object[,]
    {
        { 0, "Contains{{"\\r\\n"}}a Windows linebreak" },
        { 1, "Contains a{{"\\n"}}Linux linebreak" },
        { 2, "Contains a single Backslash r,{{"\\r"}}just in case" }
    });
""",
            operation =>
            {
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("TestLineBreaks", operation.Table);
                Assert.Equal(2, operation.Columns.Length);
                Assert.Equal(3, operation.Values.GetLength(0));
                Assert.Equal(2, operation.Values.GetLength(1));
                Assert.Equal("Contains\r\na Windows linebreak", operation.Values[0, 1]);
                Assert.Equal("Contains a\nLinux linebreak", operation.Values[1, 1]);
                Assert.Equal("Contains a single Backslash r,\rjust in case", operation.Values[2, 1]);
            });

    [ConditionalFact]
    public void DeleteDataOperation_all_args()
        => Test(
            new DeleteDataOperation
            {
                Schema = "dbo",
                Table = "People",
                KeyColumns = ["First Name"],
                KeyColumnTypes = ["string"],
                KeyValues = new object[,] { { "Hodor" }, { "Daenerys" }, { "John" }, { "Arya" }, { "Harry" } }
            },
            """
mb.DeleteData(
    schema: "dbo",
    table: "People",
    keyColumn: "First Name",
    keyColumnType: "string",
    keyValues: new object[]
    {
        "Hodor",
        "Daenerys",
        "John",
        "Arya",
        "Harry"
    });
""",
            o =>
            {
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(5, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("John", o.KeyValues[2, 0]);
            });

    [ConditionalFact]
    public void DeleteDataOperation_all_args_composite()
        => Test(
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyColumnTypes = ["string", "string"],
                KeyValues = new object[,]
                {
                    { "Hodor", null }, { "Daenerys", "Targaryen" }, { "John", "Snow" }, { "Arya", "Stark" }, { "Harry", "Strickland" }
                }
            },
            """
mb.DeleteData(
    table: "People",
    keyColumns: new[] { "First Name", "Last Name" },
    keyColumnTypes: new[] { "string", "string" },
    keyValues: new object[,]
    {
        { "Hodor", null },
        { "Daenerys", "Targaryen" },
        { "John", "Snow" },
        { "Arya", "Stark" },
        { "Harry", "Strickland" }
    });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Equal(2, o.KeyColumns.Length);
                Assert.Equal(5, o.KeyValues.GetLength(0));
                Assert.Equal(2, o.KeyValues.GetLength(1));
                Assert.Equal("Snow", o.KeyValues[2, 1]);
            });

    [ConditionalFact]
    public void DeleteDataOperation_required_args()
        => Test(
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["Last Name"],
                KeyValues = new object[,] { { "Snow" } }
            },
            """
mb.DeleteData(
    table: "People",
    keyColumn: "Last Name",
    keyValue: "Snow");
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(1, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("Snow", o.KeyValues[0, 0]);
            });

    [ConditionalFact]
    public void DeleteDataOperation_required_args_composite()
        => Test(
            new DeleteDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "John", "Snow" } }
            },
            """
mb.DeleteData(
    table: "People",
    keyColumns: new[] { "First Name", "Last Name" },
    keyValues: new object[] { "John", "Snow" });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Equal(2, o.KeyColumns.Length);
                Assert.Equal(1, o.KeyValues.GetLength(0));
                Assert.Equal(2, o.KeyValues.GetLength(1));
                Assert.Equal("Snow", o.KeyValues[0, 1]);
            });

    [ConditionalFact]
    public void DeleteDataOperation_args_with_linebreaks()
        => Test(
            new DeleteDataOperation
            {
                Table = "TestLineBreaks",
                KeyColumns = ["Id", "Description"],
                KeyValues = new object[,]
                {
                    { 0, "Contains\r\na Windows linebreak" },
                    { 1, "Contains a\nLinux linebreak" },
                    { 2, "Contains a single Backslash r,\rjust in case" },
                }
            },
            $$"""
mb.DeleteData(
    table: "TestLineBreaks",
    keyColumns: new[] { "Id", "Description" },
    keyValues: new object[,]
    {
        { 0, "Contains{{"\\r\\n"}}a Windows linebreak" },
        { 1, "Contains a{{"\\n"}}Linux linebreak" },
        { 2, "Contains a single Backslash r,{{"\\r"}}just in case" }
    });
""",
            operation =>
            {
                Assert.Equal("TestLineBreaks", operation.Table);
                Assert.Equal(2, operation.KeyColumns.Length);
                Assert.Equal(3, operation.KeyValues.GetLength(0));
                Assert.Equal(2, operation.KeyValues.GetLength(1));
                Assert.Equal("Contains\r\na Windows linebreak", operation.KeyValues[0, 1]);
                Assert.Equal("Contains a\nLinux linebreak", operation.KeyValues[1, 1]);
                Assert.Equal("Contains a single Backslash r,\rjust in case", operation.KeyValues[2, 1]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_all_args()
        => Test(
            new UpdateDataOperation
            {
                Schema = "dbo",
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Hodor" }, { "Daenerys" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Winterfell", "Stark", "Northmen" }, { "Dragonstone", "Targaryen", "Valyrian" } }
            },
            """
mb.UpdateData(
    schema: "dbo",
    table: "People",
    keyColumn: "First Name",
    keyValues: new object[]
    {
        "Hodor",
        "Daenerys"
    },
    columns: new[] { "Birthplace", "House Allegiance", "Culture" },
    values: new object[,]
    {
        { "Winterfell", "Stark", "Northmen" },
        { "Dragonstone", "Targaryen", "Valyrian" }
    });
""",
            o =>
            {
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(2, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                Assert.Equal(3, o.Columns.Length);
                Assert.Equal(2, o.Values.GetLength(0));
                Assert.Equal(3, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[1, 1]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_all_args_composite()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Hodor", null }, { "Daenerys", "Targaryen" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Stark" }, { "Targaryen" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumns: new[] { "First Name", "Last Name" },
    keyValues: new object[,]
    {
        { "Hodor", null },
        { "Daenerys", "Targaryen" }
    },
    column: "House Allegiance",
    values: new object[]
    {
        "Stark",
        "Targaryen"
    });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Equal(2, o.KeyColumns.Length);
                Assert.Equal(2, o.KeyValues.GetLength(0));
                Assert.Equal(2, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                Assert.Single(o.Columns);
                Assert.Equal(2, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[1, 0]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_all_args_composite_multi()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Hodor", null }, { "Daenerys", "Targaryen" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Winterfell", "Stark", "Northmen" }, { "Dragonstone", "Targaryen", "Valyrian" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumns: new[] { "First Name", "Last Name" },
    keyValues: new object[,]
    {
        { "Hodor", null },
        { "Daenerys", "Targaryen" }
    },
    columns: new[] { "Birthplace", "House Allegiance", "Culture" },
    values: new object[,]
    {
        { "Winterfell", "Stark", "Northmen" },
        { "Dragonstone", "Targaryen", "Valyrian" }
    });
""",
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

    [ConditionalFact]
    public void UpdateDataOperation_all_args_multi()
        => Test(
            new UpdateDataOperation
            {
                Schema = "dbo",
                Table = "People",
                KeyColumns = ["Full Name"],
                KeyValues = new object[,] { { "Daenerys Targaryen" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Dragonstone", "Targaryen", "Valyrian" } }
            },
            """
mb.UpdateData(
    schema: "dbo",
    table: "People",
    keyColumn: "Full Name",
    keyValue: "Daenerys Targaryen",
    columns: new[] { "Birthplace", "House Allegiance", "Culture" },
    values: new object[] { "Dragonstone", "Targaryen", "Valyrian" });
""",
            o =>
            {
                Assert.Equal("dbo", o.Schema);
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(1, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys Targaryen", o.KeyValues[0, 0]);
                Assert.Equal(3, o.Columns.Length);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(3, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[0, 1]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_required_args()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Daenerys" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Targaryen" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumn: "First Name",
    keyValue: "Daenerys",
    column: "House Allegiance",
    value: "Targaryen");
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(1, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys", o.KeyValues[0, 0]);
                Assert.Single(o.Columns);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[0, 0]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_required_args_multiple_rows()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name"],
                KeyValues = new object[,] { { "Hodor" }, { "Daenerys" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Stark" }, { "Targaryen" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumn: "First Name",
    keyValues: new object[]
    {
        "Hodor",
        "Daenerys"
    },
    column: "House Allegiance",
    values: new object[]
    {
        "Stark",
        "Targaryen"
    });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(2, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys", o.KeyValues[1, 0]);
                Assert.Single(o.Columns);
                Assert.Equal(2, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[1, 0]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_required_args_composite()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Daenerys", "Targaryen" } },
                Columns = ["House Allegiance"],
                Values = new object[,] { { "Targaryen" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumns: new[] { "First Name", "Last Name" },
    keyValues: new object[] { "Daenerys", "Targaryen" },
    column: "House Allegiance",
    value: "Targaryen");
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Equal(2, o.KeyColumns.Length);
                Assert.Equal(1, o.KeyValues.GetLength(0));
                Assert.Equal(2, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys", o.KeyValues[0, 0]);
                Assert.Single(o.Columns);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(1, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[0, 0]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_required_args_composite_multi()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["First Name", "Last Name"],
                KeyValues = new object[,] { { "Daenerys", "Targaryen" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Dragonstone", "Targaryen", "Valyrian" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumns: new[] { "First Name", "Last Name" },
    keyValues: new object[] { "Daenerys", "Targaryen" },
    columns: new[] { "Birthplace", "House Allegiance", "Culture" },
    values: new object[] { "Dragonstone", "Targaryen", "Valyrian" });
""",
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

    [ConditionalFact]
    public void UpdateDataOperation_required_args_multi()
        => Test(
            new UpdateDataOperation
            {
                Table = "People",
                KeyColumns = ["Full Name"],
                KeyValues = new object[,] { { "Daenerys Targaryen" } },
                Columns = ["Birthplace", "House Allegiance", "Culture"],
                Values = new object[,] { { "Dragonstone", "Targaryen", "Valyrian" } }
            },
            """
mb.UpdateData(
    table: "People",
    keyColumn: "Full Name",
    keyValue: "Daenerys Targaryen",
    columns: new[] { "Birthplace", "House Allegiance", "Culture" },
    values: new object[] { "Dragonstone", "Targaryen", "Valyrian" });
""",
            o =>
            {
                Assert.Equal("People", o.Table);
                Assert.Single(o.KeyColumns);
                Assert.Equal(1, o.KeyValues.GetLength(0));
                Assert.Equal(1, o.KeyValues.GetLength(1));
                Assert.Equal("Daenerys Targaryen", o.KeyValues[0, 0]);
                Assert.Equal(3, o.Columns.Length);
                Assert.Equal(1, o.Values.GetLength(0));
                Assert.Equal(3, o.Values.GetLength(1));
                Assert.Equal("Targaryen", o.Values[0, 1]);
            });

    [ConditionalFact]
    public void UpdateDataOperation_with_linebreaks()
        => Test(
            new UpdateDataOperation
            {
                Schema = "dbo",
                Table = "TestLineBreaks",
                KeyColumns = ["Id"],
                KeyValues = new object[,] { { 0 }, { 1 }, { 2 }, },
                Columns = ["Description"],
                Values = new object[,]
                {
                    { "Contains\r\na Windows linebreak" },
                    { "Contains a\nLinux linebreak" },
                    { "Contains a single Backslash r,\rjust in case" },
                }
            },
            $$"""
mb.UpdateData(
    schema: "dbo",
    table: "TestLineBreaks",
    keyColumn: "Id",
    keyValues: new object[]
    {
        0,
        1,
        2
    },
    column: "Description",
    values: new object[]
    {
        "Contains{{"\\r\\n"}}a Windows linebreak",
        "Contains a{{"\\n"}}Linux linebreak",
        "Contains a single Backslash r,{{"\\r"}}just in case"
    });
""",
            operation =>
            {
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("TestLineBreaks", operation.Table);
                Assert.Single(operation.KeyColumns);
                Assert.Equal(3, operation.KeyValues.GetLength(0));
                Assert.Equal(1, operation.KeyValues.GetLength(1));
                Assert.Single(operation.Columns);
                Assert.Equal(3, operation.Values.GetLength(0));
                Assert.Equal(1, operation.Values.GetLength(1));
                Assert.Equal("Contains\r\na Windows linebreak", operation.Values[0, 0]);
                Assert.Equal("Contains a\nLinux linebreak", operation.Values[1, 0]);
                Assert.Equal("Contains a single Backslash r,\rjust in case", operation.Values[2, 0]);
            });

    [ConditionalFact]
    public void AlterTableOperation_annotation_set_to_null()
    {
        var oldTable = new CreateTableOperation { Name = "Customer", };
        oldTable.AddAnnotation("MyAnnotation1", "Bar");
        oldTable.AddAnnotation("MyAnnotation2", null);

        var alterTable = new AlterTableOperation { Name = "NewCustomer", OldTable = oldTable };

        alterTable.AddAnnotation("MyAnnotation1", null);
        alterTable.AddAnnotation("MyAnnotation2", "Foo");

        Test(
            alterTable,
            """
mb.AlterTable(
    name: "NewCustomer")
    .Annotation("MyAnnotation1", null)
    .Annotation("MyAnnotation2", "Foo")
    .OldAnnotation("MyAnnotation1", "Bar")
    .OldAnnotation("MyAnnotation2", null);
""",
            operation =>
            {
                Assert.Equal("NewCustomer", operation.Name);
                Assert.Null(operation.GetAnnotation("MyAnnotation1").Value);
                Assert.Equal("Foo", operation.GetAnnotation("MyAnnotation2").Value);
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
                            new IRelationalTypeMappingSourcePlugin[]
                            {
                                new SqlServerNetTopologySuiteTypeMappingSourcePlugin(NtsGeometryServices.Instance)
                            })))));

        var builder = new IndentedStringBuilder();
        generator.Generate("mb", new[] { operation }, builder);
        var code = builder.ToString();

        Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);

        var build = new BuildSource
        {
            References = { BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"), BuildReference.ByName("NetTopologySuite") },
            Sources =
            {
                {
                    "Migration.cs", $$"""
                    using Microsoft.EntityFrameworkCore.Migrations;
                    using NetTopologySuite.Geometries;

                    #nullable disable

                    public static class OperationsFactory
                    {
                        public static void Create(MigrationBuilder mb)
                        {
                            {{code}}
                        }
                    }
"""
                }
            }
        };

        var assembly = build.BuildInMemory();
        var factoryType = assembly.GetType("OperationsFactory");
        var createMethod = factoryType.GetTypeInfo().GetDeclaredMethod("Create");
        var mb = new MigrationBuilder(activeProvider: null);
        createMethod.Invoke(null, [mb]);
        var result = mb.Operations.Cast<T>().Single();

        assert(result);
    }
}

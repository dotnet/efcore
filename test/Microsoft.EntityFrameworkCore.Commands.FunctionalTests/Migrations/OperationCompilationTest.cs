// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Commands.Migrations
{
    [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR | RuntimeFrameworks.Mono, SkipReason = "https://github.com/aspnet/EntityFramework/issues/4841")]
    public class OperationCompilationTest
    {
        private static string EOL => Environment.NewLine;

        [ConditionalFact]
        public void AddColumnOperation_required_args()
        {
            Test(
                new AddColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int)
                },
                "mb.AddColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    nullable: false);",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(typeof(int), o.ClrType);
                    });
        }

        [ConditionalFact]
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
                    IsNullable = true,
                    DefaultValue = 1
                },
                "mb.AddColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    type: \"int\"," + EOL +
                "    nullable: true," + EOL +
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
                    });
        }

        [ConditionalFact]
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
                "mb.AddColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    nullable: false," + EOL +
                "    defaultValueSql: \"1\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(typeof(int), o.ClrType);
                        Assert.Equal("1", o.DefaultValueSql);
                    });
        }

        [ConditionalFact]
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
                "mb.AddColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    nullable: false," + EOL +
                "    computedColumnSql: \"1\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(typeof(int), o.ClrType);
                        Assert.Equal("1", o.ComputedColumnSql);
                    });
        }

        [ConditionalFact]
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
                "mb.AddForeignKey(" + EOL +
                "    name: \"FK_Post_Blog_BlogId\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"BlogId\"," + EOL +
                "    principalTable: \"Blog\"," + EOL +
                "    principalColumn: \"Id\");",
                o =>
                    {
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("FK_Post_Blog_BlogId", o.Name);
                        Assert.Equal(new[] { "BlogId" }, o.Columns);
                        Assert.Equal("Blog", o.PrincipalTable);
                    });
        }

        [ConditionalFact]
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
                "mb.AddForeignKey(" + EOL +
                "    name: \"FK_Post_Blog_BlogId\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"BlogId\"," + EOL +
                "    principalSchema: \"my\"," + EOL +
                "    principalTable: \"Blog\"," + EOL +
                "    principalColumn: \"Id\"," + EOL +
                "    onUpdate: ReferentialAction.Restrict," + EOL +
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

        [ConditionalFact]
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
                "mb.AddForeignKey(" + EOL +
                "    name: \"FK_Post_Blog_BlogId1_BlogId2\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    columns: new[] { \"BlogId1\", \"BlogId2\" }," + EOL +
                "    principalTable: \"Blog\"," + EOL +
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

        [ConditionalFact]
        public void AddPrimaryKey_required_args()
        {
            Test(
                new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post",
                    Columns = new[] { "Id" }
                },
                "mb.AddPrimaryKey(" + EOL +
                "    name: \"PK_Post\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"Id\");",
                o =>
                    {
                        Assert.Equal("PK_Post", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "Id" }, o.Columns);
                    });
        }

        [ConditionalFact]
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
                "mb.AddPrimaryKey(" + EOL +
                "    name: \"PK_Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"Id\");",
                o =>
                    {
                        Assert.Equal("PK_Post", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "Id" }, o.Columns);
                    });
        }

        [ConditionalFact]
        public void AddPrimaryKey_composite()
        {
            Test(
                new AddPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post",
                    Columns = new[] { "Id1", "Id2" }
                },
                "mb.AddPrimaryKey(" + EOL +
                "    name: \"PK_Post\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    columns: new[] { \"Id1\", \"Id2\" });",
                o =>
                    {
                        Assert.Equal("PK_Post", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "Id1", "Id2" }, o.Columns);
                    });
        }

        [ConditionalFact]
        public void AddUniqueConstraint_required_args()
        {
            Test(
                new AddUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Table = "Post",
                    Columns = new[] { "AltId" }
                },
                "mb.AddUniqueConstraint(" + EOL +
                "    name: \"AK_Post_AltId\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"AltId\");",
                o =>
                    {
                        Assert.Equal("AK_Post_AltId", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "AltId" }, o.Columns);
                    });
        }

        [ConditionalFact]
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
                "mb.AddUniqueConstraint(" + EOL +
                "    name: \"AK_Post_AltId\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"AltId\");",
                o =>
                    {
                        Assert.Equal("AK_Post_AltId", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "AltId" }, o.Columns);
                    });
        }

        [ConditionalFact]
        public void AddUniqueConstraint_composite()
        {
            Test(
                new AddUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId1_AltId2",
                    Table = "Post",
                    Columns = new[] { "AltId1", "AltId2" }
                },
                "mb.AddUniqueConstraint(" + EOL +
                "    name: \"AK_Post_AltId1_AltId2\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    columns: new[] { \"AltId1\", \"AltId2\" });",
                o =>
                    {
                        Assert.Equal("AK_Post_AltId1_AltId2", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "AltId1", "AltId2" }, o.Columns);
                    });
        }

        [ConditionalFact]
        public void AlterColumnOperation_required_args()
        {
            Test(
                new AlterColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    ClrType = typeof(int)
                },
                "mb.AlterColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    nullable: false);",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(typeof(int), o.ClrType);
                    });
        }

        [ConditionalFact]
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
                    IsNullable = true,
                    DefaultValue = 1
                },
                "mb.AlterColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    type: \"int\"," + EOL +
                "    nullable: true," + EOL +
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
                    });
        }

        [ConditionalFact]
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
                "mb.AlterColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    nullable: false," + EOL +
                "    defaultValueSql: \"1\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("1", o.DefaultValueSql);
                    });
        }

        [ConditionalFact]
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
                "mb.AlterColumn<int>(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    nullable: false," + EOL +
                "    computedColumnSql: \"1\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("1", o.ComputedColumnSql);
                    });
        }

        [ConditionalFact]
        public void AlterSequenceOperation_required_args()
        {
            Test(
                new AlterSequenceOperation { Name = "EntityFrameworkHiLoSequence" },
                "mb.AlterSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));
        }

        [ConditionalFact]
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
                    IsCyclic = true
                },
                "mb.AlterSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    incrementBy: 3," + EOL +
                "    minValue: 2L," + EOL +
                "    maxValue: 4L," + EOL +
                "    cyclic: true);",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal(3, o.IncrementBy);
                        Assert.Equal(2, o.MinValue);
                        Assert.Equal(4, o.MaxValue);
                        Assert.True(o.IsCyclic);
                    });
        }

        [ConditionalFact]
        public void CreateIndexOperation_required_args()
        {
            Test(
                new CreateIndexOperation
                {
                    Name = "IX_Post_Title",
                    Table = "Post",
                    Columns = new[] { "Title" }
                },
                "mb.CreateIndex(" + EOL +
                "    name: \"IX_Post_Title\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"Title\");",
                o =>
                    {
                        Assert.Equal("IX_Post_Title", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "Title" }, o.Columns);
                    });
        }

        [ConditionalFact]
        public void CreateIndexOperation_all_args()
        {
            Test(
                new CreateIndexOperation
                {
                    Name = "IX_Post_Title",
                    Schema = "dbo",
                    Table = "Post",
                    Columns = new[] { "Title" },
                    IsUnique = true
                },
                "mb.CreateIndex(" + EOL +
                "    name: \"IX_Post_Title\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    column: \"Title\"," + EOL +
                "    unique: true);",
                o =>
                    {
                        Assert.Equal("IX_Post_Title", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "Title" }, o.Columns);
                        Assert.True(o.IsUnique);
                    });
        }

        [ConditionalFact]
        public void CreateIndexOperation_composite()
        {
            Test(
                new CreateIndexOperation
                {
                    Name = "IX_Post_Title_Subtitle",
                    Table = "Post",
                    Columns = new[] { "Title", "Subtitle" }
                },
                "mb.CreateIndex(" + EOL +
                "    name: \"IX_Post_Title_Subtitle\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    columns: new[] { \"Title\", \"Subtitle\" });",
                o =>
                    {
                        Assert.Equal("IX_Post_Title_Subtitle", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal(new[] { "Title", "Subtitle" }, o.Columns);
                    });
        }

        [ConditionalFact]
        public void CreateSchemaOperation_required_args()
        {
            Test(
                new EnsureSchemaOperation { Name = "my" },
                "mb.EnsureSchema(" + EOL +
                "    name: \"my\");",
                o => Assert.Equal("my", o.Name));
        }

        [ConditionalFact]
        public void CreateSequenceOperation_required_args()
        {
            Test(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    ClrType = typeof(long)
                },
                "mb.CreateSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal(typeof(long), o.ClrType);
                    });
        }

        [ConditionalFact]
        public void CreateSequenceOperation_required_args_not_long()
        {
            Test(
                new CreateSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    ClrType = typeof(int)
                },
                "mb.CreateSequence<int>(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal(typeof(int), o.ClrType);
                    });
        }

        [ConditionalFact]
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
                "mb.CreateSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    startValue: 3L," + EOL +
                "    incrementBy: 5," + EOL +
                "    minValue: 2L," + EOL +
                "    maxValue: 4L," + EOL +
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

        [ConditionalFact]
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
                "mb.CreateSequence<int>(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    startValue: 3L," + EOL +
                "    incrementBy: 5," + EOL +
                "    minValue: 2L," + EOL +
                "    maxValue: 4L," + EOL +
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

        [ConditionalFact]
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        Id = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
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

        [ConditionalFact]
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
                            IsNullable = true,
                            DefaultValue = 1
                        }
                    }
                },
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        PostId = table.Column<int>(name: \"Post Id\", type: \"int\", nullable: true, defaultValue: 1)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
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
                        Assert.Equal(1, o.Columns[0].DefaultValue);
                    });
        }

        [ConditionalFact]
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        Id = table.Column<int>(nullable: false, defaultValueSql: \"1\")" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
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

        [ConditionalFact]
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        Id = table.Column<int>(nullable: false, computedColumnSql: \"1\")" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
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

        [ConditionalFact]
        public void CreateTableOperation_ForeignKeys_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation { Name = "BlogId", ClrType = typeof(int) }
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        BlogId = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.ForeignKey(" + EOL +
                "            name: \"FK_Post_Blog_BlogId\"," + EOL +
                "            column: x => x.BlogId," + EOL +
                "            principalTable: \"Blog\"," + EOL +
                "            principalColumn: \"Id\");" + EOL +
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

        [ConditionalFact]
        public void CreateTableOperation_ForeignKeys_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Schema = "dbo",
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation { Name = "BlogId", ClrType = typeof(int) }
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        BlogId = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.ForeignKey(" + EOL +
                "            name: \"FK_Post_Blog_BlogId\"," + EOL +
                "            column: x => x.BlogId," + EOL +
                "            principalSchema: \"my\"," + EOL +
                "            principalTable: \"Blog\"," + EOL +
                "            principalColumn: \"Id\"," + EOL +
                "            onUpdate: ReferentialAction.SetNull," + EOL +
                "            onDelete: ReferentialAction.SetDefault);" + EOL +
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

        [ConditionalFact]
        public void CreateTableOperation_ForeignKeys_composite()
        {
            Test(
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
                            Columns = new[] { "BlogId1", "BlogId2" },
                            PrincipalTable = "Blog",
                            PrincipalColumns = new[] { "Id1", "Id2" }
                        }
                    }
                },
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        BlogId1 = table.Column<int>(nullable: false)," + EOL +
                "        BlogId2 = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.ForeignKey(" + EOL +
                "            name: \"FK_Post_Blog_BlogId1_BlogId2\"," + EOL +
                "            columns: x => new { x.BlogId1, x.BlogId2 }," + EOL +
                "            principalTable: \"Blog\"," + EOL +
                "            principalColumns: new[] { \"Id1\", \"Id2\" });" + EOL +
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

        [ConditionalFact]
        public void CreateTableOperation_PrimaryKey_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation { Name = "Id", ClrType = typeof(int) }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = "PK_Post",
                        Table = "Post",
                        Columns = new[] { "Id" }
                    }
                },
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        Id = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.PrimaryKey(\"PK_Post\", x => x.Id);" + EOL +
                "    });",
                o =>
                    {
                        Assert.NotNull(o.PrimaryKey);

                        Assert.Equal("PK_Post", o.PrimaryKey.Name);
                        Assert.Equal("Post", o.PrimaryKey.Table);
                        Assert.Equal(new[] { "Id" }, o.PrimaryKey.Columns);
                    });
        }

        [ConditionalFact]
        public void CreateTableOperation_PrimaryKey_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation { Name = "Id", ClrType = typeof(int) }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = "PK_Post",
                        Schema = "dbo",
                        Table = "Post",
                        Columns = new[] { "Id" }
                    }
                },
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        Id = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.PrimaryKey(\"PK_Post\", x => x.Id);" + EOL +
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

        [ConditionalFact]
        public void CreateTableOperation_PrimaryKey_composite()
        {
            Test(
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
                        Columns = new[] { "Id1", "Id2" }
                    }
                },
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        Id1 = table.Column<int>(nullable: false)," + EOL +
                "        Id2 = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.PrimaryKey(\"PK_Post\", x => new { x.Id1, x.Id2 });" + EOL +
                "    });",
                o =>
                    {
                        Assert.NotNull(o.PrimaryKey);

                        Assert.Equal("PK_Post", o.PrimaryKey.Name);
                        Assert.Equal("Post", o.PrimaryKey.Table);
                        Assert.Equal(new[] { "Id1", "Id2" }, o.PrimaryKey.Columns);
                    });
        }

        [ConditionalFact]
        public void CreateTableOperation_UniqueConstraints_required_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Columns =
                    {
                        new AddColumnOperation { Name = "AltId", ClrType = typeof(int) }
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        AltId = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.UniqueConstraint(\"AK_Post_AltId\", x => x.AltId);" + EOL +
                "    });",
                o =>
                    {
                        Assert.Equal(1, o.UniqueConstraints.Count);

                        Assert.Equal("AK_Post_AltId", o.UniqueConstraints[0].Name);
                        Assert.Equal("Post", o.UniqueConstraints[0].Table);
                        Assert.Equal(new[] { "AltId" }, o.UniqueConstraints[0].Columns);
                    });
        }

        [ConditionalFact]
        public void CreateTableOperation_UniqueConstraints_all_args()
        {
            Test(
                new CreateTableOperation
                {
                    Name = "Post",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation { Name = "AltId", ClrType = typeof(int) }
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
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        AltId = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.UniqueConstraint(\"AK_Post_AltId\", x => x.AltId);" + EOL +
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

        [ConditionalFact]
        public void CreateTableOperation_UniqueConstraints_composite()
        {
            Test(
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
                            Columns = new[] { "AltId1", "AltId2" }
                        }
                    }
                },
                "mb.CreateTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    columns: table => new" + EOL +
                "    {" + EOL +
                "        AltId1 = table.Column<int>(nullable: false)," + EOL +
                "        AltId2 = table.Column<int>(nullable: false)" + EOL +
                "    }," + EOL +
                "    constraints: table =>" + EOL +
                "    {" + EOL +
                "        table.UniqueConstraint(\"AK_Post_AltId1_AltId2\", x => new { x.AltId1, x.AltId2 });" + EOL +
                "    });",
                o =>
                    {
                        Assert.Equal(1, o.UniqueConstraints.Count);

                        Assert.Equal("AK_Post_AltId1_AltId2", o.UniqueConstraints[0].Name);
                        Assert.Equal("Post", o.UniqueConstraints[0].Table);
                        Assert.Equal(new[] { "AltId1", "AltId2" }, o.UniqueConstraints[0].Columns);
                    });
        }

        [ConditionalFact]
        public void DropColumnOperation_required_args()
        {
            Test(
                new DropColumnOperation
                {
                    Name = "Id",
                    Table = "Post"
                },
                "mb.DropColumn(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropColumnOperation_all_args()
        {
            Test(
                new DropColumnOperation
                {
                    Name = "Id",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropColumn(" + EOL +
                "    name: \"Id\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropForeignKeyOperation_required_args()
        {
            Test(
                new DropForeignKeyOperation
                {
                    Name = "FK_Post_BlogId",
                    Table = "Post"
                },
                "mb.DropForeignKey(" + EOL +
                "    name: \"FK_Post_BlogId\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("FK_Post_BlogId", o.Name);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropForeignKeyOperation_all_args()
        {
            Test(
                new DropForeignKeyOperation
                {
                    Name = "FK_Post_BlogId",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropForeignKey(" + EOL +
                "    name: \"FK_Post_BlogId\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("FK_Post_BlogId", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropIndexOperation_required_args()
        {
            Test(
                new DropIndexOperation
                {
                    Name = "IX_Post_Title",
                    Table = "Post"
                },
                "mb.DropIndex(" + EOL +
                "    name: \"IX_Post_Title\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("IX_Post_Title", o.Name);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropIndexOperation_all_args()
        {
            Test(
                new DropIndexOperation
                {
                    Name = "IX_Post_Title",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropIndex(" + EOL +
                "    name: \"IX_Post_Title\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("IX_Post_Title", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropPrimaryKeyOperation_required_args()
        {
            Test(
                new DropPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Table = "Post"
                },
                "mb.DropPrimaryKey(" + EOL +
                "    name: \"PK_Post\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("PK_Post", o.Name);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropPrimaryKeyOperation_all_args()
        {
            Test(
                new DropPrimaryKeyOperation
                {
                    Name = "PK_Post",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropPrimaryKey(" + EOL +
                "    name: \"PK_Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("PK_Post", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropSchemaOperation_required_args()
        {
            Test(
                new DropSchemaOperation { Name = "my" },
                "mb.DropSchema(" + EOL +
                "    name: \"my\");",
                o => Assert.Equal("my", o.Name));
        }

        [ConditionalFact]
        public void DropSequenceOperation_required_args()
        {
            Test(
                new DropSequenceOperation { Name = "EntityFrameworkHiLoSequence" },
                "mb.DropSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));
        }

        [ConditionalFact]
        public void DropSequenceOperation_all_args()
        {
            Test(
                new DropSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo"
                },
                "mb.DropSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    schema: \"dbo\");",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal("dbo", o.Schema);
                    });
        }

        [ConditionalFact]
        public void DropTableOperation_required_args()
        {
            Test(
                new DropTableOperation { Name = "Post" },
                "mb.DropTable(" + EOL +
                "    name: \"Post\");",
                o => Assert.Equal("Post", o.Name));
        }

        [ConditionalFact]
        public void DropTableOperation_all_args()
        {
            Test(
                new DropTableOperation
                {
                    Name = "Post",
                    Schema = "dbo"
                },
                "mb.DropTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    schema: \"dbo\");",
                o =>
                    {
                        Assert.Equal("Post", o.Name);
                        Assert.Equal("dbo", o.Schema);
                    });
        }

        [ConditionalFact]
        public void DropUniqueConstraintOperation_required_args()
        {
            Test(
                new DropUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Table = "Post"
                },
                "mb.DropUniqueConstraint(" + EOL +
                "    name: \"AK_Post_AltId\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("AK_Post_AltId", o.Name);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void DropUniqueConstraintOperation_all_args()
        {
            Test(
                new DropUniqueConstraintOperation
                {
                    Name = "AK_Post_AltId",
                    Schema = "dbo",
                    Table = "Post"
                },
                "mb.DropUniqueConstraint(" + EOL +
                "    name: \"AK_Post_AltId\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\");",
                o =>
                    {
                        Assert.Equal("AK_Post_AltId", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                    });
        }

        [ConditionalFact]
        public void RenameColumnOperation_required_args()
        {
            Test(
                new RenameColumnOperation
                {
                    Name = "Id",
                    Table = "Post",
                    NewName = "PostId"
                },
                "mb.RenameColumn(" + EOL +
                "    name: \"Id\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    newName: \"PostId\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("PostId", o.NewName);
                    });
        }

        [ConditionalFact]
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
                "mb.RenameColumn(" + EOL +
                "    name: \"Id\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    newName: \"PostId\");",
                o =>
                    {
                        Assert.Equal("Id", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("PostId", o.NewName);
                    });
        }

        [ConditionalFact]
        public void RenameIndexOperation_required_args()
        {
            Test(
                new RenameIndexOperation
                {
                    Name = "IX_Post_Title",
                    Table = "Post",
                    NewName = "IX_Post_PostTitle"
                },
                "mb.RenameIndex(" + EOL +
                "    name: \"IX_Post_Title\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    newName: \"IX_Post_PostTitle\");",
                o =>
                    {
                        Assert.Equal("IX_Post_Title", o.Name);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("IX_Post_PostTitle", o.NewName);
                    });
        }

        [ConditionalFact]
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
                "mb.RenameIndex(" + EOL +
                "    name: \"IX_dbo.Post_Title\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    table: \"Post\"," + EOL +
                "    newName: \"IX_dbo.Post_PostTitle\");",
                o =>
                    {
                        Assert.Equal("IX_dbo.Post_Title", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Post", o.Table);
                        Assert.Equal("IX_dbo.Post_PostTitle", o.NewName);
                    });
        }

        [ConditionalFact]
        public void RenameSequenceOperation_required_args()
        {
            Test(
                new RenameSequenceOperation { Name = "EntityFrameworkHiLoSequence" },
                "mb.RenameSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\");",
                o => Assert.Equal("EntityFrameworkHiLoSequence", o.Name));
        }

        [ConditionalFact]
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
                "mb.RenameSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    newName: \"MySequence\"," + EOL +
                "    newSchema: \"my\");",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("MySequence", o.NewName);
                        Assert.Equal("my", o.NewSchema);
                    });
        }

        [ConditionalFact]
        public void RenameTableOperation_required_args()
        {
            Test(
                new RenameTableOperation { Name = "Post" },
                "mb.RenameTable(" + EOL +
                "    name: \"Post\");",
                o => Assert.Equal("Post", o.Name));
        }

        [ConditionalFact]
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
                "mb.RenameTable(" + EOL +
                "    name: \"Post\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    newName: \"Posts\"," + EOL +
                "    newSchema: \"my\");",
                o =>
                    {
                        Assert.Equal("Post", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal("Posts", o.NewName);
                        Assert.Equal("my", o.NewSchema);
                    });
        }

        [ConditionalFact]
        public void RestartSequenceOperation_required_args()
        {
            Test(
                new RestartSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    StartValue = 1
                },
                "mb.RestartSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    startValue: 1L);",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal(1, o.StartValue);
                    });
        }

        [ConditionalFact]
        public void RestartSequenceOperation_all_args()
        {
            Test(
                new RestartSequenceOperation
                {
                    Name = "EntityFrameworkHiLoSequence",
                    Schema = "dbo",
                    StartValue = 1
                },
                "mb.RestartSequence(" + EOL +
                "    name: \"EntityFrameworkHiLoSequence\"," + EOL +
                "    schema: \"dbo\"," + EOL +
                "    startValue: 1L);",
                o =>
                    {
                        Assert.Equal("EntityFrameworkHiLoSequence", o.Name);
                        Assert.Equal("dbo", o.Schema);
                        Assert.Equal(1, o.StartValue);
                    });
        }

        [ConditionalFact]
        public void SqlOperation_required_args()
        {
            Test(
                new SqlOperation { Sql = "-- I <3 DDL" },
                "mb.Sql(\"-- I <3 DDL\");",
                o => Assert.Equal("-- I <3 DDL", o.Sql));
        }

        private void Test<T>(T operation, string expectedCode, Action<T> assert)
            where T : MigrationOperation
        {
            var generator = new CSharpMigrationOperationGenerator(new CSharpHelper());

            var builder = new IndentedStringBuilder();
            generator.Generate("mb", new[] { operation }, builder);
            var code = builder.ToString();

            Assert.Equal(expectedCode, code);

            var build = new BuildSource
            {
                References =
                {
#if NETSTANDARDAPP1_5
                    BuildReference.ByName("System.Collections"),
                    BuildReference.ByName("System.Linq.Expressions"),
                    BuildReference.ByName("System.Reflection"),
#else
                    BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#endif
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational.Design")
                },
                Sources = { @"
                    using Microsoft.EntityFrameworkCore.Migrations;

                    public static class OperationsFactory
                    {
                        public static void Create(MigrationBuilder mb)
                        {
                            " + code + @"
                        }
                    }
                " }
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

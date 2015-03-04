// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Commands.TestUtilities;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class OperationCompilationTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
        public void AddForeignKeyOperation_required_args()
        {
            Test(
                new AddForeignKeyOperation(
                    "Post",
                    /*dependentSchema:*/ null,
                    /*name:*/ null,
                    new[] { "BlogId" },
                    "Blog",
                    principalSchema: null,
                    principalColumns: null,
                    cascadeDelete: false),
                "mb.AddForeignKey(\"Post\", \"BlogId\", \"Blog\");" + EOL,
                o =>
                {
                    Assert.Equal("Post", o.DependentTable);
                    Assert.Equal(new[] { "BlogId" }, o.DependentColumns);
                    Assert.Equal("Blog", o.PrincipalTable);
                });
        }

        [Fact]
        public void AddForeignKeyOperation_all_args()
        {
            Test(
                new AddForeignKeyOperation(
                    "Post",
                    "dbo",
                    "FK_Post_Blog_BlogId",
                    new[] { "BlogId" },
                    "Blog",
                    "my",
                    new[] { "Id" },
                    cascadeDelete: true),
                "mb.AddForeignKey(\"Post\", \"BlogId\", \"Blog\", dependentSchema: \"dbo\", principalSchema: \"my\", principalColumn: \"Id\", cascadeDelete: true, name: \"FK_Post_Blog_BlogId\");" + EOL,
                o =>
                {
                    Assert.Equal("Post", o.DependentTable);
                    Assert.Equal("dbo", o.DependentSchema);
                    Assert.Equal("FK_Post_Blog_BlogId", o.Name);
                    Assert.Equal(new[] { "BlogId" }, o.DependentColumns);
                    Assert.Equal("Blog", o.PrincipalTable);
                    Assert.Equal("my", o.PrincipalSchema);
                    Assert.Equal(new[] { "Id" }, o.PrincipalColumns);
                    Assert.True(o.CascadeDelete);
                });
        }

        [Fact]
        public void AddForeignKeyOperation_composite()
        {
            Test(
                new AddForeignKeyOperation(
                    "Post",
                    /*dependentSchema:*/ null,
                    /*name:*/ null,
                    new[] { "BlogId1", "BlogId2" },
                    "Blog",
                    /*principalSchema:*/ null,
                    new[] { "Id1", "Id2" },
                    cascadeDelete: false),
                "mb.AddForeignKey(\"Post\", new[] { \"BlogId1\", \"BlogId2\" }, \"Blog\", principalColumns: new[] { \"Id1\", \"Id2\" });" + EOL,
                o =>
                {
                    Assert.Equal("Post", o.DependentTable);
                    Assert.Equal(new[] { "BlogId1", "BlogId2" }, o.DependentColumns);
                    Assert.Equal("Blog", o.PrincipalTable);
                    Assert.Equal(new[] { "Id1", "Id2" }, o.PrincipalColumns);
                });
        }

        [Fact]
        public void CreateTableOperation_ForeignKeys_required_args()
        {
            Test(
                new CreateTableOperation("Post", schema: null)
                {
                    Columns =
                    {
                        new ColumnModel("BlogId", "int", nullable: false, defaultValue: null, defaultValueSql: null)
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation(
                            "Post",
                            /*dependentSchema:*/ null,
                            /*name:*/ null,
                            new[] { "BlogId" },
                            "Blog",
                            principalSchema: null,
                            principalColumns: null,
                            cascadeDelete: false)
                    }
                },
                "mb.CreateTable(" + EOL +
                "    \"Post\"," + EOL +
                "    x => new" + EOL +
                "    {" + EOL +
                "        BlogId = x.Column(\"int\")" + EOL +
                "    })" + EOL +
                "    .ForeignKey(x => x.BlogId, \"Blog\");" + EOL,
                o =>
                {
                    Assert.Equal(1, o.ForeignKeys.Count);

                    var fk = o.ForeignKeys.First();
                    Assert.Equal("Post", fk.DependentTable);
                    Assert.Equal(new[] { "BlogId" }, fk.DependentColumns.ToArray());
                    Assert.Equal("Blog", fk.PrincipalTable);
                });
        }

        [Fact]
        public void CreateTableOperation_ForeignKeys_all_args()
        {
            Test(
                new CreateTableOperation("Post", "dbo")
                {
                    Columns =
                    {
                        new ColumnModel("BlogId", "int", nullable: false, defaultValue: null, defaultValueSql: null)
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation(
                            "Post",
                            "dbo",
                            "FK_Post_Blog_BlogId",
                            new[] { "BlogId" },
                            "Blog",
                            "my",
                            new[] { "Id" },
                            cascadeDelete: true)
                    }
                },
                "mb.CreateTable(" + EOL +
                "    \"Post\"," + EOL +
                "    \"dbo\"," + EOL +
                "    x => new" + EOL +
                "    {" + EOL +
                "        BlogId = x.Column(\"int\")" + EOL +
                "    })" + EOL +
                "    .ForeignKey(x => x.BlogId, \"Blog\", principalSchema: \"my\", principalColumn: \"Id\", cascadeDelete: true, name: \"FK_Post_Blog_BlogId\");" + EOL,
                o =>
                {
                    Assert.Equal(1, o.ForeignKeys.Count);

                    var fk = o.ForeignKeys.First();
                    Assert.Equal("Post", fk.DependentTable);
                    Assert.Equal("dbo", fk.DependentSchema);
                    Assert.Equal("FK_Post_Blog_BlogId", fk.Name);
                    Assert.Equal(new[] { "BlogId" }, fk.DependentColumns.ToArray());
                    Assert.Equal("Blog", fk.PrincipalTable);
                    Assert.Equal("my", fk.PrincipalSchema);
                    Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                    Assert.True(fk.CascadeDelete);
                });
        }

        [Fact]
        public void CreateTableOperation_ForeignKeys_composite()
        {
            Test(
                new CreateTableOperation("Post", schema: null)
                {
                    Columns =
                    {
                        new ColumnModel("BlogId1", "int", nullable: false, defaultValue: null, defaultValueSql: null),
                        new ColumnModel("BlogId2", "int", nullable: false, defaultValue: null, defaultValueSql: null)
                    },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation(
                            "Post",
                            /*dependentSchema:*/ null,
                            /*name:*/ null,
                            new[] { "BlogId1", "BlogId2" },
                            "Blog",
                            /*principalSchema:*/ null,
                            new[] { "Id1", "Id2" },
                            cascadeDelete: false)
                    }
                },
                "mb.CreateTable(" + EOL +
                "    \"Post\"," + EOL +
                "    x => new" + EOL +
                "    {" + EOL +
                "        BlogId1 = x.Column(\"int\")," + EOL +
                "        BlogId2 = x.Column(\"int\")" + EOL +
                "    })" + EOL +
                "    .ForeignKey(x => new { x.BlogId1, x.BlogId2 }, \"Blog\", principalColumns: new[] { \"Id1\", \"Id2\" });" + EOL,
                o =>
                {
                    Assert.Equal(1, o.ForeignKeys.Count);

                    var fk = o.ForeignKeys.First();
                    Assert.Equal("Post", fk.DependentTable);
                    Assert.Equal(new[] { "BlogId1", "BlogId2" }, fk.DependentColumns.ToArray());
                    Assert.Equal("Blog", fk.PrincipalTable);
                    Assert.Equal(new[] { "Id1", "Id2" }, fk.PrincipalColumns);
                });
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
                    BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("EntityFramework.Relational")
                },
                Source = @"
                    using System.Collections.Generic;
                    using Microsoft.Data.Entity.Relational.Migrations.Builders;
                    using Microsoft.Data.Entity.Relational.Migrations.Operations;

                    public static class OperationsFactory
                    {
                        public static IReadOnlyList<MigrationOperation> Create()
                        {
                            var mb = new MigrationBuilder();
                            " + code + @"
                            return mb.Operations;
                        }
                    }
                "
            };

            var assembly = build.BuildInMemory();
            var factoryType = assembly.GetType("OperationsFactory");
            var createMethod = factoryType.GetMethod("Create");
            var operations = (IReadOnlyList<MigrationOperation>)createMethod.Invoke(null, null);
            var result = (T)operations.Single();

            assert(result);
        }
    }
}

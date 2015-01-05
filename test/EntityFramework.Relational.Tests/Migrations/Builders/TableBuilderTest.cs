// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Builders
{
    public class TableBuilderTest
    {
        [Fact]
        public void PrimaryKey_sets_AddPrimaryKeyOperation()
        {
            var migrationBuilder = new MigrationBuilder();
            migrationBuilder.CreateTable("dbo.MyTable",
                c => new
                    {
                        Foo = c.Int(),
                        Bar = c.Int()
                    })
                .PrimaryKey("MyPK",
                    t => new
                        {
                            t.Foo, t.Bar
                        });

            Assert.Equal(1, migrationBuilder.Operations.Count);

            var createTableOperation = (CreateTableOperation)migrationBuilder.Operations[0];
            var primaryKeyOp = createTableOperation.PrimaryKey;

            Assert.NotNull(primaryKeyOp);
            Assert.Equal("MyPK", primaryKeyOp.PrimaryKeyName);
            Assert.Equal(new[] { "Foo", "Bar" }, primaryKeyOp.ColumnNames);
        }

        [Fact]
        public void UniqueConstraint_appends_AddUniqueConstraintOperation()
        {
            var migrationBuilder = new MigrationBuilder();
            migrationBuilder.CreateTable("dbo.MyTable",
                c => new
                    {
                        Foo = c.Int(),
                        Bar = c.Int(),
                        C1 = c.Int(),
                        C2 = c.Int()
                    })
                .PrimaryKey("MyPK", t => t.Foo)
                .UniqueConstraint("MyUC1", t => t.Bar)
                .UniqueConstraint("MyUC2", t => new { t.C1, t.C2 });

            Assert.Equal(1, migrationBuilder.Operations.Count);

            var createTableOperation = (CreateTableOperation)migrationBuilder.Operations[0];

            Assert.Equal(2, createTableOperation.UniqueConstraints.Count);
            Assert.Equal("MyUC1", createTableOperation.UniqueConstraints[0].UniqueConstraintName);
            Assert.Equal(new[] { "Bar" }, createTableOperation.UniqueConstraints[0].ColumnNames);
            Assert.Equal("MyUC2", createTableOperation.UniqueConstraints[1].UniqueConstraintName);
            Assert.Equal(new[] { "C1", "C2" }, createTableOperation.UniqueConstraints[1].ColumnNames);
        }

        [Fact]
        public void ForeignKey_appends_AddForeignKeyOperation()
        {
            var migrationBuilder = new MigrationBuilder();
            migrationBuilder.CreateTable("dbo.MyTable",
                c => new
                    {
                        Foo = c.Int(),
                        Bar = c.Int()
                    })
                .ForeignKey("MyFK",
                    t => new { t.Foo, t.Bar },
                    "dbo.MyTable2",
                    new[] { "Foo2", "Bar2" },
                    cascadeDelete: true);

            Assert.Equal(1, migrationBuilder.Operations.Count);

            var createTableOperation = (CreateTableOperation)migrationBuilder.Operations[0];

            Assert.Equal(1, createTableOperation.ForeignKeys.Count);

            var addForeignKeyOperation = createTableOperation.ForeignKeys[0];

            Assert.Equal("MyFK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("dbo.MyTable", addForeignKeyOperation.TableName);
            Assert.Equal("dbo.MyTable2", addForeignKeyOperation.ReferencedTableName);
            Assert.Equal(new[] { "Foo", "Bar" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Foo2", "Bar2" }, addForeignKeyOperation.ReferencedColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void Index_appends_CreateIndexOperation()
        {
            var migrationBuilder = new MigrationBuilder();
            migrationBuilder.CreateTable("dbo.MyTable",
                c => new
                    {
                        Foo = c.Int(),
                        Bar = c.Int()
                    })
                .Index("MyIdx",
                    t => new { t.Foo, t.Bar },
                    unique: true,
                    clustered: true);

            Assert.Equal(1, migrationBuilder.Operations.Count);

            var createTableOperation = (CreateTableOperation)migrationBuilder.Operations[0];

            Assert.Equal(1, createTableOperation.Indexes.Count);

            var createIndexOperation = (CreateIndexOperation)createTableOperation.Indexes[0];

            Assert.Equal("MyIdx", createIndexOperation.IndexName);
            Assert.Equal("dbo.MyTable", createIndexOperation.TableName);
            Assert.Equal(new[] { "Foo", "Bar" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
            Assert.True(createIndexOperation.IsClustered);
        }
    }
}

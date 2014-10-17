// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Model;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Builders
{
    public class TableBuilderTest
    {
        [Fact]
        public void PrimaryKey_sets_primary_key_on_table()
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
            var primaryKey = createTableOperation.Table.PrimaryKey;

            Assert.NotNull(primaryKey);
            Assert.Equal("MyPK", primaryKey.Name);
            Assert.Equal(new[] { "Foo", "Bar" }, primaryKey.Columns.Select(c => c.Name));
        }

        [Fact]
        public void UniqueConstraint_adds_unique_constraint_to_table()
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
            var table = createTableOperation.Table;

            Assert.Equal(2, table.UniqueConstraints.Count);
            Assert.Equal("MyUC1", table.UniqueConstraints[0].Name);
            Assert.Equal(new[] { "Bar" }, table.UniqueConstraints[0].Columns.Select(c => c.Name));
            Assert.Equal("MyUC2", table.UniqueConstraints[1].Name);
            Assert.Equal(new[] { "C1", "C2" }, table.UniqueConstraints[1].Columns.Select(c => c.Name));
        }

        [Fact]
        public void ForeignKey_appends_add_primary_key_operation()
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

            Assert.Equal(2, migrationBuilder.Operations.Count);
            Assert.IsType<AddForeignKeyOperation>(migrationBuilder.Operations[1]);

            var addForeignKeyOperation = (AddForeignKeyOperation)migrationBuilder.Operations[1];

            Assert.Equal("MyFK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("dbo.MyTable", addForeignKeyOperation.TableName);
            Assert.Equal("dbo.MyTable2", addForeignKeyOperation.ReferencedTableName);
            Assert.Equal(new[] { "Foo", "Bar" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Foo2", "Bar2" }, addForeignKeyOperation.ReferencedColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void Index_appends_create_index_operation()
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

            Assert.Equal(2, migrationBuilder.Operations.Count);
            Assert.IsType<CreateIndexOperation>(migrationBuilder.Operations[1]);

            var createIndexOperation = (CreateIndexOperation)migrationBuilder.Operations[1];

            Assert.Equal("MyIdx", createIndexOperation.IndexName);
            Assert.Equal("dbo.MyTable", createIndexOperation.TableName);
            Assert.Equal(new[] { "Foo", "Bar" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
            Assert.True(createIndexOperation.IsClustered);
        }
    }
}

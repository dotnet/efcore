// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteOperationTransformTest : SqliteOperationTransformBase
    {
        [Fact]
        public void RenameColumn_to_TableRebuild()
        {
            var operations = Transform(m => { m.RenameColumn("OldName", "A", "NewName"); }, model =>
                {
                    model.Entity("A", b =>
                        {
                            b.Property<string>("Id");
                            b.Property<string>("NewName");
                            b.Key("Id");
                        });
                });

            var steps = Assert.IsType<TableRebuildOperation>(operations[0]);
            Assert.Collection(steps.Operations,
                AssertRenameTemp("A"),
                AssertCreateTable("A", new[] { "Id", "NewName" }),
                AssertMoveData("A", new[] { "Id", "OldName" }, new[] { "Id", "NewName" }),
                AssertDropTemp("A"));
        }

        [Fact]
        public void Rebuild_filters_obviated_operations()
        {
            var t = "TableName";
            var operations = Transform(migrate =>
                {
                    migrate.DropColumn("Dropped", t);
                    migrate.AlterColumn("Altered", t, "TEXT", nullable: true);
                    migrate.AddColumn("New", t, "TEXT");
                    migrate.CreateIndex("IDX_A", t, new[] { "Indexed" }, unique: true);
                    migrate.AddPrimaryKey("PK_A", t, new[] { "Key" });
                }, model =>
                    {
                        model.Entity(t, b =>
                            {
                                b.Property<string>("Altered");
                                b.Property<string>("New");
                                b.Property<string>("Key");
                                b.Property<string>("Indexed");
                                b.Index("Indexed").Unique();
                                b.Key("Key");
                            });
                    });

            var steps = Assert.IsType<TableRebuildOperation>(operations[0]);

            Assert.Collection(steps.Operations,
                AssertRenameTemp(t),
                AssertCreateTable(t, new[] { "Key", "Altered", "Indexed", "New" }, new[] { "Key" }),
                AssertMoveData(t, new[] { "Key", "Altered", "Indexed" }, new[] { "Key", "Altered", "Indexed" }),
                AssertDropTemp(t),
                AssertCreateIndex(t, new[] { "Indexed" }, unique: true));
        }

        [Fact]
        public void DropColumn_to_TableRebuild()
        {
            var operations = Transform(migrate => { migrate.DropColumn("OldCol", "A"); }, model =>
                {
                    model.Entity("A", b =>
                        {
                            b.Property<string>("Col");
                            b.Key("Col");
                        });
                });

            var steps = Assert.IsType<TableRebuildOperation>(operations[0]);
            Assert.Collection(steps.Operations,
                AssertRenameTemp("A"),
                AssertCreateTable("A", new[] { "Col" }, new[] { "Col" }),
                AssertMoveData("A", new[] { "Col" }, new[] { "Col" }),
                AssertDropTemp("A"));
        }

        protected override SqliteOperationTransformer CreateTransformer()
            => new SqliteOperationTransformer(
                new ModelDiffer(
                    new SqliteTypeMapper(),
                    new SqliteMetadataExtensionProvider(),
                    new MigrationAnnotationProvider()));
    }
}

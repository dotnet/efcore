// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Xunit;
using Microsoft.Data.Entity.Migrations.Builders;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteOperationTransformTest
    {
        [Fact]
        public void DropColumn_to_TableRebuild()
        {
            var builder = new MigrationBuilder();
            builder.DropColumn("col1", "A");
            builder.AlterColumn("col2", "A", "TEXT", nullable: true);
            builder.AddColumn("col3", "A", "TEXT");

            var model = new ModelBuilder(new ConventionSet(), new Model());
            model.Entity("a", b =>
                {
                    b.ToSqliteTable("A");
                    b.Property<string>("2").HasSqliteColumnName("col2");
                    b.Property<string>("3").HasColumnName("col3");
                    b.Index("2").Unique();
                    b.Key("3");
                });
            var transformer = new SqliteOperationTransformer(
                new ModelDiffer(
                    new SqliteTypeMapper(),
                    new SqliteMetadataExtensionProvider(),
                    new MigrationAnnotationProvider()));

            var actual = transformer.Transform(builder.Operations.ToList(), model.Model);

            Assert.Collection(actual, op1 =>
                {
                    var rename = Assert.IsType<RenameTableOperation>(op1);
                    Assert.Equal("A", rename.Name);
                    Assert.Equal("A_temp", rename.NewName);
                },
                op2 =>
                    {
                        var create = Assert.IsType<CreateTableOperation>(op2);
                        Assert.Equal("A", create.Name);
                        Assert.Equal(2, create.Columns.Count);
                        Assert.Collection(create.PrimaryKey.Columns, s => { Assert.Equal(s, "col3"); });
                    },
                op3 =>
                    {
                        var move = Assert.IsType<MoveDataOperation>(op3);
                        Assert.Equal("A_temp", move.OldTable);
                        Assert.Equal("A", move.NewTable);
                        Assert.Equal(new[] { "col3", "col2" }, move.Columns);
                    },
                op4 =>
                    {
                        var drop = Assert.IsType<DropTableOperation>(op4);
                        Assert.Equal("A_temp", drop.Name);
                    },
                op5 =>
                    {
                        var index = Assert.IsType<CreateIndexOperation>(op5);
                        Assert.Equal(true, index.IsUnique);
                        Assert.Collection(index.Columns, s => { Assert.Equal(s, "col2"); });
                    });
        }
    }
}

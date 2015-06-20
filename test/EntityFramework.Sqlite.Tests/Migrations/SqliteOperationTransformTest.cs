// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteOperationTransformTest
    {
        [Fact]
        public void DropColumn_to_TableRebuild()
        {
            var input = new DropColumnOperation { Name = "col1", Table = "A" };
            var model = new ModelBuilder(new ConventionSet(), new Model());
            model.Entity("a", b =>
                {
                    b.SqliteTable("A");
                    b.Property<string>("2").SqliteColumn("col2");
                    b.Property<string>("3").Column("col3");
                    b.Key("3");
                });
            var transformer = new SqliteOperationTransformer(
                new ModelDiffer(new SqliteTypeMapper(), new SqliteMetadataExtensionProvider()));
            var actual = transformer.Transform(new[] { input }, model.Model);

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
                        Assert.Equal(new [] { "col3" }, create.PrimaryKey.Columns);
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
                    });
        }
    }
}

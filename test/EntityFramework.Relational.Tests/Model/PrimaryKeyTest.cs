// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class PrimaryKeyTest
    {
        [Fact]
        public void Create_and_initialize_primary_key()
        {
            var table = new Table("dbo.MyTable");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var primaryKey = new PrimaryKey(
                "MyPrimaryKey", new[] { column0, column1, }, isClustered: true);

            Assert.Equal("MyPrimaryKey", primaryKey.Name);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(2, primaryKey.Columns.Count);
            Assert.Same(column0, primaryKey.Columns[0]);
            Assert.Same(column1, primaryKey.Columns[1]);
            Assert.Same(table, primaryKey.Table);
            Assert.True(primaryKey.IsClustered);
        }

        public void Clone_replicates_instance_and_adds_column_clones_to_cache()
        {
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var primaryKey = new PrimaryKey("PK", new[] { column0, column1 }, isClustered: false);

            var cloneContext = new CloneContext();
            var clone = primaryKey.Clone(cloneContext);

            Assert.NotSame(primaryKey, clone);
            Assert.Equal("PK", clone.Name);
            Assert.Equal(2, clone.Columns.Count);
            Assert.NotSame(column0, clone.Columns[0]);
            Assert.NotSame(column1, clone.Columns[1]);
            Assert.Equal("Foo", clone.Columns[0].Name);
            Assert.Equal("Bar", clone.Columns[1].Name);
            Assert.False(clone.IsClustered);

            Assert.Same(clone.Columns[0], cloneContext.GetOrAdd(column0, () => null));
            Assert.Same(clone.Columns[1], cloneContext.GetOrAdd(column1, () => null));
        }

        public void Clone_gets_column_clones_from_cache()
        {
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var primaryKey = new PrimaryKey("PK", new[] { column0, column1 }, isClustered: false);
            
            var cloneContext = new CloneContext();
            var columnClone0 = column0.Clone(cloneContext);
            var columnClone1 = column1.Clone(cloneContext);
            var clone = primaryKey.Clone(cloneContext);

            Assert.NotSame(primaryKey, clone);
            Assert.Equal(2, clone.Columns.Count);
            Assert.Same(columnClone0, clone.Columns[0]);
            Assert.Same(columnClone1, clone.Columns[1]);
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class IndexTest
    {
        [Fact]
        public void Create_and_initialize_index()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });
            var index = new Index("MyIndex", new[] { column1 }, isUnique: true, isClustered: true);

            Assert.Equal("MyIndex", index.Name);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(1, index.Columns.Count);
            Assert.Same(column1, index.Columns[0]);
            Assert.True(index.IsUnique);
            Assert.True(index.IsClustered);
        }

        [Fact]
        public void Can_set_name()
        {
            var index = new Index("IX", new[] { new Column("Foo", typeof(int)) });

            Assert.Equal("IX", index.Name);

            index.Name = "RenamedIX";

            Assert.Equal("RenamedIX", index.Name);
        }

        public void Clone_replicates_instance_and_adds_column_clones_to_cache()
        {
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var index = new Index("IX", new[] { column0, column1 }, isUnique: true, isClustered: true);

            var cloneContext = new CloneContext();
            var clone = index.Clone(cloneContext);

            Assert.NotSame(index, clone);
            Assert.Equal("IX", clone.Name);
            Assert.Equal(2, clone.Columns.Count);
            Assert.NotSame(column0, clone.Columns[0]);
            Assert.NotSame(column1, clone.Columns[1]);
            Assert.Equal("Foo", clone.Columns[0].Name);
            Assert.Equal("Bar", clone.Columns[1].Name);
            Assert.True(clone.IsUnique);
            Assert.True(clone.IsClustered);

            Assert.Same(clone.Columns[0], cloneContext.GetOrAdd(column0, () => null));
            Assert.Same(clone.Columns[1], cloneContext.GetOrAdd(column1, () => null));
        }

        public void Clone_gets_column_clones_from_cache()
        {
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var index = new Index("IX", new[] { column0, column1 }, isUnique: true, isClustered: true);

            var cloneContext = new CloneContext();
            var columnClone0 = column0.Clone(cloneContext);
            var columnClone1 = column1.Clone(cloneContext);
            var clone = index.Clone(cloneContext);

            Assert.NotSame(index, clone);
            Assert.Equal(2, clone.Columns.Count);
            Assert.Same(columnClone0, clone.Columns[0]);
            Assert.Same(columnClone1, clone.Columns[1]);
        }
    }
}

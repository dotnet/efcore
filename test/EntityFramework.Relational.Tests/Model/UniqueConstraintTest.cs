// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class UniqueConstraintTest
    {
        [Fact]
        public void Create_and_initialize_unique_constraint()
        {
            var table = new Table("dbo.MyTable");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var uniqueConstraint = new UniqueConstraint(
                "MyUniqueConstraint", new[] { column0, column1 });

            Assert.Equal("MyUniqueConstraint", uniqueConstraint.Name);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(2, uniqueConstraint.Columns.Count);
            Assert.Same(column0, uniqueConstraint.Columns[0]);
            Assert.Same(column1, uniqueConstraint.Columns[1]);
            Assert.Same(table, uniqueConstraint.Table);
        }

        public void Clone_replicates_instance_and_adds_column_clones_to_cache()
        {
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var uniqueConstraint = new UniqueConstraint("PK", new[] { column0, column1 });

            var cloneContext = new CloneContext();
            var clone = uniqueConstraint.Clone(cloneContext);

            Assert.NotSame(uniqueConstraint, clone);
            Assert.Equal("PK", clone.Name);
            Assert.Equal(2, clone.Columns.Count);
            Assert.NotSame(column0, clone.Columns[0]);
            Assert.NotSame(column1, clone.Columns[1]);
            Assert.Equal("Foo", clone.Columns[0].Name);
            Assert.Equal("Bar", clone.Columns[1].Name);

            Assert.Same(clone.Columns[0], cloneContext.GetOrAdd(column0, () => null));
            Assert.Same(clone.Columns[1], cloneContext.GetOrAdd(column1, () => null));
        }

        public void Clone_gets_column_clones_from_cache()
        {
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var uniqueConstraint = new UniqueConstraint("PK", new[] { column0, column1 });

            var cloneContext = new CloneContext();
            var columnClone0 = column0.Clone(cloneContext);
            var columnClone1 = column1.Clone(cloneContext);
            var clone = uniqueConstraint.Clone(cloneContext);

            Assert.NotSame(uniqueConstraint, clone);
            Assert.Equal(2, clone.Columns.Count);
            Assert.Same(columnClone0, clone.Columns[0]);
            Assert.Same(columnClone1, clone.Columns[1]);
        }
    }
}

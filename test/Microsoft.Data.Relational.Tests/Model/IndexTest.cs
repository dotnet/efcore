// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
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
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class ColumnTests
    {
        [Fact]
        public void Table_gets_parent_table()
        {
            var table = new Table("Table");
            var column = new Column("Column", "int");

            Assert.Null(column.Table);

            table.AddColumn(column);

            Assert.Same(table, column.Table);
        }
    }
}

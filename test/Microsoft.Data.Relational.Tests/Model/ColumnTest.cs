// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class ColumnTest
    {
        [Fact]
        public void Create_and_initialize_column()
        {
            var column = new Column("Foo", "int")
                { IsNullable = true, DefaultValue = 5 };

            Assert.Equal("Foo", column.Name);
            Assert.Null(column.ClrType);
            Assert.Equal("int", column.DataType);
            Assert.True(column.IsNullable);
            Assert.Equal(5, column.DefaultValue);
            Assert.Null(column.DefaultSql);

            column = new Column("Bar", typeof(int), null)
                { IsNullable = false, DefaultSql = "GETDATE()" };

            Assert.Equal("Bar", column.Name);
            Assert.Same(typeof(int), column.ClrType);
            Assert.Null(column.DataType);
            Assert.False(column.IsNullable);
            Assert.Null(column.DefaultValue);
            Assert.Equal("GETDATE()", column.DefaultSql);
        }
    }
}

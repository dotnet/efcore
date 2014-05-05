// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
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

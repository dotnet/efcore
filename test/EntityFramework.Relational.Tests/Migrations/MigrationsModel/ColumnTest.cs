// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class ColumnTest
    {
        [Fact]
        public void Create_and_initialize_column()
        {
            var column = new Column("Foo", typeof(int))
                { IsNullable = true, DefaultValue = 5 };

            Assert.Equal("Foo", column.Name);
            Assert.Same(typeof(int), column.ClrType);
            Assert.Null(column.DataType);
            Assert.True(column.IsNullable);
            Assert.Equal(5, column.DefaultValue);
            Assert.Null(column.DefaultSql);
        }

        [Fact]
        public void Can_set_name()
        {
            var column = new Column("Foo", typeof(int));

            Assert.Equal("Foo", column.Name);

            column.Name = "Bar";

            Assert.Equal("Bar", column.Name);
        }
    }
}

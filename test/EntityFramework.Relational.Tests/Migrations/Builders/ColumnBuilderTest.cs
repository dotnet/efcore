// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Builders
{
    public class ColumnBuilderTest
    {
        [Fact]
        public void Computed_is_set_on_column()
        {
            var columnBuilder = new ColumnBuilder();

            Assert.True(columnBuilder.Binary(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Boolean(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Byte(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Decimal(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Double(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Guid(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Single(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Short(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Int(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Long(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.String(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.Time(defaultSql: "Sql", computed: true).IsComputed);
            Assert.True(columnBuilder.DateTimeOffset(defaultSql: "Sql", computed: true).IsComputed);
        }
    }
}

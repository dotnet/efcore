// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqliteMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_srid()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Geometry)
                .Metadata;

            Assert.Null(property.GetSqliteSrid());

            property.SetSqliteSrid(1); 

            Assert.Equal(1, property.GetSqliteSrid());

            property.SetSqliteSrid(null);

            Assert.Null(property.GetSqliteSrid());
        }

        [Fact]
        public void Can_get_and_set_dimension()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Geometry)
                .Metadata;

            Assert.Null(property.GetSqliteDimension());

            property.SetSqliteDimension("Z");

            Assert.Equal("Z", property.GetSqliteDimension());

            property.SetSqliteDimension(null);

            Assert.Null(property.GetSqliteDimension());
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Geometry { get; set; }
        }
    }
}

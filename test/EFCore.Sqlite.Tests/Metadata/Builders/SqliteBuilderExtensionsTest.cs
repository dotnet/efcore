// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class SqliteBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_srid()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Geometry)
                .ForSqliteHasSrid(1)
                .Metadata;

            Assert.Equal(1, property.GetSqliteSrid());
        }

        [Fact]
        public void Can_set_srid_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Geometry")
                .ForSqliteHasSrid(1)
                .Metadata;

            Assert.Equal(1, property.GetSqliteSrid());
        }

        [Fact]
        public void Can_set_srid_convention()
        {
            var modelBuilder = ((IConventionModel)CreateConventionModelBuilder().Model).Builder;

            var property = modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Geometry")
                .ForSqliteHasSrid(1)
                .Metadata;

            Assert.Equal(1, property.GetSqliteSrid());
        }

        [Fact]
        public void Can_set_dimension_convention()
        {
            var modelBuilder = ((IConventionModel)CreateConventionModelBuilder().Model).Builder;

            var property = modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Geometry")
                .ForSqliteHasDimension("Z")
                .Metadata;

            Assert.Equal("Z", property.GetSqliteDimension());
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
            => SqliteTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            public int Id { get; set; }
            public string Geometry { get; set; }
        }
    }
}

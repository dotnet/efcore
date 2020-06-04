// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class SqliteBuilderExtensionsTest
    {
        [ConditionalFact]
        public void Can_set_srid()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Geometry)
                .HasSrid(1);

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Geometry)
                .Metadata;

            Assert.Equal(1, property.GetSrid());
        }

        [ConditionalFact]
        public void Can_set_srid_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property<string>("Geometry")
                .HasSrid(1);

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Geometry")
                .Metadata;

            Assert.Equal(1, property.GetSrid());
        }

        [ConditionalFact]
        public void Can_set_srid_convention()
        {
            var modelBuilder = ((IConventionModel)CreateConventionModelBuilder().Model).Builder;

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Geometry")
                .HasSrid(1);

            var property = modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Geometry")
                .Metadata;

            Assert.Equal(1, property.GetSrid());
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

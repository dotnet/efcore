// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class SqliteBuilderExtensionsTest
    {
        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_srid(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Geometry)
                    .ForSqliteHasSrid(1);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Geometry)
                    .HasSrid(1);
            }

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Geometry)
                .Metadata;

            Assert.Equal(1, property.GetSrid());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_srid_non_generic(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property<string>("Geometry")
                    .ForSqliteHasSrid(1);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property<string>("Geometry")
                    .HasSrid(1);
            }

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Geometry")
                .Metadata;

            Assert.Equal(1, property.GetSrid());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_srid_convention(bool obsolete)
        {
            var modelBuilder = ((IConventionModel)CreateConventionModelBuilder().Model).Builder;

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(string), "Geometry")
                    .ForSqliteHasSrid(1);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(string), "Geometry")
                    .HasSrid(1);
            }

            var property = modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Geometry")
                .Metadata;

            Assert.Equal(1, property.GetSrid());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_dimension_convention(bool obsolete)
        {
            var modelBuilder = ((IConventionModel)CreateConventionModelBuilder().Model).Builder;

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(string), "Geometry")
                    .ForSqliteHasDimension("Z");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(string), "Geometry")
                    .HasGeometricDimension("Z");
            }

            var property = modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Geometry")
                .Metadata;

            Assert.Equal("Z", property.GetGeometricDimension());
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

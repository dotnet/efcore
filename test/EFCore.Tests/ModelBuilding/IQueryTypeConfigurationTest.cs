// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public class IQueryTypeConfigurationTest
        {
            [Fact]
            public void Configure_query_not_already_in_model()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.ApplyConfiguration(new CustomerConfiguration());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.NotNull(entityType);
            }

            [Fact]
            public void Configure_query_already_in_model()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Query<Customer>();
                builder.ApplyConfiguration(new CustomerConfiguration());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(200, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            [Fact]
            public void Override_config_in_query_type_configuration()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Query<Customer>().Property(c => c.Name).HasMaxLength(500);
                builder.ApplyConfiguration(new CustomerConfiguration());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(200, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            [Fact]
            public void Override_config_after_query_type_configuration()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.ApplyConfiguration(new CustomerConfiguration());
                builder.Query<Customer>().Property(c => c.Name).HasMaxLength(500);

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(500, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            [Fact]
            public void Apply_multiple_query_type_configurations()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.ApplyConfiguration(new CustomerConfiguration());
                builder.ApplyConfiguration(new CustomerConfiguration2());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(1000, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            [Fact]
            public void Can_use_shadow_property_for_fk_on_queryType()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Entity<Value>();
                builder.Query<QueryResult>().HasOne(x => x.Value).WithMany().HasForeignKey("DataValueId");

                var fk = Assert.Single(builder.Model.FindEntityType(typeof(QueryResult)).GetForeignKeys());
                Assert.Equal("DataValueId", fk.Properties[0].Name);
            }

            [Fact]
            public void Can_use_clr_property_for_fk_on_queryType()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Entity<Value>();
                builder.Query<QueryResult>().HasOne(x => x.Value).WithMany().HasForeignKey(e => e.ValueFk);

                var fk = Assert.Single(builder.Model.FindEntityType(typeof(QueryResult)).GetForeignKeys());
                Assert.Equal("ValueFk", fk.Properties[0].Name);
            }

            [Fact]
            public void Can_use_different_clr_property_for_fk_on_queryType()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Entity<Value>();
                builder.Query<QueryCoreResult>().HasOne(x => x.Value).WithMany().HasForeignKey(e => e.ValueFk);

                var fk = Assert.Single(builder.Model.FindEntityType(typeof(QueryCoreResult)).GetForeignKeys());
                Assert.Equal("ValueFk", fk.Properties[0].Name);
            }

            [Fact]
            public void Can_use_alternate_key()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Entity<Value>();
                builder.Query<QueryResult>().HasOne(x => x.Value).WithMany().HasPrincipalKey(e => e.AlternateId);

                var fk = Assert.Single(builder.Model.FindEntityType(typeof(QueryResult)).GetForeignKeys());
                Assert.Equal("AlternateId", fk.PrincipalKey.Properties[0].Name);
            }

            private class CustomerConfiguration : IQueryTypeConfiguration<Customer>
            {
                public void Configure(QueryTypeBuilder<Customer> builder)
                {
                    builder.Property(c => c.Name).HasMaxLength(200);
                }
            }

            private class CustomerConfiguration2 : IQueryTypeConfiguration<Customer>
            {
                public void Configure(QueryTypeBuilder<Customer> builder)
                {
                    builder.Property(c => c.Name).HasMaxLength(1000);
                }
            }
        }
    }
}

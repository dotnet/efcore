// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public class IEntityTypeConfigurationTest
        {
            [Fact]
            public void Configure_entity_not_already_in_model()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.ApplyConfiguration(new CustomerConfiguration());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.NotNull(entityType);
                Assert.Equal(nameof(Customer.AlternateKey), entityType.GetKeys().Single().Properties.Single().Name);
            }

            [Fact]
            public void Configure_entity_already_in_model()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Entity<Customer>();
                builder.ApplyConfiguration(new CustomerConfiguration());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(nameof(Customer.AlternateKey), entityType.GetKeys().Single().Properties.Single().Name);
            }

            [Fact]
            public void Override_config_in_entity_type_configuration()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.Entity<Customer>().Property(c => c.Name).HasMaxLength(500);
                builder.ApplyConfiguration(new CustomerConfiguration());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(200, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            [Fact]
            public void Override_config_after_entity_type_configuration()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.ApplyConfiguration(new CustomerConfiguration());
                builder.Entity<Customer>().Property(c => c.Name).HasMaxLength(500);

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(500, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            [Fact]
            public void Apply_multiple_entity_type_configurations()
            {
                var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

                builder.ApplyConfiguration(new CustomerConfiguration());
                builder.ApplyConfiguration(new CustomerConfiguration2());

                var entityType = builder.Model.FindEntityType(typeof(Customer));
                Assert.Equal(nameof(Customer.AlternateKey), entityType.GetKeys().Single().Properties.Single().Name);
                Assert.Equal(1000, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
            }

            private class CustomerConfiguration : IEntityTypeConfiguration<Customer>
            {
                public void Configure(EntityTypeBuilder<Customer> builder)
                {
                    builder.HasKey(c => c.AlternateKey);

                    builder.Property(c => c.Name).HasMaxLength(200);
                }
            }

            private class CustomerConfiguration2 : IEntityTypeConfiguration<Customer>
            {
                public void Configure(EntityTypeBuilder<Customer> builder)
                {
                    builder.Property(c => c.Name).HasMaxLength(1000);
                }
            }
        }
    }
}

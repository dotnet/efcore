// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class CosmosModelValidatorTest : ModelValidatorTestBase
    {
        [ConditionalFact]
        public virtual void Passes_on_valid_model()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>();

            var model = modelBuilder.Model;
            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_partition_keys()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(o => o.PartitionId)
                .Property(o => o.PartitionId).HasConversion<string>();

            var model = modelBuilder.Model;
            Validate(model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_partition_key_property()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>().HasPartitionKey("PartitionKey");

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.PartitionKeyMissingProperty(typeof(Order).Name, "PartitionKey"), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_partition_key_on_first_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders");
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NoPartitionKey(typeof(Customer).Name, "Orders"), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_partition_keys_one_last_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders");

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NoPartitionKey(typeof(Order).Name, "Orders"), model);
        }

        [ConditionalFact]
        public virtual void Detects_partition_keys_mapped_to_different_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId)
                .Property(c => c.PartitionId).ToJsonProperty("pk");
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);

            var model = modelBuilder.Model;
            VerifyError(
                CosmosStrings.PartitionKeyStoreNameMismatch(
                    nameof(Customer.PartitionId), typeof(Customer).Name, "pk", nameof(Order.PartitionId), typeof(Order).Name,
                    nameof(Order.PartitionId)), model);
        }

        [ConditionalFact]
        public virtual void Detects_partition_key_of_different_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(o => o.PartitionId)
                .Property(c => c.PartitionId).HasConversion<int>();

            var model = modelBuilder.Model;
            VerifyError(
                CosmosStrings.PartitionKeyNonStringStoreType(
                    nameof(Customer.PartitionId), typeof(Order).Name, "int"), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasNoDiscriminator();
            modelBuilder.Entity<Order>().ToContainer("Orders");

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NoDiscriminatorProperty(typeof(Customer).Name, "Orders"), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_value()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasDiscriminator().HasValue(null);
            modelBuilder.Entity<Order>().ToContainer("Orders");

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NoDiscriminatorValue(typeof(Customer).Name, "Orders"), model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasDiscriminator().HasValue("type");
            modelBuilder.Entity<Order>().ToContainer("Orders").HasDiscriminator().HasValue("type");

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.DuplicateDiscriminatorValue(typeof(Order).Name, "type", typeof(Customer).Name, "Orders"), model);
        }

        protected override TestHelpers TestHelpers => CosmosTestHelpers.Instance;

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PartitionId { get; set; }
            public ICollection<Order> Orders { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }
            public string PartitionId { get; set; }
            public Customer Customer { get; set; }
            public OrderDetails OrderDetails { get; set; }
        }

        [Owned]
        private class OrderDetails
        {
            public string ShippingAddress { get; set; }
        }
    }
}

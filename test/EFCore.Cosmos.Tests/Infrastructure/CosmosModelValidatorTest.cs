// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
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
        public virtual void Passes_on_valid_keyless_entity_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().HasPartitionKey(c => c.PartitionId).HasNoKey();

            var model = Validate(modelBuilder.Model);

            Assert.Empty(model.FindEntityType(typeof(Customer)).GetKeys());
        }

        [ConditionalFact]
        public virtual void Detects_missing_id_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.HasKey(o => o.Id);
                    b.Ignore(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NoIdProperty(typeof(Order).Name), model);
        }

        [ConditionalFact]
        public virtual void Detects_non_key_id_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.HasKey(o => o.Id);
                    b.Property<string>("id");
                    b.Ignore(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NoIdKey(typeof(Order).Name, "id"), model);
        }

        [ConditionalFact]
        public virtual void Detects_non_string_id_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.HasKey(o => o.Id);
                    b.Property<int>("id");
                    b.HasKey("id");
                    b.Ignore(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.IdNonStringStoreType("id", typeof(Order).Name, "int"), model);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_partition_keys()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(o => o.PartitionId)
                .Property(o => o.PartitionId).HasConversion<string>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_PK_partition_key()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.HasKey(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_non_key_partition_key_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.Property<string>("id");
                    b.HasKey("id");
                    b.Property(o => o.PartitionId);
                    b.HasPartitionKey(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            VerifyError(CosmosStrings.NoPartitionKeyKey(typeof(Order).Name, nameof(Order.PartitionId), "id"), modelBuilder.Model);
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
        public virtual void Detects_properties_mapped_to_same_property()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>(
                ob =>
                {
                    ob.Property(o => o.Id).ToJsonProperty("Details");
                    ob.Property(o => o.PartitionId).ToJsonProperty("Details");
                });

            var model = modelBuilder.Model;
            VerifyError(
                CosmosStrings.JsonPropertyCollision(
                    nameof(Order.PartitionId), nameof(Order.Id), typeof(Order).Name, "Details"), model);
        }

        [ConditionalFact]
        public virtual void Detects_property_and_embedded_type_mapped_to_same_property()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>(
                ob =>
                {
                    ob.Property(o => o.PartitionId).ToJsonProperty("Details");
                    ob.OwnsOne(o => o.OrderDetails).ToJsonProperty("Details");
                });

            var model = modelBuilder.Model;
            VerifyError(
                CosmosStrings.JsonPropertyCollision(
                    nameof(Order.OrderDetails), nameof(Order.PartitionId), typeof(Order).Name, "Details"), model);
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

        [ConditionalFact]
        public virtual void Passes_on_valid_concurrency_token()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>()
                .ToContainer("Orders")
                .Property<string>("_etag")
                .IsConcurrencyToken();

            var model = modelBuilder.Model;
            Validate(model);
        }

        [ConditionalFact]
        public virtual void Detects_invalid_concurrency_token()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>()
                .ToContainer("Orders")
                .Property<string>("_not_etag")
                .IsConcurrencyToken();

            var model = modelBuilder.Model;
            VerifyError(CosmosStrings.NonETagConcurrencyToken(typeof(Customer).Name, "_not_etag"), model);
        }

        protected override TestHelpers TestHelpers
            => CosmosTestHelpers.Instance;
    }
}

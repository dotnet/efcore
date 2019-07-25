// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class CosmosBuilderExtensionsTest
    {
        [ConditionalFact]
        public void Can_get_and_set_collection_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>();

            Assert.Equal(nameof(DbContext), entityType.Metadata.GetCosmosContainer());

            entityType.ToContainer("Customizer");
            Assert.Equal("Customizer", entityType.Metadata.GetCosmosContainer());

            entityType.ToContainer(null);
            Assert.Equal(nameof(DbContext), entityType.Metadata.GetCosmosContainer());

            modelBuilder.HasDefaultContainer("Unicorn");
            Assert.Equal("Unicorn", entityType.Metadata.GetCosmosContainer());
        }

        [ConditionalFact]
        public void Can_get_and_set_partition_key_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var entityTypeBuilder = modelBuilder.Entity<Customer>();
            var entityType = entityTypeBuilder.Metadata;

            ((IConventionEntityType)entityType).Builder.HasPartitionKey("pk");
            Assert.Equal("pk", entityType.GetCosmosPartitionKeyPropertyName());
            Assert.Equal(ConfigurationSource.Convention,
                ((IConventionEntityType)entityType).GetCosmosPartitionKeyPropertyNameConfigurationSource());

            entityTypeBuilder.HasPartitionKey("pk");
            Assert.Equal("pk", entityType.GetCosmosPartitionKeyPropertyName());
            Assert.Equal(ConfigurationSource.Explicit,
                ((IConventionEntityType)entityType).GetCosmosPartitionKeyPropertyNameConfigurationSource());

            Assert.False(((IConventionEntityType)entityType).Builder.CanSetPartitionKey("partition"));

            entityTypeBuilder.HasPartitionKey(null);
            Assert.Null(entityType.GetCosmosPartitionKeyPropertyName());
            Assert.Null(((IConventionEntityType)entityType).GetCosmosPartitionKeyPropertyNameConfigurationSource());
        }

        [ConditionalFact]
        public void Default_container_name_is_used_if_not_set()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            modelBuilder.HasDefaultContainer(null);

            Assert.Equal(nameof(Customer), entityType.GetCosmosContainer());
            Assert.Null(modelBuilder.Model.GetCosmosDefaultContainer());

            modelBuilder.HasDefaultContainer("db0");

            Assert.Equal("db0", entityType.GetCosmosContainer());
            Assert.Equal("db0", modelBuilder.Model.GetCosmosDefaultContainer());

            modelBuilder
                .Entity<Customer>()
                .ToContainer("db1");

            Assert.Equal("db1", entityType.GetCosmosContainer());
        }

        [ConditionalFact]
        public void Default_discriminator_can_be_removed()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Entity<Customer>();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Discriminator", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(nameof(Customer), entityType.GetDiscriminatorValue());

            modelBuilder.Entity<Customer>().HasNoDiscriminator();

            Assert.Null(entityType.GetDiscriminatorProperty());
            Assert.Null(entityType.GetDiscriminatorValue());

            modelBuilder.Entity<Customer>().HasBaseType<object>();

            Assert.Equal("Discriminator", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(nameof(Customer), entityType.GetDiscriminatorValue());

            modelBuilder.Entity<Customer>().HasBaseType((string)null);

            Assert.Null(entityType.GetDiscriminatorProperty());
        }

        protected virtual ModelBuilder CreateConventionModelBuilder() => CosmosTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public short SomeShort { get; set; }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class CosmosMetadataExtensionsTest
    {
        [ConditionalFact]
        public void Can_get_and_set_collection_name()
        {
            var modelBuilder = CreateModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>().Metadata;

            Assert.Equal(nameof(Customer), entityType.GetCosmosContainerName());

            ((IConventionEntityType)entityType).SetCosmosContainerName("Customizer");
            Assert.Equal("Customizer", entityType.GetCosmosContainerName());
            Assert.Equal(ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetCosmosContainerNameConfigurationSource());

            entityType.SetCosmosContainerName("Customizer");
            Assert.Equal("Customizer", entityType.GetCosmosContainerName());
            Assert.Equal(ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetCosmosContainerNameConfigurationSource());

            entityType.SetCosmosContainerName(null);
            Assert.Equal(nameof(Customer), entityType.GetCosmosContainerName());
            Assert.Null(((IConventionEntityType)entityType).GetCosmosContainerNameConfigurationSource());

            ((IConventionModel)modelBuilder.Model).Builder.ForCosmosHasDefaultContainerName("Unicorn");
            Assert.Equal("Unicorn", entityType.GetCosmosContainerName());
        }

        [ConditionalFact]
        public void Can_get_and_set_partition_key_name()
        {
            var modelBuilder = CreateModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>().Metadata;

            Assert.Null(entityType.GetCosmosPartitionKeyPropertyName());

            ((IConventionEntityType)entityType).SetCosmosPartitionKeyPropertyName("pk");
            Assert.Equal("pk", entityType.GetCosmosPartitionKeyPropertyName());
            Assert.Equal(ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetCosmosPartitionKeyPropertyNameConfigurationSource());

            entityType.SetCosmosPartitionKeyPropertyName("pk");
            Assert.Equal("pk", entityType.GetCosmosPartitionKeyPropertyName());
            Assert.Equal(ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetCosmosPartitionKeyPropertyNameConfigurationSource());

            entityType.SetCosmosPartitionKeyPropertyName(null);
            Assert.Null(entityType.GetCosmosPartitionKeyPropertyName());
            Assert.Null(((IConventionEntityType)entityType).GetCosmosPartitionKeyPropertyNameConfigurationSource());
        }

        private static ModelBuilder CreateModelBuilder() => new ModelBuilder(new ConventionSet());

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Guid AlternateId { get; set; }
        }
    }
}

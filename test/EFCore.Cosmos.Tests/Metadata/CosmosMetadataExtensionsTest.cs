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

            Assert.Equal(nameof(Customer), entityType.GetContainer());

            ((IConventionEntityType)entityType).SetContainer("Customizer");
            Assert.Equal("Customizer", entityType.GetContainer());
            Assert.Equal(ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetContainerConfigurationSource());

            entityType.SetContainer("Customizer");
            Assert.Equal("Customizer", entityType.GetContainer());
            Assert.Equal(ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetContainerConfigurationSource());

            entityType.SetContainer(null);
            Assert.Equal(nameof(Customer), entityType.GetContainer());
            Assert.Null(((IConventionEntityType)entityType).GetContainerConfigurationSource());

            ((IConventionModel)modelBuilder.Model).Builder.HasDefaultContainer("Unicorn");
            Assert.Equal("Unicorn", entityType.GetContainer());
        }

        [ConditionalFact]
        public void Can_get_and_set_partition_key_name()
        {
            var modelBuilder = CreateModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>().Metadata;

            Assert.Null(entityType.GetPartitionKeyPropertyName());

            ((IConventionEntityType)entityType).SetPartitionKeyPropertyName("pk");
            Assert.Equal("pk", entityType.GetPartitionKeyPropertyName());
            Assert.Equal(
                ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());

            entityType.SetPartitionKeyPropertyName("pk");
            Assert.Equal("pk", entityType.GetPartitionKeyPropertyName());
            Assert.Equal(
                ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());

            entityType.SetPartitionKeyPropertyName(null);
            Assert.Null(entityType.GetPartitionKeyPropertyName());
            Assert.Null(((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());
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

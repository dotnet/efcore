// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CosmosMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_collection_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>();

            Assert.Equal(nameof(Customer), entityType.Metadata.GetCosmosContainerName());

            entityType.ForCosmosToContainer("Customizer");
            Assert.Equal("Customizer", entityType.Metadata.GetCosmosContainerName());

            entityType.ForCosmosToContainer(null);
            Assert.Equal(nameof(Customer), entityType.Metadata.GetCosmosContainerName());

            modelBuilder.ForCosmosHasDefaultContainerName("Unicorn");
            Assert.Equal("Unicorn", entityType.Metadata.GetCosmosContainerName());
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Guid AlternateId { get; set; }
        }
    }
}

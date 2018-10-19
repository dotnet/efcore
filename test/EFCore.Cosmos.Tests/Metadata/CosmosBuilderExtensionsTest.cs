// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata
{
    public class CosmosBuilderExtensionsTest
    {
        [Fact]
        public void Default_container_name_is_used_if_not_set()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.Cosmos().ContainerName);
            Assert.Null(modelBuilder.Model.Cosmos().DefaultContainerName);

            modelBuilder.HasDefaultContainerName("db0");

            Assert.Equal("db0", entityType.Cosmos().ContainerName);
            Assert.Equal("db0", modelBuilder.Model.Cosmos().DefaultContainerName);

            modelBuilder
                .Entity<Customer>()
                .ToContainer("db1");

            Assert.Equal("db1", entityType.Cosmos().ContainerName);
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

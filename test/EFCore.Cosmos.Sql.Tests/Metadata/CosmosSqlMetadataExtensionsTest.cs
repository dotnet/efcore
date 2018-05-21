// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CosmosSqlMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_collection_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Unicorn", entityType.CosmosSql().CollectionName);

            entityType.CosmosSql().CollectionName = "Customizer";
            Assert.Equal("Customizer", entityType.CosmosSql().CollectionName);

            entityType.CosmosSql().CollectionName = null;
            Assert.Equal("Unicorn", entityType.CosmosSql().CollectionName);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Guid AlternateId { get; set; }
        }
    }
}

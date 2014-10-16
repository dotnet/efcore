// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class AtsMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.AzureTableStorage().Column);
            Assert.Equal("Name", ((IProperty)property).AzureTableStorage().Column);

            property.AzureTableStorage().Column = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.AzureTableStorage().Column);
            Assert.Equal("Eman", ((IProperty)property).AzureTableStorage().Column);

            property.AzureTableStorage().Column = null;

            Assert.Equal("Name", property.AzureTableStorage().Column);
            Assert.Equal("Name", ((IProperty)property).AzureTableStorage().Column);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.AzureTableStorage().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).AzureTableStorage().Table);

            entityType.AzureTableStorage().Table = "Customizer";

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).AzureTableStorage().Table);

            entityType.AzureTableStorage().Table = null;

            Assert.Equal("Customer", entityType.AzureTableStorage().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).AzureTableStorage().Table);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}

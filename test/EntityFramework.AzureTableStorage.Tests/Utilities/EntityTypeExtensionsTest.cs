// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Utilities
{
    public class EntityTypeExtensionsTest
    {
        [Fact]
        public void Can_lookup_by_storage_name()
        {
            var entityType = new EntityType("Customer");
            var property = entityType.AddProperty("Name", typeof(string));
            property.SetColumnName("FirstName");
            Assert.Equal(property, entityType.GetPropertyByStorageName("FirstName"));
            Assert.Equal(property, entityType.TryGetPropertyByStorageName("FirstName"));
        }

        [Fact]
        public void Lookup_by_storage_name_returns_null()
        {
            var entityType = new EntityType("Customer");
            entityType.AddProperty("Name", typeof(string));

            Assert.Equal(
                Strings.FormatPropertyWithStorageNameNotFound("FirstName", "Customer"),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetPropertyByStorageName("FirstName")).Message);
            Assert.Null(entityType.TryGetPropertyByStorageName("FirstName"));
        }
    }
}

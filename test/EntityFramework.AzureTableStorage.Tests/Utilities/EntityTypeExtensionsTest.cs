// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Utilities
{
    public class EntityTypeExtensionsTest
    {
        [Fact]
        public void Can_lookup_by_storage_name()
        {
            var entityType = new Model().AddEntityType("Customer");
            var property = entityType.GetOrAddProperty("Name", typeof(string), shadowProperty: true);
            property.AzureTableStorage().Column = "FirstName";
            Assert.Equal(property, entityType.GetPropertyByColumnName("FirstName"));
            Assert.Equal(property, entityType.TryGetPropertyByColumnName("FirstName"));
        }

        [Fact]
        public void Lookup_by_storage_name_returns_null()
        {
            var entityType = new Model().AddEntityType("Customer");
            entityType.GetOrAddProperty("Name", typeof(string), shadowProperty: true);

            Assert.Equal(
                Strings.FormatPropertyWithStorageNameNotFound("FirstName", "Customer"),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetPropertyByColumnName("FirstName")).Message);
            Assert.Null(entityType.TryGetPropertyByColumnName("FirstName"));
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityTypeExtensionsTest
    {
        [Fact]
        public void Can_lookup_by_storage_name()
        {
            var entityType = new EntityType("Customer");
            var property = entityType.AddProperty("Name", typeof(string));
            property.StorageName = "FirstName";
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

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var entityType = new EntityType("Customer");
            var modelMock = new Mock<Model>();
            entityType.Model = modelMock.Object;

            entityType.GetReferencingForeignKeys();

            modelMock.Verify(m => m.GetReferencingForeignKeys(entityType), Times.Once());
        }
    }
}

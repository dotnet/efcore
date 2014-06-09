// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.Metadata;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class AtsObjectArrayValueReaderFactoryTests
    {
        private readonly AtsValueReaderFactory _factory = new AtsValueReaderFactory();

        [Fact]
        public void It_gets_value_by_storage_name()
        {
            var entityType = new EntityType("TestType");
            var property = entityType.AddProperty("ClrName", typeof(object));
            property.StorageName = "StorageName";

            var data = new Dictionary<string, EntityProperty>
                {
                    { "StorageName", new EntityProperty(3987) },
                    { "ClrName", new EntityProperty(0) },
                };
            var buffer = new AtsNamedValueBuffer(data);

            var reader = _factory.Create(entityType, buffer);

            Assert.Equal(1, reader.Count);
            Assert.Equal(3987, reader.ReadValue<int>(0));
        }

        [Fact]
        public void Reader_does_not_contain_ignored_properties()
        {
            var entityType = new EntityType("TestType");
            entityType.AddProperty("Prop1", typeof(string));
            entityType.AddProperty("Prop2", typeof(int));
            var data = new Dictionary<string, EntityProperty>
                {
                    { "Prop2", new EntityProperty(3) },
                    { "IgnoreA", new EntityProperty(1) },
                    { "IgnoreB", new EntityProperty(2) },
                    { "Prop1", new EntityProperty("0") },
                    { "IgnoreZ", new EntityProperty(4) },
                };
            var buffer = new AtsNamedValueBuffer(data);

            var reader = _factory.Create(entityType, buffer);

            Assert.Equal(2, reader.Count);
            Assert.Equal("0", reader.ReadValue<string>(0));
            Assert.Equal(3, reader.ReadValue<int>(1));
        }

        [Fact]
        public void Reader_contains_nulls_for_unmatched_properties()
        {
            var entityType = new EntityType("TestType");
            entityType.AddProperty("Prop1", typeof(string));
            entityType.AddProperty("Prop2", typeof(int));
            var buffer = new AtsNamedValueBuffer(new Dictionary<string, EntityProperty>());
            var reader = _factory.Create(entityType, buffer);

            Assert.Equal(2,reader.Count);
            Assert.True(reader.IsNull(0));
            Assert.True(reader.IsNull(1));
        }
    }
}

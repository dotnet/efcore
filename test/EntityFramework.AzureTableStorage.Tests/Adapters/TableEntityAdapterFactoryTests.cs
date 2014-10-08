// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Adapters
{
    public class TableEntityAdapterFactoryTests
    {
        private EntityType _entityType;
        private readonly TableEntityAdapterFactory _factory;

        public TableEntityAdapterFactoryTests()
        {
            _factory = new TableEntityAdapterFactory();
            _entityType = new Model().AddEntityType(typeof(PocoTestType));
            _entityType.GetOrAddProperty("PartitionKey", typeof(string));
            _entityType.GetOrAddProperty("RowKey", typeof(string));
            _entityType.GetOrAddProperty("Timestamp", typeof(DateTimeOffset));
        }

        [Fact]
        public void It_creates_state_entry_adapter()
        {
            var entry = TestStateEntry.Mock().WithType(typeof(PocoTestType)).WithEntity(new PocoTestType());
            var adapter = _factory.CreateFromStateEntry(entry);

            Assert.NotNull(adapter);
            Assert.IsType<StateEntryTableEntityAdapter<PocoTestType>>(adapter);
        }
    }
}

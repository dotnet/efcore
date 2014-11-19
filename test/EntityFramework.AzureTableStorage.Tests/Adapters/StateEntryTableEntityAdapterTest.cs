// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Adapters
{
    public class StateEntryTableEntityAdapterTest
    {
        private static IServiceProvider CreateContextServices(IModel model)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .ServiceCollection
                .BuildServiceProvider();

            return ((IDbContextServices)new DbContext(serviceProvider, new DbContextOptions()
                .UseModel(model)
                .UseAzureTableStorage("Moria", "mellon"))).ScopedServiceProvider;
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);

            builder.Entity<ClrPoco>()
                .ForAzureTableStorage()
                .PartitionAndRowKey(s => s.PartitionKey, s => s.RowKey)
                .Timestamp(s => s.Timestamp);

            builder.Entity("ShadowEntity", pb =>
            {
                pb.Property<object>("PartitionKey");
                pb.Property<object>("RowKey");
                pb.Property<object>("Timestamp");
                pb.Property<object>("SomeProperty");
                pb.Key("PartitionKey", "RowKey");
            });

            builder.Entity<GuidKeysPoco>()
                .ForAzureTableStorage().PartitionAndRowKey(s => s.PartitionGuid, s => s.RowGuid)
                .Timestamp("Timestamp", true);

            builder.Entity<IntKeysPoco>()
                .ForAzureTableStorage().PartitionAndRowKey(s => s.PartitionID, s => s.RowID);

            builder.Entity<ClrPocoWithProp>(pb =>
            {
                pb.ForAzureTableStorage().PartitionAndRowKey(s => s.PartitionKey, s => s.RowKey);
                pb.Property(s => s.Timestamp);
                pb.Property(s => s.StringProp);
                pb.Property(s => s.IntProp);
            });

            return model;
        }

        [Fact]
        public void It_wraps_poco_in_adapter()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var obj = new ClrPoco();
            var entry = stateManager.GetOrCreateEntry(obj);
            var adapter = new StateEntryTableEntityAdapter<ClrPoco>(entry);
            Assert.Same(obj, adapter.Entity);
        }

        [Fact]
        public void It_writes_to_clr_properties()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var obj = new ClrPoco();
            var entry = stateManager.GetOrCreateEntry(obj);
            var adapter = new StateEntryTableEntityAdapter<ClrPoco>(entry);

            adapter.PartitionKey = "PartitionKey";
            adapter.RowKey = "RowKey";
            var timestamp = DateTime.Now;
            adapter.Timestamp = timestamp;

            Assert.Equal("PartitionKey", obj.PartitionKey);
            Assert.Equal("RowKey", obj.RowKey);
            Assert.Equal(timestamp, obj.Timestamp);
        }

        [Fact]
        public void It_writes_to_shadow_state_properties()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var entityType = model.GetEntityType("ShadowEntity");
            var entry = stateManager.GetOrMaterializeEntry(entityType, new AtsObjectArrayValueReader(new object[] { "PK", "RK", null, null }));
            var adapter = new StateEntryTableEntityAdapter<object>(entry);
            var pkProp = entry.EntityType.GetPropertyByColumnName("PartitionKey");
            var rkProp = entry.EntityType.GetPropertyByColumnName("RowKey");
            var tmProp = entry.EntityType.GetPropertyByColumnName("Timestamp");
            var someProp = entry.EntityType.GetPropertyByColumnName("SomeProperty");

            var timestamp = DateTimeOffset.UtcNow;
            adapter.Timestamp = timestamp;
            adapter.ReadEntity(new Dictionary<string, EntityProperty>
                {
                    { "SomeProperty", new EntityProperty(324890) }
                }, null);

            Assert.Equal("PK", entry[pkProp]);
            Assert.Equal("RK", entry[rkProp]);
            Assert.Equal(timestamp, entry[tmProp]);
            Assert.Equal(324890, entry[someProp]);
        }

        [Fact]
        public void It_reads_from_shadow_state_properties()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var entityType = model.GetEntityType("ShadowEntity");
            var entry = stateManager.GetOrMaterializeEntry(entityType, new AtsObjectArrayValueReader(new object[] { "PK", "RK", null, null }));
            var adapter = new StateEntryTableEntityAdapter<object>(entry);
            var tmProp = entry.EntityType.GetPropertyByColumnName("Timestamp");
            var someProp = entry.EntityType.GetPropertyByColumnName("SomeProperty");

            var timestamp = DateTimeOffset.UtcNow;
            entry[tmProp] = timestamp;
            entry[someProp] = 65134;

            var values = adapter.WriteEntity(null);

            Assert.Equal("PK", adapter.PartitionKey);
            Assert.Equal("RK", adapter.RowKey);
            Assert.Equal(timestamp, adapter.Timestamp);
            Assert.Equal(65134, values["SomeProperty"].Int32Value);
        }

        [Fact]
        public void It_casts_to_int_keys()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var data = new object[] { "42", "1980", "11/11/2011 11:11:11 PM" };
            var entityType = model.GetEntityType(typeof(IntKeysPoco));
            var entry = stateManager.GetOrMaterializeEntry(entityType, new AtsObjectArrayValueReader(data));
            var adapter = new StateEntryTableEntityAdapter<IntKeysPoco>(entry);

            var entity = adapter.Entity;
            Assert.Equal(42, entity.PartitionID);
            Assert.Equal(1980, entity.RowID);
        }

        [Fact]
        public void It_casts_from_int_keys()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var obj = new IntKeysPoco { PartitionID = 42, RowID = 1980 };
            var entry = stateManager.GetOrCreateEntry(obj);
            var adapter = new StateEntryTableEntityAdapter<IntKeysPoco>(entry);

            Assert.Equal("42", adapter.PartitionKey);
            Assert.Equal("1980", adapter.RowKey);
        }

        [Fact]
        public void It_interprets_guid_keys()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var data = new object[] { "80d401da-ef77-4bc6-a2b0-300025098a0e", "4b240e4f-b886-4d23-a63c-017a3d79885a", "timestamp" };
            var entityType = model.GetEntityType(typeof(GuidKeysPoco));
            var entry = stateManager.GetOrMaterializeEntry(entityType, new AtsObjectArrayValueReader(data));
            var adapter = new StateEntryTableEntityAdapter<GuidKeysPoco>(entry);

            Assert.Equal(new Guid("80d401da-ef77-4bc6-a2b0-300025098a0e"), adapter.Entity.PartitionGuid);
            Assert.Equal(new Guid("4b240e4f-b886-4d23-a63c-017a3d79885a"), adapter.Entity.RowGuid);
        }

        [Fact]
        public void It_reads_from_dictionary()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var data = new Dictionary<string, EntityProperty>
                {
                    { "StringProp", new EntityProperty("StringVal") },
                    { "IntProp", new EntityProperty(5) },
                };
            var instance = new ClrPocoWithProp();

            var entry = stateManager.GetOrCreateEntry(instance);
            var adapter = new StateEntryTableEntityAdapter<ClrPocoWithProp>(entry);

            adapter.ReadEntity(data, null);

            Assert.Equal("StringVal", instance.StringProp);
            Assert.Equal(5, instance.IntProp);
        }

        [Fact]
        public void It_skips_mismatched_types()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var data = new Dictionary<string, EntityProperty>
                {
                    { "StringProp", new EntityProperty(5) },
                    { "IntProp", new EntityProperty("string input") },
                };
            var instance = new ClrPocoWithProp();

            var entry = stateManager.GetOrCreateEntry(instance);
            var adapter = new StateEntryTableEntityAdapter<ClrPocoWithProp>(entry);

            adapter.ReadEntity(data, null);

            Assert.Equal(default(string), instance.StringProp);
            Assert.Equal(default(int), instance.IntProp);
        }

        [Fact]
        public void It_writes_to_dictionary()
        {
            var model = CreateModel();
            var stateManager = CreateContextServices(model).GetRequiredService<StateManager>();

            var instance = new ClrPocoWithProp
            {
                PartitionKey = "A",
                RowKey = "B",
                StringProp = "C",
                IntProp = 5,
                Timestamp = DateTimeOffset.UtcNow,
            };
            var entry = stateManager.GetOrCreateEntry(instance);
            var adapter = new StateEntryTableEntityAdapter<ClrPocoWithProp>(entry);

            var expected = new Dictionary<string, EntityProperty>
                {
                    { "IntProp", new EntityProperty(instance.IntProp) },
                    { "StringProp", new EntityProperty(instance.StringProp) },
                };

            Assert.Equal(expected, adapter.WriteEntity(null));
        }
    }
}

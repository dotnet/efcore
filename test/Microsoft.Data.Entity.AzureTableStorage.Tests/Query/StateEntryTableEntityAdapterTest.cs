// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class StateEntryTableEntityAdapterTest
    {
        private readonly StateEntryFactory _factory;

        public StateEntryTableEntityAdapterTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            var contextConfig = new DbContextConfiguration();
            contextConfig.Initialize(Mock.Of<IServiceProvider>(), ServiceProviderCache.Instance.GetOrAdd(new DbContextOptions()), Mock.Of<DbContextOptions>(), Mock.Of<DbContext>(), new DbContextConfiguration.ServiceProviderSource());
            _factory = new StateEntryFactory(contextConfig, new EntityMaterializerSource(new MemberMapper(new FieldMatcher())));
        }

        public class ClrPoco
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset Timestamp { get; set; }
        }

        public class GuidKeysPoco
        {
            public Guid PartitionGuid { get; set; }
            public Guid RowGuid { get; set; }
        }

        public class IntKeysPoco
        {
            public int PartitionID { get; set; }
            public int RowID { get; set; }
        }

        private IModel CreateModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);
            builder.Entity<ClrPoco>().AzureTableProperties(pb =>
                {
                    pb.PartitionKey(s => s.PartitionKey);
                    pb.RowKey(s => s.RowKey);
                    pb.Timestamp(s => s.Timestamp);
                })
                ;
            builder.Entity("ShadowEntity").Properties(pb =>
                {
                    pb.Property<object>("PartitionKey", true);
                    pb.Property<object>("RowKey", true);
                    pb.Property<object>("Timestamp", true);
                });
            builder.Entity<GuidKeysPoco>().AzureTableProperties(pb =>
                {
                    pb.PartitionKey(s => s.PartitionGuid);
                    pb.RowKey(s => s.RowGuid);
                    pb.Timestamp("Timestamp", true);
                });
            builder.Entity<IntKeysPoco>().AzureTableProperties(pb =>
                {
                    pb.PartitionKey(s => s.PartitionID);
                    pb.RowKey(s => s.RowID);
                    pb.Timestamp("Timestamp", true);
                });
            return model;
        }

        [Fact]
        public void It_wraps_poco_in_adapter()
        {
            var obj = new ClrPoco();
            var entityType = CreateModel().GetEntityType(typeof(ClrPoco));
            var entry = _factory.Create(entityType, obj);
            var adapter = new StateEntryTableEntityAdapter<ClrPoco>(entry);
            Assert.Same(obj, adapter.Entity);
        }

        [Fact]
        public void It_writes_to_clr_properties()
        {
            var obj = new ClrPoco();
            var entityType = CreateModel().GetEntityType(typeof(ClrPoco));
            var entry = _factory.Create(entityType, obj);
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
            var entityType = CreateModel().GetEntityType("ShadowEntity");
            var entry = _factory.Create(entityType, new AtsObjectArrayValueReader(new object[3]));
            var adapter = new StateEntryTableEntityAdapter<object>(entry);

            adapter.PartitionKey = "PartitionKey";
            adapter.RowKey = "RowKey";
            var timestamp = DateTime.Now;
            adapter.Timestamp = timestamp;

            Assert.Equal("PartitionKey", adapter.PartitionKey);
            Assert.Equal("RowKey", adapter.RowKey);
            Assert.Equal(timestamp, adapter.Timestamp);
        }

        [Fact]
        public void It_casts_to_int_keys()
        {
            var data = new object[] { "42", "1980", "11/11/2011 11:11:11 PM" };
            var entityType = CreateModel().GetEntityType(typeof(IntKeysPoco));
            var entry = _factory.Create(entityType, new AtsObjectArrayValueReader(data));
            var adapter = new StateEntryTableEntityAdapter<IntKeysPoco>(entry);

            var entity = adapter.Entity;
            Assert.Equal(42, entity.PartitionID);
            Assert.Equal(1980, entity.RowID);
        }

        [Fact]
        public void It_casts_from_int_keys()
        {
            var obj = new IntKeysPoco { PartitionID = 42, RowID = 1980 };
            var entityType = CreateModel().GetEntityType(typeof(IntKeysPoco));
            var entry = _factory.Create(entityType, obj);
            var adapter = new StateEntryTableEntityAdapter<IntKeysPoco>(entry);

            Assert.Equal("42", adapter.PartitionKey);
            Assert.Equal("1980", adapter.RowKey);
        }

        [Fact]
        public void It_interprets_guid_keys()
        {
            var data = new object[] { "80d401da-ef77-4bc6-a2b0-300025098a0e", "4b240e4f-b886-4d23-a63c-017a3d79885a", "timestamp" };
            var entityType = CreateModel().GetEntityType(typeof(GuidKeysPoco));
            var entry = _factory.Create(entityType, new AtsObjectArrayValueReader(data));
            var adapter = new StateEntryTableEntityAdapter<GuidKeysPoco>(entry);

            Assert.Equal(new Guid("80d401da-ef77-4bc6-a2b0-300025098a0e"), adapter.Entity.PartitionGuid);
            Assert.Equal(new Guid("4b240e4f-b886-4d23-a63c-017a3d79885a"), adapter.Entity.RowGuid);
        }
    }
}

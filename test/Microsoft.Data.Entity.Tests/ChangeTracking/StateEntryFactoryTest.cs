// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StateEntryFactoryTest
    {
        [Fact]
        public void Creates_shadow_state_only_entry_when_entity_is_fully_shadow_state()
        {
            var entityType = new EntityType("RedHook");
            entityType.AddProperty("Long", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType.AddProperty("Hammer", typeof(string), shadowProperty: true, concurrencyToken: false);

            var servicesMock = new Mock<ContextServices>();
            servicesMock.Setup(m => m.ClrPropertyGetterSource).Returns(new ClrPropertyGetterSource());
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.Services).Returns(servicesMock.Object);

            var entry = new StateEntryFactory(
                configurationMock.Object,
                new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))).Create(entityType, new Random());

            Assert.IsType<ShadowStateEntry>(entry);

            Assert.Same(configurationMock.Object, entry.Configuration);
            Assert.Same(entityType, entry.EntityType);
            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Creates_CLR_only_entry_when_entity_has_no_shadow_properties()
        {
            var entityType = new EntityType(typeof(RedHook));
            entityType.AddProperty("Long", typeof(int));
            entityType.AddProperty("Hammer", typeof(string));

            var servicesMock = new Mock<ContextServices>();
            servicesMock.Setup(m => m.ClrPropertyGetterSource).Returns(new ClrPropertyGetterSource());
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.Services).Returns(servicesMock.Object);

            var entity = new RedHook();
            var entry = new StateEntryFactory(
                configurationMock.Object,
                new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))).Create(entityType, entity);

            Assert.IsType<ClrStateEntry>(entry);

            Assert.Same(configurationMock.Object, entry.Configuration);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Creates_mixed_entry_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var entityType = new EntityType(typeof(RedHook));
            entityType.AddProperty("Long", typeof(int));
            entityType.AddProperty("Hammer", typeof(string), shadowProperty: true, concurrencyToken: false);

            var servicesMock = new Mock<ContextServices>();
            servicesMock.Setup(m => m.ClrPropertyGetterSource).Returns(new ClrPropertyGetterSource());
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.Services).Returns(servicesMock.Object);

            var entity = new RedHook();
            var entry = new StateEntryFactory(
                configurationMock.Object,
                new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))).Create(entityType, entity);

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(configurationMock.Object, entry.Configuration);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Creates_shadow_state_only_entry_from_value_buffer_when_entity_is_fully_shadow_state()
        {
            var entityType = new EntityType("RedHook");
            var property1 = entityType.AddProperty("Long", typeof(int), shadowProperty: true, concurrencyToken: false);
            var property2 = entityType.AddProperty("Hammer", typeof(string), shadowProperty: true, concurrencyToken: false);

            var servicesMock = new Mock<ContextServices>();
            servicesMock.Setup(m => m.ClrPropertyGetterSource).Returns(new ClrPropertyGetterSource());
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.Services).Returns(servicesMock.Object);

            var entry = new StateEntryFactory(configurationMock.Object, new EntityMaterializerSource(new MemberMapper(new FieldMatcher())))
                .Create(entityType, new ObjectArrayValueReader(new object[] { "Green", 77 }));

            Assert.IsType<ShadowStateEntry>(entry);

            Assert.Same(configurationMock.Object, entry.Configuration);
            Assert.Same(entityType, entry.EntityType);
            Assert.Equal(77, entry[property1]);
            Assert.Equal("Green", entry[property2]);
            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Creates_CLR_only_entry_from_value_buffer_when_entity_has_no_shadow_properties()
        {
            var entityType = new EntityType(typeof(RedHook));
            var property1 = entityType.AddProperty("Long", typeof(int));
            var property2 = entityType.AddProperty("Hammer", typeof(string));

            var servicesMock = new Mock<ContextServices>();
            servicesMock.Setup(m => m.ClrPropertyGetterSource).Returns(new ClrPropertyGetterSource());
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.Services).Returns(servicesMock.Object);

            var entry = new StateEntryFactory(configurationMock.Object, new EntityMaterializerSource(new MemberMapper(new FieldMatcher())))
                .Create(entityType, new ObjectArrayValueReader(new object[] { "Green", 77 }));

            Assert.IsType<ClrStateEntry>(entry);

            Assert.Same(configurationMock.Object, entry.Configuration);
            Assert.Same(entityType, entry.EntityType);
            Assert.Equal(77, entry[property1]);
            Assert.Equal("Green", entry[property2]);

            var entity = (RedHook)entry.Entity;
            Assert.Equal(77, entity.Long);
            Assert.Equal("Green", entity.Hammer);
        }

        [Fact]
        public void Creates_mixed_entry_from_value_buffer_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var entityType = new EntityType(typeof(RedHook));
            var property1 = entityType.AddProperty("Long", typeof(int));
            var property2 = entityType.AddProperty("Hammer", typeof(string), shadowProperty: true, concurrencyToken: false);

            var servicesMock = new Mock<ContextServices>();
            servicesMock.Setup(m => m.ClrPropertyGetterSource).Returns(new ClrPropertyGetterSource());
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.Services).Returns(servicesMock.Object);

            var entry = new StateEntryFactory(configurationMock.Object, new EntityMaterializerSource(new MemberMapper(new FieldMatcher())))
                .Create(entityType, new ObjectArrayValueReader(new object[] { "Green", 77 }));

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(configurationMock.Object, entry.Configuration);
            Assert.Same(entityType, entry.EntityType);
            Assert.Equal(77, entry[property1]);
            Assert.Equal("Green", entry[property2]);

            var entity = (RedHook)entry.Entity;
            Assert.Equal(77, entity.Long);
            Assert.Null(entity.Hammer);
        }

        private class RedHook
        {
            public int Long { get; set; }
            public string Hammer { get; set; }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class PropertyAccessorsFactoryTest
    {
        [ConditionalFact]
        public void Can_use_PropertyAccessorsFactory_on_indexed_property()
        {
            IMutableModel model = new Model();
            var entityType = model.AddEntityType(typeof(IndexedClass));
            var id = entityType.AddProperty("Id", typeof(int));
            var propertyA = entityType.AddIndexedProperty("PropertyA", typeof(string));
            model.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new IndexedClass();
            var entry = factory.Create(stateManager, entityType, entity);

            var propertyAccessors = new PropertyAccessorsFactory().Create(propertyA);
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.CurrentValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.OriginalValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.RelationshipSnapshotGetter)(entry));

            var valueBuffer = new ValueBuffer(new object[] { 1, "ValueA" });
            Assert.Equal("ValueA", propertyAccessors.ValueBufferGetter(valueBuffer));
        }

        [ConditionalFact]
        public void Can_use_PropertyAccessorsFactory_on_non_indexed_property()
        {
            IMutableModel model = new Model();
            var entityType = model.AddEntityType(typeof(NonIndexedClass));
            entityType.AddProperty("Id", typeof(int));
            var propA = entityType.AddProperty("PropA", typeof(string));
            model.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new NonIndexedClass();
            var entry = factory.Create(stateManager, entityType, entity);

            var propertyAccessors = new PropertyAccessorsFactory().Create(propA);
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.CurrentValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.OriginalValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.RelationshipSnapshotGetter)(entry));

            var valueBuffer = new ValueBuffer(new object[] { 1, "ValueA" });
            Assert.Equal("ValueA", propertyAccessors.ValueBufferGetter(valueBuffer));
        }

        private class IndexedClass
        {
            private readonly Dictionary<string, object> _internalValues = new Dictionary<string, object> { { "PropertyA", "ValueA" } };

            internal int Id { get; set; }

            public object this[string name] => _internalValues[name];
        }

        private class NonIndexedClass
        {
            internal int Id { get; set; }
            public string PropA { get; set; } = "ValueA";
        }
    }
}

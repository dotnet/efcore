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
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityTypeBuilder = modelBuilder.Entity<IndexedClass>();
            entityTypeBuilder.Property<int>("Id");
            var propertyA = entityTypeBuilder.IndexerProperty<string>("PropertyA").Metadata;

            var model = modelBuilder.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new IndexedClass();
            var entry = factory.Create(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

            var propertyAccessors = new PropertyAccessorsFactory().Create((IProperty)propertyA);
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
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityTypeBuilder = modelBuilder.Entity<NonIndexedClass>();
            entityTypeBuilder.Property<int>("Id");
            var propA = entityTypeBuilder.Property<string>("PropA").Metadata;

            var model = modelBuilder.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new NonIndexedClass();
            var entry = factory.Create(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

            var propertyAccessors = new PropertyAccessorsFactory().Create((IProperty)propA);
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.CurrentValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.OriginalValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry));
            Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.RelationshipSnapshotGetter)(entry));

            var valueBuffer = new ValueBuffer(new object[] { 1, "ValueA" });
            Assert.Equal("ValueA", propertyAccessors.ValueBufferGetter(valueBuffer));
        }

        private class IndexedClass
        {
            private readonly Dictionary<string, object> _internalValues = new() { { "PropertyA", "ValueA" } };

            internal int Id { get; set; }

            public object this[string name]
                => _internalValues[name];
        }

        private class NonIndexedClass
        {
            internal int Id { get; set; }
            public string PropA { get; set; } = "ValueA";
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class PropertyAccessorsFactoryTest
{
    [ConditionalFact]
    [Obsolete]
    public void Can_use_PropertyAccessorsFactory_on_indexed_property()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityTypeBuilder = modelBuilder.Entity<IndexedClass>();
        entityTypeBuilder.Property<int>("Id");
        var propertyA = entityTypeBuilder.IndexerProperty<string>("PropertyA").Metadata;

        var model = modelBuilder.FinalizeModel();

        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var entity = new IndexedClass();
        var entry = new InternalEntityEntry(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

        var propertyAccessors = PropertyAccessorsFactory.Instance.Create((IProperty)propertyA);
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.CurrentValueGetter)(entry));
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.OriginalValueGetter)(entry));
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry));
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.RelationshipSnapshotGetter)(entry));

        var valueBuffer = new ValueBuffer([1, "ValueA"]);
        Assert.Equal("ValueA", propertyAccessors.ValueBufferGetter(valueBuffer));
    }

    [ConditionalFact]
    [Obsolete]
    public void Can_use_PropertyAccessorsFactory_on_non_indexed_property()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityTypeBuilder = modelBuilder.Entity<NonIndexedClass>();
        entityTypeBuilder.Property<int>("Id");
        var propA = entityTypeBuilder.Property<string>("PropA").Metadata;

        var model = modelBuilder.FinalizeModel();

        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var entity = new NonIndexedClass();
        var entry = new InternalEntityEntry(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

        var propertyAccessors = PropertyAccessorsFactory.Instance.Create((IProperty)propA);
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.CurrentValueGetter)(entry));
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.OriginalValueGetter)(entry));
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry));
        Assert.Equal("ValueA", ((Func<InternalEntityEntry, string>)propertyAccessors.RelationshipSnapshotGetter)(entry));

        var valueBuffer = new ValueBuffer([1, "ValueA"]);
        Assert.Equal("ValueA", propertyAccessors.ValueBufferGetter(valueBuffer));
    }

    private class IndexedClass
    {
        private readonly Dictionary<string, object> _internalValues = new() { { "PropertyA", "ValueA" } };

        internal int Id { get; set; }

        internal object this[string name]
        {
            get => _internalValues[name];
            set => _internalValues[name] = value;
        }
    }

    private class NonIndexedClass
    {
        internal int Id { get; set; }
        public string PropA { get; set; } = "ValueA";
    }
}

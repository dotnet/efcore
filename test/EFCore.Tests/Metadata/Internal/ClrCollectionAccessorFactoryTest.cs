// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable EmptyConstructor
// ReSharper disable ConvertToAutoProperty
// ReSharper disable NotAccessedField.Local
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ClrCollectionAccessorFactoryTest
{
    [ConditionalFact]
    public void Delegate_accessor_is_returned_for_IEnumerable_navigation()
        => AccessorTest("AsIEnumerable", e => e.AsIEnumerable);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_for_ICollection_navigation()
        => AccessorTest("AsICollection", e => e.AsICollection);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_for_interface_navigation_derived_from_ICollection()
        => AccessorTest("AsIList", e => e.AsIList);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_for_concrete_generic_type_navigation()
        => AccessorTest("AsList", e => e.AsList);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_for_concrete_non_generic_type_navigation()
        => AccessorTest("AsMyCollection", e => e.AsMyCollection);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_no_setter()
        => AccessorTest("WithNoSetter", e => e.WithNoSetter);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_no_backing_field_found()
        => AccessorTest("NoBackingFound", e => e.NoBackingFound);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_no_public_constructor()
        => AccessorTest("AsMyPrivateCollection", e => e.AsMyPrivateCollection);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_no_internal_constructor()
        => AccessorTest("AsMyInternalCollection", e => e.AsMyInternalCollection);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_no_parameterless_constructor()
        => AccessorTest("AsMyUnavailableCollection", e => e.AsMyUnavailableCollection);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_auto_prop()
        => AccessorTest("AutoProp", e => e.AutoProp);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_read_only_prop()
        => AccessorTest("ReadOnlyProp", e => e.ReadOnlyProp);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_read_only_auto_prop()
        => AccessorTest("ReadOnlyAutoProp", e => e.ReadOnlyAutoProp);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_read_only_field_prop()
        => AccessorTest("ReadOnlyFieldProp", e => e.ReadOnlyFieldProp);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_write_only_prop()
        => AccessorTest("WriteOnlyProp", e => e.ReadWriteOnlyProp);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_prop_no_field_found()
        => AccessorTest("FullPropNoField", e => e.FullPropNoField);

    [ConditionalFact]
    public void Delegate_accessor_is_returned_when_read_only_prop_no_field_found()
        => AccessorTest("ReadOnlyPropNoField", e => e.ReadOnlyPropNoField);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_with_no_setter()
        => AccessorTest("WithNoSetter", e => e.WithNoSetter, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections()
        => AccessorTest("AsICollection", e => e.AsICollection, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_for_interface_navigation_derived_from_ICollection()
        => AccessorTest("AsIList", e => e.AsIList, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_for_concrete_generic_type_navigation()
        => AccessorTest("AsList", e => e.AsList, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_for_concrete_non_generic_type_navigation()
        => AccessorTest("AsMyCollection", e => e.AsMyCollection, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_auto_prop()
        => AccessorTest("AutoProp", e => e.AutoProp, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_read_only_prop()
        => AccessorTest("ReadOnlyProp", e => e.ReadOnlyProp, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_write_only_prop()
        => AccessorTest("WriteOnlyProp", e => e.ReadWriteOnlyProp, initializeCollections: false);

    [ConditionalFact]
    public void Delegate_accessor_handles_uninitialized_collections_prop_no_field_found()
        => AccessorTest("FullPropNoField", e => e.FullPropNoField, initializeCollections: false);

    private void AccessorTest(
        string navigationName,
        Func<MyEntity, IEnumerable<MyOtherEntity>> reader,
        bool initializeCollections = true)
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation(navigationName));

        var entity = new MyEntity(initializeCollections);

        var value = new MyOtherEntity();

        Assert.False(accessor.Contains(entity, value));
        accessor.Remove(entity, value);

        accessor.Add(entity, value, forMaterialization: false);

        Assert.True(accessor.Contains(entity, value));
        Assert.Single(reader(entity));

        accessor.Remove(entity, value);

        Assert.False(accessor.Contains(entity, value));
        Assert.Empty(reader(entity));
    }

    [ConditionalFact]
    public void Delegate_accessor_always_creates_collections_that_use_reference_equality_comparer()
    {
        IMutableModel model = new Model();
        var entityType = model.AddEntityType(typeof(MyEntity));
        var otherType = model.AddEntityType(typeof(MyEntityWithCustomComparer));
        var foreignKey = otherType.AddForeignKey(
            otherType.AddProperty("MyEntityId", typeof(int)),
            entityType.SetPrimaryKey(entityType.AddProperty("Id", typeof(int))),
            entityType);

        var navigation = foreignKey.SetPrincipalToDependent(
            typeof(MyEntity).GetProperty(
                nameof(MyEntity.AsICollectionWithCustomComparer),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

        RunConvention(navigation);

        var accessor = ClrCollectionAccessorFactory.Instance.Create((INavigation)navigation);

        var entity = new MyEntity(initialize: false);
        var value = new MyEntityWithCustomComparer { Id = 1 };

        Assert.False(accessor.Contains(entity, value));

        accessor.Add(entity, value, forMaterialization: false);

        value.Id = 42;

        accessor.Add(entity, value, forMaterialization: false);

        Assert.Equal(1, entity.AsICollectionWithCustomComparer.Count);
    }

    [ConditionalFact]
    public void Creating_accessor_for_navigation_without_getter_and_no_backing_field_throws()
    {
        var navigation = CreateNavigation("WriteOnlyPropNoField");

        Assert.Equal(
            CoreStrings.NoFieldOrGetter("WriteOnlyPropNoField", typeof(MyEntity).Name),
            Assert.Throws<InvalidOperationException>(() => ClrCollectionAccessorFactory.Instance.Create(navigation)).Message);
    }

    [ConditionalFact]
    public void Add_for_enumerable_backed_by_non_collection_throws()
        => Enumerable_backed_by_non_collection_throws((a, e, v) => a.Add(e, v, forMaterialization: false));

    [ConditionalFact]
    public void Contains_for_enumerable_backed_by_non_collection_throws()
        => Enumerable_backed_by_non_collection_throws((a, e, v) => a.Contains(e, v));

    [ConditionalFact]
    public void Remove_for_enumerable_backed_by_non_collection_throws()
        => Enumerable_backed_by_non_collection_throws((a, e, v) => a.Remove(e, v));

    [ConditionalFact]
    public void GetOrCreate_for_enumerable_backed_by_non_collection_throws()
        => Enumerable_backed_by_non_collection_throws((a, e, v) => a.GetOrCreate(e, forMaterialization: false));

    private void Enumerable_backed_by_non_collection_throws(Action<IClrCollectionAccessor, MyEntity, MyOtherEntity> test)
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation("AsIEnumerableNotCollection"));

        var entity = new MyEntity(initialize: true);
        var value = new MyOtherEntity();

        Assert.Equal(
            CoreStrings.NavigationBadType(
                "AsIEnumerableNotCollection", typeof(MyEntity).Name, typeof(MyEnumerable).Name, typeof(MyOtherEntity).Name),
            Assert.Throws<InvalidOperationException>(() => test(accessor, entity, value)).Message);
    }

    [ConditionalFact]
    public void Creating_accessor_for_array_navigation_throws()
    {
        var navigation = CreateNavigation("AsArray");

        Assert.Equal(
            CoreStrings.NavigationArray("AsArray", typeof(MyEntity).Name, typeof(MyOtherEntity[]).Name),
            Assert.Throws<InvalidOperationException>(() => ClrCollectionAccessorFactory.Instance.Create(navigation)).Message);
    }

    [ConditionalFact]
    public void Initialization_for_navigation_without_backing_field_throws()
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation("NoBackingFound"));

        Assert.Equal(
            CoreStrings.NavigationNoSetter("NoBackingFound", typeof(MyEntity).Name),
            Assert.Throws<InvalidOperationException>(
                () => accessor.Add(new MyEntity(false), new MyOtherEntity(), forMaterialization: false)).Message);
    }

    [ConditionalFact]
    public void Initialization_for_read_only_navigation_without_backing_field_throws()
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation("ReadOnlyPropNoField"));

        Assert.Equal(
            CoreStrings.NavigationNoSetter("ReadOnlyPropNoField", typeof(MyEntity).Name),
            Assert.Throws<InvalidOperationException>(
                () => accessor.Add(new MyEntity(false), new MyOtherEntity(), forMaterialization: false)).Message);
    }

    [ConditionalFact]
    public void Initialization_for_read_only_auto_prop_navigation()
        => AccessorTest("ReadOnlyAutoProp", e => e.ReadOnlyAutoProp, initializeCollections: false);

    [ConditionalFact]
    public void Initialization_for_read_only_navigation_backed_by_readonly_field()
        => AccessorTest("ReadOnlyFieldProp", e => e.ReadOnlyFieldProp, initializeCollections: false);

    [ConditionalFact]
    public void Initialization_for_navigation_with_private_constructor_throws()
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation("AsMyPrivateCollection"));

        Assert.Equal(
            CoreStrings.NavigationCannotCreateType("AsMyPrivateCollection", typeof(MyEntity).Name, typeof(MyPrivateCollection).Name),
            Assert.Throws<InvalidOperationException>(
                () => accessor.Add(new MyEntity(false), new MyOtherEntity(), forMaterialization: false)).Message);
    }

    [ConditionalFact]
    public void Initialization_for_navigation_with_internal_constructor_throws()
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation("AsMyInternalCollection"));

        Assert.Equal(
            CoreStrings.NavigationCannotCreateType("AsMyInternalCollection", typeof(MyEntity).Name, typeof(MyInternalCollection).Name),
            Assert.Throws<InvalidOperationException>(
                () => accessor.Add(new MyEntity(false), new MyOtherEntity(), forMaterialization: false)).Message);
    }

    [ConditionalFact]
    public void Initialization_for_navigation_without_parameterless_constructor_throws()
    {
        var accessor = ClrCollectionAccessorFactory.Instance.Create(CreateNavigation("AsMyUnavailableCollection"));

        Assert.Equal(
            CoreStrings.NavigationCannotCreateType(
                "AsMyUnavailableCollection", typeof(MyEntity).Name, typeof(MyUnavailableCollection).Name),
            Assert.Throws<InvalidOperationException>(
                () => accessor.Add(new MyEntity(false), new MyOtherEntity(), forMaterialization: false)).Message);
    }

    private INavigation CreateNavigation(string navigationName)
    {
        IMutableModel model = new Model();
        var entityType = model.AddEntityType(typeof(MyEntity));
        var otherType = model.AddEntityType(typeof(MyOtherEntity));
        var foreignKey = otherType.AddForeignKey(
            otherType.AddProperty("MyEntityId", typeof(int)),
            entityType.SetPrimaryKey(entityType.AddProperty("Id", typeof(int))),
            entityType);

        var navigation = foreignKey.SetPrincipalToDependent(
            typeof(MyEntity).GetProperty(navigationName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

        RunConvention(navigation);

        return (INavigation)navigation;
    }

    private void RunConvention(IMutableNavigation navigation)
    {
        var context = new ConventionContext<IConventionNavigationBuilder>(
            ((ForeignKey)navigation.ForeignKey).DeclaringEntityType.Model.ConventionDispatcher);

        new BackingFieldConvention(CreateDependencies())
            .ProcessNavigationAdded(((IConventionNavigation)navigation).Builder, context);
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private class MyEntity
    {
        public static readonly PropertyInfo AsICollectionProperty = typeof(MyEntity).GetProperty(
            nameof(AsICollection), BindingFlags.NonPublic | BindingFlags.Instance);

        private ICollection<MyOtherEntity> _asICollection;
        private ICollection<MyEntityWithCustomComparer> _asICollectionOfEntitiesWithCustomComparer;
        private IList<MyOtherEntity> _asIList;
        private List<MyOtherEntity> _asList;
        private MyCollection _myCollection;
        private readonly ICollection<MyOtherEntity> _withNoBackingFieldFound;
        private readonly ICollection<MyOtherEntity> _withNoSetter;
        private ICollection<MyOtherEntity> _withNoGetter;
        private IEnumerable<MyOtherEntity> _enumerable;
        private IEnumerable<MyOtherEntity> _enumerableNotCollection;
        private MyOtherEntity[] _array;
        private MyPrivateCollection _privateCollection;
        private MyInternalCollection _internalCollection;
        private MyUnavailableCollection _unavailableCollection;
        private readonly IEnumerable<MyOtherEntity> _readOnlyProp;
        private readonly IEnumerable<MyOtherEntity> _readOnlyFieldProp;
        private IEnumerable<MyOtherEntity> _writeOnlyProp;
        private IEnumerable<MyOtherEntity> _fullPropNoFieldNotFound;
        private readonly IEnumerable<MyOtherEntity> _readOnlyPropNoFieldNotFound;
        private IEnumerable<MyOtherEntity> _writeOnlyPropNoFieldNotFound;

        public MyEntity()
            : this(false)
        {
        }

        public MyEntity(bool initialize)
        {
            if (initialize)
            {
                _asICollection = new HashSet<MyOtherEntity>();
                _asICollectionOfEntitiesWithCustomComparer = new HashSet<MyEntityWithCustomComparer>();
                _asIList = new List<MyOtherEntity>();
                _asList = [];
                _myCollection = [];
                _withNoBackingFieldFound = new HashSet<MyOtherEntity>();
                _withNoSetter = new HashSet<MyOtherEntity>();
                _withNoGetter = new HashSet<MyOtherEntity>();
                _enumerable = new HashSet<MyOtherEntity>();
                _enumerableNotCollection = new MyEnumerable();
                _array = [];
                _privateCollection = MyPrivateCollection.Create();
                _internalCollection = [];
                _unavailableCollection = new MyUnavailableCollection(true);
                AutoProp = new HashSet<MyOtherEntity>();
                ReadOnlyAutoProp = new HashSet<MyOtherEntity>();
                _readOnlyProp = new HashSet<MyOtherEntity>();
                _readOnlyFieldProp = new HashSet<MyOtherEntity>();
                _writeOnlyProp = new HashSet<MyOtherEntity>();
                _fullPropNoFieldNotFound = new HashSet<MyOtherEntity>();
                _readOnlyPropNoFieldNotFound = new HashSet<MyOtherEntity>();
                _writeOnlyPropNoFieldNotFound = new HashSet<MyOtherEntity>();
            }
        }

        internal ICollection<MyOtherEntity> AsICollection
        {
            get => _asICollection;
            set => _asICollection = value;
        }

        internal ICollection<MyEntityWithCustomComparer> AsICollectionWithCustomComparer
        {
            get => _asICollectionOfEntitiesWithCustomComparer;
            set => _asICollectionOfEntitiesWithCustomComparer = value;
        }

        internal IList<MyOtherEntity> AsIList
        {
            get => _asIList;
            set => _asIList = value;
        }

        internal List<MyOtherEntity> AsList
        {
            get => _asList;
            set => _asList = value;
        }

        internal MyCollection AsMyCollection
        {
            get => _myCollection;
            set => _myCollection = value;
        }

        internal ICollection<MyOtherEntity> WithNoSetter
            => _withNoSetter;

        internal ICollection<MyOtherEntity> NoBackingFound
            => _withNoBackingFieldFound;

        internal ICollection<MyOtherEntity> WithNoGetter
        {
            set => _withNoGetter = value;
        }

        internal IEnumerable<MyOtherEntity> AsIEnumerable
        {
            get => _enumerable;
            set => _enumerable = value;
        }

        internal IEnumerable<MyOtherEntity> AsIEnumerableNotCollection
        {
            get => _enumerableNotCollection;
            set => _enumerableNotCollection = value;
        }

        internal MyOtherEntity[] AsArray
        {
            get => _array;
            set => _array = value;
        }

        internal MyPrivateCollection AsMyPrivateCollection
        {
            get => _privateCollection;
            set => _privateCollection = value;
        }

        internal MyInternalCollection AsMyInternalCollection
        {
            get => _internalCollection;
            set => _internalCollection = value;
        }

        internal MyUnavailableCollection AsMyUnavailableCollection
        {
            get => _unavailableCollection;
            set => _unavailableCollection = value;
        }

        internal IEnumerable<MyOtherEntity> AutoProp { get; set; }

        internal IEnumerable<MyOtherEntity> ReadOnlyProp
            => _readOnlyProp;

        internal IEnumerable<MyOtherEntity> ReadOnlyAutoProp { get; }

        internal IEnumerable<MyOtherEntity> ReadOnlyFieldProp
            => _readOnlyFieldProp;

        internal IEnumerable<MyOtherEntity> WriteOnlyProp
        {
            set => _writeOnlyProp = value;
        }

        internal IEnumerable<MyOtherEntity> ReadWriteOnlyProp
            => _writeOnlyProp;

        internal IEnumerable<MyOtherEntity> FullPropNoField
        {
            get => _fullPropNoFieldNotFound;
            set => _fullPropNoFieldNotFound = value;
        }

        internal IEnumerable<MyOtherEntity> ReadOnlyPropNoField
            => _readOnlyPropNoFieldNotFound;

        internal IEnumerable<MyOtherEntity> WriteOnlyPropNoField
        {
            set => _writeOnlyPropNoFieldNotFound = value;
        }

        internal IEnumerable<MyOtherEntity> ReadWriteOnlyPropNoField
            => _writeOnlyPropNoFieldNotFound;
    }

    private class MyOtherEntity;

    private class MyEntityWithCustomComparer
    {
        public int Id { get; set; }

        public override bool Equals(object obj)
            => obj is MyEntityWithCustomComparer other && Id == other.Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }

    private class MyCollection : List<MyOtherEntity>;

    private class MyPrivateCollection : List<MyOtherEntity>
    {
        private MyPrivateCollection()
        {
        }

        public static MyPrivateCollection Create()
            => [];
    }

    private class MyInternalCollection : List<MyOtherEntity>
    {
        internal MyInternalCollection()
        {
        }
    }

#pragma warning disable CS9113 // Parameter '_' is unread
    private class MyUnavailableCollection(bool _) : List<MyOtherEntity>;
#pragma warning restore CS9113

    private class MyEnumerable : IEnumerable<MyOtherEntity>
    {
        public IEnumerator<MyOtherEntity> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

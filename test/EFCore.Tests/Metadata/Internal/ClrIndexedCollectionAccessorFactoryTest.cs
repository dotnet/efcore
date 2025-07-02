// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ClrIndexedCollectionAccessorFactoryTest
{
    [Fact]
    public void Can_create_accessor_for_complex_collection_property()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);

        Assert.NotNull(accessor);
    }

    [Fact]
    public void Returns_null_for_non_collection_property()
    {
        var property = CreateScalarProperty();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);

        Assert.Null(accessor);
    }

    [Fact]
    public void Can_get_item_from_collection()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>>
        {
            Items =
            [
                new() { Number = 10 },
                new() { Number = 20 },
                new() { Number = 30 }
            ]
        };

        var value = accessor.Get(entity, 1);
        Assert.Equal(20, ((TestComplexType)value!).Number);

        var genericAccessor = Assert.IsType<ClrIndexedCollectionAccessor<TestEntity<List<TestComplexType>>, TestComplexType>>(accessor);

        var genericValue = genericAccessor.Get(entity, 1);

        Assert.Equal(((TestComplexType)value!).Number, genericValue.Number);
    }

    [Fact]
    public void Can_set_item_in_collection()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>>
        {
            Items = [
            new() { Number = 10 },
            new() { Number = 20 },
            new() { Number = 30 }
        ]
        };

        var genericAccessor = Assert.IsType<ClrIndexedCollectionAccessor<TestEntity<List<TestComplexType>>, TestComplexType>>(accessor);

        var newValue = new TestComplexType { Number = 42 };

        genericAccessor.Set(entity, 1, newValue, forMaterialization: false);

        Assert.Equal(42, entity.Items[1].Number);
    }

    [Fact]
    public void Can_set_item_for_materialization()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>>
        {
            Items = [
            new() { Number = 10 },
            new() { Number = 20 },
            new() { Number = 30 }
        ]
        };

        var genericAccessor = Assert.IsType<ClrIndexedCollectionAccessor<TestEntity<List<TestComplexType>>, TestComplexType>>(accessor);

        var newValue = new TestComplexType { Number = 42 };

        genericAccessor.Set(entity, 1, newValue, forMaterialization: true);

        Assert.Equal(42, entity.Items[1].Number);
    }

    [Fact]
    public void Get_throws_for_null_entity()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);

        Assert.Throws<NullReferenceException>(() => accessor.Get(null, 0));
    }

    [Fact]
    public void Set_throws_for_null_entity()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var newValue = new TestComplexType { Number = 42 };

        Assert.Throws<NullReferenceException>(() => accessor.Set(null, 0, newValue, false));
    }

    [Fact]
    public void Get_throws_for_index_out_of_range()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>> { Items = [new() { Number = 10 }] };

        Assert.Throws<ArgumentOutOfRangeException>(() => accessor.Get(entity, 5));
    }

    [Fact]
    public void Set_throws_for_index_out_of_range()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>> { Items = [new() { Number = 10 }] };
        var newValue = new TestComplexType { Number = 42 };

        Assert.Throws<ArgumentOutOfRangeException>(() => accessor.Set(entity, 5, newValue, false));
    }

    [Fact]
    public void Get_throws_for_null_collection()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>> { Items = null };

        Assert.Throws<NullReferenceException>(() => accessor.Get(entity, 0));
    }

    [Fact]
    public void Set_throws_for_null_collection()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>> { Items = null };
        var newValue = new TestComplexType { Number = 42 };

        Assert.Throws<NullReferenceException>(() => accessor.Set(entity, 0, newValue, false));
    }

    [Fact]
    public void Throws_with_empty_collection()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>> { Items = [] };
        var newValue = new TestComplexType { Number = 42 };

        Assert.Throws<ArgumentOutOfRangeException>(() => accessor.Get(entity, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => accessor.Set(entity, 0, newValue, false));
    }

    [Fact]
    public void Can_handle_arrays()
    {
        var property = CreateComplexCollectionProperty<TestComplexType[]>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<TestComplexType[]>
        {
            Items =
            [
                new TestComplexType { Number = 10 },
                new TestComplexType { Number = 20 },
                new TestComplexType { Number = 30 }
            ]
        };

        var value = accessor.Get(entity, 1);
        Assert.Equal(20, ((TestComplexType)value!).Number);

        var newValue = new TestComplexType { Number = 42 };
        accessor.Set(entity, 1, newValue, false);
        Assert.Equal(42, entity.Items[1].Number);

        var genericAccessor = Assert.IsType<ClrIndexedCollectionAccessor<TestEntity<TestComplexType[]>, TestComplexType>>(accessor);

        var genericValue = genericAccessor.Get(entity, 1);
        Assert.Equal(42, genericValue.Number);

        var anotherValue = new TestComplexType { Number = 99 };
        genericAccessor.Set(entity, 2, anotherValue, false);
        Assert.Equal(99, entity.Items[2].Number);

        var updatedValue = genericAccessor.Get(entity, 2);
        Assert.Equal(99, updatedValue.Number);
    }

    [Fact]
    public void Can_handle_custom_collections()
    {
        var property = CreateComplexCollectionProperty<MyList>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<MyList>
        {
            Items =
            [
                new TestComplexType { Number = 10 },
                new TestComplexType { Number = 20 },
                new TestComplexType { Number = 30 }
            ]
        };

        var value = accessor.Get(entity, 1);
        Assert.Equal(20, ((TestComplexType)value!).Number);

        var newValue = new TestComplexType { Number = 42 };
        accessor.Set(entity, 1, newValue, false);
        Assert.Equal(42, entity.Items[1].Number);

        var genericAccessor = Assert.IsType<ClrIndexedCollectionAccessor<TestEntity<MyList>, TestComplexType>>(accessor);

        var genericValue = genericAccessor.Get(entity, 1);
        Assert.Equal(42, genericValue.Number);

        var anotherValue = new TestComplexType { Number = 99 };
        genericAccessor.Set(entity, 2, anotherValue, false);
        Assert.Equal(99, entity.Items[2].Number);

        var updatedValue = genericAccessor.Get(entity, 2);
        Assert.Equal(99, updatedValue.Number);
    }

    [Fact]
    public void Can_handle_readonly_property()
    {
        var property = CreateReadOnlyComplexCollectionProperty();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntityWithReadOnlyList([
            new() { Number = 10 },
            new() { Number = 20 }
        ]);

        var value = accessor.Get(entity, 0);
        Assert.Equal(10, ((TestComplexType)value!).Number);

        var newValue = new TestComplexType { Number = 42 };
        accessor.Set(entity, 0, newValue, true);
        Assert.Equal(42, entity.ReadOnlyItems[0].Number);
    }

    [Fact]
    public void Can_handle_generic_list_types()
    {
        var property = CreateComplexCollectionProperty<List<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<List<TestComplexType>>
        {
            Items =
            [
                new() { Number = 10 },
                new() { Number = 20 },
                new() { Number = 30 }
            ]
        };

        var value = accessor.Get(entity, 2);
        Assert.Equal(30, ((TestComplexType)value!).Number);

        var newValue = new TestComplexType { Number = 99 };
        accessor.Set(entity, 0, newValue, false);
        Assert.Equal(99, entity.Items[0].Number);
    }

    [Fact]
    public void Can_handle_IList_interface()
    {
        var property = CreateComplexCollectionProperty<IList<TestComplexType>>();
        var accessor = ClrIndexedCollectionAccessorFactory.Instance.Create(property);
        var entity = new TestEntity<IList<TestComplexType>>
        {
            Items = [
                new() { Number = 10 },
                new() { Number = 20 }
            ]
        };

        var value = accessor.Get(entity, 1);
        Assert.Equal(20, ((TestComplexType)value!).Number);

        var newValue = new TestComplexType { Number = 99 };
        accessor.Set(entity, 0, newValue, false);
        Assert.Equal(99, entity.Items[0].Number);
    }

    private static IComplexProperty CreateComplexCollectionProperty<T>()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityTypeBuilder = modelBuilder.Entity<TestEntity<T>>();
        entityTypeBuilder.ComplexCollection(typeof(T), nameof(TestEntity<>.Items));

        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity<T>));
        return entityType.FindComplexProperty(nameof(TestEntity<T>.Items));
    }

    private static IProperty CreateScalarProperty()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityTypeBuilder = modelBuilder.Entity<TestEntity<List<TestComplexType>>>();
        entityTypeBuilder.Ignore(e => e.Items);
        var propertyInfo = typeof(TestEntity<List<TestComplexType>>).GetProperty(nameof(TestEntity<List<TestComplexType>>.Name))!;
        var propertyBuilder = entityTypeBuilder.Property(propertyInfo.PropertyType, propertyInfo.Name);

        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity<List<TestComplexType>>));
        return entityType.FindProperty(propertyInfo.Name);
    }

    private static IComplexProperty CreateReadOnlyComplexCollectionProperty()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityTypeBuilder = modelBuilder.Entity<TestEntityWithReadOnlyList>();

        var propertyInfo = typeof(TestEntityWithReadOnlyList).GetProperty(nameof(TestEntityWithReadOnlyList.ReadOnlyItems))!;
        var complexPropertyBuilder = entityTypeBuilder.ComplexCollection(propertyInfo.PropertyType, propertyInfo.Name);

        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntityWithReadOnlyList));
        return entityType.FindComplexProperty(propertyInfo.Name);
    }

    private class TestEntity<T>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public T Items { get; set; }
    }

    private class TestEntityWithReadOnlyList
    {
        private readonly IList<TestComplexType> _readOnlyItems;

        public TestEntityWithReadOnlyList()
        {
            _readOnlyItems = [];
        }

        public TestEntityWithReadOnlyList(IList<TestComplexType> readOnlyItems)
        {
            _readOnlyItems = readOnlyItems;
        }

        public int Id { get; set; }
        public IList<TestComplexType> ReadOnlyItems => _readOnlyItems;
    }

    private class MyList : List<TestComplexType>
    {
    }

    private class TestComplexType
    {
        public int Number { get; set; }
    }
}

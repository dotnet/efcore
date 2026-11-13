// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ClrPropertyGetterFactoryTest
{
    [Fact]
    public void Property_is_returned_if_it_implements_IClrPropertyGetter()
    {
        var property = new FakeProperty();

        Assert.Same(property, ClrPropertyGetterFactory.Instance.Create(property));
    }

    private class FakeProperty : Annotatable, IProperty, IClrPropertyGetter
    {
        public object GetClrValueUsingContainingEntity(object entity)
            => throw new NotImplementedException();

        public bool HasSentinelValueUsingContainingEntity(object entity)
            => throw new NotImplementedException();

        public object GetClrValue(object structuralObject)
            => throw new NotImplementedException();

        public bool HasSentinelValue(object structuralObject)
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetContainingForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IIndex> GetContainingIndexes()
            => throw new NotImplementedException();

        public IEnumerable<IKey> GetContainingKeys()
            => throw new NotImplementedException();

        public IClrPropertyGetter GetGetter()
            => throw new NotImplementedException();

        public IComparer<IUpdateEntry> GetCurrentValueComparer()
            => throw new NotImplementedException();

        public CoreTypeMapping FindTypeMapping()
            => throw new NotImplementedException();

        public int? GetMaxLength()
            => throw new NotImplementedException();

        public int? GetPrecision()
            => throw new NotImplementedException();

        public int? GetScale()
            => throw new NotImplementedException();

        public bool? IsUnicode()
            => throw new NotImplementedException();

        public PropertySaveBehavior GetBeforeSaveBehavior()
            => throw new NotImplementedException();

        public PropertySaveBehavior GetAfterSaveBehavior()
            => throw new NotImplementedException();

        public Func<IProperty, ITypeBase, ValueGenerator> GetValueGeneratorFactory()
            => throw new NotImplementedException();

        public ValueConverter GetValueConverter()
            => throw new NotImplementedException();

        public Type GetProviderClrType()
            => throw new NotImplementedException();

        public ValueComparer GetValueComparer()
            => throw new NotImplementedException();

        public ValueComparer GetKeyValueComparer()
            => throw new NotImplementedException();

        public ValueComparer GetProviderValueComparer()
            => throw new NotImplementedException();

        public JsonValueReaderWriter GetJsonValueReaderWriter()
            => throw new NotImplementedException();

        IReadOnlyElementType IReadOnlyProperty.GetElementType()
            => GetElementType();

        public bool IsPrimitiveCollection { get; }

        public IElementType GetElementType()
            => throw new NotImplementedException();

        public bool IsForeignKey()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyProperty.GetContainingForeignKeys()
            => throw new NotImplementedException();

        public bool IsIndex()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyIndex> IReadOnlyProperty.GetContainingIndexes()
            => throw new NotImplementedException();

        public IReadOnlyKey FindContainingPrimaryKey()
            => throw new NotImplementedException();

        public bool IsKey()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyKey> IReadOnlyProperty.GetContainingKeys()
            => throw new NotImplementedException();

        public PropertyAccessMode GetPropertyAccessMode()
            => throw new NotImplementedException();

        public object GetClrValueUsingContainingEntity(object entity, IReadOnlyList<int> indices)
            => throw new NotImplementedException();

        public bool HasSentinelValueUsingContainingEntity(object entity, IReadOnlyList<int> indices)
            => throw new NotImplementedException();

        public string Name { get; } = null!;
        public ITypeBase DeclaringType { get; } = null!;
        public Type ClrType { get; } = null!;
        public bool IsNullable { get; }
        public ValueGenerated ValueGenerated { get; }
        public bool IsConcurrencyToken { get; }
        public object? Sentinel { get; }
        public PropertyInfo? PropertyInfo { get; }
        public FieldInfo? FieldInfo { get; }

        IReadOnlyTypeBase IReadOnlyPropertyBase.DeclaringType
            => throw new NotImplementedException();

        public bool IsCollection
            => throw new NotImplementedException();
    }

    [Fact]
    public void Delegate_getter_is_returned_for_IProperty_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Customer>().Property(e => e.Id);
        var model = modelBuilder.FinalizeModel();

        var idProperty = model.FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Id))!;

        Assert.Equal(
            7, ClrPropertyGetterFactory.Instance.Create(idProperty).GetClrValueUsingContainingEntity(
                new Customer { Id = 7 }));
    }

    [Fact]
    public void Delegate_getter_is_returned_for_property_info()
        => Assert.Equal(
            7, ClrPropertyGetterFactory.Instance.Create(typeof(Customer).GetAnyProperty("Id")!).GetClrValueUsingContainingEntity(
                new Customer { Id = 7 }));

    [Fact]
    public void Delegate_getter_is_returned_for_IProperty_struct_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Customer>().Property(e => e.Id);
        var fuelProperty = modelBuilder.Entity<Customer>().Property(e => e.Fuel).Metadata;
        modelBuilder.FinalizeModel();

        Assert.Equal(
            new Fuel(1.0),
            ClrPropertyGetterFactory.Instance.Create((IPropertyBase)fuelProperty).GetClrValueUsingContainingEntity(
                new Customer { Id = 7, Fuel = new Fuel(1.0) }));
    }

    [Fact]
    public void Delegate_getter_is_returned_for_struct_property_info()
        => Assert.Equal(
            new Fuel(1.0),
            ClrPropertyGetterFactory.Instance.Create(typeof(Customer).GetAnyProperty("Fuel")!).GetClrValueUsingContainingEntity(
                new Customer { Id = 7, Fuel = new Fuel(1.0) }));

    [Fact]
    public void Delegate_getter_is_returned_for_index_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<IndexedClass>().Property(e => e.Id);
        var propertyA = modelBuilder.Entity<IndexedClass>().Metadata.AddIndexerProperty("PropertyA", typeof(string));
        var propertyB = modelBuilder.Entity<IndexedClass>().Metadata.AddIndexerProperty("PropertyB", typeof(int));
        modelBuilder.FinalizeModel();

        Assert.Equal(
            "ValueA",
            ClrPropertyGetterFactory.Instance.Create((IPropertyBase)propertyA)
                .GetClrValueUsingContainingEntity(new IndexedClass { Id = 7 }));
        Assert.Equal(
            123,
            ClrPropertyGetterFactory.Instance.Create((IPropertyBase)propertyB)
                .GetClrValueUsingContainingEntity(new IndexedClass { Id = 7 }));
    }

    [Fact]
    public void Delegate_getter_is_returned_for_IProperty_complex_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.Property(e => e.Id);
            b.ComplexProperty(e => e.Fuel).Property(e => e.Volume);
        });

        var model = modelBuilder.FinalizeModel();

        var volumeProperty = model.FindEntityType(typeof(Customer))!
            .FindComplexProperty(nameof(Customer.Fuel))!
            .ComplexType.FindProperty(nameof(Fuel.Volume))!;

        Assert.Equal(
            10.0, ClrPropertyGetterFactory.Instance.Create(volumeProperty).GetClrValueUsingContainingEntity(
                new Customer { Id = 7, Fuel = new Fuel(10.0) }));

        Assert.Equal(
            10.0, ClrPropertyGetterFactory.Instance.Create(volumeProperty).GetClrValue(new Fuel(10.0)));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Delegate_getter_is_returned_for_nullable_property_in_nested_complex_collection(bool detailedErrors, bool customSentinel)
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Blog>().ComplexCollection(
            e => e.Posts, b => b.ComplexCollection(
                e => e.Revisions, b => b.Property(e => e.PublishedOn)
                    .HasSentinel(customSentinel ? new DateOnly(2000, 1, 1) : null)));

        var model = modelBuilder.FinalizeModel();
        model.SetRuntimeAnnotation(CoreAnnotationNames.DetailedErrorsEnabled, detailedErrors);
        var property = model.FindEntityType(typeof(Blog))!
            .FindComplexProperty(nameof(Blog.Posts))!.ComplexType
            .FindComplexProperty(nameof(Post.Revisions))!.ComplexType
            .FindProperty(nameof(Revision.PublishedOn))!;

        var getter = ClrPropertyGetterFactory.Instance.Create(property);
        var revision = new Revision();
        var blog = new Blog { Posts = [new Post { Revisions = [revision] }] };

        Assert.Null(getter.GetClrValueUsingContainingEntity(blog, [0, 0]));
        Assert.Null(getter.GetClrValue(revision));
        Assert.Equal(!customSentinel, getter.HasSentinelValueUsingContainingEntity(blog, [0, 0]));
        Assert.Equal(!customSentinel, getter.HasSentinelValue(revision));

        revision.PublishedOn = new DateOnly(2000, 1, 1);

        Assert.Equal(revision.PublishedOn, getter.GetClrValueUsingContainingEntity(blog, [0, 0]));
        Assert.Equal(revision.PublishedOn, getter.GetClrValue(revision));
        Assert.Equal(customSentinel, getter.HasSentinelValueUsingContainingEntity(blog, [0, 0]));
        Assert.Equal(customSentinel, getter.HasSentinelValue(revision));

        if (detailedErrors)
        {
            var complexProperty = ((IComplexType)property.DeclaringType).ComplexProperty;
            Assert.Equal(
                CoreStrings.ComplexCollectionOrdinalOutOfRange(
                    1, complexProperty.DeclaringType.ShortNameChain(), complexProperty.Name, 1),
                Assert.Throws<InvalidOperationException>(() => getter.GetClrValueUsingContainingEntity(blog, [0, 1])).Message);

            blog.Posts[0].Revisions = null!;

            Assert.Equal(
                CoreStrings.ComplexCollectionNotInitialized(complexProperty.DeclaringType.ShortNameChain(), complexProperty.Name),
                Assert.Throws<InvalidOperationException>(() => getter.GetClrValueUsingContainingEntity(blog, [0, 0])).Message);
        }
    }

    private class Blog
    {
        public int Id { get; set; }
        public List<Post> Posts { get; set; } = [];
    }

    private class Post
    {
        public List<Revision> Revisions { get; set; } = [];
    }

    private class Revision
    {
        public DateOnly? PublishedOn { get; set; }
    }

    private static TestHelpers.TestModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        internal int Id { get; set; }
        internal Fuel Fuel { get; set; }
    }

    private struct Fuel(double volume)
    {
        public double Volume { get; } = volume;
    }

    private class IndexedClass
    {
        private readonly Dictionary<string, object> _internalValues = new() { { "PropertyA", "ValueA" }, { "PropertyB", 123 } };

        internal int Id { get; set; }

        internal object this[string name]
        {
            get => _internalValues[name];
            set => _internalValues[name] = value;
        }
    }
}

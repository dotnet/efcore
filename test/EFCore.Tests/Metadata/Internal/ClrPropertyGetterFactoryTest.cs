// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ClrPropertyGetterFactoryTest
{
    [ConditionalFact]
    public void Property_is_returned_if_it_implements_IClrPropertyGetter()
    {
        var property = new FakeProperty();

        Assert.Same(property, ClrPropertyGetterFactory.Instance.Create(property));
    }

    private class FakeProperty : Annotatable, IProperty, IClrPropertyGetter
    {
        public object GetClrValueUsingContainingEntity(object entity)
            => throw new NotImplementedException();

        public bool HasSentinelUsingContainingEntity(object entity)
            => throw new NotImplementedException();

        public object GetClrValue(object structuralObject)
            => throw new NotImplementedException();

        public bool HasSentinel(object structuralObject)
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

        public string Name { get; }
        public ITypeBase DeclaringType { get; }
        public Type ClrType { get; }
        public bool IsNullable { get; }
        public ValueGenerated ValueGenerated { get; }
        public bool IsConcurrencyToken { get; }
        public object Sentinel { get; }
        public PropertyInfo PropertyInfo { get; }
        public FieldInfo FieldInfo { get; }

        IReadOnlyEntityType IReadOnlyProperty.DeclaringEntityType
            => throw new NotImplementedException();

        IReadOnlyTypeBase IReadOnlyPropertyBase.DeclaringType
            => throw new NotImplementedException();
    }

    [ConditionalFact]
    public void Delegate_getter_is_returned_for_IProperty_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Customer>().Property(e => e.Id);
        var model = modelBuilder.FinalizeModel();

        var idProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

        Assert.Equal(
            7, ClrPropertyGetterFactory.Instance.Create(idProperty).GetClrValueUsingContainingEntity(
                new Customer { Id = 7 }));
    }

    [ConditionalFact]
    public void Delegate_getter_is_returned_for_property_info()
        => Assert.Equal(
            7, ClrPropertyGetterFactory.Instance.Create(typeof(Customer).GetAnyProperty("Id")).GetClrValueUsingContainingEntity(
                new Customer { Id = 7 }));

    [ConditionalFact]
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

    [ConditionalFact]
    public void Delegate_getter_is_returned_for_struct_property_info()
        => Assert.Equal(
            new Fuel(1.0),
            ClrPropertyGetterFactory.Instance.Create(typeof(Customer).GetAnyProperty("Fuel")).GetClrValueUsingContainingEntity(
                new Customer { Id = 7, Fuel = new Fuel(1.0) }));

    [ConditionalFact]
    public void Delegate_getter_is_returned_for_index_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<IndexedClass>().Property(e => e.Id);
        var propertyA = modelBuilder.Entity<IndexedClass>().Metadata.AddIndexerProperty("PropertyA", typeof(string));
        var propertyB = modelBuilder.Entity<IndexedClass>().Metadata.AddIndexerProperty("PropertyB", typeof(int));
        modelBuilder.FinalizeModel();

        Assert.Equal(
            "ValueA",
            ClrPropertyGetterFactory.Instance.Create((IPropertyBase)propertyA).GetClrValueUsingContainingEntity(new IndexedClass { Id = 7 }));
        Assert.Equal(
            123,
            ClrPropertyGetterFactory.Instance.Create((IPropertyBase)propertyB).GetClrValueUsingContainingEntity(new IndexedClass { Id = 7 }));
    }

    [ConditionalFact]
    public void Delegate_getter_is_returned_for_IProperty_complex_property()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Customer>(
            b =>
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

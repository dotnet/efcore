// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ClrPropertyGetterFactoryTest
    {
        [ConditionalFact]
        public void Property_is_returned_if_it_implements_IClrPropertyGetter()
        {
            var property = new FakeProperty();

            Assert.Same(property, new ClrPropertyGetterFactory().Create(property));
        }

        private class FakeProperty : Annotatable, IProperty, IClrPropertyGetter
        {
            public object GetClrValue(object entity)
                => throw new NotImplementedException();

            public bool HasDefaultValue(object entity)
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

            public Func<IProperty, IEntityType, ValueGenerator> GetValueGeneratorFactory()
                => throw new NotImplementedException();

            public ValueConverter GetValueConverter()
                => throw new NotImplementedException();

            public Type GetProviderClrType()
                => throw new NotImplementedException();

            public ValueComparer GetValueComparer()
                => throw new NotImplementedException();

            public ValueComparer GetKeyValueComparer()
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
            public IEntityType DeclaringEntityType { get; }
            public bool IsNullable { get; }
            public ValueGenerated ValueGenerated { get; }
            public bool IsConcurrencyToken { get; }
            public PropertyInfo PropertyInfo { get; }
            public FieldInfo FieldInfo { get; }

            IReadOnlyEntityType IReadOnlyProperty.DeclaringEntityType => throw new NotImplementedException();

            IReadOnlyTypeBase IReadOnlyPropertyBase.DeclaringType => throw new NotImplementedException();
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_IProperty_property()
        {
            var modelBuilder = CreateModelBuilder();
            var idProperty = modelBuilder.Entity<Customer>().Property(e => e.Id).Metadata;
            InMemoryTestHelpers.Instance.Finalize(modelBuilder);

            Assert.Equal(
                7, new ClrPropertyGetterFactory().Create((IPropertyBase)idProperty).GetClrValue(
                    new Customer { Id = 7 }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_property_info()
        {
            Assert.Equal(
                7, new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("Id")).GetClrValue(
                    new Customer { Id = 7 }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_IProperty_struct_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Customer>().Property(e => e.Id);
            var fuelProperty = modelBuilder.Entity<Customer>().Property(e => e.Fuel).Metadata;
            InMemoryTestHelpers.Instance.Finalize(modelBuilder);

            Assert.Equal(
                new Fuel(1.0),
                new ClrPropertyGetterFactory().Create((IPropertyBase)fuelProperty).GetClrValue(
                    new Customer { Id = 7, Fuel = new Fuel(1.0) }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_struct_property_info()
        {
            Assert.Equal(
                new Fuel(1.0),
                new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("Fuel")).GetClrValue(
                    new Customer { Id = 7, Fuel = new Fuel(1.0) }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_index_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<IndexedClass>().Property(e => e.Id);
            var propertyA = modelBuilder.Entity<IndexedClass>().Metadata.AddIndexerProperty("PropertyA", typeof(string));
            var propertyB = modelBuilder.Entity<IndexedClass>().Metadata.AddIndexerProperty("PropertyB", typeof(int));
            InMemoryTestHelpers.Instance.Finalize(modelBuilder);

            Assert.Equal("ValueA", new ClrPropertyGetterFactory().Create((IPropertyBase)propertyA).GetClrValue(new IndexedClass { Id = 7 }));
            Assert.Equal(123, new ClrPropertyGetterFactory().Create((IPropertyBase)propertyB).GetClrValue(new IndexedClass { Id = 7 }));
        }

        private static ModelBuilder CreateModelBuilder()
            => InMemoryTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            internal int Id { get; set; }
            internal Fuel Fuel { get; set; }
        }

        private struct Fuel
        {
            public Fuel(double volume)
                => Volume = volume;

            public double Volume { get; }
        }

        private class IndexedClass
        {
            private readonly Dictionary<string, object> _internalValues = new()
            {
                { "PropertyA", "ValueA" }, { "PropertyB", 123 }
            };

            internal int Id { get; set; }

            internal object this[string name]
            {
                get => _internalValues[name];
                set => _internalValues[name] = value;
            }
        }
    }
}

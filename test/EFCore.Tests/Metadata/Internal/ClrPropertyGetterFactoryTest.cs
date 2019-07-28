// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions;
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

        private class FakeProperty : IProperty, IClrPropertyGetter
        {
            public object GetClrValue(object entity) => throw new NotImplementedException();
            public bool HasDefaultValue(object entity) => throw new NotImplementedException();
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public string Name { get; }
            public ITypeBase DeclaringType { get; }
            public Type ClrType { get; }
            public IEntityType DeclaringEntityType { get; }
            public bool IsNullable { get; }
            public ValueGenerated ValueGenerated { get; }
            public bool IsConcurrencyToken { get; }
            public PropertyInfo PropertyInfo { get; }
            public FieldInfo FieldInfo { get; }
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_IProperty_property()
        {
            var modelBuilder = new ModelBuilder(InMemoryConventionSetBuilder.Build());
            var idProperty = modelBuilder.Entity<Customer>().Property(e => e.Id).Metadata;
            modelBuilder.FinalizeModel();

            Assert.Equal(
                7, new ClrPropertyGetterFactory().Create(idProperty).GetClrValue(
                    new Customer
                    {
                        Id = 7
                    }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_property_info()
        {
            Assert.Equal(
                7, new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("Id")).GetClrValue(
                    new Customer
                    {
                        Id = 7
                    }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_IProperty_struct_property()
        {
            var modelBuilder = new ModelBuilder(InMemoryConventionSetBuilder.Build());
            modelBuilder.Entity<Customer>().Property(e => e.Id);
            var fuelProperty = modelBuilder.Entity<Customer>().Property(e => e.Fuel).Metadata;
            modelBuilder.FinalizeModel();

            Assert.Equal(
                new Fuel(1.0),
                new ClrPropertyGetterFactory().Create(fuelProperty).GetClrValue(
                    new Customer
                    {
                        Id = 7,
                        Fuel = new Fuel(1.0)
                    }));
        }

        [ConditionalFact]
        public void Delegate_getter_is_returned_for_struct_property_info()
        {
            Assert.Equal(
                new Fuel(1.0),
                new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("Fuel")).GetClrValue(
                    new Customer
                    {
                        Id = 7,
                        Fuel = new Fuel(1.0)
                    }));
        }

        private class Customer
        {
            internal int Id { get; set; }
            internal Fuel Fuel { get; set; }
        }

        private struct Fuel
        {
            public Fuel(double volume) => Volume = volume;
            public double Volume { get; }
        }
    }
}

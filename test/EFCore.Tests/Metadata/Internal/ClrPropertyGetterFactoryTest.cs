// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ClrPropertyGetterFactoryTest
    {
        [Fact]
        public void Property_is_returned_if_it_implements_IClrPropertyGetter()
        {
            var property = new FakeProperty();

            Assert.Same(property, new ClrPropertyGetterFactory().Create(property));
        }

        private class FakeProperty : IProperty, IClrPropertyGetter
        {
            public object GetClrValue(object instance) => throw new NotImplementedException();
            public bool HasDefaultValue(object instance) => throw new NotImplementedException();
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public string Name { get; }
            public ITypeBase DeclaringType { get; }
            public Type ClrType { get; }
            public bool IsShadowProperty { get; }
            public IEntityType DeclaringEntityType { get; }
            public bool IsNullable { get; }
            public PropertySaveBehavior BeforeSaveBehavior { get; }
            public PropertySaveBehavior AfterSaveBehavior { get; }
            public bool IsReadOnlyBeforeSave { get; }
            public bool IsReadOnlyAfterSave { get; }
            public bool IsStoreGeneratedAlways { get; }
            public ValueGenerated ValueGenerated { get; }
            public bool IsConcurrencyToken { get; }
            public PropertyInfo PropertyInfo { get; }
            public FieldInfo FieldInfo { get; }
        }

        [Fact]
        public void Delegate_getter_is_returned_for_IProperty_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty("Id", typeof(int));

            Assert.Equal(7, new ClrPropertyGetterFactory().Create(idProperty).GetClrValue(new Customer { Id = 7 }));
        }

        [Fact]
        public void Delegate_getter_is_returned_for_property_info()
        {
            Assert.Equal(7, new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("Id")).GetClrValue(new Customer { Id = 7 }));
        }

        [Fact]
        public void Delegate_getter_is_returned_for_IProperty_struct_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var fuelProperty = entityType.AddProperty("Fuel", typeof(Fuel));

            Assert.Equal(
                new Fuel(1.0),
                new ClrPropertyGetterFactory().Create(fuelProperty).GetClrValue(
                    new Customer
                    {
                        Id = 7,
                        Fuel = new Fuel(1.0)
                    }));
        }

        [Fact]
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

        [Fact]
        public void Delegate_getter_throws_for_IProperty_struct_property_when_quirked()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var fuelProperty = entityType.AddProperty("QuirkyFuel", typeof(Fuel));

            try
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", true);

                Assert.Throws<InvalidOperationException>(
                    () => new ClrPropertyGetterFactory().Create(fuelProperty));

            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", false);
            }
        }

        [Fact]
        public void Delegate_getter_throws_for_struct_PropertyInfo_when_quirked()
        {
            try
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", true);

                Assert.Throws<InvalidOperationException>(
                    () => new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("QuirkyFuel")));
            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", false);
            }
        }

        [Fact]
        public void Delegate_getter_is_returned_for_IProperty_property_even_with_quirk()
        {
            try
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", true);

                var entityType = new Model().AddEntityType(typeof(Customer));
                var idProperty = entityType.AddProperty("QuirkyId", typeof(int));

                Assert.Equal(
                    7,
                    new ClrPropertyGetterFactory()
                        .Create(idProperty)
                        .GetClrValue(new Customer { QuirkyId = 7 }));

            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", false);
            }
        }

        [Fact]
        public void Delegate_getter_is_returned_for_property_info_even_with_quirk()
        {
            try
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", true);

                Assert.Equal(
                    7,
                    new ClrPropertyGetterFactory()
                        .Create(typeof(Customer).GetAnyProperty("QuirkyId"))
                        .GetClrValue(new Customer { QuirkyId = 7 }));

            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12290", false);
            }
        }

        private class Customer
        {
            internal int Id { get; set; }
            internal Fuel Fuel { get; set; }
            internal Fuel QuirkyFuel { get; set; }
            internal int QuirkyId { get; set; }
        }

        private struct Fuel
        {
            public Fuel(double volume) => Volume = volume;
            public double Volume { get; }
        }
    }
}

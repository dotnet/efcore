// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class KeyTest
    {
        [Fact]
        public void Use_of_custom_IKey_throws()
        {
            var key = new FakeKey();

            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IKey_throws), nameof(IKey), nameof(FakeKey)),
                Assert.Throws<NotSupportedException>(() => key.AsKey()).Message);
        }

        private class FakeKey : IKey
        {
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public IReadOnlyList<IProperty> Properties { get; }
            public IEntityType DeclaringEntityType { get; }
        }

        [Fact]
        public void Can_create_key_from_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var property1 = entityType.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType.GetOrAddProperty(Customer.NameProperty);
            property2.IsNullable = false;

            var key = entityType.AddKey(new[] { property1, property2 }, ConfigurationSource.Convention);

            Assert.True(new[] { property1, property2 }.SequenceEqual(key.Properties));
            Assert.Equal(ConfigurationSource.Convention, key.GetConfigurationSource());
        }

        [Fact]
        public void Validates_properties_from_same_entity()
        {
            var entityType1 = new Model().AddEntityType(typeof(Customer));
            var entityType2 = new Model().AddEntityType(typeof(Order));
            var property1 = entityType1.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType2.GetOrAddProperty(Order.NameProperty);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity($"{{'{property1.Name}', '{property2.Name}'}}", entityType1.DisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => entityType1.AddKey(new[] { property1, property2 })).Message);
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public static readonly PropertyInfo NameProperty = typeof(Order).GetProperty("Name");

            public string Name { get; set; }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class IndexTest
    {
        [Fact]
        public void Can_create_index_from_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var property1 = entityType.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType.GetOrAddProperty(Customer.NameProperty);

            var index = entityType.AddIndex(new[] { property1, property2 }, ConfigurationSource.Convention);

            Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
            Assert.False(index.IsUnique);
            Assert.Equal(ConfigurationSource.Convention, index.GetConfigurationSource());
        }

        [Fact]
        public void Can_create_unique_index_from_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var property1 = entityType.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType.GetOrAddProperty(Customer.NameProperty);

            var index = entityType.AddIndex(new[] { property1, property2 });
            index.IsUnique = true;

            Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
            Assert.True(index.IsUnique);
        }

        [Fact]
        public void Constructor_validates_properties_from_same_entity()
        {
            var property1 = new Model().AddEntityType(typeof(Customer)).GetOrAddProperty(Customer.IdProperty);
            var property2 = new Model().AddEntityType(typeof(Order)).GetOrAddProperty(Order.IdProperty);

            Assert.Equal(
                CoreStrings.IndexPropertiesWrongEntity($"{{'{property1.Name}', '{property2.Name}'}}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => property1.DeclaringEntityType.AddIndex(new[] { property1, property2 })).Message);
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
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");

            public int Id { get; set; }
        }
    }
}

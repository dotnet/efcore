// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class IndexTest
    {
        [Fact]
        public void Can_create_index_from_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var property1 = entityType.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType.GetOrAddProperty(Customer.NameProperty);

            var index = new Index(new[] { property1, property2 });

            Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
            Assert.Null(index.IsUnique);
            Assert.False(((IIndex)index).IsUnique);
        }

        [Fact]
        public void Can_create_unique_index_from_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var property1 = entityType.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType.GetOrAddProperty(Customer.NameProperty);

            var index = new Index(new[] { property1, property2 }) { IsUnique = true };

            Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
            Assert.True(index.IsUnique.Value);
        }

        [Fact]
        public void Constructor_check_arguments()
        {
            Assert.Equal(
                "properties",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new Index(null)).ParamName);

            Assert.Equal(
                Strings.CollectionArgumentIsEmpty("properties"),
                Assert.Throws<ArgumentException>(() => new Index(new Property[0])).Message);
        }

        [Fact]
        public void Constructor_validates_properties_from_same_entity()
        {
            var property1 = new Model().AddEntityType(typeof(Customer)).GetOrAddProperty(Customer.IdProperty);
            var property2 = new Model().AddEntityType(typeof(Order)).GetOrAddProperty(Order.IdProperty);

            Assert.Equal(Strings.InconsistentEntityType("properties"),
                Assert.Throws<ArgumentException>(
                    () => new Index(new[] { property1, property2 })).Message);
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

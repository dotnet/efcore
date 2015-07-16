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
    public class KeyTest
    {
        [Fact]
        public void Can_create_key_from_properties()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var property1 = entityType.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType.GetOrAddProperty(Customer.NameProperty);

            var key = new Key(new[] { property1, property2 });

            Assert.True(new[] { property1, property2 }.SequenceEqual(key.Properties));
        }

        [Fact]
        public void Validates_properties_from_same_entity()
        {
            var entityType1 = new Model().AddEntityType(typeof(Customer));
            var entityType2 = new Model().AddEntityType(typeof(Order));
            var property1 = entityType1.GetOrAddProperty(Customer.IdProperty);
            var property2 = entityType2.GetOrAddProperty(Order.NameProperty);

            Assert.Equal(Strings.InconsistentEntityType("properties"),
                Assert.Throws<ArgumentException>(
                    () => new Key(new[] { property1, property2 })).Message);
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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class IndexTest
    {
        [ConditionalFact]
        public void Can_create_index_from_properties()
        {
            var entityType = ((IConventionModel)CreateModel()).AddEntityType(typeof(Customer));
            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            var index = entityType.AddIndex(new[] { property1, property2 });

            Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
            Assert.False(index.IsUnique);
            Assert.Equal(ConfigurationSource.Convention, index.GetConfigurationSource());
        }

        [ConditionalFact]
        public void Can_create_unique_index_from_properties()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            var index = entityType.AddIndex(new[] { property1, property2 });
            index.IsUnique = true;

            Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
            Assert.True(index.IsUnique);
        }

        [ConditionalFact]
        public void Constructor_validates_properties_from_same_entity()
        {
            var model = CreateModel();
            var property1 = model.AddEntityType(typeof(Customer)).AddProperty(Customer.IdProperty);
            var property2 = model.AddEntityType(typeof(Order)).AddProperty(Order.IdProperty);

            Assert.Equal(
                CoreStrings.IndexPropertiesWrongEntity($"{{'{property1.Name}', '{property2.Name}'}}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => property1.DeclaringEntityType.AddIndex(new[] { property1, property2 })).Message);
        }

        private static IMutableModel CreateModel() => new Model();
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

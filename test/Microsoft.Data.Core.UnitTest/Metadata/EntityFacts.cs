// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Metadata
{
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class EntityFacts
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        [Fact]
        public void Can_create_entity()
        {
            var entity = new Entity(typeof(Customer));

            Assert.Equal("Customer", entity.Name);
            Assert.Same(typeof(Customer), entity.Type);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var entity = new Entity(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.AddProperty(property1);
            entity.AddProperty(property2);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));

            entity.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void Properties_are_ordered_by_name()
        {
            var entity = new Entity(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.AddProperty(property2);
            entity.AddProperty(property1);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void Can_set_and_reset_key()
        {
            var entity = new Entity(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.Key = new[] { property1, property2 };

            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Key));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));

            entity.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entity.Key));
            Assert.True(new[] { property2 }.SequenceEqual(entity.Properties));

            entity.Key = new[] { property1 };

            Assert.True(new[] { property1 }.SequenceEqual(entity.Key));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void Setting_key_properties_should_update_existing_properties()
        {
            var entity = new Entity(typeof(Customer));

            entity.AddProperty(new Property(Customer.IdProperty));

            var newIdProperty = new Property(Customer.IdProperty);

            var property2 = new Property(Customer.NameProperty);

            entity.Key = new[] { newIdProperty, property2 };

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void Can_clear_key()
        {
            var entity = new Entity(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.Key = new[] { property1, property2 };

            Assert.Equal(2, entity.Key.Count());

            entity.Key = new Property[] { };

            Assert.Equal(0, entity.Key.Count());
        }
    }
}

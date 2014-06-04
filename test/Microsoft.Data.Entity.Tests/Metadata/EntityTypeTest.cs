// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityTypeTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "type",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityType((Type)null)).ParamName);

            Assert.Equal(
                "name",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityType((string)null)).ParamName);

            var entityType = new EntityType(typeof(Random));

            Assert.Equal(
                "propertyInfo",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.AddProperty((PropertyInfo)null)).ParamName);

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.RemoveProperty(null)).ParamName);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => entityType.TryGetProperty("")).Message);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => entityType.GetProperty("")).Message);
        }

        [Fact]
        public void Storage_name_defaults_to_name()
        {
            var entityType = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.StorageName);
        }

        [Fact]
        public void Storage_name_can_be_different_from_name()
        {
            var entityType = new EntityType(typeof(Customer)) { StorageName = "CustomerTable" };

            Assert.Equal("CustomerTable", entityType.StorageName);
        }

        [Fact]
        public void Can_create_entity_type()
        {
            var entityType = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.Name);
            Assert.Same(typeof(Customer), entityType.Type);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Property_back_pointer_is_fixed_up_as_property_is_added_and_removed()
        {
            var entityType1 = new EntityType(typeof(Customer));

            var property = entityType1.AddProperty(Customer.IdProperty);

            Assert.Same(entityType1, property.EntityType);

            entityType1.RemoveProperty(property);

            Assert.Empty(entityType1.Properties);
            Assert.Null(property.EntityType);
        }

        [Fact]
        public void Properties_are_ordered_by_name()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_set_and_reset_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            entityType.SetKey(property1, property2);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));

            property1 = entityType.AddProperty(Customer.IdProperty);

            entityType.SetKey(property1);

            Assert.True(new[] { property1 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Setting_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.IdProperty);

            var newIdProperty = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            entityType.SetKey(newIdProperty, property2);

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_clear_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            entityType.SetKey(property1, property2);

            Assert.Equal(2, entityType.GetKey().Properties.Count());

            entityType.SetKey(null);

            Assert.Null(entityType.TryGetKey());
        }

        [Fact]
        public void Add_foreign_key()
        {
            var entityType = new EntityType(typeof(Order));
            entityType.SetKey(entityType.AddProperty(Order.IdProperty));

            var foreignKey
                = entityType.AddForeignKey(
                    entityType.GetKey(),
                    new[] { entityType.AddProperty(Order.CustomerUniqueProperty) });

            Assert.True(entityType.ForeignKeys.Contains(foreignKey));
        }

        [Fact]
        public void Setting_foreign_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Order));
            entityType.SetKey(entityType.AddProperty(Order.CustomerIdProperty));

            var newIdProperty = entityType.AddProperty(Order.CustomerIdProperty);
            var property2 = entityType.AddProperty(Order.CustomerUniqueProperty);

            entityType.AddForeignKey(entityType.GetKey(), new[] { newIdProperty, property2 });

            Assert.Equal(new[] { newIdProperty, property2 }, entityType.Properties.ToArray());
        }

        [Fact]
        public void FK_back_pointer_is_fixed_up_as_FK_is_added()
        {
            var entityType = new EntityType(typeof(Customer));
            var property = entityType.AddProperty(Customer.IdProperty);
            entityType.SetKey(property);
            var foreignKey
                = entityType.AddForeignKey(entityType.GetKey(), property);

            Assert.Same(entityType, foreignKey.EntityType);
            Assert.Same(entityType, property.EntityType);

            entityType.RemoveForeignKey(foreignKey);

            // Currently property is not removed when FK is removed
            Assert.Empty(entityType.ForeignKeys);
            Assert.Same(property, entityType.Properties.Single());
            Assert.Same(entityType, foreignKey.EntityType); // TODO: Throw here?
            Assert.Same(entityType, property.EntityType);
        }

        [Fact]
        public void Can_add_navigations()
        {
            var entityType = new EntityType(typeof(Order));

            var navigation = new Navigation(new Mock<ForeignKey>().Object, "Milk", pointsToPrincipal: true);

            entityType.AddNavigation(navigation);

            Assert.Same(navigation, entityType.Navigations.Single());
            Assert.Same(entityType, navigation.EntityType);
        }

        [Fact]
        public void Navigation_back_pointer_is_fixed_up_as_navigation_is_added_and_removed()
        {
            var entityType1 = new EntityType(typeof(Customer));
            entityType1.SetKey(entityType1.AddProperty(Customer.IdProperty));
            var entityType2 = new EntityType(typeof(Customer));

            var navigation
                = new Navigation(
                    new ForeignKey(
                        entityType1.GetKey(),
                        new[] { entityType1.AddProperty(Customer.IdProperty) }), "Nav", pointsToPrincipal: true);

            entityType1.AddNavigation(navigation);

            Assert.Same(entityType1, navigation.EntityType);

            entityType2.AddNavigation(navigation);

            Assert.Same(entityType2, navigation.EntityType);
            Assert.Empty(entityType1.Navigations);

            entityType2.RemoveNavigation(navigation);

            Assert.Empty(entityType2.Navigations);
            Assert.Null(navigation.EntityType);
        }

        [Fact]
        public void Properties_is_IList_to_ensure_collecting_the_count_is_fast()
        {
            Assert.IsAssignableFrom<IList<Property>>(new EntityType(typeof(Customer)).Properties);
        }

        [Fact]
        public void Can_get_property_and_can_try_get_property()
        {
            var entityType = new EntityType(typeof(Customer));
            entityType.AddProperty(Customer.IdProperty);

            Assert.Equal("Id", entityType.TryGetProperty("Id").Name);
            Assert.Equal("Id", entityType.GetProperty("Id").Name);

            Assert.Null(entityType.TryGetProperty("Nose"));

            Assert.Equal(
                Strings.FormatPropertyNotFound("Nose", "Customer"),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetProperty("Nose")).Message);
        }

        [Fact]
        public void Shadow_properties_have_CLR_flag_set_to_false()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("Mane", typeof(int), shadowProperty: true, concurrencyToken: false);

            Assert.True(entityType.GetProperty("Name").IsClrProperty);
            Assert.True(entityType.GetProperty("Id").IsClrProperty);
            Assert.False(entityType.GetProperty("Mane").IsClrProperty);
        }

        [Fact]
        public void Can_get_property_indexes()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType.AddProperty("Mane", typeof(int), shadowProperty: true, concurrencyToken: false);

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Mane").Index);
            Assert.Equal(2, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Mane").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(2, entityType.ShadowPropertyCount);
        }

        [Fact]
        public void Indexes_are_rebuilt_when_more_properties_added()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity));

            entityType.AddProperty("Name", typeof(string));
            entityType.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: true);

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(0, entityType.GetProperty("Id").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").OriginalValueIndex);

            Assert.Equal(1, entityType.ShadowPropertyCount);
            Assert.Equal(1, entityType.OriginalValueCount);

            entityType.AddProperty("Game", typeof(int), shadowProperty: true, concurrencyToken: true);
            entityType.AddProperty("Mane", typeof(int), shadowProperty: true, concurrencyToken: true);

            Assert.Equal(0, entityType.GetProperty("Game").Index);
            Assert.Equal(1, entityType.GetProperty("Id").Index);
            Assert.Equal(2, entityType.GetProperty("Mane").Index);
            Assert.Equal(3, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Game").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(2, entityType.GetProperty("Mane").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(0, entityType.GetProperty("Game").OriginalValueIndex);
            Assert.Equal(1, entityType.GetProperty("Id").OriginalValueIndex);
            Assert.Equal(2, entityType.GetProperty("Mane").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").OriginalValueIndex);

            Assert.Equal(3, entityType.ShadowPropertyCount);
            Assert.Equal(3, entityType.OriginalValueCount);
        }

        [Fact]
        public void Lazy_original_values_are_used_for_full_notification_and_shadow_enties()
        {
            Assert.True(new EntityType(typeof(FullNotificationEntity)).UseLazyOriginalValues);
        }

        [Fact]
        public void Lazy_original_values_are_used_for_shadow_enties()
        {
            Assert.True(new EntityType("Z'ha'dum").UseLazyOriginalValues);
        }

        [Fact]
        public void Eager_original_values_are_used_for_enties_that_only_implement_INotifyPropertyChanged()
        {
            Assert.False(new EntityType(typeof(ChangedOnlyEntity)).UseLazyOriginalValues);
        }

        [Fact]
        public void Eager_original_values_are_used_for_enties_that_do_no_notification()
        {
            Assert.False(new EntityType(typeof(Customer)).UseLazyOriginalValues);
        }

        [Fact]
        public void Lazy_original_values_can_be_switched_off()
        {
            Assert.False(new EntityType(typeof(FullNotificationEntity)) { UseLazyOriginalValues = false }.UseLazyOriginalValues);
        }

        [Fact]
        public void Lazy_original_values_can_be_switched_on_but_only_if_entity_does_not_require_eager_values()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity)) { UseLazyOriginalValues = false };
            entityType.UseLazyOriginalValues = true;
            Assert.True(entityType.UseLazyOriginalValues);

            Assert.Equal(
                Strings.FormatEagerOriginalValuesRequired("ChangedOnlyEntity"),
                Assert.Throws<InvalidOperationException>(() => new EntityType(typeof(ChangedOnlyEntity)) { UseLazyOriginalValues = true }).Message);
        }

        [Fact]
        public void All_properties_have_original_value_indexes_when_using_eager_original_values()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity)) { UseLazyOriginalValues = false };

            entityType.AddProperty("Name", typeof(string));
            entityType.AddProperty("Id", typeof(int));

            Assert.Equal(0, entityType.GetProperty("Id").OriginalValueIndex);
            Assert.Equal(1, entityType.GetProperty("Name").OriginalValueIndex);

            Assert.Equal(2, entityType.OriginalValueCount);
        }

        [Fact]
        public void Only_required_properties_have_original_value_indexes_when_using_lazy_original_values()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity));

            entityType.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);
            entityType.AddProperty("Id", typeof(int));

            Assert.Equal(-1, entityType.GetProperty("Id").OriginalValueIndex);
            Assert.Equal(0, entityType.GetProperty("Name").OriginalValueIndex);

            Assert.Equal(1, entityType.OriginalValueCount);
        }

        [Fact]
        public void FK_properties_are_marked_as_requiring_original_values()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity));
            entityType.SetKey(entityType.AddProperty("Id", typeof(int)));

            Assert.Equal(-1, entityType.GetProperty("Id").OriginalValueIndex);

            entityType.AddForeignKey(entityType.GetKey(), new[] { entityType.AddProperty("Id", typeof(int)) });

            Assert.Equal(0, entityType.GetProperty("Id").OriginalValueIndex);
        }

        #region Fixture

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
        }

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Guid CustomerUnique { get; set; }
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        NotifyChanging();
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        NotifyChanging();
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private void NotifyChanging([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanging != null)
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                }
            }
        }

        private class ChangedOnlyEntity : INotifyPropertyChanged
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        #endregion
    }
}

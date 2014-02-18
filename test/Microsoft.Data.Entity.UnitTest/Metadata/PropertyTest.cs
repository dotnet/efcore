// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class PropertyTest
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public string Name { get; set; }
        }

        #endregion

        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "propertyInfo",
                Assert.Throws<ArgumentNullException>(() => new Property(null)).ParamName);

            var property = new Property(Customer.NameProperty);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => property.StorageName = "").Message);

            Assert.Equal(
                "instance",
                Assert.Throws<ArgumentNullException>(() => property.GetValue(null)).ParamName);

            Assert.Equal(
                "instance",
                Assert.Throws<ArgumentNullException>(() => property.SetValue(null, "Kake")).ParamName);
        }

        [Fact]
        public void StorageName_defaults_to_name()
        {
            var property = new Property(Customer.NameProperty);

            Assert.Equal("Name", property.StorageName);
        }

        [Fact]
        public void StorageName_can_be_different_from_name()
        {
            var property = new Property(Customer.NameProperty) { StorageName = "CustomerName" };

            Assert.Equal("CustomerName", property.StorageName);
        }

        [Fact]
        public void Can_create_property()
        {
            var property = new Property(Customer.NameProperty);

            Assert.Equal("Name", property.Name);
            Assert.Same(typeof(Customer), property.DeclaringType);
            Assert.Same(typeof(string), property.Type);
        }

        [Fact]
        public void Can_get_and_set_property_value()
        {
            var entity = new Customer();
            var property = new Property(Customer.NameProperty);

            Assert.Null(property.GetValue(entity));
            property.SetValue(entity, "There is no kake");
            Assert.Equal("There is no kake", property.GetValue(entity));
        }
    }
}

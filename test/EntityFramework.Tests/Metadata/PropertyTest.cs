// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class PropertyTest
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");
            public static PropertyInfo AgeProperty = typeof(Customer).GetProperty("Age");
            public static PropertyInfo HashProperty = typeof(Customer).GetProperty("Hash");

            public int Id { get; set; }
            public string Name { get; set; }
            public byte? Age { get; set; }
            public Guid Hash { get; set; }
        }

        #endregion

        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "name",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new Property(null, null)).ParamName);
        }

        [Fact]
        public void Storage_name_defaults_to_name()
        {
            var property = new Property("Name", typeof(string));

            Assert.Equal("Name", property.StorageName);
        }

        [Fact]
        public void Storage_name_can_be_different_from_name()
        {
            var property = new Property("Name", typeof(string))
                {
                    StorageName = "CustomerName"
                };

            Assert.Equal("CustomerName", property.StorageName);
        }

        [Fact]
        public void Can_create_property_from_property_info()
        {
            var property = new Property("Name", typeof(string));

            Assert.Equal("Name", property.Name);
            Assert.Same(typeof(string), property.PropertyType);
            Assert.True(property.IsNullable);
        }

        [Fact]
        public void HasClrProperty_is_set_appropriately()
        {
            Assert.True(new Property("Kake", typeof(int)).IsClrProperty);
            Assert.True(new Property("Kake", typeof(int), shadowProperty: false, concurrencyToken: false).IsClrProperty);
            Assert.False(new Property("Kake", typeof(int), shadowProperty: true, concurrencyToken: false).IsClrProperty);
        }

        [Fact]
        public void Property_is_not_concurrency_token_by_default()
        {
            Assert.False(new Property("Name", typeof(string)).IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_as_concurrency_token()
        {
            Assert.True(new Property("Name", typeof(string), shadowProperty: false, concurrencyToken: true).IsConcurrencyToken);
        }

        [Fact]
        public void Can_get_and_set_property_index_for_normal_property()
        {
            var property = new Property("Kake", typeof(int));

            Assert.Equal(0, property.Index);
            Assert.Equal(-1, property.ShadowIndex);

            property.Index = 1;

            Assert.Equal(1, property.Index);
            Assert.Equal(-1, property.ShadowIndex);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.Index = -1).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.ShadowIndex = -1).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.ShadowIndex = 1).ParamName);
        }

        [Fact]
        public void Can_get_and_set_property_and_shadow_index_for_shadow_property()
        {
            var property = new Property("Kake", typeof(int), shadowProperty: true, concurrencyToken: false);

            Assert.Equal(0, property.Index);
            Assert.Equal(0, property.ShadowIndex);

            property.Index = 1;
            property.ShadowIndex = 2;

            Assert.Equal(1, property.Index);
            Assert.Equal(2, property.ShadowIndex);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.Index = -1).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.ShadowIndex = -1).ParamName);
        }
    }
}

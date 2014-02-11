// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        public void Can_create_property()
        {
            var property = new Property(Customer.NameProperty);

            Assert.Equal("Name", property.Name);
            Assert.Same(Customer.NameProperty, property.PropertyInfo);
        }
    }
}

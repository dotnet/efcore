// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityKeyFactoryFactoryTest
    {
        [Fact]
        public void Creates_SimpleEntityKeyFactory_for_non_composite_keys()
        {
            var entityType = new EntityType(typeof(Customer)) { Key = new[] { new Property(Customer.IdProperty) } };

            Assert.IsType<SimpleEntityKeyFactory<Customer, int>>(new EntityKeyFactoryFactory().Create(entityType));
        }

        #region Fixture

        public class Customer
        {
            public static PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public int Id { get; set; }
        }

        #endregion
    }
}

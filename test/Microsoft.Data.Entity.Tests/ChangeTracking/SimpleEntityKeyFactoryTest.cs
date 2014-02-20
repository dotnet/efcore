// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class SimpleEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_EntityKey_based_on_entity_key_values()
        {
            var factory = new SimpleEntityKeyFactory<Customer, int>(new Property(Customer.IdProperty));

            var key = factory.Create(new Customer { Id = 77 });

            Assert.IsType<SimpleEntityKey<Customer, int>>(key);
            Assert.Equal(77, key.Value);
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

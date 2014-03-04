// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityEqualityComparerTest
    {
        #region Fixture

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        [Fact]
        public void Equal_when_non_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);

            IEqualityComparer<object> comparer = new EntityEqualityComparer(model);

            Assert.True(comparer.Equals(new Customer { Id = 42 }, new Customer { Id = 42 }));
            Assert.False(comparer.Equals(new Customer { Id = 42 }, new Customer { Id = 43 }));
            Assert.False(comparer.Equals(new Customer { Id = 42 }, new Order { Id = 42 }));
            Assert.False(comparer.Equals(new Customer { Id = 42 }, new Order { Id = 43 }));
        }

        [Fact]
        public void Equal_when_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => new { c.Id, c.Name });

            IEqualityComparer<object> comparer = new EntityEqualityComparer(model);

            Assert.True(comparer.Equals(new Customer { Id = 42, Name = "Foo" }, new Customer { Id = 42, Name = "Foo" }));
            Assert.False(comparer.Equals(new Customer { Id = 42, Name = "Foo" }, new Customer { Id = 42, Name = "Bar" }));
        }

        [Fact]
        public void GetHashCode_when_non_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);

            IEqualityComparer<object> comparer = new EntityEqualityComparer(model);

            Assert.Equal(comparer.GetHashCode(new Customer { Id = 42 }), comparer.GetHashCode(new Customer { Id = 42 }));
            Assert.NotEqual(comparer.GetHashCode(new Customer { Id = 42 }), comparer.GetHashCode(new Customer { Id = 43 }));
            Assert.NotEqual(comparer.GetHashCode(new Customer { Id = 42 }), comparer.GetHashCode(new Order { Id = 42 }));
            Assert.NotEqual(comparer.GetHashCode(new Customer { Id = 42 }), comparer.GetHashCode(new Order { Id = 43 }));
        }

        [Fact]
        public void GetHashCode_when_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => new { c.Id, c.Name });

            IEqualityComparer<object> comparer = new EntityEqualityComparer(model);

            Assert.Equal(
                comparer.GetHashCode(new Customer { Id = 42, Name = "Foo" }),
                comparer.GetHashCode(new Customer { Id = 42, Name = "Foo" }));
            Assert.NotEqual(
                comparer.GetHashCode(new Customer { Id = 42, Name = "Foo" }),
                comparer.GetHashCode(new Customer { Id = 42, Name = "Bar" }));
        }
    }
}

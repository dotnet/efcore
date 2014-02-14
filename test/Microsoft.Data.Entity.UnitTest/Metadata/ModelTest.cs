// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelTest
    {
        #region Fixture

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Order
        {
        }

        #endregion

        [Fact]
        public void CanAddAndRemoveEntity()
        {
            var model = new Model();
            var entity = new Entity(typeof(Customer));

            model.AddEntity(entity);

            Assert.NotNull(model.Entity(new Customer()));

            model.RemoveEntity(entity);

            Assert.Null(model.Entity(new Customer()));
        }

        [Fact]
        public void CanGetEntityByInstance()
        {
            var model = new Model();
            model.AddEntity(new Entity(typeof(Customer)));

            var entity = model.Entity(new Customer());

            Assert.NotNull(entity);
            Assert.Equal("Customer", entity.Name);
            Assert.Same(entity, model.Entity(typeof(Customer)));
        }

        [Fact]
        public void CanGetEntityByType()
        {
            var model = new Model();
            model.AddEntity(new Entity(typeof(Customer)));

            var entity = model.Entity(typeof(Customer));

            Assert.NotNull(entity);
            Assert.Equal("Customer", entity.Name);
            Assert.Same(entity, model.Entity(typeof(Customer)));
        }

        [Fact]
        public void EntitiesAreOrderedByName()
        {
            var model = new Model();
            var entity1 = new Entity(typeof(Order));
            var entity2 = new Entity(typeof(Customer));

            model.AddEntity(entity1);
            model.AddEntity(entity2);

            Assert.True(new[] { entity2, entity1 }.SequenceEqual(model.Entities));
        }
    }
}

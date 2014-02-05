// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelFacts
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
        public void Can_add_and_remove_entity()
        {
            var model = new Model();
            var entity = new Entity(typeof(Customer));

            model.AddEntity(entity);

            Assert.NotNull(model.Entity(new Customer()));

            model.RemoveEntity(entity);

            Assert.Null(model.Entity(new Customer()));
        }

        [Fact]
        public void Can_get_entity_by_instance()
        {
            var model = new Model();
            model.AddEntity(new Entity(typeof(Customer)));

            var entity = model.Entity(new Customer());

            Assert.NotNull(entity);
            Assert.Equal("Customer", entity.Name);
            Assert.Same(entity, model.Entity(typeof(Customer)));
        }

        [Fact]
        public void Can_get_entity_by_type()
        {
            var model = new Model();
            model.AddEntity(new Entity(typeof(Customer)));

            var entity = model.Entity(typeof(Customer));

            Assert.NotNull(entity);
            Assert.Equal("Customer", entity.Name);
            Assert.Same(entity, model.Entity(typeof(Customer)));
        }

        [Fact]
        public void Entities_are_ordered_by_name()
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

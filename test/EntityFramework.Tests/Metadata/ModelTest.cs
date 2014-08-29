// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
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
        public void Members_check_arguments()
        {
            var model = new Model();

            Assert.Equal(
                "entityType",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => model.AddEntityType(null)).ParamName);

            Assert.Equal(
                "entityType",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => model.RemoveEntityType(null)).ParamName);

            Assert.Equal(
                "type",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => model.TryGetEntityType((Type)null)).ParamName);
        }

        [Fact]
        public void Can_add_and_remove_entity()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Customer));

            Assert.Null(entityType.Model);

            model.AddEntityType(entityType);

            Assert.NotNull(model.TryGetEntityType(typeof(Customer)));
            Assert.Same(model, entityType.Model);

            model.RemoveEntityType(entityType);

            Assert.Null(model.TryGetEntityType(typeof(Customer)));
            Assert.Null(entityType.Model);
        }

        [Fact]
        public void Can_get_entity_by_type()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Customer));
            model.AddEntityType(entityType);

            Assert.Same(entityType, model.GetEntityType(typeof(Customer)));
            Assert.Same(entityType, model.TryGetEntityType(typeof(Customer)));
            Assert.Null(model.TryGetEntityType(typeof(string)));

            Assert.Equal(
                Strings.FormatEntityTypeNotFound("String"),
                Assert.Throws<ModelItemNotFoundException>(() => model.GetEntityType(typeof(string))).Message);
        }

        [Fact]
        public void Can_get_entity_by_name()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Customer));
            model.AddEntityType(entityType);

            Assert.Same(entityType, model.GetEntityType(typeof(Customer).FullName));
            Assert.Same(entityType, model.TryGetEntityType(typeof(Customer).FullName));
            Assert.Null(model.TryGetEntityType(typeof(string)));

            Assert.Equal(
                Strings.FormatEntityTypeNotFound("String"),
                Assert.Throws<ModelItemNotFoundException>(() => model.GetEntityType("String")).Message);
        }

        [Fact]
        public void Entities_are_ordered_by_name()
        {
            var model = new Model();
            var entityType1 = new EntityType(typeof(Order));
            var entityType2 = new EntityType(typeof(Customer));

            model.AddEntityType(entityType1);
            model.AddEntityType(entityType2);

            Assert.True(new[] { entityType2, entityType1 }.SequenceEqual(model.EntityTypes));
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = new Model();
            var entityType1 = new EntityType(typeof(Customer));
            var entityType2 = new EntityType(typeof(Order));
            var keyProperty = new Property("Id", typeof(Customer)) { EntityType = entityType1 };
            var fkProperty = new Property("CustomerId", typeof(Order)) { EntityType = entityType2 };
            var foreignKey = entityType2.AddForeignKey(new Key(new[] { keyProperty }), fkProperty);

            model.AddEntityType(entityType1);
            model.AddEntityType(entityType2);

            var referencingForeignKeys = model.GetReferencingForeignKeys(entityType1);

            Assert.Same(foreignKey, referencingForeignKeys.Single());
            Assert.Same(foreignKey, entityType1.GetReferencingForeignKeys().Single());
        }
    }
}

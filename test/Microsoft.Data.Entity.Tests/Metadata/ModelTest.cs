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

            Assert.Same(entityType, model.GetEntityType("Customer"));
            Assert.Same(entityType, model.TryGetEntityType("Customer"));
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

        public class TopologicalSortTest
        {
            #region Fixture

            private class A
            {
            }

            private class B
            {
            }

            private class C
            {
            }

            #endregion

            [Fact]
            public void Sort_simple()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeB = new EntityType(typeof(B));
                entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeC = new EntityType(typeof(C));
                entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // B -> A -> C
                entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));
                entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));

                Assert.Equal(
                    new IEntityType[] { entityTypeB, entityTypeA, entityTypeC },
                    model.TopologicalSort().ToArray());
            }

            [Fact]
            public void Sort_reverse()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeB = new EntityType(typeof(B));
                entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeC = new EntityType(typeof(C));
                entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // C -> B -> A
                entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
                entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));

                Assert.Equal(
                    new IEntityType[] { entityTypeC, entityTypeB, entityTypeA },
                    model.TopologicalSort().ToArray());
            }

            [Fact]
            public void Sort_tree()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeB = new EntityType(typeof(B));
                entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeC = new EntityType(typeof(C));
                entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // A -> B, C -> B
                entityTypeB.AddForeignKey(entityTypeA.GetKey(), entityTypeB.AddProperty("P", typeof(int)));
                entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));
                entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));

                Assert.Equal(
                    new IEntityType[] { entityTypeA, entityTypeC, entityTypeB },
                    model.TopologicalSort().ToArray());
            }

            [Fact]
            public void Sort_no_edges()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeB = new EntityType(typeof(B));
                entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeC = new EntityType(typeof(C));
                entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // A B C
                Assert.Equal(
                    new IEntityType[] { entityTypeA, entityTypeB, entityTypeC },
                    model.TopologicalSort().ToArray());
            }

            [Fact]
            public void Sort_self_ref()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);

                // A -> A
                entityTypeA.AddForeignKey(entityTypeA.GetKey(), entityTypeA.AddProperty("P", typeof(int)));

                Assert.Equal(
                    Strings.FormatCircularDependency("A -> A"),
                    Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
            }

            [Fact]
            public void Sort_circular_direct()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeB = new EntityType(typeof(B));
                entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);

                // A -> B -> A
                entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
                entityTypeB.AddForeignKey(entityTypeA.GetKey(), entityTypeB.AddProperty("P", typeof(int)));

                Assert.Equal(
                    Strings.FormatCircularDependency("A -> B -> A"),
                    Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
            }

            [Fact]
            public void Sort_circular_transitive()
            {
                var entityTypeA = new EntityType(typeof(A));
                entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeB = new EntityType(typeof(B));
                entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var entityTypeC = new EntityType(typeof(C));
                entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // A -> B -> C -> A
                entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
                entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));
                entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));

                Assert.Equal(
                    Strings.FormatCircularDependency("A -> B -> C -> A"),
                    Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
            }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
                Assert.Throws<ArgumentNullException>(() => model.TryGetEntityType(null)).ParamName);
        }

        [Fact]
        public void Can_add_and_remove_entity()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Customer));

            model.AddEntityType(entityType);

            Assert.NotNull(model.TryGetEntityType(typeof(Customer)));

            model.RemoveEntityType(entityType);

            Assert.Null(model.TryGetEntityType(typeof(Customer)));
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

            public class A
            {
            }

            public class B
            {
            }

            public class C
            {
            }

            private readonly Property[] _properties = { new Property("P", typeof(int)) };

            #endregion

            [Fact]
            public void Sort_simple()
            {
                var entityTypeA = new EntityType(typeof(A));
                var entityTypeB = new EntityType(typeof(B));
                var entityTypeC = new EntityType(typeof(C));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // B -> A -> C
                entityTypeC.AddForeignKey(new ForeignKey(entityTypeA, _properties));
                entityTypeA.AddForeignKey(new ForeignKey(entityTypeB, _properties));

                Assert.Equal(
                    new[] { entityTypeB, entityTypeA, entityTypeC },
                    model.TopologicalSort());
            }

            [Fact]
            public void Sort_reverse()
            {
                var entityTypeA = new EntityType(typeof(A));
                var entityTypeB = new EntityType(typeof(B));
                var entityTypeC = new EntityType(typeof(C));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // C -> B -> A
                entityTypeA.AddForeignKey(new ForeignKey(entityTypeB, _properties));
                entityTypeB.AddForeignKey(new ForeignKey(entityTypeC, _properties));

                Assert.Equal(
                    new[] { entityTypeC, entityTypeB, entityTypeA },
                    model.TopologicalSort());
            }

            [Fact]
            public void Sort_tree()
            {
                var entityTypeA = new EntityType(typeof(A));
                var entityTypeB = new EntityType(typeof(B));
                var entityTypeC = new EntityType(typeof(C));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // A -> B, C -> B
                entityTypeB.AddForeignKey(new ForeignKey(entityTypeA, _properties));
                entityTypeC.AddForeignKey(new ForeignKey(entityTypeA, _properties));
                entityTypeB.AddForeignKey(new ForeignKey(entityTypeC, _properties));

                Assert.Equal(
                    new[] { entityTypeA, entityTypeC, entityTypeB },
                    model.TopologicalSort());
            }

            [Fact]
            public void Sort_no_edges()
            {
                var entityTypeA = new EntityType(typeof(A));
                var entityTypeB = new EntityType(typeof(B));
                var entityTypeC = new EntityType(typeof(C));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // A B C
                Assert.Equal(
                    new[] { entityTypeA, entityTypeB, entityTypeC },
                    model.TopologicalSort());
            }

            [Fact]
            public void Sort_self_ref()
            {
                var entityTypeA = new EntityType(typeof(A));

                var model = new Model();

                model.AddEntityType(entityTypeA);

                // A -> A
                entityTypeA.AddForeignKey(new ForeignKey(entityTypeA, _properties));

                Assert.Equal(
                    Strings.CircularDependency("A -> A"),
                    Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
            }

            [Fact]
            public void Sort_circular_direct()
            {
                var entityTypeA = new EntityType(typeof(A));
                var entityTypeB = new EntityType(typeof(B));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);

                // A -> B -> A
                entityTypeA.AddForeignKey(new ForeignKey(entityTypeB, _properties));
                entityTypeB.AddForeignKey(new ForeignKey(entityTypeA, _properties));

                Assert.Equal(
                    Strings.CircularDependency("A -> B -> A"),
                    Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
            }

            [Fact]
            public void Sort_circular_transitive()
            {
                var entityTypeA = new EntityType(typeof(A));
                var entityTypeB = new EntityType(typeof(B));
                var entityTypeC = new EntityType(typeof(C));

                var model = new Model();

                model.AddEntityType(entityTypeA);
                model.AddEntityType(entityTypeB);
                model.AddEntityType(entityTypeC);

                // A -> B -> C -> A
                entityTypeA.AddForeignKey(new ForeignKey(entityTypeB, _properties));
                entityTypeB.AddForeignKey(new ForeignKey(entityTypeC, _properties));
                entityTypeC.AddForeignKey(new ForeignKey(entityTypeA, _properties));

                Assert.Equal(
                    Strings.CircularDependency("A -> B -> C -> A"),
                    Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
            }
        }
    }
}

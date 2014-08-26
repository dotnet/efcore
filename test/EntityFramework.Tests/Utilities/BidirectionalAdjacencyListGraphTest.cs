// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class BidirectionalAdjacencyListGraphTest
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

        private class D
        {
        }

        private class E
        {
        }

        #endregion

        private class EntityTypeGraph : BidirectionalAdjacencyListGraph<NamedEntityType>
        {
            public void Populate(params NamedEntityType[] entityTypes)
            {
                AddVertices(entityTypes);

                foreach (var entityType in entityTypes)
                {
                    foreach (var foreignKey in entityType.ForeignKeys)
                    {
                        AddEdge((NamedEntityType)foreignKey.ReferencedEntityType, (NamedEntityType)foreignKey.EntityType);
                    }
                }
            }
        }

        private class NamedEntityType : EntityType
        {
            public NamedEntityType(Type type)
                : base(type)
            {
            }

            public override string ToString()
            {
                return Name;
            }
        }

        [Fact]
        public void Sort_simple()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // B -> A -> C
            entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));
            entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
                model.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_reverse()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // C -> B -> A
            entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
            entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeC.Name, entityTypeB.Name, entityTypeA.Name },
                model.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_preserves_graph()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // B -> A -> C
            entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));
            entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
                model.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());

            Assert.Equal(
                new[] { entityTypeA, entityTypeB, entityTypeC },
                model.Vertices);

            Assert.Equal(
                new[] { entityTypeC },
                model.GetOutgoingNeighbours(entityTypeA));

            Assert.Equal(
                new[] { entityTypeA },
                model.GetOutgoingNeighbours(entityTypeB));

            Assert.Equal(
                new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
                model.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_tree()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // A -> B, A -> C, C -> B
            entityTypeB.AddForeignKey(entityTypeA.GetKey(), entityTypeB.AddProperty("P", typeof(int)));
            entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));
            entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeA.Name, entityTypeC.Name, entityTypeB.Name },
                model.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_no_edges()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // A B C
            var model = new EntityTypeGraph();
            model.Populate(entityTypeC, entityTypeA, entityTypeB);

            Assert.Equal(
                new[] { entityTypeC.Name, entityTypeA.Name, entityTypeB.Name },
                model.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_self_ref()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // A -> A
            entityTypeA.AddForeignKey(entityTypeA.GetKey(), entityTypeA.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_circular_direct()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // C, A -> B -> A
            entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
            entityTypeB.AddForeignKey(entityTypeA.GetKey(), entityTypeB.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeC, entityTypeA, entityTypeB);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(B).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_circular_transitive()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // A -> C -> B -> A
            entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
            entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));
            entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_two_cycles()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeD = new NamedEntityType(typeof(D));
            entityTypeD.SetKey(entityTypeD.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeE = new NamedEntityType(typeof(E));
            entityTypeE.SetKey(entityTypeE.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // A -> C -> B -> A
            entityTypeA.AddForeignKey(entityTypeB.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
            entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));
            entityTypeC.AddForeignKey(entityTypeA.GetKey(), entityTypeC.AddProperty("P", typeof(int)));

            // A -> E -> D -> A
            entityTypeA.AddForeignKey(entityTypeD.GetKey(), entityTypeA.AddProperty("P", typeof(int)));
            entityTypeD.AddForeignKey(entityTypeE.GetKey(), entityTypeD.AddProperty("P", typeof(int)));
            entityTypeE.AddForeignKey(entityTypeA.GetKey(), entityTypeE.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC, entityTypeD, entityTypeE);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_leafy_cycle()
        {
            var entityTypeA = new NamedEntityType(typeof(A));
            entityTypeA.SetKey(entityTypeA.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.SetKey(entityTypeB.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.SetKey(entityTypeC.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));

            // C -> B -> C -> A
            entityTypeB.AddForeignKey(entityTypeC.GetKey(), entityTypeB.AddProperty("P", typeof(int)));
            entityTypeC.AddForeignKey(entityTypeB.GetKey(), entityTypeC.AddProperty("P", typeof(int)));
            entityTypeA.AddForeignKey(entityTypeC.GetKey(), entityTypeA.AddProperty("P", typeof(int)));

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(C).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
        }
    }
}

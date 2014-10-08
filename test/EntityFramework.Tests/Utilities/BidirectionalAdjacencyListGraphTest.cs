// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class BidirectionalAdjacencyListGraphTest
    {
        #region Fixture

        private class A
        {
            public int P { get; set; }
            public int P2 { get; set; }
        }

        private class B
        {
            public int P { get; set; }
            public int P2 { get; set; }
        }

        private class C
        {
            public int P { get; set; }
            public int P2 { get; set; }
        }

        private class D
        {
            public int P { get; set; }
            public int P2 { get; set; }
        }

        private class E
        {
            public int P { get; set; }
            public int P2 { get; set; }
        }

        #endregion

        private class EntityTypeGraph : BidirectionalAdjacencyListGraph<EntityType>
        {
            public void Populate(params EntityType[] entityTypes)
            {
                AddVertices(entityTypes);

                foreach (var entityType in entityTypes)
                {
                    foreach (var foreignKey in entityType.ForeignKeys)
                    {
                        AddEdge(foreignKey.ReferencedEntityType, foreignKey.EntityType);
                    }
                }
            }
        }

        [Fact]
        public void Sort_simple()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // B -> A -> C
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
                graph.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_reverse()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // C -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeC.Name, entityTypeB.Name, entityTypeA.Name },
                graph.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_preserves_graph()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // B -> A -> C
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
                graph.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());

            Assert.Equal(
                new[] { entityTypeA, entityTypeB, entityTypeC },
                graph.Vertices);

            Assert.Equal(
                new[] { entityTypeC },
                graph.GetOutgoingNeighbours(entityTypeA));

            Assert.Equal(
                new[] { entityTypeA },
                graph.GetOutgoingNeighbours(entityTypeB));

            Assert.Equal(
                new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
                graph.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_tree()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> B, A -> C, C -> B
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P2", typeof(int)), entityTypeC.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                new[] { entityTypeA.Name, entityTypeC.Name, entityTypeB.Name },
                graph.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_no_edges()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A B C
            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeC, entityTypeA, entityTypeB);

            Assert.Equal(
                new[] { entityTypeC.Name, entityTypeA.Name, entityTypeB.Name },
                graph.TopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
        }

        [Fact]
        public void Sort_self_ref()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_circular_direct()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // C, A -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeC, entityTypeA, entityTypeB);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(B).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_circular_transitive()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> C -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_two_cycles()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeD = model.AddEntityType(typeof(D));
            entityTypeD.GetOrSetPrimaryKey(entityTypeD.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeE = model.AddEntityType(typeof(E));
            entityTypeE.GetOrSetPrimaryKey(entityTypeE.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> C -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

            // A -> E -> D -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P2", typeof(int)), entityTypeD.GetPrimaryKey());
            entityTypeD.GetOrAddForeignKey(entityTypeD.GetOrAddProperty("P2", typeof(int)), entityTypeE.GetPrimaryKey());
            entityTypeE.GetOrAddForeignKey(entityTypeE.GetOrAddProperty("P2", typeof(int)), entityTypeA.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC, entityTypeD, entityTypeE);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(A).FullName + " -> " + typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
        }

        [Fact]
        public void Sort_leafy_cycle()
        {
            var model = new Model();

            var entityTypeA = model.AddEntityType(typeof(A));
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = model.AddEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = model.AddEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // C -> B -> C -> A
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());

            var graph = new EntityTypeGraph();
            graph.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(C).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
        }
    }
}

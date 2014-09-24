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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // B -> A -> C
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // C -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // B -> A -> C
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> B, A -> C, C -> B
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P2", typeof(int)), entityTypeC.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // C, A -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> C -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeD = new NamedEntityType(typeof(D));
            entityTypeD.GetOrSetPrimaryKey(entityTypeD.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeE = new NamedEntityType(typeof(E));
            entityTypeE.GetOrSetPrimaryKey(entityTypeE.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // A -> C -> B -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeA.GetPrimaryKey());

            // A -> E -> D -> A
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P2", typeof(int)), entityTypeD.GetPrimaryKey());
            entityTypeD.GetOrAddForeignKey(entityTypeD.GetOrAddProperty("P2", typeof(int)), entityTypeE.GetPrimaryKey());
            entityTypeE.GetOrAddForeignKey(entityTypeE.GetOrAddProperty("P2", typeof(int)), entityTypeA.GetPrimaryKey());

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
            entityTypeA.GetOrSetPrimaryKey(entityTypeA.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeB = new NamedEntityType(typeof(B));
            entityTypeB.GetOrSetPrimaryKey(entityTypeB.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var entityTypeC = new NamedEntityType(typeof(C));
            entityTypeC.GetOrSetPrimaryKey(entityTypeC.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            // C -> B -> C -> A
            entityTypeB.GetOrAddForeignKey(entityTypeB.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());
            entityTypeC.GetOrAddForeignKey(entityTypeC.GetOrAddProperty("P", typeof(int)), entityTypeB.GetPrimaryKey());
            entityTypeA.GetOrAddForeignKey(entityTypeA.GetOrAddProperty("P", typeof(int)), entityTypeC.GetPrimaryKey());

            var model = new EntityTypeGraph();
            model.Populate(entityTypeA, entityTypeB, entityTypeC);

            Assert.Equal(
                Strings.FormatCircularDependency(typeof(C).FullName + " -> " + typeof(B).FullName + " -> " + typeof(C).FullName + " -> " + typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => model.TopologicalSort()).Message);
        }
    }
}

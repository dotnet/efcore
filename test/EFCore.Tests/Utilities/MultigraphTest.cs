// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Utilities;

public class MultigraphTest
{
    #region Fixture

    private class Vertex
    {
        public int Id { get; set; }

        public override string ToString()
            => Id.ToString();
    }

    private class Edge
    {
        public int Id { get; set; }

        public override string ToString()
            => Id.ToString();
    }

    private class A
    {
        public static readonly PropertyInfo PProperty = typeof(A).GetProperty("P");

        public int P { get; set; }
        public int P2 { get; set; }
    }

    private class B
    {
        public static readonly PropertyInfo PProperty = typeof(B).GetProperty("P");

        public int P { get; set; }
        public int P2 { get; set; }
    }

    private class C
    {
        public static readonly PropertyInfo PProperty = typeof(C).GetProperty("P");

        public int P { get; set; }
        public int P2 { get; set; }
    }

    private class D
    {
        public static readonly PropertyInfo PProperty = typeof(D).GetProperty("P");

        public int P { get; set; }
        public int P2 { get; set; }
    }

    private class E
    {
        public static readonly PropertyInfo PProperty = typeof(E).GetProperty("P");

        public int P { get; set; }
        public int P2 { get; set; }
    }

    private class EntityTypeGraph : Multigraph<IReadOnlyEntityType, IReadOnlyForeignKey>
    {
        public void Populate(params IReadOnlyEntityType[] entityTypes)
        {
            AddVertices(entityTypes);

            foreach (var entityType in entityTypes)
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    AddEdge(foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType, foreignKey);
                }
            }
        }

        protected override string ToString(IReadOnlyEntityType vertex)
            => vertex.DisplayName();
    }

    #endregion

    [ConditionalFact]
    public void AddVertex_adds_a_vertex()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>();

        graph.AddVertex(vertexOne);
        graph.AddVertex(vertexTwo);

        Assert.Equal(2, graph.Vertices.Count());
        Assert.Equal(2, graph.Vertices.Intersect(new[] { vertexOne, vertexTwo }).Count());
    }

    [ConditionalFact]
    public void AddVertices_add_vertices()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();

        graph.AddVertices(new[] { vertexOne, vertexTwo });
        graph.AddVertices(new[] { vertexTwo, vertexThree });

        Assert.Equal(3, graph.Vertices.Count());
        Assert.Equal(3, graph.Vertices.Intersect(new[] { vertexOne, vertexTwo, vertexThree }).Count());
    }

    [ConditionalFact]
    public void AddEdge_adds_an_edge()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo });
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        graph.AddEdge(vertexOne, vertexTwo, edgeTwo);

        Assert.Empty(graph.GetEdges(vertexTwo, vertexOne));
        Assert.Equal(2, graph.GetEdges(vertexOne, vertexTwo).Count());
        Assert.Equal(2, graph.GetEdges(vertexOne, vertexTwo).Intersect(new[] { edgeOne, edgeTwo }).Count());
    }

    [ConditionalFact]
    public void AddEdge_updates_incoming_and_outgoing_neighbors()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        graph.AddEdge(vertexOne, vertexThree, edgeTwo);
        graph.AddEdge(vertexTwo, vertexThree, edgeThree);

        Assert.Equal(2, graph.GetOutgoingNeighbors(vertexOne).Count());
        Assert.Equal(2, graph.GetOutgoingNeighbors(vertexOne).Intersect(new[] { vertexTwo, vertexThree }).Count());

        Assert.Equal(2, graph.GetIncomingNeighbors(vertexThree).Count());
        Assert.Equal(2, graph.GetIncomingNeighbors(vertexThree).Intersect(new[] { vertexOne, vertexTwo }).Count());
    }

    [ConditionalFact]
    public void TopologicalSort_on_graph_with_no_edges_returns_all_vertices()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        var result = graph.TopologicalSort();
        Assert.Equal(3, result.Count());
        Assert.Equal(3, result.Intersect(new[] { vertexOne, vertexTwo, vertexThree }).Count());
    }

    [ConditionalFact]
    public void TopologicalSort_on_simple_graph_returns_all_vertices_in_order()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        // 2-> {1}
        graph.AddEdge(vertexTwo, vertexOne, edgeOne);
        // 1 -> {3}
        graph.AddEdge(vertexOne, vertexThree, edgeTwo);

        Assert.Equal(
            [vertexTwo, vertexOne, vertexThree],
            graph.TopologicalSort().ToArray());
    }

    [ConditionalFact]
    public void TopologicalSort_on_tree_graph_returns_all_vertices_in_order()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        // 1 -> {2, 3}
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        graph.AddEdge(vertexOne, vertexThree, edgeTwo);
        // 3 -> {2}
        graph.AddEdge(vertexThree, vertexTwo, edgeThree);

        Assert.Equal(
            [vertexOne, vertexThree, vertexTwo],
            graph.TopologicalSort().ToArray());
    }

    [ConditionalFact]
    public void TopologicalSort_on_self_ref_can_break_cycle()
    {
        var vertexOne = new Vertex { Id = 1 };

        var edgeOne = new Edge { Id = 1 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertex(vertexOne);

        // 1 -> {1}
        graph.AddEdge(vertexOne, vertexOne, edgeOne);

        Assert.Equal(
            [vertexOne],
            graph.TopologicalSort(
                (from, to, edges) =>
                    (from == vertexOne)
                    && (to == vertexOne)
                    && (edges.Intersect(new[] { edgeOne }).Count() == 1)).ToArray());
    }

    [ConditionalFact]
    public void TopologicalSort_can_break_simple_cycle()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        // 1 -> {2}
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        // 2 -> {3}
        graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
        // 3 -> {1}
        graph.AddEdge(vertexThree, vertexOne, edgeThree);

        Assert.Equal(
            [vertexOne, vertexTwo, vertexThree],
            graph.TopologicalSort(
                (from, to, edges) =>
                    (from == vertexThree)
                    && (to == vertexOne)
                    && (edges.Single() == edgeThree)).ToArray());
    }

    [ConditionalFact]
    public void TopologicalSort_can_break_two_cycles()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };
        var vertexFive = new Vertex { Id = 5 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };
        var edgeFour = new Edge { Id = 4 };
        var edgeFive = new Edge { Id = 5 };
        var edgeSix = new Edge { Id = 6 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree, vertexFour, vertexFive });

        // 1 -> {2, 4}
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        graph.AddEdge(vertexOne, vertexFour, edgeTwo);
        // 2 -> {3}
        graph.AddEdge(vertexTwo, vertexThree, edgeThree);
        // 3 -> {1}
        graph.AddEdge(vertexThree, vertexOne, edgeFour);
        // 4 -> {5}
        graph.AddEdge(vertexFour, vertexFive, edgeFive);
        // 5 -> {1}
        graph.AddEdge(vertexFive, vertexOne, edgeSix);

        Assert.Equal(
            [vertexTwo, vertexThree, vertexOne, vertexFour, vertexFive],
            graph.TopologicalSort(
                (from, to, edges) =>
                {
                    var edge = edges.Single();
                    return (edge == edgeOne) || (edge == edgeSix);
                }).ToArray());
    }

    [ConditionalFact]
    public void TopologicalSort_throws_with_default_message_when_cycle_cannot_be_broken()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        // 1 -> {2}
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        // 2 -> {3}
        graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
        // 3 -> {1}
        graph.AddEdge(vertexThree, vertexOne, edgeThree);

        Assert.Equal(
            CoreStrings.CircularDependency(
                string.Join(
                    " ->" + Environment.NewLine, new[] { vertexOne, vertexTwo, vertexThree, vertexOne }.Select(v => v.ToString()))),
            Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
    }

    [ConditionalFact]
    public void TopologicalSort_throws_with_formatted_message_when_cycle_cannot_be_broken()
    {
        const string message = "Formatted cycle";

        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        // 1 -> {2}
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        // 2 -> {3}
        graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
        // 3 -> {1}
        graph.AddEdge(vertexThree, vertexOne, edgeThree);

        Dictionary<Vertex, Tuple<Vertex, Vertex, IEnumerable<Edge>>> cycleData = null;

        string formatter(IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>> data)
        {
            cycleData = data.ToDictionary(entry => entry.Item1);
            return message;
        }

        Assert.Equal(
            CoreStrings.CircularDependency(message),
            Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort(formatter)).Message);

        Assert.Equal(3, cycleData.Count());

        Assert.Equal(vertexTwo, cycleData[vertexOne].Item2);
        Assert.Equal(new[] { edgeOne }, cycleData[vertexOne].Item3);

        Assert.Equal(vertexThree, cycleData[vertexTwo].Item2);
        Assert.Equal(new[] { edgeTwo }, cycleData[vertexTwo].Item3);

        Assert.Equal(vertexOne, cycleData[vertexThree].Item2);
        Assert.Equal(new[] { edgeThree }, cycleData[vertexThree].Item3);
    }

    [ConditionalFact]
    public void TopologicalSort_with_secondary_sort()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>((v1, v2) => Comparer<int>.Default.Compare(v1.Id, v2.Id));
        graph.AddVertices(new[] { vertexFour, vertexThree, vertexTwo, vertexOne });

        // 1 -> {3}
        graph.AddEdge(vertexOne, vertexThree, edgeOne);
        // 2 -> {4}
        graph.AddEdge(vertexTwo, vertexFour, edgeTwo);

        Assert.Equal(
            [vertexOne, vertexTwo, vertexThree, vertexFour],
            graph.TopologicalSort().ToArray());
    }

    [ConditionalFact]
    public void TopologicalSort_without_secondary_sort()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexFour, vertexThree, vertexTwo, vertexOne });

        // 1 -> {3}
        graph.AddEdge(vertexOne, vertexThree, edgeOne);
        // 2 -> {4}
        graph.AddEdge(vertexTwo, vertexFour, edgeTwo);

        Assert.Equal(
            [vertexTwo, vertexOne, vertexFour, vertexThree],
            graph.TopologicalSort().ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_throws_with_formatted_message_when_cycle_cannot_be_broken()
    {
        const string message = "Formatted cycle";

        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

        // 1 -> {2}
        graph.AddEdge(vertexOne, vertexTwo, edgeOne);
        // 2 -> {3}
        graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
        // 3 -> {1}
        graph.AddEdge(vertexThree, vertexOne, edgeThree);

        Dictionary<Vertex, Tuple<Vertex, Vertex, IEnumerable<Edge>>> cycleData = null;

        string formatter(IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>> data)
        {
            cycleData = data.ToDictionary(entry => entry.Item1);
            return message;
        }

        Assert.Equal(
            CoreStrings.CircularDependency(message),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort(null, formatter)).Message);

        Assert.Equal(3, cycleData.Count());

        Assert.Equal(vertexTwo, cycleData[vertexOne].Item2);
        Assert.Equal(new[] { edgeOne }, cycleData[vertexOne].Item3);

        Assert.Equal(vertexThree, cycleData[vertexTwo].Item2);
        Assert.Equal(new[] { edgeTwo }, cycleData[vertexTwo].Item3);

        Assert.Equal(vertexOne, cycleData[vertexThree].Item2);
        Assert.Equal(new[] { edgeThree }, cycleData[vertexThree].Item3);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_throws_with_formatted_message_with_no_tail_when_cycle_cannot_be_broken()
    {
        const string message = "Formatted cycle";

        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };
        var edgeThree = new Edge { Id = 3 };
        var edgeFour = new Edge { Id = 4 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree, vertexFour });

        // 2 -> {1}
        graph.AddEdge(vertexTwo, vertexOne, edgeOne);
        // 3 -> {2}
        graph.AddEdge(vertexThree, vertexTwo, edgeTwo);
        // 4 -> {3}
        graph.AddEdge(vertexFour, vertexThree, edgeThree);
        // 3 -> {4}
        graph.AddEdge(vertexThree, vertexFour, edgeFour);

        Dictionary<Vertex, Tuple<Vertex, Vertex, IEnumerable<Edge>>> cycleData = null;

        string formatter(IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>> data)
        {
            cycleData = data.ToDictionary(entry => entry.Item1);
            return message;
        }

        Assert.Equal(
            CoreStrings.CircularDependency(message),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort(null, formatter)).Message);

        Assert.Equal(2, cycleData.Count);

        Assert.Equal(vertexFour, cycleData[vertexThree].Item2);
        Assert.Equal(new[] { edgeFour }, cycleData[vertexThree].Item3);

        Assert.Equal(vertexThree, cycleData[vertexFour].Item2);
        Assert.Equal(new[] { edgeThree }, cycleData[vertexFour].Item3);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_simple()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // B -> A -> C
        entityTypeC.AddForeignKey(entityTypeC.AddProperty(C.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC);

        Assert.Equal(
            new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
            graph.BatchingTopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_reverse()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // C -> B -> A
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);
        entityTypeB.AddForeignKey(entityTypeB.AddProperty(B.PProperty), entityTypeC.FindPrimaryKey(), entityTypeC);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC);

        Assert.Equal(
            new[] { entityTypeC.Name, entityTypeB.Name, entityTypeA.Name },
            graph.BatchingTopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_preserves_graph()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // B -> A -> C
        entityTypeC.AddForeignKey(entityTypeC.AddProperty(C.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC);

        Assert.Equal(
            new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
            graph.BatchingTopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());

        Assert.Equal(
            new[] { entityTypeA, entityTypeB, entityTypeC },
            graph.Vertices);

        Assert.Equal(
            new[] { entityTypeC },
            graph.GetOutgoingNeighbors(entityTypeA));

        Assert.Equal(
            new[] { entityTypeA },
            graph.GetOutgoingNeighbors(entityTypeB));

        Assert.Equal(
            new[] { entityTypeB.Name, entityTypeA.Name, entityTypeC.Name },
            graph.BatchingTopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_tree()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // A -> B, A -> C, C -> B
        entityTypeB.AddForeignKey(entityTypeB.AddProperty(B.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);
        entityTypeC.AddForeignKey(entityTypeC.AddProperty(C.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);
        entityTypeB.AddForeignKey(entityTypeB.AddProperty("P2", typeof(int)), entityTypeC.FindPrimaryKey(), entityTypeC);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC);

        Assert.Equal(
            new[] { entityTypeA.Name, entityTypeC.Name, entityTypeB.Name },
            graph.BatchingTopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_no_edges()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // A B C
        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeC, entityTypeA, entityTypeB);

        Assert.Equal(
            new[] { entityTypeC.Name, entityTypeA.Name, entityTypeB.Name },
            graph.BatchingTopologicalSort().SelectMany(e => e).Select(e => e.Name).ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_self_ref()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        var property = entityTypeA.AddProperty("Id", typeof(int));
        entityTypeA.SetPrimaryKey(property);

        // A -> A
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA);

        Assert.Equal(
            CoreStrings.CircularDependency(nameof(A) + " ->" + Environment.NewLine + nameof(A)),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort()).Message);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_circular_direct()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // C, A -> B -> A
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);
        entityTypeB.AddForeignKey(entityTypeB.AddProperty(B.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeC, entityTypeA, entityTypeB);

        Assert.Equal(
            CoreStrings.CircularDependency(
                nameof(A) + " ->" + Environment.NewLine + nameof(B) + " ->" + Environment.NewLine + nameof(A)),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort()).Message);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_circular_transitive()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // A -> C -> B -> A
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);
        entityTypeB.AddForeignKey(entityTypeB.AddProperty(B.PProperty), entityTypeC.FindPrimaryKey(), entityTypeC);
        entityTypeC.AddForeignKey(entityTypeC.AddProperty(C.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC);

        Assert.Equal(
            CoreStrings.CircularDependency(
                nameof(A)
                + " ->"
                + Environment.NewLine
                + nameof(C)
                + " ->"
                + Environment.NewLine
                + nameof(B)
                + " ->"
                + Environment.NewLine
                + nameof(A)),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort()).Message);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_two_cycles()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        var entityTypeD = model.AddEntityType(typeof(D));
        entityTypeD.SetPrimaryKey(entityTypeD.AddProperty("Id", typeof(int)));

        var entityTypeE = model.AddEntityType(typeof(E));
        entityTypeE.SetPrimaryKey(entityTypeE.AddProperty("Id", typeof(int)));

        // A -> C -> B -> A
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);
        entityTypeB.AddForeignKey(entityTypeB.AddProperty(B.PProperty), entityTypeC.FindPrimaryKey(), entityTypeC);
        entityTypeC.AddForeignKey(entityTypeC.AddProperty(C.PProperty), entityTypeA.FindPrimaryKey(), entityTypeA);

        // A -> E -> D -> A
        entityTypeA.AddForeignKey(entityTypeA.AddProperty("P2", typeof(int)), entityTypeD.FindPrimaryKey(), entityTypeD);
        entityTypeD.AddForeignKey(entityTypeD.AddProperty("P2", typeof(int)), entityTypeE.FindPrimaryKey(), entityTypeE);
        entityTypeE.AddForeignKey(entityTypeE.AddProperty("P2", typeof(int)), entityTypeA.FindPrimaryKey(), entityTypeA);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC, entityTypeD, entityTypeE);

        Assert.Equal(
            CoreStrings.CircularDependency(
                nameof(A)
                + " ->"
                + Environment.NewLine
                + nameof(C)
                + " ->"
                + Environment.NewLine
                + nameof(B)
                + " ->"
                + Environment.NewLine
                + nameof(A)),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort()).Message);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_sorts_leafy_cycle()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        entityTypeA.SetPrimaryKey(entityTypeA.AddProperty("Id", typeof(int)));

        var entityTypeB = model.AddEntityType(typeof(B));
        entityTypeB.SetPrimaryKey(entityTypeB.AddProperty("Id", typeof(int)));

        var entityTypeC = model.AddEntityType(typeof(C));
        entityTypeC.SetPrimaryKey(entityTypeC.AddProperty("Id", typeof(int)));

        // C -> B -> C -> A
        entityTypeB.AddForeignKey(entityTypeB.AddProperty(B.PProperty), entityTypeC.FindPrimaryKey(), entityTypeC);
        entityTypeC.AddForeignKey(entityTypeC.AddProperty(C.PProperty), entityTypeB.FindPrimaryKey(), entityTypeB);
        entityTypeA.AddForeignKey(entityTypeA.AddProperty(A.PProperty), entityTypeC.FindPrimaryKey(), entityTypeC);

        var graph = new EntityTypeGraph();
        graph.Populate(entityTypeA, entityTypeB, entityTypeC);

        Assert.Equal(
            CoreStrings.CircularDependency(
                nameof(C) + " ->" + Environment.NewLine + nameof(B) + " ->" + Environment.NewLine + nameof(C)),
            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort()).Message);
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_with_secondary_sort()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>((v1, v2) => Comparer<int>.Default.Compare(v1.Id, v2.Id));
        graph.AddVertices(new[] { vertexFour, vertexThree, vertexTwo, vertexOne });

        // 1 -> {3}
        graph.AddEdge(vertexOne, vertexThree, edgeOne);
        // 2 -> {4}
        graph.AddEdge(vertexTwo, vertexFour, edgeTwo);

        Assert.Equal(
            [vertexOne, vertexTwo, vertexThree, vertexFour],
            graph.BatchingTopologicalSort().Single().ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_without_secondary_sort()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>();
        graph.AddVertices(new[] { vertexFour, vertexThree, vertexTwo, vertexOne });

        // 1 -> {3}
        graph.AddEdge(vertexOne, vertexThree, edgeOne);
        // 2 -> {4}
        graph.AddEdge(vertexTwo, vertexFour, edgeTwo);

        Assert.Equal(
            [vertexTwo, vertexOne, vertexFour, vertexThree],
            graph.BatchingTopologicalSort().Single().ToArray());
    }

    [ConditionalFact]
    public void BatchingTopologicalSort_with_batching_boundary_edge()
    {
        var vertexOne = new Vertex { Id = 1 };
        var vertexTwo = new Vertex { Id = 2 };
        var vertexThree = new Vertex { Id = 3 };
        var vertexFour = new Vertex { Id = 4 };

        var edgeOne = new Edge { Id = 1 };
        var edgeTwo = new Edge { Id = 2 };

        var graph = new Multigraph<Vertex, Edge>((v1, v2) => Comparer<int>.Default.Compare(v1.Id, v2.Id));
        graph.AddVertices(new[] { vertexFour, vertexThree, vertexTwo, vertexOne });

        // 1 -> {3}
        graph.AddEdge(vertexOne, vertexThree, edgeOne, requiresBatchingBoundary: true);
        // 2 -> {4}
        graph.AddEdge(vertexTwo, vertexFour, edgeTwo);

        var batches = graph.BatchingTopologicalSort();

        Assert.Collection(
            batches,
            b => Assert.Equal([vertexOne, vertexTwo], b.ToArray()),
            b => Assert.Equal([vertexThree, vertexFour], b.ToArray()));
    }

    private static IMutableModel CreateModel()
        => new Model();
}

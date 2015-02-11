using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class MultigraphTest
    {
        #region Fixture

        private class Vertex
        {
            public int Id { get; set; }

            public override string ToString()
            {
                return Id.ToString();
            }
        }

        private class Edge
        {
            public int Id { get; set; }

            public override string ToString()
            {
                return Id.ToString();
            }
        }

        #endregion

        [Fact]
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

        [Fact]
        public void AddVertices_add_verticies()
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

        [Fact]
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

            Assert.Equal(2, graph.Edges.Count());
            Assert.Equal(2, graph.Edges.Intersect(new[] { edgeOne, edgeTwo }).Count());

            Assert.Equal(0, graph.GetEdges(vertexTwo, vertexOne).Count());
            Assert.Equal(2, graph.GetEdges(vertexOne, vertexTwo).Count());
            Assert.Equal(2, graph.GetEdges(vertexOne, vertexTwo).Intersect(new[] { edgeOne, edgeTwo }).Count());
        }

        [Fact]
        public void AddEdge_throws_on_verticies_not_in_the_graph()
        {
            var vertexOne = new Vertex { Id = 1 };
            var vertexTwo = new Vertex { Id = 2 };

            var edgeOne = new Edge { Id = 1 };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertex(vertexOne);

            Assert.Equal(
                Strings.GraphDoesNotContainVertex(vertexTwo),
                Assert.Throws<InvalidOperationException>(() => graph.AddEdge(vertexOne, vertexTwo, edgeOne)).Message);

            Assert.Equal(
                Strings.GraphDoesNotContainVertex(vertexTwo),
                Assert.Throws<InvalidOperationException>(() => graph.AddEdge(vertexTwo, vertexOne, edgeOne)).Message);
        }

        [Fact]
        public void AddEdges_adds_multiple_edges()
        {
            var vertexOne = new Vertex { Id = 1 };
            var vertexTwo = new Vertex { Id = 2 };

            var edgeOne = new Edge { Id = 1 };
            var edgeTwo = new Edge { Id = 2 };
            var edgeThree = new Edge { Id = 3 };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo });
            graph.AddEdges(vertexOne, vertexTwo, new[] { edgeOne });
            graph.AddEdges(vertexOne, vertexTwo, new[] { edgeTwo, edgeThree });

            Assert.Equal(0, graph.GetEdges(vertexTwo, vertexOne).Count());
            Assert.Equal(3, graph.GetEdges(vertexOne, vertexTwo).Count());
            Assert.Equal(3, graph.GetEdges(vertexOne, vertexTwo).Intersect(new[] { edgeOne, edgeTwo, edgeThree }).Count());
        }

        [Fact]
        public void AddEdges_throws_on_verticies_not_in_the_graph()
        {
            var vertexOne = new Vertex { Id = 1 };
            var vertexTwo = new Vertex { Id = 2 };

            var edgeOne = new Edge { Id = 1 };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertex(vertexOne);

            Assert.Equal(
                Strings.GraphDoesNotContainVertex(vertexTwo),
                Assert.Throws<InvalidOperationException>(() => graph.AddEdges(vertexOne, vertexTwo, new[] { edgeOne })).Message);

            Assert.Equal(
                Strings.GraphDoesNotContainVertex(vertexTwo),
                Assert.Throws<InvalidOperationException>(() => graph.AddEdges(vertexTwo, vertexOne, new[] { edgeOne })).Message);
        }

        [Fact]
        public void AddEdge_updates_incomming_and_outgoing_neighbours()
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

            Assert.Equal(2, graph.GetOutgoingNeighbours(vertexOne).Count());
            Assert.Equal(2, graph.GetOutgoingNeighbours(vertexOne).Intersect(new[] { vertexTwo, vertexThree }).Count());

            Assert.Equal(2, graph.GetIncomingNeighbours(vertexThree).Count());
            Assert.Equal(2, graph.GetIncomingNeighbours(vertexThree).Intersect(new[] { vertexOne, vertexTwo }).Count());
        }

        [Fact]
        public void TopologicalSort_on_graph_with_no_edges_returns_all_verticies()
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

        [Fact]
        public void TopologicalSort_on_simple_graph_returns_all_verticies_in_order()
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
                new[] { vertexTwo, vertexOne, vertexThree },
                graph.TopologicalSort().ToArray());
        }

        [Fact]
        public void TopologicalSort_on_tree_graph_returns_all_verticies_in_order()
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
                new[] { vertexOne, vertexThree, vertexTwo },
                graph.TopologicalSort().ToArray());
        }

        [Fact]
        public void TopologicalSort_on_self_ref_can_break_cycle()
        {
            var vertexOne = new Vertex { Id = 1 };

            var edgeOne = new Edge { Id = 1 };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertex(vertexOne);

            // 1 -> {1}
            graph.AddEdge(vertexOne, vertexOne, edgeOne);

            Assert.Equal(
                new[] { vertexOne },
                graph.TopologicalSort((from, to, edges) =>
                        from == vertexOne &&
                        to == vertexOne &&
                        edges.Intersect(new[] { edgeOne }).Count() == 1).ToArray());
        }

        [Fact]
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
                new[] { vertexOne, vertexTwo, vertexThree },
                graph.TopologicalSort(
                    (from, to, edges) =>
                        from == vertexThree &&
                        to == vertexOne &&
                        edges.Single() == edgeThree).ToArray());
        }

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
                new[] { vertexTwo, vertexThree, vertexOne, vertexFour, vertexFive },
                graph.TopologicalSort(
                    (from, to, edges) =>
                    {
                        var edge = edges.Single();
                        return edge == edgeOne || edge == edgeSix;
                    }).ToArray());
        }

        [Fact]
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
                Strings.CircularDependency(string.Join(" -> ", new[] { vertexOne, vertexTwo, vertexThree, vertexOne }.Select(v => v.ToString()))),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort()).Message);
        }

        [Fact]
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

            Func<IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>>, string> formatter = data =>
            {
                cycleData = data.ToDictionary(entry => entry.Item1);
                return message;
            };

            Assert.Equal(
                Strings.CircularDependency(message),
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort(formatter)).Message);

            Assert.Equal(3, cycleData.Count());

            Assert.Equal(vertexTwo, cycleData[vertexOne].Item2);
            Assert.Equal(new[] { edgeOne }, cycleData[vertexOne].Item3);

            Assert.Equal(vertexThree, cycleData[vertexTwo].Item2);
            Assert.Equal(new[] { edgeTwo }, cycleData[vertexTwo].Item3);

            Assert.Equal(vertexOne, cycleData[vertexThree].Item2);
            Assert.Equal(new[] { edgeThree }, cycleData[vertexThree].Item3);
        }
    }
}

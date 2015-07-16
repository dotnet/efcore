// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public class Multigraph<TVertex, TEdge> : Graph<TVertex>
    {
        private readonly HashSet<TVertex> _vertices = new HashSet<TVertex>();
        private readonly HashSet<TEdge> _edges = new HashSet<TEdge>();
        private readonly Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>> _successorMap = new Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>>();

        public virtual IEnumerable<TEdge> Edges => _edges;

        public virtual IEnumerable<TEdge> GetEdges([NotNull] TVertex from, [NotNull] TVertex to)
        {
            Dictionary<TVertex, List<TEdge>> successorSet;
            if (_successorMap.TryGetValue(from, out successorSet))
            {
                List<TEdge> edgeList;
                if (successorSet.TryGetValue(to, out edgeList))
                {
                    return edgeList;
                }
            }
            return Enumerable.Empty<TEdge>();
        }

        public virtual void AddVertex([NotNull] TVertex vertex)
            => _vertices.Add(vertex);

        public virtual void AddVertices([NotNull] IEnumerable<TVertex> verticies)
            => _vertices.UnionWith(verticies);

        public virtual void AddEdge([NotNull] TVertex from, [NotNull] TVertex to, [NotNull] TEdge edge)
            => AddEdges(@from, to, new[] { edge });

        public virtual void AddEdges([NotNull] TVertex from, [NotNull] TVertex to, [NotNull] IEnumerable<TEdge> edges)
        {
            if (!_vertices.Contains(from))
            {
                throw new InvalidOperationException(Strings.GraphDoesNotContainVertex(from));
            }

            if (!_vertices.Contains(to))
            {
                throw new InvalidOperationException(Strings.GraphDoesNotContainVertex(to));
            }

            Dictionary<TVertex, List<TEdge>> successorSet;
            if (!_successorMap.TryGetValue(from, out successorSet))
            {
                successorSet = new Dictionary<TVertex, List<TEdge>>();
                _successorMap.Add(from, successorSet);
            }

            List<TEdge> edgeList;
            if (!successorSet.TryGetValue(to, out edgeList))
            {
                edgeList = new List<TEdge>();
                successorSet.Add(to, edgeList);
            }

            edgeList.AddRange(edges);
            _edges.UnionWith(edges);
        }

        public virtual IReadOnlyList<TVertex> TopologicalSort() => TopologicalSort(null, null);

        public virtual IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<TVertex, TVertex, IEnumerable<TEdge>, bool> canBreakEdge)
            => TopologicalSort(canBreakEdge, null);

        public virtual IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
            => TopologicalSort(null, formatCycle);

        public virtual IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<TVertex, TVertex, IEnumerable<TEdge>, bool> canBreakEdge,
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var sortedQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                var count = GetIncomingNeighbours(vertex).Count();
                if (count == 0)
                {
                    // Collect verticies without predecessors
                    sortedQueue.Add(vertex);
                }
                else
                {
                    // Track number of predecessors for remaining verticies
                    predecessorCounts[vertex] = count;
                }
            }

            var index = 0;
            while (sortedQueue.Count < _vertices.Count)
            {
                while (index < sortedQueue.Count)
                {
                    var currentRoot = sortedQueue[index];

                    foreach (var successor in GetOutgoingNeighbours(currentRoot).Where(neighbour => predecessorCounts.ContainsKey(neighbour)))
                    {
                        // Decrement counts for edges from sorted verticies and append any verticies that no longer have predecessors
                        predecessorCounts[successor]--;
                        if (predecessorCounts[successor] == 0)
                        {
                            sortedQueue.Add(successor);
                            predecessorCounts.Remove(successor);
                        }
                    }
                    index++;
                }

                // Cycle breaking
                if (sortedQueue.Count < _vertices.Count)
                {
                    var broken = false;

                    var candidateVertices = predecessorCounts.Keys.ToList();
                    var candidateIndex = 0;

                    // Iterrate over the unsorted verticies
                    while (candidateIndex < candidateVertices.Count
                           && !broken
                           && canBreakEdge != null)
                    {
                        var candidateVertex = candidateVertices[candidateIndex];

                        // Find verticies in the unsorted portion of the graph that have edges to the candidate
                        var incommingNeighbours = GetIncomingNeighbours(candidateVertex)
                            .Where(neighbour => predecessorCounts.ContainsKey(neighbour)).ToList();

                        foreach (var incommingNeighbour in incommingNeighbours)
                        {
                            // Check to see if the edge can be broken
                            if (canBreakEdge(incommingNeighbour, candidateVertex, _successorMap[incommingNeighbour][candidateVertex]))
                            {
                                predecessorCounts[candidateVertex]--;
                                if (predecessorCounts[candidateVertex] == 0)
                                {
                                    sortedQueue.Add(candidateVertex);
                                    predecessorCounts.Remove(candidateVertex);
                                    broken = true;
                                    break;
                                }
                            }
                        }
                        candidateIndex++;
                    }
                    if (!broken)
                    {
                        // Failed to break the cycle
                        var currentCycleVertex = predecessorCounts.First().Key;
                        var cycle = new List<TVertex>();
                        cycle.Add(currentCycleVertex);
                        var finished = false;
                        while (!finished)
                        {
                            // Find a cycle
                            foreach (var predecessor in GetIncomingNeighbours(currentCycleVertex)
                                .Where(neighbour => predecessorCounts.ContainsKey(neighbour)))
                            {
                                if (predecessorCounts[predecessor] != 0)
                                {
                                    predecessorCounts[currentCycleVertex] = -1;

                                    currentCycleVertex = predecessor;
                                    cycle.Add(currentCycleVertex);
                                    finished = predecessorCounts[predecessor] == -1;
                                    break;
                                }
                            }
                        }
                        cycle.Reverse();

                        // Throw an exception
                        if (formatCycle == null)
                        {
                            throw new InvalidOperationException(
                                Strings.CircularDependency(
                                    cycle.Select(vertex => vertex.ToString()).Join(" -> ")));
                        }
                        // Build the cycle message data
                        currentCycleVertex = cycle.First();
                        var cycleData = new List<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>();

                        foreach (var vertex in cycle.Skip(1))
                        {
                            cycleData.Add(Tuple.Create(currentCycleVertex, vertex, GetEdges(currentCycleVertex, vertex)));
                            currentCycleVertex = vertex;
                        }
                        throw new InvalidOperationException(
                            Strings.CircularDependency(
                                formatCycle(cycleData)));
                    }
                }
            }
            return sortedQueue;
        }

        public virtual IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
            => BatchingTopologicalSort(null);

        public virtual IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var currentRootsQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                var count = GetIncomingNeighbours(vertex).Count();
                if (count == 0)
                {
                    // Collect verticies without predecessors
                    currentRootsQueue.Add(vertex);
                }
                else
                {
                    // Track number of predecessors for remaining verticies
                    predecessorCounts[vertex] = count;
                }
            }

            var result = new List<List<TVertex>>();
            var nextRootsQueue = new List<TVertex>();
            var currentRootIndex = 0;

            while (currentRootIndex < currentRootsQueue.Count)
            {
                var currentRoot = currentRootsQueue[currentRootIndex];
                currentRootIndex++;

                // Remove edges from current root and add any exposed vertices to the next batch
                foreach (var successor in GetOutgoingNeighbours(currentRoot))
                {
                    predecessorCounts[successor]--;
                    if (predecessorCounts[successor] == 0)
                    {
                        nextRootsQueue.Add(successor);
                    }
                }

                // Roll lists over for next batch
                if (currentRootIndex == currentRootsQueue.Count)
                {
                    result.Add(currentRootsQueue);

                    currentRootsQueue = nextRootsQueue;
                    currentRootIndex = 0;

                    if (currentRootsQueue.Count != 0)
                    {
                        nextRootsQueue = new List<TVertex>();
                    }
                }
            }

            if (result.Sum(b => b.Count) != _vertices.Count)
            {
                // TODO: Support cycle-breaking?

                var currentCycleVertex = predecessorCounts.First(p => p.Value != 0).Key;
                var cycle = new List<TVertex>();
                cycle.Add(currentCycleVertex);
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in GetIncomingNeighbours(currentCycleVertex)
                        .Where(neighbour => predecessorCounts.ContainsKey(neighbour)))
                    {
                        if (predecessorCounts[predecessor] != 0)
                        {
                            predecessorCounts[currentCycleVertex] = -1;

                            currentCycleVertex = predecessor;
                            cycle.Add(currentCycleVertex);
                            finished = predecessorCounts[predecessor] == -1;
                            break;
                        }
                    }
                }
                cycle.Reverse();

                // Throw an exception
                if (formatCycle == null)
                {
                    throw new InvalidOperationException(
                        Strings.CircularDependency(
                            cycle.Select(vertex => vertex.ToString()).Join(" -> ")));
                }
                // Build the cycle message data
                currentCycleVertex = cycle.First();
                var cycleData = new List<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>();

                foreach (var vertex in cycle.Skip(1))
                {
                    cycleData.Add(Tuple.Create(currentCycleVertex, vertex, GetEdges(currentCycleVertex, vertex)));
                    currentCycleVertex = vertex;
                }
                throw new InvalidOperationException(
                    Strings.CircularDependency(
                        formatCycle(cycleData)));
            }

            return result;
        }

        public override IEnumerable<TVertex> Vertices => _vertices;

        public override IEnumerable<TVertex> GetOutgoingNeighbours([NotNull] TVertex from)
        {
            Dictionary<TVertex, List<TEdge>> successorSet;

            return _successorMap.TryGetValue(@from, out successorSet)
                ? successorSet.Keys
                : Enumerable.Empty<TVertex>();
        }

        public override IEnumerable<TVertex> GetIncomingNeighbours([NotNull] TVertex to)
            => _successorMap.Where(kvp => kvp.Value.ContainsKey(to)).Select(kvp => kvp.Key);
    }
}

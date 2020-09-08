// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal class Multigraph<TVertex, TEdge> : Graph<TVertex>
    {
        private readonly HashSet<TVertex> _vertices = new HashSet<TVertex>();

        private readonly Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>> _successorMap =
            new Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>>();

        private readonly Dictionary<TVertex, HashSet<TVertex>> _predecessorMap =
            new Dictionary<TVertex, HashSet<TVertex>>();

        public IEnumerable<TEdge> Edges
            => _successorMap.Values.SelectMany(s => s.Values).SelectMany(e => e).Distinct();

        public IEnumerable<TEdge> GetEdges([NotNull] TVertex from, [NotNull] TVertex to)
        {
            if (_successorMap.TryGetValue(from, out var successorSet))
            {
                if (successorSet.TryGetValue(to, out var edgeList))
                {
                    return edgeList;
                }
            }

            return Enumerable.Empty<TEdge>();
        }

        public void AddVertex([NotNull] TVertex vertex)
            => _vertices.Add(vertex);

        public void AddVertices([NotNull] IEnumerable<TVertex> vertices)
            => _vertices.UnionWith(vertices);

        public void AddEdge([NotNull] TVertex from, [NotNull] TVertex to, [CanBeNull] TEdge edge)
        {
#if DEBUG
            if (!_vertices.Contains(from))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(from));
            }

            if (!_vertices.Contains(to))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(to));
            }
#endif

            if (!_successorMap.TryGetValue(from, out var successorEdges))
            {
                successorEdges = new Dictionary<TVertex, List<TEdge>>();
                _successorMap.Add(from, successorEdges);
            }

            if (!successorEdges.TryGetValue(to, out var edgeList))
            {
                edgeList = new List<TEdge>();
                successorEdges.Add(to, edgeList);
            }

            edgeList.Add(edge);

            if (!_predecessorMap.TryGetValue(to, out var predecessors))
            {
                predecessors = new HashSet<TVertex>();
                _predecessorMap.Add(to, predecessors);
            }

            predecessors.Add(from);
        }

        public void AddEdges([NotNull] TVertex from, [NotNull] TVertex to, [NotNull] IEnumerable<TEdge> edges)
        {
#if DEBUG
            if (!_vertices.Contains(from))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(from));
            }

            if (!_vertices.Contains(to))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(to));
            }
#endif

            if (!_successorMap.TryGetValue(from, out var successorEdges))
            {
                successorEdges = new Dictionary<TVertex, List<TEdge>>();
                _successorMap.Add(from, successorEdges);
            }

            if (!successorEdges.TryGetValue(to, out var edgeList))
            {
                edgeList = new List<TEdge>();
                successorEdges.Add(to, edgeList);
            }

            edgeList.AddRange(edges);

            if (!_predecessorMap.TryGetValue(to, out var predecessors))
            {
                predecessors = new HashSet<TVertex>();
                _predecessorMap.Add(to, predecessors);
            }

            predecessors.Add(from);
        }

        public override void Clear()
        {
            _vertices.Clear();
            _successorMap.Clear();
            _predecessorMap.Clear();
        }

        public IReadOnlyList<TVertex> TopologicalSort()
            => TopologicalSort(null, null);

        public IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<TVertex, TVertex, IEnumerable<TEdge>, bool> tryBreakEdge)
            => TopologicalSort(tryBreakEdge, null);

        public IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
            => TopologicalSort(null, formatCycle);

        public IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<TVertex, TVertex, IEnumerable<TEdge>, bool> tryBreakEdge,
            [CanBeNull] Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var sortedQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                foreach (var outgoingNeighbor in GetOutgoingNeighbors(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbor))
                    {
                        predecessorCounts[outgoingNeighbor]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbor] = 1;
                    }
                }
            }

            foreach (var vertex in _vertices)
            {
                if (!predecessorCounts.ContainsKey(vertex))
                {
                    sortedQueue.Add(vertex);
                }
            }

            var index = 0;
            while (sortedQueue.Count < _vertices.Count)
            {
                while (index < sortedQueue.Count)
                {
                    var currentRoot = sortedQueue[index];

                    foreach (var successor in GetOutgoingNeighbors(currentRoot).Where(neighbor => predecessorCounts.ContainsKey(neighbor)))
                    {
                        // Decrement counts for edges from sorted vertices and append any vertices that no longer have predecessors
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

                    // Iterate over the unsorted vertices
                    while ((candidateIndex < candidateVertices.Count)
                        && !broken
                        && tryBreakEdge != null)
                    {
                        var candidateVertex = candidateVertices[candidateIndex];

                        // Find vertices in the unsorted portion of the graph that have edges to the candidate
                        var incomingNeighbors = GetIncomingNeighbors(candidateVertex)
                            .Where(neighbor => predecessorCounts.ContainsKey(neighbor)).ToList();

                        foreach (var incomingNeighbor in incomingNeighbors)
                        {
                            // Check to see if the edge can be broken
                            if (tryBreakEdge(incomingNeighbor, candidateVertex, _successorMap[incomingNeighbor][candidateVertex]))
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
                        var currentCycleVertex = _vertices.First(v => predecessorCounts.ContainsKey(v));
                        var cycle = new List<TVertex> { currentCycleVertex };
                        var finished = false;
                        while (!finished)
                        {
                            // Find a cycle
                            foreach (var predecessor in GetIncomingNeighbors(currentCycleVertex)
                                .Where(neighbor => predecessorCounts.ContainsKey(neighbor)))
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

                        ThrowCycle(cycle, formatCycle);
                    }
                }
            }

            return sortedQueue;
        }

        private void ThrowCycle(List<TVertex> cycle, Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            string cycleString;
            if (formatCycle == null)
            {
                cycleString = cycle.Select(ToString).Join(" ->" + Environment.NewLine);
            }
            else
            {
                var currentCycleVertex = cycle.First();
                var cycleData = new List<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>();

                foreach (var vertex in cycle.Skip(1))
                {
                    cycleData.Add(Tuple.Create(currentCycleVertex, vertex, GetEdges(currentCycleVertex, vertex)));
                    currentCycleVertex = vertex;
                }

                cycleString = formatCycle(cycleData);
            }

            throw new InvalidOperationException(CoreStrings.CircularDependency(cycleString));
        }

        protected virtual string ToString(TVertex vertex)
            => vertex.ToString();

        public IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
            => BatchingTopologicalSort(null);

        public IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
            [CanBeNull] Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var currentRootsQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                foreach (var outgoingNeighbor in GetOutgoingNeighbors(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbor))
                    {
                        predecessorCounts[outgoingNeighbor]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbor] = 1;
                    }
                }
            }

            foreach (var vertex in _vertices)
            {
                if (!predecessorCounts.ContainsKey(vertex))
                {
                    currentRootsQueue.Add(vertex);
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
                foreach (var successor in GetOutgoingNeighbors(currentRoot))
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
                var currentCycleVertex = _vertices.First(
                    v => predecessorCounts.TryGetValue(v, out var predecessorNumber) ? predecessorNumber != 0 : false);
                var cyclicWalk = new List<TVertex> { currentCycleVertex };
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in GetIncomingNeighbors(currentCycleVertex))
                    {
                        if (!predecessorCounts.TryGetValue(predecessor, out var predecessorCount))
                        {
                            continue;
                        }

                        if (predecessorCount != 0)
                        {
                            predecessorCounts[currentCycleVertex] = -1;

                            currentCycleVertex = predecessor;
                            cyclicWalk.Add(currentCycleVertex);
                            finished = predecessorCounts[predecessor] == -1;
                            break;
                        }
                    }
                }

                cyclicWalk.Reverse();

                var cycle = new List<TVertex>();
                var startingVertex = cyclicWalk.First();
                cycle.Add(startingVertex);
                foreach (var vertex in cyclicWalk.Skip(1))
                {
                    if (!vertex.Equals(startingVertex))
                    {
                        cycle.Add(vertex);
                    }
                    else
                    {
                        break;
                    }
                }

                cycle.Add(startingVertex);

                ThrowCycle(cycle, formatCycle);
            }

            return result;
        }

        public override IEnumerable<TVertex> Vertices
            => _vertices;

        public override IEnumerable<TVertex> GetOutgoingNeighbors(TVertex from)
            => _successorMap.TryGetValue(from, out var successorSet)
                ? successorSet.Keys
                : Enumerable.Empty<TVertex>();

        public override IEnumerable<TVertex> GetIncomingNeighbors(TVertex to)
            => _predecessorMap.TryGetValue(to, out var predecessors)
                ? predecessors
                : Enumerable.Empty<TVertex>();
    }
}

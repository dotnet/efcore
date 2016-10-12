// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Multigraph<TVertex, TEdge> : Graph<TVertex>
    {
        private readonly HashSet<TVertex> _vertices = new HashSet<TVertex>();
        private readonly HashSet<TEdge> _edges = new HashSet<TEdge>();
        private readonly Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>> _successorMap = new Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string ToString([NotNull] TVertex vertex) => vertex.ToString();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<TEdge> Edges => _edges;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddVertex([NotNull] TVertex vertex)
            => _vertices.Add(vertex);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddVertices([NotNull] IEnumerable<TVertex> vertices)
            => _vertices.UnionWith(vertices);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEdge([NotNull] TVertex from, [NotNull] TVertex to, [NotNull] TEdge edge)
            => AddEdges(from, to, new[] { edge });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEdges([NotNull] TVertex from, [NotNull] TVertex to, [NotNull] IEnumerable<TEdge> edges)
        {
            if (!_vertices.Contains(from))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(from));
            }

            if (!_vertices.Contains(to))
            {
                throw new InvalidOperationException(CoreStrings.GraphDoesNotContainVertex(to));
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
            _edges.UnionWith(edgeList);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<TVertex> TopologicalSort() => TopologicalSort(null, null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<TVertex, TVertex, IEnumerable<TEdge>, bool> canBreakEdge)
            => TopologicalSort(canBreakEdge, null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
            => TopologicalSort(null, formatCycle);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<TVertex> TopologicalSort(
            [CanBeNull] Func<TVertex, TVertex, IEnumerable<TEdge>, bool> canBreakEdge,
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var sortedQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                foreach (var outgoingNeighbour in GetOutgoingNeighbours(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbour))
                    {
                        predecessorCounts[outgoingNeighbour]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbour] = 1;
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

                    foreach (var successor in GetOutgoingNeighbours(currentRoot).Where(neighbour => predecessorCounts.ContainsKey(neighbour)))
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
                           && (canBreakEdge != null))
                    {
                        var candidateVertex = candidateVertices[candidateIndex];

                        // Find vertices in the unsorted portion of the graph that have edges to the candidate
                        var incomingNeighbours = GetIncomingNeighbours(candidateVertex)
                            .Where(neighbour => predecessorCounts.ContainsKey(neighbour)).ToList();

                        foreach (var incomingNeighbour in incomingNeighbours)
                        {
                            // Check to see if the edge can be broken
                            if (canBreakEdge(incomingNeighbour, candidateVertex, _successorMap[incomingNeighbour][candidateVertex]))
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
                                CoreStrings.CircularDependency(
                                    cycle.Select(ToString).Join(" -> ")));
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
                            CoreStrings.CircularDependency(
                                formatCycle(cycleData)));
                    }
                }
            }
            return sortedQueue;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
            => BatchingTopologicalSort(null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
            [CanBeNull] Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var currentRootsQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                foreach (var outgoingNeighbour in GetOutgoingNeighbours(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbour))
                    {
                        predecessorCounts[outgoingNeighbour]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbour] = 1;
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

                var currentCycleVertex = _vertices.First(v =>
                    {
                        int predecessorNumber;
                        if (predecessorCounts.TryGetValue(v, out predecessorNumber))
                        {
                            return predecessorNumber != 0;
                        }
                        return false;
                    });
                var cyclicWalk = new List<TVertex> { currentCycleVertex };
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in GetIncomingNeighbours(currentCycleVertex))
                    {
                        int predecessorCount;
                        if (!predecessorCounts.TryGetValue(predecessor, out predecessorCount))
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

                // Throw an exception
                if (formatCycle == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.CircularDependency(
                            cycle.Select(ToString).Join(" -> ")));
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
                    CoreStrings.CircularDependency(
                        formatCycle(cycleData)));
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<TVertex> Vertices => _vertices;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<TVertex> GetOutgoingNeighbours(TVertex from)
        {
            Dictionary<TVertex, List<TEdge>> successorSet;

            return _successorMap.TryGetValue(from, out successorSet)
                ? successorSet.Keys
                : Enumerable.Empty<TVertex>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<TVertex> GetIncomingNeighbours(TVertex to)
            => _successorMap.Where(kvp => kvp.Value.ContainsKey(to)).Select(kvp => kvp.Key);
    }
}

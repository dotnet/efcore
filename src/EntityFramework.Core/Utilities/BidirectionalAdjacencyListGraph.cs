// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Utilities
{
    public class BidirectionalAdjacencyListGraph<TVertex> : Graph<TVertex>
    {
        private readonly HashSet<TVertex> _vertices = new HashSet<TVertex>();
        private readonly Dictionary<TVertex, int> _predecessorCounts = new Dictionary<TVertex, int>();
        private readonly Dictionary<TVertex, HashSet<TVertex>> _successorMap = new Dictionary<TVertex, HashSet<TVertex>>();

        public virtual void AddVertex([NotNull] TVertex vertex)
        {
            Check.NotNull(vertex, "vertex");

            _vertices.Add(vertex);
        }

        public virtual void AddVertices([NotNull] IEnumerable<TVertex> vertices)
        {
            Check.NotNull(vertices, "vertices");

            _vertices.UnionWith(vertices);
        }

        public virtual void AddEdge([NotNull] TVertex from, [NotNull] TVertex to)
        {
            Check.NotNull(from, "from");
            Check.NotNull(to, "to");

            if (_vertices.Contains(from)
                && _vertices.Contains(to))
            {
                HashSet<TVertex> successors;
                if (!_successorMap.TryGetValue(from, out successors))
                {
                    successors = new HashSet<TVertex>();
                    _successorMap.Add(from, successors);
                }

                if (successors.Add(to))
                {
                    int predecessorCount;
                    if (!_predecessorCounts.TryGetValue(to, out predecessorCount))
                    {
                        predecessorCount = 1;
                    }
                    else
                    {
                        predecessorCount++;
                    }
                    _predecessorCounts[to] = predecessorCount;
                }
            }
        }

        public virtual IEnumerable<List<TVertex>> TopologicalSort()
        {
            var currentRootsQueue = new List<TVertex>();
            var nextRootsQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>(_predecessorCounts);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var element in _vertices)
            {
                // Create initial batch of verticies without predecessors
                if (!predecessorCounts.ContainsKey(element))
                {
                    currentRootsQueue.Add(element);
                }
            }

            var result = new List<List<TVertex>>();
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

                throw new InvalidOperationException(
                    Strings.CircularDependency(
                        cycle.Select(vertex => vertex.ToString()).Join(" -> ")));
            }

            return result;
        }

        public override IEnumerable<TVertex> Vertices
        {
            get { return _vertices; }
        }

        public override IEnumerable<TVertex> GetOutgoingNeighbours(TVertex from)
        {
            HashSet<TVertex> successorSet;
            if (_successorMap.TryGetValue(from, out successorSet))
            {
                return successorSet;
            }
            return Enumerable.Empty<TVertex>();
        }

        public override IEnumerable<TVertex> GetIncomingNeighbours(TVertex to)
        {
            return from vertexSuccessors in _successorMap
                where vertexSuccessors.Value.Contains(to)
                select vertexSuccessors.Key;
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Utilities
{
    public class BidirectionalAdjacencyListGraph<TVertex> : Graph<TVertex>
    {
        private readonly HashSet<TVertex> _vertices = new HashSet<TVertex>();
        private readonly Dictionary<TVertex, int> _predecessorCounts = new Dictionary<TVertex, int>();
        private readonly Dictionary<TVertex, HashSet<TVertex>> _successorMap = new Dictionary<TVertex, HashSet<TVertex>>();

        public override void AddVertex(TVertex vertex)
        {
            Check.NotNull(vertex, "vertex");

            _vertices.Add(vertex);
        }

        public override void AddVertices(IEnumerable<TVertex> vertices)
        {
            Check.NotNull(vertices, "vertices");

            _vertices.UnionWith(vertices);
        }

        public override void AddEdge(TVertex from, TVertex to)
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

        public override IEnumerable<List<TVertex>> TopologicalSort()
        {
            var currentRootsPriorityQueue = new List<TVertex>();
            var nextRootsPriorityQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>(_predecessorCounts);

            foreach (var element in _vertices)
            {
                if (!predecessorCounts.ContainsKey(element))
                {
                    currentRootsPriorityQueue.Add(element);
                }
            }

            var result = new List<List<TVertex>>();
            var currentRootIndex = 0;

            while (currentRootIndex < currentRootsPriorityQueue.Count)
            {
                var currentRoot = currentRootsPriorityQueue[currentRootIndex];
                currentRootIndex++;

                foreach (var successor in GetOutgoingNeighbours(currentRoot))
                {
                    predecessorCounts[successor]--;
                    if (predecessorCounts[successor] == 0)
                    {
                        nextRootsPriorityQueue.Add(successor);
                    }
                }

                if (currentRootIndex == currentRootsPriorityQueue.Count)
                {
                    result.Add(currentRootsPriorityQueue);

                    currentRootsPriorityQueue = nextRootsPriorityQueue;
                    currentRootIndex = 0;

                    if (currentRootsPriorityQueue.Count != 0)
                    {
                        nextRootsPriorityQueue = new List<TVertex>();
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
                    foreach (var predecessor in GetIncomingNeighbours(currentCycleVertex))
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

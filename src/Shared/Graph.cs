// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal abstract class Graph<TVertex>
    {
        public abstract IEnumerable<TVertex> Vertices { get; }

        public abstract void Clear();

        public abstract IEnumerable<TVertex> GetOutgoingNeighbors(TVertex from);

        public abstract IEnumerable<TVertex> GetIncomingNeighbors(TVertex to);

        public ISet<TVertex> GetUnreachableVertices(IReadOnlyList<TVertex> roots)
        {
            var unreachableVertices = new HashSet<TVertex>(Vertices);
            unreachableVertices.ExceptWith(roots);
            var visitingQueue = new List<TVertex>(roots);

            var currentVertexIndex = 0;
            while (currentVertexIndex < visitingQueue.Count)
            {
                var currentVertex = visitingQueue[currentVertexIndex];
                currentVertexIndex++;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var neighbor in GetOutgoingNeighbors(currentVertex))
                {
                    if (unreachableVertices.Remove(neighbor))
                    {
                        visitingQueue.Add(neighbor);
                    }
                }
            }

            return unreachableVertices;
        }

        public IList<ISet<TVertex>> GetWeaklyConnectedComponents()
        {
            var components = new List<ISet<TVertex>>();
            var unvisitedVertices = new HashSet<TVertex>(Vertices);
            var neighbors = new Queue<TVertex>();
            while (unvisitedVertices.Count > 0)
            {
                var unvisitedVertex = unvisitedVertices.First();
                var currentComponent = new HashSet<TVertex>();

                neighbors.Enqueue(unvisitedVertex);

                while (neighbors.Count > 0)
                {
                    var currentVertex = neighbors.Dequeue();
                    if (currentComponent.Contains(currentVertex))
                    {
                        continue;
                    }

                    currentComponent.Add(currentVertex);
                    unvisitedVertices.Remove(currentVertex);
                    foreach (var neighbor in GetOutgoingNeighbors(currentVertex))
                    {
                        neighbors.Enqueue(neighbor);
                    }

                    foreach (var neighbor in GetIncomingNeighbors(currentVertex))
                    {
                        neighbors.Enqueue(neighbor);
                    }
                }

                components.Add(currentComponent);
            }

            return components;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public abstract class Graph<TVertex>
    {
        public abstract IEnumerable<TVertex> Vertices { get; }

        public abstract IEnumerable<TVertex> GetOutgoingNeighbours([NotNull] TVertex from);

        public abstract IEnumerable<TVertex> GetIncomingNeighbours([NotNull] TVertex to);

        public virtual ISet<TVertex> GetUnreachableVertices([NotNull] IReadOnlyList<TVertex> roots)
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
                foreach (var neighbour in GetOutgoingNeighbours(currentVertex))
                {
                    if (unreachableVertices.Remove(neighbour))
                    {
                        visitingQueue.Add(neighbour);
                    }
                }
            }

            return unreachableVertices;
        }
    }
}

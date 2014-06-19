// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Utilities
{
    public abstract class Graph<TVertex>
    {
        public abstract IEnumerable<TVertex> Vertices { get; }

        public abstract void AddVertex([NotNull] TVertex vertex);

        public virtual void AddVertices([NotNull] IEnumerable<TVertex> vertices)
        {
            Check.NotNull(vertices, "vertices");

            foreach (var vertex in vertices)
            {
                AddVertex(vertex);
            }
        }

        public abstract void AddEdge([NotNull] TVertex from, [NotNull] TVertex to);

        public virtual void AddEdges([NotNull] Dictionary<TVertex, TVertex> edges)
        {
            Check.NotNull(edges, "edges");

            foreach (var edge in edges)
            {
                AddEdge(edge.Key, edge.Value);
            }
        }

        public abstract IEnumerable<TVertex> GetOutgoingNeighbours([NotNull] TVertex from);

        public abstract IEnumerable<TVertex> GetIncomingNeighbours([NotNull] TVertex to);

        public abstract IEnumerable<List<TVertex>> TopologicalSort();
    }
}

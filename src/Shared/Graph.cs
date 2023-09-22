// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Utilities;

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
}

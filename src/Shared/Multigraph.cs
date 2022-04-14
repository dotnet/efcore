// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Utilities;

internal class Multigraph<TVertex, TEdge> : Graph<TVertex>
    where TVertex : notnull
{
    private readonly HashSet<TVertex> _vertices = new();
    private readonly Dictionary<TVertex, Dictionary<TVertex, object?>> _successorMap = new();
    private readonly Dictionary<TVertex, HashSet<TVertex>> _predecessorMap = new();

    public IEnumerable<TEdge> GetEdges(TVertex from, TVertex to)
    {
        if (_successorMap.TryGetValue(from, out var successorSet))
        {
            if (successorSet.TryGetValue(to, out var edges))
            {
                return edges is IEnumerable<TEdge> edgeList ? edgeList : (new[] { (TEdge)edges! });
            }
        }

        return Enumerable.Empty<TEdge>();
    }

    public void AddVertex(TVertex vertex)
        => _vertices.Add(vertex);

    public void AddVertices(IEnumerable<TVertex> vertices)
        => _vertices.UnionWith(vertices);

    public void AddEdge(TVertex from, TVertex to, TEdge edge)
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
            successorEdges = new Dictionary<TVertex, object?>();
            _successorMap.Add(from, successorEdges);
        }

        if (successorEdges.TryGetValue(to, out var edges))
        {
            if (edges is not List<TEdge> edgeList)
            {
                edgeList = new List<TEdge> { (TEdge)edges! };
                successorEdges[to] = edgeList;
            }

            edgeList.Add(edge);
        }
        else
        {
            successorEdges.Add(to, edge);
        }

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
        Func<TVertex, TVertex, IEnumerable<TEdge>, bool> tryBreakEdge)
        => TopologicalSort(tryBreakEdge, null);

    public IReadOnlyList<TVertex> TopologicalSort(
        Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        => TopologicalSort(null, formatCycle);

    public IReadOnlyList<TVertex> TopologicalSort(
        Func<TVertex, TVertex, IEnumerable<TEdge>, bool>? tryBreakEdge,
        Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle,
        Func<string, string>? formatException = null)
    {
        var queue = new List<TVertex>();
        var predecessorCounts = new Dictionary<TVertex, int>(_predecessorMap.Count);
        foreach (var (vertex, vertices) in _predecessorMap)
        {
            predecessorCounts[vertex] = vertices.Count;
        }

        foreach (var vertex in _vertices)
        {
            if (!predecessorCounts.ContainsKey(vertex))
            {
                queue.Add(vertex);
            }
        }

        var index = 0;
        while (queue.Count < _vertices.Count)
        {
            while (index < queue.Count)
            {
                var currentRoot = queue[index];
                index++;

                foreach (var successor in GetOutgoingNeighbors(currentRoot))
                {
                    predecessorCounts[successor]--;
                    if (predecessorCounts[successor] == 0)
                    {
                        queue.Add(successor);
                    }
                }
            }

            // Cycle breaking
            if (queue.Count < _vertices.Count)
            {
                var broken = false;

                var candidateVertices = predecessorCounts.Keys.ToList();
                var candidateIndex = 0;

                while ((candidateIndex < candidateVertices.Count)
                       && !broken
                       && tryBreakEdge != null)
                {
                    var candidateVertex = candidateVertices[candidateIndex];
                    if (predecessorCounts[candidateVertex] == 0)
                    {
                        candidateIndex++;
                        continue;
                    }

                    // Find a vertex in the unsorted portion of the graph that has edges to the candidate
                    var incomingNeighbor = GetIncomingNeighbors(candidateVertex)
                        .First(
                            neighbor => predecessorCounts.TryGetValue(neighbor, out var neighborPredecessors)
                                && neighborPredecessors > 0);

                    if (tryBreakEdge(incomingNeighbor, candidateVertex, GetEdges(incomingNeighbor, candidateVertex)))
                    {
                        _successorMap[incomingNeighbor].Remove(candidateVertex);
                        _predecessorMap[candidateVertex].Remove(incomingNeighbor);
                        predecessorCounts[candidateVertex]--;
                        if (predecessorCounts[candidateVertex] == 0)
                        {
                            queue.Add(candidateVertex);
                            broken = true;
                        }

                        continue;
                    }

                    candidateIndex++;
                }

                if (broken)
                {
                    continue;
                }

                var currentCycleVertex = _vertices.First(
                    v => predecessorCounts.TryGetValue(v, out var predecessorCount) && predecessorCount != 0);
                var cycle = new List<TVertex> { currentCycleVertex };
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in GetIncomingNeighbors(currentCycleVertex))
                    {
                        if (!predecessorCounts.TryGetValue(predecessor, out var predecessorCount)
                            || predecessorCount == 0)
                        {
                            continue;
                        }

                        predecessorCounts[currentCycleVertex] = -1;

                        currentCycleVertex = predecessor;
                        cycle.Add(currentCycleVertex);
                        finished = predecessorCounts[predecessor] == -1;
                        break;
                    }
                }

                cycle.Reverse();

                // Remove any tail that's not part of the cycle
                var startingVertex = cycle[0];
                for (var i = cycle.Count - 1; i >= 0; i--)
                {
                    if (cycle[i].Equals(startingVertex))
                    {
                        break;
                    }

                    cycle.RemoveAt(i);
                }

                ThrowCycle(cycle, formatCycle, formatException);
            }
        }

        return queue;
    }

    private void ThrowCycle(
        List<TVertex> cycle,
        Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle,
        Func<string, string>? formatException = null)
    {
        string cycleString;
        if (formatCycle == null)
        {
            cycleString = cycle.Select(e => ToString(e)!).Join(" ->" + Environment.NewLine);
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

        var message = formatException == null ? CoreStrings.CircularDependency(cycleString) : formatException(cycleString);
        throw new InvalidOperationException(message);
    }

    protected virtual string? ToString(TVertex vertex)
        => vertex.ToString();

    public IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
        => BatchingTopologicalSort(null, null);

    public IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
        Func<TVertex, TVertex, IEnumerable<TEdge>, bool>? tryBreakEdge,
        Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle)
    {
        var currentRootsQueue = new List<TVertex>();
        var predecessorCounts = new Dictionary<TVertex, int>(_predecessorMap.Count);
        foreach (var (vertex, vertices) in _predecessorMap)
        {
            predecessorCounts[vertex] = vertices.Count;
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

        while (result.Sum(b => b.Count) != _vertices.Count)
        {
            var currentRootIndex = 0;
            while (currentRootIndex < currentRootsQueue.Count)
            {
                var currentRoot = currentRootsQueue[currentRootIndex];
                currentRootIndex++;

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

            // Cycle breaking
            if (result.Sum(b => b.Count) != _vertices.Count)
            {
                var broken = false;

                var candidateVertices = predecessorCounts.Keys.ToList();
                var candidateIndex = 0;

                while ((candidateIndex < candidateVertices.Count)
                       && !broken
                       && tryBreakEdge != null)
                {
                    var candidateVertex = candidateVertices[candidateIndex];
                    if (predecessorCounts[candidateVertex] == 0)
                    {
                        candidateIndex++;
                        continue;
                    }

                    // Find a vertex in the unsorted portion of the graph that has edges to the candidate
                    var incomingNeighbor = GetIncomingNeighbors(candidateVertex)
                        .First(
                            neighbor => predecessorCounts.TryGetValue(neighbor, out var neighborPredecessors)
                                && neighborPredecessors > 0);

                    if (tryBreakEdge(incomingNeighbor, candidateVertex, GetEdges(incomingNeighbor, candidateVertex)))
                    {
                        _successorMap[incomingNeighbor].Remove(candidateVertex);
                        _predecessorMap[candidateVertex].Remove(incomingNeighbor);
                        predecessorCounts[candidateVertex]--;
                        if (predecessorCounts[candidateVertex] == 0)
                        {
                            currentRootsQueue.Add(candidateVertex);
                            nextRootsQueue = new List<TVertex>();
                            broken = true;
                        }

                        continue;
                    }

                    candidateIndex++;
                }

                if (broken)
                {
                    continue;
                }

                var currentCycleVertex = _vertices.First(
                    v => predecessorCounts.TryGetValue(v, out var predecessorCount) && predecessorCount != 0);
                var cycle = new List<TVertex> { currentCycleVertex };
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in GetIncomingNeighbors(currentCycleVertex))
                    {
                        if (!predecessorCounts.TryGetValue(predecessor, out var predecessorCount)
                            || predecessorCount == 0)
                        {
                            continue;
                        }

                        predecessorCounts[currentCycleVertex] = -1;

                        currentCycleVertex = predecessor;
                        cycle.Add(currentCycleVertex);
                        finished = predecessorCounts[predecessor] == -1;
                        break;
                    }
                }

                cycle.Reverse();

                // Remove any tail that's not part of the cycle
                var startingVertex = cycle[0];
                for (var i = cycle.Count - 1; i >= 0; i--)
                {
                    if (cycle[i].Equals(startingVertex))
                    {
                        break;
                    }

                    cycle.RemoveAt(i);
                }

                ThrowCycle(cycle, formatCycle);
            }
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

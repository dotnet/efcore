// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Utilities;

internal class Multigraph<TVertex, TEdge> : Graph<TVertex>
    where TVertex : notnull
{
    private readonly IComparer<TVertex>? _secondarySortComparer;
    private readonly HashSet<TVertex> _vertices = [];
    private readonly Dictionary<TVertex, Dictionary<TVertex, object?>> _successorMap = new();
    private readonly Dictionary<TVertex, Dictionary<TVertex, object?>> _predecessorMap = new();

    public Multigraph()
    {
    }

    public Multigraph(IComparer<TVertex> secondarySortComparer)
    {
        _secondarySortComparer = secondarySortComparer;
    }

    public Multigraph(Comparison<TVertex> secondarySortComparer)
        : this(Comparer<TVertex>.Create(secondarySortComparer))
    {
    }

    public IEnumerable<TEdge> GetEdges(TVertex from, TVertex to)
    {
        if (_successorMap.TryGetValue(from, out var successorSet))
        {
            if (successorSet.TryGetValue(to, out var edges))
            {
                return edges is IEnumerable<Edge> edgeList ? edgeList.Select(e => e.Payload) : (new[] { ((Edge)edges!).Payload });
            }
        }

        return Enumerable.Empty<TEdge>();
    }

    public void AddVertex(TVertex vertex)
        => _vertices.Add(vertex);

    public void AddVertices(IEnumerable<TVertex> vertices)
        => _vertices.UnionWith(vertices);

    public void AddEdge(TVertex from, TVertex to, TEdge payload, bool requiresBatchingBoundary = false)
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

        var edge = new Edge(payload, requiresBatchingBoundary);

        if (!_successorMap.TryGetValue(from, out var successorEdges))
        {
            successorEdges = new Dictionary<TVertex, object?>();
            _successorMap.Add(from, successorEdges);
        }

        if (successorEdges.TryGetValue(to, out var edges))
        {
            if (edges is not List<Edge> edgeList)
            {
                edgeList = [(Edge)edges!];
                successorEdges[to] = edgeList;
            }

            edgeList.Add(edge);
        }
        else
        {
            successorEdges.Add(to, edge);
        }

        if (!_predecessorMap.TryGetValue(to, out var predecessorEdges))
        {
            predecessorEdges = new Dictionary<TVertex, object?>();
            _predecessorMap.Add(to, predecessorEdges);
        }

        if (predecessorEdges.TryGetValue(from, out edges))
        {
            if (edges is not List<Edge> edgeList)
            {
                edgeList = [(Edge)edges!];
                predecessorEdges[from] = edgeList;
            }

            edgeList.Add(edge);
        }
        else
        {
            predecessorEdges.Add(from, edge);
        }
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
        var batches = TopologicalSortCore(withBatching: false, tryBreakEdge, formatCycle, formatException);

        Check.DebugAssert(batches.Count < 2, "TopologicalSortCore did batching but withBatching was false");

        return batches.Count == 1
            ? batches[0]
            : [];
    }

    protected virtual string? ToString(TVertex vertex)
        => vertex.ToString();

    public IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
        => BatchingTopologicalSort(null, null);

    public IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
        Func<TVertex, TVertex, IEnumerable<TEdge>, bool>? canBreakEdges,
        Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle,
        Func<string, string>? formatException = null)
        => TopologicalSortCore(withBatching: true, canBreakEdges, formatCycle, formatException);

    private IReadOnlyList<List<TVertex>> TopologicalSortCore(
        bool withBatching,
        Func<TVertex, TVertex, IEnumerable<TEdge>, bool>? canBreakEdges,
        Func<IReadOnlyList<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string>? formatCycle,
        Func<string, string>? formatException = null)
    {
        // Performs a breadth-first topological sort (Kahn's algorithm)
        var result = new List<List<TVertex>>();
        var currentRootsQueue = new List<TVertex>();
        var nextRootsQueue = new List<TVertex>();
        var vertexesProcessed = 0;
        var batchBoundaryRequired = false;
        var currentBatch = new List<TVertex>();
        var currentBatchSet = new HashSet<TVertex>();

        var predecessorCounts = new Dictionary<TVertex, int>(_predecessorMap.Count);
        foreach (var (vertex, vertices) in _predecessorMap)
        {
            predecessorCounts[vertex] = vertices.Count;
        }

        // Bootstrap the topological sort by finding all vertexes which have no predecessors
        foreach (var vertex in _vertices)
        {
            if (!predecessorCounts.ContainsKey(vertex))
            {
                currentRootsQueue.Add(vertex);
            }
        }

        result.Add(currentBatch);

        while (vertexesProcessed < _vertices.Count)
        {
            while (currentRootsQueue.Count > 0)
            {
                // Secondary sorting: after the first topological sorting (according to dependencies between the commands as expressed in
                // the graph), we apply an optional secondary sort.
                // When sorting modification commands, this ensures a deterministic ordering and prevents deadlocks between concurrent
                // transactions locking the same rows in different orders.
                if (_secondarySortComparer is not null)
                {
                    currentRootsQueue.Sort(_secondarySortComparer);
                }

                // If we detected in the last roots pass that a batch boundary is required, close the current batch and start a new one.
                if (batchBoundaryRequired)
                {
                    currentBatch = [];
                    result.Add(currentBatch);
                    currentBatchSet.Clear();

                    batchBoundaryRequired = false;
                }

                foreach (var currentRoot in currentRootsQueue)
                {
                    currentBatch.Add(currentRoot);
                    currentBatchSet.Add(currentRoot);
                    vertexesProcessed++;

                    foreach (var successor in GetOutgoingNeighbors(currentRoot))
                    {
                        predecessorCounts[successor]--;

                        // If the successor has no other predecessors, add it for processing in the next roots pass.
                        if (predecessorCounts[successor] == 0)
                        {
                            nextRootsQueue.Add(successor);
                            CheckBatchingBoundary(successor);
                        }
                    }
                }

                // Finished passing over the current roots, move on to the next set.
                (currentRootsQueue, nextRootsQueue) = (nextRootsQueue, currentRootsQueue);
                nextRootsQueue.Clear();
            }

            // We have no more roots to process. That either means we're done, or that there's a cycle which we need to break
            if (vertexesProcessed < _vertices.Count)
            {
                var broken = false;

                var candidateVertices = predecessorCounts.Keys.ToList();
                var candidateIndex = 0;

                while ((candidateIndex < candidateVertices.Count)
                       && !broken
                       && canBreakEdges != null)
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

                    if (canBreakEdges(incomingNeighbor, candidateVertex, GetEdges(incomingNeighbor, candidateVertex)))
                    {
                        var removed = _successorMap[incomingNeighbor].Remove(candidateVertex);
                        Check.DebugAssert(removed, "Candidate vertex not found in successor map");
                        removed = _predecessorMap[candidateVertex].Remove(incomingNeighbor);
                        Check.DebugAssert(removed, "Incoming neighbor not found in predecessor map");

                        predecessorCounts[candidateVertex]--;
                        if (predecessorCounts[candidateVertex] == 0)
                        {
                            currentRootsQueue.Add(candidateVertex);
                            CheckBatchingBoundary(candidateVertex);
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

        return result;

        // Detect batch boundary (if batching is enabled).
        // If the successor has any predecessor where the edge requires a batching boundary, and that predecessor is
        // already in the current batch, then the next batch will have to be executed in a separate batch.
        // TODO: Optimization: Instead of currentBatchSet, store a batch counter on each vertex, and check if later
        // vertexes have a boundary-requiring dependency on a vertex with the same batch counter.
        void CheckBatchingBoundary(TVertex vertex)
        {
            if (withBatching
                && _predecessorMap[vertex].Any(
                    kv =>
                        (kv.Value is Edge { RequiresBatchingBoundary: true }
                            || kv.Value is IEnumerable<Edge> edges && edges.Any(e => e.RequiresBatchingBoundary))
                        && currentBatchSet.Contains(kv.Key)))
            {
                batchBoundaryRequired = true;
            }
        }
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

    public override IEnumerable<TVertex> Vertices
        => _vertices;

    public override IEnumerable<TVertex> GetOutgoingNeighbors(TVertex from)
        => _successorMap.TryGetValue(from, out var successorSet)
            ? successorSet.Keys
            : Enumerable.Empty<TVertex>();

    public override IEnumerable<TVertex> GetIncomingNeighbors(TVertex to)
        => _predecessorMap.TryGetValue(to, out var predecessors)
            ? predecessors.Keys
            : Enumerable.Empty<TVertex>();

    private record struct Edge(TEdge Payload, bool RequiresBatchingBoundary);
}

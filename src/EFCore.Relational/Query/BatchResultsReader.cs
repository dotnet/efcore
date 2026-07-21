// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides sequential access to the result sets produced by a query batch created with
///     <see cref="O:RelationalBatchQueryExtensions.Batch" /> or <see cref="O:RelationalBatchQueryExtensions.BatchAsync" />.
/// </summary>
/// <remarks>
///     <para>
///         All queries in the batch were sent to the database in a single command; the result sets must be consumed
///         in the order in which the queries were supplied to the batch. Requesting a result set invalidates any
///         lazily-consumed enumerable returned for an earlier result set.
///     </para>
///     <para>
///         The reader keeps the database connection busy until it is disposed; dispose it as soon as all required
///         results have been consumed.
///     </para>
/// </remarks>
public class BatchResultsReader : IDisposable, IAsyncDisposable
{
    private readonly IReadOnlyList<IRelationalBatchQueryingEnumerable> _queries;
    private readonly RelationalDataReader _dataReader;
    private readonly CancellationToken _cancellationToken;

    private int _nextResultSet;
    private int _readerResultSet;
    private bool _disposed;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public BatchResultsReader(
        IReadOnlyList<IRelationalBatchQueryingEnumerable> queries,
        RelationalDataReader dataReader,
        CancellationToken cancellationToken = default)
    {
        _queries = queries;
        _dataReader = dataReader;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    ///     Materializes the next result set of the batch into a <see cref="List{T}" />.
    /// </summary>
    /// <typeparam name="T">The element type of the query that produced the next result set.</typeparam>
    /// <returns>The materialized results.</returns>
    /// <exception cref="InvalidOperationException">All result sets have already been consumed.</exception>
    public virtual List<T> NextAsList<T>()
    {
        var list = new List<T>();
        using var enumerator = NextEnumerator<T>();
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.Current);
        }

        return list;
    }

    /// <summary>
    ///     Asynchronously materializes the next result set of the batch into a <see cref="List{T}" />.
    /// </summary>
    /// <typeparam name="T">The element type of the query that produced the next result set.</typeparam>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task containing the materialized results.</returns>
    /// <exception cref="InvalidOperationException">All result sets have already been consumed.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<List<T>> NextAsListAsync<T>(CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        var enumerator = await NextEnumeratorAsync<T>(cancellationToken).ConfigureAwait(false);
        await using var _ = enumerator.ConfigureAwait(false);
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            list.Add(enumerator.Current);
        }

        return list;
    }

    /// <summary>
    ///     Returns a lazily-evaluated enumerable that streams the rows of the next result set of the batch.
    /// </summary>
    /// <remarks>
    ///     The returned enumerable can only be enumerated once, and is invalidated as soon as a later result set
    ///     is requested from this reader.
    /// </remarks>
    /// <typeparam name="T">The element type of the query that produced the next result set.</typeparam>
    /// <returns>An enumerable streaming the results.</returns>
    /// <exception cref="InvalidOperationException">All result sets have already been consumed.</exception>
    public virtual IEnumerable<T> NextAsEnumerable<T>()
        => new SingleUseEnumerable<T>(NextEnumerator<T>());

    /// <summary>
    ///     Returns a lazily-evaluated async enumerable that streams the rows of the next result set of the batch.
    /// </summary>
    /// <remarks>
    ///     The returned enumerable can only be enumerated once, and is invalidated as soon as a later result set
    ///     is requested from this reader.
    /// </remarks>
    /// <typeparam name="T">The element type of the query that produced the next result set.</typeparam>
    /// <returns>An async enumerable streaming the results.</returns>
    /// <exception cref="InvalidOperationException">All result sets have already been consumed.</exception>
    public virtual IAsyncEnumerable<T> NextAsAsyncEnumerable<T>()
    {
        CheckDisposed();

        var index = ReserveNextResultSet();
        var query = _queries[index];

        return new SingleUseAsyncEnumerable<T>(this, query, index);
    }

    private IEnumerator<T> NextEnumerator<T>()
    {
        CheckDisposed();

        var index = ReserveNextResultSet();
        var enumerator = _queries[index].MaterializeResultSet<T>(_dataReader);

        Position(index);

        return enumerator;
    }

    private async Task<IAsyncEnumerator<T>> NextEnumeratorAsync<T>(CancellationToken cancellationToken)
    {
        CheckDisposed();

        var index = ReserveNextResultSet();
        var enumerator = _queries[index].MaterializeResultSetAsync<T>(
            _dataReader,
            cancellationToken == default ? _cancellationToken : cancellationToken);

        await PositionAsync(index, cancellationToken).ConfigureAwait(false);

        return enumerator;
    }

    private int ReserveNextResultSet()
        => _nextResultSet < _queries.Count
            ? _nextResultSet++
            : throw new InvalidOperationException(RelationalStrings.BatchQueryNoMoreResultSets(_queries.Count));

    private void Position(int index)
    {
        if (_readerResultSet > index)
        {
            throw new InvalidOperationException(RelationalStrings.BatchQueryResultSetInvalidated);
        }

        while (_readerResultSet < index)
        {
            if (!_dataReader.DbDataReader.NextResult())
            {
                throw new InvalidOperationException(RelationalStrings.BatchQueryNoMoreResultSets(_queries.Count));
            }

            _readerResultSet++;
        }
    }

    private async Task PositionAsync(int index, CancellationToken cancellationToken)
    {
        if (_readerResultSet > index)
        {
            throw new InvalidOperationException(RelationalStrings.BatchQueryResultSetInvalidated);
        }

        while (_readerResultSet < index)
        {
            if (!await _dataReader.DbDataReader.NextResultAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException(RelationalStrings.BatchQueryNoMoreResultSets(_queries.Count));
            }

            _readerResultSet++;
        }
    }

    private void CheckDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>
    ///     Releases the underlying data reader and command, allowing the connection to be released.
    /// </summary>
    public virtual void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _dataReader.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Asynchronously releases the underlying data reader and command, allowing the connection to be released.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public virtual async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            await _dataReader.DisposeAsync().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }

    private sealed class SingleUseEnumerable<T>(IEnumerator<T> enumerator) : IEnumerable<T>
    {
        private IEnumerator<T>? _enumerator = enumerator;

        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = _enumerator
                ?? throw new InvalidOperationException(CoreStrings.EnumerableResetNotSupported);
            _enumerator = null;

            return enumerator;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class SingleUseAsyncEnumerable<T>(
        BatchResultsReader reader,
        IRelationalBatchQueryingEnumerable query,
        int index) : IAsyncEnumerable<T>
    {
        private bool _used;

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (_used)
            {
                throw new InvalidOperationException(CoreStrings.EnumerableResetNotSupported);
            }

            _used = true;

            reader.CheckDisposed();

            var effectiveCancellationToken = cancellationToken == default ? reader._cancellationToken : cancellationToken;
            var enumerator = query.MaterializeResultSetAsync<T>(reader._dataReader, effectiveCancellationToken);
            await using var _ = enumerator.ConfigureAwait(false);

            await reader.PositionAsync(index, effectiveCancellationToken).ConfigureAwait(false);

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                yield return enumerator.Current;
            }
        }
    }
}

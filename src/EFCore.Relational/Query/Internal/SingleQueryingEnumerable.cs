// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class SingleQueryingEnumerable
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SingleQueryingEnumerable<T> Create<T>(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandResolver relationalCommandResolver,
        IReadOnlyList<ReaderColumn?>? readerColumns,
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> shaper,
        Type contextType,
        bool standAloneStateManager,
        bool detailedErrorsEnabled,
        bool threadSafetyChecksEnabled)
        => new(
            relationalQueryContext,
            relationalCommandResolver,
            readerColumns,
            shaper,
            contextType,
            standAloneStateManager,
            detailedErrorsEnabled,
            threadSafetyChecksEnabled);
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SingleQueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IRelationalBatchQueryingEnumerable
{
    private readonly RelationalQueryContext _relationalQueryContext;
    private readonly RelationalCommandResolver _relationalCommandResolver;
    private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
    private readonly Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> _shaper;
    private readonly Type _contextType;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
    private readonly bool _standAloneStateManager;
    private readonly bool _detailedErrorsEnabled;
    private readonly bool _threadSafetyChecksEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SingleQueryingEnumerable(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandResolver relationalCommandResolver,
        IReadOnlyList<ReaderColumn?>? readerColumns,
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> shaper,
        Type contextType,
        bool standAloneStateManager,
        bool detailedErrorsEnabled,
        bool threadSafetyChecksEnabled)
    {
        _relationalQueryContext = relationalQueryContext;
        _relationalCommandResolver = relationalCommandResolver;
        _readerColumns = readerColumns;
        _shaper = shaper;
        _contextType = contextType;
        _queryLogger = relationalQueryContext.QueryLogger;
        _standAloneStateManager = standAloneStateManager;
        _detailedErrorsEnabled = detailedErrorsEnabled;
        _threadSafetyChecksEnabled = threadSafetyChecksEnabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (cancellationToken != default)
        {
            _relationalQueryContext.CancellationToken = cancellationToken;
        }

        return new AsyncEnumerator(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerator<T> GetEnumerator()
        => new Enumerator(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbCommand CreateDbCommand()
        => _relationalCommandResolver(_relationalQueryContext.Parameters)
            .CreateDbCommand(
                new RelationalCommandParameterObject(
                    _relationalQueryContext.Connection,
                    _relationalQueryContext.Parameters,
                    null,
                    _relationalQueryContext.Context,
                    null, CommandSource.LinqQuery),
                Guid.Empty,
                (DbCommandMethod)(-1));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string ToQueryString()
    {
        using var dbCommand = CreateDbCommand();
        return _relationalQueryContext.RelationalQueryStringFactory.Create(dbCommand);
    }

    RelationalQueryContext IRelationalBatchQueryingEnumerable.QueryContext
        => _relationalQueryContext;

    IReadOnlyList<ReaderColumn?>? IRelationalBatchQueryingEnumerable.ReaderColumns
        => _readerColumns;

    bool IRelationalBatchQueryingEnumerable.ThreadSafetyChecksEnabled
        => _threadSafetyChecksEnabled;

    bool IRelationalBatchQueryingEnumerable.DetailedErrorsEnabled
        => _detailedErrorsEnabled;

    IRelationalCommandTemplate IRelationalBatchQueryingEnumerable.ResolveCommandTemplate()
        => _relationalCommandResolver(_relationalQueryContext.Parameters);

    IEnumerator<TResult> IRelationalBatchQueryingEnumerable.MaterializeResultSet<TResult>(RelationalDataReader dataReader)
        => new BatchResultSetEnumerator(this, dataReader) as IEnumerator<TResult>
            ?? throw new InvalidOperationException(
                RelationalStrings.BatchQueryResultTypeMismatch(
                    typeof(T).ShortDisplayName(), typeof(TResult).ShortDisplayName()));

    IAsyncEnumerator<TResult> IRelationalBatchQueryingEnumerable.MaterializeResultSetAsync<TResult>(
        RelationalDataReader dataReader,
        CancellationToken cancellationToken)
        => new BatchResultSetEnumerator(this, dataReader, cancellationToken) as IAsyncEnumerator<TResult>
            ?? throw new InvalidOperationException(
                RelationalStrings.BatchQueryResultTypeMismatch(
                    typeof(T).ShortDisplayName(), typeof(TResult).ShortDisplayName()));

    private static bool MoveNextCore(
        RelationalQueryContext relationalQueryContext,
        RelationalDataReader dataReader,
        DbDataReader dbDataReader,
        SingleQueryResultCoordinator resultCoordinator,
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> shaper,
        out T current)
    {
        var hasNext = resultCoordinator.HasNext ?? dataReader.Read();

        if (hasNext)
        {
            while (true)
            {
                resultCoordinator.ResultReady = true;
                resultCoordinator.HasNext = null;
                current = shaper(relationalQueryContext, dbDataReader, resultCoordinator.ResultContext, resultCoordinator);
                if (resultCoordinator.ResultReady)
                {
                    // We generated a result so null out previously stored values
                    resultCoordinator.ResultContext.Values = null;
                    break;
                }

                // If we are already pointing to next row, we don't need to call Read
                if (resultCoordinator.HasNext == true)
                {
                    continue;
                }

                if (!dataReader.Read())
                {
                    resultCoordinator.HasNext = false;
                    // Enumeration has ended, materialize last element
                    resultCoordinator.ResultReady = true;
                    current = shaper(relationalQueryContext, dbDataReader, resultCoordinator.ResultContext, resultCoordinator);

                    break;
                }
            }
        }
        else
        {
            current = default!;
        }

        return hasNext;
    }

    private static async ValueTask<(bool HasNext, T Current)> MoveNextCoreAsync(
        RelationalQueryContext relationalQueryContext,
        RelationalDataReader dataReader,
        DbDataReader dbDataReader,
        SingleQueryResultCoordinator resultCoordinator,
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> shaper,
        CancellationToken cancellationToken)
    {
        T current;
        var hasNext = resultCoordinator.HasNext
            ?? await dataReader.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (hasNext)
        {
            while (true)
            {
                resultCoordinator.ResultReady = true;
                resultCoordinator.HasNext = null;
                current = shaper(relationalQueryContext, dbDataReader, resultCoordinator.ResultContext, resultCoordinator);
                if (resultCoordinator.ResultReady)
                {
                    // We generated a result so null out previously stored values
                    resultCoordinator.ResultContext.Values = null;
                    break;
                }

                // If we are already pointing to next row, we don't need to call Read
                if (resultCoordinator.HasNext == true)
                {
                    continue;
                }

                if (!await dataReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    resultCoordinator.HasNext = false;
                    // Enumeration has ended, materialize last element
                    resultCoordinator.ResultReady = true;
                    current = shaper(relationalQueryContext, dbDataReader, resultCoordinator.ResultContext, resultCoordinator);

                    break;
                }
            }
        }
        else
        {
            current = default!;
        }

        return (hasNext, current);
    }

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly RelationalCommandResolver _relationalCommandResolver;
        private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
        private readonly Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _detailedErrorsEnabled;
        private readonly IConcurrencyDetector? _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;

        private IRelationalCommand? _relationalCommand;
        private RelationalDataReader? _dataReader;
        private DbDataReader? _dbDataReader;
        private SingleQueryResultCoordinator? _resultCoordinator;

        public Enumerator(SingleQueryingEnumerable<T> queryingEnumerable)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _relationalCommandResolver = queryingEnumerable._relationalCommandResolver;
            _readerColumns = queryingEnumerable._readerColumns;
            _shaper = queryingEnumerable._shaper;
            _contextType = queryingEnumerable._contextType;
            _queryLogger = queryingEnumerable._queryLogger;
            _standAloneStateManager = queryingEnumerable._standAloneStateManager;
            _detailedErrorsEnabled = queryingEnumerable._detailedErrorsEnabled;
            _exceptionDetector = _relationalQueryContext.ExceptionDetector;
            Current = default!;

            _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                ? _relationalQueryContext.ConcurrencyDetector
                : null;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
            => Current!;

        public bool MoveNext()
        {
            try
            {
                using var _ = _concurrencyDetector?.EnterCriticalSection();

                if (_dataReader == null)
                {
                    _relationalQueryContext.ExecutionStrategy.Execute(
                        this, static (_, enumerator) => InitializeReader(enumerator), null);
                }

                var hasNext = MoveNextCore(
                    _relationalQueryContext, _dataReader!, _dbDataReader!, _resultCoordinator!, _shaper, out var current);
                Current = current;

                return hasNext;
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception))
                {
                    _queryLogger.QueryCanceled(_contextType);
                }
                else
                {
                    _queryLogger.QueryIterationFailed(_contextType, exception);
                }

                throw;
            }
        }

        private static bool InitializeReader(Enumerator enumerator)
        {
            EntityFrameworkMetricsData.ReportQueryExecuting();

            var relationalCommand = enumerator._relationalCommand =
                enumerator._relationalCommandResolver.RentAndPopulateRelationalCommand(enumerator._relationalQueryContext);

            var dataReader = enumerator._dataReader = relationalCommand.ExecuteReader(
                new RelationalCommandParameterObject(
                    enumerator._relationalQueryContext.Connection,
                    enumerator._relationalQueryContext.Parameters,
                    enumerator._readerColumns,
                    enumerator._relationalQueryContext.Context,
                    enumerator._relationalQueryContext.CommandLogger,
                    enumerator._detailedErrorsEnabled,
                    CommandSource.LinqQuery));
            enumerator._dbDataReader = dataReader.DbDataReader;

            enumerator._resultCoordinator = new SingleQueryResultCoordinator();

            enumerator._relationalQueryContext.InitializeStateManager(enumerator._standAloneStateManager);

            return false;
        }

        public void Dispose()
        {
            if (_dataReader is not null)
            {
                _relationalQueryContext.Connection.ReturnCommand(_relationalCommand!);
                _dataReader.Dispose();
                _dataReader = null;
                _dbDataReader = null;
            }
        }

        public void Reset()
            => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
    }

    private sealed class AsyncEnumerator : IAsyncEnumerator<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly RelationalCommandResolver _relationalCommandResolver;
        private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
        private readonly Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _detailedErrorsEnabled;
        private readonly IConcurrencyDetector? _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;
        private readonly CancellationToken _cancellationToken;

        private IRelationalCommand? _relationalCommand;
        private RelationalDataReader? _dataReader;
        private DbDataReader? _dbDataReader;
        private SingleQueryResultCoordinator? _resultCoordinator;

        public AsyncEnumerator(SingleQueryingEnumerable<T> queryingEnumerable)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _relationalCommandResolver = queryingEnumerable._relationalCommandResolver;
            _readerColumns = queryingEnumerable._readerColumns;
            _shaper = queryingEnumerable._shaper;
            _contextType = queryingEnumerable._contextType;
            _queryLogger = queryingEnumerable._queryLogger;
            _standAloneStateManager = queryingEnumerable._standAloneStateManager;
            _detailedErrorsEnabled = queryingEnumerable._detailedErrorsEnabled;
            _exceptionDetector = _relationalQueryContext.ExceptionDetector;
            _cancellationToken = _relationalQueryContext.CancellationToken;
            Current = default!;

            _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                ? _relationalQueryContext.ConcurrencyDetector
                : null;
        }

        public T Current { get; private set; }

        public async ValueTask<bool> MoveNextAsync()
        {
            try
            {
                using var _ = _concurrencyDetector?.EnterCriticalSection();

                if (_dataReader == null)
                {
                    await _relationalQueryContext.ExecutionStrategy.ExecuteAsync(
                            this,
                            static (_, enumerator, cancellationToken) => InitializeReaderAsync(enumerator, cancellationToken),
                            null,
                            _cancellationToken)
                        .ConfigureAwait(false);
                }

                var (hasNext, current) = await MoveNextCoreAsync(
                        _relationalQueryContext, _dataReader!, _dbDataReader!, _resultCoordinator!, _shaper, _cancellationToken)
                    .ConfigureAwait(false);
                Current = current;

                return hasNext;
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception, _cancellationToken))
                {
                    _queryLogger.QueryCanceled(_contextType);
                }
                else
                {
                    _queryLogger.QueryIterationFailed(_contextType, exception);
                }

                throw;
            }
        }

        private static async Task<bool> InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
        {
            EntityFrameworkMetricsData.ReportQueryExecuting();

            var relationalCommand = enumerator._relationalCommand =
                enumerator._relationalCommandResolver.RentAndPopulateRelationalCommand(enumerator._relationalQueryContext);

            var dataReader = enumerator._dataReader = await relationalCommand.ExecuteReaderAsync(
                    new RelationalCommandParameterObject(
                        enumerator._relationalQueryContext.Connection,
                        enumerator._relationalQueryContext.Parameters,
                        enumerator._readerColumns,
                        enumerator._relationalQueryContext.Context,
                        enumerator._relationalQueryContext.CommandLogger,
                        enumerator._detailedErrorsEnabled, CommandSource.LinqQuery),
                    cancellationToken)
                .ConfigureAwait(false);
            enumerator._dbDataReader = dataReader.DbDataReader;

            enumerator._resultCoordinator = new SingleQueryResultCoordinator();

            enumerator._relationalQueryContext.InitializeStateManager(enumerator._standAloneStateManager);

            return false;
        }

        public ValueTask DisposeAsync()
        {
            if (_dataReader is not null)
            {
                _relationalQueryContext.Connection.ReturnCommand(_relationalCommand!);

                var dataReader = _dataReader;
                _dataReader = null;
                _dbDataReader = null;

                return dataReader.DisposeAsync();
            }

            return default;
        }
    }

    private sealed class BatchResultSetEnumerator : IEnumerator<T>, IAsyncEnumerator<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly IConcurrencyDetector? _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;
        private readonly RelationalDataReader _dataReader;
        private readonly DbDataReader _dbDataReader;
        private readonly CancellationToken _cancellationToken;

        private SingleQueryResultCoordinator? _resultCoordinator;

        public BatchResultSetEnumerator(
            SingleQueryingEnumerable<T> queryingEnumerable,
            RelationalDataReader dataReader,
            CancellationToken cancellationToken = default)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _shaper = queryingEnumerable._shaper;
            _contextType = queryingEnumerable._contextType;
            _queryLogger = queryingEnumerable._queryLogger;
            _standAloneStateManager = queryingEnumerable._standAloneStateManager;
            _exceptionDetector = _relationalQueryContext.ExceptionDetector;
            _dataReader = dataReader;
            _dbDataReader = dataReader.DbDataReader;
            _cancellationToken = cancellationToken;
            Current = default!;

            _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                ? _relationalQueryContext.ConcurrencyDetector
                : null;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
            => Current!;

        public bool MoveNext()
        {
            try
            {
                using var _ = _concurrencyDetector?.EnterCriticalSection();

                if (_resultCoordinator == null)
                {
                    Initialize();
                }

                var hasNext = MoveNextCore(
                    _relationalQueryContext, _dataReader, _dbDataReader, _resultCoordinator!, _shaper, out var current);
                Current = current;

                return hasNext;
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception))
                {
                    _queryLogger.QueryCanceled(_contextType);
                }
                else
                {
                    _queryLogger.QueryIterationFailed(_contextType, exception);
                }

                throw;
            }
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            try
            {
                using var _ = _concurrencyDetector?.EnterCriticalSection();

                if (_resultCoordinator == null)
                {
                    Initialize();
                }

                var (hasNext, current) = await MoveNextCoreAsync(
                        _relationalQueryContext, _dataReader, _dbDataReader, _resultCoordinator!, _shaper, _cancellationToken)
                    .ConfigureAwait(false);
                Current = current;

                return hasNext;
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception, _cancellationToken))
                {
                    _queryLogger.QueryCanceled(_contextType);
                }
                else
                {
                    _queryLogger.QueryIterationFailed(_contextType, exception);
                }

                throw;
            }
        }

        private void Initialize()
        {
            EntityFrameworkMetricsData.ReportQueryExecuting();

            _resultCoordinator = new SingleQueryResultCoordinator();

            _relationalQueryContext.InitializeStateManager(_standAloneStateManager);
        }

        // The data reader is shared across all result sets of the batch and is owned by the BatchResultsReader.
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => default;

        public void Reset()
            => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
    }
}

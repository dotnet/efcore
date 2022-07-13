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
public class SplitQueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IRelationalQueryingEnumerable
{
    private readonly RelationalQueryContext _relationalQueryContext;
    private readonly RelationalCommandCache _relationalCommandCache;
    private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
    private readonly Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, T> _shaper;
    private readonly Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>? _relatedDataLoaders;
    private readonly Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>? _relatedDataLoadersAsync;
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
    public SplitQueryingEnumerable(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandCache relationalCommandCache,
        IReadOnlyList<ReaderColumn?>? readerColumns,
        Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, T> shaper,
        Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>? relatedDataLoaders,
        Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>? relatedDataLoadersAsync,
        Type contextType,
        bool standAloneStateManager,
        bool detailedErrorsEnabled,
        bool threadSafetyChecksEnabled)
    {
        _relationalQueryContext = relationalQueryContext;
        _relationalCommandCache = relationalCommandCache;
        _readerColumns = readerColumns;
        _shaper = shaper;
        _relatedDataLoaders = relatedDataLoaders;
        _relatedDataLoadersAsync = relatedDataLoadersAsync;
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
        _relationalQueryContext.CancellationToken = cancellationToken;

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
        => _relationalCommandCache
            .GetRelationalCommandTemplate(_relationalQueryContext.ParameterValues)
            .CreateDbCommand(
                new RelationalCommandParameterObject(
                    _relationalQueryContext.Connection,
                    _relationalQueryContext.ParameterValues,
                    null,
                    null,
                    null,
                    _detailedErrorsEnabled, CommandSource.LinqQuery),
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
        return
            $"{_relationalQueryContext.RelationalQueryStringFactory.Create(dbCommand)}{Environment.NewLine}{Environment.NewLine}{RelationalStrings.SplitQueryString}";
    }

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly RelationalCommandCache _relationalCommandCache;
        private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
        private readonly Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, T> _shaper;
        private readonly Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>? _relatedDataLoaders;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _detailedErrorsEnabled;
        private readonly IConcurrencyDetector? _concurrencyDetector;

        private IRelationalCommand? _relationalCommand;
        private RelationalDataReader? _dataReader;
        private DbDataReader? _dbDataReader;
        private SplitQueryResultCoordinator? _resultCoordinator;
        private readonly IExceptionDetector _exceptionDetector;

        public Enumerator(SplitQueryingEnumerable<T> queryingEnumerable)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _relationalCommandCache = queryingEnumerable._relationalCommandCache;
            _readerColumns = queryingEnumerable._readerColumns;
            _shaper = queryingEnumerable._shaper;
            _relatedDataLoaders = queryingEnumerable._relatedDataLoaders;
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
                _concurrencyDetector?.EnterCriticalSection();

                try
                {
                    if (_dataReader == null)
                    {
                        _relationalQueryContext.ExecutionStrategy.Execute(
                            this, static (_, enumerator) => InitializeReader(enumerator), null);
                    }

                    var hasNext = _dataReader!.Read();

                    if (hasNext)
                    {
                        _resultCoordinator!.ResultContext.Values = null;
                        Current = _shaper(
                            _relationalQueryContext, _dbDataReader!, _resultCoordinator.ResultContext, _resultCoordinator);
                        if (_relatedDataLoaders != null)
                        {
                            _relatedDataLoaders.Invoke(
                                _relationalQueryContext, _relationalQueryContext.ExecutionStrategy, _resultCoordinator);
                            Current = _shaper(
                                _relationalQueryContext, _dbDataReader!, _resultCoordinator.ResultContext, _resultCoordinator);
                        }
                    }
                    else
                    {
                        Current = default!;
                    }

                    return hasNext;
                }
                finally
                {
                    _concurrencyDetector?.ExitCriticalSection();
                }
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
            EntityFrameworkEventSource.Log.QueryExecuting();

            var relationalCommand = enumerator._relationalCommand =
                enumerator._relationalCommandCache.RentAndPopulateRelationalCommand(enumerator._relationalQueryContext);

            var dataReader = enumerator._dataReader = relationalCommand.ExecuteReader(
                new RelationalCommandParameterObject(
                    enumerator._relationalQueryContext.Connection,
                    enumerator._relationalQueryContext.ParameterValues,
                    enumerator._readerColumns,
                    enumerator._relationalQueryContext.Context,
                    enumerator._relationalQueryContext.CommandLogger,
                    enumerator._detailedErrorsEnabled,
                    CommandSource.LinqQuery));
            enumerator._dbDataReader = dataReader.DbDataReader;

            enumerator._resultCoordinator = new SplitQueryResultCoordinator();

            enumerator._relationalQueryContext.InitializeStateManager(enumerator._standAloneStateManager);

            return false;
        }

        public void Dispose()
        {
            if (_dataReader is not null)
            {
                _relationalQueryContext.Connection.ReturnCommand(_relationalCommand!);
                _dataReader.Dispose();
                if (_resultCoordinator != null)
                {
                    foreach (var dataReader in _resultCoordinator.DataReaders)
                    {
                        dataReader?.DataReader.Dispose();
                    }

                    _resultCoordinator.DataReaders.Clear();

                    _resultCoordinator = null;
                }

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
        private readonly RelationalCommandCache _relationalCommandCache;
        private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
        private readonly Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, T> _shaper;
        private readonly Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>? _relatedDataLoaders;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _detailedErrorEnabled;
        private readonly IConcurrencyDetector? _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;
        private readonly CancellationToken _cancellationToken;

        private IRelationalCommand? _relationalCommand;
        private RelationalDataReader? _dataReader;
        private DbDataReader? _dbDataReader;
        private SplitQueryResultCoordinator? _resultCoordinator;

        public AsyncEnumerator(SplitQueryingEnumerable<T> queryingEnumerable)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _relationalCommandCache = queryingEnumerable._relationalCommandCache;
            _readerColumns = queryingEnumerable._readerColumns;
            _shaper = queryingEnumerable._shaper;
            _relatedDataLoaders = queryingEnumerable._relatedDataLoadersAsync;
            _contextType = queryingEnumerable._contextType;
            _queryLogger = queryingEnumerable._queryLogger;
            _standAloneStateManager = queryingEnumerable._standAloneStateManager;
            _detailedErrorEnabled = queryingEnumerable._detailedErrorsEnabled;
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
                _concurrencyDetector?.EnterCriticalSection();

                try
                {
                    if (_dataReader == null)
                    {
                        await _relationalQueryContext.ExecutionStrategy.ExecuteAsync(
                                this,
                                static (_, enumerator, cancellationToken) => InitializeReaderAsync(enumerator, cancellationToken),
                                null,
                                _cancellationToken)
                            .ConfigureAwait(false);
                    }

                    var hasNext = await _dataReader!.ReadAsync(_cancellationToken).ConfigureAwait(false);

                    if (hasNext)
                    {
                        _resultCoordinator!.ResultContext.Values = null;
                        Current = _shaper(
                            _relationalQueryContext, _dbDataReader!, _resultCoordinator.ResultContext, _resultCoordinator);
                        if (_relatedDataLoaders != null)
                        {
                            await _relatedDataLoaders(
                                    _relationalQueryContext, _relationalQueryContext.ExecutionStrategy, _resultCoordinator)
                                .ConfigureAwait(false);
                            Current =
                                _shaper(_relationalQueryContext, _dbDataReader!, _resultCoordinator.ResultContext, _resultCoordinator);
                        }
                    }
                    else
                    {
                        Current = default!;
                    }

                    return hasNext;
                }
                finally
                {
                    _concurrencyDetector?.ExitCriticalSection();
                }
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
            EntityFrameworkEventSource.Log.QueryExecuting();

            var relationalCommand = enumerator._relationalCommand =
                enumerator._relationalCommandCache.RentAndPopulateRelationalCommand(enumerator._relationalQueryContext);

            var dataReader = enumerator._dataReader = await relationalCommand.ExecuteReaderAsync(
                    new RelationalCommandParameterObject(
                        enumerator._relationalQueryContext.Connection,
                        enumerator._relationalQueryContext.ParameterValues,
                        enumerator._readerColumns,
                        enumerator._relationalQueryContext.Context,
                        enumerator._relationalQueryContext.CommandLogger,
                        enumerator._detailedErrorEnabled, CommandSource.LinqQuery),
                    cancellationToken)
                .ConfigureAwait(false);
            enumerator._dbDataReader = dataReader.DbDataReader;

            enumerator._resultCoordinator = new SplitQueryResultCoordinator();

            enumerator._relationalQueryContext.InitializeStateManager(enumerator._standAloneStateManager);

            return false;
        }

        public async ValueTask DisposeAsync()
        {
            if (_dataReader != null)
            {
                _relationalQueryContext.Connection.ReturnCommand(_relationalCommand!);
                await _dataReader.DisposeAsync().ConfigureAwait(false);
                if (_resultCoordinator != null)
                {
                    foreach (var dataReader in _resultCoordinator.DataReaders)
                    {
                        if (dataReader != null)
                        {
                            await dataReader.DataReader.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    _resultCoordinator.DataReaders.Clear();
                    _resultCoordinator = null;
                }

                _dataReader = null;
                _dbDataReader = null;
            }
        }
    }
}

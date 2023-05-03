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
public class FromSqlQueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IRelationalQueryingEnumerable
{
    private readonly RelationalQueryContext _relationalQueryContext;
    private readonly RelationalCommandCache _relationalCommandCache;
    private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
    private readonly IReadOnlyList<string> _columnNames;
    private readonly Func<QueryContext, DbDataReader, int[], T> _shaper;
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
    public FromSqlQueryingEnumerable(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandCache relationalCommandCache,
        IReadOnlyList<ReaderColumn?>? readerColumns,
        IReadOnlyList<string> columnNames,
        Func<QueryContext, DbDataReader, int[], T> shaper,
        Type contextType,
        bool standAloneStateManager,
        bool detailedErrorsEnabled,
        bool threadSafetyChecksEnabled)
    {
        _relationalQueryContext = relationalQueryContext;
        _relationalCommandCache = relationalCommandCache;
        _readerColumns = readerColumns;
        _columnNames = columnNames;
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
                    _detailedErrorsEnabled, CommandSource.FromSqlQuery),
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static int[] BuildIndexMap(IReadOnlyList<string> columnNames, DbDataReader dataReader)
    {
        var readerColumns = Enumerable.Range(0, dataReader.FieldCount)
            .ToDictionary(dataReader.GetName, i => i, StringComparer.OrdinalIgnoreCase);

        var indexMap = new int[columnNames.Count];
        for (var i = 0; i < columnNames.Count; i++)
        {
            var columnName = columnNames[i];
            if (!readerColumns.TryGetValue(columnName, out var ordinal))
            {
                if (columnNames.Count != 1)
                {
                    throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(columnName));
                }

                ordinal = 0;
            }

            indexMap[i] = ordinal;
        }

        return indexMap;
    }

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly RelationalCommandCache _relationalCommandCache;
        private readonly IReadOnlyList<ReaderColumn?>? _readerColumns;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly Func<QueryContext, DbDataReader, int[], T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _detailedErrorsEnabled;
        private readonly IConcurrencyDetector? _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;

        private IRelationalCommand? _relationalCommand;
        private RelationalDataReader? _dataReader;
        private int[]? _indexMap;

        public Enumerator(FromSqlQueryingEnumerable<T> queryingEnumerable)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _relationalCommandCache = queryingEnumerable._relationalCommandCache;
            _readerColumns = queryingEnumerable._readerColumns;
            _columnNames = queryingEnumerable._columnNames;
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
                _concurrencyDetector?.EnterCriticalSection();

                try
                {
                    if (_dataReader == null)
                    {
                        _relationalQueryContext.ExecutionStrategy.Execute(this, (_, enumerator) => InitializeReader(enumerator), null);
                    }

                    var hasNext = _dataReader!.Read();

                    Current = hasNext
                        ? _shaper(_relationalQueryContext, _dataReader.DbDataReader, _indexMap!)
                        : default!;

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

            enumerator._dataReader = relationalCommand.ExecuteReader(
                new RelationalCommandParameterObject(
                    enumerator._relationalQueryContext.Connection,
                    enumerator._relationalQueryContext.ParameterValues,
                    enumerator._readerColumns,
                    enumerator._relationalQueryContext.Context,
                    enumerator._relationalQueryContext.CommandLogger,
                    enumerator._detailedErrorsEnabled, CommandSource.FromSqlQuery));

            enumerator._indexMap = BuildIndexMap(enumerator._columnNames, enumerator._dataReader.DbDataReader);

            enumerator._relationalQueryContext.InitializeStateManager(enumerator._standAloneStateManager);

            return false;
        }

        public void Dispose()
        {
            if (_dataReader is not null)
            {
                _relationalQueryContext.Connection.ReturnCommand(_relationalCommand!);
                _dataReader?.Dispose();
                _dataReader = null;
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
        private readonly IReadOnlyList<string> _columnNames;
        private readonly Func<QueryContext, DbDataReader, int[], T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _detailedErrorsEnabled;
        private readonly IConcurrencyDetector? _concurrencyDetector;
        private readonly IExceptionDetector _exceptionDetector;

        private IRelationalCommand? _relationalCommand;
        private RelationalDataReader? _dataReader;
        private int[]? _indexMap;

        public AsyncEnumerator(FromSqlQueryingEnumerable<T> queryingEnumerable)
        {
            _relationalQueryContext = queryingEnumerable._relationalQueryContext;
            _relationalCommandCache = queryingEnumerable._relationalCommandCache;
            _readerColumns = queryingEnumerable._readerColumns;
            _columnNames = queryingEnumerable._columnNames;
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
                                (_, enumerator, cancellationToken) => InitializeReaderAsync(enumerator, cancellationToken),
                                null,
                                _relationalQueryContext.CancellationToken)
                            .ConfigureAwait(false);
                    }

                    var hasNext = await _dataReader!.ReadAsync(_relationalQueryContext.CancellationToken).ConfigureAwait(false);

                    Current = hasNext
                        ? _shaper(_relationalQueryContext, _dataReader.DbDataReader, _indexMap!)
                        : default!;

                    return hasNext;
                }
                finally
                {
                    _concurrencyDetector?.ExitCriticalSection();
                }
            }
            catch (Exception exception)
            {
                if (_exceptionDetector.IsCancellation(exception, _relationalQueryContext.CancellationToken))
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

            enumerator._dataReader = await relationalCommand.ExecuteReaderAsync(
                    new RelationalCommandParameterObject(
                        enumerator._relationalQueryContext.Connection,
                        enumerator._relationalQueryContext.ParameterValues,
                        enumerator._readerColumns,
                        enumerator._relationalQueryContext.Context,
                        enumerator._relationalQueryContext.CommandLogger,
                        enumerator._detailedErrorsEnabled, CommandSource.FromSqlQuery),
                    cancellationToken)
                .ConfigureAwait(false);

            enumerator._indexMap = BuildIndexMap(enumerator._columnNames, enumerator._dataReader.DbDataReader);

            enumerator._relationalQueryContext.InitializeStateManager(enumerator._standAloneStateManager);

            return false;
        }

        public ValueTask DisposeAsync()
        {
            if (_dataReader != null)
            {
                _relationalQueryContext.Connection.ReturnCommand(_relationalCommand!);

                var dataReader = _dataReader;
                _dataReader = null;

                return dataReader.DisposeAsync();
            }

            return default;
        }
    }
}

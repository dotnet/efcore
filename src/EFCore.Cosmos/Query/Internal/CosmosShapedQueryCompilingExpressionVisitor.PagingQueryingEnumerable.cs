// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class PagingQueryingEnumerable<T> : IEnumerable<CosmosPage<T>>, IAsyncEnumerable<CosmosPage<T>>
    {
        private readonly CosmosQueryContext _cosmosQueryContext;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly SelectExpression _selectExpression;
        private readonly Func<CosmosQueryContext, JObject, T> _shaper;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly Type _contextType;
        private readonly string _cosmosContainer;
        private readonly PartitionKey _cosmosPartitionKeyValue;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _threadSafetyChecksEnabled;
        private readonly string _continuationTokenParameterName;
        private readonly string _maxItemCountParameterName;
        private readonly string _responseContinuationTokenLimitInKbParameterName;

        public PagingQueryingEnumerable(
            CosmosQueryContext cosmosQueryContext,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            SelectExpression selectExpression,
            Func<CosmosQueryContext, JObject, T> shaper,
            Type contextType,
            string cosmosContainer,
            PartitionKey partitionKeyValueFromExtension,
            bool standAloneStateManager,
            bool threadSafetyChecksEnabled,
            string continuationTokenParameterName,
            string maxItemCountParameterName,
            string responseContinuationTokenLimitInKbParameterName)
        {
            _cosmosQueryContext = cosmosQueryContext;
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _selectExpression = selectExpression;
            _shaper = shaper;
            _contextType = contextType;
            _queryLogger = cosmosQueryContext.QueryLogger;
            _commandLogger = cosmosQueryContext.CommandLogger;
            _standAloneStateManager = standAloneStateManager;
            _threadSafetyChecksEnabled = threadSafetyChecksEnabled;
            _continuationTokenParameterName = continuationTokenParameterName;
            _maxItemCountParameterName = maxItemCountParameterName;
            _responseContinuationTokenLimitInKbParameterName = responseContinuationTokenLimitInKbParameterName;

            var partitionKey = selectExpression.GetPartitionKeyValue(cosmosQueryContext.ParameterValues);
            if (partitionKey != PartitionKey.None
                && partitionKeyValueFromExtension != PartitionKey.None
                && !partitionKeyValueFromExtension.Equals(partitionKey))
            {
                throw new InvalidOperationException(CosmosStrings.PartitionKeyMismatch(partitionKeyValueFromExtension, partitionKey));
            }

            _cosmosPartitionKeyValue = partitionKey != PartitionKey.None ? partitionKey : partitionKeyValueFromExtension;
            _cosmosContainer = cosmosContainer;
        }

        public IEnumerator<CosmosPage<T>> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IAsyncEnumerator<CosmosPage<T>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this, cancellationToken);

        private CosmosSqlQuery GenerateQuery()
            => _querySqlGeneratorFactory.Create().GetSqlQuery(
                (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                        _sqlExpressionFactory,
                        _cosmosQueryContext.ParameterValues)
                    .Visit(_selectExpression),
                _cosmosQueryContext.ParameterValues);

        private sealed class Enumerator : IEnumerator<CosmosPage<T>>, IAsyncEnumerator<CosmosPage<T>>
        {
            private readonly PagingQueryingEnumerable<T> _queryingEnumerable;
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly Func<CosmosQueryContext, JObject, T> _shaper;
            private readonly Type _contextType;
            private readonly string _cosmosContainer;
            private readonly PartitionKey _cosmosPartitionKeyValue;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
            private readonly bool _standAloneStateManager;
            private readonly CancellationToken _cancellationToken;
            private readonly IConcurrencyDetector _concurrencyDetector;
            private readonly IExceptionDetector _exceptionDetector;

            private bool _hasExecuted;
            private bool _isDisposed;

            public Enumerator(PagingQueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken = default)
            {
                _queryingEnumerable = queryingEnumerable;
                _cosmosQueryContext = queryingEnumerable._cosmosQueryContext;
                _shaper = queryingEnumerable._shaper;
                _contextType = queryingEnumerable._contextType;
                _cosmosContainer = queryingEnumerable._cosmosContainer;
                _cosmosPartitionKeyValue = queryingEnumerable._cosmosPartitionKeyValue;
                _queryLogger = queryingEnumerable._queryLogger;
                _commandLogger = queryingEnumerable._commandLogger;
                _standAloneStateManager = queryingEnumerable._standAloneStateManager;
                _exceptionDetector = _cosmosQueryContext.ExceptionDetector;
                _cancellationToken = cancellationToken;

                _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                    ? _cosmosQueryContext.ConcurrencyDetector
                    : null;
            }

            public CosmosPage<T> Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
                => MoveNextCore().GetAwaiter().GetResult();

            public ValueTask<bool> MoveNextAsync()
                => new(MoveNextCore());

            private async Task<bool> MoveNextCore()
            {
                ObjectDisposedException.ThrowIf(_isDisposed, typeof(Enumerator));

                try
                {
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        if (_hasExecuted)
                        {
                            return false;
                        }

                        _hasExecuted = true;

                        var continuationToken =
                            (string)_cosmosQueryContext.ParameterValues[_queryingEnumerable._continuationTokenParameterName];
                        var maxItemCount = (int?)_cosmosQueryContext.ParameterValues[_queryingEnumerable._maxItemCountParameterName];
                        var responseContinuationTokenLimitInKb = (int?)
                            _cosmosQueryContext.ParameterValues[_queryingEnumerable._responseContinuationTokenLimitInKbParameterName];

                        var sqlQuery = _queryingEnumerable.GenerateQuery();

                        EntityFrameworkMetricsData.ReportQueryExecuting();

                        var queryRequestOptions = new QueryRequestOptions
                        {
                            ResponseContinuationTokenLimitInKb = responseContinuationTokenLimitInKb
                        };

                        if (_cosmosPartitionKeyValue != PartitionKey.None)
                        {
                            queryRequestOptions.PartitionKey = _cosmosPartitionKeyValue;
                        }

                        var cosmosClient = _cosmosQueryContext.CosmosClient;
                        _commandLogger.ExecutingSqlQuery(_cosmosContainer, _cosmosPartitionKeyValue, sqlQuery);
                        _cosmosQueryContext.InitializeStateManager(_standAloneStateManager);

                        var results = maxItemCount.HasValue ? new List<T>(maxItemCount.Value) : [];

                        while (maxItemCount is null or > 0)
                        {
                            queryRequestOptions.MaxItemCount = maxItemCount;
                            using var feedIterator = cosmosClient.CreateQuery(
                                _cosmosContainer, sqlQuery, continuationToken, queryRequestOptions);

                            using var responseMessage = await feedIterator.ReadNextAsync(_cancellationToken).ConfigureAwait(false);

                            _commandLogger.ExecutedReadNext(
                                responseMessage.Diagnostics.GetClientElapsedTime(),
                                responseMessage.Headers.RequestCharge,
                                responseMessage.Headers.ActivityId,
                                _cosmosContainer,
                                _cosmosPartitionKeyValue,
                                sqlQuery);

                            responseMessage.EnsureSuccessStatusCode();

                            var responseMessageEnumerable = cosmosClient.GetResponseMessageEnumerable(responseMessage);
                            foreach (var resultObject in responseMessageEnumerable)
                            {
                                results.Add(_shaper(_cosmosQueryContext, resultObject));
                                maxItemCount--;
                            }

                            continuationToken = responseMessage.ContinuationToken;

                            if (responseMessage.ContinuationToken is null)
                            {
                                break;
                            }
                        }

                        Current = new CosmosPage<T>(results, continuationToken);

                        _hasExecuted = true;
                        return true;
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

            public ValueTask DisposeAsync()
            {
                Dispose();

                return default;
            }

            public void Dispose()
                => _isDisposed = true;

            public void Reset()
                => throw new NotSupportedException();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
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
    private sealed class ReadItemQueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IQueryingEnumerable
    {
        private readonly CosmosQueryContext _cosmosQueryContext;
        private readonly IEntityType _rootEntityType;
        private readonly string _cosmosContainer;
        private readonly ReadItemInfo _readItemInfo;
        private readonly PartitionKey _cosmosPartitionKey;
        private readonly Func<CosmosQueryContext, JObject, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _threadSafetyChecksEnabled;

        public ReadItemQueryingEnumerable(
            CosmosQueryContext cosmosQueryContext,
            IEntityType rootEntityType,
            List<Expression> partitionKeyPropertyValues,
            ReadItemInfo readItemInfo,
            Func<CosmosQueryContext, JObject, T> shaper,
            Type contextType,
            bool standAloneStateManager,
            bool threadSafetyChecksEnabled)
        {
            _cosmosQueryContext = cosmosQueryContext;
            _rootEntityType = rootEntityType;
            _readItemInfo = readItemInfo;
            _shaper = shaper;
            _contextType = contextType;
            _queryLogger = _cosmosQueryContext.QueryLogger;
            _standAloneStateManager = standAloneStateManager;
            _threadSafetyChecksEnabled = threadSafetyChecksEnabled;

            _cosmosContainer = rootEntityType.GetContainer()
                ?? throw new UnreachableException("Root entity type without a Cosmos container.");
            _cosmosPartitionKey = GeneratePartitionKey(
                rootEntityType, partitionKeyPropertyValues, _cosmosQueryContext.ParameterValues);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this, cancellationToken);

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public string ToQueryString()
        {
            TryGetResourceId(out var resourceId);
            return CosmosStrings.NoReadItemQueryString(resourceId, _cosmosPartitionKey);
        }

        private bool TryGetResourceId(out string resourceId)
        {
            var jsonIdDefinition = _rootEntityType.GetJsonIdDefinition();
            Check.DebugAssert(
                jsonIdDefinition != null,
                "Should not be using this enumerable if not using ReadItem, which needs an id definition.");

            var values = new List<object>(jsonIdDefinition.Properties.Count);
            foreach (var property in jsonIdDefinition.Properties)
            {
                var value = _readItemInfo.PropertyValues[property] switch
                {
                    SqlParameterExpression { Name: var parameterName } => _cosmosQueryContext.ParameterValues[parameterName],
                    SqlConstantExpression { Value: var constantValue } => constantValue,
                    _ => throw new UnreachableException()
                };

                values.Add(value);
            }

            resourceId = jsonIdDefinition.GenerateIdString(values);
            if (string.IsNullOrEmpty(resourceId))
            {
                throw new InvalidOperationException(CosmosStrings.InvalidResourceId);
            }

            return true;
        }

        private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
        {
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly string _cosmosContainer;
            private readonly PartitionKey _cosmosPartitionKey;
            private readonly Func<CosmosQueryContext, JObject, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly IConcurrencyDetector _concurrencyDetector;
            private readonly IExceptionDetector _exceptionDetector;
            private readonly ReadItemQueryingEnumerable<T> _readItemEnumerable;
            private readonly CancellationToken _cancellationToken;

            private JObject _item;
            private bool _hasExecuted;

            public Enumerator(ReadItemQueryingEnumerable<T> readItemEnumerable, CancellationToken cancellationToken = default)
            {
                _cosmosQueryContext = readItemEnumerable._cosmosQueryContext;
                _cosmosContainer = readItemEnumerable._cosmosContainer;
                _cosmosPartitionKey = readItemEnumerable._cosmosPartitionKey;
                _shaper = readItemEnumerable._shaper;
                _contextType = readItemEnumerable._contextType;
                _queryLogger = readItemEnumerable._queryLogger;
                _standAloneStateManager = readItemEnumerable._standAloneStateManager;
                _exceptionDetector = _cosmosQueryContext.ExceptionDetector;
                _readItemEnumerable = readItemEnumerable;
                _cancellationToken = cancellationToken;

                _concurrencyDetector = readItemEnumerable._threadSafetyChecksEnabled
                    ? _cosmosQueryContext.ConcurrencyDetector
                    : null;
            }

            object IEnumerator.Current
                => Current;

            public T Current { get; private set; }

            public bool MoveNext()
            {
                try
                {
                    using var _ = _concurrencyDetector?.EnterCriticalSection();

                    if (_hasExecuted)
                    {
                        return false;
                    }

                    if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                    {
                        throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                    }

                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    _item = _cosmosQueryContext.CosmosClient.ExecuteReadItem(
                        _cosmosContainer,
                        _cosmosPartitionKey,
                        resourceId);

                    return ShapeResult();
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

                    if (_hasExecuted)
                    {
                        return false;
                    }

                    if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                    {
                        throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                    }

                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    _item = await _cosmosQueryContext.CosmosClient.ExecuteReadItemAsync(
                            _cosmosContainer,
                            _cosmosPartitionKey,
                            resourceId,
                            _cancellationToken)
                        .ConfigureAwait(false);

                    return ShapeResult();
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

            public void Dispose()
            {
                _item = null;
                _hasExecuted = false;
            }

            public ValueTask DisposeAsync()
            {
                Dispose();

                return default;
            }

            public void Reset()
                => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);

            private bool ShapeResult()
            {
                var hasNext = _item is not null;

                _cosmosQueryContext.InitializeStateManager(_standAloneStateManager);

                Current
                    = hasNext
                        ? _shaper(_cosmosQueryContext, _item)
                        : default;

                _hasExecuted = true;

                return hasNext;
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly string _cosmosContainer;
        private readonly ReadItemInfo _readItemInfo;
        private readonly Func<CosmosQueryContext, JObject, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _threadSafetyChecksEnabled;

        public ReadItemQueryingEnumerable(
            CosmosQueryContext cosmosQueryContext,
            string cosmosContainer,
            ReadItemInfo readItemInfo,
            Func<CosmosQueryContext, JObject, T> shaper,
            Type contextType,
            bool standAloneStateManager,
            bool threadSafetyChecksEnabled)
        {
            _cosmosQueryContext = cosmosQueryContext;
            _cosmosContainer = cosmosContainer;
            _readItemInfo = readItemInfo;
            _shaper = shaper;
            _contextType = contextType;
            _queryLogger = _cosmosQueryContext.QueryLogger;
            _standAloneStateManager = standAloneStateManager;
            _threadSafetyChecksEnabled = threadSafetyChecksEnabled;
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
            TryGetPartitionKey(out var partitionKey);
            return CosmosStrings.NoReadItemQueryString(resourceId, partitionKey);
        }

        private bool TryGetPartitionKey(out PartitionKey partitionKeyValue)
        {
            var properties = _readItemInfo.EntityType.GetPartitionKeyProperties();
            if (!properties.Any())
            {
                partitionKeyValue = PartitionKey.None;
                return true;
            }

            var builder = new PartitionKeyBuilder();
            foreach (var property in properties)
            {
                if (TryGetParameterValue(property, out var value))
                {
                    if (value == null)
                    {
                        partitionKeyValue = PartitionKey.Null;
                        return false;
                    }
                    builder.Add(value, property);
                }
            }

            partitionKeyValue = builder.Build();

            return true;
        }

        private bool TryGetResourceId(out string resourceId)
        {
            var entityType = _readItemInfo.EntityType;
            var jsonIdDefinition = entityType.GetJsonIdDefinition();
            Check.DebugAssert(jsonIdDefinition != null,
                "Should not be using this enumerable if not using ReadItem, which needs an id definition.");

            var values = new List<object>(jsonIdDefinition.Properties.Count);
            foreach (var property in jsonIdDefinition.Properties)
            {
                if (!TryGetParameterValue(property, out var value))
                {
                    var discriminatorProperty = entityType.FindDiscriminatorProperty();
                    if (discriminatorProperty == property)
                    {
                        value = entityType.GetDiscriminatorValue();
                    }
                    else
                    {
                        Check.DebugFail("Parameters should cover all properties or we should not be using ReadItem.");
                    }
                }

                values.Add(value);
            }

            resourceId = jsonIdDefinition.GenerateIdString(values);
            if (string.IsNullOrEmpty(resourceId))
            {
                throw new InvalidOperationException(CosmosStrings.InvalidResourceId);
            }

            return true;
        }

        private bool TryGetParameterValue(IProperty property, out object value)
        {
            value = null;
            return _readItemInfo.PropertyParameters.TryGetValue(property, out var parameterName)
                && _cosmosQueryContext.ParameterValues.TryGetValue(parameterName, out value);
        }

        private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
        {
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly string _cosmosContainer;
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
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        if (!_hasExecuted)
                        {
                            if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                            {
                                throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                            }

                            if (!_readItemEnumerable.TryGetPartitionKey(out var partitionKeyValue))
                            {
                                throw new InvalidOperationException(CosmosStrings.PartitionKeyMissing);
                            }

                            EntityFrameworkMetricsData.ReportQueryExecuting();

                            _item = _cosmosQueryContext.CosmosClient.ExecuteReadItem(
                                _cosmosContainer,
                                partitionKeyValue,
                                resourceId);

                            return ShapeResult();
                        }

                        return false;
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

            public async ValueTask<bool> MoveNextAsync()
            {
                try
                {
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        if (!_hasExecuted)
                        {
                            if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                            {
                                throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                            }

                            if (!_readItemEnumerable.TryGetPartitionKey(out var partitionKeyValue))
                            {
                                throw new InvalidOperationException(CosmosStrings.PartitionKeyMissing);
                            }

                            EntityFrameworkMetricsData.ReportQueryExecuting();

                            _item = await _cosmosQueryContext.CosmosClient.ExecuteReadItemAsync(
                                    _cosmosContainer,
                                    partitionKeyValue,
                                    resourceId,
                                    _cancellationToken)
                                .ConfigureAwait(false);

                            return ShapeResult();
                        }

                        return false;
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

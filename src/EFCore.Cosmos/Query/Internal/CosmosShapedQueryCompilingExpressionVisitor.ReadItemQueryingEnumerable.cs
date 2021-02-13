// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
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
            private readonly ReadItemExpression _readItemExpression;
            private readonly Func<CosmosQueryContext, JObject, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly bool _concurrencyDetectionEnabled;

            public ReadItemQueryingEnumerable(
                CosmosQueryContext cosmosQueryContext,
                ReadItemExpression readItemExpression,
                Func<CosmosQueryContext, JObject, T> shaper,
                Type contextType,
                bool standAloneStateManager,
                bool concurrencyDetectionEnabled)
            {
                _cosmosQueryContext = cosmosQueryContext;
                _readItemExpression = readItemExpression;
                _shaper = shaper;
                _contextType = contextType;
                _queryLogger = _cosmosQueryContext.QueryLogger;
                _standAloneStateManager = standAloneStateManager;
                _concurrencyDetectionEnabled = concurrencyDetectionEnabled;
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
                TryGetPartitionId(out var partitionKey);
                return CosmosStrings.NoReadItemQueryString(resourceId, partitionKey);
            }

            private bool TryGetPartitionId(out string partitionKey)
            {
                partitionKey = null;

                var partitionKeyPropertyName = _readItemExpression.EntityType.GetPartitionKeyPropertyName();
                if (partitionKeyPropertyName == null)
                {
                    return true;
                }

                var partitionKeyProperty = _readItemExpression.EntityType.FindProperty(partitionKeyPropertyName);

                if (TryGetParameterValue(partitionKeyProperty, out var value))
                {
                    partitionKey = GetString(partitionKeyProperty, value);

                    return !string.IsNullOrEmpty(partitionKey);
                }

                return false;
            }

            private bool TryGetResourceId(out string resourceId)
            {
                var idProperty = _readItemExpression.EntityType.GetProperties()
                    .FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);

                if (TryGetParameterValue(idProperty, out var value))
                {
                    resourceId = GetString(idProperty, value);

                    if (string.IsNullOrEmpty(resourceId))
                    {
                        throw new InvalidOperationException(CosmosStrings.InvalidResourceId);
                    }

                    return true;
                }

                if (TryGenerateIdFromKeys(idProperty, out var generatedValue))
                {
                    resourceId = GetString(idProperty, generatedValue);

                    return true;
                }

                resourceId = null;
                return false;
            }

            private bool TryGetParameterValue(IProperty property, out object value)
            {
                value = null;
                return _readItemExpression.PropertyParameters.TryGetValue(property, out var parameterName)
                    && _cosmosQueryContext.ParameterValues.TryGetValue(parameterName, out value);
            }

            private static string GetString(IProperty property, object value)
            {
                var converter = property.GetTypeMapping().Converter;

                return converter is null
                    ? (string)value
                    : (string)converter.ConvertToProvider(value);
            }

            private bool TryGenerateIdFromKeys(IProperty idProperty, out object value)
            {
                var entityEntry = Activator.CreateInstance(_readItemExpression.EntityType.ClrType);

#pragma warning disable EF1001 // Internal EF Core API usage.
                var internalEntityEntry = new InternalEntityEntryFactory().Create(
                    _cosmosQueryContext.Context.GetDependencies().StateManager, _readItemExpression.EntityType, entityEntry);
#pragma warning restore EF1001 // Internal EF Core API usage.

                foreach (var keyProperty in _readItemExpression.EntityType.FindPrimaryKey().Properties)
                {
                    var property = _readItemExpression.EntityType.FindProperty(keyProperty.Name);

                    if (TryGetParameterValue(property, out var parameterValue))
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        internalEntityEntry[property] = parameterValue;
#pragma warning restore EF1001 // Internal EF Core API usage.
                    }
                }

#pragma warning disable EF1001 // Internal EF Core API usage.
                internalEntityEntry.SetEntityState(EntityState.Added);

                value = internalEntityEntry[idProperty];

                internalEntityEntry.SetEntityState(EntityState.Detached);
#pragma warning restore EF1001 // Internal EF Core API usage.

                return value != null;
            }

            private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
            {
                private readonly CosmosQueryContext _cosmosQueryContext;
                private readonly ReadItemExpression _readItemExpression;
                private readonly Func<CosmosQueryContext, JObject, T> _shaper;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
                private readonly bool _standAloneStateManager;
                private readonly bool _concurrencyDetectionEnabled;
                private readonly ReadItemQueryingEnumerable<T> _readItemEnumerable;
                private readonly CancellationToken _cancellationToken;

                private JObject _item;
                private bool _hasExecuted;

                public Enumerator(ReadItemQueryingEnumerable<T> readItemEnumerable, CancellationToken cancellationToken = default)
                {
                    _cosmosQueryContext = readItemEnumerable._cosmosQueryContext;
                    _readItemExpression = readItemEnumerable._readItemExpression;
                    _shaper = readItemEnumerable._shaper;
                    _contextType = readItemEnumerable._contextType;
                    _queryLogger = readItemEnumerable._queryLogger;
                    _standAloneStateManager = readItemEnumerable._standAloneStateManager;
                    _concurrencyDetectionEnabled = readItemEnumerable._concurrencyDetectionEnabled;
                    _readItemEnumerable = readItemEnumerable;
                    _cancellationToken = cancellationToken;
                }

                object IEnumerator.Current
                    => Current;

                public T Current { get; private set; }

                public bool MoveNext()
                {
                    if (_concurrencyDetectionEnabled)
                    {
                        _cosmosQueryContext.ConcurrencyDetector.EnterCriticalSection();
                    }

                    try
                    {
                        if (!_hasExecuted)
                        {
                            if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                            {
                                throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                            }

                            if (!_readItemEnumerable.TryGetPartitionId(out var partitionKey))
                            {
                                throw new InvalidOperationException(CosmosStrings.PartitionKeyMissing);
                            }

                            EntityFrameworkEventSource.Log.QueryExecuting();

                            _item = _cosmosQueryContext.CosmosClient.ExecuteReadItem(
                                _readItemExpression.Container,
                                partitionKey,
                                resourceId);

                            return ShapeResult();
                        }

                        return false;
                    }
                    catch (Exception exception)
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                    finally
                    {
                        if (_concurrencyDetectionEnabled)
                        {
                            _cosmosQueryContext.ConcurrencyDetector.ExitCriticalSection();
                        }
                    }
                }

                public async ValueTask<bool> MoveNextAsync()
                {
                    if (_concurrencyDetectionEnabled)
                    {
                        _cosmosQueryContext.ConcurrencyDetector.EnterCriticalSection();
                    }

                    try
                    {
                        if (!_hasExecuted)
                        {
                            if (!_readItemEnumerable.TryGetResourceId(out var resourceId))
                            {
                                throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                            }

                            if (!_readItemEnumerable.TryGetPartitionId(out var partitionKey))
                            {
                                throw new InvalidOperationException(CosmosStrings.PartitionKeyMissing);
                            }

                            EntityFrameworkEventSource.Log.QueryExecuting();

                            _item = await _cosmosQueryContext.CosmosClient.ExecuteReadItemAsync(
                                    _readItemExpression.Container,
                                    partitionKey,
                                    resourceId,
                                    _cancellationToken)
                                .ConfigureAwait(false);

                            return ShapeResult();
                        }

                        return false;
                    }
                    catch (Exception exception)
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                    finally
                    {
                        if (_concurrencyDetectionEnabled)
                        {
                            _cosmosQueryContext.ConcurrencyDetector.ExitCriticalSection();
                        }
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
                    var hasNext = !(_item is null);

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
}

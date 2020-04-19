// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
            
            public ReadItemQueryingEnumerable(
                CosmosQueryContext cosmosQueryContext,
                ReadItemExpression readItemExpression,
                Func<CosmosQueryContext, JObject, T> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _cosmosQueryContext = cosmosQueryContext;
                _readItemExpression = readItemExpression;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncEnumerator(this, cancellationToken);

            public IEnumerator<T> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public string ToQueryString()
            {
                throw new NotImplementedException("Cosmos: ToQueryString for ReadItemQueryingEnumerable #20653");
            }

            private sealed class Enumerator : ReadItemBase, IEnumerator<T>
            {
                private JObject _item;
                private bool _hasExecuted;

                public Enumerator(ReadItemQueryingEnumerable<T> readItemEnumerable) : base(readItemEnumerable)
                {
                }

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    try
                    {
                        using (CosmosQueryContext.ConcurrencyDetector.EnterCriticalSection())
                        {
                            if (!_hasExecuted)
                            {
                                if (!TryGetResourceId(out var resourceId))
                                {
                                    throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                                }
                                
                                if (!TryGetPartitionId(out var partitionKey))
                                {
                                    throw new InvalidOperationException(CosmosStrings.ParitionKeyMissing);
                                }

                                _item = CosmosClient.ExecuteReadItem(
                                    ContainerId,
                                    partitionKey,
                                    resourceId);

                                var hasNext = !(_item is null);

                                Current
                                    = hasNext
                                        ? Shaper(CosmosQueryContext, _item)
                                        : default;

                                _hasExecuted = true;

                                return hasNext;
                            }
                            
                            return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.QueryIterationFailed(ContextType, exception);

                        throw;
                    }
                }

                public void Dispose()
                {
                    _item = null;
                    _hasExecuted = false;
                }

                public void Reset() => throw new NotImplementedException();
            }

            private sealed class AsyncEnumerator : ReadItemBase, IAsyncEnumerator<T>
            {
                private JObject _item;
                private readonly CancellationToken _cancellationToken;
                private bool _hasExecuted;
                
                public AsyncEnumerator(
                    ReadItemQueryingEnumerable<T> readItemEnumerable,
                    CancellationToken cancellationToken) : base(readItemEnumerable)
                {
                    _cancellationToken = cancellationToken;
                }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        using (CosmosQueryContext.ConcurrencyDetector.EnterCriticalSection())
                        {
                            if (!_hasExecuted)
                            {

                                if (!TryGetResourceId(out var resourceId))
                                {
                                    throw new InvalidOperationException(CosmosStrings.ResourceIdMissing);
                                }

                                if (!TryGetPartitionId(out var partitionKey))
                                {
                                    throw new InvalidOperationException(CosmosStrings.ParitionKeyMissing);
                                }

                                _item = await CosmosClient.ExecuteReadItemAsync(
                                    ContainerId,
                                    partitionKey,
                                    resourceId,
                                    _cancellationToken);

                                var hasNext = !(_item is null);

                                Current
                                    = hasNext
                                        ? Shaper(CosmosQueryContext, _item)
                                        : default;

                                _hasExecuted = true;

                                return hasNext;
                            }

                            return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.QueryIterationFailed(ContextType, exception);

                        throw;
                    }
                }

                public ValueTask DisposeAsync()
                {
                    _item = null;
                    _hasExecuted = false;
                    return default;
                }
            }

            private abstract class ReadItemBase
            {
                private readonly IStateManager _stateManager;
                private readonly ReadItemExpression _readItemExpression;
                private readonly IEntityType _entityType;

                protected readonly CosmosQueryContext CosmosQueryContext;
                protected readonly CosmosClientWrapper CosmosClient;
                protected readonly string ContainerId;
                protected readonly Func<CosmosQueryContext, JObject, T> Shaper;
                protected readonly Type ContextType;
                protected readonly IDiagnosticsLogger<DbLoggerCategory.Query> Logger;

                public T Current { get; protected set; }

                protected ReadItemBase(
                    ReadItemQueryingEnumerable<T> readItemEnumerable)
                {
#pragma warning disable EF1001
                    _stateManager = readItemEnumerable._cosmosQueryContext.StateManager;
                    CosmosQueryContext = readItemEnumerable._cosmosQueryContext;
                    _readItemExpression = readItemEnumerable._readItemExpression;
                    _entityType = readItemEnumerable._readItemExpression.EntityType;
                    CosmosClient = readItemEnumerable._cosmosQueryContext.CosmosClient;
                    ContainerId = _readItemExpression.Container;
                    Shaper = readItemEnumerable._shaper;
                    ContextType = readItemEnumerable._contextType;
                    Logger = readItemEnumerable._logger;
                }

                protected bool TryGetPartitionId(out string partitionKey)
                {
                    partitionKey = null;

                    var partitionKeyProperty = _entityType.FindProperty(_entityType.GetPartitionKeyPropertyName());

                    if (TryGetParameterValue(partitionKeyProperty, out var value))
                    {
                        partitionKey = GetString(partitionKeyProperty, value);

                        return !string.IsNullOrEmpty(partitionKey);
                    }

                    return false;
                }

                protected bool TryGetResourceId(out string resourceId)
                {
                    var resourceIdProperty = _entityType.GetProperties()
                        .FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyName);

                    if (TryGetParameterValue(resourceIdProperty, out var value))
                    {
                        resourceId = GetString(resourceIdProperty, value);

                        if (string.IsNullOrEmpty(resourceId))
                        {
                            throw new InvalidOperationException(CosmosStrings.InvalidResourceId);
                        }

                        return true;
                    }

                    if (TryGenerateResourceIdFromKeys(out var generatedValue))
                    {
                        resourceId = GetString(resourceIdProperty, generatedValue);

                        return true;
                    }

                    resourceId = null;
                    return false;
                }

                private bool TryGenerateResourceIdFromKeys(out object value)
                {
                    var entityEntry = Activator.CreateInstance(_entityType.ClrType);

                    var entityProperties = entityEntry.GetType().GetProperties();

#pragma warning disable EF1001
                    var internalEntityEntry = new InternalEntityEntryFactory().Create(_stateManager, _entityType, entityEntry);

                    foreach (var entityProperty in entityProperties)
                    {
                        var property = _entityType.FindProperty(entityProperty.Name);

                        if (TryGetParameterValue(property, out var parameterValue))
                        {
                            internalEntityEntry[property] = parameterValue;
                        }
                    }

#pragma warning disable EF1001
                    var entry = new EntityEntry(internalEntityEntry) { State = EntityState.Added };

                    value = entry.Properties
                        .FirstOrDefault(
                            propertyEntry => propertyEntry.Metadata.GetJsonPropertyName() == StoreKeyConvention.IdPropertyName)
                        .CurrentValue;

                    entry.State = EntityState.Detached;

                    return !(value is null);
                }

                private bool TryGetParameterValue(IProperty property, out object value)
                {
                    value = null;
                    return _readItemExpression.PropertyParameters.TryGetValue(property, out var parameterName)
                        && CosmosQueryContext.ParameterValues.TryGetValue(parameterName, out value);
                }

                private static string GetString(IProperty property, object value)
                {
                    var converter = property.GetTypeMapping().Converter;

                    return converter is null
                        ? (string)value
                        : (string)converter.ConvertToProvider(value);
                }
            }
        }
    }
}

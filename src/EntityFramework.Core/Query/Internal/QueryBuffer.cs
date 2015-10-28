// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryBuffer : IQueryBuffer
    {
        private const int IdentityMapGarbageCollectionThreshold = 500;

        private readonly IStateManager _stateManager;
        private readonly IKeyValueFactorySource _keyValueFactorySource;

        private readonly Dictionary<IKeyValue, WeakReference<object>> _identityMap
            = new Dictionary<IKeyValue, WeakReference<object>>();

        private readonly ConditionalWeakTable<object, object> _valueBuffers
            = new ConditionalWeakTable<object, object>();

        private int _identityMapGarbageCollectionIterations;

        public QueryBuffer(
            [NotNull] IStateManager stateManager,
            [NotNull] IKeyValueFactorySource keyValueFactorySource)
        {
            _stateManager = stateManager;
            _keyValueFactorySource = keyValueFactorySource;
        }

        public virtual object GetEntity(
            IKeyValue keyValue, EntityLoadInfo entityLoadInfo, bool queryStateManager)
        {
            // hot path
            Debug.Assert(keyValue != null);

            if (queryStateManager)
            {
                var entry = _stateManager.TryGetEntry(keyValue);

                if (entry != null)
                {
                    return entry.Entity;
                }
            }

            object entity;

            WeakReference<object> weakReference;
            if (!_identityMap.TryGetValue(keyValue, out weakReference)
                || !weakReference.TryGetTarget(out entity))
            {
                entity = entityLoadInfo.Materialize();

                if (weakReference != null)
                {
                    weakReference.SetTarget(entity);
                }
                else
                {
                    GarbageCollectIdentityMap();

                    _identityMap.Add(keyValue, new WeakReference<object>(entity));
                }

                _valueBuffers.Add(entity, entityLoadInfo.ValueBuffer);
            }

            return entity;
        }

        private void GarbageCollectIdentityMap()
        {
            if (++_identityMapGarbageCollectionIterations == IdentityMapGarbageCollectionThreshold)
            {
                var deadEntries = new List<IKeyValue>();

                foreach (var entry in _identityMap)
                {
                    object _;
                    if (!entry.Value.TryGetTarget(out _))
                    {
                        deadEntries.Add(entry.Key);
                    }
                }

                foreach (var keyValue in deadEntries)
                {
                    _identityMap.Remove(keyValue);
                }

                _identityMapGarbageCollectionIterations = 0;
            }
        }

        public virtual object GetPropertyValue(object entity, IProperty property)
        {
            var entry = _stateManager.TryGetEntry(entity);

            if (entry != null)
            {
                return entry[property];
            }

            object boxedValueBuffer;
            var found = _valueBuffers.TryGetValue(entity, out boxedValueBuffer);

            Debug.Assert(found);

            var valueBuffer = (ValueBuffer)boxedValueBuffer;

            return valueBuffer[property.GetIndex()];
        }

        public virtual void StartTracking(object entity, EntityTrackingInfo entityTrackingInfo)
        {
            object boxedValueBuffer;
            if (_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                entityTrackingInfo
                    .StartTracking(_stateManager, entity, (ValueBuffer)boxedValueBuffer);
            }

            foreach (var includedEntity 
                in entityTrackingInfo.GetIncludedEntities(entity)
                    .Where(includedEntity
                        => _valueBuffers.TryGetValue(includedEntity.Entity, out boxedValueBuffer)))
            {
                includedEntity.StartTracking(_stateManager, (ValueBuffer)boxedValueBuffer);
            }
        }

        public virtual void Include(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager)
            => Include(entity, navigationPath, relatedEntitiesLoaders, 0, queryStateManager);

        private void Include(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            int currentNavigationIndex,
            bool queryStateManager)
        {
            if (entity == null
                || currentNavigationIndex == navigationPath.Count)
            {
                return;
            }

            IKeyValue primaryKeyValue;
            Func<ValueBuffer, IKeyValue> relatedKeyFactory;

            var targetEntityType
                = IncludeCore(
                    entity,
                    navigationPath[currentNavigationIndex],
                    out primaryKeyValue,
                    out relatedKeyFactory);

            var keyValueFactory
                = _keyValueFactorySource
                    .GetKeyFactory(targetEntityType.FindPrimaryKey());

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                relatedEntitiesLoaders[currentNavigationIndex](primaryKeyValue, relatedKeyFactory)
                    .Select(eli =>
                        {
                            var keyValue = keyValueFactory.Create(eli.ValueBuffer);

                            object targetEntity = null;

                            if (!ReferenceEquals(keyValue, KeyValue.InvalidKeyValue))
                            {
                                targetEntity = GetEntity(keyValue, eli, queryStateManager);
                            }

                            Include(
                                targetEntity,
                                navigationPath,
                                relatedEntitiesLoaders,
                                currentNavigationIndex + 1,
                                queryStateManager);

                            return targetEntity;
                        })
                    .Where(e => e != null)
                    .ToList());
        }

        public virtual Task IncludeAsync(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<AsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            CancellationToken cancellationToken,
            bool queryStateManager)
            => IncludeAsync(entity, navigationPath, relatedEntitiesLoaders, cancellationToken, 0, queryStateManager);

        private async Task IncludeAsync(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<AsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            CancellationToken cancellationToken,
            int currentNavigationIndex,
            bool queryStateManager)
        {
            if (entity == null
                || currentNavigationIndex == navigationPath.Count)
            {
                return;
            }

            IKeyValue primaryKeyValue;
            Func<ValueBuffer, IKeyValue> relatedKeyFactory;

            var targetEntityType
                = IncludeCore(
                    entity,
                    navigationPath[currentNavigationIndex],
                    out primaryKeyValue,
                    out relatedKeyFactory);

            var keyValueFactory
                = _keyValueFactorySource
                    .GetKeyFactory(targetEntityType.FindPrimaryKey());

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                await relatedEntitiesLoaders[currentNavigationIndex](primaryKeyValue, relatedKeyFactory)
                    .Select(async (eli, ct) =>
                        {
                            var keyValue = keyValueFactory.Create(eli.ValueBuffer);

                            object targetEntity = null;

                            if (!ReferenceEquals(keyValue, KeyValue.InvalidKeyValue))
                            {
                                targetEntity = GetEntity(keyValue, eli, queryStateManager);
                            }

                            await IncludeAsync(
                                targetEntity,
                                navigationPath,
                                relatedEntitiesLoaders,
                                ct,
                                currentNavigationIndex + 1,
                                queryStateManager);

                            return targetEntity;
                        })
                    .Where(e => e != null)
                    .ToList(cancellationToken));
        }

        private IEntityType IncludeCore(
            object entity,
            INavigation navigation,
            out IKeyValue primaryKeyValue,
            out Func<ValueBuffer, IKeyValue> relatedKeyFactory)
        {
            var keyFactory
                = _keyValueFactorySource
                    .GetKeyFactory(navigation.ForeignKey.PrincipalKey);

            var targetEntityType = navigation.GetTargetType();

            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                var entry = _stateManager.TryGetEntry(entity);

                Debug.Assert(entry != null);

                primaryKeyValue
                    = navigation.IsDependentToPrincipal()
                        ? entry.GetDependentKeyValue(navigation.ForeignKey)
                        : entry.GetPrimaryKeyValue();
            }
            else
            {
                primaryKeyValue
                    = navigation.IsDependentToPrincipal()
                        ? keyFactory
                            .Create(
                                navigation.ForeignKey.Properties,
                                (ValueBuffer)boxedValueBuffer)
                        : keyFactory
                            .Create(
                                navigation.ForeignKey.PrincipalKey.Properties,
                                (ValueBuffer)boxedValueBuffer);
            }

            if (navigation.IsDependentToPrincipal())
            {
                relatedKeyFactory
                    = valueBuffer =>
                        keyFactory
                            .Create(
                                navigation.ForeignKey.PrincipalKey.Properties,
                                valueBuffer);
            }
            else
            {
                relatedKeyFactory
                    = valueBuffer =>
                        keyFactory
                            .Create(
                                navigation.ForeignKey.Properties,
                                valueBuffer);
            }

            return targetEntityType;
        }

        private void LoadNavigationProperties(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            int currentNavigationIndex,
            IReadOnlyList<object> relatedEntities)
        {
            if (navigationPath[currentNavigationIndex].IsDependentToPrincipal()
                && relatedEntities.Any())
            {
                navigationPath[currentNavigationIndex]
                    .GetSetter()
                    .SetClrValue(entity, relatedEntities[0]);

                var inverseNavigation = navigationPath[currentNavigationIndex].FindInverse();

                if (inverseNavigation != null)
                {
                    if (inverseNavigation.IsCollection())
                    {
                        inverseNavigation
                            .GetCollectionAccessor()
                            .AddRange(relatedEntities[0], new[] { entity });
                    }
                    else
                    {
                        inverseNavigation
                            .GetSetter()
                            .SetClrValue(relatedEntities[0], entity);
                    }
                }
            }
            else
            {
                if (navigationPath[currentNavigationIndex].IsCollection())
                {
                    navigationPath[currentNavigationIndex]
                        .GetCollectionAccessor()
                        .AddRange(entity, relatedEntities);

                    var inverseNavigation = navigationPath[currentNavigationIndex].FindInverse();

                    if (inverseNavigation != null)
                    {
                        var clrPropertySetter
                            = inverseNavigation.GetSetter();

                        foreach (var relatedEntity in relatedEntities)
                        {
                            clrPropertySetter.SetClrValue(relatedEntity, entity);
                        }
                    }
                }
                else if (relatedEntities.Any())
                {
                    navigationPath[currentNavigationIndex]
                        .GetSetter()
                        .SetClrValue(entity, relatedEntities[0]);

                    var inverseNavigation = navigationPath[currentNavigationIndex].FindInverse();

                    inverseNavigation?.GetSetter()
                        .SetClrValue(relatedEntities[0], entity);
                }
            }
        }
    }
}

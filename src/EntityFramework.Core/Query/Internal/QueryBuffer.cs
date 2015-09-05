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
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryBuffer : IQueryBuffer
    {
        private const int IdentityMapGarbageCollectionThreshold = 500;

        private readonly IStateManager _stateManager;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;
        private readonly IClrCollectionAccessorSource _clrCollectionAccessorSource;
        private readonly IClrAccessorSource<IClrPropertySetter> _clrPropertySetterSource;

        private readonly Dictionary<EntityKey, WeakReference<object>> _identityMap
            = new Dictionary<EntityKey, WeakReference<object>>();

        private readonly ConditionalWeakTable<object, object> _valueBuffers
            = new ConditionalWeakTable<object, object>();

        private int _identityMapGarbageCollectionIterations;

        public QueryBuffer(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource clrCollectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> clrPropertySetterSource)
        {
            _stateManager = stateManager;
            _entityKeyFactorySource = entityKeyFactorySource;
            _clrCollectionAccessorSource = clrCollectionAccessorSource;
            _clrPropertySetterSource = clrPropertySetterSource;
        }

        public virtual object GetEntity(
            IEntityType entityType,
            EntityKey entityKey,
            EntityLoadInfo entityLoadInfo,
            bool queryStateManager)
        {
            // hot path
            Debug.Assert(entityType != null);
            Debug.Assert(entityKey != null);

            if (entityKey == EntityKey.InvalidEntityKey)
            {
                throw new InvalidOperationException(
                    Strings.InvalidEntityKeyOnQuery(entityType.DisplayName()));
            }

            if (queryStateManager)
            {
                var entry = _stateManager.TryGetEntry(entityKey);

                if (entry != null)
                {
                    return entry.Entity;
                }
            }

            object entity;

            WeakReference<object> weakReference;
            if (!_identityMap.TryGetValue(entityKey, out weakReference)
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

                    _identityMap.Add(entityKey, new WeakReference<object>(entity));
                }

                _valueBuffers.Add(entity, entityLoadInfo.ValueBuffer);
            }

            return entity;
        }

        private void GarbageCollectIdentityMap()
        {
            if (++_identityMapGarbageCollectionIterations == IdentityMapGarbageCollectionThreshold)
            {
                var deadEntries = new List<EntityKey>();

                foreach (var entry in _identityMap)
                {
                    object _;
                    if (!entry.Value.TryGetTarget(out _))
                    {
                        deadEntries.Add(entry.Key);
                    }
                }

                foreach (var entityKey in deadEntries)
                {
                    _identityMap.Remove(entityKey);
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

            return valueBuffer[property.Index];
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
        {
            Include(entity, navigationPath, relatedEntitiesLoaders, 0, queryStateManager);
        }

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

            EntityKey primaryKey;
            Func<ValueBuffer, EntityKey> relatedKeyFactory;

            var targetEntityType
                = IncludeCore(
                    entity,
                    navigationPath[currentNavigationIndex],
                    out primaryKey,
                    out relatedKeyFactory);

            var keyProperties
                = targetEntityType.GetPrimaryKey().Properties;

            var entityKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(targetEntityType.GetPrimaryKey());

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                relatedEntitiesLoaders[currentNavigationIndex](primaryKey, relatedKeyFactory)
                    .Select(eli =>
                        {
                            var entityKey
                                = entityKeyFactory
                                    .Create(keyProperties, eli.ValueBuffer);

                            object targetEntity = null;

                            if (entityKey != EntityKey.InvalidEntityKey)
                            {
                                targetEntity
                                    = GetEntity(
                                        targetEntityType,
                                        entityKey,
                                        eli,
                                        queryStateManager);
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

            EntityKey primaryKey;
            Func<ValueBuffer, EntityKey> relatedKeyFactory;

            var targetEntityType
                = IncludeCore(
                    entity,
                    navigationPath[currentNavigationIndex],
                    out primaryKey,
                    out relatedKeyFactory);

            var keyProperties
                = targetEntityType.GetPrimaryKey().Properties;

            var entityKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(targetEntityType.GetPrimaryKey());

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                await AsyncEnumerableExtensions.Select(relatedEntitiesLoaders[currentNavigationIndex](primaryKey, relatedKeyFactory), async (eli, ct) =>
                    {
                        var entityKey
                            = entityKeyFactory
                                .Create(keyProperties, eli.ValueBuffer);

                        object targetEntity = null;

                        if (entityKey != EntityKey.InvalidEntityKey)
                        {
                            targetEntity
                                = GetEntity(
                                    targetEntityType,
                                    entityKey,
                                    eli,
                                    queryStateManager);
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
            out EntityKey primaryKey,
            out Func<ValueBuffer, EntityKey> relatedKeyFactory)
        {
            var keyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(navigation.ForeignKey.PrincipalKey);

            var targetEntityType = navigation.GetTargetType();

            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                var entry = _stateManager.TryGetEntry(entity);

                Debug.Assert(entry != null);

                primaryKey
                    = navigation.PointsToPrincipal()
                        ? entry.GetDependentKeyValue(navigation.ForeignKey)
                        : entry.GetPrimaryKeyValue();
            }
            else
            {
                primaryKey
                    = navigation.PointsToPrincipal()
                        ? keyFactory
                            .Create(
                                navigation.ForeignKey.Properties,
                                (ValueBuffer)boxedValueBuffer)
                        : keyFactory
                            .Create(
                                navigation.ForeignKey.PrincipalKey.Properties,
                                (ValueBuffer)boxedValueBuffer);
            }

            if (navigation.PointsToPrincipal())
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
            if (navigationPath[currentNavigationIndex].PointsToPrincipal()
                && relatedEntities.Any())
            {
                _clrPropertySetterSource
                    .GetAccessor(navigationPath[currentNavigationIndex])
                    .SetClrValue(entity, relatedEntities[0]);

                var inverseNavigation = navigationPath[currentNavigationIndex].FindInverse();

                if (inverseNavigation != null)
                {
                    if (inverseNavigation.IsCollection())
                    {
                        _clrCollectionAccessorSource
                            .GetAccessor(inverseNavigation)
                            .AddRange(relatedEntities[0], new[] { entity });
                    }
                    else
                    {
                        _clrPropertySetterSource
                            .GetAccessor(inverseNavigation)
                            .SetClrValue(relatedEntities[0], entity);
                    }
                }
            }
            else
            {
                if (navigationPath[currentNavigationIndex].IsCollection())
                {
                    _clrCollectionAccessorSource
                        .GetAccessor(navigationPath[currentNavigationIndex])
                        .AddRange(entity, relatedEntities);

                    var inverseNavigation = navigationPath[currentNavigationIndex].FindInverse();

                    if (inverseNavigation != null)
                    {
                        var clrPropertySetter
                            = _clrPropertySetterSource
                                .GetAccessor(inverseNavigation);

                        foreach (var relatedEntity in relatedEntities)
                        {
                            clrPropertySetter.SetClrValue(relatedEntity, entity);
                        }
                    }
                }
                else if (relatedEntities.Any())
                {
                    _clrPropertySetterSource
                        .GetAccessor(navigationPath[currentNavigationIndex])
                        .SetClrValue(entity, relatedEntities[0]);

                    var inverseNavigation = navigationPath[currentNavigationIndex].FindInverse();

                    if (inverseNavigation != null)
                    {
                        _clrPropertySetterSource
                            .GetAccessor(inverseNavigation)
                            .SetClrValue(relatedEntities[0], entity);
                    }
                }
            }
        }
    }
}

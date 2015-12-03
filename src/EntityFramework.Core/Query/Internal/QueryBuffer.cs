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
        private readonly IStateManager _stateManager;

        private IWeakReferenceIdentityMap _identityMap0;
        private IWeakReferenceIdentityMap _identityMap1;
        private Dictionary<IKey, IWeakReferenceIdentityMap> _identityMaps;

        private readonly ConditionalWeakTable<object, object> _valueBuffers
            = new ConditionalWeakTable<object, object>();

        public QueryBuffer(
            [NotNull] IStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public virtual object GetEntity(
            IKey key, EntityLoadInfo entityLoadInfo, bool queryStateManager, bool throwOnNullKey)
        {
            if (queryStateManager)
            {
                var entry = _stateManager.TryGetEntry(key, entityLoadInfo.ValueBuffer, throwOnNullKey);

                if (entry != null)
                {
                    return entry.Entity;
                }
            }

            var identityMap = GetOrCreateIdentityMap(key);

            bool hasNullKey;
            var weakReference = identityMap.TryGetEntity(entityLoadInfo.ValueBuffer, out hasNullKey);
            if (hasNullKey)
            {
                if (throwOnNullKey)
                {
                    throw new InvalidOperationException(CoreStrings.InvalidKeyValue(key.DeclaringEntityType.DisplayName()));
                }
                return null;
            }

            object entity;
            if (weakReference == null
                || !weakReference.TryGetTarget(out entity))
            {
                entity = entityLoadInfo.Materialize();

                if (weakReference != null)
                {
                    weakReference.SetTarget(entity);
                }
                else
                {
                    identityMap.CollectGarbage();

                    identityMap.Add(entityLoadInfo.ValueBuffer, entity);
                }

                _valueBuffers.Add(entity, entityLoadInfo.ValueBuffer);
            }

            return entity;
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
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                boxedValueBuffer = ValueBuffer.Empty;
            }

            entityTrackingInfo
                .StartTracking(_stateManager, entity, (ValueBuffer)boxedValueBuffer);

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

            var navigation = navigationPath[currentNavigationIndex];
            var keyComparer = IncludeCore(entity, navigation);
            var key = navigation.GetTargetType().FindPrimaryKey();

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                relatedEntitiesLoaders[currentNavigationIndex](keyComparer)
                    .Select(eli =>
                        {
                            var targetEntity = GetEntity(key, eli, queryStateManager, throwOnNullKey: false);

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

            var navigation = navigationPath[currentNavigationIndex];
            var keyComparer = IncludeCore(entity, navigation);
            var key = navigation.GetTargetType().FindPrimaryKey();

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                await AsyncEnumerableExtensions.Select(relatedEntitiesLoaders[currentNavigationIndex](keyComparer), async (eli, ct) =>
                    {
                        var targetEntity = GetEntity(key, eli, queryStateManager, throwOnNullKey: false);

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

        private IIncludeKeyComparer IncludeCore(
            object entity,
            INavigation navigation)
        {
            var identityMap = GetOrCreateIdentityMap(navigation.ForeignKey.PrincipalKey);

            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                var entry = _stateManager.TryGetEntry(entity);

                Debug.Assert(entry != null);

                return identityMap.CreateIncludeKeyComparer(navigation, entry);
            }

            return identityMap.CreateIncludeKeyComparer(navigation, (ValueBuffer)boxedValueBuffer);
        }

        private static void LoadNavigationProperties(
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

        private IWeakReferenceIdentityMap GetOrCreateIdentityMap(IKey key)
        {
            if (_identityMap0 == null)
            {
                _identityMap0 = key.GetWeakReferenceIdentityMapFactory()();
                return _identityMap0;
            }

            if (_identityMap0.Key == key)
            {
                return _identityMap0;
            }

            if (_identityMap1 == null)
            {
                _identityMap1 = key.GetWeakReferenceIdentityMapFactory()();
                return _identityMap1;
            }

            if (_identityMap1.Key == key)
            {
                return _identityMap1;
            }

            if (_identityMaps == null)
            {
                _identityMaps = new Dictionary<IKey, IWeakReferenceIdentityMap>();
            }

            IWeakReferenceIdentityMap identityMap;
            if (!_identityMaps.TryGetValue(key, out identityMap))
            {
                identityMap = key.GetWeakReferenceIdentityMapFactory()();
                _identityMaps[key] = identityMap;
            }
            return identityMap;
        }
    }
}

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
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryBuffer : IQueryBuffer
    {
        private readonly LazyRef<IStateManager> _stateManager;
        private readonly LazyRef<IChangeDetector> _changeDetector;

        private IWeakReferenceIdentityMap _identityMap0;
        private IWeakReferenceIdentityMap _identityMap1;
        private Dictionary<IKey, IWeakReferenceIdentityMap> _identityMaps;

        private readonly ConditionalWeakTable<object, object> _valueBuffers
            = new ConditionalWeakTable<object, object>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryBuffer(
            [NotNull] LazyRef<IStateManager> stateManager,
            [NotNull] LazyRef<IChangeDetector> changeDetector)
        {
            _stateManager = stateManager;
            _changeDetector = changeDetector;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetEntity(
            IKey key, EntityLoadInfo entityLoadInfo, bool queryStateManager, bool throwOnNullKey)
        {
            if (queryStateManager)
            {
                var entry = _stateManager.Value.TryGetEntry(key, entityLoadInfo.ValueBuffer, throwOnNullKey);

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetPropertyValue(object entity, IProperty property)
        {
            var entry = _stateManager.Value.TryGetEntry(entity);

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StartTracking(object entity, EntityTrackingInfo entityTrackingInfo)
        {
            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                boxedValueBuffer = ValueBuffer.Empty;
            }

            entityTrackingInfo
                .StartTracking(_stateManager.Value, entity, (ValueBuffer)boxedValueBuffer);

            foreach (var includedEntity
                in entityTrackingInfo.GetIncludedEntities(entity)
                    .Where(includedEntity
                        => _valueBuffers.TryGetValue(includedEntity.Entity, out boxedValueBuffer)))
            {
                includedEntity.StartTracking(_stateManager.Value, (ValueBuffer)boxedValueBuffer);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Include(
            QueryContext queryContext,
            object entity,
            IncludeSpecification includeSpecification,
            IReadOnlyDictionary<IncludeSpecification, IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager)
        {
            var navigation = includeSpecification.Navigation;
            var keyComparer = IncludeCore(entity, navigation);
            var key = navigation.GetTargetType().FindPrimaryKey();

            LoadNavigationProperties(
                entity,
                includeSpecification,
                relatedEntitiesLoaders[includeSpecification]
                    .Load(queryContext, keyComparer)
                    .Select(eli =>
                    {
                        var targetEntity = GetEntity(key, eli, queryStateManager, throwOnNullKey: false);

                        if (targetEntity != null)
                        {
                            foreach (var reference in includeSpecification.References)
                            {
                                Include(
                                    queryContext,
                                    targetEntity,
                                    reference,
                                    relatedEntitiesLoaders,
                                    queryStateManager);
                            }
                        }

                        return targetEntity;
                    })
                    .Where(e => e != null)
                    .ToList(),
                queryStateManager);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task IncludeAsync(
            QueryContext queryContext,
            object entity,
            IncludeSpecification includeSpecification,
            IReadOnlyDictionary<IncludeSpecification, IAsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager,
            CancellationToken cancellationToken)
        {
            INavigation navigation = includeSpecification.Navigation;
            var keyComparer = IncludeCore(entity, navigation);
            var key = navigation.GetTargetType().FindPrimaryKey();

            var relatedEntityLoadInfos
                = relatedEntitiesLoaders[includeSpecification]
                    .Load(queryContext, keyComparer);

            var relatedObjects = new List<object>();

            using (var asyncEnumerator = relatedEntityLoadInfos.GetEnumerator())
            {
                while (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    var targetEntity
                        = GetEntity(key, asyncEnumerator.Current, queryStateManager, throwOnNullKey: false);

                    if (targetEntity != null)
                    {
                        foreach (var reference in includeSpecification.References)
                        {
                            await IncludeAsync(
                                queryContext,
                                targetEntity,
                                reference,
                                relatedEntitiesLoaders,
                                queryStateManager,
                                cancellationToken);
                        }

                        relatedObjects.Add(targetEntity);
                    }
                }
            }

            LoadNavigationProperties(entity, includeSpecification, relatedObjects, queryStateManager);
        }

        private IIncludeKeyComparer IncludeCore(
            object entity,
            INavigation navigation)
        {
            var identityMap = GetOrCreateIdentityMap(navigation.ForeignKey.PrincipalKey);

            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                var entry = _stateManager.Value.TryGetEntry(entity);

                Debug.Assert(entry != null);

                return identityMap.CreateIncludeKeyComparer(navigation, entry);
            }

            return identityMap.CreateIncludeKeyComparer(navigation, (ValueBuffer)boxedValueBuffer);
        }

        private void LoadNavigationProperties(
            object entity,
            IncludeSpecification includeSpecification,
            IReadOnlyList<object> relatedEntities,
            bool tracking)
        {
            if (tracking)
            {
                _changeDetector.Value.Suspend();
            }

            try
            {
                var navigation = includeSpecification.Navigation;
                var inverseNavigation = navigation.FindInverse();

                if (navigation.IsDependentToPrincipal()
                    && relatedEntities.Any())
                {
                    var relatedEntity = relatedEntities[0];

                    SetNavigation(entity, navigation, relatedEntity, tracking);

                    if (inverseNavigation != null)
                    {
                        if (inverseNavigation.IsCollection())
                        {
                            AddToCollection(relatedEntity, inverseNavigation, entity, tracking);
                        }
                        else
                        {
                            SetNavigation(relatedEntity, inverseNavigation, entity, tracking);
                        }
                    }
                }
                else
                {
                    if (navigation.IsCollection())
                    {
                        AddRangeToCollection(entity, navigation, relatedEntities, tracking);

                        if (inverseNavigation != null)
                        {
                            var setter = inverseNavigation.GetSetter();

                            foreach (var relatedEntity in relatedEntities)
                            {
                                SetNavigation(relatedEntity, inverseNavigation, setter, entity, tracking);
                            }
                        }
                    }
                    else if (relatedEntities.Any())
                    {
                        var relatedEntity = relatedEntities[0];

                        SetNavigation(entity, navigation, relatedEntity, tracking);

                        if (inverseNavigation != null)
                        {
                            SetNavigation(relatedEntity, inverseNavigation, entity, tracking);
                        }
                    }
                }
            }
            finally
            {
                if (tracking)
                {
                    _changeDetector.Value.Resume();
                }
            }
        }

        private void SetNavigation(object entity, INavigation navigation, object value, bool tracking)
            => SetNavigation(entity, navigation, navigation.GetSetter(), value, tracking);

        private void SetNavigation(object entity, INavigation navigation, IClrPropertySetter setter, object value, bool tracking)
        {
            setter.SetClrValue(entity, value);

            if (tracking)
            {
                _stateManager.Value.TryGetEntry(entity)?.SetRelationshipSnapshotValue(navigation, value);
            }
        }

        private void AddToCollection(object entity, INavigation navigation, object value, bool tracking)
        {
            navigation.GetCollectionAccessor().Add(entity, value);

            if (tracking)
            {
                _stateManager.Value.TryGetEntry(entity)?.AddToCollectionSnapshot(navigation, value);
            }
        }

        private void AddRangeToCollection(object entity, INavigation navigation, IEnumerable<object> values, bool tracking)
        {
            navigation.GetCollectionAccessor().AddRange(entity, values);

            if (tracking)
            {
                _stateManager.Value.TryGetEntry(entity)?.AddRangeToCollectionSnapshot(navigation, values);
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

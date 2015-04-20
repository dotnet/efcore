// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryBuffer : IQueryBuffer
    {
        private readonly IStateManager _stateManager;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;
        private readonly IClrCollectionAccessorSource _clrCollectionAccessorSource;
        private readonly IClrAccessorSource<IClrPropertySetter> _clrPropertySetterSource;

        private sealed class BufferedEntity
        {
            private readonly IEntityType _entityType;

            public BufferedEntity(IEntityType entityType, IValueReader valueReader)
            {
                _entityType = entityType;
                ValueReader = valueReader;
            }

            public object Instance { get; set; }

            public IValueReader ValueReader { get; }

            public void StartTracking(IStateManager stateManager)
            {
                stateManager.StartTracking(_entityType, Instance, ValueReader);
            }
        }

        private readonly Dictionary<EntityKey, BufferedEntity> _byEntityKey
            = new Dictionary<EntityKey, BufferedEntity>();

        private readonly IDictionary<object, List<BufferedEntity>> _byEntityInstance
            = new Dictionary<object, List<BufferedEntity>>();

        public QueryBuffer(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource clrCollectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> clrPropertySetterSource)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(clrCollectionAccessorSource, nameof(clrCollectionAccessorSource));
            Check.NotNull(clrPropertySetterSource, nameof(clrPropertySetterSource));

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
                return null;
            }

            if (queryStateManager)
            {
                var entry = _stateManager.TryGetEntry(entityKey);

                if (entry != null)
                {
                    return entry.Entity;
                }
            }

            BufferedEntity bufferedEntity;
            if (!_byEntityKey.TryGetValue(entityKey, out bufferedEntity))
            {
                bufferedEntity
                    = new BufferedEntity(entityType, entityLoadInfo.ValueReader)
                        {
                            // TODO: Optimize this by not materializing when not required for query execution. i.e.
                            //       entity is only needed in final results
                            Instance = entityLoadInfo.Materialize()
                        };

                _byEntityKey.Add(entityKey, bufferedEntity);
                _byEntityInstance.Add(bufferedEntity.Instance, new List<BufferedEntity> { bufferedEntity });
            }

            return bufferedEntity.Instance;
        }

        public virtual void StartTracking(object entity)
        {
            Check.NotNull(entity, nameof(entity));

            List<BufferedEntity> bufferedEntities;
            if (_byEntityInstance.TryGetValue(entity, out bufferedEntities))
            {
                foreach (var bufferedEntity in bufferedEntities.Skip(1).Where(e => e != null))
                {
                    bufferedEntity.StartTracking(_stateManager);
                }
            }
        }

        public virtual void Include(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(relatedEntitiesLoaders, nameof(relatedEntitiesLoaders));

            Include(entity, navigationPath, relatedEntitiesLoaders, 0, querySourceRequiresTracking);
        }

        private void Include(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            int currentNavigationIndex,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(relatedEntitiesLoaders, nameof(relatedEntitiesLoaders));

            if (entity == null
                || currentNavigationIndex == navigationPath.Count)
            {
                return;
            }

            EntityKey primaryKey;
            List<BufferedEntity> bufferedEntities;
            Func<IValueReader, EntityKey> relatedKeyFactory;

            var targetEntityType
                = IncludeCore(
                    entity,
                    navigationPath[currentNavigationIndex],
                    out primaryKey,
                    out bufferedEntities,
                    out relatedKeyFactory);

            var keyProperties
                = targetEntityType.GetPrimaryKey().Properties;

            var entityKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(keyProperties);

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                relatedEntitiesLoaders[currentNavigationIndex](primaryKey, relatedKeyFactory)
                    .Select(eli =>
                        {
                            var targetEntity
                                = GetTargetEntity(
                                    targetEntityType,
                                    entityKeyFactory,
                                    keyProperties,
                                    eli,
                                    bufferedEntities,
                                    querySourceRequiresTracking);

                            Include(
                                targetEntity,
                                navigationPath,
                                relatedEntitiesLoaders,
                                currentNavigationIndex + 1,
                                querySourceRequiresTracking);

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
            bool querySourceRequiresTracking)
        {
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(relatedEntitiesLoaders, nameof(relatedEntitiesLoaders));

            return IncludeAsync(entity, navigationPath, relatedEntitiesLoaders, cancellationToken, 0, querySourceRequiresTracking);
        }

        private async Task IncludeAsync(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<AsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            CancellationToken cancellationToken,
            int currentNavigationIndex,
            bool querySourceRequiresTracking)
        {
            if (entity == null
                || currentNavigationIndex == navigationPath.Count)
            {
                return;
            }

            EntityKey primaryKey;
            List<BufferedEntity> bufferedEntities;
            Func<IValueReader, EntityKey> relatedKeyFactory;

            var targetEntityType
                = IncludeCore(
                    entity,
                    navigationPath[currentNavigationIndex],
                    out primaryKey,
                    out bufferedEntities,
                    out relatedKeyFactory);

            var keyProperties
                = targetEntityType.GetPrimaryKey().Properties;

            var entityKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(keyProperties);

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                await relatedEntitiesLoaders[currentNavigationIndex](primaryKey, relatedKeyFactory)
                    .Select(async (eli, ct) =>
                        {
                            var targetEntity
                                = GetTargetEntity(
                                    targetEntityType,
                                    entityKeyFactory,
                                    keyProperties,
                                    eli,
                                    bufferedEntities,
                                    querySourceRequiresTracking);

                            await IncludeAsync(
                                targetEntity,
                                navigationPath,
                                relatedEntitiesLoaders,
                                ct,
                                currentNavigationIndex + 1,
                                querySourceRequiresTracking)
                                .WithCurrentCulture();

                            return targetEntity;
                        })
                    .Where(e => e != null)
                    .ToList(cancellationToken)
                    .WithCurrentCulture());
        }

        private IEntityType IncludeCore(
            object entity,
            INavigation navigation,
            out EntityKey primaryKey,
            out List<BufferedEntity> bufferedEntities,
            out Func<IValueReader, EntityKey> relatedKeyFactory)
        {
            var primaryKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(navigation.ForeignKey.PrincipalKey.Properties);

            var foreignKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(navigation.ForeignKey.Properties);

            var targetEntityType = navigation.GetTargetType();

            if (!_byEntityInstance.TryGetValue(entity, out bufferedEntities))
            {
                _byEntityInstance.Add(entity, bufferedEntities = new List<BufferedEntity> { null });

                var entry = _stateManager.TryGetEntry(entity);

                Debug.Assert(entry != null);

                primaryKey
                    = navigation.PointsToPrincipal()
                        ? entry.GetDependentKeySnapshot(navigation.ForeignKey)
                        : entry.GetPrimaryKeyValue();
            }
            else
            {
                // if entity is already in state manager it can't be added to the buffer. 'null' value was added to signify that
                // this means relevant key should be acquired  from state manager rather than buffered reader
                if (bufferedEntities[0] == null)
                {
                    var entry = _stateManager.TryGetEntry(entity);

                    Debug.Assert(entry != null);

                    primaryKey
                        = navigation.PointsToPrincipal()
                            ? entry.GetDependentKeySnapshot(navigation.ForeignKey)
                            : entry.GetPrimaryKeyValue();
                }
                else
                {
                    primaryKey
                        = navigation.PointsToPrincipal()
                            ? foreignKeyFactory
                                .Create(
                                    targetEntityType,
                                    navigation.ForeignKey.Properties,
                                    bufferedEntities[0].ValueReader)
                            : primaryKeyFactory
                                .Create(
                                    navigation.EntityType,
                                    navigation.ForeignKey.PrincipalKey.Properties,
                                    bufferedEntities[0].ValueReader);
                }
            }

            if (navigation.PointsToPrincipal())
            {
                relatedKeyFactory
                    = valueReader =>
                        primaryKeyFactory
                            .Create(
                                targetEntityType,
                                navigation.ForeignKey.PrincipalKey.Properties,
                                valueReader);
            }
            else
            {
                relatedKeyFactory
                    = valueReader =>
                        foreignKeyFactory
                            .Create(
                                navigation.EntityType,
                                navigation.ForeignKey.Properties,
                                valueReader);
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

        private object GetTargetEntity(
            IEntityType targetEntityType,
            EntityKeyFactory entityKeyFactory,
            IReadOnlyList<IProperty> keyProperties,
            EntityLoadInfo entityLoadInfo,
            ICollection<BufferedEntity> bufferedEntities,
            bool querySourceRequiresTracking)
        {
            var entityKey
                = entityKeyFactory
                    .Create(targetEntityType, keyProperties, entityLoadInfo.ValueReader);

            var targetEntity
                = GetEntity(
                    targetEntityType,
                    entityKey,
                    entityLoadInfo,
                    querySourceRequiresTracking);

            if (targetEntity != null)
            {
                List<BufferedEntity> bufferedTargetEntities;
                bufferedEntities.Add(
                    _byEntityInstance.TryGetValue(targetEntity, out bufferedTargetEntities)
                        ? bufferedTargetEntities[0]
                        : new BufferedEntity(targetEntityType, entityLoadInfo.ValueReader)
                            {
                                Instance = targetEntity
                            });
            }

            return targetEntity;
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryBuffer : IQueryBuffer
    {
        private readonly StateManager _stateManager;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;
        private readonly EntityMaterializerSource _materializerSource;
        private readonly ClrCollectionAccessorSource _clrCollectionAccessorSource;
        private readonly ClrPropertySetterSource _clrPropertySetterSource;

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

            public void StartTracking(StateManager stateManager)
            {
                stateManager.StartTracking(_entityType, Instance, ValueReader);
            }
        }

        private readonly Dictionary<EntityKey, BufferedEntity> _byEntityKey
            = new Dictionary<EntityKey, BufferedEntity>();

        private readonly IDictionary<object, List<BufferedEntity>> _byEntityInstance
            = new Dictionary<object, List<BufferedEntity>>();

        public QueryBuffer(
            [NotNull] StateManager stateManager,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource materializerSource,
            [NotNull] ClrCollectionAccessorSource clrCollectionAccessorSource,
            [NotNull] ClrPropertySetterSource clrPropertySetterSource)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(materializerSource, "materializerSource");
            Check.NotNull(clrCollectionAccessorSource, "clrCollectionAccessorSource");
            Check.NotNull(clrPropertySetterSource, "clrPropertySetterSource");

            _stateManager = stateManager;
            _entityKeyFactorySource = entityKeyFactorySource;
            _materializerSource = materializerSource;
            _clrCollectionAccessorSource = clrCollectionAccessorSource;
            _clrPropertySetterSource = clrPropertySetterSource;
        }

        public virtual object GetEntity(
            IEntityType entityType,
            EntityKey entityKey,
            IValueReader valueReader,
            Func<IValueReader, object> materializer,
            bool queryStateManager)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            if (entityKey == EntityKey.NullEntityKey)
            {
                return null;
            }

            if (queryStateManager)
            {
                var stateEntry = _stateManager.TryGetEntry(entityKey);

                if (stateEntry != null)
                {
                    return stateEntry.Entity;
                }
            }

            BufferedEntity bufferedEntity;
            if (!_byEntityKey.TryGetValue(entityKey, out bufferedEntity))
            {
                bufferedEntity
                    = new BufferedEntity(entityType, valueReader)
                        {
                            // TODO: Optimize this by not materializing when not required for query execution. i.e.
                            //       entity is only needed in final results
                            Instance = materializer(valueReader)
                        };

                _byEntityKey.Add(entityKey, bufferedEntity);
                _byEntityInstance.Add(bufferedEntity.Instance, new List<BufferedEntity> { bufferedEntity });
            }

            return bufferedEntity.Instance;
        }

        public virtual object GetPropertyValue(object entity, IProperty property)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(property, "property");

            var stateEntry = _stateManager.TryGetEntry(entity);

            return stateEntry != null
                ? stateEntry[property]
                : _byEntityInstance[entity][0].ValueReader
                    .ReadValue<object>(property.Index);
        }

        public virtual void StartTracking(object entity)
        {
            Check.NotNull(entity, "entity");

            List<BufferedEntity> bufferedEntities;
            if (_byEntityInstance.TryGetValue(entity, out bufferedEntities))
            {
                foreach (var bufferedEntity in bufferedEntities)
                {
                    bufferedEntity.StartTracking(_stateManager);
                }
            }
        }

        public virtual void Include(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>> relatedValueReaders)
        {
            Check.NotNull(navigationPath, "navigationPath");
            Check.NotNull(relatedValueReaders, "relatedValueReaders");

            Include(entity, navigationPath, relatedValueReaders, 0);
        }

        private void Include(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>> relatedValueReaders,
            int currentNavigationIndex)
        {
            Check.NotNull(navigationPath, "navigationPath");
            Check.NotNull(relatedValueReaders, "relatedValueReaders");

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
                relatedValueReaders[currentNavigationIndex](primaryKey, relatedKeyFactory)
                    .Select(valueReader =>
                        {
                            var targetEntity
                                = GetTargetEntity(
                                    targetEntityType,
                                    entityKeyFactory,
                                    keyProperties,
                                    valueReader,
                                    bufferedEntities);

                            Include(targetEntity, navigationPath, relatedValueReaders, currentNavigationIndex + 1);

                            return targetEntity;
                        })
                    .Where(e => e != null)
                    .ToList());
        }

        public virtual Task IncludeAsync(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<EntityKey, Func<IValueReader, EntityKey>, IAsyncEnumerable<IValueReader>>> relatedValueReaders,
            CancellationToken cancellationToken)
        {
            Check.NotNull(navigationPath, "navigationPath");
            Check.NotNull(relatedValueReaders, "relatedValueReaders");

            return IncludeAsync(entity, navigationPath, relatedValueReaders, cancellationToken, 0);
        }

        private async Task IncludeAsync(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<EntityKey, Func<IValueReader, EntityKey>, IAsyncEnumerable<IValueReader>>> relatedValueReaders,
            CancellationToken cancellationToken,
            int currentNavigationIndex)
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
                await relatedValueReaders[currentNavigationIndex](primaryKey, relatedKeyFactory)
                    .Select(async valueReader =>
                        {
                            var targetEntity
                                = GetTargetEntity(
                                    targetEntityType,
                                    entityKeyFactory,
                                    keyProperties,
                                    valueReader,
                                    bufferedEntities);

                            await IncludeAsync(
                                targetEntity,
                                navigationPath,
                                relatedValueReaders,
                                cancellationToken,
                                currentNavigationIndex + 1)
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
                    .GetKeyFactory(navigation.ForeignKey.ReferencedProperties);

            var foreignKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(navigation.ForeignKey.Properties);

            var targetEntityType = navigation.GetTargetType();

            if (!_byEntityInstance.TryGetValue(entity, out bufferedEntities))
            {
                _byEntityInstance.Add(entity, bufferedEntities = new List<BufferedEntity>());

                var stateEntry = _stateManager.TryGetEntry(entity);

                Debug.Assert(stateEntry != null);

                primaryKey
                    = navigation.PointsToPrincipal
                        ? stateEntry.GetDependentKeySnapshot(navigation.ForeignKey)
                        : stateEntry.GetPrimaryKeyValue();
            }
            else
            {
                primaryKey
                    = navigation.PointsToPrincipal
                        ? foreignKeyFactory
                            .Create(
                                targetEntityType,
                                navigation.ForeignKey.Properties,
                                bufferedEntities[0].ValueReader)
                        : primaryKeyFactory
                            .Create(
                                navigation.EntityType,
                                navigation.ForeignKey.ReferencedProperties,
                                bufferedEntities[0].ValueReader);
            }

            if (navigation.PointsToPrincipal)
            {
                relatedKeyFactory
                    = valueReader =>
                        primaryKeyFactory
                            .Create(
                                targetEntityType,
                                navigation.ForeignKey.ReferencedProperties,
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
            if (navigationPath[currentNavigationIndex].PointsToPrincipal
                && relatedEntities.Any())
            {
                _clrPropertySetterSource
                    .GetAccessor(navigationPath[currentNavigationIndex])
                    .SetClrValue(entity, relatedEntities[0]);

                var inverseNavigation = navigationPath[currentNavigationIndex].TryGetInverse();

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

                    var inverseNavigation = navigationPath[currentNavigationIndex].TryGetInverse();

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

                    var inverseNavigation = navigationPath[currentNavigationIndex].TryGetInverse();

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
            IValueReader valueReader,
            ICollection<BufferedEntity> bufferedEntities)
        {
            var entityKey
                = entityKeyFactory
                    .Create(targetEntityType, keyProperties, valueReader);

            var targetEntity
                = GetEntity(
                    targetEntityType,
                    entityKey,
                    valueReader,
                    _materializerSource.GetMaterializer(targetEntityType), // TODO: Flow materializer
                    queryStateManager: true);

            if (targetEntity != null)
            {
                List<BufferedEntity> bufferedTargetEntities;
                bufferedEntities.Add(
                    _byEntityInstance.TryGetValue(targetEntity, out bufferedTargetEntities)
                        ? bufferedTargetEntities[0]
                        : new BufferedEntity(targetEntityType, valueReader)
                            {
                                Instance = targetEntity
                            });
            }

            return targetEntity;
        }
    }
}

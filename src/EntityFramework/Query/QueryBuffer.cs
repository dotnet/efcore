// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
            private readonly IValueReader _valueReader; // TODO: This only works with buffering value readers

            public BufferedEntity(IEntityType entityType, IValueReader valueReader)
            {
                _entityType = entityType;
                _valueReader = valueReader;
            }

            public object Instance { get; set; }

            public IValueReader ValueReader
            {
                get { return _valueReader; }
            }

            public object StartTracking(StateManager stateManager)
            {
                // TODO: We are potentially materializing twice here, need another code path into the SM
                return stateManager.GetOrMaterializeEntry(_entityType, _valueReader).Entity;
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

        public virtual object GetEntity(IEntityType entityType, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            var keyProperties
                = entityType.GetPrimaryKey().Properties;

            var entityKey
                = _entityKeyFactorySource
                    .GetKeyFactory(keyProperties)
                    .Create(entityType, keyProperties, valueReader);

            var stateEntry = _stateManager.TryGetEntry(entityKey);

            if (stateEntry != null)
            {
                return stateEntry.Entity;
            }

            BufferedEntity bufferedEntity;
            if (!_byEntityKey.TryGetValue(entityKey, out bufferedEntity))
            {
                bufferedEntity
                    = new BufferedEntity(entityType, valueReader)
                        {
                            Instance = _materializerSource.GetMaterializer(entityType)(valueReader)
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
                : _byEntityInstance[entity][0].ValueReader.ReadValue<object>(property.Index);
        }

        public virtual object StartTracking(object entity)
        {
            Check.NotNull(entity, "entity");

            List<BufferedEntity> bufferedEntities;
            if (_byEntityInstance.TryGetValue(entity, out bufferedEntities))
            {
                var skip = 0;

                if (_stateManager.TryGetEntry(entity) == null)
                {
                    entity = bufferedEntities[0].StartTracking(_stateManager);

                    skip = 1;
                }

                foreach (var bufferedEntity in bufferedEntities.Skip(skip))
                {
                    bufferedEntity.StartTracking(_stateManager);
                }
            }

            return entity;
        }

        public virtual void Include(
            object entity,
            INavigation navigation,
            IEnumerable<IValueReader> relatedValueReaders)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(navigation, "navigation");
            Check.NotNull(relatedValueReaders, "relatedValueReaders");

            var primaryKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(navigation.ForeignKey.ReferencedProperties);

            var foreignKeyFactory
                = _entityKeyFactorySource
                    .GetKeyFactory(navigation.ForeignKey.Properties);

            EntityKey primaryKey;

            List<BufferedEntity> bufferedEntities;
            if (!_byEntityInstance.TryGetValue(entity, out bufferedEntities))
            {
                _byEntityInstance.Add(entity, bufferedEntities = new List<BufferedEntity>());

                var stateEntry = _stateManager.TryGetEntry(entity);

                Contract.Assert(stateEntry != null);

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
                                navigation.GetTargetType(),
                                navigation.ForeignKey.Properties,
                                bufferedEntities[0].ValueReader)
                        : primaryKeyFactory
                            .Create(
                                navigation.EntityType,
                                navigation.ForeignKey.ReferencedProperties,
                                bufferedEntities[0].ValueReader);
            }

            var relatedEntities
                = (navigation.PointsToPrincipal
                    ? relatedValueReaders
                        .Where(valueReader
                            => primaryKeyFactory
                                .Create(
                                    navigation.GetTargetType(),
                                    navigation.ForeignKey.ReferencedProperties,
                                    valueReader)
                                .Equals(primaryKey))
                    : relatedValueReaders
                        .Where(valueReader
                            => foreignKeyFactory
                                .Create(
                                    navigation.EntityType,
                                    navigation.ForeignKey.Properties,
                                    valueReader)
                                .Equals(primaryKey)))
                    .Select(valueReader =>
                        {
                            var targetEntityType = navigation.GetTargetType();
                            var targetEntity = GetEntity(targetEntityType, valueReader);

                            List<BufferedEntity> bufferedTargetEntities;
                            bufferedEntities.Add(
                                _byEntityInstance.TryGetValue(targetEntity, out bufferedTargetEntities)
                                    ? bufferedTargetEntities[0]
                                    : new BufferedEntity(targetEntityType, valueReader));

                            return targetEntity;
                        })
                    .ToList();

            if (navigation.PointsToPrincipal)
            {
                _clrPropertySetterSource
                    .GetAccessor(navigation)
                    .SetClrValue(entity, relatedEntities[0]);

                var inverseNavigation = navigation.TryGetInverse();

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
                if (navigation.IsCollection())
                {
                    _clrCollectionAccessorSource
                        .GetAccessor(navigation)
                        .AddRange(entity, relatedEntities);

                    var inverseNavigation = navigation.TryGetInverse();

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
                else
                {
                    _clrPropertySetterSource
                        .GetAccessor(navigation)
                        .SetClrValue(entity, relatedEntities[0]);

                    var inverseNavigation = navigation.TryGetInverse();

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

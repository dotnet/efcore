// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ForeignKeyValuePropagator
    {
        private readonly ClrPropertyGetterSource _getterSource;
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;
        private readonly DbContextService<ValueGeneratorCache> _valueGeneratorCache;
        private readonly DbContextService<DataStoreServices> _storeServices;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ForeignKeyValuePropagator()
        {
        }

        public ForeignKeyValuePropagator(
            [NotNull] ClrPropertyGetterSource getterSource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] DbContextService<ValueGeneratorCache> valueGeneratorCache,
            [NotNull] DbContextService<DataStoreServices> storeServices)
        {
            Check.NotNull(getterSource, "getterSource");
            Check.NotNull(collectionAccessorSource, "collectionAccessorSource");
            Check.NotNull(valueGeneratorCache, "valueGeneratorCache");
            Check.NotNull(storeServices, "storeServices");

            _getterSource = getterSource;
            _collectionAccessorSource = collectionAccessorSource;
            _valueGeneratorCache = valueGeneratorCache;
            _storeServices = storeServices;
        }

        public virtual void PropagateValue([NotNull] StateEntry stateEntry, [NotNull] IProperty property)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            Debug.Assert(property.IsForeignKey());

            if (!TryPropagateValue(stateEntry, property)
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(stateEntry, property);

                if (valueGenerator != null)
                {
                    stateEntry[property] = valueGenerator.Next(property, _storeServices).Value;
                }
            }
        }

        public virtual async Task PropagateValueAsync(
            [NotNull] StateEntry stateEntry,
            [NotNull] IProperty property,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            Debug.Assert(property.IsForeignKey());

            if (!TryPropagateValue(stateEntry, property)
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(stateEntry, property);

                if (valueGenerator != null)
                {
                    stateEntry[property] = 
                        (await valueGenerator.NextAsync(property, _storeServices, cancellationToken).WithCurrentCulture()).Value;
                }
            }
        }

        private bool TryPropagateValue(StateEntry stateEntry, IProperty property)
        {
            var entityType = property.EntityType;
            var stateManager = stateEntry.StateManager;

            foreach (var foreignKey in entityType.ForeignKeys)
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property == foreignKey.Properties[propertyIndex])
                    {
                        object valueToPropagte = null;

                        foreach (var navigation in entityType.Navigations
                            .Concat(foreignKey.ReferencedEntityType.Navigations)
                            .Where(n => n.ForeignKey == foreignKey)
                            .Distinct())
                        {
                            var principal = TryFindPrincipal(stateManager, navigation, stateEntry.Entity);

                            if (principal != null)
                            {
                                var principalEntry = stateManager.GetOrCreateEntry(principal);
                                var principalProperty = foreignKey.ReferencedProperties[propertyIndex];

                                var principalValue = principalEntry[principalProperty];
                                if (!principalProperty.PropertyType.IsDefaultValue(principalValue))
                                {
                                    valueToPropagte = principalValue;
                                    break;
                                }
                            }
                        }

                        if (valueToPropagte != null)
                        {
                            stateEntry[property] = valueToPropagte;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private IValueGenerator TryGetValueGenerator(StateEntry stateEntry, IProperty property)
        {
            foreach (var foreignKey in property.EntityType.ForeignKeys)
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property == foreignKey.Properties[propertyIndex]
                        && property.IsKey())
                    {
                        var generationProperty = foreignKey.FindRootValueGenerationProperty(propertyIndex);

                        if (generationProperty.GenerateValueOnAdd)
                        {
                            return _valueGeneratorCache.Service.GetGenerator(generationProperty);
                        }
                    }
                }
            }
            return null;
        }

        private object TryFindPrincipal(StateManager stateManager, INavigation navigation, object dependentEntity)
        {
            if (navigation.PointsToPrincipal)
            {
                return _getterSource.GetAccessor(navigation).GetClrValue(dependentEntity);
            }

            // TODO: Perf
            foreach (var principalEntry in stateManager.StateEntries
                .Where(e => e.EntityType == navigation.ForeignKey.ReferencedEntityType))
            {
                if (navigation.IsCollection())
                {
                    if (_collectionAccessorSource.GetAccessor(navigation).Contains(principalEntry.Entity, dependentEntity))
                    {
                        return principalEntry.Entity;
                    }
                }
                else if (_getterSource.GetAccessor(navigation).GetClrValue(principalEntry.Entity) == dependentEntity)
                {
                    return principalEntry.Entity;
                }
            }

            return null;
        }
    }
}

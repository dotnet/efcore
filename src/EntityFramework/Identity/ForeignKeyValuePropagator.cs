// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ForeignKeyValuePropagator
    {
        private readonly ClrPropertyGetterSource _getterSource;
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;

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
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource)
        {
            Check.NotNull(getterSource, "getterSource");
            Check.NotNull(collectionAccessorSource, "collectionAccessorSource");

            _getterSource = getterSource;
            _collectionAccessorSource = collectionAccessorSource;
        }

        public virtual void PropagateValue([NotNull] StateEntry stateEntry, [NotNull] IProperty property)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            Debug.Assert(property.IsForeignKey());

            var entityType = property.EntityType;
            var stateManager = stateEntry.StateManager;

            foreach (var foreignKey in entityType.ForeignKeys)
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property == foreignKey.Properties[propertyIndex])
                    {
                        foreach (var navigation in entityType.Navigations
                            .Concat(foreignKey.ReferencedEntityType.Navigations)
                            .Where(n => n.ForeignKey == foreignKey)
                            .Distinct())
                        {
                            var principal = TryFindPrincipal(stateManager, navigation, stateEntry.Entity);

                            if (principal != null)
                            {
                                var principalEntry = stateManager.GetOrCreateEntry(principal);

                                stateEntry[property] = principalEntry[foreignKey.ReferencedProperties[propertyIndex]];
                            }
                        }
                    }
                }
            }
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

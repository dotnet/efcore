// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class KeyPropagator : IKeyPropagator
    {
        private readonly IClrAccessorSource<IClrPropertyGetter> _getterSource;
        private readonly IClrCollectionAccessorSource _collectionAccessorSource;
        private readonly IValueGeneratorSelector _valueGeneratorSelector;

        public KeyPropagator(
            [NotNull] IClrAccessorSource<IClrPropertyGetter> getterSource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IValueGeneratorSelector valueGeneratorSelector)
        {
            _getterSource = getterSource;
            _collectionAccessorSource = collectionAccessorSource;
            _valueGeneratorSelector = valueGeneratorSelector;
        }

        public virtual void PropagateValue(InternalEntityEntry entry, IProperty property)
        {
            Debug.Assert(property.IsForeignKey());

            if (!TryPropagateValue(entry, property)
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(property);

                if (valueGenerator != null)
                {
                    entry[property] = valueGenerator.Next();
                }
            }
        }

        private bool TryPropagateValue(InternalEntityEntry entry, IProperty property)
        {
            var entityType = property.EntityType;
            var stateManager = entry.StateManager;

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property == foreignKey.Properties[propertyIndex])
                    {
                        object valueToPropagte = null;

                        foreach (var navigation in entityType.GetNavigations()
                            .Concat(foreignKey.PrincipalEntityType.GetNavigations())
                            .Where(n => n.ForeignKey == foreignKey)
                            .Distinct())
                        {
                            var principal = TryFindPrincipal(stateManager, navigation, entry.Entity);

                            if (principal != null)
                            {
                                var principalEntry = stateManager.GetOrCreateEntry(principal);
                                var principalProperty = foreignKey.PrincipalKey.Properties[propertyIndex];

                                var principalValue = principalEntry[principalProperty];
                                if (!principalProperty.IsSentinelValue(principalValue))
                                {
                                    valueToPropagte = principalValue;
                                    break;
                                }
                            }
                        }

                        if (valueToPropagte != null)
                        {
                            entry[property] = valueToPropagte;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private ValueGenerator TryGetValueGenerator(IProperty property)
        {
            var generationProperty = property.GetGenerationProperty();

            if (generationProperty != null)
            {
                return _valueGeneratorSelector.Select(generationProperty, generationProperty.EntityType);
            }

            return null;
        }

        private object TryFindPrincipal(IStateManager stateManager, INavigation navigation, object dependentEntity)
        {
            if (navigation.PointsToPrincipal())
            {
                return _getterSource.GetAccessor(navigation).GetClrValue(dependentEntity);
            }

            // TODO: Perf
            foreach (var principalEntry in stateManager.Entries
                .Where(e => e.EntityType == navigation.ForeignKey.PrincipalEntityType))
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

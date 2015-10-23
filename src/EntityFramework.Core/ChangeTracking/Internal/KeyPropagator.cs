// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class KeyPropagator : IKeyPropagator
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;

        public KeyPropagator(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector)
        {
            _valueGeneratorSelector = valueGeneratorSelector;
        }

        public virtual void PropagateValue(InternalEntityEntry entry, IProperty property)
        {
            Debug.Assert(property.IsForeignKey(entry.EntityType));

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
            var entityType = entry.EntityType;
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
                                if (!principalProperty.ClrType.IsDefaultValue(principalValue))
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
                return _valueGeneratorSelector.Select(generationProperty, generationProperty.DeclaringEntityType);
            }

            return null;
        }

        private object TryFindPrincipal(IStateManager stateManager, INavigation navigation, object dependentEntity)
        {
            if (navigation.IsDependentToPrincipal())
            {
                return navigation.GetGetter().GetClrValue(dependentEntity);
            }

            // TODO: Perf
            foreach (var principalEntry in stateManager.Entries
                .Where(e => navigation.ForeignKey.PrincipalEntityType.IsAssignableFrom(e.EntityType)))
            {
                if (navigation.IsCollection())
                {
                    if (navigation.GetCollectionAccessor().Contains(principalEntry.Entity, dependentEntity))
                    {
                        return principalEntry.Entity;
                    }
                }
                else if (navigation.GetGetter().GetClrValue(principalEntry.Entity) == dependentEntity)
                {
                    return principalEntry.Entity;
                }
            }

            return null;
        }
    }
}

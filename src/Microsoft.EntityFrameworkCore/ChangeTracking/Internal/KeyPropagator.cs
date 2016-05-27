// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
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
            Debug.Assert(property.IsForeignKey());

            if (!TryPropagateValue(entry, property)
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(property);

                if (valueGenerator != null)
                {
                    entry[property] = valueGenerator.Next(new EntityEntry(entry));
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
                        InternalEntityEntry principalEntry = null;
                        var principal = foreignKey.DependentToPrincipal?.GetGetter().GetClrValue(entry.Entity);
                        if (principal != null)
                        {
                            principalEntry = stateManager.GetOrCreateEntry(principal);
                        }
                        else if (foreignKey.PrincipalToDependent != null)
                        {
                            foreach (var danglerEntry in stateManager.GetRecordedReferers(entry.Entity, clear: false))
                            {
                                if (danglerEntry.Item1 == foreignKey.PrincipalToDependent)
                                {
                                    principalEntry = danglerEntry.Item2;
                                    break;
                                }
                            }
                        }

                        if (principalEntry != null)
                        {
                            var principalProperty = foreignKey.PrincipalKey.Properties[propertyIndex];
                            var principalValue = principalEntry[principalProperty];
                            if (!principalProperty.ClrType.IsDefaultValue(principalValue))
                            {
                                entry[property] = principalValue;
                                return true;
                            }
                        }

                        break;
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
    }
}

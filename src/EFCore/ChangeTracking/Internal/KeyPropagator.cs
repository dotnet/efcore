// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class KeyPropagator : IKeyPropagator
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public KeyPropagator(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector)
        {
            _valueGeneratorSelector = valueGeneratorSelector;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry PropagateValue(InternalEntityEntry entry, IProperty property)
        {
            Debug.Assert(property.IsForeignKey());

            var principalEntry = TryPropagateValue(entry, property);
            if (principalEntry == null
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(property);

                if (valueGenerator != null)
                {
                    var value = valueGenerator.Next(new EntityEntry(entry));

                    if (valueGenerator.GeneratesTemporaryValues)
                    {
                        entry.SetTemporaryValue(property, value);
                    }
                    else
                    {
                        entry[property] = value;
                    }
                }
            }

            return principalEntry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task<InternalEntityEntry> PropagateValueAsync(
            InternalEntityEntry entry,
            IProperty property,
            CancellationToken cancellationToken = default)
        {
            Debug.Assert(property.IsForeignKey());

            var principalEntry = TryPropagateValue(entry, property);
            if (principalEntry == null
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(property);

                if (valueGenerator != null)
                {
                    var value = await valueGenerator.NextAsync(new EntityEntry(entry), cancellationToken);

                    if (valueGenerator.GeneratesTemporaryValues)
                    {
                        entry.SetTemporaryValue(property, value);
                    }
                    else
                    {
                        entry[property] = value;
                    }
                }
            }

            return principalEntry;
        }

        private static InternalEntityEntry TryPropagateValue(InternalEntityEntry entry, IProperty property)
        {
            var entityType = entry.EntityType;
            var stateManager = entry.StateManager;

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property == foreignKey.Properties[propertyIndex])
                    {
                        var principal = foreignKey.DependentToPrincipal == null
                            ? null
                            : entry[foreignKey.DependentToPrincipal];
                        InternalEntityEntry principalEntry = null;
                        if (principal != null)
                        {
                            principalEntry = stateManager.GetOrCreateEntry(principal);
                        }
                        else if (foreignKey.PrincipalToDependent != null)
                        {
                            foreach (var danglerEntry in stateManager.GetRecordedReferrers(entry.Entity, clear: false))
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
                                if (principalEntry.HasTemporaryValue(principalProperty))
                                {
                                    entry.SetTemporaryValue(property, principalValue);
                                }
                                else
                                {
                                    entry[property] = principalValue;
                                }

                                return principalEntry;
                            }
                        }

                        break;
                    }
                }
            }

            return null;
        }

        private ValueGenerator TryGetValueGenerator(IProperty property)
        {
            var generationProperty = property.GetGenerationProperty();

            return generationProperty != null ? _valueGeneratorSelector.Select(generationProperty, generationProperty.DeclaringEntityType) : null;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class KeyPropagator : IKeyPropagator
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public KeyPropagator(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector)
        {
            _valueGeneratorSelector = valueGeneratorSelector;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry PropagateValue(InternalEntityEntry entry, IProperty property)
        {
            Check.DebugAssert(property.IsForeignKey(), $"property {property} is not part of an FK");

            var principalEntry = TryPropagateValue(entry, property);
            if (principalEntry == null
                && property.IsKey()
                && !property.IsForeignKeyToSelf())
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

                    entry.MarkUnknown(property);
                }
            }

            return principalEntry;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<InternalEntityEntry> PropagateValueAsync(
            InternalEntityEntry entry,
            IProperty property,
            CancellationToken cancellationToken = default)
        {
            Check.DebugAssert(property.IsForeignKey(), $"property {property} is not part of an FK");

            var principalEntry = TryPropagateValue(entry, property);
            if (principalEntry == null
                && property.IsKey())
            {
                var valueGenerator = TryGetValueGenerator(property);

                if (valueGenerator != null)
                {
                    var value = await valueGenerator.NextAsync(new EntityEntry(entry), cancellationToken)
                        .ConfigureAwait(false);

                    if (valueGenerator.GeneratesTemporaryValues)
                    {
                        entry.SetTemporaryValue(property, value);
                    }
                    else
                    {
                        entry[property] = value;
                    }

                    entry.MarkUnknown(property);
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
                            principalEntry = stateManager.GetOrCreateEntry(principal, foreignKey.PrincipalEntityType);
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

                            if (principalProperty != property)
                            {
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

            return generationProperty != null
                ? _valueGeneratorSelector.Select(generationProperty, generationProperty.DeclaringEntityType)
                : null;
        }
    }
}

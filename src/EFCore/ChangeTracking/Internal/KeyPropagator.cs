// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        IValueGeneratorSelector valueGeneratorSelector)
    {
        _valueGeneratorSelector = valueGeneratorSelector;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? PropagateValue(InternalEntityEntry entry, IProperty property)
    {
        Check.DebugAssert(property.IsForeignKey(), $"property {property} is not part of an FK");

        var generationProperty = property.FindGenerationProperty();
        var principalEntry = TryPropagateValue(entry, property, generationProperty);

        if (principalEntry == null
            && property.IsKey()
            && !property.IsForeignKeyToSelf())
        {
            var valueGenerator = TryGetValueGenerator(
                generationProperty,
                generationProperty == property
                    ? entry.EntityType
                    : generationProperty?.DeclaringType);

            if (valueGenerator != null)
            {
                SetValue(entry, property, valueGenerator, valueGenerator.Next(new EntityEntry(entry)));
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
    public virtual async Task<InternalEntityEntry?> PropagateValueAsync(
        InternalEntityEntry entry,
        IProperty property,
        CancellationToken cancellationToken = default)
    {
        Check.DebugAssert(property.IsForeignKey(), $"property {property} is not part of an FK");

        var generationProperty = property.FindGenerationProperty();
        var principalEntry = TryPropagateValue(entry, property, generationProperty);

        if (principalEntry == null
            && property.IsKey()
            && !property.IsForeignKeyToSelf())
        {
            var valueGenerator = TryGetValueGenerator(
                generationProperty,
                generationProperty == property
                    ? entry.EntityType
                    : generationProperty?.DeclaringType);

            if (valueGenerator != null)
            {
                SetValue(
                    entry,
                    property,
                    valueGenerator,
                    await valueGenerator.NextAsync(new EntityEntry(entry), cancellationToken).ConfigureAwait(false));
            }
        }

        return principalEntry;
    }

    private static void SetValue(InternalEntityEntry entry, IProperty property, ValueGenerator valueGenerator, object? value)
    {
        if (valueGenerator.GeneratesStableValues)
        {
            entry[property] = value;
        }
        else
        {
            entry.SetTemporaryValue(property, value);
            entry.MarkUnknown(property);
        }
    }

    private static InternalEntityEntry? TryPropagateValue(InternalEntityEntry entry, IProperty property, IProperty? generationProperty)
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
                    InternalEntityEntry? principalEntry = null;
                    if (principal != null)
                    {
                        principalEntry = stateManager.GetOrCreateEntry(principal, foreignKey.PrincipalEntityType);
                    }
                    else if (foreignKey.PrincipalToDependent != null)
                    {
                        foreach (var (navigationBase, internalEntityEntry) in stateManager.GetRecordedReferrers(entry.Entity, clear: false))
                        {
                            if (navigationBase == foreignKey.PrincipalToDependent)
                            {
                                principalEntry = internalEntityEntry;
                                break;
                            }
                        }
                    }

                    if (principalEntry != null)
                    {
                        var principalProperty = foreignKey.PrincipalKey.Properties[propertyIndex];

                        if (principalProperty != property)
                        {
                            if (generationProperty == null
                                || principalEntry.HasExplicitValue(principalProperty))
                            {
                                entry.PropagateValue(principalEntry, principalProperty, property);

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

    private ValueGenerator? TryGetValueGenerator(IProperty? generationProperty, ITypeBase? typeBase)
    {
        if (generationProperty == null)
        {
            return null;
        }

        if (!_valueGeneratorSelector.TrySelect(generationProperty, typeBase!, out var valueGenerator))
        {
            throw new NotSupportedException(
                CoreStrings.NoValueGenerator(
                    generationProperty.Name, generationProperty.DeclaringType.DisplayName(),
                    generationProperty.ClrType.ShortDisplayName()));
        }

        return valueGenerator!;
    }
}

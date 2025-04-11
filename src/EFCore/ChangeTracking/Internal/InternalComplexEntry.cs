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
public sealed class InternalComplexEntry : InternalEntryBase
{
    private readonly int[] _ordinals;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalComplexEntry(
        IRuntimeComplexType complexType,
        InternalEntryBase containingEntry,
        int ordinal)
        : base(complexType)
    {
        Check.DebugAssert(complexType.ComplexProperty.IsCollection, $"{complexType} expected to be a collection");

        ContainingEntry = containingEntry;

        var parentOrdinals = containingEntry.Ordinals;
        _ordinals = new int[parentOrdinals.Length + 1];
        parentOrdinals.CopyTo(_ordinals);
        _ordinals[parentOrdinals.Length] = ordinal;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IInternalEntry ContainingEntry { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override InternalEntityEntry EntityEntry
        => ContainingEntry switch
        {
            InternalEntityEntry entityEntry => entityEntry,
            InternalComplexEntry complexEntry => complexEntry.EntityEntry,
            _ => throw new UnreachableException("Unexpected entry type: " + ContainingEntry.GetType().ShortDisplayName())
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IComplexProperty ComplexProperty => ComplexType.ComplexProperty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IStateManager StateManager => ContainingEntry.StateManager;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Ordinal => _ordinals[^1];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ReadOnlySpan<int> Ordinals => _ordinals;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IRuntimeComplexType ComplexType => (IRuntimeComplexType)StructuralType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OnStateChanged(EntityState oldState)
    {
        if (oldState is EntityState.Detached or EntityState.Unchanged)
        {
            if (EntityState is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            {
                ContainingEntry.OnComplexPropertyModified(ComplexProperty, isModified: true);
            }
        }
        else if (EntityState is EntityState.Detached or EntityState.Unchanged)
        {
            ContainingEntry.OnComplexPropertyModified(ComplexProperty, isModified: false);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string GetPropertyPath(IReadOnlyProperty property)
    {
        Check.DebugAssert(property.DeclaringType == StructuralType || property.DeclaringType.ContainingType == StructuralType,
            "Property " + property.Name + " not contained under " + StructuralType.Name);

        return GetPropertyPath() + "." + GetShortNameChain(property.DeclaringType) + property.Name;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string GetPropertyPath(bool withElement = true)
    {
        var result = ContainingEntry is InternalComplexEntry containingEntry
            ? containingEntry.GetPropertyPath() + "."
            : "";

        result += GetShortNameChain(ComplexType.ComplexProperty.DeclaringType) + ComplexProperty.Name;

        if (withElement)
        {
            result += '[' + Ordinal + ']';
        }

        return result;
    }

    private static string GetShortNameChain(IReadOnlyTypeBase structuralType)
        => (structuralType is IReadOnlyComplexType complexType) && (complexType.ComplexProperty is IReadOnlyComplexProperty complexProperty)
            ? complexProperty.IsCollection
                ? ""
                : GetShortNameChain(complexProperty.DeclaringType) + "." + complexProperty.Name + "."
            : structuralType.ShortName() + ".";
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
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
    private int _ordinal;
    private int _originalOrdinal;

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
        _ordinal = ordinal;
        OriginalOrdinal = ordinal;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override InternalEntryBase ContainingEntry { get; }

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
    public override IReadOnlyList<int> GetOrdinals()
    {
        var parentOrdinals = ContainingEntry.GetOrdinals();
        var result = new List<int>(parentOrdinals.Count + 1);
        result.AddRange(parentOrdinals);
        result.Add(Ordinal);
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Ordinal
    {
        get => _ordinal;
        set
        {
            if (EntityState is not EntityState.Detached and not EntityState.Deleted
                && ContainingEntry.EntityState is not EntityState.Detached and not EntityState.Deleted
                && _ordinal != value)
            {
                var existingEntry = ContainingEntry.GetComplexCollectionEntry(ComplexProperty, value);
                if (existingEntry != this
                    && existingEntry.EntityState is not EntityState.Detached and not EntityState.Deleted
                    && existingEntry.Ordinal != -1)
                {
                    throw new InvalidOperationException(CoreStrings.ComplexCollectionEntryOrdinalReadOnly(
                        ComplexProperty.DeclaringType.ShortNameChain(), ComplexProperty.Name));
                }
            }

            _ordinal = value;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>

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
    public int OriginalOrdinal
    {
        get => _originalOrdinal;
        set
        {
            if (EntityState is not EntityState.Detached and not EntityState.Added
                && ContainingEntry.EntityState is not EntityState.Detached and not EntityState.Added
                && _originalOrdinal != value)
            {
                var existingEntry = ContainingEntry.GetComplexCollectionOriginalEntry(ComplexProperty, value);
                if (existingEntry != this
                    && existingEntry.EntityState is not EntityState.Detached and not EntityState.Added
                    && existingEntry.OriginalOrdinal != -1)
                {
                    throw new InvalidOperationException(CoreStrings.ComplexCollectionEntryOriginalOrdinalReadOnly(
                        ComplexProperty.DeclaringType.ShortNameChain(), ComplexProperty.Name));
                }
            }

            _originalOrdinal = value;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object? ReadPropertyValue(IPropertyBase propertyBase)
        => EntityState == EntityState.Deleted
        ? GetOriginalValue(propertyBase)
        : base.ReadPropertyValue(propertyBase);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override T ReadOriginalValue<T>(IProperty property, int originalValueIndex)
        => EntityState == EntityState.Added
        ? GetCurrentValue<T>(property)
        : base.ReadOriginalValue<T>(property, originalValueIndex);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object? GetOriginalValue(IPropertyBase propertyBase)
        => EntityState == EntityState.Added
        ? GetCurrentValue(propertyBase)
        : base.GetOriginalValue(propertyBase);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void SetEntityState(EntityState oldState, EntityState newState, bool acceptChanges, bool modifyProperties)
    {
        if (oldState is EntityState.Detached or EntityState.Deleted
            && newState is not EntityState.Detached and not EntityState.Deleted)
        {
            ContainingEntry.ValidateOrdinal(this, original: false);
        }

        if (oldState is EntityState.Detached or EntityState.Added
            && newState is not EntityState.Detached and not EntityState.Added)
        {
            ContainingEntry.ValidateOrdinal(this, original: true);
        }

        if((oldState is EntityState.Deleted && newState is EntityState.Added)
            || (oldState is EntityState.Added && newState is EntityState.Deleted))
        {
            throw new InvalidOperationException($"Cannot change the state of an element of the `{ComplexProperty.Name}` complex collection directly from deleted to added or vice versa. First mark it as Unchanged");
        }

        base.SetEntityState(oldState, newState, acceptChanges, modifyProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OnStateChanging(EntityState newState)
    {
        base.OnStateChanging(newState);

        if ((EntityState is EntityState.Detached && newState != EntityState.Deleted)
            || (EntityState is EntityState.Deleted && newState != EntityState.Detached))
        {
            StateManager.StartTracking(this);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OnStateChanged(EntityState oldState)
    {
        if ((EntityState == EntityState.Detached && oldState != EntityState.Deleted)
            || (EntityState == EntityState.Deleted && oldState != EntityState.Detached))
        {
            StateManager.StopTracking(this, oldState);
        }

        base.OnStateChanged(oldState);

        ContainingEntry.OnComplexCollectionElementStateChange(this, oldState, EntityState);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AcceptChanges()
    {
        if (EntityState == EntityState.Added)
        {
            OriginalOrdinal = Ordinal;
        }

        base.AcceptChanges();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string GetPropertyPath(IReadOnlyProperty property)
    {
        Check.DebugAssert(property.DeclaringType == StructuralType
            || property.DeclaringType.ContainingType == StructuralType
            || StructuralType.ClrType == typeof(object), // For testing
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
            result += "[" + Ordinal + "]";
        }

        return result;
    }

    private static string GetShortNameChain(IReadOnlyTypeBase structuralType)
        => (structuralType is IReadOnlyComplexType complexType) && (complexType.ComplexProperty is IReadOnlyComplexProperty complexProperty)
            ? complexProperty.IsCollection
                ? ""
                : GetShortNameChain(complexProperty.DeclaringType) + "." + complexProperty.Name + "."
            : structuralType.ShortName() + ".";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DebugView DebugView
        => new(
            () => ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault),
            () => ToDebugString());


    private string ToDebugString(
        ChangeTrackerDebugStringOptions options = ChangeTrackerDebugStringOptions.LongDefault,
        int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder
                .Append(GetPropertyPath())
                .Append(' ')
                .Append(EntityState.ToString());

            if ((options & ChangeTrackerDebugStringOptions.IncludeProperties) != 0)
            {
                DumpProperties(StructuralType, indent + 2);

                void DumpProperties(ITypeBase structuralType, int tempIndent)
                {
                    var tempIndentString = new string(' ', tempIndent);
                    foreach (var property in structuralType.GetProperties())
                    {
                        builder.AppendLine().Append(tempIndentString);

                        var currentValue = GetCurrentValue(property);
                        builder
                            .Append("  ")
                            .Append(property.Name)
                            .Append(": ");

                        AppendValue(currentValue);

                        if (property.IsPrimaryKey())
                        {
                            builder.Append(" PK");
                        }
                        else if (property.IsKey())
                        {
                            builder.Append(" AK");
                        }

                        if (property.IsForeignKey())
                        {
                            builder.Append(" FK");
                        }

                        if (IsModified(property))
                        {
                            builder.Append(" Modified");
                        }

                        if (HasTemporaryValue(property))
                        {
                            builder.Append(" Temporary");
                        }

                        if (IsUnknown(property))
                        {
                            builder.Append(" Unknown");
                        }

                        if (HasOriginalValuesSnapshot
                            && property.GetOriginalValueIndex() != -1)
                        {
                            var originalValue = GetOriginalValue(property);
                            if (!Equals(originalValue, currentValue))
                            {
                                builder.Append(" Originally ");
                                AppendValue(originalValue);
                            }
                        }
                    }

                    foreach (var complexProperty in structuralType.GetComplexProperties())
                    {
                        builder.AppendLine().Append(tempIndentString);

                        builder
                            .Append("  ")
                            .Append(complexProperty.Name)
                            .Append(" (Complex: ")
                            .Append(complexProperty.ClrType.ShortDisplayName())
                            .Append(")");

                        DumpProperties(complexProperty.ComplexType, tempIndent + 2);
                    }
                }
            }

            void AppendValue(object? value)
            {
                if (value == null)
                {
                    builder.Append("<null>");
                }
                else if (value.GetType().IsNumeric())
                {
                    builder.Append(value);
                }
                else if (value is byte[] bytes)
                {
                    builder.AppendBytes(bytes);
                }
                else
                {
                    var stringValue = value.ToString();
                    if (stringValue?.Length > 63)
                    {
                        stringValue = string.Concat(stringValue.AsSpan(0, 60), "...");
                    }

                    builder
                        .Append('\'')
                        .Append(stringValue)
                        .Append('\'');
                }
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}

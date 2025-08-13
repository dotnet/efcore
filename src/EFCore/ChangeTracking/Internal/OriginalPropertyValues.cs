// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class OriginalPropertyValues : EntryPropertyValues
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public OriginalPropertyValues(InternalEntryBase internalEntry)
        : base(internalEntry)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TValue GetValue<TValue>(string propertyName)
        => InternalEntry.GetOriginalValue<TValue>(InternalEntry.StructuralType.GetProperty(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TValue GetValue<TValue>(IProperty property)
        => InternalEntry.GetOriginalValue<TValue>(InternalEntry.StructuralType.CheckContains(property));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void SetValueInternal(IInternalEntry entry, IPropertyBase property, object? value)
        => entry.SetOriginalValue(property, value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override object? GetValueInternal(IInternalEntry entry, IPropertyBase property)
    {
        var originalValue = entry.GetOriginalValue(property);
        if (property is IComplexProperty { IsCollection: true } complexProperty)
        {
            var originalCollection = (IList?)originalValue;
            if (originalCollection == null)
            {
                return null;
            }

            // The stored original collection might contain references to the current elements,
            // so we need to recreate it using stored values.
            var clonedCollection = (IList)((IRuntimePropertyBase)complexProperty).GetIndexedCollectionAccessor()
                .Create(originalCollection.Count);
            for (var i = 0; i < originalCollection.Count; i++)
            {
                clonedCollection.Add(
                    originalCollection[i] == null
                        ? null
                        : GetPropertyValues(entry.GetComplexCollectionOriginalEntry(complexProperty, i)).ToObject());
            }

            return clonedCollection;
        }

        return originalValue;
    }

    private PropertyValues GetPropertyValues(InternalEntryBase entry)
    {
        var structuralType = entry.StructuralType;
        var properties = structuralType.GetFlattenedProperties().AsList();
        var values = new object?[properties.Count];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = entry.GetOriginalValue(properties[i]);
        }

        var cloned = new ArrayPropertyValues(entry, values);

        foreach (var nestedComplexProperty in cloned.ComplexCollectionProperties)
        {
            var collection = (IList?)GetValueInternal(entry, nestedComplexProperty);
            if (collection != null)
            {
                cloned[nestedComplexProperty] = collection;
            }
        }

        return cloned;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override InternalComplexEntry GetComplexCollectionEntry(InternalEntryBase entry, IComplexProperty complexProperty, int i)
        => entry.GetComplexCollectionOriginalEntry(complexProperty, i);
}

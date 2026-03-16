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
    protected override void SetValueInternal(IInternalEntry entry, IPropertyBase property, object? value, bool skipChangeDetection = false)
        => entry.SetOriginalValue(property, value, skipChangeDetection: skipChangeDetection);

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

            // The stored original collection contains references to the current CLR elements
            // (see SnapshotComplexCollection), so we must reconstruct each element from the
            // per-entry original value snapshots to get true original values.
            var reconstructed = (IList)((IRuntimePropertyBase)complexProperty).GetIndexedCollectionAccessor()
                .Create(originalCollection.Count);
            for (var i = 0; i < originalCollection.Count; i++)
            {
                var element = originalCollection[i];
                if (element == null)
                {
                    reconstructed.Add(null);
                    continue;
                }

                var complexEntry = entry.GetComplexCollectionOriginalEntry(complexProperty, i);
                if (!complexEntry.HasOriginalValuesSnapshot)
                {
                    complexEntry.EnsureOriginalValues();
                    SetValuesFromInstance(complexEntry, (IRuntimeTypeBase)complexProperty.ComplexType, element, skipChangeDetection: true);
                }

                reconstructed.Add(new OriginalPropertyValues(complexEntry).Clone().ToObject());
            }

            return reconstructed;
        }

        return originalValue;
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

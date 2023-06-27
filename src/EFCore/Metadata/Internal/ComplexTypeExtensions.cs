// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleNamedExpression
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ComplexTypeExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool UseEagerSnapshots(this IReadOnlyComplexType complexType)
    {
        var changeTrackingStrategy = complexType.GetChangeTrackingStrategy();

        return changeTrackingStrategy == ChangeTrackingStrategy.Snapshot
            || changeTrackingStrategy == ChangeTrackingStrategy.ChangedNotifications;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PropertyCounts CalculateCounts(this IRuntimeComplexType complexType)
    {
        var propertyIndex = 0;
        var complexPropertyIndex = 0;
        var originalValueIndex = 0;
        var shadowIndex = 0;
        var storeGenerationIndex = 0;
        var relationshipIndex = ((IRuntimeTypeBase)complexType.ComplexProperty.DeclaringType).Counts.RelationshipCount;

        var baseCounts = (complexType as ComplexType)?.BaseType?.Counts;
        if (baseCounts != null)
        {
            propertyIndex = baseCounts.PropertyCount;
            originalValueIndex = baseCounts.OriginalValueCount;
            shadowIndex = baseCounts.ShadowCount;
            storeGenerationIndex = baseCounts.StoreGeneratedCount;
        }

        foreach (var property in complexType.GetProperties())
        {
            var indexes = new PropertyIndexes(
                index: propertyIndex++,
                originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                shadowIndex: property.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: property.IsKey() || property.IsForeignKey() ? relationshipIndex++ : -1,
                storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

            ((IRuntimePropertyBase)property).PropertyIndexes = indexes;
        }

        foreach (var complexProperty in complexType.GetComplexProperties())
        {
            var indexes = new PropertyIndexes(
                index: complexPropertyIndex++,
                originalValueIndex: -1,
                shadowIndex: complexProperty.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: -1,
                storeGenerationIndex: -1);

            ((IRuntimePropertyBase)complexProperty).PropertyIndexes = indexes;
        }

        return new PropertyCounts(
            propertyIndex,
            navigationCount: 0,
            complexPropertyIndex,
            originalValueIndex,
            shadowIndex,
            relationshipCount: 0,
            storeGenerationIndex);
    }
}

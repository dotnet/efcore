// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class PropertyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool ForAdd(this ValueGenerated valueGenerated)
        => (valueGenerated & ValueGenerated.OnAdd) != 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool ForUpdate(this ValueGenerated valueGenerated)
        => (valueGenerated & ValueGenerated.OnUpdate) != 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyProperty? FindFirstDifferentPrincipal(this IReadOnlyProperty property)
    {
        var principal = property.FindFirstPrincipal();

        return principal != property ? principal : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IProperty? FindGenerationProperty(this IProperty property)
    {
        var traversalList = new List<IProperty> { property };

        var index = 0;
        while (index < traversalList.Count)
        {
            var currentProperty = traversalList[index];

            if (currentProperty.RequiresValueGenerator())
            {
                return currentProperty;
            }

            foreach (var foreignKey in currentProperty.GetContainingForeignKeys())
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (currentProperty == foreignKey.Properties[propertyIndex])
                    {
                        var nextProperty = foreignKey.PrincipalKey.Properties[propertyIndex];
                        if (!traversalList.Contains(nextProperty))
                        {
                            traversalList.Add(nextProperty);
                        }
                    }
                }
            }

            index++;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool RequiresValueGenerator(this IReadOnlyProperty property)
        => (property.ValueGenerated.ForAdd()
                && property.IsKey()
                && (!property.IsForeignKey()
                    || property.IsForeignKeyToSelf()
                    || (property.GetContainingForeignKeys().All(fk => fk.Properties.Any(p => p != property && p.IsNullable)))))
            || property.GetValueGeneratorFactory() != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsForeignKeyToSelf(this IReadOnlyProperty property)
    {
        Check.DebugAssert(property.IsKey(), "Only call this method for properties known to be part of a key.");

        foreach (var foreignKey in property.GetContainingForeignKeys())
        {
            var propertyIndex = foreignKey.Properties.IndexOf(property);
            if (propertyIndex == foreignKey.PrincipalKey.Properties.IndexOf(property))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsKey(this Property property)
        => property.Keys != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool MayBeStoreGenerated(this IProperty property)
    {
        if (property.ValueGenerated != ValueGenerated.Never
            || property.IsForeignKey())
        {
            return true;
        }

        if (property.IsKey())
        {
            var generationProperty = property.FindGenerationProperty();
            return (generationProperty != null)
                && (generationProperty.ValueGenerated != ValueGenerated.Never);
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool RequiresOriginalValue(this IReadOnlyProperty property)
        => property.DeclaringType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.ChangingAndChangedNotifications
            || property.IsConcurrencyToken
            || property.IsKey()
            || property.IsForeignKey()
            || property.IsUniqueIndex();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool RequiresOriginalValue(this IReadOnlyComplexProperty property)
        => property.ComplexType.ContainingEntityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.ChangingAndChangedNotifications;
}

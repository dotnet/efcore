// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ForeignKeyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsSelfReferencing(this IReadOnlyForeignKey foreignKey)
        => foreignKey.DeclaringEntityType == foreignKey.PrincipalEntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyNavigation> GetNavigations(this IReadOnlyForeignKey foreignKey)
    {
        if (foreignKey.PrincipalToDependent != null)
        {
            yield return foreignKey.PrincipalToDependent;
        }

        if (foreignKey.DependentToPrincipal != null)
        {
            yield return foreignKey.DependentToPrincipal;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyNavigation> FindNavigationsFrom(
        this IReadOnlyForeignKey foreignKey,
        IReadOnlyEntityType entityType)
    {
        if (foreignKey.DeclaringEntityType != entityType
            && foreignKey.PrincipalEntityType != entityType)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
        }

        return foreignKey.IsSelfReferencing()
            ? foreignKey.GetNavigations()
            : foreignKey.FindNavigations(foreignKey.DeclaringEntityType == entityType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyNavigation> FindNavigationsFromInHierarchy(
        this IReadOnlyForeignKey foreignKey,
        IReadOnlyEntityType entityType)
    {
        if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
            && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
        }

        return foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
            || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.DeclaringEntityType.IsAssignableFrom(entityType));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyNavigation> FindNavigationsTo(
        this IReadOnlyForeignKey foreignKey,
        IReadOnlyEntityType entityType)
    {
        if (foreignKey.DeclaringEntityType != entityType
            && foreignKey.PrincipalEntityType != entityType)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
        }

        return foreignKey.IsSelfReferencing()
            ? foreignKey.GetNavigations()
            : foreignKey.FindNavigations(foreignKey.PrincipalEntityType == entityType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyNavigation> FindNavigationsToInHierarchy(
        this IReadOnlyForeignKey foreignKey,
        IReadOnlyEntityType entityType)
    {
        if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
            && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
        }

        return foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
            || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.PrincipalEntityType.IsAssignableFrom(entityType));
    }

    private static IEnumerable<IReadOnlyNavigation> FindNavigations(
        this IReadOnlyForeignKey foreignKey,
        bool toPrincipal)
    {
        if (toPrincipal)
        {
            if (foreignKey.DependentToPrincipal != null)
            {
                yield return foreignKey.DependentToPrincipal;
            }
        }
        else
        {
            if (foreignKey.PrincipalToDependent != null)
            {
                yield return foreignKey.PrincipalToDependent;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void GetPropertiesWithMinimalOverlapIfPossible(
        this IForeignKey foreignKey,
        out IReadOnlyList<IProperty> foreignKeyProperties,
        out IReadOnlyList<IProperty> principalKeyProperties)
    {
        // Finds the foreign key properties (and their associated principal key properties) of this foreign key where those
        // properties are not overlapping with any other foreign key, or all properties of the foreign key if there is not
        // a smaller set of non-overlapping properties.

        foreignKeyProperties = foreignKey.Properties;
        principalKeyProperties = foreignKey.PrincipalKey.Properties;

        var count = foreignKeyProperties.Count;
        if (count == 1)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var dependentProperty = foreignKey.Properties[i];

            if (dependentProperty.GetContainingForeignKeys().Count() > 1)
            {
                if (ReferenceEquals(foreignKeyProperties, foreignKey.Properties))
                {
                    foreignKeyProperties = foreignKey.Properties.ToList();
                    principalKeyProperties = foreignKey.PrincipalKey.Properties.ToList();
                }

                ((List<IProperty>)foreignKeyProperties).Remove(dependentProperty);
                ((List<IProperty>)principalKeyProperties).Remove(foreignKey.PrincipalKey.Properties[i]);
            }
        }

        if (!foreignKeyProperties.Any())
        {
            foreignKeyProperties = foreignKey.Properties;
            principalKeyProperties = foreignKey.PrincipalKey.Properties;
        }
    }
}

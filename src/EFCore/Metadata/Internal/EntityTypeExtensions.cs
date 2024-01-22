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
public static class EntityTypeExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static MemberInfo GetNavigationMemberInfo(
        this IReadOnlyEntityType entityType,
        string navigationName)
    {
        MemberInfo? memberInfo;
        if (entityType.IsPropertyBag)
        {
            memberInfo = entityType.FindIndexerPropertyInfo()!;
        }
        else
        {
            memberInfo = entityType.ClrType.GetMembersInHierarchy(navigationName).FirstOrDefault();

            if (memberInfo == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoClrNavigation(navigationName, entityType.DisplayName()));
            }
        }

        return memberInfo;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsOwned(this IReadOnlyEntityType entityType)
        => entityType.IsOwned();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyForeignKey? FindDeclaredOwnership(this IReadOnlyEntityType entityType)
        => entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IConventionForeignKey? FindDeclaredOwnership(this IConventionEntityType entityType)
        => entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyEntityType? FindInOwnershipPath(this IReadOnlyEntityType entityType, Type targetType)
    {
        if (entityType.ClrType == targetType)
        {
            return entityType;
        }

        var owner = entityType;
        while (true)
        {
            var ownership = owner.FindOwnership();
            if (ownership == null)
            {
                return null;
            }

            owner = ownership.PrincipalEntityType;
            if (owner.ClrType.IsAssignableFrom(targetType))
            {
                return owner;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsInOwnershipPath(this IReadOnlyEntityType entityType, Type targetType)
        => entityType.FindInOwnershipPath(targetType) != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsInOwnershipPath(this IReadOnlyEntityType entityType, IReadOnlyEntityType targetType)
        => entityType.IsInOwnershipPath(targetType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool UseEagerSnapshots(this IReadOnlyEntityType entityType)
    {
        var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

        return changeTrackingStrategy is ChangeTrackingStrategy.Snapshot or ChangeTrackingStrategy.ChangedNotifications;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PropertyCounts CalculateCounts(this IRuntimeEntityType entityType)
    {
        var propertyIndex = 0;
        var navigationIndex = 0;
        var complexPropertyIndex = 0;
        var originalValueIndex = 0;
        var shadowIndex = 0;
        var relationshipIndex = 0;
        var storeGenerationIndex = 0;

        var baseCounts = entityType.BaseType?.Counts;
        if (baseCounts != null)
        {
            propertyIndex = baseCounts.PropertyCount;
            navigationIndex = baseCounts.NavigationCount;
            complexPropertyIndex = baseCounts.ComplexPropertyCount;
            originalValueIndex = baseCounts.OriginalValueCount;
            shadowIndex = baseCounts.ShadowCount;
            relationshipIndex = baseCounts.RelationshipCount;
            storeGenerationIndex = baseCounts.StoreGeneratedCount;
        }

        foreach (var property in entityType.GetDeclaredProperties())
        {
            var indexes = new PropertyIndexes(
                index: propertyIndex++,
                originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                shadowIndex: property.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: property.IsKey() || property.IsForeignKey() ? relationshipIndex++ : -1,
                storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

            ((IRuntimePropertyBase)property).PropertyIndexes = indexes;
        }

        CountComplexProperties(entityType.GetDeclaredComplexProperties());

        var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

        foreach (var navigation in entityType.GetDeclaredNavigations()
                     .Union<IPropertyBase>(entityType.GetDeclaredSkipNavigations()))
        {
            var indexes = new PropertyIndexes(
                index: navigationIndex++,
                originalValueIndex: -1,
                shadowIndex: navigation.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: ((IReadOnlyNavigationBase)navigation).IsCollection && isNotifying ? -1 : relationshipIndex++,
                storeGenerationIndex: -1);

            ((IRuntimePropertyBase)navigation).PropertyIndexes = indexes;
        }

        foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
        {
            var indexes = new PropertyIndexes(
                index: -1,
                originalValueIndex: -1,
                shadowIndex: -1,
                relationshipIndex: -1,
                storeGenerationIndex: -1);

            ((IRuntimePropertyBase)serviceProperty).PropertyIndexes = indexes;
        }

        return new PropertyCounts(
            propertyIndex,
            navigationIndex,
            complexPropertyIndex,
            originalValueIndex,
            shadowIndex,
            relationshipIndex,
            storeGenerationIndex);

        void CountComplexProperties(IEnumerable<IComplexProperty> complexProperties)
        {
            foreach (var complexProperty in complexProperties)
            {
                var indexes = new PropertyIndexes(
                    index: complexPropertyIndex++,
                    originalValueIndex: -1,
                    shadowIndex: complexProperty.IsShadowProperty() ? shadowIndex++ : -1,
                    relationshipIndex: -1,
                    storeGenerationIndex: -1);

                ((IRuntimePropertyBase)complexProperty).PropertyIndexes = indexes;

                var complexType = complexProperty.ComplexType;
                foreach (var property in complexType.GetProperties())
                {
                    var complexIndexes = new PropertyIndexes(
                        index: propertyIndex++,
                        originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                        shadowIndex: property.IsShadowProperty() ? shadowIndex++ : -1,
                        relationshipIndex: property.IsKey() || property.IsForeignKey() ? relationshipIndex++ : -1,
                        storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

                    ((IRuntimePropertyBase)property).PropertyIndexes = complexIndexes;
                }

                CountComplexProperties(complexType.GetComplexProperties());
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static EntityType? LeastDerivedType(this EntityType entityType, EntityType otherEntityType)
        => (EntityType?)((IReadOnlyEntityType)entityType).LeastDerivedType(otherEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsAssignableFrom(this EntityType entityType, IReadOnlyEntityType otherEntityType)
        => ((IReadOnlyEntityType)entityType).IsAssignableFrom(otherEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsStrictlyDerivedFrom(this EntityType entityType, IReadOnlyEntityType otherEntityType)
        => ((IReadOnlyEntityType)entityType).IsStrictlyDerivedFrom(otherEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? GetDiscriminatorValue(this EntityType entityType)
        => ((IReadOnlyEntityType)entityType).GetDiscriminatorValue();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyKey? FindDeclaredPrimaryKey(this IReadOnlyEntityType entityType)
        => entityType.BaseType == null ? entityType.FindPrimaryKey() : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyNavigation> FindDerivedNavigations(
        this IReadOnlyEntityType entityType,
        string navigationName)
        => entityType.GetDerivedTypes().Select(t => t.FindDeclaredNavigation(navigationName)!)
            .Where(n => n != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IProperty CheckContains(
        this IEntityType entityType,
        IProperty property)
    {
        Check.NotNull(property, nameof(property));

        if (!property.DeclaringType.ContainingEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyDoesNotBelong(property.Name, property.DeclaringType.DisplayName(), entityType.DisplayName()));
        }

        return property;
    }
}

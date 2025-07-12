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
    public static PropertyCounts CalculateCounts(this IRuntimeEntityType entityType)
    {
        var propertyIndex = 0;
        var navigationIndex = 0;
        var complexPropertyIndex = 0;
        var complexCollectionIndex = 0;
        var originalValueIndex = 0;
        var shadowIndex = 0;
        var relationshipIndex = 0;
        var storeGenerationIndex = 0;

        var baseCounts = entityType.BaseType?.CalculateCounts();
        if (baseCounts != null)
        {
            propertyIndex = baseCounts.PropertyCount;
            navigationIndex = baseCounts.NavigationCount;
            complexPropertyIndex = baseCounts.ComplexPropertyCount;
            complexCollectionIndex = baseCounts.ComplexCollectionCount;
            originalValueIndex = baseCounts.OriginalValueCount;
            shadowIndex = baseCounts.ShadowCount;
            relationshipIndex = baseCounts.RelationshipCount;
            storeGenerationIndex = baseCounts.StoreGeneratedCount;
        }

        foreach (IRuntimeProperty property in entityType.GetDeclaredProperties())
        {
            var indexes = new PropertyIndexes(
                index: propertyIndex++,
                originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                shadowIndex: property.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: property.IsKey() || property.IsForeignKey() ? relationshipIndex++ : -1,
                storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

            property.PropertyIndexes = indexes;
        }

        CountComplexProperties(
            entityType.GetDeclaredComplexProperties(),
            ref propertyIndex,
            ref complexPropertyIndex,
            ref complexCollectionIndex,
            ref originalValueIndex,
            ref shadowIndex,
            ref relationshipIndex,
            ref storeGenerationIndex);

        var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

        foreach (IRuntimeNavigationBase navigation in entityType.GetDeclaredNavigations()
                    .Union<INavigationBase>(entityType.GetDeclaredSkipNavigations()))
        {
            var indexes = new PropertyIndexes(
                index: navigationIndex++,
                originalValueIndex: -1,
                shadowIndex: navigation.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: navigation.IsCollection && isNotifying ? -1 : relationshipIndex++,
                storeGenerationIndex: -1);

            navigation.PropertyIndexes = indexes;
        }

        foreach (IRuntimeServiceProperty serviceProperty in entityType.GetDeclaredServiceProperties())
        {
            var indexes = new PropertyIndexes(
                index: -1,
                originalValueIndex: -1,
                shadowIndex: -1,
                relationshipIndex: -1,
                storeGenerationIndex: -1);

            serviceProperty.PropertyIndexes = indexes;
        }

        return new PropertyCounts(
            propertyIndex,
            navigationIndex,
            complexPropertyIndex,
            complexCollectionIndex,
            originalValueIndex,
            shadowIndex,
            relationshipIndex,
            storeGenerationIndex);
    }

    private static void CountComplexProperties(
        IEnumerable<IComplexProperty> complexProperties,
        ref int propertyIndex,
        ref int complexPropertyIndex,
        ref int complexCollectionIndex,
        ref int originalValueIndex,
        ref int shadowIndex,
        ref int relationshipIndex,
        ref int storeGenerationIndex)
    {
        foreach (IRuntimeComplexProperty complexProperty in complexProperties)
        {
            CalculateCounts(
                complexProperty,
                ref propertyIndex,
                ref complexPropertyIndex,
                ref complexCollectionIndex,
                ref originalValueIndex,
                ref shadowIndex,
                ref relationshipIndex,
                ref storeGenerationIndex);
        }
    }

    private static void CalculateCounts(
        IRuntimeComplexProperty complexProperty,
        ref int propertyIndex,
        ref int complexPropertyIndex,
        ref int complexCollectionIndex,
        ref int originalValueIndex,
        ref int shadowIndex,
        ref int relationshipIndex,
        ref int storeGenerationIndex)
    {
        var indexes = new PropertyIndexes(
            index: complexProperty.IsCollection ? complexCollectionIndex++ : complexPropertyIndex++,
            originalValueIndex: complexProperty.IsCollection ? originalValueIndex++ : -1,
            shadowIndex: complexProperty.IsShadowProperty() ? shadowIndex++ : -1,
            relationshipIndex: -1,
            storeGenerationIndex: -1);

        complexProperty.PropertyIndexes = indexes;

        var parentPropertyIndex = propertyIndex;
        var parentComplexPropertyIndex = complexPropertyIndex;
        var parentComplexCollectionIndex = complexCollectionIndex;
        var parentOriginalValueIndex = originalValueIndex;
        var parentShadowIndex = shadowIndex;
        var parentRelationshipIndex = relationshipIndex;
        var parentStoreGenerationIndex = storeGenerationIndex;

        if (complexProperty.IsCollection)
        {
            propertyIndex = 0;
            complexPropertyIndex = 0;
            complexCollectionIndex = 0;
            originalValueIndex = 0;
            shadowIndex = 0;
            relationshipIndex = 0;
            storeGenerationIndex = 0;
        }

        var complexType = complexProperty.ComplexType;
        foreach (IRuntimeProperty property in complexType.GetProperties())
        {
            var complexIndexes = new PropertyIndexes(
                index: propertyIndex++,
                originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                shadowIndex: property.IsShadowProperty() ? shadowIndex++ : -1,
                relationshipIndex: property.IsKey() || property.IsForeignKey() ? relationshipIndex++ : -1,
                storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

            property.PropertyIndexes = complexIndexes;
        }

        CountComplexProperties(
            complexType.GetComplexProperties(),
            ref propertyIndex,
            ref complexPropertyIndex,
            ref complexCollectionIndex,
            ref originalValueIndex,
            ref shadowIndex,
            ref relationshipIndex,
            ref storeGenerationIndex);

        if (complexProperty.IsCollection)
        {
            ((IRuntimeComplexType)complexProperty.ComplexType).SetCounts(new PropertyCounts(
                propertyIndex,
                navigationCount: 0,
                complexPropertyIndex,
                complexCollectionIndex,
                originalValueIndex,
                shadowIndex,
                relationshipIndex,
                storeGenerationIndex));

            propertyIndex = parentPropertyIndex;
            complexPropertyIndex = parentComplexPropertyIndex;
            complexCollectionIndex = parentComplexCollectionIndex;
            originalValueIndex = parentOriginalValueIndex;
            shadowIndex = parentShadowIndex;
            relationshipIndex = parentRelationshipIndex;
            storeGenerationIndex = parentStoreGenerationIndex;
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
}

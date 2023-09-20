// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class PropertyBaseExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static int GetShadowIndex(this IPropertyBase propertyBase)
        => ((IRuntimePropertyBase)propertyBase).GetShadowIndex();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static int GetStoreGeneratedIndex(this IPropertyBase propertyBase)
        => ((IRuntimePropertyBase)propertyBase).GetStoreGeneratedIndex();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static int GetRelationshipIndex(this IPropertyBase propertyBase)
        => ((IRuntimePropertyBase)propertyBase).GetRelationshipIndex();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static int GetOriginalValueIndex(this IPropertyBase propertyBase)
        => ((IRuntimePropertyBase)propertyBase).GetOriginalValueIndex();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PropertyIndexes GetPropertyIndexes(this IPropertyBase propertyBase)
        => ((IRuntimePropertyBase)propertyBase).PropertyIndexes;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PropertyAccessors GetPropertyAccessors(this IPropertyBase propertyBase)
        => ((IRuntimePropertyBase)propertyBase).Accessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsShadowProperty(this PropertyBase propertyBase)
        => ((IReadOnlyPropertyBase)propertyBase).IsShadowProperty();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsIndexerProperty(this PropertyBase propertyBase)
        => ((IReadOnlyPropertyBase)propertyBase).IsIndexerProperty();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Note: only use this to find the property/field that defines the property in the model. Use
    // GetMemberInfo to get the property/field to use, which may be different.
    public static MemberInfo? GetIdentifyingMemberInfo(
        this IReadOnlyPropertyBase propertyBase)
    {
        var indexerPropertyInfo = propertyBase.DeclaringType.FindIndexerPropertyInfo();
        return indexerPropertyInfo != null && propertyBase.PropertyInfo == indexerPropertyInfo
            ? null
            : (propertyBase.PropertyInfo ?? (MemberInfo?)propertyBase.FieldInfo);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryGetMemberInfo(
        this IPropertyBase propertyBase,
        bool forMaterialization,
        bool forSet,
        out MemberInfo? memberInfo,
        out string? errorMessage)
    {
        memberInfo = null;
        errorMessage = null;

        var propertyInfo = propertyBase.PropertyInfo;
        var fieldInfo = propertyBase.FieldInfo;
        var setterProperty = propertyInfo?.FindSetterProperty();
        var getterProperty = propertyInfo?.FindGetterProperty();

        var isCollectionNav = (propertyBase as IReadOnlyNavigationBase)?.IsCollection == true;
        var hasField = fieldInfo != null;
        var hasSetter = setterProperty != null;
        var hasGetter = getterProperty != null;

        var mode = propertyBase.GetPropertyAccessMode();

        if (forMaterialization)
        {
            switch (mode)
            {
                case PropertyAccessMode.Field:
                case PropertyAccessMode.FieldDuringConstruction:
                {
                    if (hasField)
                    {
                        memberInfo = fieldInfo;
                        return true;
                    }

                    if (isCollectionNav)
                    {
                        return true;
                    }

                    errorMessage = GetNoFieldErrorMessage(propertyBase);
                    return false;
                }
                case PropertyAccessMode.Property when hasSetter:
                    memberInfo = setterProperty;
                    return true;
                case PropertyAccessMode.Property when isCollectionNav:
                    return true;
                case PropertyAccessMode.Property:
                    errorMessage = hasGetter
                        ? CoreStrings.NoSetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode))
                        : CoreStrings.NoProperty(fieldInfo?.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));

                    return false;
                case PropertyAccessMode.PreferField:
                case PropertyAccessMode.PreferFieldDuringConstruction:
                {
                    if (hasField)
                    {
                        memberInfo = fieldInfo;
                        return true;
                    }

                    if (hasSetter)
                    {
                        memberInfo = setterProperty;
                        return true;
                    }

                    break;
                }
                case PropertyAccessMode.PreferProperty when hasSetter:
                    memberInfo = setterProperty;
                    return true;
                case PropertyAccessMode.PreferProperty when hasField:
                    memberInfo = fieldInfo;
                    return true;
            }

            if (isCollectionNav)
            {
                return true;
            }

            errorMessage = CoreStrings.NoFieldOrSetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName());
            return false;
        }

        if (forSet)
        {
            if (mode == PropertyAccessMode.Field)
            {
                if (hasField)
                {
                    memberInfo = fieldInfo;
                    return true;
                }

                if (isCollectionNav)
                {
                    return true;
                }

                errorMessage = GetNoFieldErrorMessage(propertyBase);
                return false;
            }

            if (mode == PropertyAccessMode.Property)
            {
                if (hasSetter)
                {
                    memberInfo = setterProperty;
                    return true;
                }

                if (isCollectionNav)
                {
                    return true;
                }

                errorMessage = hasGetter
                    ? CoreStrings.NoSetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode))
                    : CoreStrings.NoProperty(fieldInfo?.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));

                return false;
            }

            if (mode == PropertyAccessMode.PreferField)
            {
                if (hasField)
                {
                    memberInfo = fieldInfo;
                    return true;
                }

                if (hasSetter)
                {
                    memberInfo = setterProperty;
                    return true;
                }
            }

            if (mode is PropertyAccessMode.PreferProperty
                or PropertyAccessMode.FieldDuringConstruction
                or PropertyAccessMode.PreferFieldDuringConstruction)
            {
                if (hasSetter)
                {
                    memberInfo = setterProperty;
                    return true;
                }

                if (hasField)
                {
                    memberInfo = fieldInfo;
                    return true;
                }
            }

            if (isCollectionNav)
            {
                return true;
            }

            errorMessage = CoreStrings.NoFieldOrSetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName());
            return false;
        }

        // forGet
        if (mode == PropertyAccessMode.Field)
        {
            if (hasField)
            {
                memberInfo = fieldInfo;
                return true;
            }

            errorMessage = GetNoFieldErrorMessage(propertyBase);
            return false;
        }

        if (mode == PropertyAccessMode.Property)
        {
            if (hasGetter)
            {
                memberInfo = getterProperty;
                return true;
            }

            errorMessage = hasSetter
                ? CoreStrings.NoGetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode))
                : CoreStrings.NoProperty(fieldInfo?.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));

            return false;
        }

        if (mode == PropertyAccessMode.PreferField)
        {
            if (hasField)
            {
                memberInfo = fieldInfo;
                return true;
            }

            if (hasGetter)
            {
                memberInfo = getterProperty;
                return true;
            }
        }

        if (mode is PropertyAccessMode.PreferProperty
            or PropertyAccessMode.FieldDuringConstruction
            or PropertyAccessMode.PreferFieldDuringConstruction)
        {
            if (hasGetter)
            {
                memberInfo = getterProperty;
                return true;
            }

            if (hasField)
            {
                memberInfo = fieldInfo;
                return true;
            }
        }

        errorMessage = CoreStrings.NoFieldOrGetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName());
        return false;
    }

    private static string GetNoFieldErrorMessage(IPropertyBase propertyBase)
        => ((EntityType)propertyBase.DeclaringType).GetServiceProperties()
            .Any(p => typeof(ILazyLoader).IsAssignableFrom(p.ClrType))
            || ((EntityType)propertyBase.DeclaringType).ConstructorBinding?.ParameterBindings
            .OfType<ServiceParameterBinding>()
            .Any(b => b.ServiceType == typeof(ILazyLoader))
            == true
                ? CoreStrings.NoBackingFieldLazyLoading(
                    propertyBase.Name, propertyBase.DeclaringType.DisplayName())
                : CoreStrings.NoBackingField(
                    propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
}

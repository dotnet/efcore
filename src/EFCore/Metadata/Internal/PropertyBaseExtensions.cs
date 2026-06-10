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

        var isCollection = propertyBase.IsCollection;
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

                    if (isCollection)
                    {
                        return true;
                    }

                    errorMessage = GetNoFieldErrorMessage(propertyBase);
                    return false;
                }
                case PropertyAccessMode.Property when hasSetter:
                    memberInfo = setterProperty;
                    return true;
                case PropertyAccessMode.Property when isCollection:
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

            if (isCollection)
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

                if (isCollection)
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

                if (isCollection)
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

            if (isCollection)
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

    /// <summary>
    ///     Builds the message for the diagnostic that fires when a member conflicts with an existing
    ///     member on the structural type or one of its base types. The kind of the conflicting member
    ///     is humanized via <see cref="GetMemberKindString" /> so the user-facing message uses stable
    ///     labels like "property", "complex property", "navigation", "skip navigation", or
    ///     "service property" regardless of whether the conflicting member came from a model or a
    ///     runtime model.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static string FormatConflictingMemberMessage(
        this IReadOnlyPropertyBase conflictingMember,
        string newMemberName,
        IReadOnlyTypeBase owningType)
    {
        var conflictingMemberKind = GetMemberKindString(conflictingMember);
        var owningTypeDisplayName = owningType.DisplayName();

        // Compare the actual metadata instances rather than display names to avoid false positives when
        // two distinct types share a simple name (e.g. same name in different namespaces or hierarchies).
        return conflictingMember.DeclaringType == owningType
            ? CoreStrings.ConflictingPropertyOrNavigationWithKind(newMemberName, owningTypeDisplayName, conflictingMemberKind)
            : CoreStrings.ConflictingPropertyOrNavigationOnBaseType(
                newMemberName,
                owningTypeDisplayName,
                conflictingMemberKind,
                ((IReadOnlyTypeBase)conflictingMember.DeclaringType).DisplayName());
    }

    /// <summary>
    ///     Returns a human-readable label for the kind of the given member (e.g. "property",
    ///     "complex property", "navigation", "skip navigation", "service property"). Used to build
    ///     user-facing diagnostic messages without coupling the message text to internal CLR class
    ///     names (such as <c>RuntimeProperty</c> or <c>SkipNavigation</c>).
    /// </summary>
    private static string GetMemberKindString(IReadOnlyPropertyBase member)
        => member switch
        {
            IReadOnlyComplexProperty => "complex property",
            IReadOnlySkipNavigation => "skip navigation",
            IReadOnlyNavigation => "navigation",
            IReadOnlyServiceProperty => "service property",
            IReadOnlyProperty => "property",
            _ => member.GetType().Name
        };

    private static string GetNoFieldErrorMessage(IPropertyBase propertyBase)
        => propertyBase.DeclaringType switch
        {
            EntityType entityType
                when entityType.GetServiceProperties().Any(p => typeof(ILazyLoader).IsAssignableFrom(p.ClrType))
                || entityType.ConstructorBinding?.ParameterBindings
                    .OfType<ServiceParameterBinding>()
                    .Any(b => b.ServiceType == typeof(ILazyLoader))
                == true
                => CoreStrings.NoBackingFieldLazyLoading(
                    propertyBase.Name, propertyBase.DeclaringType.DisplayName()),
            _ => CoreStrings.NoBackingField(
                propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode)),
        };
}

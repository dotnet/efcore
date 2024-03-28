// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public interface IRuntimeModel : IModel
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool SkipDetectChanges { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    object? RelationalModel { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyDictionary<MemberInfo, QualifiedName>? GetUnsafeAccessors()
    {
        var accessorsAnnotation = FindRuntimeAnnotation(CoreAnnotationNames.UnsafeAccessors);
        if (accessorsAnnotation != null)
        {
            return accessorsAnnotation.Value as IReadOnlyDictionary<MemberInfo, QualifiedName>;
        }

        var accessors = new Dictionary<MemberInfo, QualifiedName>();
        foreach (var entityType in GetEntityTypes())
        {
            AddPropertyAccessors(entityType, accessors);

            foreach (var property in entityType.GetDeclaredServiceProperties())
            {
                AddAccessors(property, accessors);
            }

            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                AddAccessors(navigation, accessors);
            }

            foreach (var navigation in entityType.GetDeclaredSkipNavigations())
            {
                AddAccessors(navigation, accessors);
            }
        }

        SetRuntimeAnnotation(CoreAnnotationNames.UnsafeAccessors, accessors);
        return accessors;

        static void AddPropertyAccessors(ITypeBase structuralType, Dictionary<MemberInfo, QualifiedName> accessors)
        {
            foreach (var property in structuralType.GetDeclaredProperties())
            {
                AddAccessors(property, accessors);
            }

            foreach (var complexProperty in structuralType.GetDeclaredComplexProperties())
            {
                AddAccessors(complexProperty, accessors);
                AddPropertyAccessors(complexProperty.ComplexType, accessors);
            }
        }

        static void AddAccessors(IPropertyBase property, Dictionary<MemberInfo, QualifiedName> accessors)
        {
            if (property.FindRuntimeAnnotationValue(CoreAnnotationNames.UnsafeAccessors) is not (string?, string?)[] propertyAccessors)
            {
                return;
            }

            SetAccessor(propertyAccessors.Length < 1 ? (null, null) : propertyAccessors[0], forMaterialization: false, forSet: false);
            SetAccessor(propertyAccessors.Length < 2 ? (null, null) : propertyAccessors[1], forMaterialization: false, forSet: true);
            SetAccessor(propertyAccessors.Length < 3 ? (null, null) : propertyAccessors[2], forMaterialization: true, forSet: false);
            SetAccessor(propertyAccessors.Length < 4 ? (null, null) : propertyAccessors[3], forMaterialization: true, forSet: true);

            void SetAccessor((string?, string?) accessor, bool forMaterialization, bool forSet)
            {
                if (accessor.Item1 == null
                    || accessor.Item2 == null)
                {
                    return;
                }

                var memberInfo = property.GetMemberInfo(forMaterialization, forSet);
                if (memberInfo is PropertyInfo propertyInfo)
                {
                    memberInfo = forSet ? propertyInfo.SetMethod! : propertyInfo.GetMethod!;
                }

                accessors[memberInfo] = new QualifiedName(accessor.Item1, accessor.Item2);
            }
        }
    }
}

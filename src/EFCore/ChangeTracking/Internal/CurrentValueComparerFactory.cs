// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CurrentValueComparerFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IComparer<IUpdateEntry> Create(IPropertyBase propertyBase)
    {
        var modelType = propertyBase.ClrType;
        var nonNullableModelType = modelType.UnwrapNullableType();
        if (IsGenericComparable(modelType, nonNullableModelType))
        {
            return (IComparer<IUpdateEntry>)Activator.CreateInstance(
                typeof(EntryCurrentValueComparer<>).MakeGenericType(modelType),
                propertyBase)!;
        }

        if (typeof(IStructuralComparable).IsAssignableFrom(nonNullableModelType))
        {
            return new StructuralEntryCurrentValueComparer(propertyBase);
        }

        if (typeof(IComparable).IsAssignableFrom(nonNullableModelType))
        {
            return new EntryCurrentValueComparer(propertyBase);
        }

        if (propertyBase is IProperty property)
        {
            var converter = property.GetTypeMapping().Converter;
            if (converter != null)
            {
                var providerType = converter.ProviderClrType;
                var nonNullableProviderType = providerType.UnwrapNullableType();
                if (IsGenericComparable(providerType, nonNullableProviderType))
                {
                    var comparerType = modelType.IsClass
                        ? typeof(NullableClassCurrentProviderValueComparer<,>).MakeGenericType(modelType, providerType)
                        : modelType == converter.ModelClrType
                            ? typeof(CurrentProviderValueComparer<,>).MakeGenericType(modelType, providerType)
                            : typeof(NullableStructCurrentProviderValueComparer<,>).MakeGenericType(
                                nonNullableModelType, providerType);

                    return (IComparer<IUpdateEntry>)Activator.CreateInstance(comparerType, propertyBase, converter)!;
                }

                if (typeof(IStructuralComparable).IsAssignableFrom(nonNullableProviderType))
                {
                    return new StructuralEntryCurrentProviderValueComparer(propertyBase, converter);
                }

                if (typeof(IComparable).IsAssignableFrom(nonNullableProviderType))
                {
                    return new EntryCurrentProviderValueComparer(propertyBase, converter);
                }

                throw new InvalidOperationException(
                    CoreStrings.NonComparableKeyTypes(
                        propertyBase.DeclaringType.DisplayName(),
                        propertyBase.Name,
                        modelType.ShortDisplayName(),
                        providerType.ShortDisplayName()));
            }
        }

        throw new InvalidOperationException(
            CoreStrings.NonComparableKeyType(
                propertyBase.DeclaringType.DisplayName(),
                propertyBase.Name,
                modelType.ShortDisplayName()));

        static bool IsGenericComparable(Type type, Type nonNullableType)
            => typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type)
                || typeof(IComparable<>).MakeGenericType(nonNullableType).IsAssignableFrom(nonNullableType)
                || type.IsEnum
                || nonNullableType.IsEnum;
    }
}

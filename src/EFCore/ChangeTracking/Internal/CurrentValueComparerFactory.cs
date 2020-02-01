// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
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
        public virtual IComparer<IUpdateEntry> Create([NotNull] IPropertyBase propertyBase)
        {
            var modelType = propertyBase.ClrType;
            var nonNullableModelType = modelType.UnwrapNullableType();
            if (IsGenericComparable(modelType, nonNullableModelType))
            {
                return (IComparer<IUpdateEntry>)Activator.CreateInstance(
                    typeof(CurrentValueComparer<>).MakeGenericType(modelType),
                    propertyBase);
            }

            if (typeof(IStructuralComparable).IsAssignableFrom(nonNullableModelType))
            {
                return new StructuralCurrentValueComparer(propertyBase);
            }

            if (typeof(IComparable).IsAssignableFrom(nonNullableModelType))
            {
                return new CurrentValueComparer(propertyBase);
            }

            if (propertyBase is IProperty property)
            {
                var converter = property.GetValueConverter()
                    ?? property.GetTypeMapping().Converter;

                if (converter != null)
                {
                    var providerType = converter.ProviderClrType;
                    var nonNullableProviderType = providerType.UnwrapNullableType();
                    if (IsGenericComparable(providerType, nonNullableProviderType))
                    {
                        var comparerType = modelType.IsClass
                            ? typeof(NullableClassCurrentProviderValueComparer<,>).MakeGenericType(modelType, converter.ProviderClrType)
                            : modelType == converter.ModelClrType
                                ? typeof(CurrentProviderValueComparer<,>).MakeGenericType(modelType, converter.ProviderClrType)
                                : typeof(NullableStructCurrentProviderValueComparer<,>).MakeGenericType(
                                    nonNullableModelType, converter.ProviderClrType);

                        return (IComparer<IUpdateEntry>)Activator.CreateInstance(comparerType, propertyBase, converter);
                    }

                    if (typeof(IStructuralComparable).IsAssignableFrom(nonNullableProviderType))
                    {
                        return new StructuralCurrentProviderValueComparer(propertyBase, converter);
                    }

                    if (typeof(IComparable).IsAssignableFrom(nonNullableProviderType))
                    {
                        return new CurrentProviderValueComparer(propertyBase, converter);
                    }

                    throw new InvalidOperationException(
                        CoreStrings.NonComparableKeyTypes(
                            propertyBase.DeclaringType.DisplayName(),
                            propertyBase.Name,
                            modelType.ShortDisplayName(),
                            providerType.ShortDisplayName() ));
                }
            }

            throw new InvalidOperationException(
                CoreStrings.NonComparableKeyType(
                    propertyBase.DeclaringType.DisplayName(),
                    propertyBase.Name,
                    modelType.ShortDisplayName()));

            bool IsGenericComparable(Type type, Type nonNullableType)
                => typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type)
                    || typeof(IComparable<>).MakeGenericType(nonNullableType).IsAssignableFrom(nonNullableType)
                    || type.IsEnum
                    || nonNullableType.IsEnum;
        }
    }
}

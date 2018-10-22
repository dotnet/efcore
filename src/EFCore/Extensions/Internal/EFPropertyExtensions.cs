// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerStepThrough]
    // ReSharper disable once InconsistentNaming
    public static class EFPropertyExtensions
    {
        private static readonly string _efTypeName = typeof(EF).FullName;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsEFProperty([NotNull] this MethodCallExpression methodCallExpression)
            => IsEFPropertyMethod(methodCallExpression.Method);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsEFPropertyMethod([CanBeNull] this MethodInfo methodInfo)
            => Equals(methodInfo, EF.PropertyMethod)
               // fallback to string comparison because MethodInfo.Equals is not
               // always true in .NET Native even if methods are the same
               || methodInfo?.IsGenericMethod == true
               && methodInfo.Name == nameof(EF.Property)
               && methodInfo.DeclaringType?.FullName == _efTypeName;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsEFIndexer(this MethodInfo methodInfo)
            => !methodInfo.IsStatic
               && "get_Item".Equals(methodInfo.Name, StringComparison.Ordinal)
               && typeof(object) == methodInfo.ReturnType
               && methodInfo.GetParameters()?.Count() == 1
               && typeof(string) == methodInfo.GetParameters().First().ParameterType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression CreateEFPropertyExpression(
            [NotNull] this Expression target,
            [NotNull] IPropertyBase property,
            bool makeNullable = true)
            => CreateEFPropertyExpression(target, property.DeclaringType.ClrType, property.ClrType, property.Name, makeNullable);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression CreateEFPropertyExpression(
            [NotNull] this Expression target,
            [NotNull] MemberInfo memberInfo)
            => CreateEFPropertyExpression(target, memberInfo.DeclaringType, memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), makeNullable: false);

        private static Expression CreateEFPropertyExpression(
            Expression target,
            Type propertyDeclaringType,
            Type propertyType,
            string propertyName,
            bool makeNullable)
        {
            if (propertyDeclaringType != target.Type
                && target.Type.GetTypeInfo().IsAssignableFrom(propertyDeclaringType.GetTypeInfo()))
            {
                target = Expression.Convert(target, propertyDeclaringType);
            }

            if (makeNullable)
            {
                propertyType = propertyType.MakeNullable();
            }

            return Expression.Call(
                EF.PropertyMethod.MakeGenericMethod(propertyType),
                target,
                Expression.Constant(propertyName));
        }
    }
}

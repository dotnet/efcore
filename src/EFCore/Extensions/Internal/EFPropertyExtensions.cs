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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    // ReSharper disable once InconsistentNaming
    public static class EFPropertyExtensions
    {
        private static readonly string _efTypeName = typeof(EF).FullName;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool TryGetEFPropertyArguments(
            [NotNull] this MethodCallExpression methodCallExpression,
            out Expression entityExpression,
            out string propertyName)
        {
            if (IsEFProperty(methodCallExpression)
                && methodCallExpression.Arguments[1] is ConstantExpression propertyNameExpression)
            {
                entityExpression = methodCallExpression.Arguments[0];
                propertyName = (string)propertyNameExpression.Value;
                return true;
            }

            (entityExpression, propertyName) = (null, null);
            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsEFProperty([NotNull] this MethodCallExpression methodCallExpression)
            => IsEFPropertyMethod(methodCallExpression.Method);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsEFPropertyMethod([CanBeNull] this MethodInfo methodInfo)
            => Equals(methodInfo, EF.PropertyMethod)
               // fallback to string comparison because MethodInfo.Equals is not
               // always true in .NET Native even if methods are the same
               || methodInfo?.IsGenericMethod == true
               && methodInfo.Name == nameof(EF.Property)
               && methodInfo.DeclaringType?.FullName == _efTypeName;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool TryGetEFIndexerArguments(
            [NotNull] this MethodCallExpression methodCallExpression,
            out Expression entityExpression,
            out string propertyName)
        {
            if (IsEFIndexer(methodCallExpression)
                && methodCallExpression.Arguments[0] is ConstantExpression propertyNameExpression)
            {
                entityExpression = methodCallExpression.Object;
                propertyName = (string)propertyNameExpression.Value;
                return true;
            }

            (entityExpression, propertyName) = (null, null);
            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsEFIndexer([NotNull] this MethodCallExpression methodCallExpression)
            => IsEFIndexer(methodCallExpression.Method);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsEFIndexer(this MethodInfo methodInfo)
            => !methodInfo.IsStatic
               && "get_Item".Equals(methodInfo.Name, StringComparison.Ordinal)
               && typeof(object) == methodInfo.ReturnType
               && methodInfo.GetParameters()?.Count() == 1
               && typeof(string) == methodInfo.GetParameters().First().ParameterType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression CreateEFPropertyExpression(
            [NotNull] this Expression target,
            [NotNull] IPropertyBase property,
            bool makeNullable = true)
            => CreateEFPropertyExpression(target, property.DeclaringType.ClrType, property.ClrType, property.Name, makeNullable);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression CreateEFPropertyExpression(
            [NotNull] this Expression target,
            [NotNull] MemberInfo memberInfo)
            => CreateEFPropertyExpression(
                target, memberInfo.DeclaringType, memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), makeNullable: false);

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

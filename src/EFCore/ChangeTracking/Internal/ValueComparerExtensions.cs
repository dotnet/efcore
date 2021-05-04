// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class ValueComparerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ValueComparer ToNonNullNullableComparer(this ValueComparer comparer)
        {
            var type = comparer.EqualsExpression.Parameters[0].Type;
            var nullableType = type.MakeNullable();

            var newEqualsParam1 = Expression.Parameter(nullableType, "v1");
            var newEqualsParam2 = Expression.Parameter(nullableType, "v2");
            var newHashCodeParam = Expression.Parameter(nullableType, "v");
            var newSnapshotParam = Expression.Parameter(nullableType, "v");

            return (ValueComparer)Activator.CreateInstance(
                typeof(NonNullNullableValueComparer<>).MakeGenericType(nullableType),
                Expression.Lambda(
                    comparer.ExtractEqualsBody(
                        Expression.Convert(newEqualsParam1, type),
                        Expression.Convert(newEqualsParam2, type)),
                    newEqualsParam1, newEqualsParam2),
                Expression.Lambda(
                    comparer.ExtractHashCodeBody(
                        Expression.Convert(newHashCodeParam, type)),
                    newHashCodeParam),
                Expression.Lambda(
                    Expression.Convert(
                        comparer.ExtractSnapshotBody(
                            Expression.Convert(newSnapshotParam, type)),
                        nullableType),
                    newSnapshotParam))!;
        }

        private sealed class NonNullNullableValueComparer<T> : ValueComparer<T>
        {
            public NonNullNullableValueComparer(
                LambdaExpression equalsExpression,
                LambdaExpression hashCodeExpression,
                LambdaExpression snapshotExpression)
                : base(
                    (Expression<Func<T?, T?, bool>>)equalsExpression,
                    (Expression<Func<T, int>>)hashCodeExpression,
                    (Expression<Func<T?, T?>>)snapshotExpression)
            {
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ValueComparerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ValueComparer ToNonNullNullableComparer([NotNull] this ValueComparer comparer)
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
                    newSnapshotParam));
        }

        private class NonNullNullableValueComparer<T> : ValueComparer<T>
        {
#pragma warning disable CA1061 // Do not hide base class methods
            public NonNullNullableValueComparer(
#pragma warning restore CA1061 // Do not hide base class methods
                LambdaExpression equalsExpression,
                LambdaExpression hashCodeExpression,
                LambdaExpression snapshotExpression)
                : base(
                    (Expression<Func<T, T, bool>>)equalsExpression,
                    (Expression<Func<T, int>>)hashCodeExpression,
                    (Expression<Func<T, T>>)snapshotExpression)
            {
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, nameof(source));

            if (source is IAsyncEnumerable<TSource> enumerable)
            {
                return enumerable;
            }

            if (source is IAsyncEnumerableAccessor<TSource> entityQueryableAccessor)
            {
                return entityQueryableAccessor.AsyncEnumerable;
            }

            throw new InvalidOperationException(CoreStrings.IQueryableNotAsync(typeof(TSource)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IQueryable Select(
            [NotNull] this IQueryable source, [NotNull] string propertyName, [NotNull] Type sourceType, [NotNull] Type resultType)
            => (IQueryable)_selectMethod.MakeGenericMethod(sourceType, resultType).Invoke(null, new object[] { source, propertyName });

        private static readonly MethodInfo _selectMethod
            = typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethods(nameof(Select)).Single(mi => mi.IsGenericMethodDefinition);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IQueryable<TResult> Select<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source, [NotNull] string propertyName)
            where TResult : class
            where TSource : class
        {
            var parameter = Expression.Parameter(typeof(TSource), "e");
            return source.Select(Expression.Lambda<Func<TSource, TResult>>(
                Expression.MakeMemberAccess(parameter, typeof(TSource).GetAnyProperty(propertyName)),
                parameter));
        }
    }
}

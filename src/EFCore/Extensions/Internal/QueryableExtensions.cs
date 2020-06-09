// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class QueryableExtensions
    {
        internal static readonly MethodInfo LeftJoinMethodInfo = typeof(QueryableExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(LeftJoin)).Single(mi => mi.GetParameters().Length == 5);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
            [NotNull] this IQueryable<TOuter> outer,
            [NotNull] IEnumerable<TInner> inner,
            [NotNull] Expression<Func<TOuter, TKey>> outerKeySelector,
            [NotNull] Expression<Func<TInner, TKey>> innerKeySelector,
            [NotNull] Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            throw new NotImplementedException();
        }
    }
}

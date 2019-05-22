// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    }
}

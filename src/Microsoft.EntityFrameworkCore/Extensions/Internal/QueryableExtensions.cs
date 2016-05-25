// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    public static class QueryableExtensions
    {
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, nameof(source));

            var enumerable = source as IAsyncEnumerable<TSource>;

            if (enumerable != null)
            {
                return enumerable;
            }

            var entityQueryableAccessor = source as IAsyncEnumerableAccessor<TSource>;

            if (entityQueryableAccessor != null)
            {
                return entityQueryableAccessor.AsyncEnumerable;
            }

            throw new InvalidOperationException(CoreStrings.IQueryableNotAsync(typeof(TSource)));
        }
    }
}

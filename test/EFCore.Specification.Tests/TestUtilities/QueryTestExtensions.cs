// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class QueryTestExtensions
    {
        public static TResult Maybe<TSource, TResult>(this TSource caller, Func<TSource, TResult> result)
            where TResult : class
            => caller == null ? null : result(caller);

        public static TResult? MaybeScalar<TSource, TResult>(this TSource caller, Func<TSource, TResult> result)
            where TResult : struct
            => caller != null ? (TResult?)result(caller) : null;

        public static TResult? MaybeScalar<TSource, TResult>(this TSource caller, Func<TSource, TResult?> result)
            where TResult : struct
            => caller != null ? result(caller) : null;

        public static IEnumerable<TResult> MaybeDefaultIfEmpty<TResult>(this IEnumerable<TResult> caller)
            => caller == null ? new List<TResult> { default } : caller.DefaultIfEmpty();
    }
}

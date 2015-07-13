// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    internal static class QueryAnnotationExtensions
    {
        public static readonly MethodInfo QueryAnnotationMethodInfo
            = typeof(QueryAnnotationExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(QueryAnnotation));

        public static IQueryable<TEntity> QueryAnnotation<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] QueryAnnotationBase annotation)
            where TEntity : class
            => QueryableHelpers.CreateQuery(source, s => s.QueryAnnotation(annotation));
    }
}

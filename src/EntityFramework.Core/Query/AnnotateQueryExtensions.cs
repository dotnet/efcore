// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public static class AnnotateQueryExtensions
    {
        internal static readonly MethodInfo AnnotateQueryMethodInfo
            = typeof(AnnotateQueryExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(AnnotateQuery));

        public static IQueryable<TEntity> AnnotateQuery<TEntity>([NotNull] this IQueryable<TEntity> source, [NotNull] object annotation) where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(annotation, nameof(annotation));

            return source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    AnnotateQueryMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    new[] { source.Expression, Expression.Constant(annotation) }));
        }
    }
}

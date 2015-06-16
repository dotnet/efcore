// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalQueryableExtensions
    {
        internal static readonly MethodInfo FromSqlMethodInfo
            = typeof(RelationalQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(FromSql));

        [QueryAnnotationMethod]
        public static IQueryable<TEntity> FromSql<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] string sql,
            [NotNull] params object[] parameters)
            where TEntity : class
            => QueryableHelpers.CreateQuery(source, s => s.FromSql(sql, parameters));

        internal static readonly MethodInfo UseRelationalNullSemanticsMethodInfo
            = typeof(RelationalQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(UseRelationalNullSemantics));

        [QueryAnnotationMethod]
        public static IQueryable<TEntity> UseRelationalNullSemantics<TEntity>(
            [NotNull] this IQueryable<TEntity> source)
            where TEntity : class
            => QueryableHelpers.CreateQuery(source, s => s.UseRelationalNullSemantics());
    }
}

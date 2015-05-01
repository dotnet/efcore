// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Query.Annotations;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDbSetExtensions
    {
        public static IQueryable<TEntity> FromSql<TEntity>([NotNull] this DbSet<TEntity> set, [NotNull] string sql)
            where TEntity : class
            => Check.NotNull(set, nameof(set))
                .AnnotateQuery(
                    new FromSqlQueryAnnotation(
                        Check.NotEmpty(sql, nameof(sql))));

        public static IQueryable<TEntity> FromSql<TEntity>(
            [NotNull] this DbSet<TEntity> set,
            [NotNull] string sql,
            [NotNull] params object[] parameters)
            where TEntity : class
            => Check.NotNull(set, nameof(set))
                .AnnotateQuery(
                    new FromSqlQueryAnnotation(
                        Check.NotEmpty(sql, nameof(sql)),
                        Check.NotNull(parameters, nameof(parameters))));

        public static DbSet<TEntity> UseRelationalNullSemantics<TEntity>([NotNull] this DbSet<TEntity> dbSet) where TEntity : class
        {
            Check.NotNull(dbSet, nameof(dbSet));

            var annotatedQueryable = dbSet.AnnotateQuery(new UseRelationalNullSemanticsQueryAnnotation());

            return new InternalDbSet<TEntity>(annotatedQueryable, dbSet);
        }
    }
}

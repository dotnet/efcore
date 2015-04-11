// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Annotations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDbSetExtensions
    {
        public static IQueryable<TEntity> FromSql<TEntity>([NotNull]this DbSet<TEntity> dbSet, [NotNull]string sql)
            where TEntity : class
        {
            Check.NotNull(dbSet, nameof(dbSet));
            Check.NotEmpty(sql, nameof(sql));

            return dbSet.AnnotateQuery(new FromSqlQueryAnnotation(sql));
        }

        public static IQueryable<TEntity> FromSql<TEntity>([NotNull]this DbSet<TEntity> dbSet, [NotNull]string sql, [NotNull] params object[] parameters)
            where TEntity : class
        {
            Check.NotNull(dbSet, nameof(dbSet));
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            return dbSet.AnnotateQuery(new FromSqlQueryAnnotation(sql, parameters));
        }
    }
}

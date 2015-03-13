// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDbSetExtensions
    {
        public static DbSet<TEntity> FromSql<TEntity>([NotNull]this DbSet<TEntity> dbSet, [NotNull]string query)
            where TEntity : class
        {
            Check.NotNull(dbSet, nameof(dbSet));
            Check.NotNull(query, nameof(query));

            return dbSet.AddAnnotation("Sql", query);
        }
    }
}

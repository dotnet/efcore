// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

// ReSharper disable PossibleNullReferenceException

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class Extensions
    {
        private static readonly MethodInfo _getObjectQueryMethodInfo = typeof(DbQuery).Assembly
            .GetType("System.Data.Entity.Internal.Linq.IInternalQuery", throwOnError: true)
            .GetProperty("ObjectQuery")
            .GetMethod;

        public static IQueryable<TEntity> DisableQueryCache<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            var internalQuery = typeof(DbQuery<TEntity>)
                .GetProperty("System.Data.Entity.Internal.Linq.IInternalQueryAdapter.InternalQuery", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetMethod
                .Invoke(query, Array.Empty<object>());

            var objectQuery = (ObjectQuery)_getObjectQueryMethodInfo.Invoke(internalQuery, Array.Empty<object>());

            objectQuery.EnablePlanCaching = false;

            return query;
        }

        public static IQueryable<TEntity> ApplyTracking<TEntity>(this IQueryable<TEntity> query, bool tracking)
            where TEntity : class
        {
            return tracking ? query : query.AsNoTracking();
        }

        public static DbSqlQuery<TEntity> ApplyTracking<TEntity>(this DbSqlQuery<TEntity> query, bool tracking)
            where TEntity : class
        {
            return tracking ? query : query.AsNoTracking();
        }
    }
}

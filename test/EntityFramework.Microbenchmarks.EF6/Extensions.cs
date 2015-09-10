// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Microbenchmarks.EF6
{
    public static class Extensions
    {
        private static MethodInfo _getObjectQueryMethodInfo = typeof(DbQuery).Assembly
            .GetType("System.Data.Entity.Internal.Linq.IInternalQuery", throwOnError: true)
            .GetProperty("ObjectQuery")
            .GetMethod;

        public static IQueryable<TEntity> ApplyCaching<TEntity>(this IQueryable<TEntity> query, bool caching)
            where TEntity : class
        {
            if (!caching)
            {
                var internalQuery = typeof(DbQuery<TEntity>)
                    .GetProperty("System.Data.Entity.Internal.Linq.IInternalQueryAdapter.InternalQuery", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetMethod
                    .Invoke(query, new object[0]);

                var objectQuery = (ObjectQuery)_getObjectQueryMethodInfo.Invoke(internalQuery, new object[0]);

                objectQuery.EnablePlanCaching = false;
            }

            return query;
        }

        public static IQueryable<TEntity> ApplyTracking<TEntity>(this IQueryable<TEntity> query, bool tracking)
            where TEntity : class
        {
            return tracking
                    ? query
                    : query.AsNoTracking();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Sql Server database specific extension methods for LINQ queries.
    /// </summary>
    public static class SqlServerQueryableExtensions
    {
        /// <summary>
        ///     <para>
        ///         Applies temporal 'AsOf' operation on the given DbSet, which only returns elements that were present in the database at a given point in time.
        ///     </para>
        ///     <para>
        ///         Temporal queries are always set as 'NoTracking'.
        ///     </para>
        /// </summary>
        /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
        /// <param name="pointInTime"><see cref="DateTime" /> representing a point in time for which the results should be returned.</param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the entities at a given point in time.</returns>
        public static IQueryable<TEntity> TemporalAsOf<TEntity>(
            this DbSet<TEntity> source,
            DateTime pointInTime)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            var queryableSource = (IQueryable)source;

            return queryableSource.Provider.CreateQuery<TEntity>(
                GenerateTemporalAsOfQueryRoot<TEntity>(
                    queryableSource,
                    pointInTime)).AsNoTracking();
        }

        /// <summary>
        ///     <para>
        ///         Applies temporal 'FromTo' operation on the given DbSet, which only returns elements that were present in the database between two points in time. 
        ///     </para>
        ///     <para>
        ///         Elements that were created at the starting point as well as elements that were removed at the end point are not included in the results.
        ///     </para>
        ///     <para>
        ///         All versions of entities in that were present within the time range are returned, so it is possible to return multiple entities with the same key.
        ///     </para>
        ///     <para>
        ///         Temporal queries are always set as 'NoTracking'.
        ///     </para>
        /// </summary>
        /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
        /// <param name="from">Point in time representing the start of the period for which results should be returned.</param>
        /// <param name="to">Point in time representing the end of the period for which results should be returned.</param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the entities present in a given time range.</returns>
        public static IQueryable<TEntity> TemporalFromTo<TEntity>(
            this DbSet<TEntity> source,
            DateTime from,
            DateTime to)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            var queryableSource = (IQueryable)source;

            return queryableSource.Provider.CreateQuery<TEntity>(
                GenerateRangeTemporalQueryRoot<TEntity>(
                    queryableSource,
                    from,
                    to,
                    TemporalOperationType.FromTo)).AsNoTracking();
        }

        /// <summary>
        ///     <para>
        ///         Applies temporal 'Between' operation on the given DbSet, which only returns elements that were present in the database between two points in time. 
        ///     </para>
        ///     <para>
        ///         Elements that were created at the starting point are not included in the results, however elements that were removed at the end point are included in the results.
        ///     </para>
        ///     <para>
        ///         All versions of entities in that were present within the time range are returned, so it is possible to return multiple entities with the same key.
        ///     </para>
        ///     <para>
        ///         Temporal queries are always set as 'NoTracking'.
        ///     </para>
        /// </summary>
        /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
        /// <param name="from">Point in time representing the start of the period for which results should be returned.</param>
        /// <param name="to">Point in time representing the end of the period for which results should be returned.</param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the entities present in a given time range.</returns>
        public static IQueryable<TEntity> TemporalBetween<TEntity>(
            this IQueryable<TEntity> source,
            DateTime from,
            DateTime to)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            var queryableSource = (IQueryable)source;

            return queryableSource.Provider.CreateQuery<TEntity>(
                GenerateRangeTemporalQueryRoot<TEntity>(
                    queryableSource,
                    from,
                    to,
                    TemporalOperationType.Between)).AsNoTracking();
        }

        /// <summary>
        ///     <para>
        ///         Applies temporal 'ContainedIn' operation on the given DbSet, which only returns elements that were present in the database between two points in time. 
        ///     </para>
        ///     <para>
        ///         Elements that were created at the starting point as well as elements that were removed at the end point are included in the results.
        ///     </para>
        ///     <para>
        ///         All versions of entities in that were present within the time range are returned, so it is possible to return multiple entities with the same key.
        ///     </para>
        ///     <para>
        ///         Temporal queries are always set as 'NoTracking'.
        ///     </para>
        /// </summary>
        /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
        /// <param name="from">Point in time representing the start of the period for which results should be returned.</param>
        /// <param name="to">Point in time representing the end of the period for which results should be returned.</param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the entities present in a given time range.</returns>
        public static IQueryable<TEntity> TemporalContainedIn<TEntity>(
            this DbSet<TEntity> source,
            DateTime from,
            DateTime to)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            var queryableSource = (IQueryable)source;

            return queryableSource.Provider.CreateQuery<TEntity>(
                GenerateRangeTemporalQueryRoot<TEntity>(
                    queryableSource,
                    from,
                    to,
                    TemporalOperationType.ContainedIn)).AsNoTracking();
        }

        /// <summary>
        ///     <para>
        ///         Applies temporal 'All' operation on the given DbSet, which returns all historical versions of the entities as well as their current state. 
        ///     </para>
        ///     <para>
        ///         Temporal queries are always set as 'NoTracking'.
        ///     </para>
        /// </summary>
        /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the entities and their historical versions.</returns>
        public static IQueryable<TEntity> TemporalAll<TEntity>(
            this DbSet<TEntity> source)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            var queryableSource = (IQueryable)source;
            var queryRootExpression = (QueryRootExpression)queryableSource.Expression;
            var entityType = queryRootExpression.EntityType;

            var temporalQueryRootExpression = new TemporalAllQueryRootExpression(
                queryRootExpression.QueryProvider!,
                entityType);

            return queryableSource.Provider.CreateQuery<TEntity>(temporalQueryRootExpression)
                .AsNoTracking();
        }

        private static Expression GenerateTemporalAsOfQueryRoot<TEntity>(
            IQueryable source,
            DateTime pointInTime)
        {
            var queryRootExpression = (QueryRootExpression)source.Expression;
            var entityType = queryRootExpression.EntityType;

            return new TemporalAsOfQueryRootExpression(
                queryRootExpression.QueryProvider!,
                entityType,
                pointInTime: pointInTime);
        }

        private static Expression GenerateRangeTemporalQueryRoot<TEntity>(
            IQueryable source,
            DateTime from,
            DateTime to,
            TemporalOperationType temporalOperationType)
        {
            var queryRootExpression = (QueryRootExpression)source.Expression;
            var entityType = queryRootExpression.EntityType;

            return new TemporalRangeQueryRootExpression(
                queryRootExpression.QueryProvider!,
                entityType,
                from: from,
                to: to,
                temporalOperationType: temporalOperationType);
        }
    }
}
